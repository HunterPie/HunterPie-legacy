
## HunterPie
### Why is my HunterPie crashing on startup?
If you use RivaTuner make sure to update it to the latest version, it has a bug that makes HunterPie crash because it uses Direct3D. Also make sure you have all the [**requirements**](https://haato3o.github.io/HunterPie/HunterPie/installation#requirements) to run HunterPie.

### I'm getting an error about MonsterHunterWorld.XXXXXX.map

Due to how HunterPie works, I have to map all static addresses manually, so it usually takes ~1 to 2 hours after a new Monster Hunter World patch hits for everything to work properly again. Also, always make sure auto-update is enabled when we get a new MHW version so HunterPie can always get the latest map version.

### Can I translate HunterPie's interface?
Yes! Just make sure to read [**this**](https://github.com/Haato3o/HunterPie/wiki/Localization) so you know how to get started.

## Overlay

### My overlay isn't appearing for me

#### DirectX 11
If you use DirectX 11 make sure your game is either in ***borderless window*** OR ***windowed mode***, the overlay will not be visible if you play in fullscreen mode. Make sure you have the overlay enabled and that the widgets do have content to show.

#### DirectX 12
DirectX 12 does let you see the overlay even when the game is in fullscreen mode. If you still can't see the overlay, make sure it's enabled and that all widgets you want are enabled. Try pressing your design mode hotkey (default is ScrollLock), if you still don't see the widgets then restart HunterPie with adminstrator privileges.

### My damage meter is not working

The game doesn't store player's damage in memory when you're in offline mode, in expedition/guiding lands or in certain quests (e.g: Zorah Magdaros's and Kulve Taroth's quest). It also doesn't store damages that your party have done to monsters that aren't part of your quest objective.

### I'm getting FPS issues

HunterPie also has a couple of settings to decrease resources usage:
- **Disable hardware acceleration:** Disabling hardware acceleration will make HunterPie use your CPU to render everything, removing the stress on your graphics card. This option will make everything smoother for some computers at cost of a little bit more of CPU.
- **Decrease animations framerate:** HunterPie is really animation heavy, so decreasing the animation framerate will make it use less resources.
- **Disable widgets that you don't want:** Disabling widgets that you don't want will make HunterPie have less stuff to render.
- **Reduce scan delay:** The scan delay is the time the scanner will wait to scan the memory again, making the number low will make the memory scanner read the game's memory more frequently, which means it will use more CPU. Increase the delay to decrease CPU usage at cost of accuracy.

> **Attention:** If you use GSync/FreeSync there's nothing I can do for now, [**that's a known issue for WPF**](https://github.com/dotnet/wpf/issues/2294) (HunterPie's UI framework) and is still in the to-be-fixed list in WPF's official GitHub.


### Can I resize and move widgets with my mouse instead of changing the X, Y coordinates manually?
Yes! A design mode was introduced on version 1.0.3.3, now you can press ScrollLock (you can change the keybind in HunterPie settings too) to enable design mode when the game is running. Use your left mouse button to move widgets around and your mouse scroll to rescale them individually.


### How do I use the abnormalities tray widget (player buff/debuff)?
HunterPie *v1.0.3.6* introduced an abnormality widget. Please, make sure to read the [**documentation**](https://github.com/Haato3o/HunterPie/wiki/Abnormalities-Tray) before using it.

### Monster parts, ailments, capture notification when?
That was introduced in version v1.0.3.80. Read the new monster widget [**documentation**](https://github.com/Haato3o/HunterPie/wiki/Monster-Widget) to learn about the new features.

### I can't see monster parts
Due to how the game works, only the quest host can see the monster parts health while the other party members only have access to the flinch/break counter.

## Discord Rich Presence

### My discord status isn't showing my in-game activity
Make sure you have it enabled in HunterPie settings. Also make sure you have the ***Display currently running game as a status message.*** option enabled on Discord. If none of these are your problem, run HunterPie with administrator privileges.

![Discord show game option](https://cdn.discordapp.com/attachments/402557384209203200/693553071581692024/unknown.png)