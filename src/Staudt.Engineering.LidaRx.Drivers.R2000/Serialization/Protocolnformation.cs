using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    /// <summary>
    /// Answer dto for get_protocol_information
    /// 
    /// Url:
    /// http://*sensor IP address*/cmd/get_protocol_info
    /// </summary>
    class Protocolnformation
    {
        public string ProtocolName { get; set; }
        public uint VersionMajor { get; set; }
        public uint VersionMinor { get; set; }
        public string AvailableCommands { get; set; }
    }
}
