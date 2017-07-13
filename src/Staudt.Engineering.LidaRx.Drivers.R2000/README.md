LidaRx driver for Pepperl+Fuchs OMDxxx-R2000 devices
====================================================

This device driver adds support for Pepperl+Fuchs OMDxxx-R2000 family 2D scanners.

Features
--------

- Support for Firmwares 0.97+ (PFSDP protocol v1.00 to v1.02) (automatic handling of protocol differences)
- Support both HD and UHD models
- TCP wire protocol for data stream
- Discards invalid data points
- Driver periodically fetches device status
- Automatic data stream watchdog reset (driver selects optimal reset method based on firmware version)
- Stronly typed settings API for motor speed and sample rate (includes validation of supplied parameters)

Roadmap
-------

- Implement UDP connector
- Complete API
	- HMI configuration
	- Network configuration
	- Device control (reboot and factory reset)
	- Read/Write user notes and user tag
	- Measuring configuration
		- Set transmitter mode
		- Change scan direction
- Implement ZeroConf / Bonjour device discovery

Example
-------

```csharp
// note: R2000 is set to query DHCP for its IP configuration in this example
var address = IPAddress.Parse("192.168.1.214");

using (var r2000 = new R2000Scanner(address, R2000ConnectionType.TCPConnection))
{
    r2000.Connect();
    r2000.SetScanFrequency(10);
    r2000.SetSamplingRate(R2000SamplingRate._252kHz);

	// write the periodic (every 10s) status response to the console
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
			Console.WriteLine($"Got {x.Count} points for scan {x.Scan}");
		});

	Console.ReadLine();      // wait here 'till user hits the enter key
	sweep.StopScan();
}
```

> Note: the driver published `R2000Status` `ILidarEvents` periodically. You can register them in the observable stream 
> as you would with `LidarPoints` or `LidarEvents`:

API
---

### Constructor

| Constructor |
| --- |
| `R2000Scanner(IPAddress address, R2000ConnectionType connectionType, int fetchStatusInterval)` |

| Parameter | Description | Default
|---|---|---|
| `address` | The IP Address of the R2000 scanner |
| `conntectionType` | Type of connector to use for the scan data streaming. Note: `R2000ConnectionType.UDPConnection` is not implemented |
| `fetchStatusInterval` | How often the driver should retrieve the device status, in ms. Set to 0 to disable. | 10000ms

### Device specific API

#### Motor speed adjustement

| Method |
| --- |
| `void SetScanFrequency(double frequencyHz)` |
| `async Task SetScanFrequencyAsync(double frequencyHz)` |

| Parameter | Description | Default
| --- | --- | --- |
| `frequencyHz` | The target scan frequency |

> Note: R2000 devices can change the scan frequency during data streaming but will set the `UnstableRotation` flag in the transfered 
> frames as long as the target speed is not reached.

> Note: The datasheet states that the parameter is a `double` however the firmware seems to Round() the parameter.

> Note: The driver checks if the supplied value is in the supported range on devices with protocol versions >= 1.01

#### Sample rate adjustement

| Method |
| --- |
| `void SetSamplingRate(R2000SamplingRate targetSamplingRate))` |
| `async Task SetSamplingRateAsync(R2000SamplingRate targetSamplingRate))` |

| Parameter | Description | Default
| --- | --- | --- |
| `targetSamplingRate` | A value from `R2000SamplingRate` enum |

> Note: the driver uses a configuration table to check if the selected value is supported given the scanner's scan frequency 
> see table 2.1 in the datascheet for valid values. Please consider that R2000 HD devices do not support sampling rates above 84kHz.

> Note: The driver checks if the supplied value is in the supported range on devices with protocol versions >= 1.01

> By setting `R2000SamplingRate.AutomaticMaximum` the driver will select the highest possible sampling rate for the connected scanner
> given the configured scan frequency.

#### Helpers & device status

| Method |
| --- |
| `R2000Status FetchScannerStatus()` |
| `async Task<R2000Status> FetchScannerStatusAsync()` |

Retrieves the devices status and returns a `R2000Status` object.

| Property | Type | Description
| --- | --- | --- |
| CurrentScanFrequency | `double` | Measured scan frequency in Hz
| SystemLoad  | `uint` | Scanner SoC load in % 
| CurrentTemperature | `int` | Device temperature in °C
| Uptime | `uint` | Scanner uptime in minutes