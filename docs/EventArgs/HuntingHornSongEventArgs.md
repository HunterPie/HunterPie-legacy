# HuntingHornSongEventArgs
<a href="?p=EventArgs/HunterPie.Core.Events.md"><ns>namespace HunterPie.Core.Events</ns></a>

## Properties

### <Type>sHuntingHornSong[]</Type> Songs

Available Hunting Horn songs
### <Type>Int32[]</Type> SongQueue

This array is organized based on the <seealso cref="LastSongIndex"/>
### <Type>Int32[]</Type> RawSongQueue

Same as <see cref="SongQueue"/>, but it's in the same order as they appear in memory.
### <Type>Int64</Type> SongsQueued

Number of songs in the <see cref="SongQueue"/>
### <Type>Int64</Type> LastSongIndex

Index of the last song added to the queue.
### <Type>Int32[]</Type> SongIndexesQueue

<see cref="Songs"/>.
### <Type>Int32[]</Type> RawSongIndexesQueue

Same as <see cref="SongIndexesQueue"/>, but in the order they appear in memory.
### <Type>Int64</Type> SongIndexesQueued

Number of songs queued in the <see cref="SongIndexesQueue"/>
### <Type>Int64</Type> SongIndexesFirstIndex

Index of the first song in the <see cref="RawSongIndexesQueue"/>
### <Type>Bool</Type> IsCastingSongs

Whether the player is casting buffs based on their player Action Id
