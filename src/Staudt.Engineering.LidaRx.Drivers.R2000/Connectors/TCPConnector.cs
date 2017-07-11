#region Copyright
//
// This file is part of Staudt Engineering's LidaRx library
//
// Copyright (C) 2017 Yannic Staudt / Staudt Engieering
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion


using Newtonsoft.Json;
using Staudt.Engineering.LidaRx.Drivers.R2000.Serialization;
using Staudt.Engineering.LidaRx.Drivers.R2000.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Connectors
{
    class TCPConnector : IR2000Connector
    {
        HttpClient httpClient;
        TcpClient tcpClient;

        bool watchdogEnabled;
        int watchdogTimeout;
        R2000ProtocolVersion protocolVersion;

        SemaphoreSlim sem = new SemaphoreSlim(1, 1);
        bool running = false;

        Thread watchdog;
        Thread receiveData;
        Thread dispatchData;
        CancellationTokenSource cts;
        ConcurrentBag<byte[]> ReceivedBuffers = new ConcurrentBag<byte[]>();

        TcpHandleRequestCommandResult currentHandle;
        IPAddress r2000IpAddress;

        public TCPConnector(
            HttpClient httpc,
            IPAddress address,
            bool enableWatchdog = true,
            int watchdogTimeout = 10000,
            R2000ProtocolVersion protocolVersion = R2000ProtocolVersion.v100)
        {
            this.observers = new List<IObserver<ScanFramePoint>>();

            this.httpClient = httpc;
            this.tcpClient = new TcpClient();
            this.r2000IpAddress = address;

            this.watchdogEnabled = enableWatchdog;
            this.watchdogTimeout = watchdogTimeout;
            this.protocolVersion = protocolVersion;
        }

        #region IDisposable
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // disposal of managed ressources
                    foreach (var observer in observers.ToArray())
                        if (observers.Contains(observer))
                            observer.OnCompleted();

                    observers.Clear();
                }

                // TODO: disposal of non-nanaged ressources here
                // TODO: set fields null if required

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region IR2000Connector
        public async Task StartAsync()
        {
            await sem.WaitAsync();

            // we're already running
            if (running)
                return;

            // request new handle
            var requestBuild = new StringBuilder();
            requestBuild.Append("request_handle_tcp?packet_type=B&start_angle=0");

            if(watchdogEnabled)
            {
                requestBuild.Append("&watchdog=on");
                requestBuild.Append($"&watchdogtimeout={this.watchdogTimeout}");
            }
            else
            {
                requestBuild.Append("&watchdog=off");
            }

            var requestUrl = requestBuild.ToString();

            // get the handle
            var handle = await httpClient.GetAsAsync<TcpHandleRequestCommandResult>(requestUrl);

            if(handle.ErrorCode != R2000ErrorCode.Success)
            {
                // TODO:
                throw new Exception("Blub");
            }

            this.currentHandle = handle;

            // start threads
            this.cts = new CancellationTokenSource();

            this.receiveData = new Thread(ReceiveData);
            this.receiveData.Start();
            this.dispatchData = new Thread(DispatchData);
            this.dispatchData.Start();

            if (watchdogEnabled)
            {
                // on newer protocol versions the R2000 supports using the TCP "back" channel 
                // to reset the watchdog counter. On older versions we issue HTTP queries...
                if (this.protocolVersion >= R2000ProtocolVersion.v101)
                    this.watchdog = new Thread(FeedWatchdogTcp);
                else
                    this.watchdog = new Thread(FeedWatchdogHttp);

                this.watchdog.Start();
            }

            // connect
            await tcpClient.ConnectAsync(this.r2000IpAddress, this.currentHandle.Port);

            // start sending stuff
            var started = await httpClient.GetAsAsync<StartCanOutputResponse>($"start_scanoutput?handle={currentHandle.HandleName}");

            if(started.ErrorCode != R2000ErrorCode.Success)
            {
                // abort and throw
                cts.Cancel();
                throw new Exception($"Couldn't start the scan data transmission because {started.ErrorText}");
            }

            this.running = true;
            sem.Release();
        }

        private async void DispatchData()
        {
            while (!cts.IsCancellationRequested)
            {
                byte[] buff = null;

                if (!ReceivedBuffers.TryTake(out buff))
                    await Task.Delay(10);
                else
                {
                    // deserialize the header
                    var header = buff.BytesToStruct<ScanFrameHeader>(0);

                    // make sure we have a comprehensible packet
                    if (header.Magic != 0xa25c)
                        continue;

                    // unit: 1/10000th of a degree
                    var angle = ((float)header.FirstAngleInThisPacket) / 10000;
                    var angleInc = ((float)header.AnglularIncrement) / 10000;

                    var offsetIncrement = (ushort)Marshal.SizeOf<ScanFramePointNative>();

                    for (var offset = header.HeaderSize; offset < header.PacketSize; offset += offsetIncrement)
                    {
                        var pointNative = buff.BytesToStruct<ScanFramePointNative>(offset);

                        var point = new ScanFramePoint()
                        {
                            Amplitude = pointNative.Amplitude,
                            Distance = pointNative.Distance,
                            Angle = angle,
                            ScanCounter = header.ScanNumber
                        };
                       
                        foreach (var o in observers)
                        {
                            o.OnNext(point);
                        }

                        angle += angleInc;
                    }
                }                    
            }
        }

        private async void ReceiveData()
        {
            while (!tcpClient.Connected)
            {
                if (cts.IsCancellationRequested)
                    break;

                await Task.Delay(10);
            }

            var stream = tcpClient.GetStream();
            byte[] buff = new byte[tcpClient.ReceiveBufferSize];

            while(stream.CanRead)
            {
                if (cts.IsCancellationRequested)
                    break;

                // read some chars
                var count = await stream.ReadAsync(buff, 0, (int)tcpClient.ReceiveBufferSize);

                // we need (at least) a full header
                if (count < Marshal.SizeOf<ScanFrameHeader>())
                {
                    await Task.Delay(1);
                    continue;
                }
                else
                {
                    var slice = new byte[count];
                    Array.Copy(buff, slice, count);
                    ReceivedBuffers.Add(slice);
                }

                Array.Clear(buff, 0, count);
            }

            // request cancellation if it's not already done!
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        private async void FeedWatchdogTcp()
        {
            // repeat
            while (!tcpClient.Connected)
            {
                if (cts.IsCancellationRequested)
                    break;

                await Task.Delay(10);
            }

            var stream = tcpClient.GetStream();
            byte[] feedMessage = new byte[] { 0x66, 0x65, 0x65, 0x64, 0x77, 0x64, 0x67, 0x04 };

            var feedInterval = this.watchdogTimeout / 4;

            // at most once every second as per datasheet
            if (feedInterval < 1000)
                feedInterval = 1000;

            while (stream.CanWrite)
            {
                if (cts.IsCancellationRequested)
                    break;

                await stream.WriteAsync(feedMessage, 0, feedMessage.Length);
                await Task.Delay(feedInterval);
            }
             
            // request cancellation if it's not already done!
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        private async void FeedWatchdogHttp()
        {
            // wait until we're connected
            while (!tcpClient.Connected)
            {
                if (cts.IsCancellationRequested)
                    break;

                await Task.Delay(10);
            }

            var feedInterval = this.watchdogTimeout / 4;

            // at most once every second as per datasheet
            if (feedInterval < 1000)
                feedInterval = 1000;

            while (tcpClient.Connected)
            {
                if (cts.IsCancellationRequested)
                    break;

                var result = await httpClient.GetAsAsync<SetParameterResult>($"feed_watchdog?handle={currentHandle.HandleName}");

                if (result.ErrorCode != R2000ErrorCode.Success)
                {
                    // todo
                }


                await Task.Delay(feedInterval);
            }

            // request cancellation if it's not already done!
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IObservable stuff
        /// <summary>
        /// Observers that have subscribed
        /// </summary>
        protected List<IObserver<ScanFramePoint>> observers;

        public IDisposable Subscribe(IObserver<ScanFramePoint> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber<ScanFramePoint>(observers, observer);
        }
        #endregion
    }

    class TcpHandleRequestCommandResult
    {
        [JsonProperty(PropertyName = "port")]
        public int Port { get; set; }

        [JsonProperty(PropertyName = "handle")]
        public string HandleName { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public R2000ErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_text")]
        public string ErrorText { get; set; }
    }

    class StartCanOutputResponse
    {
        [JsonProperty(PropertyName = "error_code")]
        public R2000ErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_text")]
        public string ErrorText { get; set; }
    }


}
