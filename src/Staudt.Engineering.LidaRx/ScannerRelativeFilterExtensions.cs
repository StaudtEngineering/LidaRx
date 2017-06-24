using System;
using System.Collections.Generic;
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

        public static IObservable<IObservable<IList<LidarPoint>>> BufferByScan(this IObservable<ILidarEvent> source, TimeSpan bufferDuration)
        {
            return source.OfType<LidarPoint>().GroupBy(x => x.Scan).Select(x => x.Buffer(bufferDuration).Where(y => y.Count > 0));
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
