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
        public R2000EmitterType EmitterType { get; set; }
        public double RadialRangeMin { get; set; }
        public double RadialRangeMax { get; set; }
        public double RadialResolution { get; set; }
        public double AngularFieldOfView { get; set; }
        public double AngularResulution { get; set; }
        public uint ScanFrequencyMin { get; set; }
        public uint ScanFrequencyMax { get; set; }
        public uint SamplingRateMin { get; set; }
        public uint SamplingRateMax { get; set; }
        public uint MaxConnections { get; set; }
        public uint MaxScanSectors { get; set; }
        public uint MaxDataRegions { get; set; }

        public R2000ErrorCode ErrorCode { get; set; }
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
        public R2000IpMode IpMode { get; set; }
        public string IPAdress { get; set; }
        public string SubnetMask { get; set; }
        public string Gateway { get; set; }
        public string MacAddress { get; set; }
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
        public R2000OperationMode OperationMode { get; set; }
        public double ScanFrequency { get; set; }
        public R2000ScanDirection ScanDirection { get; set; }
        public uint SamplesPerScan { get; set; }
        public double CurrentScanFrequency { get; set; }
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
