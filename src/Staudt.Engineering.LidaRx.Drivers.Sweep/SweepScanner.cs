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
        static SemaphoreSlim _semaphoreSlimConnect = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Creates a new driver for a Scanse.io Sweep LIDAR scanner
        /// </summary>
        /// <param name="serialPort"></param>
        public SweepScanner(string portName)
        {
            serialPort = new SerialPortStream(portName, 115200, 8, Parity.None, StopBits.One);
            serialPort.ErrorReceived += SerialPort_ErrorReceived;

//            serialPort.ReadTimeout = 500;
            //serialPort.WriteTimeout = 500;
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            // TODO?
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
            SimpleCommandTxRx(dxCommand);

            // gather information about the device
            var idCommand = new DeviceInformationCommand();
            var ivCommand = new VersionInformationCommand();
            SimpleCommandTxRx(idCommand);            
            SimpleCommandTxRx(ivCommand);

            ExtractDeviceAndVersionInformation(idCommand, ivCommand);

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
            await SimpleCommandTxRxAsync(dxCommand);

            // gather information about the device
            var idCommand = new DeviceInformationCommand();
            var ivCommand = new VersionInformationCommand();

            await SimpleCommandTxRxAsync(idCommand);
            await SimpleCommandTxRxAsync(ivCommand);

            ExtractDeviceAndVersionInformation(idCommand, ivCommand);

            _semaphoreSlimConnect.Release();
        }

        private void ExtractDeviceAndVersionInformation(DeviceInformationCommand idCommand, VersionInformationCommand ivCommand)
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

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public override void StartScan()
        {
            throw new NotImplementedException();
        }

        public override Task StartScanAsync()
        {
            throw new NotImplementedException();
        }

        public override void StopScan()
        {
            // make sure we're in a halfway known state...
            var dxCommand = new StopDataAcquisitionCommand();
            SimpleCommandTxRx(dxCommand);
        }

        public async override Task StopScanAsync()
        {
            // make sure we're in a halfway known state...
            var dxCommand = new StopDataAcquisitionCommand();
            await SimpleCommandTxRxAsync(dxCommand);
        }

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
                        var dxCommand = new StopDataAcquisitionCommand();
                        SimpleCommandTxRx(dxCommand);
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
