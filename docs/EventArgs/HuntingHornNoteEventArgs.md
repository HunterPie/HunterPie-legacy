# HuntingHornNoteEventArgs
<a href="?p=EventArgs/HunterPie.Core.Events.md"><ns>namespace HunterPie.Core.Events</ns></a>

## Properties

### <Type>Byte[]</Type> RawNotes

The last 4 notes the player has played, unorganized and in the same order as they appear in memory.
### <Type>Byte[]</Type> Notes

The last 4 notes the player has played, this is organized in the same order as they appear in the HUD
### <Type>Int64</Type> FirstNoteIndex

The index of the first note, this is required if you're using the <b>RawNotes</b>.
### <Type>Int64</Type> NotesQueued

The amount of valid notes currently in <b>RawNotes</b> array.
### <Type>sHuntingHornSong[]</Type> Candidates

Possible songs that can be played with the current Notes
