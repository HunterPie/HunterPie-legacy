# HuntingHornSongCastEventArgs
<a href="?p=EventArgs/HunterPie.Core.Events.md"><ns>namespace HunterPie.Core.Events</ns></a>

## Properties

### <Type>Int32[]</Type> SongsIdsQueue

Current casted songs
### <Type>Int32[]</Type> RawSongsIdsQueue

Unordered version of <see cref="SongsIdsQueue"/>
### <Type>Int64</Type> SongIdsFirstIndex

First index of <see cref="RawSongsIdsQueue"/>
### <Type>Byte</Type> PlayStartAt

you want to start from the first, second or third song
### <Type>Int64</Type> PlayCurrentAt

Last song index played
### <Type>Bool</Type> IsCastingBuffs

Whether the player is casting the buffs based on their player Action id.
### <Type>Bool</Type> IsDoubleCasting

Whether the player is double casting songs.
