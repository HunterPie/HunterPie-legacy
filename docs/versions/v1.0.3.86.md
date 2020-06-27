**UPDATE 1.0.3.86**

**Specialized Tool Widget**

The Specialized Tools Widget got redesigned to match the rest of the overlay. The design changes include:

- **Specialized Tool Icons:** The specialized tool widget now shows the equipped tool icon.
- **Timer & Cooldown bar:** Instead of using a circle progress bar, the new design uses a bar, the bar has a similar effect to the damage meter bar. When the specialized tool is on cooldown, the bar will grow and when it's being used by the player the bar will shrink.

![SpecializedToolDesign](https://cdn.discordapp.com/attachments/402557384209203200/697167080339800095/unknown.png)

**Damage Meter Widget**

The damage meter now has way more information and also new settings, instead of choosing only one mode, you can choose whether you want to show only total damage, only damage per second, both, or none at all!

- **Show Total Damage:** If enabled, it will display all party members total damage.
- **Show Damage Per Second:** If enabled, it will display all party members damage per second.
> **Note:** If you join a quest in progress (e.g: SOS), the DPS will **not** be accurate, since the timer only starts when you join the quest, and not when the quest started.
- **Extra informations:** The meter also shows all party members HR, MR, and a crown on top left if that player is party leader.
- **Weapon Icons:** Some of the weapon icons were changed for better icons. [Special thanks to *Othello RHIN#1097* for his awesome icons](https://github.com/OthelloRhin/MHW_Icons_SVG)

![DamageMeterDesign](https://cdn.discordapp.com/attachments/402557384209203200/697175904417480754/unknown.png)

**Abnormalities Widget**

You can now enable abnormality names for each abnormality bar individually. So if you want to keep a debuff bar without names and a buff bar with names, for example, you can!

> **Attention:** Enabling the abnormality names option will force your bar to be vertically alligned.

![AbnormalityNameImage](https://cdn.discordapp.com/attachments/402557384209203200/697178504369799249/unknown.png)

**Monster Health Widget**

The monster widget also got new features:

- **New monster bar mode:** Shows all monsters until one is manually selected on the map. If the monster is unselected, then all monsters will be displayed again.
- **Health bar text format:** You can now change the text format for the health bar. Here are some useful text format presets for you:
    - **500/1000 (50%):** {Health:0}/{TotalHealth:0} ({Percentage:0}%)
    - **500/1000:** {Health:0}/{TotalHealth:0}
    - **50%:** {Percentage:0}%
    - **500 (50%):** {Health:0} ({Percentage}%)
    - **500.0/1000.0 (50.0%):** {Health:0.0}/{TotalHealth:0.0} ({Percentage:0.0})
> **Note:** Changing the text format does **not** require restarting, so feel free to play with the formats.

**Other Changes**

- Added position tooltip when you hover your mouse over a widget in design mode.

**Bug Fixes**

- Fixed monster health bar getting pushed down when it's docked at the bottom;
- Fixed monster data, read [this](https://github.com/Haato3o/HunterPie/pull/35) to see which monsters changed.