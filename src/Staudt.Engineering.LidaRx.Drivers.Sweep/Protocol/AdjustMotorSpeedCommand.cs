using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class AdjustMotorSpeedCommand : ISweepCommand
    {
        public SweepMotorSpeed TargetSpeed { get; private set; }

        public AdjustMotorSpeedCommand(SweepMotorSpeed targetSpeed)
        {
            this.TargetSpeed = targetSpeed;
        }

        public char[] Command
        {
            get
            {
                var result = new char[] { 'M', 'S', '0', '0', '\n' };
                var speedParameter = ((int)this.TargetSpeed).ToString("00").ToCharArray();
                Array.Copy(speedParameter, 0, result, 2, 2);
                return result;
            }
        }

        public int ExpectedAnswerLength => 9;

        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'M' and 'S'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolError("Expected answer to MS command, received different header", response);

            // validate the checksum
            if(!SweepProtocolHelpers.StatusChecksumValid(response))
                throw new SweepProtocolError("Checksum is not valid", response);

            // check if the echoed motor speed code matches what we sent
            var echoedSpeedcode = (SweepMotorSpeed)SweepProtocolHelpers.AsciiBytesToInt(response, 2, 2);

            if(echoedSpeedcode != this.TargetSpeed)
                throw new SweepProtocolError("Echoed speed code missmatched", response);

            // analyze the status
            this.Status = (AdjustMotorSpeedResult)SweepProtocolHelpers.AsciiBytesToInt(response, 5, 2);
        }

        public Nullable<AdjustMotorSpeedResult> Status { get; private set; } = null;
    }

    enum AdjustMotorSpeedResult
    {
        Success = 0,

        /// <summary>
        /// Failed to process command. The command was sent with an invalid parameter. 
        /// Use a valid parameter when trying again
        /// </summary>
        ErrorInvalidParameter = 11,

        /// <summary>
        /// Failed to process command. Motor speed has not yet stabilized to the previous setting. 
        /// Motor speed setting NOT changed to new value.Wait until motor speed has stabilized
        /// before trying to adjust it again.
        /// </summary>
        ErrorNotYetStabilized = 12

    }
}
