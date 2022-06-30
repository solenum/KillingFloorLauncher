using Avalonia.Controls.ApplicationLifetimes;
using KFLauncher.Models;
using KFLauncher.Views;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KFLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Launcher launcher = new();

        #region values
        private string _gamePath = String.Empty;
        public string GamePath
        {
            get => this._gamePath;
            set
            {
                this.RaiseAndSetIfChanged(ref _gamePath, value);
                this.launcher.GamePath = value;
            }
        }

        private bool _disableCache = false;
        public bool DisableCache
        {
            get => this._disableCache;
            set
            {
                this.RaiseAndSetIfChanged(ref _disableCache, value);
                this.launcher.internalConfig.Config.DisableCache = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }

        private bool _optimizePerformance = true;
        public bool OptimizePerformance
        {
            get => this._optimizePerformance;
            set
            {
                this.RaiseAndSetIfChanged(ref _optimizePerformance, value);
                this.launcher.internalConfig.Config.OptimizePerformance = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }

        private bool _disableMusic = false;
        public bool DisableMusic
        {
            get => this._disableMusic;
            set
            {
                this.RaiseAndSetIfChanged(ref _disableMusic, value);
                this.launcher.internalConfig.Config.DisableMusic = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }

        private bool _skipIntro = true;
        public bool SkipIntro
        {
            get => this._skipIntro;
            set
            {
                this.RaiseAndSetIfChanged(ref _skipIntro, value);
                this.launcher.internalConfig.Config.SkipIntro = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }

        private bool _increaseCacheLimit = true;
        public bool IncreaseCacheLimit
        {
            get => this._increaseCacheLimit;
            set
            {
                this.RaiseAndSetIfChanged(ref _increaseCacheLimit, value);
                this.launcher.internalConfig.Config.IncreaseCacheLimit = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }

        private bool _unlockFramerate = true;
        public bool UnlockFramerate
        {
            get => this._unlockFramerate;
            set
            {
                this.RaiseAndSetIfChanged(ref _unlockFramerate, value);
                this.launcher.internalConfig.Config.UnlockFramerate = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }

        private bool _fixMouseInput = true;
        public bool FixMouseInput
        {
            get => this._fixMouseInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _fixMouseInput, value);
                this.launcher.internalConfig.Config.FixMouseInput = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }

        private bool _disableMovies = false;
        public bool DisableMovies
        {
            get => this._disableMovies;
            set
            {
                this.RaiseAndSetIfChanged(ref _disableMovies, value);
                this.launcher.internalConfig.Config.DisableMovies = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }

        private bool _quickHeal = false;
        public bool QuickHeal
        {
            get => this._quickHeal;
            set
            {
                this.RaiseAndSetIfChanged(ref _quickHeal, value);
                this.launcher.internalConfig.Config.QuickHeal = value;
                this.launcher.internalConfig.WriteConfig();
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            this.LoadSettings();
        }

        private void LoadSettings()
        {
            this.GamePath = this.launcher.GamePath;
            this.DisableCache = this.launcher.internalConfig.Config.DisableCache;
            this.OptimizePerformance = this.launcher.internalConfig.Config.OptimizePerformance;
            this.DisableMusic = this.launcher.internalConfig.Config.DisableMusic;
            this.SkipIntro = this.launcher.internalConfig.Config.SkipIntro;
            this.IncreaseCacheLimit = this.launcher.internalConfig.Config.IncreaseCacheLimit;
            this.UnlockFramerate = this.launcher.internalConfig.Config.UnlockFramerate;
            this.FixMouseInput = this.launcher.internalConfig.Config.FixMouseInput;
            this.DisableMovies = this.launcher.internalConfig.Config.DisableMovies;
            this.QuickHeal = this.launcher.internalConfig.Config.QuickHeal;
        }

        private async void FindGameFolder()
        {
            string? result = await this.launcher.OpenFinderDialog();
            if (result is not null)
            {
                this.GamePath = result;
            }
        }

        private async Task StartGame()
        {
            await this.launcher.StartGame();
            this.Close();
        }

        private void DetectConfig()
        {
            this.GamePath = this.launcher.DetectConfig();
        }

        private void SetRecommended()
        {
            this.launcher.kfConfig.SetRecommended();
            this.LoadSettings();
        }

        private async void Close()
        {
            var appReference = (IClassicDesktopStyleApplicationLifetime?)Avalonia.Application.Current?.ApplicationLifetime ?? null;
            if (appReference is null)
            {
                return;
            }

            appReference.MainWindow.Close();
        }
    }
}
