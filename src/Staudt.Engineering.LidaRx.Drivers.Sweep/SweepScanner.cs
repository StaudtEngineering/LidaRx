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
using System;
using System.Threading.Tasks;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep
{
    public class SweepScanner : LidarScannerBase
    {
        SerialPortStream serialPort;

        /// <summary>
        /// True when connected with a Sweep
        /// </summary>
        public override bool Connected => serialPort?.IsOpen ?? false;

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
        }

        public override void Connect()
        {
            serialPort.Open();

        }

        public override Task ConnectAsync()
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
    }
}
