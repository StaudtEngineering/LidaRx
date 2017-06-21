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

using System;
using System.Threading.Tasks;
using System.Numerics;

namespace Staudt.Engineering.LidaRx
{
    public interface ILidarScanner : IObservable<ILidarEvent>, IDisposable
    {
        /// <summary>
        /// The scanner's position in 3d space
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// The scanner's orientation in 3d space
        /// </summary>
        Quaternion Orientation { get; set; }

        /// <summary>
        /// Indicates whether this scanner is connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// True when currently scanning
        /// </summary>
        bool IsScanning { get; }

        /// <summary>
        /// Conntect to the scanner
        /// </summary>
        void Connect();

        /// <summary>
        /// Connect to the scanner
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Disconnect from the scanner
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Disconnect from the scanner
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

        /// <summary>
        /// Start scanning
        /// </summary>
        void StartScan();

        /// <summary>
        /// Start scanning
        /// </summary>
        /// <returns></returns>
        Task StartScanAsync();

        /// <summary>
        /// Stop scanning
        /// </summary>
        void StopScan();

        /// <summary>
        /// Stop scanning
        /// </summary>
        /// <returns></returns>
        Task StopScanAsync();

    }
}
