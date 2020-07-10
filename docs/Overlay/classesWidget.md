HunterPie v1.0.3.91 introduced a new feature. The Classes widget!

# What is the class widget?
It's a widget that shows the information about your current equipped weapon.

# Which weapons are supported?
Since it's a widget that is still in development, some weapons are not supported yet, but they are going to be added eventually. Right now the following classes are supported:

## Class Widget Settings
Each class has it's own separate setting, so if you want to place this widget for Charge Blade in (x, y) and for Insect Glaive in (x2, y2) you can! Use the Design Mode to rescale and move this widget. You can also disable them separately in your `HunterPie > Settings > Classes`.

- [Dual Blades](?p=Overlay/classesWidget.md#dual-blades);
- [Long Sword](?p=Overlay/classesWidget.md#long-sword);
- [Gun Lance](?p=Overlay/classesWidget.md#gun-lance);
- [Hammer](?p=Overlay/classesWidget.md#hammer)
- [Switch Axe](?p=Overlay/classesWidget.md#switch-axe);
- [Charge Blade](?p=Overlay/classesWidget.md#charge-blade);
- [Insect Glaive](?p=Overlay/classesWidget.md#insect-glaive);
- [Bow](?p=Overlay/classesWidget.md#bow);

## Dual Blades
- **Features:**
  - **Demon Gauge:** Represented by the sword gauge, it also has a blinking animation when the gauge is decreasing.
  - **Demon Mode Indicator** The dual blades icon indicates whether your Demon Mode is on.
  - **Safi'jiiva health regen. hit counter:** It's represented by the diamond with the Safi'jiiva skill icon, it shows how many hits until you actiave the health regeneration.

![dualBladesWidgetExample](https://cdn.discordapp.com/attachments/402557384209203200/721088634459258991/unknown.png)

## Long Sword
- **Features:**
  - **Spirit gauge:** It's represented by the sword gauge, it also has a indicator located on 70% of the gauge, this is the exact required amount you need for the **Spirit Jumping Slash + Spirit Blade III + Spirit Roundslash** and **Spirit I, II, III + Roundslash** comboes.
  - **Spirit Gauge blinking timer:** It's represented by the timer under the gauge, it's triggered by the Iai Slash and/or Helm Breaker.
  - **Charge level and percentage**: It's represented by the diamond and it's number by the right side of the gauge.
  - **Safi'jiiva health regen. hit counter:** It's represented by the diamond with the Safi'jiiva skill icon, it shows how many hits until you actiave the health regeneration.

![longswordWidgetExample](https://cdn.discordapp.com/attachments/678287048200683572/714161955807690782/class_p4.png)

## Hammer
- **Features:**
  - **Charge Bar:** It's represented by the diamond progress bar, when it reaches the maximum charge level the diamond starts blinking.
  > **Note:** The charging bar is affected by the scan delay you set in your HunterPie's settings, the default is 150ms, so it should not be a problem.
  - **Power charge indicator:** It's represented by the hammer icon on top right of the diamond. When power mode is off, the icon fades away.
  - **Charge level:** It's the number inside the diamond.
  - **Auto hide:** This class helper automatically hides when the weapon is sheathed.
  - **Safi'jiiva health regen. hit counter:** It's represented by the diamond with the Safi'jiiva skill icon, it shows how many hits until you actiave the health regeneration.

![hammerWidgetExample](https://cdn.discordapp.com/attachments/678287048200683572/716172353297711185/classes_p6.png)

## Gun Lance
- **Features:**
  - **Wyvernstake Blast timer:** It's represented by the timer in the main diamond and the orange diamond.
  - **Wyvern's Fire timer:** It's represented by the green diamond. It becomes red when it's on cooldown, and green again when it's available again.
  - **Ammo:** It's represented by the ammo icons, they work just like the in-game ammo counter.
  - **Special Ammo:** It's represented by the special ammo under the normal ammo. It works just like in-game;
  - **Safi'jiiva health regen. hit counter:** It's represented by the diamond with the Safi'jiiva skill icon, it shows how many hits until you actiave the health regeneration.

![gunlanceWidgetExample](https://cdn.discordapp.com/attachments/402557384209203200/711302826424401970/unknown.png) 

## Switch Axe
- **Features:**
  - **Axe Slam buff timer:** It's represented by the orange diamond.
  - **Sword inner gauge:** It's displayed by the right diamond.
  - **Safi'jiiva health regen. hit counter:** It's represented by the diamond with the Safi'jiiva skill icon, it shows how many hits until you actiave the health regeneration.
  - **Sword Charge:** It's represented by the left diamond, it displays the charge percentage and when the Switch Axe is fully charged a timer will start.

> **Note:** The timer takes longer to countdown and always start from 45 when using the Power Prolonger set skill. That's just how the game works, but should not be an issue when playing.

![switchAxeWidgetExample](https://cdn.discordapp.com/attachments/402557384209203200/711303655470661652/unknown.png)

## Charge Blade
- **Features:**
  - **Hidden Phial Charger gauge:** It's represented by the bar on the right side of the widget. It grows and changes it's color based on the stage of your phial charge.
  - **Phials counter:** It's represented by the top diamond.
  - **Sword buff timer:** It's represented by the left diamond.
  - **Shield buff timer:** It's represented by the right diamond.
  - **Poweraxe buff timer:** It's represented by the bottom diamond.
  - **Safi'jiiva health regen. hit counter:** It's represented by the diamond with the Safi'jiiva skill icon, it shows how many hits until you actiave the health regeneration.

![chargeBladeWidgetExample](https://cdn.discordapp.com/attachments/402557384209203200/711302522710655036/unknown.png)

## Insect Glaive
- **Features:**
  - **Charged Kinsect timer:** It's represented by the biggest diamond, the icon changes depending on the charge type.
  - **Red, White, Orange buff timers:** They're the smallest diamonds, when they're inactive the border stops glowing.
  - **Kinsect Stamina:** It's the bar under the smallest diamonds.
  - **Safi'jiiva health regen. hit counter:** It's represented by the diamond with the Safi'jiiva skill icon, it shows how many hits until you actiave the health regeneration.

![insectGlaiveWidgetExample](https://cdn.discordapp.com/attachments/402557384209203200/711302251993628744/unknown.png)

## Bow
- **Features:**
  - **Charge bar:** It's represented by the green diamond progress bar.
  > **Note:** The charging bar is affected by the scan delay you set in your HunterPie's settings, the default is 150ms, so it should not be a problem.
  - **Charge level and max charge level:** Represented by the numbers inside the diamond. Number on top is the current charge level, and the number on bottom is the maximum charge level.
  - **Auto hide:** This class helper automatically hides when the weapon is sheathed.
  - **Safi'jiiva health regen. hit counter:** It's represented by the diamond with the Safi'jiiva skill icon, it shows how many hits until you actiave the health regeneration.

![bowWidgetExample](https://cdn.discordapp.com/attachments/678287048200683572/714929411249668176/class_p5.png)