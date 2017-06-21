using System;
using System.Linq;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class MotorReadyCommand : ISweepCommand
    {

        public char[] Command => new[] { 'M', 'Z', '\r' };
        public int ExpectedAnswerLength => 5;


        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'M' and 'Z'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolError("Expected answer to MZ command, received different header", response);

            // decode the status
            var r = new string(response, 2, 2);

            this.DeviceReady = (r == "00");
        }

        #region results

        public bool? DeviceReady { get; private set; } = null;

        #endregion

    }

    
}
