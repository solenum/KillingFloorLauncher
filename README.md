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

![The server browser, with the details panel open on the selected server](docs/servers.png)

Most of these improvements will be noticable right away.  The one caveat to this is the capped frame-rate in multiplayer, **which is uncapped as soon as you press any mouse button.**

This tool by default works with the steam version of Killing Floor, but should work with non-steam versions if you supply the game directory path manually.

Windows and linux (including proton) are both supported, macos builds but is untested.

### **Modifying settings in-game can overwrite this tools changes, be sure to change your settings in-game first and then re-launch the game via this tool!**

## How to use this tool
Download the release for your platform, put it wherever you like and run it.

The tool finds your install by reading steams own library index (`libraryfolders.vdf`), so it picks up games on other drives and external disks as well, on windows, linux and macos.  If that fails, point it at the directory yourself on the Launch tab.

Clicking 'Launch Killing Floor' injects the config and starts the game through steam.

If the game is lacking config files or they are malformed, the tool will attempt to generate a default one (based on the default configuration the game generates for new installations), it will then inject that config and launch the game.

## Server browser
The 'Servers' tab lists every Killing Floor server steam knows about, with live player counts, map and ping.  Password protected servers are marked with a padlock.  Clicking a server opens a panel underneath with its address, a copy button and who is playing right now, names, scores and how long they have been in.  Hitting 'Connect' (or double clicking the row) injects your config and then starts the game on that server.

That goes through `steam://run/1250//<ip>:<port>/` rather than the obvious `steam://connect`, because steam works out which game a `connect` link belongs to by querying the server, and KF servers do not report an app id it accepts: you get "app id specified by server is invalid" on either the game port or the query port.  Naming the app in the url and letting unreal take the address as a launch argument sidesteps the lookup entirely.

By default the launcher stays open once the game is on its way.  You can have it minimize or close instead under 'After launching' on the Launch tab, though note that minimizing stops the window being painted, and tiling window managers that keep it on screen anyway (bspwm, i3, ...) will show a stale window until you resize it, so leave it open or close it there.

If Killing Floor is already running, steam has no way in: it drops launch arguments for an app it is already running, and `steam://connect` asks the server for an app id that KF servers do not report.  So in that case the launcher copies the console command to your clipboard instead, press ~ in game and paste it.

The list itself has to come from the steam web api, because valves old keyless master server no longer resolves.  There are two ways to feed it:

* **A list url.**  One machine polls steam with one api key and serves the result, and every launcher reads that.  Nobody else needs a key.  Releases ship pointed at `https://everparser.com/kf-servers.json`, so out of the box there is nothing to set up.
* **Your own api key.**  Grab one from [steamcommunity.com/dev/apikey](https://steamcommunity.com/dev/apikey) and paste it into the launcher once, it is stored with the rest of your settings.

Player counts, ping and the padlock are read straight from the servers themselves over A2S either way, no key involved.

## Hosting the list for everyone
`tools/` has everything: a script that asks steam for the list and writes it out, plus a systemd timer that runs it every 30 seconds.  The file it writes is steams own reply byte for byte, so the launcher reads a relay and the api directly with the same code.

```sh
sudo install -m 755 tools/kf-serverlist.sh /usr/local/bin/
sudo install -m 644 tools/kf-serverlist.service tools/kf-serverlist.timer /etc/systemd/system/

# the key lives here and nowhere else, never in git
printf 'STEAM_API_KEY=%s\n' "$YOUR_KEY" | sudo tee /etc/kf-serverlist.env
sudo chmod 600 /etc/kf-serverlist.env

sudo systemctl enable --now kf-serverlist.timer
```

It writes `/var/www/html/kf-servers.json` by default (`OUT=` in the env file changes that), ~160KB for the ~500 servers that are usually up.  Serve that path with whatever webserver you already run, then point launchers at `https://yourhost/kf-servers.json` in the 'Where the server list comes from' box.

To make that the default for everyone who downloads a release, set `DefaultListUrl` in `KFLauncher/Models/ServerBrowser.cs` before tagging.  The launcher then just works, with the api key box as a fallback if your host is down.

## Why did you make this
I got tired of fixing my config files every time the game broke them.  Why not set them as read-only you ask?  Doing so will prevent other important changes from saving, such as skin selection, server favorites, input settings etc etc.

On-boarding friends into the world of Killing Floor is also a difficult task when the game runs terribly out of the box and requires some digging into configuration files and changing a bunch of values, which is off-putting to begin with.

## Future plans
This tool is very early-stage and only does some basic QoL changes.  While it is a simple operation, it saves the headache of dealing with the game overwriting your configuration changes constantly, and helps with on-boarding new people to the game without having them dig into configuration files to get the game playable.

Future plans include some of the following:
* ~~The ability to select what patches you want this tool to apply~~
* The ability to modify most/all in-game settings from the launcher
* ~~An embeded server-browser, with favorites, ability to connect from launcher, etc~~ (favorites and a password prompt still to come)
* Joining a server without closing the game first, if steam ever grows a way in
* ~~Make less ugly~~
* Fix the many bugs that exist

## Building it yourself
Needs the .NET 10 SDK, nothing else:

```sh
dotnet build KFLauncher/KFLauncher.csproj
dotnet run --project KFLauncher/KFLauncher.csproj -- --selftest
```

`--selftest` checks the parts that are easy to break quietly: the A2S info and player parsers, the server list parser, the ini patcher, the wine registry patcher and the running game detection.  Pushing a `v*` tag builds the self contained binaries for both platforms and puts them on a release.

### Screenshot
The patches, and where the launcher gets its server list from:

![The launch tab](docs/launch.png)
