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
        /// Creates a new driver for a Scanse.io Sweep LIDAR scanner
        /// </summary>
        /// <param name="serialPort"></param>
        public SweepScanner(string portName)
        {
            serialPort = new SerialPortStream(portName, 115200, 8, Parity.None, StopBits.One);
            serialPort.ErrorReceived += SerialPort_ErrorReceived;

            serialPort.ReadTimeout = 500;
            serialPort.WriteTimeout = 500;
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            // TODO?
        }

        public override void Connect()
        {
            // already connected
            if (Connected)
                return;

            serialPort.Open();

            var mrCommand = new Protocol.MotorReadyCommand();
            SimpleCommandTxRx(mrCommand);

            var miCommand = new MotorInformationCommand();
            SimpleCommandTxRx(miCommand);

            var liCommand = new SampleRateInformationCommand();
            SimpleCommandTxRx(liCommand);

            var idCommand = new DeviceInformationCommand();
            SimpleCommandTxRx(idCommand);

            var ivCommand = new VersionInformationCommand();
            SimpleCommandTxRx(ivCommand);

            var msCommand = new AdjustMotorSpeedCommand(SweepMotorSpeed.Speed5Hz);
            SimpleCommandTxRx(msCommand);

            var lrCommand = new AdjustSampleRateCommand(SweepSampleRate.SampleRate1000);
            SimpleCommandTxRx(lrCommand);
            //rialPort.Read()

        }

        void SimpleCommandTxRx(ISweepCommand cmd)
        {
            var buffer = new char[cmd.ExpectedAnswerLength];

            // TX then RX
            serialPort.Write(cmd.Command, 0, cmd.Command.Length);            
            serialPort.Read(buffer, 0, cmd.ExpectedAnswerLength);

            // process the response
            cmd.ProcessResponse(buffer);
        }

        public override Task ConnectAsync()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override Task StopScanAsync()
        {
            throw new NotImplementedException();
        }

        #region Async serial RX

        #endregion

        #region Asyns serial TX

        #endregion

        #region IDisposable Support
        private bool disposedValue = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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
