using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using KFLauncher.Models;
using KFLauncher.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace KFLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly InternalConfig internalConfig = new();
        private KFConfig kfConfig;
        public JsonConfig Config { get; set; } = new();

        public MainWindowViewModel()
        {
            this.Config = this.internalConfig.ReadConfig(this.Config);
            this.kfConfig = new KFConfig(this.Config);

            if (this.Config.FirstLaunch)
            {
                if (!this.internalConfig.AppFileExists("KillingFloor.ini"))
                {
                    Debug.WriteLine("First launch, backing up KillingFloor.ini");
                    string kf = this.kfConfig.KillingFloorIni;
                    this.internalConfig.WriteFile("KillingFloor.ini", kf);
                }
                if (!this.internalConfig.AppFileExists("User.ini"))
                {
                    Debug.WriteLine("First launch, backing up User.ini");
                    string usr = this.kfConfig.UserIni;
                    this.internalConfig.WriteFile("User.ini", usr);
                }
            }

            this.Config.FirstLaunch = false;
            this.internalConfig.WriteConfig(this.Config);
        }
        
        private async void OpenFinder()
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
                this.Config.GamePath = result;
            }

            this.internalConfig.WriteConfig(this.Config);
        }

        private async Task StartGame()
        {
            // patch configs
            this.internalConfig.WriteConfig(this.Config);
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
            Config.GamePath = this.kfConfig.DetectGamePath();
            
            this.internalConfig.WriteConfig(this.Config);
        }
        
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

            this.internalConfig.WriteConfig(this.Config);
        }

        private void RestoreConfig()
        {
            string kf = this.internalConfig.ReadFile("KillingFloor.ini");
            string usr = this.internalConfig.ReadFile("User.ini");
            if (kf.Length > 100)
            {
                Debug.WriteLine("Restoring KillingFloor.ini");
                Debug.WriteLine(kf);
                this.kfConfig.KillingFloorIni = kf;
            }
            if (usr.Length > 100)
            {
                Debug.WriteLine("Restoring User.ini");
                this.kfConfig.UserIni = usr;
            }
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
