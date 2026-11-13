# Local PC Steam Achievement Manager

A fork of [Steam Achievement Manager](https://github.com/gibbed/SteamAchievementManager) focused on discovering **locally installed** Steam games without downloading the central `https://gib.me/sam/games.xml` catalogue.

## Local game discovery (experimental)

- Steam must be running and logged into the account whose achievements you want to manage.
- The picker reads Steam's installation location from the Windows registry (with a conventional Program Files fallback) and discovers additional library folders from `steamapps/libraryfolders.vdf`.
- It scans `steamapps/appmanifest_*.acf` in each library and uses the local AppIDs and names to populate the picker.
- SAM's existing `IsSubscribedApp` check filters applications against the **currently logged-in** Steam client account. Installing a game on the PC does not necessarily establish access for the current account.
- **Only installed games** appear in this initial version. Owned but uninstalled games are outside its current scope. Manual **Add Game** still works.
- The main picker no longer needs the `gib.me` catalogue, although game/achievement artwork may still involve network requests.
- If Steam is installed in an unusual location and cannot be found, set the `SAM_STEAM_PATH` environment variable to the Steam **installation directory** (the directory containing `steamapps`), then restart the picker.
- An unreadable manifest or disconnected secondary Steam library is skipped. Missing the root Steam installation produces an explanatory error.

### Build

This project still targets .NET Framework 4.8 and x86. After installing the .NET Framework 4.8 Developer Pack and a compatible .NET SDK, run:

```powershell
dotnet restore .\SAM.sln
dotnet build .\SAM.sln -c Debug -p:Platform=x86
.\bin\SAM.Picker.exe
```

This feature is a work in progress. The scanner and its integration still need to be compiled and tested on a Windows Steam installation.

## Upstream attribution

Steam Achievement Manager (SAM) is a lightweight, portable application used to manage achievements and statistics in the popular PC gaming platform Steam. This application requires the [Steam client](https://store.steampowered.com/about/), a Steam account and network access. Steam must be running and the user must be logged in.

This is the code for SAM. The closed-source version originally released in 2008, last major release in 2011, and last updated in 2013 (a hotfix).

The code is being made available so that those interested can do as they like with it.

There are some changes to the code since the last closed-source release:
- General code maintenance to bring it into a more modern state.
- Icons have been replaced with ones from the Fugue Icons set.
- Version has been bumped to 7.0.x.x to indicate the open-source release.

[Download latest release](https://github.com/gibbed/SteamAchievementManager/releases/latest).

[![Build status](https://ci.appveyor.com/api/projects/status/00vic6jliar6j0ol/branch/master?svg=true)](https://ci.appveyor.com/project/gibbed/steamachievementmanager/branch/master)

## Attribution

Most (if not all) icons are from the [Fugue Icons](https://p.yusukekamiyamane.com/) set.
