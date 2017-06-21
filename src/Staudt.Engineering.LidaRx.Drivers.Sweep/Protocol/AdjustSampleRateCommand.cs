using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class AdjustSampleRateCommand : ISweepCommand
    {
        public SweepSampleRate TargetSamplingRate { get; private set; }

        public AdjustSampleRateCommand(SweepSampleRate targetSampleRate)
        {
            this.TargetSamplingRate = targetSampleRate;
        }

        public char[] Command
        {
            get
            {
                var result = new char[] { 'L', 'R', '0', '0', '\n' };
                var speedParameter = ((int)this.TargetSamplingRate).ToString("00").ToCharArray();
                Array.Copy(speedParameter, 0, result, 2, 2);
                return result;
            }
        }

        public int ExpectedAnswerLength => 9;

        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'L' and 'R'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolError("Expected answer to LR command, received different header", response);

            // validate the checksum
            if(!SweepProtocolHelpers.StatusChecksumValid(response))
                throw new SweepProtocolError("Checksum is not valid", response);

            // check if the echoed motor speed code matches what we sent
            var echoedSampleRate = (SweepSampleRate)SweepProtocolHelpers.AsciiBytesToInt(response, 2, 2);

            if(echoedSampleRate != this.TargetSamplingRate)
                throw new SweepProtocolError("Echoed speed code missmatched", response);

            // analyze the status
            this.Status = (AdjustSampleRateResult)SweepProtocolHelpers.AsciiBytesToInt(response, 5, 2);
        }

        public Nullable<AdjustSampleRateResult> Status { get; private set; } = null;
    }

    enum AdjustSampleRateResult
    {
        Success = 0,

        /// <summary>
        /// Failed to process command. The command was sent with an invalid parameter. 
        /// Use a valid parameter when trying again
        /// </summary>
        ErrorInvalidParameter = 11,
    }
}
