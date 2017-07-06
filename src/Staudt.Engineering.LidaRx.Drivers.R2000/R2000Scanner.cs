using System;
using System.Threading.Tasks;

namespace Staudt.Engineering.LidaRx.Drivers.R2000
{
    public class R2000Scanner : LidarScannerBase
    {
        public override bool Connected => throw new NotImplementedException();

        public override bool IsScanning => throw new NotImplementedException();

        public override void Connect()
        {
            throw new NotImplementedException();
        }

        public override Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public override void StartScan()
        {
            throw new NotImplementedException();
        }

        public override Task StartScanAsync()
        {
            throw new NotImplementedException();
        }

        public override void StopScan()
        {
            throw new NotImplementedException();
        }

        public override Task StopScanAsync()
        {
            throw new NotImplementedException();
        }
    }
}
