using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SharpTalk
{
    [StructLayout(LayoutKind.Sequential)]
    struct TTS_INDEX_T
    {
        public uint IndexValue;
        public uint SampleNumber;
        uint _reserved;
    }
}
