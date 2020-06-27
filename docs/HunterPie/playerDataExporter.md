## What's the Player Data Exporter?
It's a feature of HunterPie that exports your currently character basic information data, it's useful for streamers who want to make commands such as **!build**, **!session** so their viewers can easily request your build link or session ID, for example.

## Does it track monster information, map information?
No. It only tracks the basic information of your character, you can see the information it tracks in the following data structure:

```cs
    public struct Data
    {
        public string Name;            // This is your character name
        public int HR;                 // This is your character hunter rank
        public int MR;                 // This is your character master rank
        public string BuildURL;        // This is your current character build link to Honey Hunters World
        public string Session;         // This is your Session ID
        public string SteamSession;    // This is the link you can use so people can join your session directly from Steam
        public int Playtime;           // This is your current character playtime in seconds
    }
```

## How is the data exported?
The data is exported to a `json` file named `PlayerData.json` inside the DataExport folder. So you can easily parse it to integrate the information with your Twitch Bot commands.
Example of an exported data:
```json
{
  "Name": "Lyss",
  "HR": 254,
  "MR": 93,
  "BuildURL": "https://honeyhunterworld.com/mhwbi/?1962,277,235,264,169,244,65,-;;;;-at6;at5;de4;el4;el4,0,0,209,0,0,209,96,0,96,19,0,224,64,16,96,0,0,224,90,90,3:16:10:0:0:0:0",
  "Session": "j68fMsjZv2@W",
  "SteamSession": "steam://joinlobby/582010/109775241132541949/76561198067758630",
  "Playtime": 365015
}
```

## How often is the data exported?
The player data is exported whenever you change maps, session or weapon. Which means it keeps information as fresh as possible without using much resources.