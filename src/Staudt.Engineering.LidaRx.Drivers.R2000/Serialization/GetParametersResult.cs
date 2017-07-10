#region Copyright
//
// This file is part of Staudt Engineering's LidaRx library
//
// Copyright (C) 2017 Yannic Staudt / Staudt Engieering
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using Staudt.Engineering.LidaRx.Drivers.R2000.Helpers;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    class BasicSensorInformation
    {
        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "device_family")]
        public R2000DeviceFamily DeviceFamilly { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "part")]
        public string Part { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "serial")]
        public string Serial { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "revision_fw")]
        public string RevisionFirmware { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "revision_hw")]
        public string RevisionHardware { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
        [JsonProperty(PropertyName = "user_tag")]
        public string UserDefinedTag { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
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
        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "feature_flags")]
        public string[] Features { get; set; }

    //    [R2000ParameterType(R2000ParameterType.ReadOnlyStatic)]
    //    [JsonProperty(PropertyName = "emitter_type")]
    //    public R2000EmitterType EmitterType { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "radial_range_min")]
        public double RadialRangeMin { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "radial_range_max")]
        public double RadialRangeMax { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "radial_resolution")]
        public double RadialResolution { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "angular_fov")]
        public double AngularFieldOfView { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "angular_resolution")]
        public double AngularResulution { get; set; }

        //[R2000ParameterType(R2000ParameterType.ReadOnlyStatic)]
        //[JsonProperty(PropertyName = "scan_frequency_min")]
        //public uint ScanFrequencyMin { get; set; }

        //[R2000ParameterType(R2000ParameterType.ReadOnlyStatic)]
        //[JsonProperty(PropertyName = "scan_frequency_max")]
        //public uint ScanFrequencyMax { get; set; }

        //[R2000ParameterType(R2000ParameterType.ReadOnlyStatic)]
        //[JsonProperty(PropertyName = "sampling_rate_min")]
        //public uint SamplingRateMin { get; set; }

        //[R2000ParameterType(R2000ParameterType.ReadOnlyStatic)]
        //[JsonProperty(PropertyName = "sampling_rate_max")]
        //public uint SamplingRateMax { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "max_connections")]
        public uint MaxConnections { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "max_scan_sectors")]
        public uint MaxScanSectors { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
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
        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
        [JsonProperty(PropertyName = "ip_mode")]
        public R2000IpMode IpMode { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
        [JsonProperty(PropertyName = "ip_address")]
        public string IPAdress { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
        [JsonProperty(PropertyName = "subnet_mask")]
        public string SubnetMask { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
        [JsonProperty(PropertyName = "gateway")]
        public string Gateway { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnly)]
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
        //[R2000ParameterType(R2000ParameterType.Volatile)]
        //[JsonProperty(PropertyName = "operating_mode")]
        //public R2000OperationMode OperationMode { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
        [JsonProperty(PropertyName = "scan_frequency")]
        public double ScanFrequency { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
        [JsonProperty(PropertyName = "scan_direction")]
        public R2000ScanDirection ScanDirection { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadWrite)]
        [JsonProperty(PropertyName = "samples_per_scan")]
        public uint SamplesPerScan { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnly)]
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
