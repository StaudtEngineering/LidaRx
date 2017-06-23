using Staudt.Engineering.LidaRx.Drivers.Sweep;
using Staudt.Engineering.LidaRx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;

namespace Staudt.Engineering.LidaRx.SandboxApp
{
    class Program
    {
        static void Main(string[] args)
        {

                using (var sweep = new SweepScanner("COM3"))
                {
                    sweep.Connect();

                    sweep.OfType<LidarErrorEvent>().Subscribe(x => Console.WriteLine("Error {0}", x.Msg));

                    sweep.OfType<LidarPoint>().GroupBy(x => x.Scan).Subscribe(x =>
                    {

                        x.Buffer(TimeSpan.FromMilliseconds(1000)).Subscribe(y =>
                        {
                            if(y.Count > 0)
                                Console.WriteLine($"Got {y.Count} points, out for scan {x.Key}");
                        });


                        
                        
                        //Console.WriteLine($"Got scan frame: X:{x.Point.X} Y:{x.Point.Y} Z:{x.Point.Z}");
                    },
                    () => Console.WriteLine("On completed"));

                    Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Subscribe(x => Console.WriteLine($"Discarded frames: {sweep.DiscardedFrames}"));
                    Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(250)).Subscribe(x => Console.WriteLine($"Discarded bytes: {sweep.DiscardedBytes}"));

                sweep.StartScan();

                    while(sweep.IsScanning)
                    {
                    if (Console.ReadLine() != null)
                        break;
                    }
                    //ObservableExtensions.Subscribe(sweep, ;

                if(sweep.IsScanning)
                {

                }
                }
            

        }
    }
}
