# Discord-Rich-Presence-for-TIDAL

Lightweight app to display your currently playing song on TIDAL and its timecode to other Discord users through Rich Presence. The indication will vanish shortly after you pause the playback.

This project is neither created nor supported by Discord or TIDAL.

Feel free to make suggestions or report bugs.


## Screenshots
![status extended](https://user-images.githubusercontent.com/14842772/106336759-e2368000-628f-11eb-9c22-46472bb85e63.png)

![status server member list](https://user-images.githubusercontent.com/14842772/106336758-e19de980-628f-11eb-82c1-b29274d18d8e.png)

![status friends page](https://user-images.githubusercontent.com/14842772/106336757-e1055300-628f-11eb-8f2a-2a08577b9385.png)


## Instructions

This application uses .NET 5.0, if not installed, it can be downloaded from [here](https://dotnet.microsoft.com/download).

You can find the latest release [here](https://github.com/Kaufi-Jonas/Discord-Rich-Presence-for-TIDAL/releases) (only windows supported). The program shows an icon in the taskbar that allows you to temporarily disable it.

If you'd like this app to be started with Windows, press Win + R and enter `shell:startup`. Put a shortcut of the app's .exe into the folder that just opened.


## Some Notes regarding Implementation

My goal was to create a clean, lightweight and easily extendible app.

Currently, the window title is used to retrieve the playing song's title and artist(s). Those information are then sent to Discord using [Lachee/discord-rpc-csharp](https://github.com/Lachee/discord-rpc-csharp). I took some inspiration from [KNIF/TIDAL-RPC](https://github.com/KNIF/TIDAL-RPC) and am using his Discord App Id. Due to the way Discord implemented RPC, I don't see any option to display the currently playing track's album cover.

In order to retrieve the currently playing song's timecode, I am reading the variable in TIDAL's memory which stores this information. This ensures, that the timecode shown in Discord is always in sync.

How do I get the address of this variable? I am using a similar process to what you would do in Cheat Engine: Do a first scan when the song starts and then rescan using an approximation of the real value (assumption: playback wasn't interrupted) until there is only one address left. To be able to do that programatically and in general read other processes' memory, I use [Squalr/Squalr.Engine.Scanning](https://github.com/Squalr/Squalr).

Since the address has to be found first, there is a slight delay before the timecode is shown in Discord.
