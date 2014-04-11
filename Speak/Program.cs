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
            while(true)
            {
                tts.Speak(Console.ReadLine());
            }
        }
    }
}
