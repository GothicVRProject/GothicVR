## Compiling DLL

## Pre-setup
(tested mingw-w64 version: 12.2.0)

```powershell
choco install mingw
```


## Compile

```powershell
cmake -G "MinGW Makefiles" -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build
```