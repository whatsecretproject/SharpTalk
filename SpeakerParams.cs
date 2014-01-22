using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SharpTalk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpeakerParams
    {
        /// <summary>
        /// The sex of the speaker
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public Sex Sex;

        /// <summary>
        /// Smoothness, in %
        /// </summary>
        public short Smoothness;

        /// <summary>
        /// Assertiveness, in %
        /// </summary>
        public short Assertiveness;

        /// <summary>
        /// Average pitch, in Hz
        /// </summary>
        public short AveragePitch;

        /// <summary>
        /// Breathiness, in decibels (dB)
        /// </summary>
        public short Breathiness;

        /// <summary>
        /// Richness, in %
        /// </summary>
        public short Richness;

        /// <summary>
        /// Number of fixed samples of open glottis
        /// </summary>
        public short NumFixedSampOG;

        /// <summary>
        /// Laryngealization, in %
        /// </summary>
        public short Laryngealization;

        /// <summary>
        /// Head size, in %
        /// </summary>
        public short HeadSize;

        /// <summary>
        /// Fourth formant resonance frequency, in Hz
        /// </summary>
        public short Formant4ResFreq;

        /// <summary>
        /// Fourth formant bandwidth, in Hz
        /// </summary>
        public short Formant4Bandwidth;

        /// <summary>
        /// Fifth formant resonance frequency, in Hz
        /// </summary>
        public short Formant5ResFreq;

        /// <summary>
        /// Fifth formant bandwidth, in Hz
        /// </summary>
        public short Formant5Bandwidth;

        /// <summary>
        /// Parallel fourth formant frequency, in Hz
        /// </summary>
        public short Parallel4Freq;

        /// <summary>
        /// Parallel fifth formant frequency, in Hz
        /// </summary>
        public short Parallel5Freq;

        /// <summary>
        /// Gain of frication source, in dB
        /// </summary>
        public short GainFrication;

        /// <summary>
        /// Gain of aspiration source, in dB
        /// </summary>
        public short GainAspiration;

        /// <summary>
        /// Gain of voicing source, in dB
        /// </summary>
        public short GainVoicing;

        /// <summary>
        /// Gain of nasalization, in dB
        /// </summary>
        public short GainNasalization;

        /// <summary>
        /// Gain of cascade formant resonator 1, in dB
        /// </summary>
        public short GainCFR1;

        /// <summary>
        /// Gain of cascade formant resonator 2, in dB
        /// </summary>
        public short GainCFR2;

        /// <summary>
        /// Gain of cascade formant resonator 3, in dB
        /// </summary>
        public short GainCFR3;

        /// <summary>
        /// Gain of cascade formant resonator 4, in dB
        /// </summary>
        public short GainCFR4;

        /// <summary>
        /// Loudness, gain input to cascade 1st formant in dB
        /// </summary>
        public short Loudness;

        /// <summary>
        /// (f0-dependent spectral tilt in % of max)frm 75 to 90 for 10to8
        /// In other words, not the slightest clue what this does.
        /// </summary>
        public short SpectralTilt;

        /// <summary>
        /// Baseline fall, in Hz
        /// </summary>
        public short BaselineFall;

        /// <summary>
        /// Lax breathiness, in %
        /// </summary>
        public short LaxBreathiness;

        /// <summary>
        /// Quickness, in %
        /// </summary>
        public short Quickness;

        /// <summary>
        /// Hat rise, in Hz
        /// </summary>
        public short HatRise;

        /// <summary>
        /// Stress rise, in Hz
        /// </summary>
        public short StressRise;

        /// <summary>
        /// Glottal speed
        /// </summary>
        public short GlottalSpeed;

        /// <summary>
        /// Output gain multiplier for FVTM
        /// </summary>
        public short OutputGainMultiplier;
    }

    public enum Sex : short
    {
        Female = 0,
        Male = 1
    }
}
