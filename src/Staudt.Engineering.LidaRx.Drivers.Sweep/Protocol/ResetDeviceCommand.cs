using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class ResetDeviceCommand : ISweepCommand
    {
        public char[] Command => new[] { 'R', 'R', '\n' };
        public int ExpectedAnswerLength => 0;

        public void ProcessResponse(char[] response)
        {
            // no response to RR
        }
    }
}
}
