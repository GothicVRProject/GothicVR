# GothicVR

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
![Platforms](https://img.shields.io/static/v1?label=Platforms&message=PCVR%20|%20Quest2%20|%20Pico4&color=darkgreen)
[![Release](https://img.shields.io/github/release-pre/GothicVRProject/GothicVR)](https://github.com/GothicVRProject/GothicVR/releases/latest)

Fan project recreating the classic Gothic I and II experience in VR.


## Usage
For instructions on playing the game or adding contributions, please check our [Docs](Docs).

(🤫 Shortcut to the installation instructions: [here](Docs/setup/player.md). Thank me later. 😉)


## Our Tenets (until you know better ones)
1. We provide native Gothic  experience -  To achieve it we will import original Gothic assets from local installations. Dynamically and during runtime. Do you still remember Bloodwyn forcing you to pay your protection money? You'll experience it again. And you'll pay... We promise!
2. We add best in class VR mechanics - VR offers new ways of being immersed into the world of Gothic. How about crafting a blade with your hammer blow, drawing a two-handed weapon from your left shoulder, or casting a spell with hand gestures? You dream it, we build it.
3. We will put enhancements to original Gothic data wherever useful - The original Gothic games were built when computers weren't this beefy. 20 years later, it changed. We think of adding more details like additional grassy grass to the Barrier or muggy mugs inside the Old Camp.
4. We will adopt all of Piranha Bytes' ZenGine games - But first things first. Let's start with a full playable Gothic 1 port, followed by Gothic 2 and it's addon NotR.
5. We just can't get enough! - Once done with the original experiences we will add own VR game modes and support for mods, and total conversions. How about a coop Scavenger hunt with your friends? Wouldn't this be amazing? And how about re-experiencing a few of your most beloved community mods in VR? Yes. We feel the same. <3


## How to contribute
We're always looking for people with knowledge and/or spirit. Feel free to reach out to us via gothicVR(at)outlook.com or visit us at our Discord server [Gothic VR](https://discord.gg/StV9pbkBer).

## Workflow/Gameflow

![data flow](Docs/development/diagrams/data-flow.drawio.png)


1. GothicVR requests data from ZenKit.dll (.net standard 2.1 shared library which is cross-OS compatibel).
2. The dll itself forwards request to libzenkitcapi.dll/.so as it includes the original ZenKit parser library.
3. ZenKitCAPI loads the file system data.
4. The data is then returned to Unity to build Unity C# objects like Meshes.


## Dependencies
GothicVR is using the following projects:
* [ZenKit](https://github.com/GothicKit/ZenKit) (Gothic asset parser)
* [ZenKitCAPI](https://github.com/GothicKit/ZenKitCAPI) (C++ -> C interface)
* [ZenKitCS](https://github.com/GothicKit/ZenKitCS) (C# endpoint for C interface)


## Credits
Big shoutout towards
* [ZenKit](https://github.com/GothicKit/ZenKit) - Our single source of truth for parsing Gothic assets at runtime.
* [OpenGothic](https://github.com/Try/OpenGothic) - Our reliable inspiration and code support on how to remake the classic Gothic games.


## FAQ

**Q: Why do you use an external framework to parse Gothic assets?**  
A: ZenKit is a full Gothic asset parser and already used by OpenGothic. It is feature complete and works with Gothic1, Gothic2, and Gothic2 the Night of the Raven assets. Why reinventing the wheel? ¯\_(ツ)_/¯

**Q: Why do you integrate ZenKit as shared library (DLL) instead of using its code directly?**  
A: As ZenKit is written in C++, we need a way to communicate with C#. The way to go is shared libraries as they can be used within C# via _DllImport_.
