using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using KFLauncher.Models;
using KFLauncher.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KFLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly InternalConfig internalConfig = new();
        private KFConfig kfConfig;
        public JsonConfig config { get; set; } = new();

        public MainWindowViewModel()
        {
            this.config = this.internalConfig.ReadConfig(this.config);
            this.kfConfig = new KFConfig(this.config);
        }
        
        private async void FindGameFolder()
        {
            var appReference = (IClassicDesktopStyleApplicationLifetime?)Avalonia.Application.Current?.ApplicationLifetime ?? null;
            if (appReference is null)
            {
                return;
            }

            var dialog = new OpenFolderDialog();
            var window = (MainWindow)appReference.MainWindow;
            var result = await dialog.ShowAsync(window);

            if (result is not null)
            {
                this.config.GamePath = result;
            }

            this.internalConfig.WriteConfig(this.config);
        }

        private async Task StartGame()
        {
            // patch configs
            this.kfConfig.FixConfig();
            this.kfConfig.ApplySetPatches();

            await Task.Delay(500);

            // open kf via steam uri
            var ps = new ProcessStartInfo("steam://run/1250")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);

            // TODO: often breaks due to file access denied (in use by other process)
            //await Task.Delay(10000);
            //this.kfConfig.EnableWatcher();
            this.Close();
        }

        private void DetectConfig()
        {
            this.config.GamePath = this.kfConfig.DetectGamePath();
        }

        private void SetRecommended()
        {
            this.kfConfig.SetRecommended();
        }

        private void Close()
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
