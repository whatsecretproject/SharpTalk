using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        public const TTSVoice DefaultSpeaker = TTSVoice.Paul;

        #region FonixTalk functions

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DtCallbackRoutine(
            int lParam1,
            int lParam2,
            uint drCallbackParameter,
            uint uiMsg);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechStartupEx(
            out IntPtr handle,
            uint uiDeviceNumber,
            uint dwDeviceOptions,
            DtCallbackRoutine callback,
            ref IntPtr dwCallbackParameter);

        [DllImport("FonixTalk.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool TextToSpeechSelectLang(IntPtr handle, uint lang);

        [DllImport("FonixTalk.dll")]
        static extern uint TextToSpeechStartLang(
            [MarshalAs(UnmanagedType.LPStr)]
            string lang);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechSetSpeaker(IntPtr handle, TTSVoice speaker);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechGetSpeaker(IntPtr handle, out TTSVoice speaker);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechGetRate(IntPtr handle, out uint rate);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechSetRate(IntPtr handle, uint rate);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechSpeakA(IntPtr handle,
            [MarshalAs(UnmanagedType.LPStr)] 
            string msg,
            uint flags);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechShutdown(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechPause(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechResume(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechReset(IntPtr handle,
            [MarshalAs(UnmanagedType.Bool)]
            bool bReset);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechSync(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechSetVolume(IntPtr handle, int type, int volume);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechGetVolume(IntPtr handle, int type, out int volume);

        [DllImport("FonixTalk.dll")]
        static extern unsafe MMRESULT TextToSpeechSetSpeakerParams(IntPtr handle, IntPtr spDefs);

        [DllImport("FonixTalk.dll")]
        static extern unsafe MMRESULT TextToSpeechGetSpeakerParams(IntPtr handle, uint uiIndex,
             out IntPtr ppspCur,
             out IntPtr ppspLoLimit,
             out IntPtr ppspHiLimit,
             out IntPtr ppspDefault);

        [DllImport("FonixTalk.dll")]
        static unsafe extern MMRESULT TextToSpeechAddBuffer(IntPtr handle, TTSBufferT.TTS_BUFFER_T* buffer);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechOpenInMemory(IntPtr handle, uint format);

        [DllImport("FonixTalk.dll")]
        static extern MMRESULT TextToSpeechCloseInMemory(IntPtr handle);

        [DllImport("FonixTalk.dll")]
        static unsafe extern MMRESULT TextToSpeechReturnBuffer(IntPtr handle, TTSBufferT.TTS_BUFFER_T* buffer);

        #endregion

        [DllImport("user32.dll")]
        private static extern uint RegisterWindowMessage(
            [MarshalAs(UnmanagedType.LPStr)]
            string lpString);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private const uint WAVE_FORMAT_1M16 = 0x00000004;

        private const uint TTS_NOT_SUPPORTED = 0x7FFF;
        private const uint TTS_NOT_AVAILABLE = 0x7FFE;
        private const uint TTS_LANG_ERROR = 0x4000;

        private IntPtr handle;
        private DtCallbackRoutine callback;

        private TTSBufferT buffer;
        private Stream bufferStream;

        private IntPtr speakerParamsPtr, dummy1, dummy2, dummy3;

        // Message types
        private static uint uiIndexMsg = RegisterWindowMessage("DECtalkIndexMessage");
        private static uint uiErrorMsg = RegisterWindowMessage("DECtalkErrorMessage");
        private static uint uiBufferMsg = RegisterWindowMessage("DECtalkBufferMessage");
        private static uint uiPhonemeMsg = RegisterWindowMessage("DECtalkVisualMessage");

        /// <summary>
        /// Initializes a new instance of the engine.
        /// </summary>
        /// <param name="language">The language to load.</param>
        public FonixTalkEngine(string language)
        {
            Init(language);
        }

        /// <summary>
        /// Initializes a new instance of the engine in US English.
        /// </summary>
        public FonixTalkEngine()
        {
            Init("US");
        }

        /// <summary>
        /// Initialize a new instance of the engine with the specified language, rate, and speaker voice.
        /// </summary>
        /// <param name="language">The language ID.</param>
        /// <param name="rate">The speaking rate to set.</param>
        /// <param name="speaker">The speaker voice to set.</param>
        public FonixTalkEngine(string language, uint rate, TTSVoice speaker)
        {
            Init(language);
            this.Voice = speaker;
            this.Rate = rate;
        }

        /// <summary>
        /// Initializes a new instance of the engine in US English with the specified rate and speaker voice.
        /// </summary>
        /// <param name="rate">The speaking rate to set.</param>
        /// <param name="speaker">The speaker voice to set.</param>
        public FonixTalkEngine(uint rate, TTSVoice speaker)
        {
            Init(LanguageCode.EnglishUS);
            this.Voice = speaker;
            this.Rate = rate;
        }

        private void Init(string lang)
        {
            callback = new DtCallbackRoutine(this.TTSCallback);
            buffer = new TTSBufferT();
            bufferStream = null;

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

            Speak("[:phone on]"); // Enable singing by default
        }

        /// <summary>
        /// Writes speech data to an internal buffer and returns it as a byte array containing 16-bit 11025Hz mono PCM data.
        /// </summary>
        /// <param name="input">The input text to process.</param>
        /// <returns></returns>
        public byte[] SpeakToMemory(string input)
        {
            using (bufferStream = new MemoryStream())
            {
                Check(TextToSpeechOpenInMemory(handle, WAVE_FORMAT_1M16));
                unsafe { Check(TextToSpeechAddBuffer(handle, buffer.ValuePointer)); }
                Speak(input);
                Sync();
                TextToSpeechReset(handle, false);
                Check(TextToSpeechCloseInMemory(handle));
                return ((MemoryStream)bufferStream).ToArray();
            }
        }

        /// <summary>
        /// Writes speech data to the specified stream as 16-bit 11025Hz mono PCM data.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="input">The input text to process.</param>
        /// <returns></returns>
        public void SpeakToStream(Stream stream, string input)
        {
            bufferStream = stream;
            Check(TextToSpeechOpenInMemory(handle, WAVE_FORMAT_1M16));
            unsafe { Check(TextToSpeechAddBuffer(handle, buffer.ValuePointer)); }
            Speak(input);
            Sync();
            TextToSpeechReset(handle, false);
            Check(TextToSpeechCloseInMemory(handle));
            bufferStream = null;
        }

        /// <summary>
        /// Writes speech data to a PCM WAV file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="input">The input text to process.</param>
        public void SpeakToWAVFile(string path, string input)
        {
            const int HeaderSize = 44;
            const int FormatChunkSize = 16;
            const short WaveAudioFormat = 1;
            const short NumChannels = 1;
            const int SampleRate = 11025;
            const short BitsPerSample = 16;
            const int ByteRate = (NumChannels * BitsPerSample * SampleRate) / 8;
            const short BlockAlign = NumChannels * BitsPerSample / 8;

            using(MemoryStream dataStream = new MemoryStream())
            {
                SpeakToStream(dataStream, input);
                int sizeInBytes = (int)dataStream.Length;
                using(BinaryWriter writer = new BinaryWriter(File.Create(path), Encoding.ASCII))
                {
                    writer.Write("RIFF".ToCharArray());
                    writer.Write(sizeInBytes + HeaderSize - 8);
                    writer.Write("WAVE".ToCharArray());
                    writer.Write("fmt ".ToCharArray());
                    writer.Write(FormatChunkSize);
                    writer.Write(WaveAudioFormat);
                    writer.Write(NumChannels);
                    writer.Write(SampleRate);
                    writer.Write(ByteRate);
                    writer.Write(BlockAlign);
                    writer.Write(BitsPerSample);
                    writer.Write("data".ToCharArray());
                    writer.Write(sizeInBytes);
                    dataStream.Position = 0;
                    dataStream.CopyTo(writer.BaseStream);
                }
            }
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

        private void TTSCallback(int lParam1, int lParam2, uint drCallbackParameter, uint uiMsg)
        {
            if (uiMsg == uiPhonemeMsg && this.Phoneme != null)
            {
                PhonemeTag tag = new PhonemeTag();
                tag.DWData = lParam2;
                this.Phoneme(this, new PhonemeEventArgs((char)tag.PMData.ThisPhoneme, tag.PMData.Duration));
            }
            else if (uiMsg == uiBufferMsg)
            {
                bufferStream.Write(buffer.GetBufferBytes(), 0, (int)buffer.Length);
                bool full = buffer.Full;
                buffer.Reset();

                if (full)
                {
                    unsafe { Check(TextToSpeechAddBuffer(handle, buffer.ValuePointer)); }
                }
            }
            else if (uiMsg == uiErrorMsg)
            {
                // You fucked up!
            }
            else if (uiMsg == uiIndexMsg)
            {
                // I don't even know what index messages are for...
            }
        }

        /// <summary>
        /// Returns the current speaker parameters.
        /// </summary>
        /// <returns></returns>
        public SpeakerParams GetSpeakerParams()
        {
            Check(TextToSpeechGetSpeakerParams(handle, 0, out speakerParamsPtr, out dummy1, out dummy2, out dummy3));
            return (SpeakerParams)Marshal.PtrToStructure(speakerParamsPtr, typeof(SpeakerParams));
        }

        /// <summary>
        /// Sets the current speaker parameters.
        /// </summary>
        /// <param name="sp">The parameters to pass to the engine.</param>
        public void SetSpeakerParams(SpeakerParams sp)
        {
            Check(TextToSpeechGetSpeakerParams(handle, 0, out speakerParamsPtr, out dummy1, out dummy2, out dummy3));

            int size = Marshal.SizeOf(typeof(SpeakerParams));            
            IntPtr tempPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(sp, tempPtr, false);
            CopyMemory(speakerParamsPtr, tempPtr, (uint)size);
            Marshal.FreeHGlobal(tempPtr);

            Check(TextToSpeechSetSpeakerParams(handle, speakerParamsPtr));            
        }

        /// <summary>
        /// Gets or sets the voice currently assigned to the engine.
        /// </summary>
        public TTSVoice Voice
        {
            get
            {
                TTSVoice voice;
                Check(TextToSpeechGetSpeaker(handle, out voice));
                return voice;
            }
            set
            {
                Check(TextToSpeechSetSpeaker(handle, value));
            }
        }

        /// <summary>
        /// Gets or sets the current speaking rate of the TTS voice.
        /// </summary>
        public uint Rate
        {
            get
            {
                uint rate;
                Check(TextToSpeechGetRate(handle, out rate));
                return rate;
            }
            set
            {
                Check(TextToSpeechSetRate(handle, value));
            }
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
            Check(TextToSpeechReset(handle, false));
        }

        /// <summary>
        /// Blocks until all previously queued text is processed.
        /// </summary>
        public void Sync()
        {
            Check(TextToSpeechSync(handle));
        }

        /// <summary>
        /// Causes the engine to begin asynchronously speaking a specified phrase. If the engine is in the middle of speaking, the message passed will be queued.
        /// </summary>
        /// <param name="msg">The phrase for the engine to speak.</param>
        public void Speak(string msg)
        {
            Check(TextToSpeechSpeakA(handle, msg, (uint)SpeakFlags.Force));
        }

        private static void Check(MMRESULT code)
        {
            if (code != MMRESULT.MMSYSERR_NOERROR)
            {
                throw new FonixTalkException(code);
            }
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        ~FonixTalkEngine()
        {
            TextToSpeechShutdown(handle);
            buffer.Dispose();
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            TextToSpeechShutdown(handle);
            buffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
