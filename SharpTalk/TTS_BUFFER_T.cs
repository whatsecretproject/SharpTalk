using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SharpTalk
{
    unsafe struct TTS_BUFFER_T : IDisposable
    {
        IntPtr DataPtr;
        TTS_PHONEME_T* PhonemeArrayPtr;
        TTS_INDEX_T* IndexArrayPtr;
        
        uint MaxBufferLength;
        uint MaxPhonemeChanges;
        uint MaxIndexMarks;
        
        uint BufferLength;
        uint PhonemeChangeCount;
        uint IndexMarkCount;

        uint _reserved;

        public byte[] GetBufferBytes()
        {
            byte[] buffer = new byte[BufferLength];
            Marshal.Copy(DataPtr, buffer, 0, (int)BufferLength);
            return buffer;
        }

        public static TTS_BUFFER_T CreateNew(int bufferSize)
        {
            TTS_BUFFER_T buffer = new TTS_BUFFER_T();
            buffer.MaxBufferLength = (uint)bufferSize;
            buffer.DataPtr = Marshal.AllocHGlobal(bufferSize);
            return buffer;
        }

        private void Delete()
        {
            Marshal.FreeHGlobal(DataPtr);
        }

        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }
    }
}
