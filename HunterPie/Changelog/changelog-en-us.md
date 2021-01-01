![banner](https://cdn.discordapp.com/attachments/402557384209203200/794657766395478016/v104.png)

# HunterPie v1.0.4

Happy New Year! HunterPie v1.0.4 is mainly focused on bug fixes and minor enhancements to the user experience.

## Monster Widget

- Added special break threshold logic for Elder Dragons. Some elder dragons have a different part break behavior (e.g: You can only break Teostra's head if their Health is under 20%).

## Core

- Added option to automatically minimize HunterPie after launching the game.
- Added option to disable Anti-Aliasing.
- Added safety measures to avoid `config.json` corruption.
- Added scrollbar to the settings tab when tabs overflow.
- Added monster position tracker.
- Added crafting data for every craftable item.
- Added `Debugger.LogObject(object obj);`
- Changed design for the changelog markdown.

## Bug Fixes

- Fixed Party & Party Members not displaying correctly when joining SOSes.
- Fixed Damage chart when joining in-progress quests.
- Fixed HunterPie crashing due to PID being under 10 when creating the Rich Presence connection.
- Fixed bug where EZ Max Potion would increase stamina bar in the player widget.
