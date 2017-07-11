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

using Newtonsoft.Json;
using Staudt.Engineering.LidaRx.Drivers.R2000.Helpers;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    class SensorCapabilitiesInformation : R2000ProtocolBaseResponse
    {
        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "feature_flags")]
        public string[] Features { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic, minVersion: R2000ProtocolVersion.v101)]
        [JsonProperty(PropertyName = "emitter_type")]
        public R2000EmitterType EmitterType { get; set; }

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

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic, minVersion: R2000ProtocolVersion.v101)]
        [JsonProperty(PropertyName = "scan_frequency_min")]
        public uint ScanFrequencyMin { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic, minVersion: R2000ProtocolVersion.v101)]
        [JsonProperty(PropertyName = "scan_frequency_max")]
        public uint ScanFrequencyMax { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic, minVersion: R2000ProtocolVersion.v101)]
        [JsonProperty(PropertyName = "sampling_rate_min")]
        public uint SamplingRateMin { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic, minVersion: R2000ProtocolVersion.v101)]
        [JsonProperty(PropertyName = "sampling_rate_max")]
        public uint SamplingRateMax { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "max_connections")]
        public uint MaxConnections { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "max_scan_sectors")]
        public uint MaxScanSectors { get; set; }

        [R2000ParameterInfo(R2000ParameterType.ReadOnlyStatic)]
        [JsonProperty(PropertyName = "max_data_regions")]
        public uint MaxDataRegions { get; set; }
    }

}
