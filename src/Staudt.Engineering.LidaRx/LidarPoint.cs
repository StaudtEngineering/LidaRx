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

using System.Numerics;

namespace Staudt.Engineering.LidaRx
{
    public class LidarPoint : ILidarEvent
    {
        public LidarPoint(Vector3 point, float azimuth, float distance, byte amplitude, long scan, ILidarScanner scanner)
        {
            this.Point = point;
            this.Amplitude = amplitude;
            this.Azimuth = azimuth;
            this.Distance = distance;
            this.Scan = scan;
            this.Scanner = scanner;
        }

        /// <summary>
        /// The point in carthesian 3d space
        /// </summary>
        public Vector3 Point { get; }

        /// <summary>
        /// The azimuth of this sample in degrees
        /// </summary>
        public float Azimuth { get; }

        /// <summary>
        /// The measured distance in mm
        /// </summary>
        public float Distance { get; }

        /// <summary>
        /// Signal strength, higher values equal better confidence
        /// </summary>
        public byte Amplitude { get; }

        /// <summary>
        /// A scan counter, overflows
        /// </summary>
        public long Scan { get; }

        /// <summary>
        /// Scanner which emitted this point
        /// </summary>
        public ILidarScanner Scanner { get; }
    }
}
