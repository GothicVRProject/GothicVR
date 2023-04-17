This project relies on two dll/so libraries

* PxCs.dll - Managed .NET library; can be used on linux systems as well; _extern_ bridge between libhoenix-shared and Unity
* libphoenix-shared.dll - Unmanaged C library; actual Gothic asset parser logic; *.a for some Windows builds and *.so for arm64


libphoenix-shared OS specific versions:
* .dll - Windows version
* .so - arm64 version
* .linux.so - Linux version (Needs to be renamed to .so before using Unity editor on a linux system. Hint: OpenXR plugin isn't compatible with Linux runtimes. Development on this system might be cumbersome: https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.7/manual/index.html)
* .dylib - MacOSX version (Showed error "Cannot set deprecated build target 'OSXIntel', therefore not added right now)