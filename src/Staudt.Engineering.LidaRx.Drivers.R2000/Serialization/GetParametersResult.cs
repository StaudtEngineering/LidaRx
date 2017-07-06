using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Staudt.Engineering.LidaRx.Drivers.R2000.Helpers;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    class BasicSensorInformation
    {
        [JsonProperty(PropertyName = "device_family")]
        public R2000DeviceFamily DeviceFamilly { get; set; }

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; set; }

        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; }

        [JsonProperty(PropertyName = "part")]
        public string Part { get; set; }

        [JsonProperty(PropertyName = "serial")]
        public string Serial { get; set; }

        [JsonProperty(PropertyName = "revision_fw")]
        public string RevisionFirmware { get; set; }

        [JsonProperty(PropertyName = "revision_hw")]
        public string RevisionHardware { get; set; }

        [JsonProperty(PropertyName = "user_tag")]
        public string UserDefinedTag { get; set; }

        [JsonProperty(PropertyName = "user_notes")]
        public string UserDefinedNotes { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public R2000ErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_text")]
        public string ErrorText { get; set; }
    }

    public enum R2000DeviceFamily
    {
        /// <summary>
        /// Note value 0 or 2 == Reserved
        /// </summary>
        Reserved = 0,
        OMDxxxR2000UHD = 1,
        OMDxxxR2000HD = 3
    }

    class SensorCapabilitiesInformation
    {
        [JsonProperty(PropertyName = "features_flags")]
        public string[] Features { get; set; }

        [JsonProperty(PropertyName = "emitter_type")]
        public R2000EmitterType EmitterType { get; set; }

        [JsonProperty(PropertyName = "radial_range_min")]
        public double RadialRangeMin { get; set; }

        [JsonProperty(PropertyName = "radial_range_max")]
        public double RadialRangeMax { get; set; }

        [JsonProperty(PropertyName = "radial_resolution")]
        public double RadialResolution { get; set; }

        [JsonProperty(PropertyName = "angular_fov")]
        public double AngularFieldOfView { get; set; }

        [JsonProperty(PropertyName = "angular_resolution")]
        public double AngularResulution { get; set; }

        [JsonProperty(PropertyName = "scan_frequency_min")]
        public uint ScanFrequencyMin { get; set; }

        [JsonProperty(PropertyName = "scan_frequency_max")]
        public uint ScanFrequencyMax { get; set; }

        [JsonProperty(PropertyName = "sampling_rate_min")]
        public uint SamplingRateMin { get; set; }

        [JsonProperty(PropertyName = "sampling_rate_max")]
        public uint SamplingRateMax { get; set; }

        [JsonProperty(PropertyName = "max_connections")]
        public uint MaxConnections { get; set; }

        [JsonProperty(PropertyName = "max_scan_sectors")]
        public uint MaxScanSectors { get; set; }

        [JsonProperty(PropertyName = "max_data_regions")]
        public uint MaxDataRegions { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public R2000ErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_text")]
        public string ErrorText { get; set; }
    }

    public enum R2000EmitterType
    {
        Undefined = 0,

        /// <summary>
        /// 660nm
        /// </summary>
        RedLaser = 1,

        /// <summary>
        /// 905nm
        /// </summary>
        InfraredLaser = 2
    }

    class EthernetConfigurationInformation
    {
        [JsonProperty(PropertyName = "ip_mode")]
        public R2000IpMode IpMode { get; set; }

        [JsonProperty(PropertyName = "ip_address")]
        public string IPAdress { get; set; }

        [JsonProperty(PropertyName = "subnet_mask")]
        public string SubnetMask { get; set; }

        [JsonProperty(PropertyName = "gateway")]
        public string Gateway { get; set; }

        [JsonProperty(PropertyName = "mac_address")]
        public string MacAddress { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public R2000ErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_text")]
        public string ErrorText { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum R2000IpMode
    {
        /// <summary>
        /// Configuration using IP_Address, Subnet_Mask and GateWay info
        /// </summary>
        [EnumMember(Value = "static")]
        Static,

        /// <summary>
        /// Automatic configuration using "Zero Configuration Networking"
        /// </summary>
        [EnumMember(Value = "autoip")]
        AutoIp,

        /// <summary>
        /// Automatic configuration using a DHCP server
        /// </summary>
        [EnumMember(Value = "dhcp")]
        Dhcp
    }

    class MeasuringConfigurationInformation
    {
        [JsonProperty(PropertyName = "operating_mode")]
        public R2000OperationMode OperationMode { get; set; }

        [JsonProperty(PropertyName = "scan_frequency")]
        public double ScanFrequency { get; set; }

        [JsonProperty(PropertyName = "scan_direction")]
        public R2000ScanDirection ScanDirection { get; set; }

        [JsonProperty(PropertyName = "samples_per_scan")]
        public uint SamplesPerScan { get; set; }

        [JsonProperty(PropertyName = "scan_frequency_measured")]
        public double CurrentScanFrequency { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public R2000ErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_text")]
        public string ErrorText { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum R2000OperationMode
    {
        [EnumMember(Value = "measure")]
        Measure,

        [EnumMember(Value = "transmitter_off")]
        TransmitterOff
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum R2000ScanDirection
    {
        [EnumMember(Value = "cw")]
        Clockwise,

        [EnumMember(Value = "ccw")]
        CounterClockwise
    }
}
