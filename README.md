# KillingFloorLauncher

![Banner logo](https://i.imgur.com/7mNKO7X.png)

# What is this?
This is a standalone-launcher for Killing Floor (1).

The goal of this launcher is to inject the games configuration files with some sane values to provide a better playing experience.  These improvements are (currently):
* **Uncapped frame-rate**
* **Improved net speed / performance**
* **Better mouse input**

Most of these improvements will be noticable right away.  The one caveat to this is the capped frame-rate in multiplayer, **which is uncapped as soon as you press any mouse button.**

This tool by default works with the steam version of Killing Floor, but should work with non-steam versions if you supply the game directory path manually.

### **Modifying settings in-game can overwrite this tools changes, be sure to change your settings in-game first and then re-launch the game via this tool!**

## How to use this tool
Download the latest release from the right, place the .exe on your desktop (or anywhere) and run it.

The tool will attempt to locate your games directory by crawling logical drives for steam libraries, and then crawling those steam libraries for the games installation.  If this fails, you can specify the directory manually.

Clicking the 'Launch Killing Floor' button will inject the config, and then start Killing Floor via the steam uri.

If the game is lacking config files or they are malformed, the tool will attempt to generate a default one (based on the default configuration the game generates for new installations), it will then inject that config and launch the game.

## Why did you make this
I got tired of fixing my config files every time the game broke them.  Why not set them as read-only you ask?  Doing so will prevent other important changes from saving, such as skin selection, server favorites, input settings etc etc.

On-boarding friends into the world of Killing Floor is also a difficult task when the game runs terribly out of the box and requires some digging into configuration files and changing a bunch of values, which is off-putting to begin with.

## Future plans
This tool is very early-stage and only does some basic QoL changes.  While it is a simple operation, it saves the headache of dealing with the game overwriting your configuration changes constantly, and helps with on-boarding new people to the game without having them dig into configuration files to get the game playable.

Future plans include some of the following:
* The ability to select what patches you want this tool to apply
* The ability to modify most/all in-game settings from the launcher
* An embeded server-browser, with favorites, ability to connect from launcher, etc.

More to come!

Contact me on discord at solenum#8718

or on IRC at ##oodnet / libera
