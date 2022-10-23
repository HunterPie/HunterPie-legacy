![image](https://user-images.githubusercontent.com/35552782/181646644-2ee86197-1c6a-4447-ab27-b9ec5b93a34a.png)

[![Discord](https://img.shields.io/discord/678286768046342147?color=7289DA&label=Discord&logo=discord&logoColor=white&style=flat-square)](https://discord.gg/5pdDq4Q)
[![NexusMods](https://img.shields.io/badge/Download-Nexus-white.svg?color=da8e35&style=flat-square&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/monsterhunterworld/mods/2645)
[![Paypal](https://img.shields.io/badge/donate-Paypal-blue.svg?color=62b2fc&style=flat-square&label=Donate)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=F2QA6HEQZ366A&source=url)
[![Patreon](https://img.shields.io/badge/Support-Patreon-blue.svg?color=fc8362&style=flat-square&logo=patreon&logoColor=white)](https://www.patreon.com/HunterPie)

[![GitHub license](https://img.shields.io/github/license/Haato3o/HunterPie?color=c20067&style=flat-square)](https://github.com/Haato3o/HunterPie/blob/master/LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/Haato3o/HunterPie?color=b440de&style=flat-square)](https://github.com/Haato3o/HunterPie/stargazers)

> **Warning**: 
> This version of HunterPie is no longer maintained and instead has been replaced by [HunterPie v2](https://github.com/HunterPie/HunterPie).


## About
HunterPie is a modern and simple to use overlay with support for Discord Rich Presence for Monster Hunter: World.

## How to install

#### Requirements

- [.NET Framework >= 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)

#### Installation

- Download the latest release [here](https://github.com/Haato3o/HunterPie/releases/latest);
- Extract it anywhere you want;
- Open the extracted folder and start the **HunterPie.exe**, it will automatically look for new updates.

#### Uninstallation

- Delete HunterPie folder

## Build instructions

If you want to build HunterPie by yourself, you might need:
- [Python](https://www.python.org/downloads/)
- [NuGet](https://www.nuget.org/downloads)

For the release build:

```bash
nuget restore HunterPie.sln
msbuild HunterPie.sln -property:Configuration=Release
```

The apps will be in _{HunterPie|Update}/bin/Release_

> **ATTENTION:** Don't forget to disable auto-update, otherwise your local build will be overwritten by the files in HunterPie's update server.

## Features

### Core
- Automatic updates
- [Build exporter to Honey Hunters World](https://hunterpie.haato.dev/?p=Integrations/honeyHuntersWorld.md)
- [Decoration & Charms exporter to Honey Hunters World](https://hunterpie.haato.dev/?p=Integrations/honeyHuntersWorld.md)
- [Automatic Player Data Exporter](https://hunterpie.haato.dev/?p=HunterPie/playerDataExporter.md)
- [Discord Rich Presence Support](https://hunterpie.haato.dev/?p=Integrations/discord.md)
- [Plugin Support](https://github.com/Haato3o/HunterPie.Plugins)

### Overlay
- [Monster Widget](https://hunterpie.haato.dev/?p=Overlay/monstersWidget.md)
- [Harvest Box Widget](https://hunterpie.haato.dev/?p=Overlay/harvestBoxWidget.md)
- [Specialized Tools Widget](https://hunterpie.haato.dev/?p=Overlay/specializedToolWidget.md)
- [Abnormalities Tracker Widget](https://hunterpie.haato.dev/?p=Overlay/abnormalitiesWidget.md)
- [Class Helper Widget](https://hunterpie.haato.dev/?p=Overlay/classesWidget.md)
- [Damage Meter Widget](https://hunterpie.haato.dev/?p=Overlay/damageMeterWidget.md)
