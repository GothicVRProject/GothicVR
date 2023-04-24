# Setup

## GothicVR
> Hint: Check _Player documentation_ wiki page for additional setup instructions.

> Hint: GothicVR repository uses ```git lfs```. Please set it up via ```git lfs install``` to check out binary data (e.g. shared libraries inside _Dependencies_)


Instructions:
1. Check out [GothicVR](https://github.com/GothicVRProject/GothicVR)
2. Check the README.md file for build instructions
1. Install Unity, change settings mentioned in _Player documentation_, and click on _Play_ ;-)


## phoenix-csharp-interface
Project is required if you need to change interface methods for fetching phoenix data or you want to execute tests for loading gothic assets.

Instructions:
1. Check out [phoenix-csharp-interface](https://github.com/GothicKit/phoenix-csharp-interface)
1. Check the README.md file for build and test instructions
1. If you built a new version of PxCs.dll, then put it into GothicVR folder at Assets/GothicVR/Dependencies


## phoenix-shared-interface
> Hint: Latest builds to use are stored at the repository's [GitHub Action page](https://github.com/GothicKit/phoenix-shared-interface/actions)

> Hint: If you build a new version of the shared library and put it into GothicVR, you need to store at least both: .dll and .so (for ARM64). Otherwise changes won't be reflected on all VR devices.

Project is required if you need to change C++/C interface methods for fetching phoenix data.

Instructions:
1. Check out [phoenix-shared-interface](https://github.com/GothicKit/phoenix-shared-interface)
1. Check the README.md file for build instructions
1. If you built a new version of libphoenix-shared.dll/.so, then put it into GothicVR folder at Assets/GothicVR/Dependencies

