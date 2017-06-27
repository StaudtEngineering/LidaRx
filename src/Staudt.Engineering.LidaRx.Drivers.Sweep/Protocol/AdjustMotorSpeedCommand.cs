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

using System;
using Staudt.Engineering.LidaRx.Drivers.Sweep.Exceptions;

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
                throw new SweepProtocolErrorException("Expected answer to MS command, received different header", response);

            // validate the checksum
            if(!SweepProtocolHelpers.StatusChecksumValid(response))
                throw new SweepProtocolErrorException("Checksum is not valid", response);

            // check if the echoed motor speed code matches what we sent
            var echoedSpeedcode = (SweepMotorSpeed)SweepProtocolHelpers.AsciiBytesToInt(response, 2, 2);

            if(echoedSpeedcode != this.TargetSpeed)
                throw new SweepProtocolErrorException("Echoed speed code missmatched", response);

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
