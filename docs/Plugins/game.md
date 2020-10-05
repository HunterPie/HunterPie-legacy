# Game

<a href="?p=Plugins/HunterPie.Core.md"><ns>namespace HunterPie.Core</ns></a>

This is the Game context, it has the Player and Monsters information.

## Events

### OnClockChange
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

This is dispatched every 10 seconds, it's mainly used by the Rich Presence to be updated, but you can use it too if you'd like.

## Properties
### Player
<params>Type: <a href="?p=Plugins/player.md"><Type>Player</Type></a></params>

This is the <a href="?p=Plugins/player.md"><Type>Player</Type></a> context, it handles everything related to the local player.

### FirstMonster
<params>Type: <a href="?p=Plugins/monster.md"><Type>Monster</Type></a></params>

This is the first monster context, it handles everything related to the first monster.

### SecondMonster
<params>Type: <a href="?p=Plugins/monster.md"><Type>Monster</Type></a></params>

This is the second monster context, it handles everything related to the second monster.

### ThirdMonster
<params>Type: <a href="?p=Plugins/monster.md"><Type>Monster</Type></a></params>

This is the third monster context, it handles everything related to the third monster.

### HuntedMonster
<params>Type: <a href="?p=Plugins/monster.md"><Type>Monster</Type></a></params>

This references the monster that is targeted by the player. A monster can be considered a target when it's either locked on (if the lock-on option is enabled) or if it's pinned by the player in the map.
