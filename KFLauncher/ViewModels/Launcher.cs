using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using KFLauncher.Models;
using KFLauncher.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFLauncher.ViewModels
{
    internal class Launcher
    {
        private InternalConfig internalConfig = new();
        private KFConfig kfConfig;
        public string GamePath 
        { 
            get 
            {
                return this.internalConfig.Config.GamePath; 
            }
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = this.kfConfig.DetectGamePath();
                }
                this.internalConfig.Config.GamePath = value;
                this.internalConfig.WriteConfig();
            } 
        }
        
        public Launcher()
        {
            this.kfConfig = new KFConfig(this.internalConfig);
            this.GamePath = this.internalConfig.Config.GamePath;
        }

        public string DetectConfig()
        {
            this.GamePath = "";
            return this.GamePath;
        }

        public async Task StartGame()
        {
            this.kfConfig.FixConfig();

            // patch configs
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
        }

        public async Task<string?> OpenFinderDialog()
        {
            var appReference = (IClassicDesktopStyleApplicationLifetime?)Avalonia.Application.Current?.ApplicationLifetime ?? null;
            if (appReference is null)
            {
                return null;
            }

            var dialog = new OpenFolderDialog();
            var window = (MainWindow)appReference.MainWindow;
            var result = await dialog.ShowAsync(window);

            return result;
        }
    }
}
