using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class SampleRateInformationCommand : ISweepCommand
    {
        public char[] Command => new[] { 'L', 'I', '\n' };
        public int ExpectedAnswerLength => 5;

        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'M' and 'Z'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolError("Expected answer to LI command, received different header", response);

            // decode the status
            var speedInfo = SweepProtocolHelpers.AsciiBytesToInt(response, 2, 2);

            if (speedInfo < 1 && speedInfo > 3)
                throw new SweepProtocolError("Received sample rate info is out of range ([1;3])", response);

            this.SampleRate = (SweepSampleRate)speedInfo;
        }

        public Nullable<SweepSampleRate> SampleRate { get; private set; } = null;
    }
}
