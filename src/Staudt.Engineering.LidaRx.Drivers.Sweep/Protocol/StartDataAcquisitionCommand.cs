using System;
using System.Linq;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class StartDataAcquisitionCommand : ISweepCommand
    {
        public char[] Command => new[] { 'D', 'S', '\r' };
        public int ExpectedAnswerLength => 6;


        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'M' and 'Z'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolError("Expected answer to DS command, received different header", response);

            // validated the checksum
            if (!SweepProtocolHelpers.StatusChecksumValid(response))
                throw new SweepProtocolError("Checksum is not valid", response);

            // decode the status
            var r = new string(response, 2, 2);

            this.Success = (r == "00");
        }

        public bool? Success { get; private set; } = null;
    }    
}
