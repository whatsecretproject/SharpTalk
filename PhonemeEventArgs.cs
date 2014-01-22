using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTalk
{
    public class PhonemeEventArgs : EventArgs
    {
        public readonly char Phoneme;
        public readonly uint Duration;
        public PhonemeEventArgs(char phoneme, uint duration)
        {
            this.Phoneme = phoneme;
            this.Duration = duration;
        }
    }
}
