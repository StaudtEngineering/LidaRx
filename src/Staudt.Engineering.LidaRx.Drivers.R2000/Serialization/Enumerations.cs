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
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    public enum R2000DeviceFamily
    {
        /// <summary>
        /// Note value 0 or 2 == Reserved
        /// </summary>
        Reserved = 0,
        OMDxxxR2000UHD = 1,
        OMDxxxR2000HD = 3
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
