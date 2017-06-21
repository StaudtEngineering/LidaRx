using System;
using System.Threading.Tasks;
using System.Numerics;

namespace Staudt.Engineering.LidaRx
{
    public interface ILidarScanner : IObservable<ILidarEvent>
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
