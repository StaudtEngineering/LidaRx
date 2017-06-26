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

using System.Collections.Generic;
using System.Linq;

namespace Staudt.Engineering.LidaRx
{
    /// <summary>
    /// Represents one full scan of the LIDAR scanner
    /// </summary>
    public class LidarScan
    {
        public LidarScan(long scan, IList<LidarPoint> points)
        {
            this.Scan = scan;
            this.Points = points;
        }

        /// <summary>
        /// All the points in this scan
        /// </summary>
        public IList<LidarPoint> Points { get; private set; }

        /// <summary>
        /// The scan ID
        /// </summary>
        public long Scan { get; private set; }

        /// <summary>
        /// Number of points
        /// </summary>
        public int Count
        {
            get
            {
                return this.Points.Count();
            }
        }

        /// <summary>
        /// Number of points
        /// </summary>
        public long LongCount
        {
            get
            {
                return this.Points.LongCount();
            }

        }
    }
}
