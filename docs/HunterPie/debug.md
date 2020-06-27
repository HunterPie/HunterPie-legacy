HunterPie has some built-in development options.

## Enable Debug Messages

This option will enable debug messages, mainly memory related messages or messages about monsters.

## Enable Unknown Monster Ailments

Since some of the statuses are still unknown, enabling this option will force HunterPie to not skip the unknown ailments. They'll show as `Unknown (ID)`.

## Custom Monster Data

This option lets you load a custom monster data file.

- Open your `config.json` located in your HunterPie root folder;
- Scroll all the way down and change the value of `CustomMonsterData` from `null` to the path of your custom monster data file.
> **WARNING:** The path must use either double backslashes or single slash.
- Start HunterPie, go to the debug settings tab and enable the `Load custom monster data` option and then restart HunterPie.

You can find the base Monster Data file [here](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/Monster/MonsterData.xml).