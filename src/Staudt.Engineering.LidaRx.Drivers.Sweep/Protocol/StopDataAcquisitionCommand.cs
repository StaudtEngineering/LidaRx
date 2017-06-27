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

using Staudt.Engineering.LidaRx.Drivers.Sweep.Exceptions;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class StopDataAcquisitionCommand : ISweepCommand
    {
        public char[] Command => new[] { 'D', 'X', '\r' };
        public int ExpectedAnswerLength => 6;


        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'M' and 'Z'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolErrorException("Expected answer to DX command, received different header", response);

            // validated the checksum
            if (!SweepProtocolHelpers.StatusChecksumValid(response))
                throw new SweepProtocolErrorException("Checksum is not valid", response);

            // decode the status
            var r = new string(response, 2, 2);

            this.Success = (r == "00");
        }

        public bool? Success { get; private set; } = null;
    }

    
}
