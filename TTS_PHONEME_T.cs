using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SharpTalk
{
    [StructLayout(LayoutKind.Sequential)]
    struct TTS_PHONEME_T
    {
        public uint Phoneme;
        public uint PhonemeSampleNumber;
        public uint PhonemeDuration;
        private uint _reserved;
    }
}
