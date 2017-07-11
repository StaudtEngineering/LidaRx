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
        /// Filter for LidarStatusEvents
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<LidarStatusEvent> OnlyStatusEvents(this IObservable<ILidarEvent> source)
        {
            return source.OfType<LidarStatusEvent>();
        }

        /// <summary>
        /// Filter for LidarStatusEvents with a given LidarStatusLevel
        /// </summary>
        /// <param name="source"></param>
        /// <param name="levelFilter"></param>
        /// <returns></returns>
        public static IObservable<LidarStatusEvent> OnlyStatusEvents(this IObservable<ILidarEvent> source, LidarStatusLevel levelFilter)
        {
            return source.OfType<LidarStatusEvent>().Where(x => x.Level == levelFilter);
        }

        /// <summary>
        /// Helper class
        /// </summary>
        class PointByScanBuffer : ConcurrentDictionary<long, List<LidarPoint>>
        {
            public long LastScan = -1;
            public readonly SemaphoreSlim ScanPublishLock = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Buffer the points into scans. 
        /// 
        /// Note: Introduces a delay of one scan duration
        /// Note: buffering is done PER scanner, thus you will get packages of points from a single scanner
        /// 
        /// Warn: when interrupting scanning the last scan will remain in the buffer and will be delayed 
        /// until another scan (from a given scanner) comes in. This means that after a scan pause you'll get
        /// one outdated scan round!
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<LidarScan> BufferByScan(this IObservable<LidarPoint> source)
        {
            var scanStream = new Subject<LidarScan>();
            var bufferCollector = new ConcurrentDictionary<ILidarScanner, PointByScanBuffer>();            

            source.Subscribe(x =>
                {
                    var pointBuffer = bufferCollector.GetOrAdd(x.Scanner, (k) => new PointByScanBuffer());
                    var bufferedList = pointBuffer.GetOrAdd(x.Scan, (k) => new List<LidarPoint>());
                    bufferedList.Add(x);

                    // we can publish the last scan as it's completed (júst got a point from n+1)
                    if (pointBuffer.LastScan < x.Scan)
                    {
                        pointBuffer.ScanPublishLock.Wait(); // acq. lock

                        if (pointBuffer.LastScan < x.Scan)
                        {
                            List<LidarPoint> lastScanPoints = null;

                            if (pointBuffer.TryRemove(pointBuffer.LastScan, out lastScanPoints))
                            {
                                var toPublish = new LidarScan(pointBuffer.LastScan, lastScanPoints.AsReadOnly());
                                scanStream.OnNext(toPublish);
                            }

                            // remember the current scan (we update this no matter if we actually had a
                            // previous scan to escape our start condition)
                            pointBuffer.LastScan = x.Scan;
                        }

                        pointBuffer.ScanPublishLock.Release();
                    }
                });

            return scanStream;
        }

    }
}
