![banner](https://cdn.discordapp.com/attachments/402557384209203200/743894519329587251/update-10396.png)

**Plugins System**

HunterPie now supports plugins. Plugins have access to **everything** HunterPie handles, including player data, monster data, game events. If you're interested in developing your own extensions, you can find an example [here](https://github.com/Haato3o/HunterPie.Plugins/blob/master/TwitchIntegration/main.cs).

**Monster Widget**


- **String formats:** Added ailments build, timers and parth health text format option. See the special strings below:
    - **{}{Current}** - Current bar value.
    - **{}{Max}** - Maxium bar value.
    - **{}{Percentage}** - Current value / Maximum value * 100 (Without the % at the end).

Special strings are strings that will be replaced when HunterPie renders the text. They are case-sensitive and need to be written exactly as shown in the table above.

**Examples:**
- **{}{Current}/{Max} ({Percentage}%)**: 500/1000 (50%)
- **{}{Percentage}%**: 50%

---

**Specialized Tools Widget**

- **Compact mode:** A compact mode for the Specialized Tools Widget has been added, it will only show the cooldown and current timer, the tool icon and a diamond bar around the icon.

![img](https://cdn.discordapp.com/attachments/402557384209203200/742950877828087808/design_mantle_compact.png)

---

**Timers**

- Quest timer is now automatically adjusted depending on your *Focus Skill* level.
- Buff timers affected by *Power Prolonger* are now automatically adjusted depending on your Skill level.

---

**Damage Meter Widget**

- **DPS Counter:** Damage meter now calculates the *Damage Per Second* **AFTER** a party member hits the monster for the first time.
- **Resize option:** You can now resize the damage meter widget, making it horizontally or vertically aligned.

![wee](https://cdn.discordapp.com/attachments/402557384209203200/743905320107245649/unknown.png)

---

**Design Mode**

- Minor optimizations to the Design Mode toggle, it should take less time for Widgets to be rendered when toggling the Design Mode.
- Added Position, Scale and Render time text to the Widgets when Design Mode is enabled.

---

**Other Changes**

- Added Frostfang Barioth break thresholds.
- Added style file to overwrite styles globally for all themes without having to make a new theme from scratch. You can find that file in HunterPie.Resources/UI/Overwrite.xaml.
- Upgraded to .NET Framework 4.8.
- Added option to force DirectX 11 optimized fullscreen to be behind the overlay.
- Settings window components now matches the window width instead of staying at a fixed witdth.

---

**Bug Fixes**

- Fixed abnormalities tray adding an empty space to the abnormalities icons when in Design Mode;
- Fixed bug that would corrupt user's *config.json* if their computer was shutdown unexpectedly;
- Fixed HunterPie hooking to Stracker's console window to get the game version instead of getting the game window;
