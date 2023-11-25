# Setup

This section contains any relevant info to get your installation of GothicVR up and running.

## Installing the Game Assets

To comply with the Gothic Modding License, the project does not contain any of the original Gothic game assets. Because of that, you have to provide the Gothic game data yourself.

This process differs between different devices so make sure to choose the right instructions from the sections below.
It is assumed that you have a working installation of Gothic on your PC; GOG, Steam or CD installations should all work interchangeably.

### PCVR

GothicVR needs to know where your Gothic installation is located for streaming the assets at runtime. All you have to do is set the right path in the `GameSettings.json` file.

> Note: The default installation path for the Steam version of Gothic is `C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic` - where the backslashes are already escaped so that you can copy this string directly into your `GameSettings.json`.

Information on where the `GameSettings.json` is located and more about the other settings can be found in the [GameSettings](#GameSettings) section.

And easy alternative to adjusting the game settings is to start the application and use the provided filepicker to select a legal copy of Gothic 1.
Doing so will adjust the game settings file in the background and start the game right away.

### Quest 2/Pico 4

To get the Gothic assets onto your headset you have to copy them to the GothicVR data directory on your headset: `/storage/emulated/0/Android/data/com.GothicVRProject.GothicVR/files/`. After the copy, the folder should look something like the following (Where required folders are marked; the other directories can be removed). Make sure that the Gothic game files are **not** in their own `files/Gothic` subfolder!

```
/storage/emulated/0/Android/data/com.GothicVRProject.GothicVR
├── files
│   ├── ...
│   ├── GameSettings.json (already installed)
│   ├── Data (required)
│   ├── DirectX 8.0
│   ├── launcher
│   ├── Miles
│   ├── Readme.htm
│   ├── Saves
│   ├── system
│   ├── VDFS.CFG
│   └── _work (required)
```

To copy your game files onto your device you can use `adb`. To access your headset's files you have to enable the Developer Mode in your headset's settings.

> Note: The following assumes you have adb installed, the Developer Mode enabled and your headset connected to your PC - if not, see below
> 
> To install `adb` search in your preferred search engine for `how to install adb on <windows/linux>`.
> 
> For enabling the Developer Mode, just search for `how to enable developer mode on <quest 2/pico 4>`.
>
> To connect your headset, use a USB Type-C cable.

Firstly, check if your headset can be reached from your PC with `adb devices`. This should list at least one device; if it shows more than one, make sure to specify your headset in the following commands with `adb -s <ID> ...`.

If your headset is correctly connected, use `adb push <Path to your Gothic dir>/* /storage/emulated/0/Android/data/com.GothicVRProject.GothicVR/files/`.

You can confirm the correct file structure by listing it with `adb shell ls /storage/emulated/0/Android/data/com.GothicVRProject.GothicVR/files/`.

## GameSettings

The `GameSettings.json` file can be used to modify some of GothicVR's settings.

| Setting                | Location          | Required        | Example                                                   | Description                                                                                                                                                                                                 |
|------------------------|-------------------|-----------------|-----------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| GothicIPath            | GameSettings.json | Yes (PCVR only) | C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic | GothicVR requires a full Gothic1 installation/file-dump at runtime. Name its location here (Windows users: Escape the backslash with `\\`). <br>This setting gets ignored on Android (e.g. Pico 4/Quest 2). |
| GothicILanguage        | GameSettings.json | Yes             | en                                                        | Name of the language, Gothic is installed with: cs/en/de/fr/it/pl/es/ru                                                                                                                                     |
| LogLevel               | GameSettings.json | No              | Warning                                                   | Defines the level of logging that will be saved. Values are: Debug/Warning/Error/Exception.                                                                                                                 |
| GothicMenuFontPath     | GameSettings.json | No              | Gothic_Titel_Offiziell.ttf                                | Font which is used within Gothic Menu. .ttf/.otf supported. Feel free to check (e.g.) on worldofgothic for some cool fonts. If not defined, the game will fallback to a default font.                       |
| GothicSubtitleFontPath | GameSettings.json | No              | Gothic_Ingame_Offiziell.ttf                               | Same like font setting above but for subtitles in game.                                                                                                                                                     |

Where to find the `GameSettings.json`:
* **PCVR:** Location is inside download/installation directory: `.\GVR_Data\StreamingAssets\GameSettings.json`
* **Mobile VR (Quest2/Pico4):** After starting the game for the first time (and/or the file doesn't exist), it will create the `GameSettings.json` file at `/storage/emulated/0/Android/data/com.GothicVRProject.GothicVR/files/GameSettings.json`

# Logging

Where to find the log files?
* **PCVR:** Location directory for the logging file: `C:\Users\%USERPROFILE%\AppData\LocalLow\GothicVRProject\GothicVR\gothicvr_log.txt`
* **Mobile VR (Quest2/Pico4):** Location directory for the logging file:`/storage/emulated/0/Android/data/com.GothicVRProject.GothicVR/files/gothicvr_log.txt`

# How to Play
TBD
