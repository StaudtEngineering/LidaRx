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
            var t = Task.Run(async () =>
            {
                using (var sweep = new SweepScanner("COM3"))
                {
                    await sweep.ConnectAsync();
                }

            });

            t.Wait();

            

        }
    }
}
