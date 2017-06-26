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
using System.Linq;
using System.Reactive.Linq;

namespace Staudt.Engineering.LidaRx
{
    /// <summary>
    /// Rx helpers for scanner relative position/boundary scanning
    /// </summary>
    public static class ScannerRelativeFilterExtensions
    {

        public static IObservable<LidarPoint> PointsInAzimuthRange(
            this IObservable<ILidarEvent> source,
            float azimuthStart,
            float azimuthEnd)
        {
            return source.OfType<LidarPoint>()
                .Where(x => x.Azimuth >= azimuthStart)
                .Where(x => x.Azimuth <= azimuthEnd);
        }

        /// <summary>
        /// Filter for at points closer than minDistance in a (sensor centric)  polar range in at least minConsecutiveScans consecutive scans 
        /// </summary>
        /// <param name="sweep"></param>
        /// <param name="azimuthStart"></param>
        /// <param name="azimuthEnd"></param>
        /// <param name="minDistance"></param>
        /// <param name="consecutiveScansTimeout"></param>
        /// <param name="minConsecutiveScans"></param>
        /// <returns></returns>
        public static IObservable<LidarPoint> RadiusRangeMinDistance(
            this IObservable<ILidarEvent> sweep,
            float azimuthStart,
            float azimuthEnd,
            float minDistance,
            int minConsecutiveScans,
            TimeSpan consecutiveScansTimeout)
        {
            return sweep.OfType<LidarPoint>()
                .Where(x => x.Distance <= minDistance)
                .Where(x => x.Azimuth >= azimuthStart && x.Azimuth <= azimuthEnd)
                .GroupBy(x => x.Scan)
                .Buffer(consecutiveScansTimeout, minConsecutiveScans)
                // discard buffers with less than two scans
                .Where(x => x.Count >= minConsecutiveScans)
                // flatten the buffer list         
                .SelectMany(x => x)
                // flatten the grouping  
                .SelectMany(x => x);
        }


        public static IObservable<LidarPoint> RadiusRangeMinDistance(
            this IObservable<ILidarEvent> sweep,
            float azimuthStart,
            float azimuthEnd,
            float minDistance)
        {
            return sweep.RadiusRangeMinDistance(azimuthStart, azimuthEnd, minDistance, 2, TimeSpan.FromMilliseconds(2000));
        }

    
    }
}
