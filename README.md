# unZENity-VR
This repository aims to leverage Unity as Open Source VR solution to work with Gothic 1/2, Mods, and Total Conversions.

It's a PoC if the idea of _Importing everything_ from Gothic and act as the new engine (ZenGine --> Unity) behind the scenes can work.

Heavily inspired and reusing code from:
* [phoenix](https://github.com/lmichaelis/phoenix) - Gothic1/2 asset reading and parsing framework
  * which is re-using [ZenLib](https://github.com/ataulien/ZenLib)
  * and is implemented in [OpenGothic](https://github.com/Try/OpenGothic)

## How to contribute
We're always looking for people with knowledge and/or spirit. Feel free to reach out to us via gothicVR(at)outlook.com or visit us at our Discord server [Gothic VR](https://discord.gg/3EzACMVx).

## Workflow/Gameflow

![common interfaces between bridge and unity](./Documentation/Images/common-interfaces-bridge-unity.drawio.png)

* unZENity-VR requests data from phoenix-csharp-bridge DLL.
* The DLL uses it's compiled phoenix dependency to read Gothic assets.
* The data is then returned to Unity to build Unity C# objects like Meshes.
* Unity also registers functions on the DLL to react to Daedalus events.

Below is an example workflow of how data is requested:
![data flow](./Documentation/Images/data-flow.drawio.png)


## FAQ

**Q: Why do you use an external framework to parse Gothic assets?**  
A: phoenix is a full Gothic asset parser and already used by OpenGothic which is feature complete to work with Gothic1, Gothic2, and Gothic2 the Night of the Raven. Why reinventing the wheel? ¯\_(ツ)_/¯

**Q: Why do you use integrate phoenix as shared library (DLL)?**  
A: As phoenix is written in C++, we need a way to communicate with C#. The way to go is shared libraries as they can be used within C# via _DllImport_.

**Q: What an interesting project name...**  
A: True! It's a combination of Gothic, Unity, Zen, VR, and a grain of insanity ;-)
