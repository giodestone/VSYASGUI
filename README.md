# Vintage Story Yet Another Simple GUI (VSYASGUI)
Provides a basic GUI for viewing the console and managing players visually. Requires a mod to be installed on the server, which provides a simplified HTTP API for the GUI to interface with.

The goal of this project was to create an easy to use GUI which allows for management of the Vintage Story Server. The secondary goals were learn about creating code mods for the Vintage Story API, and learning WPF (Windows Presentation Foundation).

The GUI is written in C# and WPF (Windows Platform Foundation) and uses MVVM (Model, View, View Model/Model View Controller).

The mod is written in C# and utilizes the Vintage Story modding API.

# Installation
TODO






# Building
The solution is built with Visual Studio 2026 (not code) with WPF extensions added targeting .NET 8.






# Implementation Details

This project is split across three .sln: The Mod for the server (`VSYUASGUI-Mod`), the GUI (`VSYASGUI-WPF-App`), and the shared/common classes (`VSYASGUI-CommonLib`).

The Task async pattern is used throughout, with some classes being threadsafe (e.g. `CpuLoadCalc`).

Overall, I think the implementation is okay for this scope. I wish I had moved more of the request/responses to the common library as they are very heavily intertwined. There are also a number of performance enhancements to be made: namely reducing the number of player update data if it is up to date, and changing the log cache to use a ring buffer.

The following sections provide an implementation overview, rationale, and any specific improvements. A reflection section is provided. 


## GUI

The GUI is implemented using multiple pages which try to have a 'connection' flow, whereas the API is stateless.

### Design

In this project, Figma was used to prototype the GUI. Notably, the Figma templates use the material design (which is unavailable in WPF without a lot of work). The [images are available here](TODO).

[Google's Material 3 Foundations](https://m3.material.io/foundations) were used as guiding principles. However, as a lot of them are specific to mobile-first design using the Material 3 GUI framework these had to be adapted. Due to scope, accessibility using screen readers was not tested, nor accommodations provided.

The interface was separated into two states: not connected and connected. The connected state was further subdivided into overview/console and player management.

#### User Profile

The user profile that was used to design the current features:

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

Error tolerance was implemented using the `Error` enum, where various errors can be represented by a value (where acceptable).

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

The API is stateless, as to respect RESTful principles. Only an API key is required to correctly communicate.

Error tolerance was built in, as I don't think a HTTP API should crash the server.

The `HttpApi` class contains the majority of the logic for responding to the API. Each endpoint is implemented as a task.

The HTTP API runs asynchronously, with the `RunOnApiThread(...)` function being used on any VintageStory API related operations, as the API is not considered threadsafe. 

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

The provided GUI works sufficiently as to allow the exposure of some views. The catgo