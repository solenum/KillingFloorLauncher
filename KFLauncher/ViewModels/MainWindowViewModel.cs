using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KFLauncher.Models;
using System;
using System.Collections.Generic;
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

        public MainWindowViewModel()
        {
            this.Config = InternalConfig.ReadConfig();
            this.kfConfig = new KFConfig(this.Config);

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
                            if (live[i] is null)
                            {
                                continue;
                            }

                            batch[i].Players = live[i]!.Players;
                            batch[i].Map = live[i]!.Map;
                            batch[i].Ping = live[i]!.Ping;
                            batch[i].Passworded = live[i]!.Passworded;
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
                this.IsRefreshing = false;
                this.refresh = null;
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
