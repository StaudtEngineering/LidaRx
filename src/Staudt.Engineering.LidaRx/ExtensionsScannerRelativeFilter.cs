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
        /// <summary>
        /// Filter by points in an azimuth range
        /// </summary>
        /// <param name="source"></param>
        /// <param name="azimuthStart">Range start azimuth in degrees (in scanner relative coordinates)</param>
        /// <param name="azimuthEnd">Range end azimuth in degrees (in scanner relative coordinates)</param>
        /// <returns></returns>
        public static IObservable<LidarPoint> PointsInAzimuthRange(
            this IObservable<LidarPoint> source,
            float azimuthStart,
            float azimuthEnd)
        {
            // wrap around at 360°
            azimuthStart = azimuthStart % 360;
            azimuthEnd = azimuthEnd % 360;

            if(azimuthStart < 0)
            {
                azimuthStart = 360 + azimuthStart;
            }

            if(azimuthEnd < 0)
            {
                azimuthEnd = 360 + azimuthEnd;
            }


            return source.Where(x => x.Azimuth >= azimuthStart || x.Azimuth <= azimuthEnd);            
        }

        /// <summary>
        /// Filter points that are in a distance range (in scanner relative coordinates)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="distanceMin">Minimum distance in mm</param>
        /// <param name="distanceMax">Maximum distance in mm</param>
        /// <returns></returns>
        public static IObservable<LidarPoint> PointsInDistanceRange(
            this IObservable<LidarPoint> source,
            float distanceMin,
            float distanceMax)
        {
            if (distanceMax < distanceMin)
                throw new ArgumentException("Distance min must be smaller than distance max");

            return source.Where(x => x.Distance >= distanceMin)
                .Where(x => x.Distance <= distanceMax);
        }

        /// <summary>
        /// Filter for at points closer than minDistance in a (sensor centric)  polar range
        /// </summary>
        /// <param name="sweep"></param>
        /// <param name="azimuthStart"></param>
        /// <param name="azimuthEnd"></param>
        /// <param name="minDistance"></param>
        /// <returns></returns>
        public static IObservable<LidarPoint> RadiusRangeMinDistance(
            this IObservable<LidarPoint> sweep,
            float azimuthStart,
            float azimuthEnd,
            float minDistance)
        {
            return sweep.OfType<LidarPoint>()
                .Where(x => x.Distance <= minDistance)
                .PointsInAzimuthRange(azimuthStart: azimuthStart, azimuthEnd: azimuthEnd);
        }

        /// <summary>
        /// Filter for at points further than maxDistance in a (sensor centric)  polar range
        /// </summary>
        /// <param name="source"></param>
        /// <param name="azimuthStart"></param>
        /// <param name="azimuthEnd"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static IObservable<LidarPoint> RadiusRangeMaxDistance(
            this IObservable<LidarPoint> source,
            float azimuthStart,
            float azimuthEnd,
            float maxDistance)
        {
            return source.Where(x => x.Distance >= maxDistance)
                .PointsInAzimuthRange(azimuthStart: azimuthStart, azimuthEnd: azimuthEnd);
        }
    }
}
