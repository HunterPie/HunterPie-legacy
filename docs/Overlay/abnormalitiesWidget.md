# Introduction
HunterPie has a built-in abnormality tracker, you can see the list of supported abnormalities [here](?p=Internal/abnormalities.md).

## Table of Content

<ol id="content_table">
    <li><a href="#design-structure">Design Structure</a></li>
    <li><a href="abnormality-bar">Abnormality Bar</a></li>
    <ol>
        <li><a href="creating-a-new-bar">Creating a New Bar</a></li>
        <li><a href="configuring-your-new-bar">Configuring Your New Bar</a></li>
        <li><a href="abnormality-bar-orientations">Abnormality Bar Orientations</a></li>
        <li><a href="abnormalities-sorting">Abnormalities Sorting</a></li>
    </ol>
</ol>

## Design Structure

HunterPie's abnormality bar is designed to be clean, simple and highly customizable.

An Abnormality display has three basic informations:

- **Duration Bar:** It's a circular bar around the abnormality icon, it's a visual indicator on how much time left that abnormality has. It also indicates the abnormality type, depending on the color.
    - <span style="background:#329745">Green</span>: Buff;
    - <span style="Background:#973232">Red</span>: Debuff;
- **Icon:** It's the in-game icon for that abnormality.
- **Time Left:** A text representation of how much time left that abnormality has.

![Design](https://cdn.discordapp.com/attachments/402557384209203200/737050073648857148/abnorm_design.png)

The abnormality bar consists of a bunch of abnormalities display, representing each active buff or debuff your character has at the moment.

![DesignBar](https://cdn.discordapp.com/attachments/402557384209203200/737046253480837140/unknown.png)

## Abnormality Bar
### Creating a New Bar

HunterPie lets you create up to 5 different abnormality bars, each one of them has it's own separate settings, so you can have one that shows their name and other that does not, for example.

To create a new bar, just go to your HunterPie settings, click on the `Buff Bar` tab, and then click on the `+` icon.

<video controls width="600">
    <source src="https://cdn.discordapp.com/attachments/402557384209203200/737148193053081720/ieqsQFSXOa.webm"/>
</video>


> Note: If you have your game open while doing this, you will need to restart your HunterPie for the bar to appear.

### Configuring Your New Bar

After creating a new bar, you need to select which abnormalities you want that bar to display. To open your abnormality bar settings you have two options:

- Go to `HunterPie settings -> Buff bar` and click on the cog icon.<br>
OR
- Press <kbd>ScrollLock</kbd> (or whatever key you set to toggle Design Mode) to toggle Design Mode and then click on the cog icon that will appear on top right of your abnormality bar.

After you've opened the settings window, you can choose which abnormalities you want by clicking on their icon. You can also select them all by clicking on the `Select All` button.

<video controls width=600>
    <source src="https://cdn.discordapp.com/attachments/402557384209203200/737150212514119680/9wtX4CyM1a.webm">
</video>

### Abnormality Bar Orientations

HunterPie allows you to change the abnormality bar orientation, you can choose to either keep the bar vertically or horizontally aligned, each orientation has it's own behaviours.

- <highlight>**Horizontal:**</highlight> Keeping the bar horizontally aligned will make all abnormalities appear side-by-side, when the maximum *width* is reached, a new row will be created to fit all abnormalities within the bar bounds.
- <highlight>**Vertical:**</highlight> Keeping the bar vertically aligned will make all abnormalities appear on top of each other, when the maximum *height* is reached, a new column will be created to fit all abnormalities within the bar bounds.

![barsAlignment](https://i.gyazo.com/2dd724cfe94137187d8b8d2c4efa68c2.png)

### Abnormalities Sorting

Abnormalities are sorted based on their time left, HunterPie keeps the lowest time always at the top. HunterPie also warns the player when their buff is close to expire, a red blinking animation will start playing when that happens.

<video controls width=600>
    <source src="https://cdn.discordapp.com/attachments/402557384209203200/737147146725359616/lcqqLSfJMs.webm">
</video>