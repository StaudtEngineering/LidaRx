using System.Linq;
using System;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    class SweepProtocolHelpers
    {
        const int AsciiNumberBlockOffset = 48;


        public static int AsciiBytesToInt(char[] input, int offset, int length)
        {
            var numbers = input.Skip(offset).Take(length).Select(x => x - AsciiNumberBlockOffset);

            if (numbers.Any(x => x < 0 || x > 9))
                throw new SweepProtocolError($"Failed to parse ASCII number at {offset} + {length} chars in this message", input);

            return numbers.Reverse().Select((x, i) => x * (int)Math.Pow(10, i)).Sum();
        }

    }
}
