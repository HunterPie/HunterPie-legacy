[![Discord](https://img.shields.io/discord/678286768046342147?color=%237289DA&label=Discord&logo=Discord&logoColor=%2399AAB5&style=for-the-badge)](https://discord.gg/5pdDq4Q)

# HunterPie
HunterPie is a software that reads the game memory (only reads, it doesn't write anything to it) to find the necessary data for discord rich presence and to update the overlay with useful stuff.
HunterPie is a C# version of [HunterPy](https://github.com/Haato3o/HunterPy/), it has a better UI and overlay compared to the Python version and is also faster and lighter.

# Installation
You can download the latest version [here](https://github.com/Haato3o/HunterPie/releases/latest), extract the file and then run HunterPie.exe, it will auto-update whenever there's a newer version.

# How to use
## Rich presence
The discord integration is enabled by default, you just need to open your game and let HunterPie running in the background, however it can be disabled anytime in HunterPie settings.

## Overlay
**THE OVERLAY ONLY WORKS WHEN THE GAME IS IN BORDERLESS FULLSCREEN OR WINDOWED!**

HunterPie has an overlay that can be toggled on/off. You can also disable any of the widgets in the overlay separately in case you don't like one of them as well as change the widgets position.

### Monster health
Shows monsters name, current health, total health, monster weaknesses, monster size crown, the enrage icon pops up next to monster's name and the health bar starts blinking whenever monster is enraged.
![Monsters health widget](https://camo.githubusercontent.com/2b9e5ac20443a947446005d2e6ac132417adb6f3/68747470733a2f2f692e696d6775722e636f6d2f625642364148772e706e67)

### DPS Meter

This widget shows your party DPS. You can customize colors in HunterPie settings tab. This widget will show your damage per second if you joined the quest at the start, and total damage if you joined after the quest was started (e.g: in a S.O.S)
> **Note:** It does not work in expeditions due to how Monster Hunter World works.

![DPS Meter solo](https://cdn.discordapp.com/attachments/402557384209203200/681543050337714189/unknown.png) 
![DPS Meter Party](https://cdn.discordapp.com/attachments/402557384209203200/681546567748157482/unknown.png)

### Mantles Cooldown
Shows the mantles cooldown and timer when wearing one, this widget only appears when the cooldown and timer are actually running. You can also change the widget color in HunterPie settings, it uses HEX color with alpha (#RRGGBBAA)

### Harvest Box
Shows fertilizer names and amount left and the total items in harvest box. It only appears in Astera and gathering hub.

### More widgets and info to be added soon...

### Uninstallation

Just delete HunterPie folder.

## Bugs and suggestions
If you find any bug or have any suggestion, please contact me on Discord (Haato#0704) or open an issue ticket [here](https://github.com/Haato3o/HunterPie/issues)

## Credits
+ [R00telement](https://github.com/r00telement) for his [SmartHunter](https://github.com/r00telement/SmartHunter) application that helped me finding the monsters health
+ [Ezekial711](https://github.com/Ezekial711) for his [modding guide](https://github.com/Ezekial711/MonsterHunterWorldModding) that helped me find and map all mantles