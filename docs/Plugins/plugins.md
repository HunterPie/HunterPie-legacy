# HunterPie - Plugins

HunterPie supports simple one-file plugins to be compiled into a `.dll` file and loaded into it's environment. This documentation is still in progress and the plugins system is still in development, so things may change in the future.

## Getting Started

Every mod needs at least two files, an entry file that will be compiled into a `.dll` that can be distributed later on and a `module.json` that will specify the module information. You can also add other `.dll` dependencies to the folder of your plugin and reference them in your module information.

### Module.json

These informations are used when building the mod from a C# file (marked in the EntryPoint property), and will be used in the future to display each plugin information in HunterPie's console.

```js
    {
        "Name": "MyCoolMod", // The plugin name, this is the name your dll will receive after the build
        "Description": "A very cool module for HunterPie", // The plugin description
        "EntryPoint": "main.cs", // Specifies the file to be compiled by HunterPie
        "Author": "MyName", // You!
        "Version": "1.0.0", // Mod version 
        "Dependencies": [] // DLL paths relative to the mod folder
    }
```

### Entry point

The entry point is the C# script that will be compiled, it can have any name you want, and **should not** be distributed to the end-user, unless you want them to re-compile the mod every time their mod is loaded.

## Extensions

An extension has access to every HunterPie `public` core feature, that includes:
- **Game events:** All events triggered by HunterPie when something happens.
- **Game Data:** This includes player, buffs, debuffs, monster, game and everything HunterPie keeps track of.
- **Internal data:** This includes Honey IDs, Monster data, abnormalities data, game structure definitions.

TODO

