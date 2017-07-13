LidaRx documentation
====================

This is the lidaRx base library. It defines a set of API and some Rx extension methods for "domain specific" tasks.

Common API and interfaces
-------------------------

LidaRx is based on a handful of interfaces and classes:

`ILidarScanner` defines the base functionality that a device driver should provide: [ILidarScanner.cs](ILidarScanner.cs)

`ILidarEvent` is the (currently empty) base interface for every event published by a `ILidarScanner`.

`LidarPoint` represents a single scanned point in 3d space. In addition to absolute carthesian coordinates it provides 
access to the scanner relative polar coordinates as well as to the signal strength information (if provided by scanner)
and to a scan counter.

`LidarScan` represents a single scan (one scanner head revolution) worth of scan data.

`LidarStatusEvent` provides a unified way for device drivers to publish status information to the consuming application.
A single `LidarStatusEvent` contains a `LidarStatusLevel` and a `Message`

About units
-----------

Unless indicated otherwise LidaRx uses metric and SI units. 

- Distances are always mm (single precision 32bit float).
- Angles are in degree (single precision 32bit float)

Rx / Extensions
---------------

LidarRx provides a few extensions to `IObservable<ILidarEvents>` and `IObservable<LidarPoint>` to help you deal with
common tasks.

```csharp
/// Get only lidar points out of the event stream
IObservable<LidarPoint> OnlyLidarPoints(this IObservable<ILidarEvent> source);

/// Filter for LidarStatusEvents
IObservable<LidarStatusEvent> OnlyStatusEvents(this IObservable<ILidarEvent> source);


/// Filter for LidarStatusEvents with a given LidarStatusLevel
public static IObservable<LidarStatusEvent> OnlyStatusEvents(this IObservable<ILidarEvent> source, LidarStatusLevel levelFilter);

/// Buffer the points into scans. 
/// 
/// Note: Introduces a delay of one scan duration
/// Note: buffering is done PER scanner, thus you will get packages of points from a single scanner
/// 
/// Warn: when interrupting scanning the last scan will remain in the buffer and will be delayed 
/// until another scan (from a given scanner) comes in. This means that after a scan pause you'll get
/// one outdated scan round!
public static IObservable<LidarScan> BufferByScan(this IObservable<LidarPoint> source);
```

3D geometry filter:

```csharp        
/// Only returns points that are in a given regular box in carthesian space
public static IObservable<LidarPoint> PointsInBox(this IObservable<LidarPoint> source,
    Vector3 vertex1,
    Vector3 vertex2);
```

Polar / scanner relative coordinates:

```csharp        
/// Filter by points in an azimuth range 
/// Note: handles negative to positive ranges too (ex. -45° to +45°)
public static IObservable<LidarPoint> PointsInAzimuthRange(
	this IObservable<LidarPoint> source,
	float azimuthStart,
    float azimuthEnd);

/// Filter points that are in a distance range (in scanner relative coordinates)
public static IObservable<LidarPoint> PointsInDistanceRange(
    this IObservable<LidarPoint> source,
    float distanceMin,
    float distanceMax);

/// Filter for at points closer than minDistance in a (sensor centric)  polar range
public static IObservable<LidarPoint> RadiusRangeMinDistance(
    this IObservable<LidarPoint> sweep,
    float azimuthStart,
    float azimuthEnd,
    float minDistance);

/// Filter for at points further than maxDistance in a (sensor centric)  polar range
public static IObservable<LidarPoint> RadiusRangeMaxDistance(
    this IObservable<LidarPoint> source,
    float azimuthStart,
    float azimuthEnd,
    float maxDistance);
```

Implementing device drivers
---------------------------

`ILidarScanner` defines the common API that every LidaRx device driver is required to implement. 
To simplify driver implementations LidaRx provides `LidarScannerBase` which implements a part of
the "hard" `IObservale<ILidarEvent>` and `IDisposable` work.

Additionally it provides a simple helper (`TransfromScannerToSystemCoordinates(float azimuth, float distance)`)
for the 3D geometry related to the scanner position and orientation.

