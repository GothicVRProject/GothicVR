This project relies on the following dll/so libraries:
* ZenKit.dll - Managed .NET library; can be used on linux systems as well; _extern_ bridge between libzenkitcapi and Unity
* libzenkitcapi.* - Unmanaged C library; actual Gothic asset parser logic; *.a for some Windows builds and *.so for arm64
* libdmusic-* / DMCs.dll - Music related implementations from DirectMusic
* I18N.*.dll - Managed .NET library from Mono. Required to get Windows codepages inside builds. Taken from _INSTALL_DIR\Unity\Hub\Editor\VERSION\Editor\Data\MonoBleedingEdge\lib\mono\gac\*_


libzenkicapi OS specific versions:
* .dll - Windows version
* .so - arm64 version
* .linux.so - (not used!) Linux version (Needs to be renamed to .so before using Unity editor on a linux system. Hint: OpenXR plugin isn't compatible with Linux runtimes. Development on this system might be cumbersome: https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.7/manual/index.html)
* .dylib - (not used!) MacOSX version (Showed error "Cannot set deprecated build target 'OSXIntel', therefore not added right now)


I18N.*.dll:
* I18N - General language support
* I18N.MidEast - Windows-1250 codepage
* I18N.Other - Windows-1251 codepage
* I18N.West - Windows-1252 codepage
