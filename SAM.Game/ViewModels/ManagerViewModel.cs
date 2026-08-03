/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAM.Core;
using SAM.Core.Stats;
using APITypes = SAM.API.Types;

namespace SAM.Game.ViewModels
{
    public sealed partial class ManagerViewModel : ObservableObject, IDisposable
    {
        private const int MaximumConcurrentIconDownloads = 4;

        private readonly long _GameId;
        private readonly API.Client _Client;
        private readonly UserStats _UserStats;
        private readonly API.Callbacks.UserStatsReceived _UserStatsReceivedCallback;
        private readonly DispatcherTimer _CallbackTimer;

        private readonly List<StatDefinition> _StatDefinitions = new();
        private readonly List<AchievementDefinition> _AchievementDefinitions = new();
        private readonly SemaphoreSlim _IconLimit = new(MaximumConcurrentIconDownloads);

        private CancellationTokenSource _IconCancellation = new();
        private int _PendingIcons;

        public ObservableCollection<AchievementViewModel> Achievements { get; } = new();

        public ObservableCollection<StatViewModel> Statistics { get; } = new();

        /// <summary>
        /// Raised for anything the user needs to see. The window turns these into
        /// dialogs, which keeps this type free of window references.
        /// </summary>
        public event Action<string> ErrorRaised;

        /// <summary>
        /// Asks the window a yes or no question. Reset needs three of them.
        /// </summary>
        public event Func<string, Task<bool>> ConfirmRequested;

        public ManagerViewModel(long gameId, API.Client client)
        {
            this._GameId = gameId;
            this._Client = client ?? throw new ArgumentNullException(nameof(client));
            this._UserStats = new UserStats(client);

            GameCatalog catalog = new(client);
            string name = catalog.GetName((uint)gameId);
            this.Title = string.IsNullOrEmpty(name) == true
                ? $"Steam Achievement Manager 7.0 | {gameId.ToString(CultureInfo.InvariantCulture)}"
                : $"Steam Achievement Manager 7.0 | {name}";

            this._UserStatsReceivedCallback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
            this._UserStatsReceivedCallback.OnRun += this.OnUserStatsReceived;

            this._CallbackTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Background,
                (_, _) => this._Client.RunCallbacks(false));
            this._CallbackTimer.Start();
        }

        #region Bound state
        [ObservableProperty]
        private string _Title;

        [ObservableProperty]
        private string _GameStatus = "";

        [ObservableProperty]
        private string _DownloadStatus;

        [ObservableProperty]
        private bool _IsInputEnabled = true;

        [ObservableProperty]
        private bool _IsStatsEditingEnabled;

        [ObservableProperty]
        private string _SearchText = "";

        [ObservableProperty]
        private bool _ShowLockedOnly;

        [ObservableProperty]
        private bool _ShowUnlockedOnly;

        partial void OnSearchTextChanged(string value) => this.RebuildAchievements();

        partial void OnShowLockedOnlyChanged(bool value)
        {
            if (value == true && this.ShowUnlockedOnly == true)
            {
                this.ShowUnlockedOnly = false;
            }
            this.RebuildAchievements();
        }

        partial void OnShowUnlockedOnlyChanged(bool value)
        {
            if (value == true && this.ShowLockedOnly == true)
            {
                this.ShowLockedOnly = false;
            }
            this.RebuildAchievements();
        }
        #endregion

        #region Refresh
        [RelayCommand]
        public void Refresh()
        {
            this.Achievements.Clear();
            this.Statistics.Clear();
            this.CancelPendingIcons();

            // This still triggers the UserStatsReceived callback in addition to the call
            // result, so there is no need to implement call results for the time being.
            if (this._UserStats.Request() == API.CallHandle.Invalid)
            {
                this.ErrorRaised?.Invoke("Failed to request stats.");
                return;
            }

            this.GameStatus = "Retrieving stat information...";
            this.IsInputEnabled = false;
        }

        private void OnUserStatsReceived(APITypes.UserStatsReceived param)
        {
            // Callbacks arrive for every app this process hears about, not just ours.
            // The low half of a CGameID is the application id.
            if ((uint)param.GameId != (uint)this._GameId)
            {
                return;
            }

            if (param.Result != 1)
            {
                this.GameStatus = $"Error while retrieving stats: {UserStats.TranslateError(param.Result)}";
                this.IsInputEnabled = true;
                return;
            }

            UserGameStatsSchema schema = UserGameStatsSchema.Load(
                this._GameId,
                this._UserStats.GetCurrentGameLanguage());
            if (schema == null)
            {
                this.GameStatus = "Failed to load schema.";
                this.IsInputEnabled = true;
                return;
            }

            this._AchievementDefinitions.Clear();
            this._AchievementDefinitions.AddRange(schema.Achievements);
            this._StatDefinitions.Clear();
            this._StatDefinitions.AddRange(schema.Stats);

            try
            {
                this.RebuildAchievements();
            }
            catch (Exception e)
            {
                this.GameStatus = "Error when handling achievements retrieval.";
                this.IsInputEnabled = true;
                this.ErrorRaised?.Invoke($"Error when handling achievements retrieval:\n{e}");
                return;
            }

            try
            {
                this.Statistics.Clear();
                foreach (StatInfo stat in this._UserStats.ReadStatistics(this._StatDefinitions))
                {
                    StatViewModel model = new(stat);
                    model.EditRejected += this.OnStatEditRejected;
                    this.Statistics.Add(model);
                }
            }
            catch (Exception e)
            {
                this.GameStatus = "Error when handling stats retrieval.";
                this.IsInputEnabled = true;
                this.ErrorRaised?.Invoke($"Error when handling stats retrieval:\n{e}");
                return;
            }

            this.GameStatus =
                $"Retrieved {this.Achievements.Count} achievements and {this.Statistics.Count} statistics.";
            this.IsInputEnabled = true;
        }

        private void OnStatEditRejected(StatViewModel stat, string message)
        {
            this.ErrorRaised?.Invoke(message);
        }

        private void OnProtectedChangeRejected(AchievementViewModel achievement)
        {
            this.ErrorRaised?.Invoke(
                "Sorry, but this is a protected achievement and cannot be managed with Steam Achievement Manager.");
        }
        #endregion

        #region Achievement list
        private void RebuildAchievements()
        {
            if (this._AchievementDefinitions.Count == 0)
            {
                return;
            }

            string search = this.SearchText.Length > 0 ? this.SearchText : null;
            bool wantLocked = this.ShowLockedOnly;
            bool wantUnlocked = this.ShowUnlockedOnly;

            this.CancelPendingIcons();
            this.Achievements.Clear();

            foreach (AchievementDefinition def in this._AchievementDefinitions)
            {
                if (string.IsNullOrEmpty(def.Id) == true)
                {
                    continue;
                }

                if (this._UserStats.TryReadAchievement(def.Id, out bool isAchieved, out uint unlockTime) == false)
                {
                    continue;
                }

                bool wanted = (wantLocked == false && wantUnlocked == false) || isAchieved switch
                {
                    true => wantUnlocked,
                    false => wantLocked,
                };
                if (wanted == false)
                {
                    continue;
                }

                if (search != null &&
                    def.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0 &&
                    def.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                AchievementInfo info = new()
                {
                    Id = def.Id,
                    IsAchieved = isAchieved,
                    WasAchieved = isAchieved,
                    UnlockTime = isAchieved == true && unlockTime > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(unlockTime).LocalDateTime
                        : null,
                    IconNormal = string.IsNullOrEmpty(def.IconNormal) ? null : def.IconNormal,
                    IconLocked = string.IsNullOrEmpty(def.IconLocked) ? def.IconNormal : def.IconLocked,
                    Permission = def.Permission,
                    Name = def.Name,
                    Description = def.Description,
                };

                AchievementViewModel model = new(info);
                model.ProtectedChangeRejected += this.OnProtectedChangeRejected;
                this.Achievements.Add(model);
            }

            this.StartIconDownloads();
        }

        [RelayCommand]
        public void LockAll() => this.SetAll(false);

        [RelayCommand]
        public void UnlockAll() => this.SetAll(true);

        [RelayCommand]
        public void InvertAll()
        {
            foreach (AchievementViewModel achievement in this.Achievements)
            {
                achievement.SetAchievedQuietly(achievement.IsAchieved == false);
            }
        }

        private void SetAll(bool achieved)
        {
            foreach (AchievementViewModel achievement in this.Achievements)
            {
                achievement.SetAchievedQuietly(achieved);
            }
        }
        #endregion

        #region Icons
        private void CancelPendingIcons()
        {
            this._IconCancellation.Cancel();
            this._IconCancellation.Dispose();
            this._IconCancellation = new();
            this._PendingIcons = 0;
            this.DownloadStatus = null;
        }

        private void StartIconDownloads()
        {
            CancellationToken cancellationToken = this._IconCancellation.Token;

            foreach (AchievementViewModel achievement in this.Achievements)
            {
                string icon = achievement.IsAchieved == true
                    ? achievement.Info.IconNormal
                    : achievement.Info.IconLocked;
                if (string.IsNullOrEmpty(icon) == true)
                {
                    continue;
                }

                this._PendingIcons++;
                _ = this.DownloadIconAsync(achievement, icon, cancellationToken);
            }

            this.UpdateDownloadStatus();
        }

        private async Task DownloadIconAsync(
            AchievementViewModel achievement,
            string icon,
            CancellationToken cancellationToken)
        {
            try
            {
                await this._IconLimit.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    Uri uri = new(string.Create(
                        CultureInfo.InvariantCulture,
                        $"https://cdn.steamstatic.com/steamcommunity/public/images/apps/{this._GameId}/{icon}"));
                    byte[] data = await Downloader.GetBytesAsync(uri, cancellationToken).ConfigureAwait(false);

                    using MemoryStream stream = new(data, false);
                    Bitmap bitmap = new(stream);

                    await Dispatcher.UIThread.InvokeAsync(() => achievement.Icon = bitmap);
                }
                finally
                {
                    this._IconLimit.Release();
                }
            }
            catch (Exception)
            {
                // A missing icon just leaves the placeholder in place.
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this._PendingIcons--;
                    this.UpdateDownloadStatus();
                });
            }
        }

        private void UpdateDownloadStatus()
        {
            this.DownloadStatus = this._PendingIcons > 0
                ? $"Downloading {this._PendingIcons} icons..."
                : null;
        }
        #endregion

        #region Store and reset
        [RelayCommand]
        public void Store()
        {
            List<AchievementInfo> changed = this.Achievements
                .Select(a => a.Info)
                .Where(i => i.IsModified == true)
                .ToList();

            string failedId = this._UserStats.StoreAchievements(changed);
            if (failedId != null)
            {
                this.ErrorRaised?.Invoke(
                    $"An error occurred while setting the state for {failedId}, aborting store.");
                this.Refresh();
                return;
            }

            List<StatInfo> modified = this.Statistics
                .Select(s => s.Info)
                .Where(s => s.IsModified == true)
                .ToList();

            failedId = this._UserStats.StoreStatistics(modified);
            if (failedId != null)
            {
                this.ErrorRaised?.Invoke(
                    $"An error occurred while setting the value for {failedId}, aborting store.");
                this.Refresh();
                return;
            }

            if (this._UserStats.Commit() == false)
            {
                this.ErrorRaised?.Invoke("An error occurred while storing, aborting.");
                this.Refresh();
                return;
            }

            this.ErrorRaised?.Invoke($"Stored {changed.Count} achievements and {modified.Count} statistics.");
            this.Refresh();
        }

        [RelayCommand]
        public async Task ResetAsync()
        {
            if (this.ConfirmRequested == null)
            {
                return;
            }

            if (await this.ConfirmRequested("Are you absolutely sure you want to reset stats?") == false)
            {
                return;
            }

            bool achievementsToo = await this.ConfirmRequested("Do you want to reset achievements too?");

            if (await this.ConfirmRequested("Really really sure?") == false)
            {
                return;
            }

            if (this._UserStats.ResetAll(achievementsToo) == false)
            {
                this.ErrorRaised?.Invoke("Failed to reset stats.");
                return;
            }

            this.Refresh();
        }
        #endregion

        public void Dispose()
        {
            this._CallbackTimer.Stop();
            this._IconCancellation.Cancel();
            this._IconCancellation.Dispose();
            this._IconLimit.Dispose();
        }
    }
}
