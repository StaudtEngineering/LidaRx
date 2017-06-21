using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class MotorInformationCommand : ISweepCommand
    {
        public char[] Command => new[] { 'M', 'I', '\n' };
        public int ExpectedAnswerLength => 5;

        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'M' and 'Z'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolError("Expected answer to MZ command, received different header", response);

            // decode the status
            var speedInfo = SweepProtocolHelpers.AsciiBytesToInt(response, 2, 2);

            if(speedInfo < 0 && speedInfo > 11)
                throw new SweepProtocolError("Received speed info is out of range ([0;10]Hz)", response);

            this.MotorSpeed = (SweepMotorSpeed)speedInfo;
        }

        public Nullable<SweepMotorSpeed> MotorSpeed { get; private set; } = null;
    }
}
