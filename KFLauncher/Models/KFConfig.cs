using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KFLauncher.Models
{
    internal class KFConfig
    {
        private readonly JsonConfig config;

        private string KillingFloorIniPath => Path.Combine(this.config.GamePath, "System", "KillingFloor.ini");
        private string UserIniPath => Path.Combine(this.config.GamePath, "System", "User.ini");

        public string KillingFloorIni
        {
            get => ReadIni(this.KillingFloorIniPath);
            set => WriteIni(this.KillingFloorIniPath, value);
        }

        public string UserIni
        {
            get => ReadIni(this.UserIniPath);
            set => WriteIni(this.UserIniPath, value);
        }

        public KFConfig(JsonConfig config)
        {
            this.config = config;
        }

        #region patches
        public void FixConfig()
        {
            // TODO: better detect if the config is malformed
            string ini = this.KillingFloorIni;
            if (ini.Length < 1000)
            {
                this.KillingFloorIni = DefaultConfigs.KillingFloorIni;
            }

            ini = this.UserIni;
            if (ini.Length < 1000)
            {
                this.UserIni = DefaultConfigs.UserIni;
            }
        }

        public void ApplySetPatches()
        {
            this.ApplyAllFixes();
        }

        public void ApplyAllFixes()
        {
            // TODO: investigate best values (networking specifically seems meh?)
            if (this.config.DisableCache)
            {
                this.FixCacheDisable();
            }
            else
            {
                this.FixCacheEnable();
            }
            if (this.config.UnlockFramerate)
            {
                this.FixFPSLock();
                this.FixNetspeed();
            }
            if (this.config.OptimizePerformance)
            {
                this.FixPerformance();
            }
            if (this.config.FixMouseInput)
            {
                this.FixMouseLag();
            }
            if (this.config.IncreaseCacheLimit)
            {
                this.FixCacheSizeEnable();
            }
            else
            {
                this.FixCacheSizeDisable();
            }
            if (this.config.DisableMovies)
            {
                this.FixMoviesDisable();
            }
            else
            {
                this.FixMoviesEnable();
            }
            if (this.config.DisableMusic)
            {
                this.FixMusic();
            }
            if (this.config.SkipIntro)
            {
                this.FixIntroDisable();
            }
            else
            {
                this.FixIntroEnable();
            }
            if (this.config.QuickHeal)
            {
                this.FixQuickHealEnable();
            }
            else
            {
                this.FixQuickHealDisable();
            }
            if (this.config.SetResolution)
            {
                this.FixResolution();
            }
            if (this.config.LockMouse)
            {
                this.FixMouseGrab();
            }
        }

        /// <summary>
        /// Under proton the pointer is wines business, not the games: without these the cursor walks
        /// straight off onto another monitor and a click tabs you out mid wave.  Wine reads this at
        /// startup, so patching before launch is the moment it takes effect.
        /// </summary>
        void FixMouseGrab()
        {
            // .../steamapps/common/KillingFloor -> .../steamapps/compatdata/1250/pfx/user.reg
            string prefix = Path.GetFullPath(Path.Combine(this.config.GamePath, "..", "..", "compatdata", "1250", "pfx", "user.reg"));
            if (!File.Exists(prefix))
            {
                return;
            }

            Debug.WriteLine("Locking the mouse to the game window");
            string reg = File.ReadAllText(prefix);
            reg = PatchReg(reg, X11Driver, "GrabFullscreen", "Y");
            reg = PatchReg(reg, X11Driver, "DXGrab", "Y");
            File.WriteAllText(prefix, reg);
        }

        void FixResolution()
        {
            Debug.WriteLine("Fixing resolution");
            string ini = this.KillingFloorIni;
            ini = PatchIni(ini, "WindowedViewportX", this.config.ResX);
            ini = PatchIni(ini, "WindowedViewportY", this.config.ResY);
            ini = PatchIni(ini, "FullscreenViewportX", this.config.ResX);
            ini = PatchIni(ini, "FullscreenViewportY", this.config.ResY);
            ini = PatchIni(ini, "MenuViewportX", this.config.ResX);
            ini = PatchIni(ini, "MenuViewportY", this.config.ResY);
            this.KillingFloorIni = ini;
        }

        void FixMoviesDisable()
        {
            try
            {
                string movies = Path.Combine(this.config.GamePath, "Movies");
                if (Directory.Exists(movies))
                {
                    Debug.WriteLine("Disabling movies");
                    Directory.Move(movies, Path.Combine(this.config.GamePath, "_Movies"));
                }
            }
            catch
            {

            }
        }

        void FixMoviesEnable()
        {
            try
            {
                string movies = Path.Combine(this.config.GamePath, "_Movies");
                if (Directory.Exists(movies))
                {
                    Debug.WriteLine("Enabling movies");
                    Directory.Move(movies, Path.Combine(this.config.GamePath, "Movies"));
                }
            }
            catch
            {

            }
        }

        public void FixQuickHealEnable()
        {
            Debug.WriteLine("Binding quickheal");
            this.UserIni = PatchIni(this.UserIni, "Q", "getweapon syringe | onrelease SwitchToLastWeapon | onrelease quickheal");
        }

        public void FixQuickHealDisable()
        {
            Debug.WriteLine("Unbinding quickheal");
            this.UserIni = PatchIni(this.UserIni, "Q", "QuickHeal");
        }

        public void FixIntroEnable()
        {
            Debug.WriteLine("Enabling intro");
            this.KillingFloorIni = PatchIni(this.KillingFloorIni, "LocalMap", "KFintro.rom");
        }

        public void FixIntroDisable()
        {
            Debug.WriteLine("Disabling intro");
            this.KillingFloorIni = PatchIni(this.KillingFloorIni, "LocalMap", "KF-Menu.rom");
        }

        public void FixMusic()
        {
            Debug.WriteLine("Disabling music");
            this.UserIni = PatchIni(this.UserIni, "bDisableMusicInGame", "True");
            this.KillingFloorIni = PatchIni(this.KillingFloorIni, "MusicVolume", "0.0000");
        }

        public void FixCacheEnable()
        {
            Debug.WriteLine("Enabling cache");
            string ini = this.KillingFloorIni;
            ini = PatchIni(ini, "UsePrecaching", "True");
            ini = PatchIni(ini, "UsePrecache", "True");
            ini = PatchIni(ini, "bNeverPrecache", "False");
            this.KillingFloorIni = ini;
        }

        public void FixCacheDisable()
        {
            Debug.WriteLine("Disabling cache");
            string ini = this.KillingFloorIni;
            ini = PatchIni(ini, "UsePrecaching", "False");
            ini = PatchIni(ini, "UsePrecache", "False");
            ini = PatchIni(ini, "bNeverPrecache", "True");
            this.KillingFloorIni = ini;
        }

        public void FixCacheSizeEnable()
        {
            Debug.WriteLine("Fixing cache size");
            this.KillingFloorIni = PatchIni(this.KillingFloorIni, "CacheSizeMegs", "256");
        }

        public void FixCacheSizeDisable()
        {
            Debug.WriteLine("Fixing cache size");
            this.KillingFloorIni = PatchIni(this.KillingFloorIni, "CacheSizeMegs", "32");
        }

        public void FixFPSLock()
        {
            Debug.WriteLine("Fixing FPS lock");
            string ini = this.KillingFloorIni;
            ini = PatchIni(ini, "MaxClientFrameRate", "200");
            ini = PatchIni(ini, "MinDesiredFrameRate", "1.0000");
            this.KillingFloorIni = ini;
        }

        public void FixPerformance()
        {
            Debug.WriteLine("Fixing performance");
            string ini = this.KillingFloorIni;
            ini = PatchIni(ini, "CheckForOverflow", "True"); // False?
            ini = PatchIni(ini, "AvoidHitches", "True"); // False?
            this.KillingFloorIni = ini;
        }

        public void FixMouseLag()
        {
            Debug.WriteLine("Fixing mouse lag");
            this.KillingFloorIni = PatchIni(this.KillingFloorIni, "ReduceMouseLag", "True"); // False?

            string ini = this.UserIni;
            ini = PatchIni(ini, "MouseSamplingTime", "0.001");
            ini = PatchIni(ini, "MouseAccelThreshold", "-1");
            ini = PatchIni(ini, "MouseSmoothingMode", "0");
            ini = PatchIni(ini, "MouseSmoothingStrength", "0.000000");
            this.UserIni = ini;
        }

        public void FixNetspeed()
        {
            Debug.WriteLine("Fixing net speed");
            string ini = this.UserIni;
            ini = PatchIni(ini, "ConfiguredInternetSpeed", "15000");
            ini = PatchIni(ini, "LeftMouse", "Fire | netspeed 30000");
            ini = PatchIni(ini, "MiddleMouse", "AltFire | netspeed 30000");
            ini = PatchIni(ini, "RightMouse", "Aiming | netspeed 30000");
            this.UserIni = ini;
        }
        #endregion

        #region io
        private const string X11Driver = @"[Software\\Wine\\X11 Driver]";

        /// <summary>Sets a value in a wine user.reg, adding the section if it is not there yet.</summary>
        internal static string PatchReg(string reg, string section, string key, string value)
        {
            string entry = $"\"{key}\"=\"{value}\"";
            List<string> output = new();
            bool inSection = false;
            bool written = false;

            foreach (string line in reg.Replace("\r\n", "\n").Split('\n'))
            {
                if (line.StartsWith('['))
                {
                    // leaving our section without having written the value, so write it here
                    if (inSection && !written)
                    {
                        output.Add(entry);
                        written = true;
                    }

                    inSection = line.StartsWith(section, StringComparison.Ordinal);
                }

                if (inSection && line.StartsWith($"\"{key}\"=", StringComparison.Ordinal))
                {
                    output.Add(entry);
                    written = true;
                    continue;
                }

                output.Add(line);
            }

            if (!written)
            {
                if (!inSection)
                {
                    output.Add(string.Empty);
                    output.Add($"{section} {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
                }

                output.Add(entry);
            }

            return string.Join("\n", output);
        }

        internal static string PatchIni(string ini, string key, string value)
        {
            // anchored to the start of a line, otherwise short keys ("Q") eat other entries ("Quality=3")
            return Regex.Replace(ini, $@"^{Regex.Escape(key)}\s*=[^\r\n]*", $"{key}={value}", RegexOptions.Multiline);
        }

        private static string ReadIni(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static void WriteIni(string path, string ini)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                return;
            }

            // people mark these read only to stop the game clobbering them, which would otherwise
            // throw straight through the launch command
            if (File.Exists(path))
            {
                FileAttributes attributes = File.GetAttributes(path);
                if (attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                }
            }

            File.WriteAllText(path, ini);
        }

        /// <summary>Steam drops launch arguments when the game already runs, so we need to know.</summary>
        public static bool IsGameRunning()
        {
            try
            {
                // under proton the process is a wine one, and its name is cut to 15 characters, so
                // go by the command line instead, which still carries the full path to the exe
                if (OperatingSystem.IsLinux())
                {
                    foreach (string cmdline in Directory.EnumerateDirectories("/proc"))
                    {
                        string path = Path.Combine(cmdline, "cmdline");
                        if (File.Exists(path) && IsGameCommandLine(ReadQuietly(path)))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            catch (IOException)
            {
                // /proc entries come and go while we walk them
            }

            Process[] all;
            try
            {
                all = Process.GetProcesses();
            }
            catch
            {
                return false;
            }

            try
            {
                return all.Any(p => IsGameProcess(p.ProcessName));
            }
            finally
            {
                foreach (Process process in all)
                {
                    process.Dispose();
                }
            }
        }

        private static string ReadQuietly(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Windows and macos report the whole process name.  Matching the whole thing rather than a
        /// prefix keeps a launcher binary called KillingFloorLauncher out of it.
        /// </summary>
        internal static bool IsGameProcess(string name)
        {
            foreach (string suffix in new[] { ".exe", ".ex", ".bin" })
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name[..^suffix.Length];
                    break;
                }
            }

            return name.Equals("KillingFloor", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>KF2 lives in common/KillingFloor2 and runs KFGame.exe, so neither pattern hits it.</summary>
        internal static bool IsGameCommandLine(string cmdline)
        {
            cmdline = cmdline.Replace('\\', '/').ToLowerInvariant();

            return cmdline.Contains("killingfloor.exe") || cmdline.Contains("/killingfloor/");
        }

        /// <summary>Find the game via steams own library index, on any platform.</summary>
        public static string DetectGamePath()
        {
            Debug.WriteLine("Detecting game path..");

            foreach (string library in SteamLibraries())
            {
                string game = Path.Combine(library, "steamapps", "common", "KillingFloor");
                if (File.Exists(Path.Combine(game, "System", "KillingFloor.ini")) || Directory.Exists(Path.Combine(game, "System")))
                {
                    return game;
                }
            }

            return string.Empty;
        }

        private static IEnumerable<string> SteamLibraries()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] roots =
            [
                // windows
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"),
                // linux, including flatpak
                Path.Combine(home, ".steam", "steam"),
                Path.Combine(home, ".local", "share", "Steam"),
                Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam"),
                // macos
                Path.Combine(home, "Library", "Application Support", "Steam"),
            ];

            foreach (string root in roots.Where(r => Path.IsPathRooted(r) && Directory.Exists(r)))
            {
                yield return root;

                // extra libraries (other drives, external disks) are listed in the vdf
                string vdf = Path.Combine(root, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(vdf))
                {
                    continue;
                }

                foreach (Match match in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s+\"(.+?)\""))
                {
                    yield return match.Groups[1].Value.Replace("\\\\", "\\");
                }
            }
        }
        #endregion
    }
}
