Staudt Engineering / lidaRx
---------------------------

A lightweight but powerful Lidar scanner driver and data processing library for 
.NET providing support for multiple Lidar scanners and an intuitive way to process 
samples received by the scanner. 

Features
========

- Unified data processing
- 3D coordinates 
	- Configurable sensor position and orientation
	- Sensor position and orientation updatable at runtime (== your sensor can move, scanned coordinates will be transformed accordingly)
- Vendor agnostic sensor fusion (join data streams from multiple scanners)
- Unified base API: connect/disconnect, start/stop scanning and simple status information
- Polar coordinates filters
	- `PointsInAzimuthRange`
	- `PointsInDistanceRange`
	- `RadiusRangeMinDistance`
	- `RadiusRangeMaxDistance`
- Carthesian coordinates filters
	- `PointsInBox`
- Grouping/Transforming operators
	- `BufferByScan`
- Plus all the power of Rx.NET at your fingertip
- Fully async
- Cross plattform: runs on .NET Core too(!)

Supported devices
=================

- Scanse.io SWEEP
- Pepperl+Fuchs OMDxxx-R2000 device familly (HD and UHD)

Show me some code
=================

```csharp
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
		.OnlyLidarPoints()					// filter away all those status messages
        .Where(pt => pt.Distance > 400)		// unit is mm
		.Where(pt => pt.Distance <= 1500);	// unit is mm

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
```

License
=======

lidaRx is dual licenced. Unless you've made a separate licence agreement with
Staudt Engineering (use contact form on http://www.staudt-engineering.com) you
can use lidaRx under the GNU Lesser General Public License v3.0. The full 
licence is available in this repository.

Support
=======

Commercial support is available, support for open source usage is limited on a 
"free time available" basis, but please feel free to open issues.