using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SAM.API;

namespace SAM.Picker
{
    public partial class MainWindow : Window
    {
        private Client? _SteamClient;
        private ObservableCollection<GameInfoViewModel> _AllGames = new();
        private ObservableCollection<GameInfoViewModel> _FilteredGames = new();

        public MainWindow()
        {
            InitializeComponent();
            GamesListBox.ItemsSource = _FilteredGames;
            Loaded += OnLoaded;
            Title = "SAM v8.0 (Legacy Mode)";
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusLabel.Text = "Initializing Steam...";
                _SteamClient = new Client();
                _SteamClient.Initialize(0); 

                StatusLabel.Text = "Scanning all games...";
                await LoadGames();

                StatusLabel.Text = $"Found {_AllGames.Count} games.";
                FilterGames();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to initialize Steam: {ex.Message}", "Error");
            }
        }

        private async Task LoadGames()
        {
            var detectedGames = new Dictionary<uint, string>();

            try
            {
                Dispatcher.Invoke(() => StatusLabel.Text = "Downloading game list (games.xml)...");
                using var http = new System.Net.Http.HttpClient();
                string xml = await http.GetStringAsync("https://gib.me/sam/games.xml");
                
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(xml);
                
                Dispatcher.Invoke(() => StatusLabel.Text = "Checking game ownership (IsSubscribedApp)...");
                var nodes = doc.SelectNodes("/games/game");
                if (nodes != null)
                {
                    int total = nodes.Count;
                    int i = 0;
                    foreach (System.Xml.XmlNode node in nodes)
                    {
                        if (uint.TryParse(node.InnerText, out uint appId))
                        {
                            if (_SteamClient?.SteamApps008.IsSubscribedApp(appId) == true)
                            {
                                string name = _SteamClient?.SteamApps001.GetAppData(appId, "name");
                                if (string.IsNullOrEmpty(name)) name = "App " + appId;
                                detectedGames[appId] = name;
                            }
                        }
                        
                        i++;
                        if (i % 1000 == 0)
                        {
                            Dispatcher.Invoke(() => StatusLabel.Text = $"Checking ownership: {i}/{total} ...");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Game List Fetch: {ex.Message}");
                // Fallback to minimal
                detectedGames[480] = "SpaceWar";
            }

            // Update UI
            var sortedGames = detectedGames.OrderBy(x => x.Value).ToList();
            Dispatcher.Invoke(() => {
                _AllGames.Clear();
                foreach (var kvp in sortedGames) {
                    _AllGames.Add(new GameInfoViewModel { Id = kvp.Key, Name = kvp.Value });
                }
            });
        }

        private string GetGameImageCandidate(uint id)
        {
            try
            {
                var lang = _SteamClient?.SteamApps008.GetCurrentGameLanguage() ?? "english";
                string candidate = _SteamClient?.SteamApps001.GetAppData(id, $"small_capsule/{lang}") ?? "";
                if (!string.IsNullOrEmpty(candidate)) return $"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{id}/{candidate}";

                if (lang != "english") {
                    candidate = _SteamClient?.SteamApps001.GetAppData(id, "small_capsule/english") ?? "";
                    if (!string.IsNullOrEmpty(candidate)) return $"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{id}/{candidate}";
                }

                candidate = _SteamClient?.SteamApps001.GetAppData(id, "logo") ?? "";
                if (!string.IsNullOrEmpty(candidate)) return $"https://cdn.steamstatic.com/steamcommunity/public/images/apps/{id}/{candidate}.jpg";
            }
            catch { }
            return $"https://cdn.steamstatic.com/steam/apps/{id}/capsule_184x69.jpg";
        }

        private async Task LoadCapsule(GameInfoViewModel viewModel)
        {
            if (viewModel.Image != null) return;
            try
            {
                string url = GetGameImageCandidate(viewModel.Id);
                using var http = new System.Net.Http.HttpClient();
                var data = await http.GetByteArrayAsync(url);
                
                await Dispatcher.InvokeAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new System.IO.MemoryStream(data);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    viewModel.Image = bitmap;
                });
            }
            catch { }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e) { FilterGames(); }

        private void FilterGames()
        {
            string query = SearchBox.Text.ToLower().Trim();
            _FilteredGames.Clear();

            var matches = _AllGames
                .Where(g => string.IsNullOrEmpty(query) || 
                            g.Name.ToLower().Contains(query) || 
                            g.Id.ToString().Contains(query))
                .Take(1000) 
                .ToList();

            foreach (var match in matches) {
                _FilteredGames.Add(match);
                if (match.Image == null) _ = LoadCapsule(match);
            }
            StatusLabel.Text = $"Showing {matches.Count} games (Total: {_AllGames.Count})";
        }

        private void OnGameDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (GamesListBox.SelectedItem is GameInfoViewModel game) LaunchGameManager(game.Id);
        }

        private void LaunchGameManager(uint appId)
        {
            try {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string exeName = "SAM.Game.exe";
                string targetPath = Path.Combine(basePath, exeName);

                if (!File.Exists(targetPath))
                {
                    // Fallback for debug environment where SAM.Game outputs to root \bin\
                    string devPath = Path.GetFullPath(Path.Combine(basePath, "..\\..\\..\\..\\..\\bin", exeName));
                    if (File.Exists(devPath)) targetPath = devPath;
                }

                Process.Start(new ProcessStartInfo { 
                    FileName = targetPath, 
                    Arguments = appId.ToString(), 
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(targetPath)
                });
            } catch (Exception ex) {
                System.Windows.MessageBox.Show($"Failed: {ex.Message}", "Error");
            }
        }
        private void OnAddGameClick(object sender, RoutedEventArgs e)
        {
            uint appId = 480; 
            string name = _SteamClient?.SteamApps001.GetAppData(appId, "name") ?? "Manual App " + appId;
            var viewModel = new GameInfoViewModel { Id = appId, Name = name };
            _AllGames.Add(viewModel);
            FilterGames();
            _ = LoadCapsule(viewModel);
        }
    }

    public class GameInfoViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private BitmapImage? _image;
        public uint Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BitmapImage? Image { get => _image; set { _image = value; OnPropertyChanged(nameof(Image)); } }
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}