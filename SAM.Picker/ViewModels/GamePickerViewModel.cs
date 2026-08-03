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
using APITypes = SAM.API.Types;

namespace SAM.Picker.ViewModels
{
    public sealed partial class GamePickerViewModel : ObservableObject, IDisposable
    {
        private const int MaximumConcurrentLogoDownloads = 4;

        private readonly API.Client _Client;
        private readonly GameCatalog _Catalog;
        private readonly API.Callbacks.AppDataChanged _AppDataChangedCallback;
        private readonly DispatcherTimer _CallbackTimer;

        private readonly Dictionary<uint, GameViewModel> _AllGames = new();
        private readonly HashSet<string> _LogosAttempted = new();
        private readonly SemaphoreSlim _LogoLimit = new(MaximumConcurrentLogoDownloads);

        private CancellationTokenSource _LogoCancellation = new();
        private int _PendingLogos;

        public ObservableCollection<GameViewModel> Games { get; } = new();

        /// <summary>
        /// Raised when something goes wrong that the user should see. The window turns
        /// this into a dialog, keeping this type free of any window references.
        /// </summary>
        public event Action<string> ErrorRaised;

        public GamePickerViewModel(API.Client client)
        {
            this._Client = client ?? throw new ArgumentNullException(nameof(client));
            this._Catalog = new GameCatalog(client);

            this._AppDataChangedCallback = client.CreateAndRegisterCallback<API.Callbacks.AppDataChanged>();
            this._AppDataChangedCallback.OnRun += this.OnAppDataChanged;

            this._CallbackTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Background,
                this.OnCallbackTimer);
            this._CallbackTimer.Start();
        }

        #region Bound state
        [ObservableProperty]
        private string _SearchText = "";

        [ObservableProperty]
        private string _AddGameId = "";

        [ObservableProperty]
        private string _Status = "";

        [ObservableProperty]
        private string _DownloadStatus;

        [ObservableProperty]
        private bool _IsBusy;

        [ObservableProperty]
        private GameViewModel _SelectedGame;

        [ObservableProperty]
        private bool _ShowGames = true;

        [ObservableProperty]
        private bool _ShowDemos;

        [ObservableProperty]
        private bool _ShowMods;

        [ObservableProperty]
        private bool _ShowJunk;

        partial void OnSearchTextChanged(string value) => this.ApplyFilter();

        partial void OnShowGamesChanged(bool value) => this.ApplyFilter();

        partial void OnShowDemosChanged(bool value) => this.ApplyFilter();

        partial void OnShowModsChanged(bool value) => this.ApplyFilter();

        partial void OnShowJunkChanged(bool value) => this.ApplyFilter();
        #endregion

        private void OnCallbackTimer(object sender, EventArgs e)
        {
            this._Client.RunCallbacks(false);
        }

        private void OnAppDataChanged(APITypes.AppDataChanged param)
        {
            if (param.Result == false)
            {
                return;
            }

            if (this._AllGames.TryGetValue(param.Id, out GameViewModel game) == false)
            {
                return;
            }

            game.Info.Name = this._Catalog.GetName(game.Id);
            game.NotifyNameChanged();
        }

        #region Loading
        [RelayCommand]
        public async Task RefreshAsync()
        {
            if (this.IsBusy == true)
            {
                return;
            }

            this.IsBusy = true;
            this.AddGameId = "";
            this.CancelPendingLogos();
            this._AllGames.Clear();
            this._LogosAttempted.Clear();

            try
            {
                this.Status = "Downloading game list...";
                List<GameListEntry> entries = await GameList.DownloadAsync().ConfigureAwait(true);

                this.Status = "Checking game ownership...";
                foreach (GameListEntry entry in entries)
                {
                    this.AddGame(entry.Id, entry.Type);
                }
            }
            catch (Exception e)
            {
                // Spacewar is owned by every account, so the picker stays usable offline.
                this.AddGame(480, "normal");
                this.ErrorRaised?.Invoke($"Could not download the game list.\n\n{e.Message}");
            }
            finally
            {
                this.ApplyFilter();
                this.IsBusy = false;
            }

            this.StartLogoDownloads();
        }

        private void AddGame(uint id, string type)
        {
            if (this._AllGames.ContainsKey(id) == true)
            {
                return;
            }

            if (this._Catalog.OwnsGame(id) == false)
            {
                return;
            }

            GameInfo info = new(id, type)
            {
                Name = this._Catalog.GetName(id),
            };
            this._AllGames.Add(id, new GameViewModel(info));
        }

        [RelayCommand]
        public void AddGameById()
        {
            if (uint.TryParse(this.AddGameId, out uint id) == false)
            {
                this.ErrorRaised?.Invoke("Please enter a valid game ID.");
                return;
            }

            if (this._Catalog.OwnsGame(id) == false)
            {
                this.ErrorRaised?.Invoke("You don't own that game.");
                return;
            }

            // Matches the original: adding one game replaces the list with just that game.
            this.CancelPendingLogos();
            this._AllGames.Clear();
            this._LogosAttempted.Clear();
            this.AddGameId = "";
            this.AddGame(id, "normal");
            this.ShowGames = true;
            this.ApplyFilter();
            this.StartLogoDownloads();
        }
        #endregion

        #region Filtering
        private void ApplyFilter()
        {
            string search = string.IsNullOrEmpty(this.SearchText) == true ? null : this.SearchText;

            this.Games.Clear();
            foreach (GameViewModel game in this._AllGames.Values.OrderBy(g => g.Name, StringComparer.CurrentCulture))
            {
                if (search != null &&
                    game.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                bool wanted = game.Type switch
                {
                    "normal" => this.ShowGames,
                    "demo" => this.ShowDemos,
                    "mod" => this.ShowMods,
                    "junk" => this.ShowJunk,
                    _ => true,
                };
                if (wanted == false)
                {
                    continue;
                }

                this.Games.Add(game);
            }

            this.Status = $"Displaying {this.Games.Count} games. Total {this._AllGames.Count} games.";

            if (this.Games.Count > 0)
            {
                this.SelectedGame = this.Games[0];
            }
        }
        #endregion

        #region Logos
        private void CancelPendingLogos()
        {
            this._LogoCancellation.Cancel();
            this._LogoCancellation.Dispose();
            this._LogoCancellation = new();
            this._PendingLogos = 0;
            this.DownloadStatus = null;
        }

        /// <summary>
        /// Only owned games reach this point, so fetching every logo is cheap enough that
        /// tracking which rows are on screen is not worth the complexity.
        /// </summary>
        private void StartLogoDownloads()
        {
            CancellationToken cancellationToken = this._LogoCancellation.Token;

            foreach (GameViewModel game in this._AllGames.Values)
            {
                if (game.Logo != null)
                {
                    continue;
                }

                string url = this._Catalog.GetLogoUrl(game.Id);
                if (string.IsNullOrEmpty(url) == true)
                {
                    continue;
                }

                game.ImageUrl = url;
                if (this._LogosAttempted.Add(url) == false)
                {
                    continue;
                }

                this._PendingLogos++;
                _ = this.DownloadLogoAsync(game, url, cancellationToken);
            }

            this.UpdateDownloadStatus();
        }

        private async Task DownloadLogoAsync(GameViewModel game, string url, CancellationToken cancellationToken)
        {
            try
            {
                await this._LogoLimit.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    byte[] data = await Downloader
                        .GetBytesAsync(new Uri(url), cancellationToken)
                        .ConfigureAwait(false);

                    using MemoryStream stream = new(data, false);
                    Bitmap bitmap = new(stream);

                    await Dispatcher.UIThread.InvokeAsync(() => game.Logo = bitmap);
                }
                finally
                {
                    this._LogoLimit.Release();
                }
            }
            catch (Exception)
            {
                // A missing or corrupt capsule just leaves the placeholder in place.
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this._PendingLogos--;
                    this.UpdateDownloadStatus();
                });
            }
        }

        private void UpdateDownloadStatus()
        {
            this.DownloadStatus = this._PendingLogos > 0
                ? $"Downloading {this._PendingLogos} game icons..."
                : null;
        }
        #endregion

        [RelayCommand]
        public void LaunchSelected()
        {
            GameViewModel game = this.SelectedGame;
            if (game == null)
            {
                return;
            }

            try
            {
                API.AppHost.Start("SAM.Game", game.Id.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                this.ErrorRaised?.Invoke($"Failed to start SAM.Game.\n\n{e.Message}");
            }
        }

        public void Dispose()
        {
            this._CallbackTimer.Stop();
            this._LogoCancellation.Cancel();
            this._LogoCancellation.Dispose();
            this._LogoLimit.Dispose();
        }
    }
}
