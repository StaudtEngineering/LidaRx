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

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Connectors
{
    class TCPConnector : IR2000Connector
    {
        HttpClient httpClient;
        TcpClient tcpClient;

        bool watchdogEnabled;
        int watchdogTimeout;

        SemaphoreSlim sem = new SemaphoreSlim(1, 1);
        bool running = false;

        Thread watchdog;
        Thread receiveData;
        CancellationTokenSource cts;

        TcpHandleRequestCommandResult currentHandle;
        IPAddress r2000IpAddress;

        public TCPConnector(
            HttpClient httpc,
            IPAddress address,
            bool enableWatchdog = true,
            int watchdogTimeout = 10000)
        {
            this.observers = new List<IObserver<ScanFramePoint>>();

            this.httpClient = httpc;
            this.tcpClient = new TcpClient();
            this.r2000IpAddress = address;

            this.watchdogEnabled = enableWatchdog;
            this.watchdogTimeout = watchdogTimeout;
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

            if (watchdogEnabled)
            {
                this.watchdog = new Thread(FeedWatchdog);
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

        private async void ReceiveData()
        {
            while (!tcpClient.Connected)
            {
                await Task.Delay(10);
            }

            var stream = tcpClient.GetStream();
            byte[] buff = new byte[tcpClient.ReceiveBufferSize];

            // repeat
            while (true)
            {
                while(stream.CanRead)
                {
                    // read some chars
                    var count = await stream.ReadAsync(buff, 0, (int)tcpClient.ReceiveBufferSize);

                    // we need (at least) a full header
                    if (count < Marshal.SizeOf<ScanFrameHeader>())
                        continue;

                    // deserialize the header
                    var header = buff.BytesToStruct<ScanFrameHeader>(0);

                    // make sure we have a comprehensible packet
                    if (header.Magic != 0xa25c)
                        continue;

                    var angle = ((float)header.FirstAngleInThisPacket) / 10000;
                    var angleInc = ((float)header.AnglularIncrement) / 10000;

                    for (var offset = header.HeaderSize + 1; offset < header.PacketSize; offset += Marshal.SizeOf<ScanFramePointNative>())
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

                    // clear the array
                    Array.Clear(buff, 0, count);
                }
            }

            // request cancellation if it's not already done!
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        private async void FeedWatchdog()
        {
            // repeat
            while (true)
            {



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
