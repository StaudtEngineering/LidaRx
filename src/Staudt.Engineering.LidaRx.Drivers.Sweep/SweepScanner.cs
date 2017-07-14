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
using Staudt.Engineering.LidaRx.Drivers.Sweep.Exceptions;
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
        /// As Sweep is very inacurate in the close range, you can choose to discard any point
        /// closer than MinPointDistance (mm). Defaults to 200mm
        /// </summary>
        public float MinPointDistance { get; set; } = 200;

        /// <summary>
        /// Semaphore for the connection process
        /// </summary>
        SemaphoreSlim _semaphoreSlimConnect = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Number of discarded frames (because of checksum errors)
        /// </summary>
        public long DiscardedFrames { get; private set; } = 0;

        /// <summary>
        /// Number of discarded bytes (because of lost bytes)
        /// </summary>
        public long DiscardedBytes { get; private set; } = 0;

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
            serialPort.ErrorReceived += (sender, e) => PublishLidarEvent(new LidarStatusEvent(e.ToString(), LidarStatusLevel.Error));

            serialPort.ReadTimeout = 500;
            serialPort.WriteTimeout = 500;
        }

        public override void Connect()
        {
            ConnectAsync().Wait();
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

            await WaitForStabilizedMotorSpeedAsync(TimeSpan.FromSeconds(10), throwOnFail: true);
            await UpdateDeviceInfoAsync();

            _semaphoreSlimConnect.Release();
        }     

        public override void Disconnect()
        {
            DisconnectAsync().Wait();
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
            StartScanAsync().Wait();
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

            _semaphoreSlimScanStart.Release();
        }

        public override void StopScan()
        {
            StopScanAsync().Wait();
        }

        public async override Task StopScanAsync()
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the Sweep scanner.");

            await _semaphoreSlimScanStart.WaitAsync();

            // send a stop command to sweep
            var cmd = new StopDataAcquisitionCommand();
            var cmdBytes = cmd.Command.Select(x => (byte)x).ToArray();
            await serialPort.WriteAsync(cmdBytes, 0, cmdBytes.Length);

            // stop the threads
            scanProcessingCts.Cancel();

            // wait for termination
            while (this.scanProcessingThreads.Any(x => x.IsAlive))
            {
                await Task.Delay(1);
            }

            this.scanProcessingThreads.Clear();
            this.rxScanBuffer.Clear();

            await Task.Delay(1);
            serialPort.DiscardInBuffer();

            // send a second DX command
            await serialPort.WriteAsync(cmdBytes, 0, cmdBytes.Length);

            // throwaway stuff in the RX buffer!
            Thread.Sleep(1);
            serialPort.DiscardInBuffer();

            this._isScanning = false;

            _semaphoreSlimScanStart.Release();
        }

        #region Sweep specific interface

        /// <summary>
        /// Set sweep the motor speed
        /// </summary>
        /// <param name="targetSpeed"></param>
        /// <param name="smartInterleave">Automatically pause scanning if necessary to adjust the speed, then resume scanning</param>
        public void SetMotorSpeed(SweepMotorSpeed targetSpeed, bool smartInterleave = true)
        {
            SetMotorSpeedAsync(targetSpeed, smartInterleave).Wait();
        }

        /// <summary>
        /// Set sweep the motor speed
        /// </summary>
        /// <param name="targetSpeed"></param>
        /// <param name="smartInterleave">Automatically pause scanning if necessary to adjust the speed, then resume scanning</param>
        public async Task SetMotorSpeedAsync(SweepMotorSpeed targetSpeed, bool smartInterleave = true)
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the Sweep scanner.");

            if (this.Info.MotorSpeed == targetSpeed)
                return;

            bool restartScanning = _isScanning;

            if(_isScanning && !smartInterleave)
                throw new InvalidOperationException("Cannot change device configuration while scan is running");

            await _semaphoreConfigurationChanges.WaitAsync();

            if (_isScanning)
            {
                await StopScanAsync();

                // give sweep some time to recover
                await Task.Delay(250);
            }

            var cmd = new AdjustMotorSpeedCommand(targetSpeed);
            await SimpleCommandTxRxAsync(cmd);

            if (cmd.Status == AdjustMotorSpeedResult.Success)
            {
                await WaitForStabilizedMotorSpeedAsync(TimeSpan.FromSeconds(30));
                this.Info.MotorSpeed = targetSpeed;
            }
            else
            {
                throw new SweepProtocolErrorException($"Adjust motor speed command failed with status {cmd.Status}", null);
            }

            if (restartScanning)
            {
                await StartScanAsync();
            }

            _semaphoreConfigurationChanges.Release();
        }

        /// <summary>
        /// Set the sample rate
        /// </summary>
        /// <param name="targetRate"></param>
        /// <param name="smartInterleave">Automatically pause scanning if necessary to adjust the sample rate, then resume scanning</param>
        public void SetSamplingRate(SweepSampleRate targetRate, bool smartInterleave = true)
        {
            SetSamplingRateAsync(targetRate, smartInterleave).Wait();
        }

        /// <summary>
        /// Set the sample rate
        /// </summary>
        /// <param name="targetRate"></param>
        /// <param name="smartInterleave">Automatically pause scanning if necessary to adjust the sample rate, then resume scanning</param>
        public async Task SetSamplingRateAsync(SweepSampleRate targetRate, bool smartInterleave = true)
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the Sweep scanner.");

            bool restartScanning = _isScanning;

            if (_isScanning && !smartInterleave)
                throw new InvalidOperationException("Cannot change device configuration while scan is running");

            await _semaphoreConfigurationChanges.WaitAsync();

            if (_isScanning)
            {
                await StopScanAsync();

                // give sweep some time to recover
                await Task.Delay(250);
            }

            var cmd = new AdjustSampleRateCommand(targetRate);
            await SimpleCommandTxRxAsync(cmd);

            if (cmd.Status == AdjustSampleRateResult.Success)
            {
                this.Info.SampleRate = targetRate;
            }
            else
            {
                throw new SweepProtocolErrorException($"Adjust sample rate command failed with status {cmd.Status}", null);
            }

            if (restartScanning)
            {
                await StartScanAsync();
            }

            _semaphoreConfigurationChanges.Release();
        }

        /// <summary>
        /// Send ID & IV commands to update the device info property
        /// </summary>
        /// <returns></returns>
        public async Task UpdateDeviceInfoAsync()
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the Sweep scanner.");

            // gather information about the device
            var idCommand = new DeviceInformationCommand();
            var ivCommand = new VersionInformationCommand();

            await SimpleCommandTxRxAsync(idCommand);
            await SimpleCommandTxRxAsync(ivCommand);

            FillDeviceInfo(idCommand, ivCommand);
        }

        /// <summary>
        /// Send ID & IV commands to update the device info property
        /// </summary>
        public void UpdateDeviceInfo()
        {
            UpdateDeviceInfoAsync().Wait();
        }

        /// <summary>
        /// Wait until motor speed is stabilized
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="throwOnFail">Throw an exception on failure instead of just returning false</param>
        /// <returns>True on success</returns>
        public bool WaitForStabilizedMotorSpeed(TimeSpan timeout, bool throwOnFail = true)
        {
            return WaitForStabilizedMotorSpeedAsync(timeout, throwOnFail).Result;
        }

        /// <summary>
        /// Wait until motor speed is stabilized
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="throwOnFail">Throw an exception on failure instead of just returning false</param>
        /// <returns>True on success</returns>
        public async Task<bool> WaitForStabilizedMotorSpeedAsync(TimeSpan timeout, bool throwOnFail = true)
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the Sweep scanner.");

            var mrCommand = new MotorReadyCommand();
            var limit = DateTime.Now + timeout;

            while (DateTime.Now < limit)
            {
                try
                {
                    await SimpleCommandTxRxAsync(mrCommand);

                    if (mrCommand.DeviceReady == true)
                        break;
                }
                catch { }

                await Task.Delay(50);
            }

            if (throwOnFail && mrCommand.DeviceReady == false)
            {
                throw new SweepMotorStabilizationTimeoutException(timeout.Seconds);
            }

            return true;
        }

        #endregion

        #region Helpers

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
                // cleanup
                potentialFrame.Clear();

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
                    var localDiscardedBytes = 0;

                    // find a whole frame
                    do
                    {
                        // trow away one byte and get the next instead
                        // ...until we get something meaningful
                        if (scan.Count == 0)
                            refillScanBuffer();
                        else
                        {
                            localDiscardedBytes++;
                            potentialFrame.Dequeue();
                            potentialFrame.Enqueue(scan.Dequeue());
                        }

                        // exit point
                        if (scanProcessingCts.IsCancellationRequested)
                            return;
                    }
                    while (!ChecksumIsValid(potentialFrame));

                    this.DiscardedBytes += localDiscardedBytes;
                    PublishLidarEvent(new LidarStatusEvent($"Checksum error, had to discard {localDiscardedBytes} bytes to recover to a valid read window", LidarStatusLevel.Warning));
                }

                buffer = potentialFrame.ToArray();

                // extract data
                var errorSync = buffer[0];

                // skip on error packets
                if ((errorSync & (1 << 1)) == 1)
                {
                    PublishLidarEvent(new LidarStatusEvent("Communication error with LIDAR module (Sweep error bit E0)", LidarStatusLevel.Error));
                    continue;
                }

                // check if this is a sync packet              
                if ((errorSync & 0x1) == 1)
                    this.ScanCounter++;

                // get coordinates and signal amplitude
                var azimuth = (buffer[1] + (buffer[2] << 8)) / 16.0f;
                var distance = (buffer[3] + (buffer[4] << 8)) * 10;     // convert cm to mm
                var signal = buffer[5];

                // discard short range points
                if (distance < this.MinPointDistance)
                    continue;

                // transform the polar to carthesian coordinates within the applications coordinate 
                // system (not scanner centric)
                var carthesianPoint = base.TransfromScannerToSystemCoordinates(azimuth, distance);

                // exit point
                if (scanProcessingCts.IsCancellationRequested)
                    return;

                // propagate the new lidar point through the system
                var scanPacket = new LidarPoint(carthesianPoint, azimuth, distance, signal, this.ScanCounter, this);
                PublishLidarEvent(scanPacket);
            }
        }

        private void FillDeviceInfo(DeviceInformationCommand idCommand, VersionInformationCommand ivCommand)
        {
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

        void PublishLidarEvent(ILidarEvent ev)
        {
            foreach (var observer in base.observers)
            {
                observer.OnNext(ev);
            }
        }

        #endregion

        #region Serial RXTX stuff

        void SimpleCommandTxRx(ISweepCommand cmd)
        {
            SimpleCommandTxRxAsync(cmd).Wait();
        }

        async Task SimpleCommandTxRxAsync(ISweepCommand cmd)
        {
            var buffer = new byte[cmd.ExpectedAnswerLength];

            // throwaway stuff in the RX buffer!
            serialPort.DiscardInBuffer();

            // TX then RX
            await serialPort.WriteAsync(cmd.Command.Select(x => (byte)x).ToArray(), 0, cmd.Command.Length);
            var bytesRead = await serialPort.ReadAsync(buffer, 0, cmd.ExpectedAnswerLength);

            if(bytesRead == cmd.ExpectedAnswerLength)
            {
                cmd.ProcessResponse(buffer.Select(x => (char)x).ToArray());
            }
            else
            {
                throw new SweepProtocolErrorException(
                    $"Answer was {bytesRead} bytes long instead of expected {cmd.ExpectedAnswerLength}",
                    buffer.Select(x => (char)x).ToArray());
            }           
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

                    if(serialPort.IsOpen)
                        serialPort.Flush();

                    if(!serialPort.IsDisposed)
                        serialPort.Dispose();
                }

                disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
