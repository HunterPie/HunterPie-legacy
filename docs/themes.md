## Introduction
HunterPie supports partial theming, so you can change the colors, text style of the widgets but not the widget shape itself. If you want to make your own themes, please make sure to read this documentation.

## Getting Started

To make themes you need to edit text files, first make sure you have either [Notepad++](https://notepad-plus-plus.org/downloads/) or [Visual Studio Code](https://code.visualstudio.com/).

## Making your own monster bar theme

- Copy [this](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Themes/RedMonsterBar.xaml) theme file and use it as a base for your theme.
- Save it in your `HunterPie/Themes` folder with whatever name you want (make sure the name doesn't collide with one of HunterPie's [main theme file name](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Themes/)).
- Open the file you just saved with a text editor.

## Monster Bar Theme Style Keys

If you look at the file content, you'll see that all styles have a Key, that's the style ID that HunterPie uses to get the style from your theme file and load it, **DO NOT** change the style Key, if you do that your theme won't show at all.

| x:Key                                         | Type  | Description                                                                            |
|-----------------------------------------------|-------|----------------------------------------------------------------------------------------|
| OVERLAY_MONSTER_HEALTH_BAR_COLOR              | Color | Changes the monster health bar color.                                                  |
| OVERLAY_MONSTER_STAMINA_BAR_COLOR             | Color | Changes the monster stamina bar color.                                                 |
| OVERLAY_MONSTER_BAR_TEXT_STYLE                | Style | Changes the monster health bar text (the one that says the health/total health).       |
| OVERLAY_MONSTER_NAME_TEXT_STYLE               | Style | Changes the monster name text.                                                         |
| OVERLAY_MONSTER_AILMENT_BAR_COLOR             | Color | Changes the monster ailment bar color.                                                 |
| OVERLAY_MONSTER_PART_BAR_COLOR                | Color | Changes the monster part bar color.                                                    |
| OVERLAY_MONSTER_PART_NAME_TEXT_STYLE          | Style | Changes the monster part name text.                                                    |
| OVERLAY_MONSTER_PART_COUNTER_BACKGROUND_STYLE | Style | Changes the prism color that is on the left side of the part bar.                      |
| OVERLAY_MONSTER_PART_COUNTER_TEXT_STYLE       | Style | Changes the text style of the prism that is on the left side of the part/ailments bar. |
| OVERLAY_MONSTER_BACKGROUND                    | Style | Changes the monster container style.                                                   |

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