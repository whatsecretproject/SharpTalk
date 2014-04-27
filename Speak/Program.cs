using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SharpTalk;

namespace Speak
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SharpTalk Speaking Terminal";
            using (var tts = new FonixTalkEngine())
            {
                string msg = "";
                while ((msg = Console.ReadLine()) != "exit")
                {
                    tts.Speak(msg);
                }
            }
        }
    }
}
