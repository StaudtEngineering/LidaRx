LidaRx driver for Scanse.IO Sweep
=================================

[![NuGet](https://img.shields.io/nuget/v/Staudt.Engineering.LidaRx.Drivers.Sweep.svg)](https://www.nuget.org/packages/Staudt.Engineering.LidaRx/)
[![NuGet](https://img.shields.io/nuget/vpre/Staudt.Engineering.LidaRx.Drivers.Sweep.svg)](https://www.nuget.org/packages/Staudt.Engineering.LidaRx/)

This device driver adds support for [Scanse.IO's Sweep scanner](http://scanse.io/) to LidaRx. 

Features
--------

- Compatible with Sweep Firmware 1.4 (as per datasheet)
- Strongly typed settings API for motor speed and sample rate
- Automatically pauses/resumes scan when changing parameters (sample rate and rotation frequency) during scan

Roadmap
-------

- (Eventually) support upgrading the firmware

API
---

### Constructor

| Constructor |
| --- |
| `SweepScanner(string portName)` |

| Parameter | Description | Default
|---|---|---|
| `portName` | The OS device specifier for the Serial port / UART, ex. `COM3` on Windows or `/dev/ttyACM0` on Linux |

### Device specific API

#### Motor speed adjustement

| Method |
| --- |
| `void SetMotorSpeed(SweepMotorSpeed targetSpeed, bool smartInterleave)` |
| `async Task SetMotorSpeedAsync(SweepMotorSpeed targetSpeed, bool smartInterleave)` |

| Parameter | Description | Default
| --- | --- | --- |
| `targetSpeed` | A value from `SweepMotorSpeed` enum |
| `smartInterleave` | Whether or not the driver should pause scanning (if running) prior to setting the value. If set to `false` you have to ensure the scan is stopped. | `true`

> Note: Sweep takes up to 10 seconds to stabilize it's speed. The scanner's status LED is blinking during this time. The two methods `WaitForStabilizedMotorSpeed()` (up to 30s) before returning.
> If motor speed stabilization fails an exception is thrown.


| Method |
| --- |
| `bool WaitForStabilizedMotorSpeed(TimeSpan timeout, bool throwOnFail)` |
| `async Task<bool> WaitForStabilizedMotorSpeedAsync(TimeSpan timeout, bool throwOnFail)` |

| Parameter | Description | Default
| --- | --- | --- |
| `timeout` | Wait for timeout to elapse before failing |
| `throwOnFail` | You can suppress the exception throwing by setting this to `false` | `true`

> Note: datasheet states that Sweep takes up to 6s to stabilize. By experience 10 seconds is the least to wait in many cases.

#### Sample rate adjustement

| Method |
| --- |
| `void SetSamplingRate(SweepSampleRate targetRate, bool smartInterleave)` |
| `async Task SetSamplingRateAsync(SweepSampleRate targetRate, bool smartInterleave)` |

| Parameter | Description | Default
| --- | --- | --- |
| `targetRate` | A value from `SweepSampleRate` enum |
| `smartInterleave` | Whether or not the driver should pause scanning (if running) prior to setting the value. If set to `false` you have to ensure the scan is stopped. | `true`


#### Helpers & device status

| Method |
| --- |
| `void UpdateDeviceInfo()` |
| `async Task UpdateDeviceInfoAsync()` |

Retrieves the device information and version information from connected Sweep scanner. 
Information can be access in the `SweepScanner.Info` property:

| Property | Type | Description
| --- | --- | --- |
| Model		| `string` | Should read `Sweep`
| Protocol  | `string` | 
| FirmwareVersion | `string` | The firmware revision
| HardwareVersion | `string` | The hardware revision 
| SerialNumber | `string` | The serial number of your scanner, ex `000002014`
| BitRate | `int` | The bitrate of the serial connection, usually reads 115200
| LaserState | `char` |
| Mode | `char` |
| Diagnostic | `char` |
| MotorSpeed | `SweepMotorSpeed` | The current motor speed (range is 1Hz to 10Hz)
| SampleRate | `SweepSampleRate` | The current sample rate (500Hz, 750Hz or 1000Hz)