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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using Staudt.Engineering.LidaRx.Drivers.R2000.Exceptions;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Connectors
{
    class TCPConnector : IR2000Connector
    {
        HttpClient httpClient;
        TcpClient tcpClient;

        bool watchdogEnabled;
        int watchdogTimeout;

        // Note: need to know this in order to decide which communication channel to use 
        // to reset the watchdog timer. When > 1.01 use the TCP "back channel"
        R2000ProtocolVersion protocolVersion; 

        SemaphoreSlim sem = new SemaphoreSlim(1, 1);
        bool running = false;

        Thread watchdog;
        Thread receiveData;
        Thread dispatchData;
        CancellationTokenSource cts;
        ConcurrentQueue<byte[]> ReceivedBuffers = new ConcurrentQueue<byte[]>();

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
            this.statusObservers = new List<IObserver<LidarStatusEvent>>();

            this.httpClient = httpc;
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

                    foreach (var observer in statusObservers.ToArray())
                        if (statusObservers.Contains(observer))
                            observer.OnCompleted();

                    statusObservers.Clear();

                    if (!cts.IsCancellationRequested)
                        cts.Cancel();
                }                

                this.tcpClient?.Dispose();
                this.ReceivedBuffers = null;

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

            if (running)
            {
                sem.Release();
                return;
            }

            // create a new tcp client
            this.tcpClient = new TcpClient();

            // build query to request a new handle
            var requestBuild = new StringBuilder();
            requestBuild.Append("request_handle_tcp?packet_type=C&start_angle=0");

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
                sem.Release();  // avoid deadlock
                throw new R2000ProtocolErrorException(handle, "Could not acquire tcp handle");
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
            var started = await httpClient.GetAsAsync<R2000ProtocolBaseResponse>($"start_scanoutput?handle={currentHandle.HandleName}");

            if(started.ErrorCode != R2000ErrorCode.Success)
            {
                // abort and throw
                cts.Cancel();
                throw new R2000ProtocolErrorException(started, "Couldn't start the tcp data transmission");
            }

            this.running = true;
            sem.Release();
        }

        Dictionary<uint, LidarStatusEvent> errorFlagsWithMessage = new Dictionary<uint, LidarStatusEvent>()
        {
            { (1 << 1), new LidarStatusEvent("Scanner sampling rate was modified during this scan", LidarStatusLevel.Info) },
            { (1 << 2), new LidarStatusEvent("R2000 reported invalid data in frame. Consistency can't be guaranteed!", LidarStatusLevel.Warning) },
            { (1 << 3), new LidarStatusEvent("R2000 reported unstable rotation", LidarStatusLevel.Warning) },
            { (1 << 4), new LidarStatusEvent("R2000 reported skipped packet(s). Please check your CPU / Network load and adapt scan frequency and sampling rate accordingly", LidarStatusLevel.Info) },
            { (1 << 10), new LidarStatusEvent("Device temperature below waring threshold (0 °C)", LidarStatusLevel.Warning) },
            { (1 << 11), new LidarStatusEvent("Device temperature above waring threshold (80 °C)", LidarStatusLevel.Warning) },
            { (1 << 12), new LidarStatusEvent("Device CPU is about to over-load", LidarStatusLevel.Warning) },
            { (1 << 18), new LidarStatusEvent("Device temperature below error threshold (-10 °C)", LidarStatusLevel.Error) },
            { (1 << 19), new LidarStatusEvent("Device temperature above error threshold (85 °C)", LidarStatusLevel.Error) },
            { (1 << 20), new LidarStatusEvent("Device CPU overload", LidarStatusLevel.Error) },
        };

        private async void DispatchData()
        {
            var headerSize = Marshal.SizeOf<ScanFrameHeader>();

            while (!cts.IsCancellationRequested)
            {
                byte[] buff = null;

                if (!ReceivedBuffers.TryDequeue(out buff))
                    await Task.Delay(10);
                else
                {
                    // not a full frame!
                    if (buff.Length < headerSize)
                        continue;

                    // deserialize the header
                    var header = buff.BytesToStruct<ScanFrameHeader>(0);

                    // make sure we have a comprehensible packet
                    if (header.Magic != 0xa25c)
                        continue;

                    // check for error flags and publish them!
                    var flagMessages = errorFlagsWithMessage.Where(ft => (header.StatusFlags & ft.Key) > 0).Select(ft => ft.Value);

                    foreach(var msg in flagMessages)
                        foreach (var o in statusObservers)
                            o.OnNext(msg);
      

                    // unit: 1/10000th of a degree
                    var angle = ((float)header.FirstAngleInThisPacket) / 10000;
                    var angleInc = ((float)header.AnglularIncrement) / 10000;

                    var offsetIncrement = (ushort)Marshal.SizeOf<ScanFramePointNative>();
                    var countPoints = header.NumberOfPointsThisPacket;
                    var offset = header.HeaderSize;

                    for(int idx = 0; idx < countPoints; idx++)
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
                        offset += offsetIncrement;
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

            try
            {

                while (stream.CanRead)
                {
                    if (cts.IsCancellationRequested)
                        break;

                    byte[] buff = new byte[2048];

                    // read some chars
                    var count = await stream.ReadAsync(buff, 0, buff.Length);
                    Array.Resize(ref buff, count);
                    ReceivedBuffers.Enqueue(buff);
                }

            }
            catch(Exception ex)
            {
                // TODO
            }

            // request cancellation if it's not already done!
            if (!cts.IsCancellationRequested)
                cts.Cancel();

            running = false;
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

            running = false;
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
                var result = await httpClient.GetAsAsync<SetParameterResult>($"feed_watchdog?handle={currentHandle.HandleName}");

                if (result.ErrorCode != R2000ErrorCode.Success)
                {
                    // todo
                }

                await Task.Delay(feedInterval);

                if (cts.IsCancellationRequested)
                    break;
            }

            // request cancellation if it's not already done!
            if (!cts.IsCancellationRequested)
                cts.Cancel();

            running = false;
        }

        public async Task StopAsync()
        {
            await sem.WaitAsync();

            if(!running)
            {
                sem.Release();
                return;
            }

            // stop the scan output and release the handle
            var stopOutResult = await httpClient.GetAsAsync<R2000ProtocolBaseResponse>($"stop_scanoutput?handle={currentHandle.HandleName}");
            var releaseHandleResult = await httpClient.GetAsAsync<R2000ProtocolBaseResponse>($"release_handle?handle={currentHandle.HandleName}");

            // stop processing
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }

            if (tcpClient != null)
            {
                // dispose and throw away
                tcpClient.Dispose();
                tcpClient = null;
            }

            // "clear" the buffer
            ReceivedBuffers = new ConcurrentQueue<byte[]>();

            this.running = false;

            sem.Release();
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

        protected List<IObserver<LidarStatusEvent>> statusObservers;

        public IDisposable Subscribe(IObserver<LidarStatusEvent> observer)
        {
            if (!statusObservers.Contains(observer))
                statusObservers.Add(observer);
            return new Unsubscriber<LidarStatusEvent>(statusObservers, observer);
        }

        #endregion
    }

    class TcpHandleRequestCommandResult : R2000ProtocolBaseResponse
    {
        [JsonProperty(PropertyName = "port")]
        public int Port { get; set; }

        [JsonProperty(PropertyName = "handle")]
        public string HandleName { get; set; }
    }
}
