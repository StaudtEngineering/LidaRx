using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep
{
    sealed class ProtocolCommands
    {
        #region simple commands

        public readonly char[] StartAcquisition = new[] { 'D', 'S', '\n' };
        public readonly char[] StopAcquisition = new[] { 'D', 'X', '\n' };
        public readonly char[] ResetDevice = new[] { 'R', 'R', '\n' };

        #endregion

        #region Complex commangs

        /// <summary>
        /// Build an adjust motor speed command
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static char[] MotorSpeedAdjust(SweepMotorSpeed speed)
        {
            if (speed == SweepMotorSpeed.SpeedUnknown)
                throw new ArgumentException("You can't set an unknown motor speed", "speed");

            var command = new[] { 'M', 'S' };

            // conver the speed enum to ASCII chars
            var speedParameter = speed.ToString("00").ToCharArray();

            // concat radix command, param and LF
            var result = command.Concat(speedParameter).Concat(new[] { '\n' }).ToArray();
            return result;
        }
             
        /// <summary>
        /// Build a sample rate adjust command
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public static char[] SampleRateAdjust(SweepSampleRate sampleRate)
        {
            if (sampleRate == SweepSampleRate.SampleRateUnknown)
                throw new ArgumentException("You can't set an unknown sample rate", "sampleRate");

            var command = new[] { 'L', 'R' };

            // conver the speed enum to ASCII chars
            var speedParameter = sampleRate.ToString("00").ToCharArray();

            // concat radix command, param and LF
            var result = command.Concat(speedParameter).Concat(new[] { '\n' }).ToArray();
            return result;
        }
        #endregion
    }
}
