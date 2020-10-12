# Debugger

<a href="?p=Plugins/HunterPie.Logger.md"><ns>namespace HunterPie.Logger</ns></a>

## Using the logging API

Plugins can log stuff to the console using the built-in logging API.

### Examples

```cs
using HunterPie.Logger;

// [...] Implements the class and IPlugin interface
public void Initialize(Game context) {
    this.Log("Hello World!");
    // >>> [ModuleName] Hello World!

    Debugger.Log("Hello World!");
    // >>> [LOG] Hello World!

    Debugger.Warn("Hello World!");
    // >>> Hello World!

    Debugger.Error("Hello World!");
    // >>> [ERROR] Hello World!
}
```

## Static Methods

### Debugger.Log(<Type>Object</Type> message)

This will log to the console normally with a white message.

### Debugger.Warn(<Type>Object</Type> message)

This will log to the console with a yellow message.

### Debugger.Error(<Type>Object</Type> message)

This will log to the console with a red message.

### Debugger.Module(<Type>Object</Type> message, <Type>String</Type> name = "MODULE")

This will log to the console with a green message. It's used by plugins and the plugin manager.

### <Interface>IPlugin</Interface> Extensions

#### this.Log(<Type>Object</Type> message)

This is a wrapper of `Debugger.Module`, works the same way except you don't need to set the name of the module manually.
