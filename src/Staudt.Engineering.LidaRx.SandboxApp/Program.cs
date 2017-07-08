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
using Staudt.Engineering.LidaRx.Drivers.R2000;
using System.Net;

namespace Staudt.Engineering.LidaRx.SandboxApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //SweepTest();

            using (var r2000 = new R2000Scanner(IPAddress.Parse("192.168.1.214"), R2000ConnectionType.TCPConnection))
            {
                r2000.Connect();
            }


        }

        private static void SweepTest()
        {
            using (var sweep = new SweepScanner("COM3"))
            {
                sweep.Connect();
                sweep.SetMotorSpeed(SweepMotorSpeed.Speed10Hz);
                sweep.SetSampleRate(SweepSampleRate.SampleRate1000);

                sweep.OfType<LidarErrorEvent>().Subscribe(x => Console.WriteLine("Error {0}", x.Msg));

                /*
                sweep.OfType<LidarPoint>().Buffer(TimeSpan.FromMilliseconds(1000)).Subscribe(x =>
                {
                        Console.WriteLine($"Got {x.Count} points");
                        
                        
                    //Console.WriteLine($"Got scan frame: X:{x.Point.X} Y:{x.Point.Y} Z:{x.Point.Z}");
                },
                () => Console.WriteLine("On completed"));
                */

                sweep.OnlyLidarPoints()
                    .BufferByScan()
                    .Subscribe(scan =>
                    {
                        Console.WriteLine($"Got {scan.Count} points for scan {scan.Scan}");
                    });

                sweep.OfType<LidarPoint>()
                    .Where(x => x.Distance >= 800 && x.Distance <= 1400)
                    .PointsInAzimuthRange(45, 125)
                    .BufferByScan()
                    // put all the points in a list again
                    //.GroupByUntil(x => 1, x => Observable.Timer(TimeSpan.FromMilliseconds(100)))
                    // 
                    //.SelectMany(x => x.ToList())
                    //.Where(x => x.Count >= 10)
                    //.Throttle(TimeSpan.FromMilliseconds(100))
                    //.Where(x => x.Count > 10)
                    /*.SelectMany(group => group.Buffer(2).Timeout(TimeSpan.FromMilliseconds(500)))*/
                    //.Where(scan => scan.Count >= 2)

                    //.Average(x => x.Distance)
                    .Subscribe(x =>
                    {
                        Console.WriteLine($"Distance: {x.Points.Average(y => y.Distance)}  / points {x.Count}");

                    });

                /*
                .SelectMany(x => x.Last().ToList())
                .Subscribe(scan =>
                {
                    Console.WriteLine($"Got something in the range / points: {scan.Count} / average distance: {scan.Average(p => p.Distance)}!");

                    //scan.Buffer(TimeSpan.FromMilliseconds(100)).Where(x => x.Count > 2)
                },
                onCompleted: () => { },
                onError: ex =>
                {
                    // blub 
                });//*/

                /*
                .Where(x => x.Count() > 2)
                //.TimeInterval()
                //.Where(x => x.Interval.Milliseconds < 500)                    
                .Subscribe(_ =>
                {
                    //_

                    //_.Su

                    Console.WriteLine("Got something in the range!");
                });

*/
                sweep.StartScan();

                while (sweep.IsScanning)
                {
                    if (Console.ReadLine() != null)
                        break;
                }
                //ObservableExtensions.Subscribe(sweep, ;

                if (sweep.IsScanning)
                {
                    sweep.StopScan();
                }
            }
        }
    }
}
