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
    class MeasuringConfigurationInformation : R2000ProtocolBaseResponse
    {
        [R2000ParameterInfo(R2000ParameterType.Volatile, minVersion: R2000ProtocolVersion.v102)]
        [JsonProperty(PropertyName = "operating_mode")]
        public R2000OperationMode OperationMode { get; set; }

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
    }
}
