using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Staudt.Engineering.LidaRx
{
    public class LidarPoint : ILidarEvent
    {
        public LidarPoint(Vector3 point, float azimuth, float distance, byte amplitude, long scan)
        {
            this.Point = point;
            this.Amplitude = amplitude;
            this.Azimuth = azimuth;
            this.Distance = distance;
            this.Scan = scan;
        }

        /// <summary>
        /// The point in carthesian 3d space
        /// </summary>
        Vector3 Point { get; }

        /// <summary>
        /// The azimuth of this sample in degrees
        /// </summary>
        float Azimuth { get; }

        /// <summary>
        /// The measured distance in mm
        /// </summary>
        float Distance { get; }

        /// <summary>
        /// Signal strength, higher values equal better confidence
        /// </summary>
        byte Amplitude { get; }

        /// <summary>
        /// A scan counter, overflows
        /// </summary>
        long Scan { get; }
    }
}
