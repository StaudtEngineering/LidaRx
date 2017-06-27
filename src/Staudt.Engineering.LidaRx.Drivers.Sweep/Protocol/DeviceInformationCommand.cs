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
    class DeviceInformationCommand : ISweepCommand
    {
        public char[] Command => new[] { 'I', 'D', '\n' };
        public int ExpectedAnswerLength => 18;

        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'M' and 'Z'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolErrorException("Expected answer to ID command, received different header", response);

            // decode the frame
            this.SerialBitrate = SweepProtocolHelpers.AsciiBytesToInt(response, 2, 6);
            this.LaserState = SweepProtocolHelpers.AsciiByteToChar(response, 8);
            this.Mode = SweepProtocolHelpers.AsciiByteToChar(response, 9);
            this.Diagnostic = SweepProtocolHelpers.AsciiByteToChar(response, 10);
            var speedInfo = SweepProtocolHelpers.AsciiBytesToInt(response, 11, 2);

            if (speedInfo < 0 && speedInfo > 11)
                throw new SweepProtocolErrorException("Received speed info is out of range ([0;10]Hz)", response);

            this.MotorSpeed = (SweepMotorSpeed)speedInfo;

            this.SampleRate = SweepProtocolHelpers.AsciiBytesToInt(response, 13, 4);
        }

        public Nullable<int> SerialBitrate { get; private set; } = null;
        public Nullable<char> LaserState { get; private set; } = null;
        public Nullable<char> Mode { get; private set; } = null;
        public Nullable<char> Diagnostic { get; private set; } = null;
        public Nullable<SweepMotorSpeed> MotorSpeed { get; private set; } = null;
        public Nullable<int> SampleRate { get; private set; } = null;
    }
}
