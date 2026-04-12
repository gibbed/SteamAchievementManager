# Modernization & Discovery Overhaul (v7.0.42 - .NET 8 WPF)

## 📌 Overview
This Pull Request bumps the Steam Achievement Manager (SAM) to **v7.0.42**, bringing a massive modernization effort by transitioning the Picker from legacy WinForms to a modern, high-fidelity WPF interface. Alongside the UI revamp, it completely fixes critical issues with game discovery that occurred due to deprecated Steam API endpoints, restoring the classic SAM exact library detection.

## ✨ Key Features & UI Changes
* **WPF Interface Rewrite**: Replaced the outdated WinForms `GamePicker.cs` window with a fully responsive, dark-themed `MainWindow.xaml` WPF interface. 
* **Real-time Search Filter**: Introduced a responsive text-based search/filter bar exactly at the top of the grid to allow users to quickly find massive libraries instantaneously.
* **.NET 8 Upgrade**: Cleaned up the `.csproj` file to utilize `.NET 8`, taking advantage of better runtime performance while strictly maintaining `x86` output (essential for `steamclient.dll` interop). 
* **Version Update**: Correctly reflects the new target version natively in the application window as `v7.0.42`.

## 🐛 Bug Fixes & Engine Restorations
* **Restored Full Library Discovery**: 
  The official `ISteamApps/GetAppList` web endpoints are heavily rate-limited and deprecated. 
  Re-implemented the battle-tested, classic Gibbed discovery engine: This application now properly downloads the master `games.xml` list again and directly queries the local client API using `SteamApps008.IsSubscribedApp(...)`. This restores 100% discovery precision and eliminates generic "App 1000" fallbacks.
* **Fixed Missing Image Capsules**: 
  Images reliant on generic static store CDN locations (`capsule_184x69.jpg`) failed frequently because retired/delisted games return HTTP 404s. Reverted the image resolver to safely request cached metadata properties (`small_capsule`, `logo`) from the local client API (`SteamApps001`). Now, all graphics load gracefully and asynchronously.
* **Process Start Pathing Fixed**: 
  Resolved an issue where users clicking on a game from `SAM.Picker.exe` encountered an error because it couldn't locate `SAM.Game.exe` in local debug/dev environments. The startup info now traverses the solution properly to gracefully link with the `\bin\` execution directory.

## 🛠️ Technical Details
* Added `GameInfoViewModel.cs` for clean XAML databinding (INotifyPropertyChanged).
* Added global `DispatcherUnhandledException` handlers to capture and diagnose rendering/API execution exceptions silently rather than crash-to-desktop.
* Maintained compatibility with `steam_api.dll` and original Gibbed API hooks.
