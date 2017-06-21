using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    /// <summary>
    /// 
    /// </summary>
    public class SweepProtocolError : Exception
    {
        /// <summary>
        /// Wire format message that generated this exception
        /// </summary>
        public char[] SweepMessage { get; private set; }

        public SweepProtocolError(string message, char[] protocolMessage) : base(message)
        {
            this.SweepMessage = protocolMessage;
        }
    }
}
