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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Linq;
using System.Threading;

namespace Staudt.Engineering.LidaRx
{
    /// <summary>
    /// All the base extensions
    /// </summary>
    public static class BaseExtensions
    {
        /// <summary>
        /// Get only lidar points out of the event stream
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<LidarPoint> OnlyLidarPoints(this IObservable<ILidarEvent> source)
        {
            return source.OfType<LidarPoint>();
        }

        /// <summary>
        /// Filter for LidarErrorEvents
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<LidarErrorEvent> OnlyErrorEvents(this IObservable<ILidarEvent> source)
        {
            return source.OfType<LidarErrorEvent>();
        }

        /// <summary>
        /// Buffer the points into scans. Introduces a delay of one scan duration
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<LidarScan> BufferByScan(this IObservable<LidarPoint> source)
        {
            var scanStream = new Subject<LidarScan>();
            var bufferCollector = new ConcurrentDictionary<long, List<LidarPoint>>();
            long lastScan = -1;

            var scanPublishLock = new SemaphoreSlim(1, 1);

            source.Subscribe(x =>
                {
                    List<LidarPoint> bufferedList = bufferCollector.GetOrAdd(x.Scan, (k) => new List<LidarPoint>());
                    bufferedList.Add(x);

                    // we can publish the last scan as it's completed (júst got a point from n+1)
                    if (lastScan < x.Scan)
                    {
                        scanPublishLock.Wait(); // acq. lock
                        List<LidarPoint> lastScanPoints = null;

                        if (bufferCollector.TryRemove(lastScan, out lastScanPoints))
                        {
                            var toPublish = new LidarScan(lastScan, lastScanPoints.AsReadOnly());
                            scanStream.OnNext(toPublish);
                        }

                        // remember the current scan (we update this no matter if we actually had a
                        // previous scan to escape our start condition)
                        lastScan = x.Scan;

                        scanPublishLock.Release();
                    }
                });

            return scanStream;
        }

    }
}
