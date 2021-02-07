![banner](https://cdn.discordapp.com/attachments/402557384209203200/804141562182369310/v105.png)

# HunterPie v1.0.5

HunterPie v1.0.5 brings a lot of new features, including new plugin window to make it even easier to install plugins, HunterPie Native, new APIs and much more!

## HunterPie Native

HunterPie native is a native extension for HunterPie that gets injected into the game itself so HunterPie can interact with it natively (e.g call in-game functions, use the chat, receive events, etc). Some of the native functions require [CRCBypass](https://www.nexusmods.com/monsterhunterworld/mods/3473), so make sure to install them first if you want to use the game function hooking features.

### Native features

- **Input injection:** HunterPie can now simulate inputs, this is useful if a plugin needs to interact with the game UI.
- **Chat messages:** HunterPie can now send messages to the chat.
- **System messages:** HunterPie can now send system messages to the game client.
- **GMD parsing & indexing:** HunterPie can now get localized strings directly from the game.

## Plugins

The plugins window was completely redesigned, adding new functionalities and features. Official plugins are also indexed in this new plugins window. 

> Credits to Amadare for implementing it.

## Other Changes

- Added WriteProcessMemory API.
- Added streamer mode for widgets, it will let OBS and similar window capture softwares to detect & capture HunterPie's widgets.
- Refactored the Widget class to make it easier for plugins to create their own widgets. 

## Bug Fixes

- Fixed HunterPie creating Overlay and initialized game scanner when the game version isn't supported
- HunterPie should no longer crash due to events callback
- Fixed textless Patreon button in the Links tab.
