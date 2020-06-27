## How it works
Monster Hunter: World stores all the player damages in memory, it's used by the game after your hunt, when the MVP screen shows up. What HunterPie does is simply get that value, formats, calculate the damage per second based on when the hunt started and displays it in a widget.

## Why it isn't working in [INSERT ANYTHING HERE]?
Due to how it works, the game **DOES NOT** track your damage everywhere.
- It does **not** work when you have no quest objective;
- It does **not** work when you are in offline mode;
- It does **not** work in Kulve Taroth **siege** or Zorah Magdaros quest.

## Information tracked by the Damage Meter
The information that the damage meter widget displays are:
- **Party Leader:** It's represented by the small crown on top of their class icon.
- **Player HR and MR:** Represented by the number on top and on bottom respectively next to their class icon.
- **Player class:** Represented by their class icons.
- **Total Damage:** Number on top of the damage per second. This **can** be disabled.
- **Damage Per Second:** Number below the total damage. This **can** be disabled.
- **Damage percentage:** Represented by the right most number.

> **Note:** If you disable both total damage and damage per second, only the percentage will be visible.

![damageMeterWidget](https://media.discordapp.net/attachments/402557384209203200/697175904417480754/unknown.png?width=259&height=181)