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

namespace Staudt.Engineering.LidaRx.Drivers.Sweep
{
    /// <summary>
    /// Sweep motor speed
    /// </summary>
    public enum SweepMotorSpeed
    {
        Speed0Hz = 0,
        Speed1Hz = 1,
        Speed2Hz = 2,
        Speed3Hz = 3,
        Speed4Hz = 4,
        Speed5Hz = 5,
        Speed6Hz = 6,
        Speed7Hz = 7,
        Speed8Hz = 8,
        Speed9Hz = 9,
        Speed10Hz = 10,
        SpeedUnknown
    }

    /// <summary>
    /// Sweep scan sample rate
    /// </summary>
    public enum SweepSampleRate
    {
        SampleRate500 = 1,
        SampleRate750 = 2,
        SampleRate1000 = 3,
        SampleRateUnknown
    }

    sealed class SweepConfigHelpers
    {
        public static SweepSampleRate IntToSweepSampleRate(int value)
        {
            switch(value)
            {
                case 500:
                    return SweepSampleRate.SampleRate500;
                case 750:
                    return SweepSampleRate.SampleRate750;
                case 1000:
                    return SweepSampleRate.SampleRate1000;
                default:
                    return SweepSampleRate.SampleRateUnknown;
            }
        }
    }
}
