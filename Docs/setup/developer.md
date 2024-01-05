# Setup

First make sure that your GothicVR installation is working. The setup instructions can be found in the [player documentation](player.md).

## Git LFS

The GothicVR repository uses `git lfs`. Please set it up via `git lfs install` to check out binary data (e.g. shared libraries inside `Dependencies`).

## ZenKitCS and ZenKitCAPI

These projects are required if you need to change interface methods for fetching ZenKit data or you want to execute tests for loading gothic assets.

**Instructions:**
1. Check out [ZenKitCS](https://github.com/GothicKit/ZenKitCS) and/or [ZenKitCAPI](https://github.com/GothicKit/ZenKitCAPI)
1. Check the `README.md` file(s) for build and test instructions
1. If you built a new version, then put the created libraries (ZenKit.dll, zenkitcapi.dll, libzenkitcapi.so) into the GothicVR folder at `Assets/GothicVR/Dependencies`

> Hint: Latest builds of all three libs are stored in one package at ZenKitCS' repository [GitHub Action page](https://github.com/GothicKit/ZenKitCS/actions)
