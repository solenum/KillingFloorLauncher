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

        /// <summary>Steam ships System/, copies people move around by hand often have system/.</summary>
        private string SystemPath => ResolveDir(this.config.GamePath, "System");
        private string KillingFloorIniPath => ResolveFile(this.SystemPath, "KillingFloor.ini");
        private string UserIniPath => ResolveFile(this.SystemPath, "User.ini");

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
        // KF repeats some keys in every renderer section, and leaves others out of the file
        // entirely until the game writes them, so each one says where it belongs.
        private const string URL = "[URL]";
        private const string GameEngine = "[Engine.GameEngine]";
        private const string LevelInfo = "[Engine.LevelInfo]";
        private const string NetDriver = "[IpDrv.TcpNetDriver]";
        private const string Audio = "[ALAudio.ALAudioSubsystem]";
        private const string Input = "[Engine.Input]";
        private const string PlayerInput = "[Engine.PlayerInput]";
        private const string PlayerController = "[Engine.PlayerController]";
        private const string Player = "[Engine.Player]";
        private const string ROPlayer = "[ROEngine.ROPlayer]";

        private const string QuickHealBind = "getweapon syringe | onrelease SwitchToLastWeapon | onrelease quickheal";

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

        /// <summary>
        /// Everything the checkboxes ask for, in one read and one write per file.  Every toggle
        /// writes both ways: unticking a box has to put the stock value back, or the only way out
        /// of a patch is the restore button.  Returns whatever the user should be told about.
        /// </summary>
        public string ApplySetPatches()
        {
            List<string> warnings = new();
            string kf = this.KillingFloorIni;
            string user = this.UserIni;

            // precaching: off trades a longer load for hitches the first time an asset appears
            string precache = this.config.DisableCache ? "False" : "True";
            kf = PatchIni(kf, "UsePrecaching", precache);
            kf = PatchIni(kf, "UsePrecache", precache);
            kf = PatchIni(kf, "bNeverPrecache", this.config.DisableCache ? "True" : "False");
            kf = SetIni(kf, GameEngine, "CacheSizeMegs", this.config.IncreaseCacheLimit ? "256" : "32");

            // the engine drops detail to hold MinDesiredFrameRate, which reads as stutter
            kf = SetIni(kf, LevelInfo, "MaxClientFrameRate", this.config.UnlockFramerate ? "300.000000" : "+90.0");
            kf = PatchIni(kf, "MinDesiredFrameRate", this.config.UnlockFramerate ? "1.000000" : "35.000000");

            // the client clamps its own rate to these two, so raising ConfiguredInternetSpeed on
            // its own did nothing: the netspeed chained onto the mouse was being cut to 10000
            kf = SetIni(kf, NetDriver, "MaxClientRate", this.config.ImproveNetcode ? "20000" : "15000");
            kf = SetIni(kf, NetDriver, "MaxInternetClientRate", this.config.ImproveNetcode ? "20000" : "10000");
            user = SetIni(user, Player, "ConfiguredInternetSpeed", this.config.ImproveNetcode ? "20000" : "9636");
            foreach ((string bind, string stock) in new[] { ("LeftMouse", "Fire"), ("MiddleMouse", "AltFire"), ("RightMouse", "Aiming") })
            {
                user = ChainBind(user, bind, stock, "netspeed 20000", this.config.ImproveNetcode);
            }

            // ReduceMouseLag flushes the gpu every frame, which is the lag it claims to reduce
            kf = PatchIni(kf, "ReduceMouseLag", this.config.FixMouseInput ? "False" : "True");
            user = SetIni(user, PlayerInput, "MouseSamplingTime", this.config.FixMouseInput ? "0.001000" : "0.008333");
            user = SetIni(user, PlayerInput, "MouseAccelThreshold", this.config.FixMouseInput ? "-1" : "0.000000");
            user = SetIni(user, PlayerInput, "MouseSmoothingMode", this.config.FixMouseInput ? "0" : "1");
            user = SetIni(user, PlayerInput, "MouseSmoothingStrength", this.config.FixMouseInput ? "0.000000" : "0.200000");
            user = SetIni(user, PlayerInput, "bEnableMouseSmoothing", this.config.FixMouseInput ? "False" : "True");

            kf = PatchIni(kf, "CheckForOverflow", this.config.OptimizePerformance ? "True" : "False");
            kf = PatchIni(kf, "AvoidHitches", this.config.OptimizePerformance ? "True" : "False");

            kf = SetIni(kf, Audio, "MusicVolume", this.config.DisableMusic ? "0.000000" : "0.010000");
            user = SetIni(user, ROPlayer, "bDisableMusicInGame", this.config.DisableMusic ? "True" : "False");

            // the stock config ships openal on the safe settings, which is not what it can do
            kf = SetIni(kf, Audio, "UseEAX", this.config.BetterAudio ? "True" : "False");
            kf = SetIni(kf, Audio, "Use3DSound", this.config.BetterAudio ? "True" : "False");
            kf = SetIni(kf, Audio, "UseDefaultDriver", this.config.BetterAudio ? "False" : "True");
            kf = SetIni(kf, Audio, "Channels", this.config.BetterAudio ? "64" : "32");

            kf = SetIni(kf, URL, "LocalMap", this.config.SkipIntro ? "KF-Menu.rom" : "KFIntro.rom");

            user = SetIni(user, ROPlayer, "bUseBlurEffect", this.config.DisableBlur ? "False" : "True");
            user = SetIni(user, PlayerController, "bNeverSwitchOnPickup", this.config.NoSwitchOnPickup ? "True" : "False");

            user = SwapBind(user, "Q", "QuickHeal", QuickHealBind, this.config.QuickHeal);

            // the game resets the view to DefaultFOV at trader time and on map change, so the value
            // goes in the config, and rides along on the forward bind to survive anything else
            string fov = this.config.SetFov ? Number(this.config.Fov, string.Empty, 30, 170, warnings, "field of view") : string.Empty;
            bool setFov = fov.Length > 0;
            user = SetIni(user, PlayerController, "DesiredFOV", setFov ? fov : "85.000000");
            user = SetIni(user, PlayerController, "DefaultFOV", setFov ? fov : "85.000000");
            user = ChainBind(user, "W", "MoveForward", $"fov {fov}", setFov);

            if (this.config.SetResolution)
            {
                string x = Number(this.config.ResX, string.Empty, 320, 16000, warnings, "width");
                string y = Number(this.config.ResY, string.Empty, 240, 16000, warnings, "height");

                if (x.Length > 0 && y.Length > 0)
                {
                    foreach (string key in new[] { "WindowedViewportX", "FullscreenViewportX", "MenuViewportX" })
                    {
                        kf = PatchIni(kf, key, x);
                    }
                    foreach (string key in new[] { "WindowedViewportY", "FullscreenViewportY", "MenuViewportY" })
                    {
                        kf = PatchIni(kf, key, y);
                    }
                }
            }

            this.KillingFloorIni = kf;
            this.UserIni = user;

            this.Movies(this.config.DisableMovies);
            this.MouseGrab(this.config.LockMouse);

            return string.Join("  ", warnings);
        }

        /// <summary>
        /// Under proton the pointer is wines business, not the games: without these the cursor walks
        /// straight off onto another monitor and a click tabs you out mid wave.  Wine reads this at
        /// startup, so patching before launch is the moment it takes effect.
        /// </summary>
        private void MouseGrab(bool grab)
        {
            string prefix = this.ProtonPrefix();
            if (prefix.Length == 0)
            {
                return;
            }

            try
            {
                string registry = Path.Combine(prefix, "user.reg");
                Debug.WriteLine($"{(grab ? "Locking" : "Releasing")} the mouse");
                string reg = File.ReadAllText(registry);
                reg = PatchReg(reg, X11Driver, "GrabFullscreen", grab ? "Y" : "N");
                reg = PatchReg(reg, X11Driver, "DXGrab", grab ? "Y" : "N");
                File.WriteAllText(registry, reg);
            }
            catch (Exception ex)
            {
                // wine rewrites this file itself, so we can lose a race with it
                TraceLog.Error("mouse grab", ex);
            }
        }

        /// <summary>
        /// The prefix the game runs in.  Set one by hand for a copy steam does not manage, otherwise
        /// look where proton puts the one for this app id, in the library the game itself is in and
        /// then in every other library steam knows about.
        /// </summary>
        public string ProtonPrefix()
        {
            if (this.config.ProtonPrefix.Length > 0)
            {
                return Directory.Exists(this.config.ProtonPrefix) ? this.config.ProtonPrefix : string.Empty;
            }

            foreach (string prefix in PrefixCandidates(this.config.GamePath))
            {
                if (File.Exists(Path.Combine(prefix, "user.reg")))
                {
                    return prefix;
                }
            }

            return string.Empty;
        }

        private static IEnumerable<string> PrefixCandidates(string gamePath)
        {
            // .../steamapps/common/KillingFloor -> .../steamapps/compatdata/1250/pfx
            if (gamePath.Length > 0 && Path.IsPathRooted(gamePath))
            {
                yield return Path.GetFullPath(Path.Combine(gamePath, "..", "..", "compatdata", AppId, "pfx"));
            }

            foreach (string library in SteamLibraries())
            {
                yield return Path.Combine(library, "steamapps", "compatdata", AppId, "pfx");
            }

            string wine = Environment.GetEnvironmentVariable("WINEPREFIX") ?? string.Empty;
            if (wine.Length > 0)
            {
                yield return wine;
            }
        }

        private void Movies(bool disable)
        {
            try
            {
                string from = ResolveDir(this.config.GamePath, disable ? "Movies" : "_Movies");
                string to = Path.Combine(this.config.GamePath, disable ? "_Movies" : "Movies");

                if (Directory.Exists(from) && !Directory.Exists(to))
                {
                    Debug.WriteLine($"{(disable ? "Disabling" : "Enabling")} movies");
                    Directory.Move(from, to);
                }
            }
            catch (Exception ex)
            {
                // a locked or missing folder is not worth failing a launch over
                TraceLog.Error("movies", ex);
            }
        }

        /// <summary>False when the path points somewhere without a System folder to patch.</summary>
        public bool HasGameFiles => this.config.GamePath.Length > 0 && Directory.Exists(this.SystemPath);
        #endregion

        #region io
        private const string X11Driver = @"[Software\\Wine\\X11 Driver]";
        private const string AppId = "1250";

        /// <summary>A number we are willing to write into the config, or the fallback and a moan.</summary>
        private static string Number(string value, string fallback, int low, int high, List<string> warnings, string what)
        {
            if (int.TryParse(value.Trim(), out int number) && number >= low && number <= high)
            {
                return number.ToString();
            }

            warnings.Add($"Ignored the {what}: \"{value.Trim()}\" is not a number between {low} and {high}.");

            return fallback;
        }

        /// <summary>
        /// Set a key the game may not have written yet, without trampling a key of the same name
        /// somewhere it means something else (MaxClientRate is the net driver and the demo
        /// recorder, CacheSizeMegs is the game and the editor).
        ///
        /// Where the game already keeps it exactly once, that is the one, wherever it lives: KF
        /// inherits from red orchestra, so settings turn up under [ROEngine.ROPlayer] and other
        /// places nobody would guess.  Otherwise the section named here is the one that counts,
        /// and it is created if the game has never written it.
        /// </summary>
        internal static string SetIni(string ini, string section, string key, string value)
        {
            string entry = $"{key}={value}";
            string pattern = $@"^{Regex.Escape(key)}\s*=";

            if (Regex.Matches(ini, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase).Count == 1)
            {
                return PatchIni(ini, key, value);
            }

            string newline = ini.Contains("\r\n") ? "\r\n" : "\n";
            List<string> output = new();
            bool inSection = false;
            bool written = false;

            foreach (string line in ini.Replace("\r\n", "\n").Split('\n'))
            {
                if (line.StartsWith('['))
                {
                    // leaving our section without having written the value, so write it here
                    if (inSection && !written)
                    {
                        output.Insert(EndOfSection(output), entry);
                        written = true;
                    }

                    inSection = line.Trim().Equals(section, StringComparison.OrdinalIgnoreCase);
                }
                else if (inSection && Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase))
                {
                    if (!written)
                    {
                        output.Add(entry);
                        written = true;
                    }

                    continue;
                }

                output.Add(line);
            }

            if (!written)
            {
                int at = EndOfSection(output);
                if (!inSection)
                {
                    output.Insert(at++, string.Empty);
                    output.Insert(at++, section);
                }

                output.Insert(at, entry);
            }

            return string.Join(newline, output);
        }

        /// <summary>Where a new entry goes: after the last setting, above the blank line, not below it.</summary>
        private static int EndOfSection(List<string> output)
        {
            int at = output.Count;
            while (at > 0 && output[at - 1].Length == 0)
            {
                at--;
            }

            return at;
        }

        internal static string GetIni(string ini, string key)
        {
            Match match = Regex.Match(ini, $@"^{Regex.Escape(key)}\s*=([^\r\n]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        /// <summary>
        /// Hang a console command off the end of a bind, or take ours back off, without touching
        /// what is bound to the key: the fov and netspeed fixes ride along on binds people change.
        /// </summary>
        internal static string ChainBind(string ini, string key, string stock, string tail, bool enable)
        {
            string current = GetIni(ini, key);
            if (current.Length == 0)
            {
                current = stock;
            }

            string verb = tail.Split(' ')[0];
            string bare = Regex.Replace(current, $@"\s*\|\s*{Regex.Escape(verb)}\s+\S+", string.Empty, RegexOptions.IgnoreCase).Trim();
            if (bare.Length == 0)
            {
                bare = stock;
            }

            return SetIni(ini, Input, key, enable ? $"{bare} | {tail}" : bare);
        }

        /// <summary>Swap a whole bind between stock and ours, and leave anything else well alone.</summary>
        internal static string SwapBind(string ini, string key, string stock, string ours, bool enable)
        {
            string current = GetIni(ini, key);
            if (current.Length > 0
                && !current.Equals(stock, StringComparison.OrdinalIgnoreCase)
                && !current.Equals(ours, StringComparison.OrdinalIgnoreCase))
            {
                return ini;
            }

            return SetIni(ini, Input, key, enable ? ours : stock);
        }

        /// <summary>The directory as the disk spells it, which on linux is not always as we do.</summary>
        internal static string ResolveDir(string parent, string name)
        {
            string exact = Path.Combine(parent, name);
            if (parent.Length == 0 || Directory.Exists(exact) || !Directory.Exists(parent))
            {
                return exact;
            }

            foreach (string directory in Directory.EnumerateDirectories(parent))
            {
                if (string.Equals(Path.GetFileName(directory), name, StringComparison.OrdinalIgnoreCase))
                {
                    return directory;
                }
            }

            return exact;
        }

        internal static string ResolveFile(string parent, string name)
        {
            string exact = Path.Combine(parent, name);
            if (parent.Length == 0 || File.Exists(exact) || !Directory.Exists(parent))
            {
                return exact;
            }

            foreach (string file in Directory.EnumerateFiles(parent))
            {
                if (string.Equals(Path.GetFileName(file), name, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            return exact;
        }

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
            // anchored to the start of a line, otherwise short keys ("Q") eat other entries
            // ("Quality=3").  case insensitive because the game does not care either.
            return Regex.Replace(ini, $@"^{Regex.Escape(key)}\s*=[^\r\n]*", $"{key}={value}", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        private static string ReadIni(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static void WriteIni(string path, string ini)
        {
            // a relative path means we have no game directory, and "System" in the working
            // directory is not something to go writing an ini into
            if (!Path.IsPathRooted(path) || !Directory.Exists(Path.GetDirectoryName(path)))
            {
                return;
            }

            // people mark these read only to stop the game clobbering them, which would otherwise
            // throw straight through the launch command.  put it back afterwards, it was deliberate
            bool readOnly = false;
            if (File.Exists(path))
            {
                FileAttributes attributes = File.GetAttributes(path);
                readOnly = attributes.HasFlag(FileAttributes.ReadOnly);
                if (readOnly)
                {
                    File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                }
            }

            // write beside it and swap, so a crash or a full disk cannot leave a half written
            // config where the game (and the restore button) expects one
            string temp = path + ".tmp";
            File.WriteAllText(temp, ini);
            File.Move(temp, path, true);

            if (readOnly)
            {
                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
            }
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
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // /proc entries come and go while we walk them, and some are not ours to read
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
                string game = ResolveDir(Path.Combine(library, "steamapps", "common"), "KillingFloor");
                if (Directory.Exists(ResolveDir(game, "System")))
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
