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
    /// Enables usage of the DECtalk TTS engine.
    /// </summary>
    public class DECTalkEngine
    {
        /// <summary>
        /// Fired when a phoneme event is invoked by the engine.
        /// </summary>
        public event EventHandler<PhonemeEventArgs> Phoneme;
        public const uint DefaultRate = 200;        

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DtCallbackRoutine(
            int lParam1,
            int lParam2,
            uint drCallbackParameter,
            uint uiMsg);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechStartup(
            IntPtr hwnd,
            out IntPtr handle,
            uint uiDeviceNumber,
            uint dwDeviceOptions);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechStartupEx(
            out IntPtr handle,
            uint uiDeviceNumber,
            uint dwDeviceOptions,
            DtCallbackRoutine callback, 
            int dwCallbackParameter);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        [return : MarshalAs(UnmanagedType.Bool)]        
        static extern bool TextToSpeechSelectLang(IntPtr handle, uint lang);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechStartLang(
            [MarshalAs(UnmanagedType.LPTStr)]
            string lang);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechSetSpeaker(IntPtr handle, uint speaker);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechGetSpeaker(IntPtr handle, out uint speaker);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechGetRate(IntPtr handle, out uint rate);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechSetRate(IntPtr handle, uint rate);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechSpeak(IntPtr handle, 
            [MarshalAs(UnmanagedType.LPStr)] 
            string msg, 
            uint flags);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechShutdown(IntPtr handle);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechPause(IntPtr handle);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechResume(IntPtr handle);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechReset(IntPtr handle, 
            [MarshalAs(UnmanagedType.Bool)]
            bool bReset);

        [DllImport("dectalk.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern uint TextToSpeechSync(IntPtr handle);

        private IntPtr handle;
        private DtCallbackRoutine callback;

        /// <summary>
        /// Initializes a new instance of the engine.
        /// </summary>
        /// <param name="language">The language to load.</param>
        public DECTalkEngine(string language)
        {
            Init(language);
        }

        /// <summary>
        /// Initializes a new instance of the engine in US English.
        /// </summary>
        public DECTalkEngine()
        {
            Init("US");
        }

        private void Init(string lang)
        {
            callback = new DtCallbackRoutine(this.TTSCallback);
            var langid = TextToSpeechStartLang(lang);
            TextToSpeechSelectLang(IntPtr.Zero, langid);
            //TextToSpeechStartup(IntPtr.Zero, out handle, 0xFFFFFFFF, 0);
            TextToSpeechStartupEx(out handle, 0xFFFFFFFF, 0, callback, 0);
            SetSpeaker(Speaker.Paul);
            SetRate(DefaultRate);
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
            uint uiMsg) // For some reason uiMsg holds nonsense instead of the proper flags
        {
            if (this.Phoneme != null)
            {
                PhonemeTag tag = new PhonemeTag();
                tag.DWData = lParam2;
                this.Phoneme(this, new PhonemeEventArgs((char)tag.PMData.ThisPhoneme, tag.PMData.Duration));
            }
        }

        /// <summary>
        /// Sets the voice used by the engine.
        /// </summary>
        /// <param name="spkr">The speaker to assign</param>
        public void SetSpeaker(Speaker spkr)
        {
            TextToSpeechSetSpeaker(handle, (uint)spkr);
        }

        /// <summary>
        /// Gets the voice currently being used by the engine.
        /// </summary>
        /// <returns></returns>
        public Speaker GetSpeaker()
        {
            uint spkr;
            TextToSpeechGetSpeaker(handle, out spkr);
            return (Speaker)spkr;
        }

        /// <summary>
        /// Gets the engine's current speaking rate.
        /// </summary>
        /// <returns></returns>
        public uint GetRate()
        {
            uint rate;
            TextToSpeechGetRate(handle, out rate);
            return rate;
        }

        /// <summary>
        /// Sets the engine's current speaking rate.
        /// </summary>
        /// <param name="rate">The rate to assign.</param>
        public void SetRate(uint rate)
        {
            TextToSpeechSetRate(handle, rate);
        }

        /// <summary>
        /// Pauses TTS audio output.
        /// </summary>
        public void Pause()
        {
            TextToSpeechPause(handle);
        }

        /// <summary>
        /// Resumes previously paused TTS audio output.
        /// </summary>
        public void Resume()
        {
            TextToSpeechResume(handle);
        }

        /// <summary>
        /// Flushes all previously queued text from the TTS system and stops any audio output.
        /// </summary>
        public void Reset()
        {
            TextToSpeechReset(handle, true);
        }

        /// <summary>
        /// Blocks until all previously queued text is processed.
        /// </summary>
        public void Sync()
        {
            TextToSpeechSync(handle);
        }

        /// <summary>
        /// Causes the engine to begin asynchronously speaking a specified phrase. If the engine is in the middle of speaking, the message passed will be queued.
        /// </summary>
        /// <param name="msg">The phrase for the engine to speak.</param>
        public void Speak(string msg)
        {
            TextToSpeechSpeak(handle, msg, (uint)SpeakFlags.Force);
        }

        ~DECTalkEngine()
        {
            TextToSpeechShutdown(handle);
        }
    }
}
