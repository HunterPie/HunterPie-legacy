# DaysLeftEventArgs

<ns>namespace HunterPie.Core.Events</ns>

## Properties 

### <Type>[Byte]</Type> Days

This depends on the [Modifier](#bool-modifier) property and on what's firing the event. See below:

- OnArgosyDaysChange - If [Modifier](#bool-modifier) is true, this indicates the days left until Argosy leaves town. If this is false, it indicates how long until Argosy comes back.
- OnTailraidersDaysChange - Indicates how many days until the Tailraiders are back from their adventure.

### <Type>[Bool]</Type> Modifier

Depends on what event triggered it. See below:

- OnArgosyDaysChange - Whether Argosy is in town or not.
- OnTailraidersDaysChange - Whether Tailraiders are deployed or not.
