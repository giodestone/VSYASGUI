# Vintage Story Yet Another Server GUI (VSYASGUI)

![server overview page of GUI](https://github.com/giodestone/VSYASGUI/blob/main/Images/Image2.png)

Provides a basic GUI for viewing the server console and managing players visually. Requires a mod to be installed on the server, which provides a simplified HTTP API for the GUI to interface with.

Key features:
* View server resource usage
* View console history
* Issue commands
* View connected players
* Kick, ban, unban recently connected players

Please report any bugs or enhancements to [issues tab](https://github.com/giodestone/VSYASGUI/issues).

# Installation
This mod is provided in two parts: the GUI and mod that must be added to the server. This is a server-side mod.

The mod may be download through [the Vintage Story Mod DB](https://mods.vintagestory.at/show/mod/46221#tab-description).

0. Install .NET 10 Desktop runtime - already required for Vintage Story >=1.22.0 (Windows: https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
1. Download the mod `vsyasguimod-x.x.x.zip` from [the releases page](https://github.com/giodestone/VSYASGUI/releases)
2. Place into your dedicated server's mod folder.
3. Download the GUI `vsyasgui-x.x.x.zip` from [the releases page](https://github.com/giodestone/VSYASGUI/releases)
4. Extract the GUI `vsyasgui-x.x.x.zip`
5. Open the exe
6. Optionally, change the `BindURL` in the `data/ModConfig/` to allow access outside of the machine (`http://*:8181/`), to allow access on LAN.

**Note: the API does not use HTTPS.** Therefore, please **do not expose the endpoint port (by default 8181) to the internet**. The API key can be extracted from your usage of the GUI using a man in the middle attack and used to execute commands on the server. By default, you should not be at risk, as the endpoint is bound to localhost (the local machine running the server).

Note: The mod may be used in a non-dedicated server. However, the API does not work when paused.

# Configuration

Configuration files provided may be used to change certain behaviours.

## Mod
The config is stored in `vsyasgui-mod-config.json` in the Vintage Story Mod Config Directory. On Windows, that is: `%AppData%\Roaming\VintagestoryData\ModConfig`

* bindurl: the URL the HTTP Client will bind to. Security warning: exposing the endpoint API to the internet is not recommended.
* apikey: the key that should be used to perform authenticated principles.
* maxconsoleentriescache: the maximum number of log entries (does not correspond to lines) that will be cached for retrieval by the GUI.
* cpuusagepolltimems: every how many milliseconds to poll the CPU usage

## GUI
The recently connected endpoints/api keys can be cleared by selecting Application > Clear Configuration.

The config is stored in `settings.json`.

* currentapikey: most recently selected api key.
* apikeyhistory: previously selected api keys (including the most recently selected).
* currentendpoint: most recently selected endpoint.
* endpointaddresses: previously selected endpoints.
* serverpollintervalms: every how many milliseconds to poll the server for things like statistics
* maxfailedconnectionrequests: how many times a the connection request fail before considering the server offline.



# Gallery

*Login page*
![login page](https://github.com/giodestone/VSYASGUI/blob/main/Images/Image1.png)
*Overview page*
![server overview showing console and resources used by process](https://github.com/giodestone/VSYASGUI/blob/main/Images/Image2.png)
*Player overview page - player details*
![player overview page with a player selected along with join dates, groups etc.](https://github.com/giodestone/VSYASGUI/blob/main/Images/Image3.png)
*Player overview page - player options*
![player overview page showing a players details focusing in on the ban unban and kick buttons](https://github.com/giodestone/VSYASGUI/blob/main/Images/Image4.png)
*Connecting page*
![connecting page with cancel button](https://github.com/giodestone/VSYASGUI/blob/main/Images/Image5.png)
*Connection failed page*
![connection failed page with reason](https://github.com/giodestone/VSYASGUI/blob/main/Images/Image6.png)



# Building

Follow [this guide to setup your development environment](https://wiki.vintagestory.at/Modding:Setting_up_your_Development_Environment#Setup_the_Environment) for Vintage Story Code Mod development.

The solution is built with Visual Studio 2026 (not code) with WPF extensions added.

Vintage Story should be installed.

New versions of both the GUI may be created by right clicking on `VSYUASGUI-Mod` or `VSYASGUI-WPF-App` projects, and selecting 'Publish'.



# Implementation Details

The goal of this project was to create an easy to use GUI which allows for management of the Vintage Story Server. The secondary goals were learn about creating code mods for the Vintage Story API, and learning WPF (Windows Presentation Foundation).

The GUI is written in C# and WPF (Windows Platform Foundation) and uses MVVM (Model, View, View Model/Model View Controller).

The mod is written in C# and utilizes the Vintage Story modding API.

This project is split across three .sln: The Mod for the server (`VSYUASGUI-Mod`), the GUI (`VSYASGUI-WPF-App`), and the shared/common classes (`VSYASGUI-CommonLib`).

The Task async pattern is used throughout, with some classes being threadsafe (e.g. `CpuLoadCalc`).

Overall, I think the implementation is okay for this scope. I wish I had moved more of the request/responses to the common library as they are very heavily intertwined. There are also a number of performance enhancements to be made: namely reducing the number of player update data if it is up to date, and changing the log cache to use a ring buffer.

The following sections provide an implementation overview, rationale, and any specific improvements. A reflection section is provided. 


## GUI

The GUI is implemented using multiple pages which try to have a 'connection' flow, whereas the API is stateless.

### Design

In this project, Figma was used to prototype the GUI. Notably, the Figma templates use the material design (which is unavailable in WPF without a lot of work).

[Google's Material 3 Foundations](https://m3.material.io/foundations) were used as guiding principles. However, as a lot of them are specific to mobile-first design using the Material 3 GUI framework these had to be adapted. Due to scope, accessibility using screen readers was not tested, nor accommodations provided.

The interface was separated into two states: not connected and connected. The connected state was further subdivided into overview/console and player management.

#### User Profile

The user profile which the GUI was designed for is:

* Age: Adult
* Culture/Language/Geography: Anglosphere
* Disabilities: No significant disabilities affecting cognition, vision, or interaction
* Education: High school education
* Culture reference: Knowledge of Vintage Story, sufficiently computer-savvy to understand the benefits of a dedicated server
* Needs: To understand the status of the server, manage players for a small home server, maybe run some commands.

The user profile affected the addition of error messages, available statistics, and wording of certain parameters.


#### Application Flows

A successful first-time flow was identified as:
1. User installs mod on server (drag + drop).
2. User launches server.
3. User launches GUI.
4. User has good default options which allow for connection.
5. User is able to execute commands
6. User is able to view player details
7. User is able to kick/ban players.

A failed application flow may look like:
1. User does not launch server
2. User launches GUI
3. User attempts to connect
4. User is informed of nature of connection issue, and resolution steps.

An exceptional circumstance may be:
1. User launches server
2. User launches GUI
3. User connects to server
4. Server crashes
5. User relaunches server
6. GUI reflects that server has been re-launched

### Code Overview

Error tolerance was implemented using the `Error` enum, where various errors can be represented by a value (where acceptable). This was used to introduce error tolerance, similar to the way Godot Engine does, and also allows for the later read-back of the error.

The views are implemented using `Page` system, as to make state management easier. This removes the need for custom view management code (which would be required if `UserControl` was used). I am aware this is better for web-based applications.

Each view tries to maximize the use of the XML, and to have as little logic in each corresponding view class.

The `ApiConnection` model provides ways to interact with the API. The View Model `ConnectionPresenter` provides the majority of properties and functions (connection checking etc.) for the connection (`ConnectPage`, `ConnectingPage`, `ConnectionFailedPage`), console, and player (`Server`) views. All blocking connection-related blocking tasks are implemented using the async task model.

The API uses templating to provide reusable functions to interpret the incoming requests, namely `ApiConnection.RequestApiInfo<T>(...)` and its derivatives. It allows for a mostly generic way of processing incoming requests.

The `Config` model provides an interface to the saved user config, which is provided using the `ConfigPresenter`. It is primarily used in the `ConnectPage` to allow for API/endpoint connection history and saving this to disk.


### Improvements
The `ConnectionPresenter` class could be split up into further classes to divorce responsibilities, namely: connecting/connection check, server heartbeat/statistics, and server operations.

Any `InstanceAwareResponseBase` class must is not automatically checked at the point of deserialization. This makes adding any future classes a bit more error prone than I would like.

Similarly to the Mod/API, I think moving this logic into the CommonLib project would help couple the request/response.

Icons would make the GUI prettier. However, the SVG library refused to correctly work and render.

Ban/kick reason and duration should be customisable.



## Mod/HTTP API

The API is stateless, as to respect RESTful principles. Only an API key is required to correctly communicate. POST is used exclusively at the moment, as some of the requests need to send over a body, and no robust method of checking GET requests has been implemented.

Error tolerance was built in, as I don't think a HTTP API should crash the server.

The `HttpApi` class contains the majority of the logic for responding to the API. Each endpoint is implemented as a task.

The HTTP API runs asynchronously, with the `RunOnApiThread(...)` function being used on any VintageStory API related operations, as the API is not considered threadsafe. 

### API Endpoints

The API key must be provided in the header under the `ApiKey` key.

| Endpoint    | Method | Request Class              | Response Class             | Purpose                                     |
|-------------|--------|----------------------------|----------------------------|---------------------------------------------|
| /           | POST   | `ConnectionRequest`        | `ConnectionCheckResponse`  | For connection checking.                    |
| /command    | POST   | `CommandRequest`           | `ConsoleCommandResponse`   | Send command to be executed.                |
| /console    | POST   | `ConsoleRequest`           | `ConsoleEntriesResponse`   | Retrieve console entries.                   |
| /players    | POST   | `PlayerOverviewRequest`    | `PlayerOverviewResponse`   | Retrieve overview of all connected players. |
| /statistics | POST   | `ServerStatisticsRequest`  | `ServerStatisticsResponse` | Retrieve server statistics.                 |

### Improvements

The `HttpApi` class could be split up further, as currently any new API endpoint requires a new function. This could be split out into its own class, and made tighter with the request classes.

Adding encryption, or better yet HTTPS support. Currently, the API does not use HTTPS as using self-signed certificates with `HttpListener` would require installation. Bypassing this with `X509Store` did not work. This has been left on the `https-failed-attempt`. Implementing this would either require encrypting the request body, or using a different provider (other than `HttpListener`) which allows for easier use of self-signed SSL certificates in-code (and do not require the user to install the certificate manually using system tools). 

The `LogCache` class could be upgraded with a ring buffer, as currently it uses a `Queue` to store a maximum number of messages.



## Common Lib

The Common Library provides classes and information that is shared across the mod and GUI to make de/serialization easier.

The `RequestBase` abstract class provides the basis for all requests. At least, it contains the endpoint and API key. This is mostly designed as a storage class.

The `ResponseBase` abstract class provides the base for all requests. The `InstanceAwareResponseBase` provides additional field for the the instance GUId (used to clear the console, if the server is restarted during the 'connected' page). This is designed as a storage class.

The `PlayerOverview` class is a wrapper for player details the vintage story API provides. This wrapper class was created to reduce dependency on the Vintage Story API outside of the mod, and to include some more interesting details. Similarly, this is designed as a storage class.

### Improvements

Like in previous sections a tighter integration of requests and responses is likely desirable. Within the current framework this may be implemented by using an `ApiCommunication` class, which contains both the request and response along with inherited de/serialization logic. The creation of the request could be provided in some way

The `PlayerOverview` class could be expanded to take into account offline players. The Vintage Story API has limitations, where the API only provides information to connected players from the last restart. This player information is available as a part of a JSON on the server, however was not implemented as this project was already growing arms and legs.



## Reflection on Suitability

The provided mod and application fulfil the goal of providing an easy to use interface for a locally running server.

The GUI allows access to the server console/log, ability to view player information visually, and perform basic player management. An explicit disconnection screen would be beneficial. Moving to [Avolina](https://avaloniaui.net/) to allow cross-platform support would also help, as Vintage Story is multi-platform.

The API provides stateless access to core features of the server. The API could be made using more than just POST method. HTTPS implementation would provide further security.
