using Staudt.Engineering.LidaRx.Drivers.R2000.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.R2000
{
    /// <summary>
    /// Sampling rates to choose from
    /// </summary>
    public enum R2000SamplingRate
    {
        /// <summary>
        /// Let the driver choose the maximum sampling rate based on the current scan frequency
        /// </summary>
        AutomaticMaximum,
        _252kHz = 250_000,
        _210kHz = 210_000,
        _180kHz = 180_000,
        _140kHz = 140_000,
        _120kHz = 120_000,
        _105kHz = 105_000,
        _90kHz = 90_000,
        _84kHz = 84_000,
        _83kHz = 83_000,
        _82kHz = 82_000,
        _81kHz = 81_000,
        _80kHz = 80_000,
        _72kHz = 72_000,
        _60kHz = 60_000,
        _45kHz = 45_000,
        _40kHz = 40_000,
        _36kHz = 36_000,
        _30kHz = 30_000,
        _24kHz = 24_000,
        _23kHz = 23_000,
        _20kHz = 20_000,
        _18kHz = 18_000,
        _12kHz = 12_000,
        _9kHz = 9_000,
        _8kHz = 8_000,
        _6kHz = 6_000,
        _5kHz = 5_000,
        _4kHz = 4_000
    }

    /// <summary>
    /// The R2000 has a tabled spec. specifying which sampling rate is okay for a given scan frequency and vice-versa
    /// </summary>
    class SamplingRateSetting
    {
        public R2000SamplingRate MaximumSampleRate { get; private set; }
        public uint SamplesPerScan { get; private set; }
        public double MaximumScanFrequency { get; private set; }
        public R2000DeviceFamily DeviceFamily { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="df"></param>
        /// <param name="sr"></param>
        /// <param name="sps"></param>
        /// <param name="msf"></param>
        private SamplingRateSetting(R2000DeviceFamily df, R2000SamplingRate sr, uint sps, double msf)
        {
            this.MaximumSampleRate = sr;
            this.SamplesPerScan = sps;
            this.DeviceFamily = df;
            this.MaximumScanFrequency = msf;
        }

        /// <summary>
        /// Valid configurations
        /// </summary>
        public readonly static IEnumerable<SamplingRateSetting> Table = new[]
        {
                #region UHD devices
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 25200, 10),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 16800, 15),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 12600, 20),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 10080, 25),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 8400, 30),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 7200, 35),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 6300, 40),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 5600, 45),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._252kHz, 5040, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._210kHz, 4200, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._180kHz, 3600, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._140kHz, 2800, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._120kHz, 2400, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._105kHz, 2100, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._90kHz, 1800, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._84kHz, 1680, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._72kHz, 1440, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._60kHz, 1200, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._45kHz, 900, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._40kHz, 800, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._36kHz, 720, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._30kHz, 600, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._24kHz, 480, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._23kHz, 450, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._20kHz, 400, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._18kHz, 360, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._12kHz, 240, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._9kHz, 180, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._8kHz, 144, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._6kHz, 120, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._5kHz, 90, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000UHD, R2000SamplingRate._4kHz, 72, 50),
                #endregion

                #region HD devices
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._84kHz, 8400, 10),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._80kHz, 7200, 11),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._82kHz, 6300, 13),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._84kHz, 5600, 15),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._81kHz, 5040, 16),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._84kHz, 4200, 20),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._83kHz, 3600, 23),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._84kHz, 2800, 30),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._84kHz, 2400, 35),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._84kHz, 2100, 40),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._83kHz, 1800, 46),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._84kHz, 1680, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._72kHz, 1440, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._60kHz, 1200, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._45kHz, 900, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._40kHz, 800, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._36kHz, 720, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._30kHz, 600, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._24kHz, 480, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._23kHz, 450, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._20kHz, 400, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._18kHz, 360, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._12kHz, 240, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._9kHz, 180, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._8kHz, 144, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._6kHz, 120, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._5kHz, 90, 50),
                new SamplingRateSetting(R2000DeviceFamily.OMDxxxR2000HD, R2000SamplingRate._4kHz, 72, 50)
                #endregion
            };

    }
}
