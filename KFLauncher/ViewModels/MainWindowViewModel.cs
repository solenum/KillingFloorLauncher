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

        public MainWindowViewModel()
        {
            this.GamePath = this.launcher.GamePath;
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
