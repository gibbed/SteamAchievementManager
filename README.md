# Steam Achievement Manager

Steam Achievement Manager (SAM) is a lightweight, portable application used to manage achievements and statistics in the popular PC gaming platform Steam. This application requires the [Steam client](https://store.steampowered.com/about/), a Steam account and network access. Steam must be running and the user must be logged in.

This is the code for SAM. The closed-source version originally released in 2008, last major release in 2011, and last updated in 2013 (a hotfix).

The code is being made available so that those interested can do as they like with it.

There are some changes to the code since the last closed-source release:
- General code maintenance to bring it into a more modern state.
- Icons have been replaced with ones from the Fugue Icons set.
- Version has been bumped to 7.0.x.x to indicate the open-source release.

[Download latest release](https://github.com/gibbed/SteamAchievementManager/releases/latest).

[![Build status](https://ci.appveyor.com/api/projects/status/00vic6jliar6j0ol/branch/master?svg=true)](https://ci.appveyor.com/project/gibbed/steamachievementmanager/branch/master)

## Building

```powershell
dotnet build SAM.sln -c Release
```

The solution only has `x86` configurations (the game communicates with the
32-bit Steam client), so there is no `Any CPU` build.

## Tests

Unit tests live in `SAM.Game.Tests` (xunit) and cover the pure logic: schema
(KeyValue) parsing, statistic constraints, and the Steam error messages.

```powershell
dotnet test SAM.sln -c Release
```

## Attribution

Most (if not all) icons are from the [Fugue Icons](https://p.yusukekamiyamane.com/) set.
