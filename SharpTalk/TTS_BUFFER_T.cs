using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SharpTalk
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct TTS_BUFFER_T : IDisposable
    {
        const int BufferSize = 16384;

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

        // after here additional member for managed stuff

        GCHandle _pinHandle;

        public byte[] GetBufferBytes()
        {
            byte[] buffer = new byte[BufferLength];
            Marshal.Copy(DataPtr, buffer, 0, (int)BufferLength);
            return buffer;
        }

        public uint Length
        {
            get { return BufferLength; }
        }

        public void Reset()
        {
            BufferLength = 0;
            PhonemeChangeCount = 0;
            IndexMarkCount = 0;
        }

        public static TTS_BUFFER_T CreateNew()
        {
            TTS_BUFFER_T buffer = new TTS_BUFFER_T();
            buffer._pinHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            buffer.MaxBufferLength = (uint)BufferSize;
            buffer.DataPtr = Marshal.AllocHGlobal(BufferSize);
            
            return buffer;
        }

        private void Delete()
        {
            Marshal.FreeHGlobal(DataPtr);
            _pinHandle.Free();
        }

        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }
    }
}
