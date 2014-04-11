using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTalk;

namespace Speak
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SharpTalk Speaking Terminal";
            FonixTalkEngine tts = new FonixTalkEngine();
            tts.Phoneme += tts_Phoneme;
            while(true)
            {
                tts.Speak(Console.ReadLine());
            }
        }

        static void tts_Phoneme(object sender, PhonemeEventArgs e)
        {
            Console.WriteLine(e.Phoneme);
        }
    }
}
