# unZENity-VR

![Platforms](https://img.shields.io/static/v1?label=Platforms&message=PCVR%20|%20Ques2%20|%20Pico4&color=green)
![License](https://img.shields.io/github/license/GothicVRProject/GothicVR?label=License&color=important)
[![Build](https://img.shields.io/github/actions/workflow/status/GothicVRProject/GothicVR/build.yml?label=Build&branch=main)](https://img.shields.io/github/actions/workflow/status/GothicKit/phoenix-shared-interface/build.yml)

Fan project recreating the classic Gothic experience in VR.


# Usage
For instructions on playing the game or adding contributions, please check out Wiki: https://github.com/GothicVRProject/GothicVR/wiki  
(🤫 Shortcut for [Gamers](https://github.com/GothicVRProject/GothicVR/wiki/01-_-Gamer). Thank me later. 😉) 


## Our Tenets (until you know better ones)
1. We provide native Gothic  experience -  To achieve it we will import original Gothic assets from local installations. Dynamically and during runtime. Do you still remember Bloodwyn forcing you to pay your protection money? You'll experience it again. And you'll pay... We promise!
2. We add best in class VR mechanics - VR offers new ways of being immersed into the world of Gothic. How about crafting a blade with your hammer blow, drawing a two-handed weapon from your left shoulder, or casting a spell with hand gestures? You dream it, we build it.
3. We will put enhancements to original Gothic data wherever useful - The original Gothic games were built when computers weren't this beefy. 20 years later, it changed. We think of adding more details like additional grassy grass to the Barrier or muggy mugs inside the Old Camp.
4. We will adopt all of Piranha Bytes' ZenGine games - But first things first. Let's start with a full playable Gothic 1 port, followed by Gothic 2 and it's addon NotR.
5. We just can't get enough! - Once done with the original experiences we will add own VR game modes and support for mods, and total conversions. How about a coop Scavenger hunt with your friends? Wouldn't this be amazing? And how about re-experiencing a few of your most beloved community mods in VR? Yes. We feel the same. <3


## How to contribute
We're always looking for people with knowledge and/or spirit. Feel free to reach out to us via gothicVR(at)outlook.com or visit us at our Discord server [Gothic VR](https://discord.gg/3EzACMVx).

## Workflow/Gameflow

![data flow](./Documentation/Images/data-flow.drawio.png)


1. unZENity-VR requests data from PxCs.dll (.net standard 2.1 shared library which is cross-OS compatibel)
1. The dll itself forwards request to libphoenix-shared.dll/.so as it includes the original phoenix parser library.
1. phoenix-shared-interface loads the file system data.
1. The data is then returned to Unity to build Unity C# objects like Meshes.


## Dependencies
unZENity-VR is using the following projects:
* [phoenix](https://github.com/GothicKit/phoenix) (Gothic asset parser)
* [phoenix-shared-interface](https://github.com/GothicKit/phoenix-shared-interface) (C++ -> C interface)
* [phoenix-csharp-interface](https://github.com/GothicKit/phoenix-csharp-interface) (C# endpoint for C interface)


## Credits
Big shoutout towards
* [phoenix](https://github.com/GothicKit/phoenix) - Our single source of truth for parsing Gothic assets at runtime.
* [OpenGothic](https://github.com/Try/OpenGothic) - Our reliable inspiration and code support on how to remake the classic Gothic games. 


## FAQ

**Q: Why do you use an external framework to parse Gothic assets?**  
A: phoenix is a full Gothic asset parser and already used by OpenGothic which is feature complete to work with Gothic1, Gothic2, and Gothic2 the Night of the Raven. Why reinventing the wheel? ¯\_(ツ)_/¯

**Q: Why do you use integrate phoenix as shared library (DLL)?**  
A: As phoenix is written in C++, we need a way to communicate with C#. The way to go is shared libraries as they can be used within C# via _DllImport_.
