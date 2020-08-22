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

An extension has access to every HunterPie public core feature, that includes:
- **Game events:** All events triggered by HunterPie when something happens.
- **Game Data:** This includes player, buffs, debuffs, monster, game and everything HunterPie keeps track of.
- **Internal data:** This includes Honey IDs, Monster data, abnormalities data, game structure definitions.

## Making your plugin

Plugins must implement the basic <Interface>IPlugin</Interface> interface in order to be loaded correctly.

```cs
namespace HunterPie.Plugins
{
    public interface IPlugin
    {
        string Name { get; set; }
        string Description { get; set; }
        Game Context { get; set; }

        void Initialize(Game context);
        void Unload();
    }
}
```

### Initialize

Note that the `Initialize` method has a context parameter, that's passed to your plugin whenever the game starts. A <Type>Game</Type> is the game context, it englobes the <Type>Player</Type>, <Type>Monster</Type> and everything else that's happening inside the game.

This method is what HunterPie will look for and call when loading all plugins, so the first thing you must do is assign the plugin Name, Description and the Context accordingly inside the Initialize method.

### Unload

The unload method is what HunterPie will call when unloading your plugin, you **should** unhook all events you hooked in the Initialize and dispose everything you've used (if applicable) in this method to avoid memory leaks.

### Game Context

The <Type>Game</Type> is the core of HunterPie and the core of your plugin. You can read more about it [here](?p=Plugins/game.md).