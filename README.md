# Discord-Rich-Presence-for-TIDAL

Lightweight app to display your currently playing song on TIDAL to other Discord users through Rich Presence. The indication will vanish shortly after you pause the playback.

This project is neither created nor supported by Discord or TIDAL.

Feel free to make suggestions or report bugs.


## Screenshots
![Screenshot status extended](https://user-images.githubusercontent.com/14842772/105393019-28e70300-5c1c-11eb-9093-c866c29b3c5e.png)

![Screenshot status](https://user-images.githubusercontent.com/14842772/105393020-297f9980-5c1c-11eb-9130-efa9562e3c4b.png)


## Instructions

This application uses .NET 5.0, if not installed, it can be downloaded from [here](https://dotnet.microsoft.com/download).

You can find the latest release [here](https://github.com/Kaufi-Jonas/Discord-Rich-Presence-for-TIDAL/releases) (only windows supported). The program shows an icon in the taskbar that allows you to temporarily disable it.

If you'd like this app to be started with Windows, press Win + R and enter `shell:startup`. Put a shortcut of the app's .exe into the folder that just opened.


## Some Notes regarding Implementation

My goal was to create a clean, lightweight and easily extendible app.

Currently, the window title is used to retrieve the playing song's title and artist(s). Those information are then sent to Discord using [Lachee/discord-rpc-csharp](https://github.com/Lachee/discord-rpc-csharp). I took some inspiration from [KNIF/TIDAL-RPC](https://github.com/KNIF/TIDAL-RPC) and am using his Discord App Id. Due to the way Discord implemented RPC, I don't see any option to display the currently playing track's album cover.

