HunterPie supports different languages, if you want to make your own localization file, please make sure to read this.

You can see which languages HunterPie currently supports [here](https://github.com/Haato3o/HunterPie/tree/master/HunterPie/Languages).

### Supported Languages
- [German](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/de-de.xml)
- [English](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/en-us.xml)
- [Spanish](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/es-es.xml)
- [French](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/fr-fr.xml)
- [Italian](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/ita-it.xml)
- [Japanese](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/jp-jp.xml)
- [Korean](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/ko-kr.xml)
- [Portuguese (Brazilian)](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/pt-br.xml)
- [Russian](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/ru-ru.xml)
- [Simplified Chinese](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/zh-cn.xml)
- [Traditional Chinese](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/zh-tw.xml)

### Getting Started

#### Requirements

- A decent text editor, I recommend:
    - [Visual Studio Code](https://code.visualstudio.com/)
    - [Sublime Text](https://www.sublimetext.com/)
    - [Notepad++](https://notepad-plus-plus.org/)
- The english strings file, you can find it [here](https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Languages/en-us.xml).

### Localizing

Now that you have the requirements, it's time to start translating!

Use the `en-us.xml` you have downloaded as a base file, **make sure to rename the file to anything else** so it doesn't get overwritten during the HunterPie update process.

Do not touch the first line of the XML document, that is telling our XML which version of XML we are using, and which encoding type that file is. It should always stay like this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
```

On second line, change the value of `lang` to whatever language you are translating it to.
E.g
```xml
<!-- Before -->
<Strings lang="English">

<!-- After -->
<Strings lang="Japanese">
```

Now we can translate the rest of the file! Always change the `Name` not the `ID`. The `ID` is used internally by HunterPie and should not be touched. 
> **Note:** Please, try to keep all translations as accurate as possible with the in-game strings. You can simplify long strings as long as they don't lose their meaning, though.

E.g:
```xml
<!-- Before -->
<Monsters>
    <Monster ID="em001_00" Name="Rathian"/>
    <Monster ID="em001_01" Name="Pink Rathian"/>
    <Monster ID="em001_02" Name="Gold Rathian"/>
    <Monster ID="em002_00" Name="Rathalos"/>
    <Monster ID="em002_01" Name="Azure Rathalos"/>
    [...]

<!-- After -->
<Monsters>
    <Monster ID="em001_00" Name="リオレイア"/>
    <Monster ID="em001_01" Name="リオレイア亜種"/>
    <Monster ID="em001_02" Name="リオレイア希少種"/>
    <Monster ID="em002_00" Name="リオレウス"/>
    <Monster ID="em002_01" Name="リオレウス亜種"/>
    [...]
```

### XML Special Characters

XML has some characters that should be replaced by something else in order for it to work.

| Character | Replaced by
|:---------:| :---------: 
|     &     | `&amp;`
|     <     | `&lt;`
|     >     |  `&gt`
|     "     | `&quot;`
|     '     |  `&apos;`


**Example:**
```xml
<!-- This might give an error -->
<Abnormality ID="MISC_073_00" Name="Attack Up & Def Up (S)"/>

<!-- This will work -->
<Abnormality ID="MISC_073_00" Name="Attack Up &amp; Def Up (S)"/>
```

### Errors

HunterPie will show an error message and show exactly what's wrong in your file if there were any problems.