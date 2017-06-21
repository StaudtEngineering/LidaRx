using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Staudt.Engineering.LidaRx
{
    public abstract class LidarScannerBase : ILidarScanner
    {
        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }

        /// <summary>
        /// The scan counter
        /// </summary>
        protected long ScanCounter = 0;

        /// <summary>
        /// Instantiation with a known position and orientation in 3d space
        /// </summary>
        /// <param name="position"></param>
        /// <param name="orientation"></param>
        public LidarScannerBase(Vector3 position, Quaternion orientation)
        {
            this.observers = new List<IObserver<ILidarEvent>>();

            this.Position = position;
            this.Orientation = orientation;            
        }

        /// <summary>
        /// Instantiation given that the scanner IS the coordinate space origin
        /// </summary>
        public LidarScannerBase()
            : this(Vector3.Zero, Quaternion.Identity)
        {
        }        

        public abstract void StartScan();
        public abstract Task StartScanAsync();
        public abstract void StopScan();
        public abstract Task StopScanAsync();

        #region Helpers

        /// <summary>
        /// Translates scanner relative LIDAR coordinates to the true spacial position relative to where
        /// the scanner is positioned and how it's orientation is
        /// </summary>
        /// <param name="azimuth"></param>
        /// <param name="distance"></param>
        protected Vector3 TransfromScannerToSystemCoordinates(float azimuth, float distance)
        {
            var rad = azimuth * Math.PI / 180;
            var scannerRelativeX = (float)(distance * Math.Cos(rad));
            var scannerRelativeY = (float)(distance * Math.Sin(rad));

            var vector = new Vector3(scannerRelativeX, scannerRelativeY, 0);

            var translated = this.Position + vector;
            var rotated = Vector3.Transform(translated, this.Orientation);

            return rotated;
        }

        #endregion

        #region Base implementation of the Rx stuff

        /// <summary>
        /// Observers that have subscribed
        /// </summary>
        private List<IObserver<ILidarEvent>> observers;

        /// <summary>
        /// Subscript to updates from this lidar scanner
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<ILidarEvent> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }

        /// <summary>
        /// Helper class for single subscriptions
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<ILidarEvent>> _observers;
            private IObserver<ILidarEvent> _observer;

            public Unsubscriber(List<IObserver<ILidarEvent>> observers, IObserver<ILidarEvent> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }


        #endregion
    }
}
