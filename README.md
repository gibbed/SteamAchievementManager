# Steam Achievement Manager

Steam Achievement Manager (SAM) is a lightweight, portable application used to manage achievements and statistics in the popular PC gaming platform Steam. This application requires the [Steam client](https://store.steampowered.com/about/), a Steam account and network access. Steam must be running and the user must be logged in.

SAM runs on Windows, Linux and macOS.

This is the code for SAM. The closed-source version originally released in 2008, last major release in 2011, and last updated in 2013 (a hotfix).

The code is being made available so that those interested can do as they like with it.

There are some changes to the code since the last closed-source release:
- General code maintenance to bring it into a more modern state.
- Icons have been replaced with ones from the Fugue Icons set.
- Version has been bumped to 7.0.x.x to indicate the open-source release.
- The user interface is now [Avalonia](https://avaloniaui.net/) rather than Windows Forms, and the application targets .NET 10, so it runs on Linux and macOS as well as Windows.

[Download latest release](https://github.com/gibbed/SteamAchievementManager/releases/latest).

## Running

Releases are self-contained, so there is nothing to install. Unpack the archive and run `SAM.Picker`. Keep `SAM.Picker` and `SAM.Game` in the same directory, because the picker launches the achievement manager from alongside itself.

SAM finds Steam automatically. On Windows it reads the install path from the registry. On Linux it looks at `$XDG_DATA_HOME/Steam`, `~/.steam/root`, `~/.steam/steam`, `~/.local/share/Steam` and the Flatpak location under `~/.var/app/com.valvesoftware.Steam`. On macOS it looks in `~/Library/Application Support/Steam`. Set `SAM_STEAM_PATH` to point it somewhere else.

### macOS

Steam for macOS is an x86_64 application running under Rosetta, and an x86_64 `steamclient.dylib` cannot be loaded into an arm64 process. The macOS release is therefore x86_64 on both Intel and Apple Silicon.

## Building

Requires the [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or later.

```
dotnet build SAM.sln
```

That writes both applications to `bin/`. To produce a self-contained build for one platform:

```
dotnet publish SAM.Picker/SAM.Picker.csproj -c Release -r linux-x64 --self-contained -o publish
dotnet publish SAM.Game/SAM.Game.csproj     -c Release -r linux-x64 --self-contained -o publish
```

Both projects publish into the same directory on purpose. Substitute `win-x64` or `osx-x64` for other platforms.

## Attribution

Most (if not all) icons are from the [Fugue Icons](https://p.yusukekamiyamane.com/) set.
