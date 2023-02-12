# unZENity-VR
This repository aims to leverage Unity as Open Source VR solution to work with Gothic 1/2, Mods, and Total Conversions.

It's a PoC if the idea of _Importing everything_ from Gothic and act as the new engine (ZenGine --> Unity) behind the scenes can work.

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



## What and how to use

### World meshes
1. (manually done) Extract VDF (e.g. worlds.vdf) into .zen (ZenLib)
2. (todo) Convert .zen into .3ds (e.g. oldcamp.3ds) (which tool?)
3. (done) Import oldcamp.3ds into Unity
4. (manually done) Extract VDF (e.g. texture.vdf) into zen (ZenLib)
5. (todo) Convert .tex textures into .tga (which tool?)
6. (done) While creating mesh->materials we need to reference the TGA files in Unity

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


## Howto load worldmesh
* Check 
  * ImportStaticMesh.cpp/BsZenLib::ImportAndCacheStaticMesh()
  * ImportStaticMesh.cpp/BsZenLib::ImportAndCacheStaticMeshGeometry()
  * zCProgMeshProto.h/ZenLoad::zCProgMeshProto()

--> parser.readWorldMesh() --> during reading of .zen file it will parse mesh into this object.

## FAQ

**Q: What an awkward name...**  
A: True! It's a combination of Gothic, Unity, Zen, VR, and a grain of insanity.
