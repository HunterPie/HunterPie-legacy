# Game

<ns>namespace HunterPie.Core</ns>

This is the Game context, it has the Player and Monsters information.

## Events

### OnClockChange
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

This is dispatched every 10 seconds, it's mainly used by the Rich Presence to be updated, but you can use it too if you'd like.

## Properties
### Player
<params>Type: <Type>Player</Type></params>

This is the <Type>Player</Type> context, it handles everything related to the local player. You can read more [here](?p=Plugins/player.md).

### FirstMonster
<params>Type: <Type>Monster</Type></params>

This is the first monster context, it handles everything related to the first monster. You can read more [here](?p=Plugins/monster.md)

### SecondMonster
<params>Type: <Type>Monster</Type></params>

This is the second monster context, it handles everything related to the first monster. You can read more [here](?p=Plugins/monster.md)

### ThirdMonster
<params>Type: <Type>Monster</Type></params>

This is the third monster context, it handles everything related to the first monster. You can read more [here](?p=Plugins/monster.md)

### HuntedMonster
<params>Type: <Type>Monster</Type></params>

This references the monster that is targeted by the player. A monster can be considered a target when it's either locked on (if the lock-on option is enabled) or if it's pinned by the player in the map.