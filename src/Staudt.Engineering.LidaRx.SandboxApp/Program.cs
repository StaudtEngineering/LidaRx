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
                    sweep.SetMotorSpeed(SweepMotorSpeed.Speed2Hz);

                sweep.OfType<LidarErrorEvent>().Subscribe(x => Console.WriteLine("Error {0}", x.Msg));

                    sweep.OfType<LidarPoint>().Buffer(TimeSpan.FromMilliseconds(1000)).Subscribe(x =>
                    {
                         Console.WriteLine($"Got {x.Count} points");
                        
                        
                        //Console.WriteLine($"Got scan frame: X:{x.Point.X} Y:{x.Point.Y} Z:{x.Point.Z}");
                    },
                    () => Console.WriteLine("On completed"));

                    Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Subscribe(x => Console.WriteLine($"Discarded frames: {sweep.DiscardedFrames}"));
                    Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(250)).Subscribe(x => Console.WriteLine($"Discarded bytes: {sweep.DiscardedBytes}"));


                    Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)).Subscribe(x =>
                    {
                        if (sweep.Info.SampleRate == SweepSampleRate.SampleRate1000)
                            sweep.SetSampleRate(SweepSampleRate.SampleRate750);
                        else if(sweep.Info.SampleRate == SweepSampleRate.SampleRate750)
                            sweep.SetSampleRate(SweepSampleRate.SampleRate500);
                        else
                            sweep.SetSampleRate(SweepSampleRate.SampleRate1000);

                        Console.WriteLine($"Sample rate set to {sweep.Info.SampleRate}");
                    });



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
