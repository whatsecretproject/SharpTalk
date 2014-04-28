using System;
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
                string msg;
                while ((msg = Console.ReadLine()) != "exit")
                {
                    tts.Speak(msg);
                }
            }
        }
    }
}
