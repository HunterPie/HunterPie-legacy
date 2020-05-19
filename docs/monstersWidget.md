A new monster widget was introduced in HunterPie version 1.0.3.80, the whole design was remade from scratch, new features were added and now it supports monster ailments/statuses and parts health.

## Modes
The monster widget has 4 different modes. You can switch modes either by changing them manually in the settings tab, or by pressing your hotkey to change the bar mode (default is Alt+Up)

### Show all monsters at once
This mode shows all monsters at once, it's the default mode.

![Show all monsters at once](https://cdn.discordapp.com/attachments/402557384209203200/692795094151200848/unknown.png)

### Show all monsters but highlight my target
This mode shows all monsters but decreases the size and opacity of the monsters that aren't your target.

![Show all but highlight target](https://cdn.discordapp.com/attachments/402557384209203200/692795152476930108/unknown.png)

### Show only hunted monster
This mode is basically a boss bar, it is always centrelized and it's width is higher than the default bars

![Boss bar mode](https://cdn.discordapp.com/attachments/402557384209203200/692794622077829130/unknown.png)

### Show all but hide inactive monsters after X seconds
It's similar to the first mode, but it hides the monster that haven't been damaged after 15 seconds.

## Targetting a monster
If you have the ***Only show target*** mode enabled, you need to manually open your in-game map and pin the monster you want. However, quests do that automatically for you, so you only need to manually pin your target when you're in expeditions/Guiding Lands.

## Features
- **Enrage:** The Monster Widget now has a better enrage animation, the whole bar and the monster icon will start blinking red and a timer will be shown under the icon, the timer shows when the enrage is going to end.
- **Parts & Ailments:** They're shown under or above the monster bar depending on which docking you chose in settings (read more about docking [here](#TODO)). By default they will be visible whenever a value change, like counter, timer, buildup or health and will be hidden again after a few seconds (configurable in settings).
> **WARNING:** You need to be quest host to have access to the part health information. If you play multiplayer and is not the host, then the parts will always show with full health, but the flinch/counter will work normally.
- **Stamina:** Stamina is shown under the boss bar, the yellow bar and the numbers under it represents that monster stamina.
- **Capture Indicator:** When the monster HP is under its capture threshold, a shock trap icon will pop up on the top right next to that monster's health bar.
- **Monster Crown:** Crowns are displayed by the monster name.

## Settings & Customization
Currently the new monster widget supports:
- Enable/Disable widget;
- Change Monster bar mode;
- Change the docking
  - **Top:** The monster bar will stay on top of the parts/ailments;
  - **Bottom:** The monster bar will stay under the parts/ailments;
- Enable monster weaknesses;
- Hide inactive parts after X seconds;
- Customize how many seconds until it hides the parts;
- Enable parts individually, breakable parts and ailments;
- Choose how many columns and rows HunterPie should use to show parts/ailments;
