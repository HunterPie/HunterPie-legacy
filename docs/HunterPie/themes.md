## Introduction
HunterPie lets you customize some of it's UI, if you want to make your custom themes, please read this documentation carefully. If you have any issues, feel free to ask in `#themes-help` [Discord channel](https://discord.gg/HereZ8D).

## Getting Started

To make themes you need to edit text files, first make sure you have either [Notepad++](https://notepad-plus-plus.org/downloads/) or [Visual Studio Code](https://code.visualstudio.com/).

## Making your own monster bar theme

- Copy [this](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Resources/DefaultTheme.xaml) theme file and use it as a base for your theme.
- Save it in your `HunterPie/Themes` folder with whatever name you want (make sure the name doesn't collide with one of HunterPie's [main theme file name](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Themes/)).
- Open the file you just saved with a text editor.

## Monster Bar Theme Style Keys

If you look at the file content, you'll see that all styles have a Key, that's the style ID that HunterPie uses to get the style from your theme file and load it, **DO NOT** change the style Key, if you do that your theme won't show at all.

### Style key definitions

Here you can find what each key is used for, their `Type` and `TargetType`.

<details>
    <summary>Monster Bar</summary>

| x:Key                                      | Type      | TargetType            | Description
|:------------------------------------------:|:---------:|:---------------------:|:--------------------------------------------------------------------------------------------------:
|OVERLAY_MONSTER_BAR_WIDTH_3                 |sys:Double |          None         | Sets the monster component width when HunterPie is displaying 3 monster bars. 
|OVERLAY_MONSTER_BAR_WIDTH_2                 |sys:Double |          None         | Sets the monster component width when HunterPie is displaying 2 monster bars.
|OVERLAY_MONSTER_BAR_WIDTH_1                 |sys:Double |          None         | Sets the monster component width when HunterPie is displaying 1 monster bar.
|OVERLAY_MONSTER_BAR_HEIGHT                  |sys:Double |          None         | Sets the monster component height.
|OVERLAY_SHOW_MONSTER_ICON                   |Visibility |          None         | Sets wheter HunterPie the Monster icon visibility.
|OVERLAY_MONSTER_HEALTH_BAR_STYLE            |Style      |Custom:MinimalHealthBar| Sets the monster health bar style.
|OVERLAY_MONSTER_HEADER_DISPLAY              |Style      |StackPanel             | Sets the monster name and crown panel style.
|OVERLAY_MONSTER_NAME_TEXT_STYLE             |Style      |TextBlock              | Sets the monster name style.
|OVERLAY_MONSTER_BAR_TEXT_STYLE              |Style      |TextBlock              | Sets the monster health text style.
|OVERLAY_MONSTER_STAMINA_BAR_STYLE           |Style      |Custom:MinimalHealthBar| Sets the monster stamina bar style.
|OVERLAY_MONSTER_STAMINA_TEXT_STYLE          |Style      |TextBlock              | Sets the monster stamina text style.
|OVERLAY_MONSTER_WEAKNESS_DISPLAY            |Style      |StackPanel             | Sets the monster weakness panel style.
|OVERLAY_MONSTER_BACKGROUND                  |Style      |StackPanel             | Sets the whole monster component style.

</details>

<details>
    <summary>Monster Parts</summary>

| x:Key                                      | Type      | TargetType            | Description
|:------------------------------------------:|:---------:|:---------------------:|:--------------------------------------------------------------------------------------------------:
|OVERLAY_MONSTER_SUB_PART_STYLE              |Style      |Monster:MonsterPart    | Sets the monster parts component style.
|OVERLAY_MONSTER_PART_BAR_STYLE              |Style      |Custom:MinimalHealthBar| Sets the monster parts health bar style.
|OVERLAY_MONSTER_TENDERIZE_BAR_STYLE         |Style      |Custom:MinimalHealthBar| Sets the monster parts tenderized bar style.
|OVERLAY_MONSTER_PART_NAME_TEXT_STYLE        |Style      |TextBlock              | Sets the monster parts name text style.
|OVERLAY_MONSTER_PART_HEALTH_TEXT_STYLE      |Style      |TextBlock              | Sets the monster parts health text style.
|OVERLAY_MONSTER_PART_COUNTER_BACKGROUND_STYLE|Style     |Polyline               | Sets the monster parts counter background style.
|OVERLAY_MONSTER_PART_COUNTER_TEXT_STYLE     |Style      |TextBlock              | Sets the monster parts counter text style.

</details>

<details>
    <summary>Monster Ailments</summary>

| x:Key                                      | Type      | TargetType            | Description
|:------------------------------------------:|:---------:|:---------------------:|:--------------------------------------------------------------------------------------------------:
|OVERLAY_MONSTER_SUB_AILMENT_STYLE           |Style      |Monster:MonsterAilment | Sets the monster ailments component style.
|OVERLAY_MONSTER_AILMENT_BAR_STYLE              |Style      |Custom:MinimalHealthBar| Sets the monster ailments health bar style.
|OVERLAY_MONSTER_AILMENT_NAME_TEXT_STYLE        |Style      |TextBlock              | Sets the monster ailments name text style.
|OVERLAY_MONSTER_AILMENT_HEALTH_TEXT_STYLE      |Style      |TextBlock              | Sets the monster ailments health text style.
|OVERLAY_MONSTER_AILMENT_COUNTER_BACKGROUND_STYLE|Style     |Polyline               | Sets the monster ailments counter background style.
|OVERLAY_MONSTER_AILMENT_COUNTER_TEXT_STYLE     |Style      |TextBlock              | Sets the monster ailments counter text style.

</details>

--- 

## Custom Target Types
### sys:Double

To use a `sys:Double` type you must include the `System` namespace in your theme header. Just add the following line inside the `ResourceDictionary` tag on top of your theme file.

```xml
    xmlns:sys="clr-namespace:System;assembly=mscorlib"
```

### Custom:MinimalHealthBar

To use a `Custom:MinimalHealthBar` type, you must include the `Custom_Controls` namespace to your theme header. Just add the following line to the `ResourceDictionary` tag on top of your theme file.

```
    xmlns:Custom="clr-namespace:HunterPie.GUIControls.Custom_Controls;assembly=Hunterpie"
```

### Custom:MinimalHealthBar

To use a `Monster:MonsterPart` and `Monster:MonsterAilment` types, you must include the `Monster_Widget.Parts` namespace to your theme header. Just add the following line to the `ResourceDictionary` tag on top of your theme file.

```
    xmlns:Monster="clr-namespace:HunterPie.GUI.Widgets.Monster_Widget.Parts;assembly=Hunterpie"
```

---

## Colors, Gradients, etc...

XAML supports different kinds of brushes and you can use any of them in your theme. All the following examples will be using `OVERLAY_MONSTER_HEALTH_BAR_COLOR` for explanation purposes.

### Colors

XAML uses Hex Code with alpha channel (opacity)

|Color| # | A  | R  | G  | B  |
|-----|---|----|----|----|----|
|White| # | FF | FF | FF | FF |
|Transparent| # | 00 | FF | FF | FF |
| Black | # | FF | 00 | 00| 00

### Solid Color Brush

It's just one color, use this if you don't want a gradient/nothing fancy and one color is fine for you.

![ExampleSolidColorBrush](https://media.discordapp.net/attachments/402557384209203200/698180624552296568/unknown.png?width=718&height=352)

#### Code

```xml

<SolidColorBrush x:Key="OVERLAY_MONSTER_HEALTH_BAR_COLOR">#FF9E0000</SolidColorBrush>

```

### Linear Gradient Brush

Linear is well... Linear! It goes from `(X1, Y1)` to `(X2, Y2)`. Below we have an example of linear gradients that goes from the points (0.5, 0) to (0.5, 1).

![LinerGradientExample1](https://media.discordapp.net/attachments/402557384209203200/698187082941857812/Untitled-1.png?width=699&height=342)

Understanding points isn't really important, you can do it by trial and error until you get your gradient direction right.

#### StartPoint

It determines where the gradient will start, since the gradient is always relative to the bounds of the object, then we will always go from 0 to 1.

#### EndPoint

It determines where the gradient will end, it is also relative to the bounds of the object so the values can go from 0 to 1.

### Examples

#### Left to Right

- StartPoint: 0, 0.5
- EndPoint: 1, 0.5

![LeftToRightLinearExample](https://media.discordapp.net/attachments/402557384209203200/698190043528822884/unknown.png?width=687&height=319)

#### Bottom left to Top Right (Diagonal)

- StartPoint: 0, 1
- EndPoint: 1, 0

![BottomLeftToTopRightExample](https://media.discordapp.net/attachments/402557384209203200/698192177712594995/unknown.png?width=687&height=318)

#### Gradient Stop

A gradient stop is part of a gradient, it will tell which color you want in your gradient and the offset where that color will stop. You can have as many gradient stops as you want in a gradient. I recommend using [this site](https://cssgradient.io/) so you can choose colors and see the correct offsets.

![GradientStopExample](https://media.discordapp.net/attachments/402557384209203200/698194618776289371/unknown.png?width=963&height=276)

In that example we have 3 different `GradientStop` one that ends at 0, one that ends at 0.33 and the last one that ends at 0.63. Converting it to XAML would be like this:

```xml
    <LinearGradientBrush x:Key="OVERLAY_MONSTER_HEALTH_BAR_COLOR" StartPoint="0,0.5" EndPoint="1,0.5">
        <GradientStop Color="#FF7441E4" Offset="0"/>
        <GradientStop Color="#FFA032F9" Offset="0.33"/>
        <GradientStop Color="#FFE058E9" Offset="0.63"/>
    </LinearGradientBrush>
```

### Image Brush

You can also use images as brushes, so if you have a texture that you want to use, feel free to use it.

#### Properties
- **ImageSource:** It's the source of the image, this value should follow this format: `pack://siteoforigin:,,,/<Path to the image>`.
- **Stretch**
    - **None -** Will not stretch the image at all, keeping it the size of the original image.
    - **Fill -** Will stretch the image as much as possible to fit the size of the box, this option can deform the image.
    - **Uniform -** Will resize the image uniformly without deforming the image.
    - **UniformToFill -** This option will resize the image uniformly without deforming the image, but increasing it's size based on the widest side of the box.

```xml
    <ImageBrush x:Key="OVERLAY_MONSTER_HEALTH_BAR_COLOR" ImageSource="pack://siteoforigin:,,,/Themes/MyImageName.png" Stretch="None"/>

```


### Radial Gradient Brush

TODO

## Texts

TODO