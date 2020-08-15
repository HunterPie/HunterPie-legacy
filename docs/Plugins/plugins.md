# HunterPie - Plugins

HunterPie supports simple one-file plugins to be compiled into a `.dll` file and loaded into it's environment. This documentation is still in progress and the plugins system is still in development, so things may change in the future.

## Getting Started

Every mod needs at least two files, an entry file that will be compiled into a `.dll` that can be distributed later on and a `module.json` that will specify the module information. You can also add other `.dll` dependencies to the folder of your plugin and reference them in your module information.

### Module.json structure
```js
    {
        "Name": "MyCoolMod", // The plugin name
        "Description": "A very cool module for HunterPie", // The plugin description
        "EntryPoint": "main.cs" // Specifies the file to be compiled by HunterPie
    }
```
