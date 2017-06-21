﻿using System.Linq;
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

        public static char AsciiByteToChar(char[] input, int offset)
        {
            var value = (char)(input[offset] - AsciiNumberBlockOffset);

            if (value < 0 || value > 9)
                throw new SweepProtocolError($"Failed to parse ASCII number at {offset} + 1 chars in this message", input);

            return value;
        }

        public static bool StatusChecksumValid(char[] input)
        {
            // take the 3 relevant bytes
            var copy = input.Skip(input.Length - 4).Take(3).ToArray();

            // checksum formula as per sweep user manual
            var chechsumCalculated = ((copy[0] + copy[1]) & 0x3f) + 0x30;
            var checksumByteValue = copy[2];

            return (checksumByteValue == chechsumCalculated);
        }
    }
}
