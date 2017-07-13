Staudt Engineering / lidaRx
===========================

[![Build status](https://ci.appveyor.com/api/projects/status/sy2l3c91cvlnd1p6?svg=true)](https://ci.appveyor.com/project/pysco68/lidarx) 
[![NuGet](https://img.shields.io/nuget/v/Staudt.Engineering.LidaRx.svg)](https://www.nuget.org/packages/Staudt.Engineering.LidaRx/)
[![NuGet](https://img.shields.io/nuget/vpre/Staudt.Engineering.LidaRx.svg)](https://www.nuget.org/packages/Staudt.Engineering.LidaRx/)

A lightweight but powerful Lidar scanner driver and data processing library for 
.NET providing support for multiple Lidar scanners and an intuitive way to process 
samples received by the scanner. 

Features
--------

- Unified data processing
- 3D coordinates 
	- Configurable sensor position and orientation
	- Sensor position and orientation updatable at runtime (== your sensor can move, scanned coordinates will be transformed accordingly)
- Vendor agnostic sensor fusion (join data streams from multiple scanners)
- Comprehensive API an helpers: [LidaRx readme](src/Staudt.Engineering.LidaRx/README.md)
	- Unified base API for scanners: connect/disconnect, start/stop scanning and simple status information
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
- Cross plattform support: 
	- .NET Standard 1.5 / .NET Core 1.0
	- .NET Standard 2.0 / .NET Core 2.0
	- .NET 4.6+ 

Supported devices
-----------------

- Scanse.io SWEEP [Readme](src/Staudt.Engineering.LidaRx.Drivers.Sweep/README.md)
- Pepperl+Fuchs OMDxxx-R2000 device familly (HD and UHD) [Readme](src/Staudt.Engineering.LidaRx.Drivers.R2000/README.md)

Show me some code!
------------------

Here's a  simple example using a Scanse.io Sweep sensor. 

Basically the programm connects to the Sweep on `Com1`, set the motor speed to 10Hz and the sample rate to 1kHz

```csharp
using (var sweep = new SweepScanner("COM1"))
{
    await sweep.ConnectAsync();
    await sweep.SetMotorSpeedAsync(SweepMotorSpeed.Speed10Hz);
    await sweep.SetSampleRateAsync(SweepSampleRate.SampleRate1000);

	await sweep.StartScanAsync();
```

...then the programm registers for `LidarStatusEvent` with the `LidarStatusLevel.Error` and logs the
messages to the console.

```csharp
	// log errors to the console
	sweep.OnlyStatusEvents(LidarStatusLevel.Error).Subscribe(ev =>
	{
		Console.WriteLine($"Error: {ev.Message}");
	});
```

Next, the programm takes the LIDAR point stream and filters away all the points that are outside of the distance
range 40cm to 100cm (imagine two concentric circles around the scanner; only points between them propagate in the
resulting `Observable<LidarPoint>` stream)

```csharp
    // using the data stream for multiple subscriptions
    var pointsBetween400and1000mm = sweep
		.OnlyLidarPoints()					// filter away all those status messages
        .Where(pt => pt.Distance > 400)		// unit is mm
		.Where(pt => pt.Distance <= 1000);	// unit is mm
```

Finally we use the restrained stream as source for a Rx `Buffer()` which collects all the points into consecutive
"1 second long" buffers. In the second part the program uses the `pointsBetween400and1000mm` stream and restricts 
it further to points in the azimut range of -45 to +45 degree.

```csharp
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
```

This part uses the full stream of `LidarPoint` from the scanner but instead of buffering on a time basis as in
the code above it buffers by scan (basically per scanner head revolution).

```csharp
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

Roadmap
-------

- Usable implementation of 3D colision test (think of `PointsIn3DModel(string pathToStlFile)` )
- Points to object matching
- Point agglomeration tracking 

License
-------

lidaRx is dual licenced. Unless you've made a separate licence agreement with Staudt 
Engineering (for example because you can't stand the LGPL / use contact form on 
http://www.staudt-engineering.com) you can use lidaRx under the GNU Lesser General 
Public License v3.0. The full licence text is available in this repository: [LICENCE](LICENCE)

Support & Contribution
----------------------

Commercial support and device driver developement service is available, support
for open source usage is limited on a "free time available" basis, but please 
feel free to open issues or pull requests if you can think of something that
would make this library more awesome :)