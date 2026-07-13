# Steam Achievement Manager

Steam Achievement Manager (SAM) is a lightweight, portable application used to manage achievements and statistics in the popular PC gaming platform Steam. This application requires the [Steam client](https://store.steampowered.com/about/), a Steam account and network access. Steam must be running and the user must be logged in.

## Batch idle launcher (this fork)

The game picker supports selecting multiple games. Its **Start selected (idle)** button starts every selected app without opening a SAM manager window for each one. Use the drop-down beside the button to open normal manager windows or stop only selected idle sessions. A persistent green/red status indicator shows the number of idle sessions currently running, beside an explicit button to stop them all.

An idle session is still one `SAM.Game.exe` process per Steam app ID, because Steam assigns the app ID at process startup. It has no window or taskbar entry, leaving the picker as the only visible control window. Stopping sessions, or closing the picker, signals them to release their Steam connection and exit cleanly, with a forced shutdown only as a fallback.

For a release build, run `dotnet build SAM.sln --configuration Release -p:Platform=x86`; the executables are written to `upload/`.

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
