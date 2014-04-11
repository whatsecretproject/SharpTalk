using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace SharpTalk
{
    /// <summary>
    /// Wraps the functions contained in the FonixTalk TTS engine.
    /// </summary>
    public class FonixTalkEngine : IDisposable
    {
        /// <summary>
        /// Fired when a phoneme event is invoked by the engine.
        /// </summary>
        public event EventHandler<PhonemeEventArgs> Phoneme;

        /// <summary>
        /// The default speaking rate assigned to new instances of the engine.
        /// </summary>
        public const uint DefaultRate = 200;

        /// <summary>
        /// The default voice assigned to new instances of the engine.
        /// </summary>
        public const Speaker DefaultSpeaker = Speaker.Paul;

        #region FonixTalk functions

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DtCallbackRoutine(
            int lParam1,
            int lParam2,
            uint drCallbackParameter,
            uint uiMsg);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechStartupEx(
            out IntPtr handle,
            uint uiDeviceNumber,
            uint dwDeviceOptions,
            DtCallbackRoutine callback, 
            ref IntPtr dwCallbackParameter);

        [DllImport("FonixTalk.dll")]
        [return : MarshalAs(UnmanagedType.Bool)]        
        static extern bool TextToSpeechSelectLang(IntPtr handle, uint lang);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechStartLang(
            [MarshalAs(UnmanagedType.LPStr)]
            string lang);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechSetSpeaker(IntPtr handle, uint speaker);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechGetSpeaker(IntPtr handle, out uint speaker);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechGetRate(IntPtr handle, out uint rate);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechSetRate(IntPtr handle, uint rate);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechSpeakA(IntPtr handle, 
            [MarshalAs(UnmanagedType.LPStr)] 
            string msg, 
            uint flags);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechShutdown(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechPause(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechResume(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechReset(IntPtr handle, 
            [MarshalAs(UnmanagedType.Bool)]
            bool bReset);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechSync(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechSetVolume(IntPtr handle, int type, int volume);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechGetVolume(IntPtr handle, int type, out int volume);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechSetSpeakerParams(IntPtr handle, IntPtr spDefs);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechGetSpeakerParams(IntPtr handle, uint uiIndex,
             out IntPtr ppspCur,
             out IntPtr ppspLoLimit,
             out IntPtr ppspHiLimit,
             out IntPtr ppspDefault);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechAddBuffer(IntPtr handle, ref TTS_BUFFER_T buffer);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechOpenInMemory(IntPtr handle, uint format);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechCloseInMemory(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechReturnBuffer(IntPtr handle, ref TTS_BUFFER_T buffer);

        #endregion

        [DllImport("user32.dll")]
        private static extern uint RegisterWindowMessage(
            [MarshalAs(UnmanagedType.LPStr)]
            string lpString);

        const uint WAVE_FORMAT_1M16 = 0x00000004;

        private const uint TTS_NOT_SUPPORTED = 0x7FFF;
        private const uint TTS_NOT_AVAILABLE = 0x7FFE;
        private const uint TTS_LANG_ERROR = 0x4000;

        private IntPtr handle;
        private DtCallbackRoutine callback;

        private TTS_BUFFER_T buffer;
        private bool bufferActive;

        // Message types
        private uint uiID_Index_Msg = RegisterWindowMessage("DECtalkIndexMessage");
        private uint uiID_Error_Msg = RegisterWindowMessage("DECtalkErrorMessage");
        private uint uiID_Buffer_Msg = RegisterWindowMessage("DECtalkBufferMessage");

        /// <summary>
        /// Initializes a new instance of the engine.
        /// </summary>
        /// <param name="language">The language to load.</param>
        public FonixTalkEngine(string language)
        {
            Init(language, DefaultRate, DefaultSpeaker);
        }

        /// <summary>
        /// Initializes a new instance of the engine in US English.
        /// </summary>
        public FonixTalkEngine()
        {
            Init("US", DefaultRate, DefaultSpeaker);
        }

        /// <summary>
        /// Initialize a new instance of the engine with the specified language, rate, and speaker voice.
        /// </summary>
        /// <param name="language">The language ID.</param>
        /// <param name="rate">The speaking rate to set.</param>
        /// <param name="speaker">The speaker voice to set.</param>
        public FonixTalkEngine(string language, uint rate, Speaker speaker)
        {
            Init(language, rate, speaker);
        }

        /// <summary>
        /// Initializes a new instance of the engine in US English with the specified rate and speaker voice.
        /// </summary>
        /// <param name="rate">The speaking rate to set.</param>
        /// <param name="speaker">The speaker voice to set.</param>
        public FonixTalkEngine(uint rate, Speaker speaker)
        {
            Init("US", rate, speaker);
        }

        private void Init(string lang, uint rate, Speaker spkr)
        {
            callback = new DtCallbackRoutine(this.TTSCallback);
            buffer = new TTS_BUFFER_T();
            bufferActive = false;

            if (lang != LanguageCode.None)
            {
                uint langid = TextToSpeechStartLang(lang);

                if ((langid & TTS_LANG_ERROR) != 0)
                {
                    if (langid == TTS_NOT_SUPPORTED)
                    {
                        throw new FonixTalkException("This version of DECtalk does not support multiple languages.");
                    }
                    else if (langid == TTS_NOT_AVAILABLE)
                    {
                        throw new FonixTalkException("The specified language was not found.");
                    }
                }

                if (!TextToSpeechSelectLang(IntPtr.Zero, langid))
                {
                    throw new FonixTalkException("The specified language failed to load.");
                }
            }
            
            Check(TextToSpeechStartupEx(out handle, 0xFFFFFFFF, 0, callback, ref handle));
            SetSpeaker(spkr);
            SetRate(rate);
            Speak("[:phone on]"); // Enable singing by default
        }

        /// <summary>
        /// Starts speech-to-memory mode and sends audio data to the buffer.
        /// </summary>
        /// <param name="maxSizeInBytes">The maximum size of the buffer in bytes.</param>
        /// <returns></returns>
        public bool StartBuffer(int maxSizeInBytes)
        {
            if (bufferActive) return false;
            buffer = TTS_BUFFER_T.CreateNew(maxSizeInBytes);
            Check(TextToSpeechOpenInMemory(handle, WAVE_FORMAT_1M16));
            Check(TextToSpeechAddBuffer(handle, ref buffer));
            return true;
        }

        /// <summary>
        /// Returns the buffered data if the engine is in speech-to-memory mode. Otherwise, returns null.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            if (!bufferActive) return null;
            Check(TextToSpeechReturnBuffer(handle, ref buffer));
            return buffer.GetBufferBytes();
        }

        /// <summary>
        /// Stops speech-to-memory mode.
        /// </summary>
        /// <returns></returns>
        public bool StopBuffer()
        {
            if (!bufferActive) return false;
            buffer.Dispose();
            Check(TextToSpeechCloseInMemory(handle));
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PhonemeMark
        {
            public byte ThisPhoneme;
            public byte NextPhoneme;
            public ushort Duration;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct PhonemeTag
        {
            [FieldOffset(0)]
            public PhonemeMark PMData;
            [FieldOffset(0)]
            public int DWData;
        }

        private void TTSCallback(
            int lParam1,
            int lParam2,
            uint drCallbackParameter,
            uint uiMsg)
        {
            if (uiMsg == uiID_Index_Msg && this.Phoneme != null)
            {
                PhonemeTag tag = new PhonemeTag();
                tag.DWData = lParam2;
                this.Phoneme(this, new PhonemeEventArgs((char)tag.PMData.ThisPhoneme, tag.PMData.Duration));
            }
            else if (uiMsg == uiID_Buffer_Msg)
            {
                // Handle buffer message
            }
            else if (uiMsg == uiID_Error_Msg)
            {
                // You fucked up!
            }
        }

        /// <summary>
        /// Sets the voice used by the engine.
        /// </summary>
        /// <param name="spkr">The speaker to assign</param>
        public void SetSpeaker(Speaker spkr)
        {
            Check(TextToSpeechSetSpeaker(handle, (uint)spkr));
        }

        /// <summary>
        /// Gets the voice currently being used by the engine.
        /// </summary>
        /// <returns></returns>
        public Speaker GetSpeaker()
        {
            uint spkr;
            Check(TextToSpeechGetSpeaker(handle, out spkr));
            return (Speaker)spkr;
        }

        /// <summary>
        /// Gets the engine's current speaking rate.
        /// </summary>
        /// <returns></returns>
        public uint GetRate()
        {
            uint rate;
            Check(TextToSpeechGetRate(handle, out rate));
            return rate;
        }

        /// <summary>
        /// Sets the engine's current speaking rate.
        /// </summary>
        /// <param name="rate">The rate to assign.</param>
        public void SetRate(uint rate)
        {
            Check(TextToSpeechSetRate(handle, rate));
        }

        /// <summary>
        /// Pauses TTS audio output.
        /// </summary>
        public void Pause()
        {
            Check(TextToSpeechPause(handle));
        }

        /// <summary>
        /// Resumes previously paused TTS audio output.
        /// </summary>
        public void Resume()
        {
            Check(TextToSpeechResume(handle));
        }

        /// <summary>
        /// Flushes all previously queued text from the TTS system and stops any audio output.
        /// </summary>
        public void Reset()
        {
            Check(TextToSpeechReset(handle, true));
        }

        /// <summary>
        /// Blocks until all previously queued text is processed.
        /// </summary>
        public void Sync()
        {
            Check(TextToSpeechSync(handle));
        }

        /// <summary>
        /// Gets the current volume of the TTS system.
        /// </summary>
        /// <returns></returns>
        public int GetVolume()
        {
            int vol;
            Check(TextToSpeechGetVolume(handle, 1, out vol));
            return vol;
        }

        /// <summary>
        /// Sets the volume of the TTS system.
        /// </summary>
        /// <param name="volume">The volume to set</param>
        public void SetVolume(int volume)
        {
            Check(TextToSpeechSetVolume(handle, 1, volume));
        }

        /// <summary>
        /// Causes the engine to begin asynchronously speaking a specified phrase. If the engine is in the middle of speaking, the message passed will be queued.
        /// </summary>
        /// <param name="msg">The phrase for the engine to speak.</param>
        public void Speak(string msg)
        {
            Check(TextToSpeechSpeakA(handle, msg, (uint)SpeakFlags.Force));
        }

        private static void Check(uint code)
        {
            if (code != 0)
            {
                throw new FonixTalkException((MMRESULT)code);
            }
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        ~FonixTalkEngine()
        {
            TextToSpeechShutdown(handle);
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            TextToSpeechShutdown(handle);
            GC.SuppressFinalize(this);
        }
    }
}
