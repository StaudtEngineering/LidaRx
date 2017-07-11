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
using Staudt.Engineering.LidaRx.Drivers.R2000.Helpers;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    class BasicSensorInformation : R2000ProtocolBaseResponse
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
    }    
}
