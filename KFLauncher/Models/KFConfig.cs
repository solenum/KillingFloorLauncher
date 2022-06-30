using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using KFLauncher.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KFLauncher.Models
{
    internal class KFConfig
    {
        private InternalConfig config;
        private readonly string KillingFloorIniPath = "\\System\\KillingFloor.ini";
        private readonly string UserIniPath = "\\System\\User.ini";
        private FileSystemWatcher? watcher = null;
        private DateTime _lastEventTime = DateTime.Now;

        private string KillingFloorIni
        {
            get
            {
                if (File.Exists(this.config.Config.GamePath + this.KillingFloorIniPath))
                {
                    return this.ReadIni(this.config.Config.GamePath + this.KillingFloorIniPath);
                }
                return String.Empty;
            }
            set => this.WriteIni(this.config.Config.GamePath + this.KillingFloorIniPath, value);

        }
        private string UserIni 
        {
            get
            {
                if (File.Exists(this.config.Config.GamePath + this.UserIniPath))
                {
                    return this.ReadIni(this.config.Config.GamePath + this.UserIniPath);
                }
                return String.Empty;
            }
            set => this.WriteIni(this.config.Config.GamePath + this.UserIniPath, value);
        }

        public KFConfig(InternalConfig config)
        {
            this.config = config;
        }

        public void EnableWatcher()
        {
            // TODO: struggles with file access
            this.watcher = new FileSystemWatcher(this.config.Config.GamePath + "\\System\\");

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += ConfigChanged;
            
            watcher.Filter = "*.ini";
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
        }

        private void ConfigChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed || DateTime.Now.Subtract(this._lastEventTime).TotalMilliseconds < 500)
            {
                return;
            }
            this._lastEventTime = DateTime.Now;

            if (e.FullPath.Contains(this.KillingFloorIniPath) || e.FullPath.Contains(this.UserIniPath))
            {
                this.ApplySetPatches();
            }
        }

        public void FixConfig()
        {
            // TODO: better detect if the config is malformed
            string ini = this.KillingFloorIni;
            if (ini.Length < 1000)
            {
                ini = DefaultConfigs.KillingFloorIni;
                this.KillingFloorIni = ini;
            }

            ini = this.UserIni;
            if (ini.Length < 1000)
            {
                ini = DefaultConfigs.UserIni;
                this.UserIni = ini;
            }
        }

        public void ApplySetPatches()
        {
            this.ApplyAllFixes();
        }
        
        public void ApplyAllFixes()
        {
            // TODO: investigate best values (networking specifically seems meh?)
            if (this.config.Config.DisableCache)
            {
                this.FixCacheDisable();
            }
            else
            {
                this.FixCacheEnable();
            }
            if (this.config.Config.UnlockFramerate)
            {
                this.FixFPSLock();
                this.FixNetspeed();
            }
            if (this.config.Config.OptimizePerformance)
            {
                this.FixPerformance();
            }
            if (this.config.Config.FixMouseInput)
            {
                this.FixMouseLag();
            }
            if (this.config.Config.IncreaseCacheLimit)
            {
                this.FixCacheSizeEnable();
            }
            else
            {
                this.FixCacheSizeDisable();
            }
            if (this.config.Config.DisableMovies)
            {
                this.FixMoviesDisable();
            }
            else
            {
                this.FixMoviesEnable();
            }
            if (this.config.Config.DisableMusic)
            {
                this.FixMusic();
            }
            if (this.config.Config.SkipIntro)
            {
                this.FixIntroDisable();
            }
            else
            {
                this.FixIntroEnable();
            }
            if (this.config.Config.QuickHeal)
            {
                this.FixQuickHealEnable();
            }
            else
            {
                this.FixQuickHealDisable();
            }
        }
        public void SetRecommended()
        {
            this.config.Config.DisableCache = false;
            this.config.Config.OptimizePerformance = true;
            this.config.Config.DisableMusic = false;
            this.config.Config.SkipIntro = true;
            this.config.Config.IncreaseCacheLimit = true;
            this.config.Config.UnlockFramerate = true;
            this.config.Config.FixMouseInput = true;
            this.config.Config.DisableMovies = false;
            this.config.WriteConfig();
        }

        void FixMoviesDisable()
        {
            if (Directory.Exists(this.config.Config.GamePath + "\\Movies"))
            {
                Debug.WriteLine("Enabling movies");
                Directory.Move(this.config.Config.GamePath + "\\Movies", this.config.Config.GamePath + "\\_Movies");
            }
        }

        void FixMoviesEnable()
        {
            if (Directory.Exists(this.config.Config.GamePath + "\\_Movies"))
            {
                Debug.WriteLine("Disabling movies");
                Directory.Move(this.config.Config.GamePath + "\\_Movies", this.config.Config.GamePath + "\\Movies");
            }
        }

        public void FixQuickHealEnable()
        {
            Debug.WriteLine("Binding quickheal");
            string ini = this.UserIni;
            ini = this.PatchIni(ini, "Q", "getweapon syringe | onrelease SwitchToLastWeapon | onrelease quickheal");
            this.UserIni = ini;
        }

        public void FixQuickHealDisable()
        {
            Debug.WriteLine("Unbinding quickheal");
            string ini = this.UserIni;
            ini = this.PatchIni(ini, "Q", "QuickHeal");
            this.UserIni = ini;
        }

        public void FixIntroEnable()
        {
            Debug.WriteLine("Enabling intro");
            string ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "LocalMap", "KFintro.rom");
            this.KillingFloorIni = ini;
        }
        public void FixIntroDisable()
        {
            Debug.WriteLine("Disabling intro");
            string ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "LocalMap", "KF-Menu.rom");
            this.KillingFloorIni = ini;
        }

        public void FixMusic()
        {
            Debug.WriteLine("Disabling music");
            string ini = this.UserIni;
            ini = this.PatchIni(ini, "bDisableMusicInGame", "True");
            this.UserIni = ini;

            ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "MusicVolume", "0.0000");
            this.KillingFloorIni = ini;
        }

        public void FixCacheEnable()
        {
            Debug.WriteLine("Enabling cache");
            string ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "UsePrecaching", "True");
            ini = this.PatchIni(ini, "UsePrecache", "True");
            ini = this.PatchIni(ini, "bNeverPrecache", "False");
            this.KillingFloorIni = ini;
        }

        public void FixCacheDisable()
        {
            Debug.WriteLine("Disabling cache");
            string ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "UsePrecaching", "False");
            ini = this.PatchIni(ini, "UsePrecache", "False");
            ini = this.PatchIni(ini, "bNeverPrecache", "True");
            this.KillingFloorIni = ini;
        }

        public void FixCacheSizeEnable()
        {
            Debug.WriteLine("Fixing cache size");
            string ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "CacheSizeMegs", "256");
            this.KillingFloorIni = ini;
        }

        public void FixCacheSizeDisable()
        {
            Debug.WriteLine("Fixing cache size");
            string ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "CacheSizeMegs", "32");
            this.KillingFloorIni = ini;
        }

        public void FixFPSLock()
        {
            Debug.WriteLine("Fixing FPS lock");
            string ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "MaxClientFrameRate", "200");
            ini = this.PatchIni(ini, "MinDesiredFrameRate", "1.0000");
            this.KillingFloorIni = ini;
        }
        public void FixPerformance()
        {
            Debug.WriteLine("Fixing performance");
            string ini = this.KillingFloorIni;
            //ini = this.PatchIni(ini, "UseTripleBuffering", "True");
            ini = this.PatchIni(ini, "CheckForOverflow", "True"); // False?
            ini = this.PatchIni(ini, "AvoidHitches", "False"); // False?
            this.KillingFloorIni = ini;
        }
        public void FixMouseLag()
        {
            Debug.WriteLine("Fixing mouse lag");
            string ini = this.KillingFloorIni;
            ini = this.PatchIni(ini, "ReduceMouseLag", "True"); // False?
            this.KillingFloorIni = ini;
            
            ini = this.UserIni;
            ini = this.PatchIni(ini, "MouseSamplingTime", "0.001");
            ini = this.PatchIni(ini, "MouseAccelThreshold", "-1");
            ini = this.PatchIni(ini, "MouseSmoothingMode", "0");
            ini = this.PatchIni(ini, "MouseSmoothingStrength", "0.000000");
            this.UserIni = ini;
        }

        public void FixNetspeed()
        {
            Debug.WriteLine("Fixing net speed");
            string ini = this.UserIni;
            ini = this.PatchIni(ini, "ConfiguredInternetSpeed", "15000");
            ini = this.PatchIni(ini, "LeftMouse", "Fire | netspeed 30000");
            ini = this.PatchIni(ini, "MiddleMouse", "AltFire | netspeed 30000");
            ini = this.PatchIni(ini, "RightMouse", "Aiming | netspeed 30000");
            this.UserIni = ini;
        }

        private string PatchIni(string ini, string key, string value) 
        {
            string regex = $"({key})(=|\\s=|=\\s)*[^\r\n\b]*";
            return Regex.Replace(ini, regex, $"{key}={value}");
        }

        private string ReadIni(string path)
        {
            //while (!AwaitFileAccess(path)) { }
            string ini = String.Empty;
            if (File.Exists(path))
            {
                ini = File.ReadAllText(path);
            }

            return ini;
        }

        private void WriteIni(string path, string ini)
        {
            //while (AwaitFileAccess(path) == false) { }
            File.WriteAllText(path, ini);
        }

        public string DetectGamePath()
        {
            Debug.WriteLine("Detecting game path..");

            // scan for steam installation directories
            List<string> steamDirectories = new();
            string[] drives = Directory.GetLogicalDrives();
            foreach (string drive in drives)
            {
                steamDirectories.AddRange(
                    CrawlDirectories(drive, new string[] { "library", "steam", "games", "program files" })
                    .Where(path => path.ToLower().Contains("steam"))
                    );
            }

            // scan steam directories for the games folder
            List<string> gameDirectories = new();
            foreach (string dir in steamDirectories)
            {
                gameDirectories.AddRange(
                    CrawlDirectories(dir, new string[] { "steamapps", "common", "killingfloor" })
                    .Where(path => path.ToLower().Contains("killingfloor") && !path.Contains("2"))
                    );
            }
            gameDirectories = gameDirectories.Distinct().ToList();

            return gameDirectories.FirstOrDefault(String.Empty);
        }

        private static List<string> CrawlDirectories(string path, string[] matches, int depth = 0)
        {
            List<string> directories = new();
            try
            {
                foreach (string dir in Directory.EnumerateDirectories(path))
                {
                    string folderName = new DirectoryInfo(dir).Name;
                    if (matches.Any(folderName.ToLower().Contains))
                    {
                        directories.Add(dir);

                        if (depth > 5)
                        {
                            break;
                        }

                        directories.AddRange(CrawlDirectories(dir, matches, depth + 1));
                    }
                }
            }
            catch
            {

            }

            return directories;
        }

        private static bool AwaitFileAccess(string path)
        {
            // TODO: this is stupid and hardly works
            try
            {
                using (FileStream file = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return file.CanRead;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
