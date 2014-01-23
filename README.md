SharpTalk
=========

A .NET wrapper for the DECtalk TTS engine.


This project was inspired by the creative antics of those utilizing Moonbase Alpha's TTS feature. Aeiou.

I searched around exhausively and for a decent TTS engine apart from Microsoft's SAPI, which has a .NET implementation. I don't like SAPI because its features are complicated, it depends on having custom voices installed, and SSML generally makes me want to puke.

Eventually, I came across DECtalk and its accompanying SDK, from which I was able to get documentation for its functions. I spent countless hours implementing these in C# using P/Invoke, and I eventually got it working.


Features
-----
* Asynchronous speaking
* Phoneme events for mouth movements in games/animations
* Sync() method makes it easy to synchronize voice output
* Adjustable voice and speaking rate
* Multiple engines can be independently controlled and talking at the same time
* Voices can be paused/resumed


How to use
------

Using the library is very simple and straightforward. Here is the basic code to make the engine speak:

```cs
DECTalkEngine tts = new DECTalkEngine();
tts.Speak("John Madden!");
```

You can easily change the voice of the engine, too! For example, here is a piece of code that uses the Frank voice:

```cs
DECTalkEngine tts = new DECTalkEngine();
tts.SetSpeaker(Speaker.Frank);
tts.Speak("Here comes another Chinese earthquake! [:phone on][brbrbrbrbrbrbrbrbrbrbrbrbrbrbrbrbrbrbr]");
```
