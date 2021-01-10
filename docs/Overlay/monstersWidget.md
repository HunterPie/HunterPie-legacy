# Monster Widget

Monster widget displays useful information about the current monsters in the map. It also has many options for customization.

### Table of content

<ol id="content_table">
    <li><a href="#design-structure">Design Structure</a></li><li>
    <a href="#string-formats">String Formats</a></li>
    <ol>
        <li><a href="#health-bar-formats">Health Bar Formats</a></li>
        <li><a href="#part-health-bar-formats">Part health bar formats</a></li>
    </ol>
    <li><a href="#monster-bar-modes">Monster Bar Modes</a></li>
    <ol>
        <li><a href="#show-all-monsters-at-once">Show all monsters at once</a></li>
        <li><a href="#show-all-monsters-but-hightlight-my-target">Show all monsters but highlight my target</a></li>
        <li><a href="#show-only-hunted-monster">Show only hunted monster</a></li>
        <li><a href="#show-all-but-hide-inactive-monsters-after-x-seconds">Show all but hide inactive monsters after X seconds</a></li>
        <li><a href="#show-all-until-a-monster-is-selected">Show all until a monster is selected</a></li>
    </ol>
    <li><a href="#targeting-a-monster">Targeting a Monster</a></li>
    <ol>
        <li><a href="#targeting-a-monster-from-map-default">Targeting a monster from map (Default)</a></li>
        <li><a href="#targeting-a-monster-using-in-game-lockon">Targeting a monster using in-game lockon</a></li>
    </ol>
    <li><a href="#monster-parts--ailments">Monster Parts & Ailments</a></li>
    <ol>
        <li><a href="#parts--removable-parts">Parts & Removable Parts</a></li>
        <li><a href="#ailments">Ailments</a></li>
    </ol>
</ol>

## Design structure
![design-structure-image](https://cdn.discordapp.com/attachments/402557384209203200/732313125923192852/monster_widget_structure.png)

## String formats

### Health bar formats

The monster health bar supports customizable string formatting, here's the list of the current supported special strings and what will replace them:

Format              | Replaced by                               | Example
--------------------|-------------------------------------------|----------
{Health:0}          | Rounded down health                       | 14595
{Health:0.0}        | Health with 1 decimal place               | 14595.5
{TotalHealth:0}     | Rounded down total health                 | 15000
{TotalHealth:0.0}   | Total health with 1 decimal place         | 15000.0
{Percentage:0}      | Health percentage rounded down            | 97%
{Percentage:0.0}    | Health percentage with 1 decimal place    | 97.3%

### Part health bar formats

The monster parts health bar also support customizable string formatting, here's the list:

Format              | Replaced by                               | Example
--------------------|-------------------------------------------|----------
{Current}           | Current part health                       | 435
{Max}               | Maximum part health                       | 600
{Percentage}        | Percentage of health                      | 72%
{Tenderize}         | Time left until tenderize effect expires  | 2:00

## Monster bar modes
The monster widget has 5 different bar modes, each one has it's own behaviour and characteristics. You can switch between them by either going to HunterPie settings tab and clicking on the `Monster bar mode` box or pressing the <kbd>Alt</kbd>+<kbd>Up</kbd> hotkey.

### Show all monsters at once
This mode shows all monsters at once, it's the default mode.

![Show all monsters at once](https://cdn.discordapp.com/attachments/402557384209203200/732293041594957955/unknown.png)

### Show all monsters but highlight my target
This mode shows all monsters but decreases the opacity of the monsters that aren't your target.

![Show all but highlight target](https://cdn.discordapp.com/attachments/402557384209203200/732293162487251054/unknown.png)

### Show only hunted monster
This mode is basically a boss bar, it is always centrelized and it's width is higher than the default bars. Having no targets will hide **ALL** monster bars.

![Boss bar mode](https://cdn.discordapp.com/attachments/402557384209203200/732293343425462282/unknown.png)

### Show all but hide inactive monsters after X seconds
It's similar to the first mode, but it hides the monster that haven't been damaged after 15 seconds.

### Show all until a monster is selected
This mode is similar to the [Show only hunted monster](#show-only-hunted-monster), but when there's no target, all monsters are displayed.

## Targeting a monster
There are two ways to target a monster, both of them have it's pros and cons:

### Targeting a monster from map (Default)
This is the default way to target a monster, when you get in a quest the game will automatically target your quest objective for you, but when you are in expeditions/Guiding Lands you **must** open your map and target the monster by pressing <kbd>Tab</kbd> on their icon or by clicking their investigation gauge on bottom left of your map.

> **Note:** If there are more than 1 monster as the quest objective, the game will only automatically target the first one, so you'll have to manually target the next one.

> **Note 2:** If you haven't found the monster in map yet, the game will only automatically target it for you after you've discovered them.

### Targeting a monster using in-game lockon
This option must be turned on in your `HunterPie Settings -> Monster` tab. It will make HunterPie set the target based on which monster you have locked-on. This is the most intuitive way to target a monster, however has some limitations:
- In order for the lockon work properly, you need to set the `Target settings` in your game to **Large Monsters Only**, otherwise small monsters will mess with the lockon system.
- Captured monsters will mess with the lockon, this is a known issue that is still in progress to fix. However it is fixed after the monster despawn.

> **Note:** For a better experience with the lock-on, I strongly recommend these settings:<br>
![lockonSettings](https://cdn.discordapp.com/attachments/678286885059166228/718120460856066098/unknown.png)

## Monster Parts & Ailments

<warning>WARNING: THIS FEATURE DOES NOT WORK PROPERLY IF YOU'RE NOT THE PARTY HOST, IT'S A GAME LIMITATION AND I CANNOT FIX IT.</warning>

### Parts & Removable Parts

Monster parts are, well... Parts of the monster that can be damaged! See the design structure below:

![partStructureDesign](https://cdn.discordapp.com/attachments/402557384209203200/732323920685957180/monster-part-structure.png)

Parts that can be broken have a break threshold shown by the side of the flinch counter. See the image below:

![flinchBreakExplanation](https://cdn.discordapp.com/attachments/402557384209203200/732326495732760595/counter-exp.png)

- **n/x+:** This means there are multiple break thresholds, it will break for the first time at `x` flinches.
- **n/x:** This means there are only one break threshold, it will break when you flinch that part `x` times.
- **n**: This means you cannot break this part, it is just a flinch counter.

---

### Tenderize Timer

HunterPie can track and display each part tenderize timer, however, since they're in the same part display component on the Overlay UI, you need that part to be enabled in order to see it.

A Tenderize bar will appear under the affected part health bar, keeping that part always visible until the effect is over.

![tenderizeBar](https://media.discordapp.net/attachments/402557384209203200/741015551383437412/unknown.png?width=250&height=53)

### Ailments

Monster ailments have the same design as the part displays, however some ailments have two states: Buildups and Timers.

- **Buildups:** Some ailments like paralysis, poison, blast, etc, have what's called a "Buildup", whenever you hit the monster, it may increase the buildup value by a bit, whenever the buildup hits the maximum value the monster is affected by that abnormality and a timer might start depending on the ailment.
- **Timer:** A timer that counts down until the ailment effect is over.
- **Counter:** Counts how many time you've inflicted that ailment on the monster.

### Settings

Disabling Ailments will cause the parts to use all the available display, same thing happens when you disable parts and removable parts.

### Understanding parts columns and rows

Some monster have a lot of parts and ailments, HunterPie has a limit of how many parts it can show at once, it can be configured in your `HunterPie settings -> Monsters`. 

Columns and rows are dynamic, which means they will resize depending on how many displays it needs to fit in the parts/ailments display.

#### Columns
Adding more columns will display more parts when there's an overflow in the last column.

### Rows
Adding more rows will make each column display more parts at each column.

> Example: 2 columns & 8 rows<br>
![example1](https://cdn.discordapp.com/attachments/402557384209203200/732293562183581818/unknown.png)

> Example 2: 3 columns & 8 rows<br>
![example2](https://cdn.discordapp.com/attachments/402557384209203200/732293792341819392/unknown.png)