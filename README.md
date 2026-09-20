# KillingFloorLauncher

![Banner logo](https://i.imgur.com/7mNKO7X.png)

[Download for Windows](https://github.com/solenum/KillingFloorLauncher/releases/latest/download/KFLauncher-windows-x64.exe) · [Download for Linux](https://github.com/solenum/KillingFloorLauncher/releases/latest/download/KFLauncher-linux-x64)

Both are single self-contained files, no runtime to install: download, run it (on linux `chmod +x` first).  macOS has to be built from source for now, `dotnet build KFLauncher/KFLauncher.csproj` with the .NET 10 SDK.

# What is this?
This is a standalone-launcher for Killing Floor (1).

The goal of this launcher is to inject the games configuration files with some sane values to provide a better playing experience.  These improvements are (currently):
* **Uncapped frame-rate**
* **Improved net speed / performance**
* **Better mouse input**
* **Field of view** (the stock 85 is cropped rather than widened on a widescreen monitor, and the game resets it at trader time, so the launcher sets it in the config *and* chains it onto the forward bind)
* **Mouse locked to the game window** under proton, so the cursor cannot wander onto a second monitor mid-wave

It also has a server browser, so you can find a server and jump straight into it without going through the in-game menus.

Most of these improvements will be noticable right away.  The one caveat to this is the capped frame-rate in multiplayer, **which is uncapped as soon as you press any mouse button.**

This tool by default works with the steam version of Killing Floor, but should work with non-steam versions if you supply the game directory path manually.

### **Modifying settings in-game can overwrite this tools changes, be sure to change your settings in-game first and then re-launch the game via this tool!**

## How to use this tool
Download the latest release from the right, place the .exe on your desktop (or anywhere) and run it.

The tool will attempt to locate your games directory by crawling logical drives for steam libraries, and then crawling those steam libraries for the games installation.  If this fails, you can specify the directory manually.

Clicking the 'Launch Killing Floor' button will inject the config, and then start Killing Floor via the steam uri.

## Server browser
The 'Servers' tab lists every Killing Floor server steam knows about, with live player counts, map and ping.  Password protected servers are marked with a padlock.  Hitting 'Connect' (or double clicking the row) injects your config and then hands the server to steam (`steam://connect/ip:port`), which starts the game and joins it.

By default the launcher stays open once the game is on its way.  You can have it minimize or close instead under 'After launching' on the Launch tab, though note that minimizing stops the window being painted, and tiling window managers that keep it on screen anyway (bspwm, i3, ...) will show a stale window until you resize it, so leave it open or close it there.

If Killing Floor is already running, steam has no way in: it drops launch arguments for an app it is already running, and `steam://connect` asks the server for an app id that KF servers do not report.  So in that case the launcher copies the console command to your clipboard instead, press ~ in game and paste it.

The list itself comes from the steam web api, which needs a free api key tied to your steam account (valves old keyless master server no longer resolves).  Grab one from [steamcommunity.com/dev/apikey](https://steamcommunity.com/dev/apikey) and paste it into the launcher once, it is stored with the rest of your settings.  Player counts and ping are read straight from the servers themselves, no key involved.

If the game is lacking config files or they are malformed, the tool will attempt to generate a default one (based on the default configuration the game generates for new installations), it will then inject that config and launch the game.

## Why did you make this
I got tired of fixing my config files every time the game broke them.  Why not set them as read-only you ask?  Doing so will prevent other important changes from saving, such as skin selection, server favorites, input settings etc etc.

On-boarding friends into the world of Killing Floor is also a difficult task when the game runs terribly out of the box and requires some digging into configuration files and changing a bunch of values, which is off-putting to begin with.

## Future plans
This tool is very early-stage and only does some basic QoL changes.  While it is a simple operation, it saves the headache of dealing with the game overwriting your configuration changes constantly, and helps with on-boarding new people to the game without having them dig into configuration files to get the game playable.

Future plans include some of the following:
* ~~The ability to select what patches you want this tool to apply~~
* The ability to modify most/all in-game settings from the launcher
* ~~An embeded server-browser, with ability to connect from launcher~~ (favorites and a password prompt still to come)
* Joining a server without closing the game first, if steam ever grows a way in
* ~~Make less ugly~~
* Fix the many bugs that exist

### Screenshot

![Screenshot of the app](https://i.imgur.com/fnWiZNq.png)
