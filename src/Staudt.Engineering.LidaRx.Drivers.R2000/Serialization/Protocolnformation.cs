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

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    /// <summary>
    /// Answer dto for get_protocol_information
    /// 
    /// Url:
    /// http://*sensor IP address*/cmd/get_protocol_info
    /// </summary>
    class ProtocolInformation
    {
        [JsonProperty(PropertyName = "protocol_name")]
        public string ProtocolName { get; set; }

        [JsonProperty(PropertyName = "version_major")]
        public uint VersionMajor { get; set; }

        [JsonProperty(PropertyName = "version_minor")]
        public uint VersionMinor { get; set; }

        [JsonProperty(PropertyName = "commands")]
        public string[] AvailableCommands { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public R2000ErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_text")]
        public string ErrorText { get; set; }
    }
}
