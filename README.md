# Vintage Story Yet Another Simple GUI (VSYASGUI)
Provides a basic GUI for viewing the console and managing players visually. Requires a mod to be installed on the server, which provides a simplified HTTP API for the GUI to interface with.

The GUI is written in C# and WPF (Windows Platform Foundation).

The mod is written in C# and utilizes the Vintage Story modding API.

# Installation
TODO

# Building
The solution is built with Visual Studio 2026 (not code) with WPF extensions added targeting .NET 8.

# Implementation Details

This project is split across three .sln: The Mod for the server (`VSYUASGUI-Mod`), the GUI (`VSYASGUI-WPF-App`), and the shared/common classes (`VSYASGUI-CommonLib`).

The Task async pattern is used throughout, with some classes being threadsafe (e.g. `CpuLoadCalc`). 

## Mod

The `HttpApi` class contains the majority of the logic.

The HTTP API runs asynchronously, with the `RunOnApiThread(...)` function being used on any VintageStory API related operations, as the API is not considered threadsafe. 