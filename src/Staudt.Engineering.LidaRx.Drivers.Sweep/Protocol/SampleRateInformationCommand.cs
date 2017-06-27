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
using System;

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
                throw new SweepProtocolErrorException("Expected answer to LI command, received different header", response);

            // decode the status
            var speedInfo = SweepProtocolHelpers.AsciiBytesToInt(response, 2, 2);

            if (speedInfo < 1 && speedInfo > 3)
                throw new SweepProtocolErrorException("Received sample rate info is out of range ([1;3])", response);

            this.SampleRate = (SweepSampleRate)speedInfo;
        }

        public Nullable<SweepSampleRate> SampleRate { get; private set; } = null;
    }
}
