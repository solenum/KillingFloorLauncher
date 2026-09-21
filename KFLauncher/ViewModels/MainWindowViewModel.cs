using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KFLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KFLauncher.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly KFConfig kfConfig;
        private readonly List<ServerInfo> allServers = new();
        private CancellationTokenSource? refresh;
        private CancellationTokenSource? playerQuery;

        public JsonConfig Config { get; }

        /// <summary>Swapped wholesale rather than mutated, one grid rebind instead of one per row.</summary>
        [ObservableProperty]
        private IReadOnlyList<ServerInfo> servers = [];

        /// <summary>Nothing to ask steam with: no relay to read and no key of our own.</summary>
        public bool NeedsServerSource => this.Config.ServerListUrl.Length == 0 && this.Config.SteamApiKey.Length == 0;

        [ObservableProperty]
        private string status = string.Empty;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string filter = string.Empty;

        [ObservableProperty]
        private bool hideEmpty = true;

        [ObservableProperty]
        private bool hideFull;

        [ObservableProperty]
        private ServerInfo? selectedServer;

        [ObservableProperty]
        private IReadOnlyList<PlayerInfo> players = [];

        [ObservableProperty]
        private string playersStatus = string.Empty;

        /// <summary>Saved servers, kept as their own copies so the big refresh cannot drop them.</summary>
        public ObservableCollection<ServerInfo> Favorites { get; } = new();

        [ObservableProperty]
        private string favoriteAddress = string.Empty;

        [ObservableProperty]
        private ServerInfo? selectedFavorite;

        [ObservableProperty]
        private string favoritesStatus = string.Empty;

        public MainWindowViewModel()
        {
            this.Config = InternalConfig.ReadConfig();
            this.kfConfig = new KFConfig(this.Config);

            // a locked or unreadable ini must not cost us the window: the launch tab still works
            try
            {
                if (this.Config.FirstLaunch)
                {
                    if (!InternalConfig.AppFileExists("KillingFloor.ini"))
                    {
                        Debug.WriteLine("First launch, backing up KillingFloor.ini");
                        InternalConfig.WriteFile("KillingFloor.ini", this.kfConfig.KillingFloorIni);
                    }
                    if (!InternalConfig.AppFileExists("User.ini"))
                    {
                        Debug.WriteLine("First launch, backing up User.ini");
                        InternalConfig.WriteFile("User.ini", this.kfConfig.UserIni);
                    }

                    this.Config.FirstLaunch = false;
                }
            }
            catch (Exception ex)
            {
                TraceLog.Error("first launch backup", ex);
            }

            // an empty url means "whatever this build ships with", so configs written before a
            // relay existed pick it up too
            if (this.Config.ServerListUrl.Length == 0)
            {
                this.Config.ServerListUrl = ServerBrowser.DefaultListUrl;
            }

            if (this.Config.GamePath.Length == 0)
            {
                this.Config.GamePath = KFConfig.DetectGamePath();
            }

            InternalConfig.WriteConfig(this.Config);
            this.Config.PropertyChanged += (_, e) =>
            {
                InternalConfig.WriteConfig(this.Config);
                if (e.PropertyName is nameof(JsonConfig.SteamApiKey) or nameof(JsonConfig.ServerListUrl))
                {
                    this.OnPropertyChanged(nameof(this.NeedsServerSource));
                }
            };

            foreach (Favorite favorite in this.Config.Favorites)
            {
                if (IPEndPoint.TryParse(favorite.Query, out IPEndPoint? query))
                {
                    this.Favorites.Add(new ServerInfo
                    {
                        Query = query,
                        GamePort = favorite.GamePort,
                        Name = $"{query.Address}:{favorite.GamePort}",
                        IsFavorite = true,
                    });
                }
            }

            if (this.Favorites.Count > 0)
            {
                _ = this.RefreshFavoritesCommand.ExecuteAsync(null);
            }

            if (!this.NeedsServerSource)
            {
                _ = this.RefreshServersCommand.ExecuteAsync(null);
            }
        }

        #region servers
        [RelayCommand]
        private async Task RefreshServers()
        {
            if (this.NeedsServerSource)
            {
                this.Status = "Set a server list url, or paste a steam web api key, to see servers";
                return;
            }

            SafeCancel(this.refresh);
            CancellationTokenSource cts = new();
            this.refresh = cts;

            this.IsRefreshing = true;
            this.Status = "Asking steam for servers..";

            try
            {
                List<ServerInfo> servers = await ServerBrowser.FetchListAsync(this.Config.ServerListUrl, this.Config.SteamApiKey, cts.Token);
                foreach (ServerInfo server in servers)
                {
                    server.IsFavorite = this.Favorites.Any(f => f.Query.Equals(server.Query));
                }

                this.allServers.Clear();
                this.allServers.AddRange(servers);
                this.ApplyFilter();
                this.Status = $"{servers.Count} servers, checking who is home..";

                // steams counts are a minute or so stale, so ask each server itself and time the reply
                // as ping.  one batch of results per trip to the ui thread, a dispatch per server
                // buries it under thousands of tiny callbacks and the window stops redrawing.
                int done = 0;
                foreach (ServerInfo[] batch in servers.Chunk(64))
                {
                    A2SInfo?[] live = await Task.WhenAll(batch.Select(server => ServerBrowser.QueryAsync(server.Query, ct: cts.Token)));

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        for (int i = 0; i < batch.Length; i++)
                        {
                            if (live[i] is not null)
                            {
                                batch[i].Apply(live[i]!);
                            }
                        }

                        done += batch.Length;
                        this.Status = $"{servers.Count} servers, pinged {done}..";
                    });

                    cts.Token.ThrowIfCancellationRequested();
                }

                this.ApplyFilter();
                this.Status = $"{this.allServers.Count} servers, {this.allServers.Sum(s => s.Players)} players online";
            }
            catch (HttpRequestException ex)
            {
                bool relay = this.Config.ServerListUrl.Length > 0;
                this.Status = ex.StatusCode switch
                {
                    HttpStatusCode.Forbidden when !relay => "Steam rejected that api key",
                    _ when relay => $"Could not read the server list from {this.Config.ServerListUrl} ({ex.StatusCode})",
                    _ => $"Could not reach the steam api ({ex.StatusCode})",
                };
            }
            catch (OperationCanceledException)
            {
                // superseded by a newer refresh
            }
            catch (Exception ex)
            {
                TraceLog.Error("refresh", ex);
                this.Status = $"Server list failed: {ex.Message}";
            }
            finally
            {
                // a newer refresh may already own these, in which case they are not ours to clear
                if (this.refresh == cts)
                {
                    this.IsRefreshing = false;
                    this.refresh = null;
                }

                cts.Dispose();
            }
        }

        [RelayCommand]
        private void GetApiKey()
        {
            Open(ServerBrowser.ApiKeyUrl);
        }

        [RelayCommand]
        private async Task Connect(ServerInfo? server)
        {
            if (server is null)
            {
                return;
            }

            // cancelling runs every registered callback on whatever thread calls it, and there can
            // be hundreds of in flight socket cancellations, so do not do it on the ui thread
            try
            {
                TraceLog.Log("connect: cancelling refresh");
                CancellationTokenSource? inFlight = this.refresh;
                await Task.Run(() => SafeCancel(inFlight));

                bool running = await Task.Run(KFConfig.IsGameRunning);
                TraceLog.Log($"connect: game already running = {running}");

                if (running)
                {
                    // nothing steam offers reaches a running KF: launch arguments are dropped for an
                    // app it already runs, and steam://connect wants an app id from the server that
                    // KF does not report.  the games own console is the way in.
                    await this.CopyAsync(server.ConsoleCommand);
                    this.Status = $"Killing Floor is already running. Press ~ in game and paste: {server.ConsoleCommand} (copied to your clipboard)";
                    TraceLog.Log($"connect: game running, handed over console command {server.ConsoleCommand}");

                    return;
                }

                if (!this.kfConfig.HasGameFiles)
                {
                    this.Status = "No System folder at the game path, set it on the Launch tab";
                    return;
                }

                // patching walks both ini files a fair few times, keep it off the ui thread
                this.Status = $"Patching config, then joining {server.Name}..";
                TraceLog.Log("connect: patching config");
                await Task.Run(() =>
                {
                    this.kfConfig.FixConfig();
                    this.kfConfig.ApplySetPatches();
                });

                TraceLog.Log("connect: getting out of the way");
                this.GetOutOfTheWay();

                TraceLog.Log($"connect: opening {server.LaunchUri}");
                await Task.Run(() => Open(server.LaunchUri));
                TraceLog.Log("connect: steam uri handed over");

                this.Status = $"Handed {server.Name} to steam";
            }
            catch (Exception ex)
            {
                TraceLog.Error("connect", ex);
                this.Status = $"Could not join {server.Name}: {ex.Message}";
            }
        }

        partial void OnSelectedServerChanged(ServerInfo? value) => _ = this.LoadPlayers(value);

        /// <summary>Who is on the selected server, asked of the server itself.</summary>
        private async Task LoadPlayers(ServerInfo? server)
        {
            SafeCancel(this.playerQuery);
            this.playerQuery = null;
            this.Players = [];

            if (server is null)
            {
                this.PlayersStatus = string.Empty;
                return;
            }

            CancellationTokenSource cts = new();
            this.playerQuery = cts;
            this.PlayersStatus = "Asking the server who is playing..";

            List<PlayerInfo>? players;
            try
            {
                players = await ServerBrowser.QueryPlayersAsync(server.Query, ct: cts.Token);
            }
            catch (Exception ex)
            {
                // nothing awaits this, so an escaping exception would just vanish
                TraceLog.Error("player query", ex);
                this.PlayersStatus = "Could not ask the server who is playing";
                return;
            }

            // another row was clicked while this one was still answering
            if (cts.IsCancellationRequested)
            {
                return;
            }

            this.Players = players ?? [];
            this.PlayersStatus = players switch
            {
                null => "The server did not answer",
                { Count: 0 } => "Nobody playing right now",
                _ => $"{players.Count} playing",
            };
        }

        [RelayCommand]
        private async Task CopyAddress()
        {
            if (this.SelectedServer is null)
            {
                return;
            }

            await this.CopyAsync(this.SelectedServer.Address);
            this.Status = $"Copied {this.SelectedServer.Address}";
        }

        partial void OnFilterChanged(string value) => this.ApplyFilter();

        partial void OnHideEmptyChanged(bool value) => this.ApplyFilter();

        partial void OnHideFullChanged(bool value) => this.ApplyFilter();

        private bool Passes(ServerInfo server)
        {
            if (this.HideEmpty && server.Players == 0)
            {
                return false;
            }
            if (this.HideFull && server.Players >= server.MaxPlayers)
            {
                return false;
            }

            return this.Filter.Length == 0
                || server.Name.Contains(this.Filter, StringComparison.OrdinalIgnoreCase)
                || server.Map.Contains(this.Filter, StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyFilter()
        {
            this.Servers = this.allServers.Where(this.Passes).OrderByDescending(s => s.Players).ToList();
        }
        #endregion

        #region favorites
        /// <summary>The star in the server list.  Favorites are copies, so a refresh cannot drop them.</summary>
        [RelayCommand]
        private void ToggleFavorite(ServerInfo? server)
        {
            if (server is null)
            {
                return;
            }

            ServerInfo? saved = this.Favorites.FirstOrDefault(f => f.Query.Equals(server.Query));
            if (saved is not null)
            {
                this.Favorites.Remove(saved);
            }
            else
            {
                this.Favorites.Add(server.Clone());
            }

            this.MarkFavorite(server.Query, saved is null);
            this.SaveFavorites();
        }

        /// <summary>Add a server nobody has to find in the list first, by "ip:port" off a website.</summary>
        [RelayCommand]
        private async Task AddFavorite()
        {
            string text = this.FavoriteAddress.Trim();
            if (text.Length == 0)
            {
                return;
            }

            // what people are handed is the game port, the one they would type after "open"
            if (!text.Contains(':'))
            {
                text += ":7707";
            }

            if (!IPEndPoint.TryParse(text, out IPEndPoint? entered))
            {
                this.FavoritesStatus = $"{text} is not an ip and port";
                return;
            }

            ushort gamePort = (ushort)entered.Port;
            if (gamePort == 0)
            {
                this.FavoritesStatus = "That address needs a port, usually 7707";
                return;
            }

            if (this.Favorites.Any(f => f.Query.Address.Equals(entered.Address) && f.GamePort == gamePort))
            {
                this.FavoritesStatus = "That server is already saved";
                return;
            }

            this.FavoritesStatus = $"Asking {entered} who it is..";

            // unreal answers A2S one port above the game, but plenty of hosts move it, so if the
            // usual place is quiet try the port as given before giving up on it
            IPEndPoint query = new(entered.Address, Math.Min(entered.Port + 1, 65535));
            A2SInfo? live = await ServerBrowser.QueryAsync(query);
            if (live is null)
            {
                query = entered;
                live = await ServerBrowser.QueryAsync(query);
            }

            ServerInfo server = new()
            {
                Query = query,
                GamePort = gamePort,
                Name = $"{entered.Address}:{gamePort}",
                IsFavorite = true,
            };

            if (live is not null)
            {
                server.Apply(live);
            }

            this.Favorites.Add(server);
            this.MarkFavorite(server.Query, true);
            this.SaveFavorites();
            this.FavoriteAddress = string.Empty;
            this.FavoritesStatus = live is null
                ? $"Saved {server.Address}, which did not answer.  It will be checked again on refresh."
                : $"Saved {server.Name}";
        }

        [RelayCommand]
        private async Task RefreshFavorites()
        {
            if (this.Favorites.Count == 0)
            {
                this.FavoritesStatus = "Star a server in the list, or add one by address";
                return;
            }

            this.FavoritesStatus = "Checking saved servers..";

            ServerInfo[] saved = [.. this.Favorites];

            A2SInfo?[] live;
            try
            {
                live = await Task.WhenAll(saved.Select(server => ServerBrowser.QueryAsync(server.Query)));
            }
            catch (Exception ex)
            {
                // this one runs at startup, where nothing is watching the task it returns
                TraceLog.Error("favorites refresh", ex);
                this.FavoritesStatus = "Could not check the saved servers";
                return;
            }

            for (int i = 0; i < saved.Length; i++)
            {
                if (live[i] is not null)
                {
                    saved[i].Apply(live[i]!);
                }
                else
                {
                    saved[i].Ping = -1;
                    saved[i].Players = 0;
                }
            }

            int up = live.Count(l => l is not null);
            this.FavoritesStatus = $"{up} of {saved.Length} saved servers answered";

            // a server can move its query port, or report a different game port than we saved
            this.SaveFavorites();
        }

        /// <summary>Keep the star in the big list in step with the favorites tab, and back.</summary>
        private void MarkFavorite(IPEndPoint query, bool favorite)
        {
            foreach (ServerInfo server in this.allServers.Where(s => s.Query.Equals(query)))
            {
                server.IsFavorite = favorite;
            }
        }

        /// <summary>Assigning the list is what saves it, the config writes itself on a change.</summary>
        private void SaveFavorites()
        {
            this.Config.Favorites = [.. this.Favorites.Select(f => new Favorite(f.Query.ToString(), f.GamePort))];
        }
        #endregion

        #region launcher
        [RelayCommand]
        private async Task Browse()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop
                || desktop.MainWindow is null)
            {
                return;
            }

            IReadOnlyList<IStorageFolder> result = await desktop.MainWindow.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions { Title = "Select the Killing Floor install directory" });

            if (result.Count > 0)
            {
                this.Config.GamePath = result[0].Path.LocalPath;
            }
        }

        [RelayCommand]
        private async Task StartGame()
        {
            try
            {
                CancellationTokenSource? inFlightRefresh = this.refresh;
                await Task.Run(() => SafeCancel(inFlightRefresh));

                if (!this.kfConfig.HasGameFiles)
                {
                    this.Status = "No System folder at the game path, set it below";
                    return;
                }

                this.Status = "Patching config files..";
                await Task.Run(() =>
                {
                    this.kfConfig.FixConfig();
                    this.kfConfig.ApplySetPatches();
                });

                this.GetOutOfTheWay();

                // shell execute can sit there for a while waiting on steam
                await Task.Run(() => Open("steam://run/1250"));
                this.Status = "Killing Floor is starting";
            }
            catch (Exception ex)
            {
                TraceLog.Error("launch", ex);
                this.Status = $"Could not launch: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DetectConfig()
        {
            string path = await Task.Run(KFConfig.DetectGamePath);
            this.Config.GamePath = path;
            this.Status = path.Length > 0 ? $"Found game at {path}" : "Could not find the game, set the path manually";
        }

        [RelayCommand]
        private void SetRecommended()
        {
            this.Config.DisableCache = false;
            this.Config.OptimizePerformance = true;
            this.Config.DisableMusic = false;
            this.Config.SkipIntro = false;
            this.Config.IncreaseCacheLimit = true;
            this.Config.UnlockFramerate = true;
            this.Config.FixMouseInput = true;
            this.Config.DisableMovies = false;
            this.Config.QuickHeal = true;
        }

        [RelayCommand]
        private async Task RestoreConfig()
        {
            await Task.Run(() =>
            {
                string kf = InternalConfig.ReadFile("KillingFloor.ini");
                string usr = InternalConfig.ReadFile("User.ini");
                if (kf.Length > 100)
                {
                    Debug.WriteLine("Restoring KillingFloor.ini");
                    this.kfConfig.KillingFloorIni = kf;
                }
                if (usr.Length > 100)
                {
                    Debug.WriteLine("Restoring User.ini");
                    this.kfConfig.UserIni = usr;
                }
            });

            this.Status = "Restored the original configuration files";
        }
        #endregion

        /// <summary>The refresh disposes its own source, so a late cancel can land on a dead one.</summary>
        private static void SafeCancel(CancellationTokenSource? cts)
        {
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>So the console command is there to paste if steam will not switch the game.</summary>
        private async Task CopyAsync(string text)
        {
            Window? window = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window?.Clipboard is not null)
            {
                await window.Clipboard.SetTextAsync(text);
            }
        }

        private static void Open(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }

        /// <summary>
        /// What the launcher does once the game is on its way, see <see cref="JsonConfig.AfterLaunch"/>.
        /// Minimizing suspends rendering, which on a tiling wm that never hides the window reads as
        /// a frozen ui until something forces a resize, hence leaving it open being the default.
        /// </summary>
        private void GetOutOfTheWay()
        {
            Window? window = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window is null)
            {
                return;
            }

            switch (this.Config.AfterLaunch)
            {
                case 1:
                    window.WindowState = WindowState.Minimized;
                    break;

                case 2:
                    window.Close();
                    break;
            }
        }
    }
}
