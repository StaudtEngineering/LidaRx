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

using RJCP.IO.Ports;
using Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep
{
    public class SweepScanner : LidarScannerBase
    {
        /// <summary>
        /// The serial port used to communicate with Sweep
        /// </summary>
        SerialPortStream serialPort;

        /// <summary>
        /// True when connected with a Sweep
        /// </summary>
        public override bool Connected => serialPort?.IsOpen ?? false;

        /// <summary>
        /// True when currently scanning
        /// </summary>
        public override bool IsScanning => _isScanning;
        bool _isScanning = false;

        /// <summary>
        /// The status / info for the connected sweep
        /// </summary>
        public SweepInfo Info { get; } = new SweepInfo();

        /// <summary>
        /// Semaphore for the connection process
        /// </summary>
        SemaphoreSlim _semaphoreSlimConnect = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Coordination 
        /// </summary>
        List<Thread> scanProcessingThreads = new List<Thread>();
        CancellationTokenSource scanProcessingCts;
        SemaphoreSlim _semaphoreSlimScanStart = new SemaphoreSlim(1, 1);
        SemaphoreSlim _semaphoreConfigurationChanges = new SemaphoreSlim(1, 1);

        // rxBuffer queue
        Queue<byte[]> rxScanBuffer = new Queue<byte[]>();


        /// <summary>
        /// Creates a new driver for a Scanse.io Sweep LIDAR scanner
        /// </summary>
        /// <param name="serialPort"></param>
        public SweepScanner(string portName)
        {
            serialPort = new SerialPortStream(portName, 115200, 8, Parity.None, StopBits.One);
            serialPort.ErrorReceived += SerialPort_ErrorReceived;
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            var msg = new LidarErrorEvent(e.ToString());

            foreach (var observer in base.observers)
            {
                observer.OnNext(msg);
            }
        }

        public override void Connect()
        {
            _semaphoreSlimConnect.Wait();

            // already connected
            if (Connected)
                return;

            serialPort.Open();

            // make sure we're in a halfway known state...
            var dxCommand = new StopDataAcquisitionCommand();
            serialPort.Write(dxCommand.Command, 0, dxCommand.Command.Length);

            Thread.Sleep(100);
            serialPort.DiscardInBuffer();

            if(WaitForStabilizedMotorSpeed(TimeSpan.FromSeconds(10)) == false)
            {
                // todo custom exception
                throw new Exception("Could not connect in time");
            }

            // gather information about the device
            RetrieveDeviceInformation().Wait();

            _semaphoreSlimConnect.Release();
        }

        public async override Task ConnectAsync()
        {
            await _semaphoreSlimConnect.WaitAsync();

            // already connected
            if (Connected)
                return;

            serialPort.Open();

            // make sure we're in a halfway known state...
            var dxCommand = new StopDataAcquisitionCommand();
            await serialPort.WriteAsync(dxCommand.Command.Select(x => (byte)x).ToArray(), 0, dxCommand.Command.Length);

            await Task.Delay(100);
            serialPort.DiscardInBuffer();

            // todo: async version of this
            if (WaitForStabilizedMotorSpeed(TimeSpan.FromSeconds(10)) == false)
            {
                // todo custom exception
                throw new Exception("Could not connect in time");
            }

            await RetrieveDeviceInformation();

            _semaphoreSlimConnect.Release();
        }

        private async Task RetrieveDeviceInformation()
        {
            // gather information about the device
            var idCommand = new DeviceInformationCommand();
            var ivCommand = new VersionInformationCommand();

            await SimpleCommandTxRxAsync(idCommand);
            await SimpleCommandTxRxAsync(ivCommand);

            this.Info.BitRate = idCommand.SerialBitrate.Value;
            this.Info.LaserState = idCommand.LaserState.Value;
            this.Info.Mode = idCommand.Mode.Value;
            this.Info.Diagnostic = idCommand.Diagnostic.Value;
            this.Info.MotorSpeed = idCommand.MotorSpeed ?? SweepMotorSpeed.SpeedUnknown;
            this.Info.SampleRate = SweepConfigHelpers.IntToSweepSampleRate(idCommand.SampleRate.Value);

            this.Info.SerialNumber = ivCommand.SerialNumber.ToString();
            this.Info.Protocol = ivCommand.ProtocolVersion.Value.ToString();
            this.Info.FirmwareVersion = ivCommand.FirmwareVersion.ToString();
            this.Info.HardwareVersion = ivCommand.HardwareVersion.ToString();
            this.Info.Model = ivCommand.Model;
        }

        public override void Disconnect()
        {
            _semaphoreSlimConnect.Wait();

            if(Connected)
            {
                // make sure we're in a halfway known state...
                var dxCommand = new StopDataAcquisitionCommand();
                SimpleCommandTxRx(dxCommand);

                // close the serial port
                serialPort.Close();
            }            

            _semaphoreSlimConnect.Release();
        }

        public async override Task DisconnectAsync()
        {
            await _semaphoreSlimConnect.WaitAsync();

            if (Connected)
            {
                // make sure we're in a halfway known state...
                var dxCommand = new StopDataAcquisitionCommand();
                await SimpleCommandTxRxAsync(dxCommand);

                // close the serial port
                serialPort.Close();
            }

            _semaphoreSlimConnect.Release();
        }

        public override void StartScan()
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the Sweep scanner.");

            _semaphoreSlimScanStart.Wait();

            if (IsScanning)
                return;

            var startScanCommand = new StartDataAcquisitionCommand();
            SimpleCommandTxRx(startScanCommand);

            if (startScanCommand.Success == true)
                StartScanThreadsInternal();

            _semaphoreSlimScanStart.Release();
        }

        public async override Task StartScanAsync()
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the Sweep scanner.");

            await _semaphoreSlimScanStart.WaitAsync();

            if (IsScanning)
                return;

            var startScanCommand = new StartDataAcquisitionCommand();
            await SimpleCommandTxRxAsync(startScanCommand);

            if (startScanCommand.Success == true)
                StartScanThreadsInternal();

            _semaphoreSlimScanStart.Release();
        }

        private void StartScanThreadsInternal()
        {
            this._isScanning = true;

            this.scanProcessingCts = new CancellationTokenSource();

            var serialPollThread = new Thread(PollSerialPort);
            this.scanProcessingThreads.Add(serialPollThread);

            var processingThread = new Thread(ProcessLidarScanFrames);
            this.scanProcessingThreads.Add(processingThread);

            serialPollThread.Start();
            processingThread.Start();
        }

        public long DiscardedFrames { get; private set; } = 0;
        public long DiscardedBytes { get; private set; } = 0;

        private void PollSerialPort()
        {
            while (true)
            {
                // stop any further processing here...
                if (scanProcessingCts.IsCancellationRequested)
                    return;

                byte[] buffer = new byte[1024];
                var bytes = serialPort.Read(buffer, 0, buffer.Length);

                if (bytes > 0)
                {
                    Array.Resize(ref buffer, bytes);
                    rxScanBuffer.Enqueue(buffer);
                }
            }
        }

        private bool ChecksumIsValid(Queue<byte> protentialFrame)
        {
            // checksum validation
            var checksumByteValue = protentialFrame.ElementAt(6);
            var checksumCalculated = (protentialFrame.Take(6).Aggregate(0, (acc, x) => acc + x)) % 255;

            return checksumByteValue == checksumCalculated;
        }

        private void ProcessLidarScanFrames()
        {
            byte[] buffer;
            Queue<byte> potentialFrame = new Queue<byte>(7);
            Queue<byte> scan = new Queue<byte>();

            Action refillScanBuffer = () =>
            {
                if (rxScanBuffer.Count > 0)
                {
                    var s = rxScanBuffer.Dequeue();

                    foreach (var x in s)
                        scan.Enqueue(x);
                }
                else
                    Thread.Sleep(1);                
            };

            while (true)
            {
                // get the next 7 bytes
                while (potentialFrame.Count < 7)
                {
                    if (scan.Count == 0)
                        refillScanBuffer();
                    else
                        potentialFrame.Enqueue(scan.Dequeue()); // get a byte

                    // exit point
                    if (scanProcessingCts.IsCancellationRequested)
                        return;
                }

                if (!ChecksumIsValid(potentialFrame))
                {
                    // garbage... :S
                    DiscardedFrames++;

                    // find a whole frame
                    do
                    {
                        // trow away one byte and get the next instead
                        // ...until we get something meaningful
                        if (scan.Count == 0)
                            refillScanBuffer();
                        else
                        {
                            DiscardedBytes++;
                            potentialFrame.Dequeue();
                            potentialFrame.Enqueue(scan.Dequeue());
                        }

                        // exit point
                        if (scanProcessingCts.IsCancellationRequested)
                            return;
                    }
                    while (!ChecksumIsValid(potentialFrame));
                }

                buffer = potentialFrame.ToArray();

                // extract data
                var errorSync = buffer[0];
                var isSync = (errorSync & 0x1) == 1;

                if (isSync) this.ScanCounter++;

                var azimuth = (buffer[1] + (buffer[2] << 8)) / 16.0f;
                var distance = buffer[3] + (buffer[4] << 8);
                var signal = buffer[5];

                var carthesianPoint = base.TransfromScannerToSystemCoordinates(azimuth, distance);
                var scanPacket = new LidarPoint(carthesianPoint, azimuth, distance, signal, this.ScanCounter);

                // exit point
                if (scanProcessingCts.IsCancellationRequested)
                    return;

                foreach (var observer in base.observers)
                {
                    observer.OnNext(scanPacket);
                }

                // cleanup
                potentialFrame.Clear();
            }
        }

        public override void StopScan()
        {
            // send a stop command...
            var cmd = new StopDataAcquisitionCommand();
            serialPort.Write(cmd.Command, 0, cmd.Command.Length);

            // stop the threads
            scanProcessingCts.Cancel();

            // wait for termination
            while (this.scanProcessingThreads.Any(x => x.IsAlive))
            {
                Thread.Sleep(1);
            } 

            // clear the waiting list
            this.scanProcessingThreads.Clear();

            // throwaway stuff in the RX buffer!
            Thread.Sleep(20);
            serialPort.DiscardInBuffer();

            // send a second DX command
            SimpleCommandTxRx(cmd);

            // throwaway stuff in the RX buffer!
            Thread.Sleep(20);
            serialPort.DiscardInBuffer();

            this._isScanning = false;
        }

        public async override Task StopScanAsync()
        {
            // make sure we're in a halfway known state...
            var dxCommand = new StopDataAcquisitionCommand();
            await SimpleCommandTxRxAsync(dxCommand);
        }

        #region Sweep specific interface

        public void SetMotorSpeed(SweepMotorSpeed targetSpeed)
        {
            _semaphoreConfigurationChanges.Wait();

            bool restartScanning = _isScanning;

            if (_isScanning)
            {
                StopScan();
            }

            var cmd = new AdjustMotorSpeedCommand(targetSpeed);
            SimpleCommandTxRx(cmd);

            if (cmd.Status == AdjustMotorSpeedResult.Success)
            {
                WaitForStabilizedMotorSpeed(TimeSpan.FromSeconds(10));
                RetrieveDeviceInformation().Wait();
            }
            else
            {
                // todo?
            }

            if(restartScanning)
            {
                StartScan();
            }

            _semaphoreConfigurationChanges.Release();

        }

        public void SetSampleRate(SweepSampleRate targetRate)
        {
            _semaphoreConfigurationChanges.Wait();

            bool restartScanning = _isScanning;

            if (_isScanning)
            {
                StopScan();
            }

            var cmd = new AdjustSampleRateCommand(targetRate);
            SimpleCommandTxRx(cmd);

            if(cmd.Status == AdjustSampleRateResult.Success)
            {


            }
            else
            {


            }

            if (restartScanning)
            {
                StartScan();
            }

            _semaphoreConfigurationChanges.Release();
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Wait until motor speed is stabilized
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="throwOnFail"></param>
        /// <returns></returns>
        bool WaitForStabilizedMotorSpeed(TimeSpan timeout, bool throwOnFail = true)
        {
            var mrCommand = new MotorReadyCommand();
            var limit = DateTime.Now + timeout;

            while(DateTime.Now < limit)
            {
                try
                {
                    SimpleCommandTxRx(mrCommand);

                    if (mrCommand.DeviceReady == true)
                        break;
                }
                catch { }

                Thread.Sleep(50);
            }

            if (throwOnFail && mrCommand.DeviceReady == false)
            {
                // todo: custom exception
                throw new Exception("Device motor speed did not stabilize in time");
            }

            return true;
        }

        #endregion

        #region serial RXTX stuff

        void SimpleCommandTxRx(ISweepCommand cmd)
        {
            var buffer = new char[cmd.ExpectedAnswerLength];

            // TX then RX
            serialPort.Write(cmd.Command, 0, cmd.Command.Length);
            serialPort.Read(buffer, 0, cmd.ExpectedAnswerLength);

            // process the response
            cmd.ProcessResponse(buffer);
        }

        async Task SimpleCommandTxRxAsync(ISweepCommand cmd)
        {
            var buffer = new byte[cmd.ExpectedAnswerLength];

            // TX then RX
            await serialPort.WriteAsync(cmd.Command.Select(x => (byte)x).ToArray(), 0, cmd.Command.Length);
            await serialPort.ReadAsync(buffer, 0, cmd.ExpectedAnswerLength);

            // process the response
            cmd.ProcessResponse(buffer.Select(x => (char)x).ToArray());
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(Connected && IsScanning)
                    {
                        // make sure we're in a halfway known state...
                        StopScan();
                    }

                    serialPort.Flush();
                    serialPort.Dispose();
                }

                // TODO: disposal of non-nanaged ressources here
                // TODO: set fields null if required

                disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
