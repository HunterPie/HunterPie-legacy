# Streamer Mode

Streamer mode exists to circumvent a problem with OBS and similar window capturing softwares. It basically removes the window flags from the widgets that made them "Invisible" to your Alt+Tab because OBS can't capture invisible windows.

## Configuring streamer mode

1. Enable Streamer mode on whatever widget you want to capture.
<img src="https://cdn.discordapp.com/attachments/402557384209203200/808512616241954856/unknown.png" width="50%">

2. Open your OBS and create a new capture source.
<img src="https://media.discordapp.net/attachments/402557384209203200/808513028974444554/unknown.png" width="50%">

3. Select Window Capture and create the new capture source.
<img src="https://media.discordapp.net/attachments/402557384209203200/808513197166166037/unknown.png" width="50%">

4. Select the Widget you want, **SELECT THE CAPTURE METHOD AS WINDOWS GRAPHICS CAPTURE** and the "Window title must match" option.
<video controls width="600">
    <source src="https://cdn.discordapp.com/attachments/402557384209203200/808513577832284220/RQxi91MCBA.webm">
</video>

## Removing the black background from a widget in OBS

OBS has issues when capturing a transparent window, so in order to circumvent that, we need to apply a *color key* filter to remove the black color.

<img src="https://media.discordapp.net/attachments/402557384209203200/808514960694050837/unknown.png" width="50%">

1. Right click your window capture you created for the widget and click on "Filters"
<img src="https://media.discordapp.net/attachments/402557384209203200/808515163660877904/unknown.png" width="50%">

2. Create a new filter and select the *Color Key*
<video controls width="600">
    <source src="https://cdn.discordapp.com/attachments/402557384209203200/808516129681178664/kjZfKRCbSS.webm">
</video>

3. Select "Custom Color" and then select the black (#000000FF)
<img src="https://media.discordapp.net/attachments/402557384209203200/808516323498917938/unknown.png" width="50%">

> **NOTE:** You can also play around with the other settings until you find the sweet spot for that widget transparency, however, always keep in mind: This is far from perfect, but it was introduced to workaround OBS limitations.