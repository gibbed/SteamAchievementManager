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

## WinUI 3 version (SAM.App)

`SAM.App` is an optional, modern Windows 11 front end for the same Steam code. It sits next to the
classic WinForms tools and does not replace them: `SAM.sln` (SAM.API, SAM.Picker, SAM.Game) builds
as before.

- **One program, same model:** `SAM.exe` without arguments opens the library, `SAM.exe <app id>`
  opens a game in its own process, like SAM.Picker and SAM.Game.
- **Library:** tiles with capsule images or a list view, search, filter by type, open by app ID,
  context menu (store page, show in Steam, copy app ID). The gear next to "Tiles | List" chooses what
  a tile shows (name, app ID, achievements, sets, medal, profile visibility); a fresh install shows
  only cover and name. Games are grouped into Completed / In Progress / Not Started, each section
  collapsible, or shown as one area. Sorting by name, app ID, progress, sets, category or visibility,
  ascending or descending. The game list and images are cached in
  `%LOCALAPPDATA%\SteamAchievementManager`, with the last copy of the game list as fallback when
  gib.me is unreachable.
- **List view:** app ID, icon, name, profile visibility, medal, achievements and sets with progress
  bars, category. Column headers work like in Excel: click to sort (∧/∨), filter with search and
  checkboxes, show, hide and reorder columns (arrows or drag); order and visibility are saved.
- **Game window:** unlock, lock and edit statistics as before, with a header image, progress, search
  and filters. Pending changes are marked and counted, a summary is shown before saving, protected
  achievements are skipped by the bulk actions, and closing or reloading warns about unsaved changes.
  Resetting is a single dialog with an explicit confirmation.
- **Achievement sets** (Steam beta since October 2026): achievements are grouped by set in Steam's
  order, each set collapsible with its medal and progress; archived achievements are marked and,
  like on Steam, count neither for the set nor for the game, and DLC sets only count when the DLC is
  owned. The schema's `groups` (name, `dlcappid`, order) and the per-achievement `groupid` /
  `archived` are read from Steam's cached schema.
- **Medals like on Steam:** large blue for 100 %, blue for one base game set, the blue double medal
  for several, the purple double medal for DLC sets, grey for anything open, with separate counts
  for base game and DLC sets. Shine and sparkles are animated (a pre-rendered image strip moved by
  the compositor, so it costs the UI thread nothing) and follow the Windows "Animation effects"
  setting.
- **Achievement lists** (switch "Library | Achievement lists" in the title bar, same window): sorts
  every owned game into the categories of the
  [Steam Achievement Statistics](https://steamcommunity.com/sharedfiles/filedetails/?id=3128095897)
  collections (All, Done, Not 100 % Anymore, Started, Can't Finish), each for all games, games
  visible on the profile and Profile Features Limited games, plus Sets (game sets, DLC sets,
  completed, in progress, not started) and Untouched (all, visible, limited). The category list has
  collapsible groups with a medal per category; the game list has the same sortable, filterable and
  reorderable columns (app ID, game, first and last unlock, medal, progress), a date range filter,
  manually added app IDs and "Can't Finish" with a reason. **Export** writes Steam guide text in
  Steam's markup (tables, headings, numbered lists), split alphabetically so no section exceeds the
  character limit. All data comes from Steam's own files on this PC
  (`userdata\<account>\config\librarycache\achievement_progress.json`, `appcache\stats` for unlock
  times and the schema, `appcache\librarycache` for icons), so no game is launched and nothing is
  written to Steam.
- **Look:** Mica, custom title bar, light and dark theme following Windows. Keyboard: `Ctrl+F` search,
  `F5` reload, `Ctrl+S` save.
- **Languages:** 47 translations covering 72 language/country variants, right-to-left layout for
  Arabic and Hebrew, picked from Windows or chosen in the library. Texts live in
  `SAM.App/Strings/*.json`; `python tools/strings.py` checks every file against `en.json` and
  regenerates `Localization/Strings.g.cs`.

It is a 64-bit app (.NET 10, Windows App SDK 1.8, unpackaged and self-contained). `SAM.API.x64`
compiles the same `SAM.API` sources for x64, and the schema reader and stat definitions are
linked from `SAM.Game` rather than copied. Building needs the .NET 10 SDK only:

```
dotnet build SAM.WinUI.sln -c Release -p:Platform=x64
dotnet publish SAM.App/SAM.App.csproj -c Release -r win-x64 -p:Platform=x64 -o upload/SAM.App
```

The medal images in `SAM.App/Assets/Medals` are generated from the animated medal SVGs:
`python tools/make-medals.py <folder>` writes the static versions (the Windows SVG renderer supports
neither CSS nor masks), `python tools/make-medal-frames.py <folder>` renders the animation with
headless Microsoft Edge into `Assets/Medals/Animated` and `Helpers/MedalAnimations.g.cs`.

Requires Windows 10 version 2004 or later. Debug builds understand `--demo <app id>` as the first
argument (game window with the real schema from Steam's cache and simulated states, without talking
to Steam), `--medals` (all medals in several sizes) and `--language <locale>` for checking
translations.

## Attribution

Most (if not all) icons are from the [Fugue Icons](https://p.yusukekamiyamane.com/) set.
