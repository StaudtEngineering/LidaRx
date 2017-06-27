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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;

namespace Staudt.Engineering.LidaRx
{
    public static class BoundingBoxExtensions
    {

        /// <summary>
        /// Only returns points that are in a given regular box in carthesian space
        /// </summary>
        /// <param name="source"></param>
        /// <param name="vertex1">Corner one</param>
        /// <param name="vertex2">Corner two</param>
        /// <returns></returns>
        public static IObservable<LidarPoint> PointsInBox(
            this IObservable<LidarPoint> source,
            Vector3 vertex1,
            Vector3 vertex2)
        {
            var l = new[] { vertex1, vertex2 };

            if (l.Any(x => x == null))
                throw new ArgumentException("Vertexes can't be null");

            var xMin = l.Min(x => x.X);
            var xMax = l.Max(x => x.X);
            var yMin = l.Min(x => x.Y);
            var yMax = l.Max(x => x.Y);
            var zMin = l.Min(x => x.Z);
            var zMax = l.Max(x => x.Z);

            return source
                .Where(p => p.Point.X <= xMax && p.Point.X >= xMin)
                .Where(p => p.Point.Y <= yMax && p.Point.Y >= yMin)
                .Where(p => p.Point.Z <= zMax && p.Point.Z >= zMin);
        }


    }
}
