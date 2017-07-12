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
                r2000.SetSamplingRate(R2000SamplingRate._8kHz);
                r2000.SetScanFrequency(10);
                r2000.SetSamplingRate(R2000SamplingRate._180kHz);

                r2000.OnlyStatusEvents().Subscribe(ev =>
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Event: {ev.Level.ToString()} / {ev.Message}");
                    Console.ForegroundColor = oldColor;
                });
                

                r2000.OfType<R2000Status>().Subscribe(_ =>
                {
                    Console.WriteLine($"R2000 status:");
                    Console.WriteLine($"\tScan Frequency {_.CurrentScanFrequency} Hz");
                    Console.WriteLine($"\tUptime {_.Uptime} min");
                    Console.WriteLine($"\tTemperature {_.CurrentTemperature} °C");
                    Console.WriteLine($"\tSystem load {_.SystemLoad}%");
                    Console.WriteLine("--------------------------------------------");
                });


                r2000.OnlyLidarPoints()
                    .BufferByScan()
                    .Subscribe(x =>
                    {
                        //Console.WriteLine($"Scans per second: {x.Count}");
                        Console.WriteLine($"Got {x.Count} points for scan {x.Scan} / Min {x.Points.Min(pt => pt.Azimuth)} :: Max {x.Points.Max(pt => pt.Azimuth)}");

                    });

                        /*
                        r2000.OnlyLidarPoints()
                            .BufferByScan()
                            .Where(x => x.Count < 25200)
                            .Subscribe(x =>
                            {
                                //Console.WriteLine($"Scans per second: {x.Count}");
                                Console.WriteLine($"Got {x.Count} points for scan {x.Scan} / Min {x.Points.Min(pt => pt.Azimuth)} :: Max {x.Points.Max(pt => pt.Azimuth)}");

                                var gaps = new List<LidarPoint>();

                                LidarPoint lastPoint = x.Points.First();
                                var expectedIncr = 360f / 25150;

                                foreach(var pt in x.Points.Skip(1))
                                {
                                    var diff = pt.Azimuth - lastPoint.Azimuth - expectedIncr;

                                    if (diff > 0)
                                    {
                                        gaps.Add(lastPoint);
                                        gaps.Add(pt);
                                    }

                                    lastPoint = pt;
                                }

                                if (gaps.Count > 0)
                                {
                                    Console.WriteLine("Gaps: " +
                                    gaps.Skip(1).Aggregate(
                                    gaps.First().Azimuth.ToString(),
                                    (acc, pt) => acc + ", " + pt.Azimuth));

                                }
                            });*/

                                            r2000.OnlyLidarPoints()
                    .Where(x => x.Distance >= 400 && x.Distance <= 1200)
                    .PointsInAzimuthRange(-45, 45)
                    .BufferByScan()                          
                    .Subscribe(x =>
                    {                        
                        Console.WriteLine($"Distance: {x.Points.Average(y => y.Distance)}  / points {x.Count}");
                    });
                    

                r2000.StartScan();


                while (true)
                {
                    var line = Console.ReadLine();


                    if (line == "q")
                        break;
                    else if(line == "t")
                    {
                        if (r2000.IsScanning)
                            r2000.StopScan();
                        else
                            r2000.StartScan();
                    }
                }

                r2000.Disconnect();
            }




        }

        private async static void SweepTest()
        {
            using (var sweep = new SweepScanner("COM1"))
            {
                await sweep.ConnectAsync();
                await sweep.SetMotorSpeedAsync(SweepMotorSpeed.Speed10Hz);
                await sweep.SetSampleRateAsync(SweepSampleRate.SampleRate1000);

                await sweep.StartScanAsync();

                // log errors to the console
                sweep.OnlyStatusEvents(LidarStatusLevel.Error).Subscribe(ev =>
                {
                    Console.WriteLine($"Error: {ev.Message}");
                });

                // using the data stream for multiple subscriptions
                var pointsBetween400and1000mm = sweep
                    .OnlyLidarPoints()                  // filter away all those status messages
                    .Where(pt => pt.Distance > 400)     // unit is mm
                    .Where(pt => pt.Distance <= 1500);  // unit is mm

                // buffer in 1second long samples
                pointsBetween400and1000mm
                    .Buffer(TimeSpan.FromSeconds(1000))
                    .Subscribe(buffer =>
                    {
                        Console.WriteLine($"{buffer.Count} points in [400;1000]mm range per second");
                    });

                // this narrows down the point stream to points in the -45 to +45 degree range
                pointsBetween400and1000mm
                    .PointsInAzimuthRange(-45, 45)
                    .Subscribe(pt =>
                    {
            // write the points to disk?!
        });

                // buffer the lidar points in scans
                sweep.OnlyLidarPoints()
                    .BufferByScan()
                    .Subscribe(scan =>
                    {
                        Console.WriteLine($"Got {scan.Count} points for scan {scan.Scan}");
                        Console.WriteLine($"Most distant point: {scan.Points.Max(pt => pt.Distance)}mm");
                        Console.WriteLine($"Closest point: {scan.Points.Min(pt => pt.Distance)}mm");
                    });


                Console.ReadLine();      // wait here 'till user hits the enter key
                sweep.StopScan();
            }

            using (var sweep = new SweepScanner("COM3"))
            {
                sweep.Connect();
                sweep.SetMotorSpeed(SweepMotorSpeed.Speed10Hz);
                sweep.SetSampleRate(SweepSampleRate.SampleRate1000);

                sweep.OfType<LidarStatusEvent>().Subscribe(x => Console.WriteLine("Error {0}", x.Message));

                /*
                sweep.OfType<LidarPoint>().Buffer(TimeSpan.FromMilliseconds(1000)).Subscribe(x =>
                {
                        Console.WriteLine($"Got {x.Count} points");
                        
                        
                    //Console.WriteLine($"Got scan frame: X:{x.Point.X} Y:{x.Point.Y} Z:{x.Point.Z}");
                },
                () => Console.WriteLine("On completed"));
                */

                sweep.OnlyStatusEvents(LidarStatusLevel.Error).Subscribe(ev =>
                {
                    Console.WriteLine($"Error: {ev.Message}");
                });

                // using the data stream for multiple subscriptions
                var pointsBetween400and1000mm = sweep.OnlyLidarPoints()
                    .Where(x => x.Distance > 400 && x.Distance <= 1500); // unit is mm

                // buffer in 1second long samples
                pointsBetween400and1000mm
                    .Buffer(TimeSpan.FromSeconds(1000))
                    .Subscribe(buffer =>
                    {
                        Console.WriteLine($"{buffer.Count} points in [400;1000]mm range / s");
                    });

                // this narrows down the point stream to points in the -45 to +45 degree range
                pointsBetween400and1000mm
                    .PointsInAzimuthRange(-45, 45)
                    .Subscribe(pt =>
                    {
                        // write the points to disk?!
                    });

                // buffer the lidar points in scans
                sweep.OnlyLidarPoints()
                    .BufferByScan()
                    .Subscribe(scan =>
                    {
                        Console.WriteLine($"Got {scan.Count} points for scan {scan.Scan}");
                        Console.WriteLine($"Most distant point: {scan.Points.Max(pt => pt.Distance)}mm");
                        Console.WriteLine($"Closest point: {scan.Points.Min(pt => pt.Distance)}mm");
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
