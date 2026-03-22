# Vintage Story Yet Another Simple GUI (VSYASGUI)
Provides a basic GUI for viewing the console and managing players visually. Requires a mod to be installed on the server, which provides a simplified HTTP API for the GUI to interface with.

The GUI is written in C# and WPF (Windows Platform Foundation) and uses MVVM (Model, View, View Model/Model View Controller).

The mod is written in C# and utilizes the Vintage Story modding API.

# Installation
TODO

# Building
The solution is built with Visual Studio 2026 (not code) with WPF extensions added targeting .NET 8.

# Implementation Details

The goal of this project was to create a simple code mod for the Vintage Story API, learn WPF, and make a simple GUI for managing a server.

This project is split across three .sln: The Mod for the server (`VSYUASGUI-Mod`), the GUI (`VSYASGUI-WPF-App`), and the shared/common classes (`VSYASGUI-CommonLib`).

The Task async pattern is used throughout, with some classes being threadsafe (e.g. `CpuLoadCalc`).

Overall, I think the implementation is okay for this scope. I wish I had moved more of the request/responses to the common library as they are very heavily intertwined. There are also a number of performance enhancements to be made: namely reducing the number of player update data if it is up to date, and changing the log cache to use a ring buffer.

## Mod/HTTP API

The `HttpApi` class contains the majority of the logic for responding to the API.

The HTTP API runs asynchronously, with the `RunOnApiThread(...)` function being used on any VintageStory API related operations, as the API is not considered threadsafe. 

