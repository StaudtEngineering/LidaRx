using System;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class VersionInformationCommand : ISweepCommand
    {
        public char[] Command => new[] { 'I', 'V', '\n' };
        public int ExpectedAnswerLength => 21;

        public void ProcessResponse(char[] response)
        {
            // check that the first two chars are 'M' and 'Z'
            if (response[0] != Command[0] || response[1] != Command[1])
                throw new SweepProtocolError("Expected answer to IV command, received different header", response);

            // decode the frame
            this.Model = new string(response, 2, 5);
            this.ProtocolVersion = SweepProtocolHelpers.AsciiBytesToInt(response, 7, 2) / 10.0f;
            this.FirmwareVersion = SweepProtocolHelpers.AsciiBytesToInt(response, 9, 2) / 10.0f;
            this.HardwareVersion = SweepProtocolHelpers.AsciiBytesToInt(response, 11, 1);
            this.SerialNumber = SweepProtocolHelpers.AsciiBytesToInt(response, 12, 8);
        }

        public string Model { get; private set; } = null;
        public Nullable<float> ProtocolVersion { get; private set; } = null;
        public Nullable<float> FirmwareVersion { get; private set; } = null;
        public Nullable<int> HardwareVersion { get; private set; } = null;
        public Nullable<int> SerialNumber { get; private set; } = null;
    }
}
