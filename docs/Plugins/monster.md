# Monster

<a href="?p=Plugins/HunterPie.Core.md"><ns>namespace HunterPie.Core</ns></a>

## Events
### OnMonsterSpawn
<params><Type>Object</Type> source, <Type>MonsterSpawnEventArgs</Type> args</params>

This is dispatched right when the monster spawns and everything has been scanned properly.

### OnMonsterDespawn
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

This is dispatched when the monster leaves the map or it's corpse is despawned from the map.

### OnMonsterDeath
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

This is dispatched when the monster current health reaches zero.

### OnMonsterAilmentsCreate
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

This is dispatched when HunterPie finishes scanning all monster ailments from memory.

### OnCrownChange
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

This is dispatched whenever the monster size that indicates it's crown changes. Some monsters triggers this event multiple times.

### OnTargetted
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

This is dispatched whenever the monster is targeted by the local player.

### OnHPUpdate
<params><Type>Object</Type> source, <Type>MonsterUpdateEventArgs</Type> args</params>

This is dispatched whenever the monster current health value changes.

### OnStaminaUpdate
<params><Type>Object</Type> source, <Type>MonsterUpdateEventArgs</Type> args</params>

This is dispatched whenever the monster current stamina value changes.

### OnEnrage
<params><Type>Object</Type> source, <Type>MonsterUpdateEventArgs</Type> args</params>

This is dispatched whenever the monster becomes enraged.

### OnUnenrage
<params><Type>Object</Type> source, <Type>MonsterUpdateEventArgs</Type> args</params>

This is dispatched whenever the monster's enrage state is over.

### OnEnrageTimerUpdate
<params><Type>Object</Type> source, <Type>MonsterUpdateEventArgs</Type> args</params>

This is dispatched whenever the monster enrage timer value changes.

### OnAlatreonElementShift
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

This is dispatched whenever Alatreon shifts its elements. This event is used only by Alatreon.

## Properties

Type | Property name | Description
:----|:--------------|:-----------------
<Type>String</Type> | Id | Monster EM id
<Type>Int32</Type> | GameId | Monster game id
<Type>Float</Type> | SizeMultiplier | Monster current size multiplier, it defines the monster crown.
<Type>String</Type> | Crown | HunterPie internal crown icon name.
<Type>Float</Type> | MaxHealth | Monster maximum health.
<Type>Float</Type> | Health | Monster current health.
<Type>Dictionary&lt;String, Int32&gt; | Weakeness | Monster weaknesses, the key is the weakness icon name, the value is the weakness stars.
<Type>Float</Type> | HPPercentage | Health / MaxHealth
<Type>Bool</Type> | IsTarget | Whether this monster is the target or not
<Type>Int32</Type> | IsSelect | Whether this, another monster or no monsters are selected by the player.
<Type>Bool</Type> | IsAlive | Whether this monster is alive or not.
<Type>Bool</Tyoe> | IsActuallyAlive | Same as IsAlive, but changes it's state after everything has been scanned.
<Type>Float</Type> | EnrageTimer | Enrage current timer in seconds.
<Type>Float</Type> | EnrageTimerStatic | Enrage maximum timer in seconds.
<Type>Bool</Type> | IsEnraged | Whether the monster is enraged or not.
<Type>Float</Type> | Stamina | Monster current stamina.
<Type>Float</Tyoe> | MaxStamina | Monster maximum stamina.

