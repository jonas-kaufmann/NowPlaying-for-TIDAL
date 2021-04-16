# NowPlaying-for-TIDAL
Lightweight app that aims to imitate Spotify's Now Playing feature in Discord using Game Activity. This project is neither created nor supported by Discord or TIDAL.

Feel free to make suggestions or report bugs.


## Screenshots
![Discord Profile Small](https://user-images.githubusercontent.com/14842772/115017631-198ff280-9eb7-11eb-8be9-7906653fecc6.png)

![Discord Profile Full](https://user-images.githubusercontent.com/14842772/114887949-2524e000-9e09-11eb-8028-0cb69545815e.png)


## Installation Instructions

This application only runs under Windows and requires [.NET 5.0](https://dotnet.microsoft.com/download).

You can find an installer under [releases](https://github.com/Kaufi-Jonas/NowPlaying-for-TIDAL/releases). The installer will create a StartMenu and Autostart entry. The program shows an icon in the taskbar that allows you to temporarily disable it.

This application runs in your task bar, if you wish to temporarily disable or close it, you can find those options in the tray icon's menu.


## Instructions for Album Artwork

This feature is disabled by default because Discord has a limit of 300 asset slots. Due to that, everyone has to create his own Discord Application.

Head over to the [Discord Developer Portal's Application Page](https://discord.com/developers/applications) and create an application. The name you choose will later be visible in your status like so: `Playing <application name>`. I chose `Music on TIDAL` which will then show up as `Playing Music on TIDAL`.

Copy the Application ID from the General Information page and insert it into this program's config file. You can find the config file by right clicking on the tray menu icon.

The last thing we need is an authentication token, so that Discord knows it's you, the owner of the application, who is uploading the album artworks. To attain this token, again, make sure that you are on [Discord Developer Portal's Application Page](https://discord.com/developers/applications). Open the developer tools of your webbrowser. You can typically accomplish this by pressing `Ctrl+Shift+I`. Switch to the Network or Network analysis tab. Reload the web page. A lot of request will now appear on your screen. In these search for any request containing a request header called `Authorization`. When found, copy it's value to this program's config. The final result will look something like this:
```
{
  "DiscordAppId": "633539288744553243",
  "DiscordMfaToken": "BHNoagHMUUxSg63Rx5NuJHcU.dpPUny.owfVRsszxtzAHmTztKaccQ2jm9S"
}
```


## Some Notes regarding Implementation

My goal was to create a clean, lightweight and easily extensible app.

Currently, the window title is used to retrieve the playing song's title and artist(s). Those information are then sent to Discord using [Lachee/discord-rpc-csharp](https://github.com/Lachee/discord-rpc-csharp).

In order to retrieve the currently playing song's timecode, I am reading the variable in TIDAL's memory which stores this information. This ensures, that the timecode shown in Discord is always in sync.

How do we get the address of this variable? I am using a similar process to what you would do in Cheat Engine: Do a first scan when the song starts and then rescan using an approximation of the real value (assumption: playback wasn't interrupted) until there is only one address left. To be able to do that programatically and in general read other processes' memory, I use [Squalr/Squalr.Engine.Scanning](https://github.com/Squalr/Squalr).

Since the address has to be found first, there is a slight delay before the timecode is shown in Discord.

Using the earlier attained song's title and artist(s), we can now query TIDAL's API for information about the track. The authentication token needed to interact with TIDAL's API is read from the TIDAL desktop app's log files. However, this token can expire. If you experience that the `Play on TIDAL` button is no longer visible, simply restart the desktop app and the token will be renewed.

We have now attained an URL pointing to the track and an URL pointing to its album artwork. We can use latter to upload the artwork as an asset to our Discord Application. Due to caching, it takes roughly 10 mins before an uploaded asset is availabe.

If the space is full, we need to delete an asset first. This asset is chosen using the LRU strategy (Least Recently Used). However, for LRU to work, we need to track when an artwork has been used the last time. This information is stored in `DiscordAssets.json`.


Big Thanks to [maybeclean](https://github.com/maybeclean) for teaching me how to manage Discord Application Assets and providing a base implementation of the album artwok feature. Details can be found in [this issue](https://github.com/Kaufi-Jonas/Discord-Rich-Presence-for-TIDAL/issues/2).
