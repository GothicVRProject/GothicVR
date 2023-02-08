# Gothic-unZENity-VR
This repository aims to leverage Unity as Open Source VR solution to work with Gothic 1/2, Mods, and Total Conversions.

It's a PoC if the idea of _Importing everything from Gothic and act as the new engine (ZenGine --> Unity) behind the scenes can work.

Heavily inspired and reusing code from:
* [REGoth-bs](https://github.com/REGoth-project/REGoth-bs) ([Documentation](https://regoth-project.github.io/REGoth-bs/index.html))
* [OpenGothic](https://github.com/Try/OpenGothic)


## Gothic folder and files to import

Relevant Gothic folder and included files:
```
/_work/DATA
  /Music/* --> .sgt, .sty, .dls
  /PRESETS --> .zen
  /scripts --> .dat
/Data      --> .vdf
```

File types and including data:
| File | Information |
|-|-|
| .vdf | Compiled files. Can include everything like world meshes and wav. |
| .dat | Compiled .d (Daedalus) scripts. |
| .zen | World files with Waypoints and VOB (virtual object) placements. |
| .sgt/.sty/.dls | Something related to audio... ¯\_(ツ)_/¯ |


## Workflow/Gameflow
![Gothic-Unity-as-ZenGine-flow](./Documentation/Images/Gothic-Unity-as-ZenGine-flow.png)


## lib/ZenLib

**install**  
```powershell
choco install cmake --installargs 'ADD_CMAKE_TO_PATH=System'
choco install mingw
```

**build**  
```powershell
mkdir build
cd build
cmake -G "MinGW Makefiles" -D ZENLIB_BUILD_EXAMPLES=On ..
cmake --build .
```

**samples**  
```powershell
cd build/samples
mkdir vdf-test

# Will extract ZEN files out of VDFS
./vdf_unpack.exe "C:\Program Files (x86)\Steam\steamapps\common\Gothic\Data\worlds.vdf" ./vdf-test
```

```powershell
cd build/samples

# Extracts VOB entries (e.g. Waypoints) from ZEN file (Like Spacer is doing)
./zen_load.exe "C:\Program Files (x86)\Steam\steamapps\common\Gothic\Data\worlds.vdf" "world.zen"

```


## FAQ

**Q: What an awkward name...**  
A: True! It's a combination of Gothic, Unity, Zen, VR, and a grain of insanity.