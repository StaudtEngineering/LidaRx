using Staudt.Engineering.LidaRx.Drivers.Sweep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staudt.Engineering.LidaRx.SandboxApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var sweep = new SweepScanner("COM3"))
            {
                sweep.Connect();
            }

        }
    }
}
