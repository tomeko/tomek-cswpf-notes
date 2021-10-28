# tomek-cswpf-notes

## Overview

A "cliff notes" for various WPF/.NET Core 3.1 application implementation and concepts. Of course the [official WPF-Samples](https://github.com/microsoft/WPF-Samples) covers a far greater range but it's missing a few things that aren't really easy to find otherwise (e.g. CpBlk memop implementation). So it's really just for my own reference and a bit better organized than a ton of random gists.

I have lots more to add so I expect this to be updated regularly. Hope you find something useful!

## Content Summary

| File                      | Description                                                  |
| ------------------------- | ------------------------------------------------------------ |
| `App.xaml/cs`             | Overrides `OnStartup`, calls `SplashScreen`, simple logger, DataContext controller |
| `converters.cs`           | Various converters used to modify data types from XAML       |
| `Demo.xaml/cs`            | Interface to demo various utilities, data binding, etc.      |
| `DemoSplash.xaml/cs`      | Splash screen with progress bar                              |
| `DemoUserControl.xaml/cs` | DependencyProperty example with a user control               |
| `dialogs.cs`              | Custom message dialog entirely in code-behind                |
| `extensions.cs`           | Various extension methods (see Extensions)                   |
| `fileops.cs`              | Recursive directory copy, Zip from file list, Open file/path dialogs |
| `memops.cs`               | Implementation of the `CpBlk` instruction, and regular RTL memcpy |
| `miscutil.cs`             | Miscellaneous utility functions (see Miscellaneous)          |
| `networkutil.cs`          | Application network usage monitor                            |
| `serialization.cs`        | XML, struct (with endianness adjust), Base64                 |

