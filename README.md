<div align="center">
  
# [Steam-Achievement-Manager-Unlock-All](https://github.com/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All) <img src="https://github.com/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All/blob/b9551c46ea13749dea35c19897396fa462a444e7/.assets/Icons/Steam.svg"  width="3%" height="3%">

</div>

<div align="center">
  
---

This is a modernized fork of [gibbed's Steam Achievement Manager (SAM)](https://github.com/gibbed/SteamAchievementManager), with a fully automated achievement unlocking system. This fork introduces a new project, **SAM.PickerAuto**, which enables one-click, unattended unlocking of all achievements for all games in your Steam library. Not only that, but also counts with a multi-loop system, allowing you to repeat the unlock process multiple times with a single click, or even infinitely until you decide to stop it.
  
---

</div>

<div align="center">

![GitHub Code Size in Bytes](https://img.shields.io/github/languages/code-size/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Commits](https://img.shields.io/github/commit-activity/t/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All/master)
![GitHub Last Commit](https://img.shields.io/github/last-commit/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Forks](https://img.shields.io/github/forks/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Language Count](https://img.shields.io/github/languages/count/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub License](https://img.shields.io/github/license/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Stars](https://img.shields.io/github/stars/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Contributors](https://img.shields.io/github/contributors/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![GitHub Created At](https://img.shields.io/github/created-at/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All)
![wakatime](https://wakatime.com/badge/github/BrenoFariasdaSilva/Steam-Achievement-Manager-Unlock-All.svg)

</div>

<div align="center">
  
![RepoBeats Statistics](https://repobeats.axiom.co/api/embed/f833f8caf83561972596ad8ce7aee46c5e58a164.svg "Repobeats analytics image")

</div>

## Table of Contents
- [Steam-Achievement-Manager-Unlock-All ](#steam-achievement-manager-unlock-all-)
	- [Table of Contents](#table-of-contents)
	- [Introduction](#introduction)
	- [Requirements](#requirements)
	- [Features](#features)
	- [Directory Structure](#directory-structure)
	- [Building the Solution](#building-the-solution)
	- [Usage](#usage)
		- [Standard Mode](#standard-mode)
		- [Automated Mode (Unlock All)](#automated-mode-unlock-all)
			- [Exit Codes (from SAM.Game.exe in headless mode)](#exit-codes-from-samgameexe-in-headless-mode)
	- [Implementation Details](#implementation-details)
		- [UI Changes](#ui-changes)
		- [Automation Logic](#automation-logic)
		- [Headless Unlocking (SAM.Game)](#headless-unlocking-samgame)
		- [Inter-Process Communication](#inter-process-communication)
	- [File Changes Overview](#file-changes-overview)
	- [Development Setup (Visual Studio / Build Instructions)](#development-setup-visual-studio--build-instructions)
		- [1. Install Visual Studio (Required)](#1-install-visual-studio-required)
			- [Recommended Version](#recommended-version)
			- [Download](#download)
			- [Required Workload (MANDATORY)](#required-workload-mandatory)
		- [2. Required Components](#2-required-components)
			- [Individual Components](#individual-components)
		- [3. Opening the Project](#3-opening-the-project)
		- [4. Restore Dependencies](#4-restore-dependencies)
		- [5. Build the Solution](#5-build-the-solution)
			- [Build Entire Solution](#build-entire-solution)
			- [Keyboard Shortcut (Windows)](#keyboard-shortcut-windows)
			- [Cancel Build](#cancel-build)
			- [Compile Active Project Only](#compile-active-project-only)
			- [Run Code Analysis (Optional)](#run-code-analysis-optional)
		- [6. Output Location](#6-output-location)
		- [7. Running the Application](#7-running-the-application)
			- [Standard Mode](#standard-mode-1)
			- [Automated Mode](#automated-mode)
		- [8. Build Configuration Notes](#8-build-configuration-notes)
		- [9. Common Build Issues](#9-common-build-issues)
			- [Missing .NET Target](#missing-net-target)
			- [NuGet Errors](#nuget-errors)
			- [Build Fails on SAM.Game](#build-fails-on-samgame)
	- [Attribution](#attribution)
	- [Contributing](#contributing)
	- [Collaborators](#collaborators)
	- [License](#license)
		- [Apache License 2.0](#apache-license-20)

## Introduction

Detailed project description.
Steam Achievement Manager (SAM) is a powerful tool that allows users to manage, unlock, and edit their Steam achievements and statistics for any game. This fork of SAM introduces a new project called **SAM.PickerAuto**, which provides an automated achievement unlocking system. With just one click, users can unlock all achievements for all games in their Steam library without any manual intervention. The automation is handled through a new executable, `SAM.PickerAuto.exe`, which iterates through the games in the user's library and launches `SAM.Game.exe` in headless mode to unlock achievements silently in the background. This enhancement significantly streamlines the process of unlocking achievements, making it more efficient

## Requirements

- .NET Framework 4.x (matching the target version of the solution)
- Steam client installed and logged in
- Visual Studio (for development and building)
- Optional: Git for version control
- Optional: NuGet package manager for dependency management

## Features

- **Original SAM Functionality**: Manage, unlock, and edit Steam achievements and statistics for any game, with a portable WinForms UI.
- **SAM.PickerAuto**: A new executable (`SAM.PickerAuto.exe`) that automates the achievement unlocking process for all games in your list, requiring only a single click.
- **Unlock All Button**: Added to the main toolbar, this button triggers a fully automated, asynchronous process that:
  - Iterates through every visible game in the list (respects current filters/search).
  - Launches `SAM.Game.exe <AppID> --unlock-all` in headless mode for each AppID, passing a `--unlock-all` argument.
  - Waits for the achievement list to load, selects all achievements, commits them to Steam, and closes the process before moving to the next game.
  - Runs in the background without freezing the UI, with progress and results shown in the status bar.
- **Headless Mode for SAM.Game**: `SAM.Game.exe` now supports a `--unlock-all` argument for silent, non-interactive unlocking (no UI, no dialogs, exit code signals result).
- **Robust Inter-Process Automation**: All automation is handled via process invocation and public API, with no UI automation or message-pumping hacks.
- **Multi-Loop Unlock All**: The Unlock All automation supports a loop count. Enter a number in the "Loops" box to repeat the filtered game list, or use "-1" for infinite looping.
- **Live progress and ETA**: The bottom-left status bar continuously shows `Loop 1 of 3 - ETA 02h:14m:37s - Processing 25/150 Counter-Strike 2`. Game numbers are one-based and reset each loop; the total respects the active filter. The ETA uses an adaptive moving average of actual completed-game durations and covers the current game plus every remaining finite-loop attempt. It starts as `ETA Calculating...` until a measurement exists, updates once per second while a game is running, and is `ETA N/A (Infinite)` for infinite loops.

## Directory Structure

```
SAM.API/           # Steamworks API interop and callbacks
SAM.Game/          # Achievement manager for a single game (now supports headless automation)
SAM.Picker/        # Standard game picker UI
SAM.PickerAuto/    # Automated picker with Unlock All button and automation logic
SAM.sln            # Solution file (includes all four projects)
```

## Building the Solution

1. **Project Integration**
	- `SAM.PickerAuto` is a direct copy of `SAM.Picker`, renamed and registered in `SAM.sln` with a unique GUID.
	- `SAM.PickerAuto.csproj` is updated with `<AssemblyName>SAM.PickerAuto</AssemblyName>` so it builds to `SAM.PickerAuto.exe`.
	- All four projects are included in the solution and build together. No project references are required between Picker/PickerAuto.

2. **.sln and .csproj Changes**
	- `SAM.sln` contains entries for all four projects, including the new `SAM.PickerAuto` with its own build configs.
	- `SAM.PickerAuto.csproj` is renamed and updated as above.

## Usage

### Standard Mode

Run `SAM.Picker.exe` to manually select and unlock achievements for individual games, as in the original SAM.

### Automated Mode (Unlock All)

1. Run `SAM.PickerAuto.exe`.
2. Use the search/filter as desired to limit the games to process.
3. Click the **Unlock All** button (located to the right of the game filtering button in the toolbar).
4. The app will:
	- Disable the Unlock All and Refresh buttons.
	- For each game in the filtered list:
	  - Launch `SAM.Game.exe <AppID> --unlock-all` in headless mode (no window).
	  - Wait for the process to finish (fully async, UI remains responsive).
	  - Update the status bar with progress and exit code.
	- When complete, re-enable the buttons and show a summary dialog.

#### Exit Codes (from SAM.Game.exe in headless mode)

- `0`: Success (all achievements unlocked)
- `1`: Steam/stats error
- `2`: Client initialization failure
- `3`: Steamworks DLL not found

## Implementation Details

### UI Changes

- In `SAM.PickerAuto/GamePicker.Designer.cs`, a new `ToolStripButton` labeled **Unlock All** is added immediately after the game filtering button. It is wired to an async handler (`OnUnlockAll`).

### Automation Logic

- In `SAM.PickerAuto/GamePicker.cs`, the `OnUnlockAll` handler:
  - Snapshots the filtered game list.
  - Disables relevant UI buttons.
  - For each game, launches `SAM.Game.exe <AppID> --unlock-all` with `UseShellExecute=false` and `CreateNoWindow=true`.
  - Awaits process exit asynchronously.
  - Updates the status bar with progress.
  - Re-enables UI and shows a summary when done.

### Headless Unlocking (SAM.Game)

- In `SAM.Game/Program.cs`, a new code path is added:
  - If the second argument is `--unlock-all`, the app runs in headless mode (no forms, no dialogs).
  - Calls into a new static class `AutoUnlocker` to perform the unlock logic:
	 - Waits for Steam stats to load.
	 - Loads all achievement IDs.
	 - Unlocks all eligible achievements and commits them.
	 - Exits with the appropriate code.

### Inter-Process Communication

- No IPC or message-passing is required. All automation is handled by launching `SAM.Game.exe` with arguments and monitoring its exit code.

## File Changes Overview

**1. SAM.sln**
	- Added project entry for `SAM.PickerAuto` with unique GUID and build configs.

**2. SAM.PickerAuto.csproj**
	- Renamed from `SAM.Picker.csproj`.
	- Set `<AssemblyName>SAM.PickerAuto</AssemblyName>`.

**3. GamePicker.Designer.cs (SAM.PickerAuto)**
	- Added `Unlock All` button to the toolbar after the filter button.

**4. GamePicker.cs (SAM.PickerAuto)**
	- Added `using System.Threading.Tasks;`.
	- Implemented `OnUnlockAll` async handler as described above.

**5. Program.cs (SAM.Game)**
	- Added headless mode: parses `--unlock-all` argument, runs unlock logic, exits with code, no UI.

**6. AutoUnlocker.cs (SAM.Game)**
	- New static class for headless automation logic: waits for stats, unlocks all, commits, returns status.

## Development Setup (Visual Studio / Build Instructions)

This section explains how to install the required tools, open the project, and build the solution to generate the executables, including `SAM.PickerAuto.exe`.

---

### 1. Install Visual Studio (Required)

#### Recommended Version
- Visual Studio Community (free)

#### Download
- https://visualstudio.microsoft.com/vs/community/

#### Required Workload (MANDATORY)
During installation, select:

- **.NET Desktop Development**

This is required for:
- WinForms (UI projects)
- C# build tools
- MSBuild integration

---

### 2. Required Components

Inside Visual Studio Installer, ensure the following are selected:

#### Individual Components
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
- MSBuild
- Windows 10/11 SDK
- C# and Visual Basic Roslyn compiler
- Git for Windows (optional but recommended)

---

### 3. Opening the Project

1. Open Visual Studio
2. Click:

```
File → Open → Project/Solution
```

3. Select:

```
SAM.sln
```

4. Wait for:
- NuGet restore (automatic)
- Project indexing
- Solution load completion

---

### 4. Restore Dependencies

If required:

```
Build → Restore NuGet Packages
```

or right-click solution:

```
Restore NuGet Packages
```

---

### 5. Build the Solution

#### Build Entire Solution

```
Build → Build Solution
```

#### Keyboard Shortcut (Windows)
```
Ctrl + Shift + B
```

This will compile:
- SAM.API
- SAM.Game
- SAM.Picker
- SAM.PickerAuto

Windows 10 command-line build (Release/x86):

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "SAM.sln" /restore /m /p:Configuration=Release /p:Platform=x86
```

---

#### Cancel Build
```
Ctrl + Break
```

---

#### Compile Active Project Only
```
Ctrl + F7
```

---

#### Run Code Analysis (Optional)
```
Alt + F11
```

---

### 6. Output Location

After a Release/x86 build, the complete runtime distribution is generated in:

```
upload/
```

Depending on configuration:

- `SAM.Picker.exe`
- `SAM.PickerAuto.exe`
- `SAM.Game.exe`

For manual distribution, copy the complete `upload/` directory to `Releases/Steam-Achievement-Manager-Unlock-All-8.1.0/` and archive that directory as `Releases/Steam-Achievement-Manager-Unlock-All-8.1.0.zip`. Keep all generated runtime files together, including `SAM.API.dll` and any dependency DLLs.

---

### 7. Running the Application

#### Standard Mode
Run:
```
SAM.Picker.exe
```

#### Automated Mode
Run:
```
SAM.PickerAuto.exe
```

Then click:
```
Unlock All
```

---

### 8. Build Configuration Notes

Ensure configuration is set correctly in Visual Studio:

- Debug → for development
- Release → for production build

Use top toolbar:

```
Solution Configuration: Release
Solution Platform: Any CPU
```

---

### 9. Common Build Issues

#### Missing .NET Target
- Install required .NET Framework via Visual Studio Installer

#### NuGet Errors
- Restore packages manually

#### Build Fails on SAM.Game
- Ensure Steamworks dependencies are present in project references

## Attribution

Original project by [gibbed](https://github.com/gibbed/SteamAchievementManager). Most icons are from the [Fugue Icons](https://p.yusukekamiyamane.com/) set.

This fork adds full automation and headless achievement unlocking. See code comments and commit history for detailed implementation notes.

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**. If you have suggestions for improving the code, your insights will be highly welcome.
In order to contribute to this project, please follow the guidelines below or read the [CONTRIBUTING.md](CONTRIBUTING.md) file for more details on how to contribute to this project, as it contains information about the commit standards and the entire pull request process.
Please follow these guidelines to make your contributions smooth and effective:

1. **Set Up Your Environment**: Ensure you've followed the setup instructions in the [Setup](#setup) section to prepare your development environment.

2. **Make Your Changes**:
	- **Create a Branch**: `git checkout -b feature/YourFeatureName`
	- **Implement Your Changes**: Make sure to test your changes thoroughly.
	- **Commit Your Changes**: Use clear commit messages, for example:
	  - For new features: `git commit -m "FEAT: Add some AmazingFeature"`
	  - For bug fixes: `git commit -m "FIX: Resolve Issue #123"`
	  - For documentation: `git commit -m "DOCS: Update README with new instructions"`
	  - For refactorings: `git commit -m "REFACTOR: Enhance component for better aspect"`
	  - For snapshots: `git commit -m "SNAPSHOT: Temporary commit to save the current state for later reference"`
	- See more about crafting commit messages in the [CONTRIBUTING.md](CONTRIBUTING.md) file.

3. **Submit Your Contribution**:
	- **Push Your Changes**: `git push origin feature/YourFeatureName`
	- **Open a Pull Request (PR)**: Navigate to the repository on GitHub and open a PR with a detailed description of your changes.

4. **Stay Engaged**: Respond to any feedback from the project maintainers and make necessary adjustments to your PR.

5. **Celebrate**: Once your PR is merged, celebrate your contribution to the project!

## Collaborators

We thank the following people who contributed to this project:

<table>
  <tr>
	 <td align="center">
		<a href="#" title="defina o titulo do link">
		  <img src="https://github.com/BrenoFariasdaSilva.png" width="100px;" alt="My Profile Picture"/><br>
		  <sub>
			 <b>Breno Farias da Silva</b>
		  </sub>
		</a>
	 </td>
  </tr>
</table>

## License

### Apache License 2.0

This project is licensed under the [Apache License 2.0](LICENSE). This license permits use, modification, distribution, and sublicense of the code for both private and commercial purposes, provided that the original copyright notice and a disclaimer of warranty are included in all copies or substantial portions of the software. It also requires a clear attribution back to the original author(s) of the repository. For more details, see the [LICENSE](LICENSE) file in this repository.
