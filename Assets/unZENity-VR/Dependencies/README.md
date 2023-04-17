This project relies on two dll/so

* PxCs.dll (Managed .NET library; can be used on linux systems as well; extern bridge between libhoenix-shared and Unity)
* libphoenix-shared.dll (Unmanaged C library; actual Gothic asset parser logic; *.a for some Windows builds and *.so for linux).


libphoenix-shared OS specific versions:
* .dll - Windows64 version
* .so - Linux version
* .arm.so - arm64 version (Needs to be renamed to .so before building the target on Unity; Will be built as an automatic renaming step inside Unity in a future commit)
* .dylib - MacOSX versioon (Showed error "Cannot set deprecated build target 'OSXIntel', therefore not added right now)