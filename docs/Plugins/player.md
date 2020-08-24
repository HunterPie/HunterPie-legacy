# Player

<a href="?p=Plugins/HunterPie.Core.md"><ns>namespace HunterPie.Core</ns></a>

This class has all the local player information.

## Events

All <Type>PlayerEvents</Type> events have two parameters, <Type>Object</Type> source and <Type>EventArgs</Type> args. Some of the events below have `EventArgs` that can be converted to either <Type>PlayerEventArgs</Type> or <Type>PlayerLocationEventArgs</Type>

### OnCharacterLogin
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player logins on a character.

### OnCharacterLogout
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player logs out from a character and goes back to the main screen or closes the game.

---

> The following event arguments can be converted from <Type>EventArgs</Type> to <Type>PlayerEventArgs</Type>

### OnLevelChange
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player high rank changes. 

### OnWeaponChange
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player changes classes. This is not triggered if you change from a weapon of the same class to another of the same class. (E.g: switching bows). Use [OnClassChange](#OnClassChange) for that instead.

### OnSessionChange
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when you join another session, or when the Session Id changes.

### OnClassChange
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player changes weapon. This is triggered even if the player changes weapons within the same class.

---

> The following event arguments can be converted from <Type>EventArgs</Type> to <Type>PlayerLocationEventArgs</Type>

### OnZoneChange
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player goes into another zone, like when teleporting, going into quests or returning from quests.

### OnPeaceZoneEnter
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player enters towns, houses, gathering hubs and the main menu. This is triggered after [OnZoneChange](#onZoneChange).

### OnVillageEnter
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player enters zones where the Harvest Box can be visible. This is triggered after [OnZoneChange](#onZoneChange).

### OnPeaceZoneLeave
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player leaves peaceful zones.

### OnVillageLeave
<params><Type>Object</Type> source, <Type>EventArgs</Type> args</params>

Occurs when the player leaves zones where Harvest Box widget can be visible.

## Properties

<properties>

**Type** | **Property name** | **Description**
:---|:-------------|:------------
<Type>String</Type> | Name | Player name.
<Type>Int32</Type> | Level | Player high rank.
<Type>Int32</Type> | MasterRank | Player master rank.
<Type>Int32</Type> | PlayTime | Player play time in second.
<Type>Bool</Type> | IsLoggedOn | Whether the player is logged on or not.
<Type>Byte</Type> | WeaponID | The current weapon class id.
<Type>String</Type> | WeaponName | The current class name.
<Type>Int64</Type> | ClassAddress | The current class information address in the game memory.
<Type>sPlayerSkill[]</Type> | Skills | Array with all player set skills. See the [sPlayerSkill](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/Definitions/sPlayerSkill.cs) structure for more details. 
<Type>Int32</Type> | ZoneID | The current stage id.
<Type>String</Type> | ZoneName | The current stage name in HunterPie's selected localization.
<Type>Int32</Type> | LastZoneID | The last stage id.
<Type>Bool</Type> | InPeaceZone | Whether the player is in a peaceful zone, this includes the main menu.
<Type>Bool</Type> | InHarvestZone | Whether the player is in a Harvest Box zone.
<Type>String</Type> | SessionID | The current session id.
<Type>Int64</Type> | SteamSession | The current steam session id. It's used by Steam for people to join your session.
<Type>Int64</Type> | SteamID | The local player steam account id.
<Type>Float</Type> | Health | The player current health.
<Type>Float</Type> | MaxHealth | The player maximum health.
<Type>Float</Type> | Stamina | The player current stamina.
<Type>Float</Type> | MaxStamina | The player maximum stamina.
<Type>Vector3</Type> | Position | The player current position in the map. See [Vector3](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/Definitions/sVector3.cs) for more details.
<Type>Party</Type> | PlayerParty | The current player party, see [Party](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/Party/Party.cs) for details.
<Type>HarvestBox</Type> | Harvest | The current player Harvest Box information. See [HarvestBox](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/HarvestBox.cs) for more details.
<Type>Activities</Type> | Activity | Argosy, Tailraiders and Steam fuel information. See [Activities](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Activities.cs) for more details.
<Type>Mantle</Type> | PrimaryMantle | Player current equipped Specialized Tool in the primary slot. See [Mantle](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Mantle.cs) for more details.
<Type>Mantle</Type> | SecondaryMantle | Player current equipped Specialized Tool in the secondary slot. See [Mantle](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Mantle.cs) for more details.
</properties>

### Jobs

Information about the current player class. Each weapon has it's own class and they're all based on a class named `Job`.

<properties>

**Type** | **Property name** | **Description**
:--------|:------------------|:-------------------
<Type>Greatsword</Type> | Greatsword | Greatsword information. See [Greatsword](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/Greatsword.cs) for more details.
<Type>DualBlades</Type> | DualBlades | Dual Blades information. See [DualBlades](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/DualBlades.cs) for more details.
<Type>Longsword</Type> | Longsword | Longsword information. See [Longsword](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/Longsword.cs) for more details.
<Type>Hammer</Type> | Hammer | Hammer information. See [Hammer](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/Hammer.cs) for more details.
<Type>Lance</Type> | Lance | Lance information. See [Lance](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/Lance.cs) for more details.
<Type>GunLance</Type> | GunLance | Gun Lance information. See [GunLance](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/GunLance.cs) for more information.
<Type>SwitchAxe</Type> | SwitchAxe | Switch Axe information. See [SwitchAxe](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/SwitchAxe.cs) for more details.
<Type>ChargeBlade</Type> | ChargeBlade | Charge Blade information. See [ChargeBlade](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/ChargeBlade.cs) for more details.
<Type>InsectGlaive</Type> | InsectGlaive | Insect Glaive information. See [InsectGlaive](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/InsectGlaive.cs) for more details.
<Type>Bow</Type> | Bow | Bow information. See [Bow](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/Bow.cs) for more details.
<Type>LightBowgun</Type> | LightBowgun | Light Bowgun information. See [LightBowgun](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/LightBowgun.cs) for more details.
<Type>HeavyBowgun</Type> | HeavyBowgun | Heavy Bowgun information. See [HeavyBowgun](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/LPlayer/Jobs/HeavyBowgun.cs) for more details.  

</properties>