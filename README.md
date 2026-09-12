# Steam Achievement Manager

**Steam Achievement Manager (SAM)** is a powerful, lightweight, open-source utility designed to manage achievements and in-game statistics on the Steam gaming platform.

---

## ✨ Features

- 🎮 **Modern Steam Dark UI**: Fully redesigned user interface built with WPF adhering to Steam's signature Dark Theme.
- 🌍 **30-Language Support**: Complete localization with human-readable JSON files in the `languages/` folder.
- 👁️ **Hidden Achievement Marking & Filtering**: Clear visual badges for hidden achievements and flexible filtering options.
- 🏆 **Live Achievement & Stat Counter**: Real-time counter showing normal vs. hidden achievements.
- ⚡ **High Performance & Stability**: Thread-safe Steam IPC synchronization, instant library scanning, and asynchronous capsule rendering.
- 🛠️ **Automated CI/CD**: Built-in GitHub Actions workflow and `build.bat` script for easy building from source.

---

## 🚀 Requirements

1. **Steam Client**: Steam must be running and logged in with your account.
2. **.NET Framework 4.8**: Included in Windows 10 & Windows 11 by default.

---

## 📦 Building from Source

To compile the application locally:

1. Clone the repository:
   ```bash
   git clone https://github.com/gibbed/SteamAchievementManager.git
   ```
2. Run the automated build script:
   ```cmd
   build.bat
   ```
3. Compiled binaries will be located in the `upload/` folder.
