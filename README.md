# DivaModManager
Diva Mod Manager is a tool that allows gamers to download, install, and manage mods for Hatsune Miku: Project Diva Mega Mix+. This manager relies on [DivaModLoader](https://github.com/blueskythlikesclouds/DivaModLoader) to load mods and is basically a GUI frontend with extra QoL features. This is also a fork of my other project, [Unverum](https://github.com/TekkaGB/Unverum), so if it looks familiar, that's why.

## Getting Started
### Prerequisites
When you first open the exe, you'll get a message saying to install .NET 6 Runtime if you don't already have it installed. Please do so if that's the case.

### Setup
After the exe actually launches, you'll first be prompted if you would want to launch using the Steam shortcut url or directly through the game exe itself. Generally it doesn't matter but the choice is there. Afterwards, it will automatically setup for you. If it can't find the path to your game, then it will open a prompt to select the game's exe. After getting the path, Diva Mod Manager will then install the latest release of DivaModLoader to your game. If you already have DivaModLoader installed along with some mods, you will see all of your mods auto-populate the grid (all enabled and in alphanumeric folder order). If you for some reason need to setup again, just click the Setup button again.

tl;dr You shouldn't have to do anything for setup since it does everything for you.

## Features
### Installing Mods
Before you can manage and load some mods, you have to install some.

There are 3 methods of doing this:
1. Using the built in Mod Browser tab to download mods found on GameBanana
2. Using 1-click install buttons (once they're added in) from browsing mods directly from the GameBanana website
3. Downloading mods from other sources such as the Discord server and dragging and dropping them onto the mod grid for easy install.

### Managing Mods
Managing mods is as simple as dragging the order of the rows to prioritize the top and enabling which mods you want in the build. Once you have your desired loadout, click Save and loadout will be ready to be used on the next launch of the game. Click the Launch button to quickly launch the game after saving the loadout.

### Auto Updates
Diva Mod Manager also supports auto updates for mods downloaded from GameBanana. Click the Check for Updates button for Diva Mod Manager to check if any are available for the currently selected game. It will also check if there are updates for both DivaModLoader and Diva Mod Manager. These updates are also checked when launching the manager.

### Sorting
You can sort your mods alphabetically by clicking the Name header and by which ones are enabled by clicking the Enabled header.

### Configuring Mods
You can edit each mod's config.toml by right-clicking the row and clicking Configure Mod. A basic text editor will appear for you to edit, save, and close.

### Creating Mods
You can click Create Mod to allow you to autogenerate a folder with a config.toml with the fields that you enter. There's also a field to add a preview image that will show instead of the GameBanana preview image on the right side of the manager. (The manager attempts to load the first file that it sees with the name preview. Most extensions are supported, even .gif!) You can then drop any folders and files you wish inside the folder that it opens.

### Loadouts
You can also generate different loadouts by clicking Edit Loadouts. This button gives you the option to add a new loadout with your desired name, rename your current loadout, or delete your current loadout.

## FAQ
### Is this safe? My antivirus is getting set off.
Yes this application is safe. Antivirus tends to trigger false alarms, especially due to it needing to be connected to the internet in order to be compatible with 1-click installations and updating. You can check out the source code for yourself if you're suspicious of anything as well.

### Why won’t DivaModManager open?
I made it so only one instance is running at a time so if it’s already running, the app won’t open. Check to see if you can end the process in task manager or even restart your pc if you don’t know how to do that. 

### Why doesn't DivaModManager have permissions to copy over files?
Try running as administrator or checking to see if any antivirus is preventing the application from operating on files.
