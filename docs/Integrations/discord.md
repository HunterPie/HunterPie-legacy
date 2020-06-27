HunterPie has support for Discord Rich Presence, a way to show your friends what you're doing in Monster Hunter: World.

![example](https://cdn.discordapp.com/attachments/402557384209203200/694982548413087794/unknown.png)

## Main Features

### Player tracker

HunterPie will track your character data and display it on your Discord activity. HunterPie currently displays:

- Player Name;
- Player High Rank;
- Player Master Rank;
- Player equipped weapon;
- Player Location (e.g: If you're in Gathering Hub, Seliana, Ancient Forest, etc);
- Player Party;

### Monster Tracker

When the player is in a zone with monsters, the rich presence will be updated with the monster you're hunting and display its name and health percentage.
> **Note:** The health percentage display can be disabled.

### Join Requests

Other players with HunterPie can request to join your session directly from Discord! HunterPie will also display a request confirmation so you can accept the person without having to Alt+Tab to your Discord.
> **NOTE:** Due to how the "Ask To Join" Discord feature works, the person requesting to join the session **MUST** have HunterPie since it handles both `OnJoinRequest` and `OnJoin` events sent by Discord.

![Request confirmation](https://cdn.discordapp.com/attachments/629774066819268618/694747685000839218/d8224f0efe.png)