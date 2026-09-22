using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KFLauncher.Models
{
    /// <summary>Run with --selftest.  Covers the server list parsers and the ini patcher.</summary>
    internal static class SelfTest
    {
        public static int Run()
        {
            int failed = 0;

            // steam web api server list
            const string json = """
            {"response":{"servers":[
              {"addr":"1.2.3.4:7708","gameport":7707,"name":"Some KF Server","appid":1250,"players":3,"max_players":6,"map":"KF-BioticsLab"},
              {"addr":"not an address","gameport":7707,"name":"Broken","players":0,"max_players":6,"map":"KF-Manor"}
            ]}}
            """;

            List<ServerInfo> servers = ServerBrowser.ParseServerList(json);
            failed += Check(servers.Count == 1, $"unparsable addresses are skipped, got {servers.Count}");
            failed += Check(servers[0].Name == "Some KF Server", $"name parsed, got {servers[0].Name}");
            failed += Check(servers[0].Slots == "3/6", $"players parsed, got {servers[0].Slots}");
            failed += Check(servers[0].PingText == "—", "ping is unknown until the server answers");
            failed += Check(servers[0].LaunchUri == "steam://run/1250//1.2.3.4:7707/", $"launch uri names the app and the game port, got {servers[0].LaunchUri}");
            failed += Check(servers[0].ConsoleCommand == "open 1.2.3.4:7707", $"console command uses the game port, got {servers[0].ConsoleCommand}");
            failed += Check(ServerBrowser.ParseServerList("""{"response":{}}""").Count == 0, "empty response handled");

            // a relay url is used as given, otherwise we go to steam ourselves with the key
            failed += Check(ServerBrowser.BuildListUrl("https://vps/kf-servers.json", "key") == "https://vps/kf-servers.json", "relay url wins");
            failed += Check(ServerBrowser.BuildListUrl("", "abc123").Contains("key=abc123"), "api key url carries the key");
            failed += Check(ServerBrowser.BuildListUrl("", "abc123").Contains("%5Cappid%5C1250"), "api key url filters on the app id");

            // A2S_INFO reply with the extra data field carrying the game port
            List<byte> info = [0xFF, 0xFF, 0xFF, 0xFF, 0x49, 0x11];
            Str(info, "KF Server");
            Str(info, "KF-BioticsLab");
            Str(info, "KFGame");
            Str(info, "Killing Floor");
            info.AddRange([0xE2, 0x04, 5, 6, 0, (byte)'d', (byte)'w', 0, 1]);
            Str(info, "1.0");
            info.AddRange([0x80, 0x1B, 0x1E]);

            // and the same reply with a colour code in the name, which is what servers really send
            List<byte> coloured = [0xFF, 0xFF, 0xFF, 0xFF, 0x49, 0x11, 0x1B, 0xF8, 0x40, 0x40];
            coloured.AddRange(info.GetRange(6, info.Count - 6));
            A2SInfo? colourful = ServerBrowser.ParseInfo(coloured.ToArray(), new IPEndPoint(IPAddress.Loopback, 27015), 0);
            failed += Check(colourful?.Name == "KF Server", $"colour codes are stripped from names, got {colourful?.Name}");

            // a name in latin1, which is not valid utf8 and used to come through as diamonds
            List<byte> latin1 = [0xFF, 0xFF, 0xFF, 0xFF, 0x49, 0x11, (byte)'W', (byte)'S', 0xBB, (byte)'6', (byte)'0', 0x00];
            latin1.AddRange(info.GetRange(info.IndexOf(0) + 1, info.Count - info.IndexOf(0) - 1));
            A2SInfo? mixed = ServerBrowser.ParseInfo(latin1.ToArray(), new IPEndPoint(IPAddress.Loopback, 27015), 0);
            failed += Check(mixed?.Name == "WS\u00BB60", $"a name that is not utf8 is read as latin1, got {mixed?.Name}");

            A2SInfo? parsed = ServerBrowser.ParseInfo(info.ToArray(), new IPEndPoint(IPAddress.Loopback, 27015), 42);
            failed += Check(parsed is not null, "info reply parses");
            failed += Check(parsed?.Name == "KF Server", $"name parsed, got {parsed?.Name}");
            failed += Check(parsed?.Map == "KF-BioticsLab", $"map parsed, got {parsed?.Map}");
            failed += Check(parsed?.Players == 5 && parsed?.MaxPlayers == 6, $"players parsed, got {parsed?.Players}/{parsed?.MaxPlayers}");
            failed += Check(parsed?.GamePort == 7707, $"game port from extra data, got {parsed?.GamePort}");
            failed += Check(ServerBrowser.ParseInfo([0xFF, 0xFF], new IPEndPoint(IPAddress.Loopback, 1), 0) is null, "short reply rejected");

            // a reply cut off right where the extra data field would start: no game port, and the
            // query port must not be mistaken for one, that is a connect to the wrong port
            A2SInfo? noEdf = ServerBrowser.ParseInfo(info.ToArray().AsSpan(0, info.Count - 4), new IPEndPoint(IPAddress.Loopback, 27015), 0);
            failed += Check(noEdf?.GamePort == 0, $"an absent game port stays unknown, got {noEdf?.GamePort}");

            ServerInfo server = new() { Query = new IPEndPoint(IPAddress.Loopback, 27015), Name = "old", GamePort = 7707 };
            server.Apply(noEdf!);
            failed += Check(server.GamePort == 7707, $"an unknown game port leaves the saved one alone, got {server.GamePort}");
            failed += Check(server.Name == "KF Server" && server.MaxPlayers == 6, "the live reply refreshes name and slots");

            server.Apply(parsed!);
            failed += Check(server.GamePort == 7707, "a reported game port is taken");

            // favorites are copies, so refreshing the big list cannot reach into the saved ones
            server.IsFavorite = true;
            ServerInfo copy = server.Clone();
            server.Players = 99;
            failed += Check(copy.IsFavorite && copy.Players != 99 && copy.Address == server.Address, "a favorite is its own copy");

            // fov lives in the config and rides along with the forward bind, since the game resets
            // the view at trader time
            string user = "DesiredFOV=85.000000\r\nDefaultFOV=85.000000\r\nW=MoveForward\r\nUp=MoveForward\r\n";
            string withFov = KFConfig.PatchIni(KFConfig.PatchIni(user, "W", "MoveForward | fov 95"), "DefaultFOV", "95");
            failed += Check(withFov.Contains("W=MoveForward | fov 95"), "forward bind carries the fov command");
            failed += Check(withFov.Contains("Up=MoveForward\r\n"), "the other bind for the same command is untouched");
            failed += Check(withFov.Contains("DefaultFOV=95"), "default fov set");

            // wine registry patching for the pointer grab
            const string section = @"[Software\\Wine\\X11 Driver]";
            string reg = section + " 123\n#time=abc\n\"GrabFullscreen\"=\"N\"\n\n[Software\\\\Wine\\\\Other] 1\n\"Keep\"=\"Me\"\n";

            string patchedReg = KFConfig.PatchReg(reg, section, "GrabFullscreen", "Y");
            failed += Check(patchedReg.Contains("\"GrabFullscreen\"=\"Y\""), "existing value replaced");
            failed += Check(!patchedReg.Contains("\"GrabFullscreen\"=\"N\""), "old value gone");
            failed += Check(patchedReg.Contains("\"Keep\"=\"Me\""), "other sections untouched");

            patchedReg = KFConfig.PatchReg(reg, section, "DXGrab", "Y");
            failed += Check(patchedReg.IndexOf("\"DXGrab\"", StringComparison.Ordinal) < patchedReg.IndexOf("[Software\\\\Wine\\\\Other]", StringComparison.Ordinal), "new value lands inside its own section");
            failed += Check(KFConfig.PatchReg("", section, "DXGrab", "Y").Contains(section), "missing section is created");

            // spotting the running game by its command line, which is what proton leaves us
            failed += Check(KFConfig.IsGameCommandLine("/home/e/.steam/steam/steamapps/common/KillingFloor/System/KillingFloor.exe\0-nostartupmovies"), "proton command line matched");
            failed += Check(KFConfig.IsGameCommandLine("/games/steamapps/common/KillingFloor/kf"), "native linux binary in the game dir matched");
            failed += Check(!KFConfig.IsGameCommandLine("/usr/bin/steam"), "steam itself is not the game");
            failed += Check(!KFConfig.IsGameCommandLine("/steamapps/common/killingfloor2/Binaries/Win64/KFGame.exe"), "killing floor 2 is not the game");

            // and by process name everywhere else, where the name is not cut short
            failed += Check(KFConfig.IsGameProcess("KillingFloor.ex"), "truncated proton process matched");
            failed += Check(KFConfig.IsGameProcess("KillingFloor.exe"), "windows process matched");
            failed += Check(KFConfig.IsGameProcess("killingfloor"), "bare name matched");
            failed += Check(!KFConfig.IsGameProcess("KillingFloorLau"), "our own launcher is not the game");
            failed += Check(!KFConfig.IsGameProcess("KFGame"), "killing floor 2 is not the game");

            // A2S_PLAYER reply
            List<byte> players = [0xFF, 0xFF, 0xFF, 0xFF, 0x44, 2, 0];
            Str(players, "Viking");
            players.AddRange([0xDE, 0x0A, 0, 0]);                       // score 2782
            players.AddRange(BitConverter.GetBytes(1359.0f));           // 22:39 connected
            players.Add(1);
            Str(players, "hdsee");
            players.AddRange([0xC6, 0x03, 0, 0]);                       // score 966
            players.AddRange(BitConverter.GetBytes(float.NaN));         // servers really do send this

            List<PlayerInfo>? parsedPlayers = ServerBrowser.ParsePlayers(players.ToArray());
            failed += Check(parsedPlayers?.Count == 2, $"both players parsed, got {parsedPlayers?.Count}");
            failed += Check(parsedPlayers?[0].Name == "Viking" && parsedPlayers[0].Score == 2782, "name and score parsed");
            failed += Check(parsedPlayers?[0].TimeText == "22:39", $"time formatted, got {parsedPlayers?[0].TimeText}");
            failed += Check(parsedPlayers?[1].TimeText == "0:00", "a nan time does not blow up");
            failed += Check(ServerBrowser.ParsePlayers([0xFF, 0xFF, 0xFF, 0xFF, 0x44, 9, 0]) is { Count: 0 }, "a lying count does not run off the end");

            // favorites are a record inside the settings file, which json has to round trip
            JsonConfig config = new() { Favorites = [new Favorite("1.2.3.4:7708", 7707, "hunter2")] };
            JsonConfig? reloaded = JsonSerializer.Deserialize<JsonConfig>(JsonSerializer.Serialize(config));
            failed += Check(reloaded?.Favorites is [{ Query: "1.2.3.4:7708", GamePort: 7707, Password: "hunter2" }], "favorites survive a save and load");

            // and one saved before there were passwords still loads
            JsonConfig? old = JsonSerializer.Deserialize<JsonConfig>("""{"Favorites":[{"Query":"1.2.3.4:7708","GamePort":7707}]}""");
            failed += Check(old?.Favorites is [{ GamePort: 7707, Password: null }], "a favorite saved by an older build still loads");

            // the difficulty the server list carries, as "dedicated;flag;difficulty"
            failed += Check(ServerBrowser.ParseDifficulty("d;0;2") == 2, "difficulty read from the list tag");
            failed += Check(ServerBrowser.ParseDifficulty("l;1;4") == 4, "difficulty read off a listen server too");
            failed += Check(ServerBrowser.ParseDifficulty("nonsense") == -1, "an unreadable tag means unknown");
            failed += Check(ServerBrowser.ParseDifficulty(null) == -1, "a missing tag means unknown");

            // a player list too big for one packet comes back in numbered pieces
            byte[] whole = [0xFF, 0xFF, 0xFF, 0xFF, 0x44, 1, 0, (byte)'a', 0, 1, 2, 3, 4, 5, 6, 7, 8];
            byte[] head = [0xFE, 0xFF, 0xFF, 0xFF, 1, 0, 0, 0, 2, 0, 0x00, 0x04];
            byte[] tail = [0xFE, 0xFF, 0xFF, 0xFF, 1, 0, 0, 0, 2, 1, 0x00, 0x04];
            byte[]? glued = ServerBrowser.Reassemble([[.. head, .. whole[..9]], [.. tail, .. whole[9..]]]);
            failed += Check(glued is not null && glued.SequenceEqual(whole), "a split reply is glued back together");

            // some servers leave the payload size out, which only the first piece gives away
            byte[]? noSize = ServerBrowser.Reassemble([[.. head[..10], .. whole[..9]], [.. tail[..10], .. whole[9..]]]);
            failed += Check(noSize is not null && noSize.SequenceEqual(whole), "a split reply without the size field is glued too");
            failed += Check(ServerBrowser.Reassemble([[.. head, .. whole]]) is null, "a piece missing means no reply at all");

            // keys the game has not written yet have to be added, not quietly skipped
            const string sections = "[Engine.Input]\r\nQ=QuickHeal\r\n\r\n[Engine.PlayerController]\r\nDesiredFOV=85\r\n";
            string added = KFConfig.SetIni(sections, "[Engine.PlayerController]", "bNeverSwitchOnPickup", "True");
            failed += Check(added.Contains("DesiredFOV=85\r\nbNeverSwitchOnPickup=True"), "a missing key is added under the last setting in its section");
            failed += Check(added.IndexOf("bNeverSwitchOnPickup", StringComparison.Ordinal) > added.IndexOf("[Engine.PlayerController]", StringComparison.Ordinal), "and lands inside it");
            failed += Check(added.Contains("Q=QuickHeal"), "without disturbing the rest");

            string invented = KFConfig.SetIni(sections, "[KFMod.KFHumanPawn]", "bUseBlurEffect", "False");
            failed += Check(invented.Contains("[KFMod.KFHumanPawn]\r\nbUseBlurEffect=False"), "a missing section is created for it");
            failed += Check(KFConfig.SetIni(sections, "[Engine.PlayerController]", "DesiredFOV", "95").Contains("DesiredFOV=95"), "an existing key is just set");
            failed += Check(KFConfig.GetIni(sections, "Q") == "QuickHeal", "a value can be read back out");

            // binds are the users, we only hang our own command off the end of one
            string chained = KFConfig.ChainBind("W=MoveForward | crouch\n", "W", "MoveForward", "fov 95", true);
            failed += Check(chained.Contains("W=MoveForward | crouch | fov 95"), $"the fov rides along on whatever is bound, got {KFConfig.GetIni(chained, "W")}");
            failed += Check(KFConfig.GetIni(KFConfig.ChainBind(chained, "W", "MoveForward", "fov 95", false), "W") == "MoveForward | crouch", "and comes back off without taking the rest with it");
            failed += Check(KFConfig.GetIni(KFConfig.ChainBind(chained, "W", "MoveForward", "fov 110", true), "W") == "MoveForward | crouch | fov 110", "a changed fov replaces the old one");

            string custom = "Q=say hello\n";
            failed += Check(KFConfig.SwapBind(custom, "Q", "QuickHeal", "quickheal thing", true) == custom, "a bind we did not put there is left alone");
            failed += Check(KFConfig.GetIni(KFConfig.SwapBind("Q=QuickHeal\n", "Q", "QuickHeal", "quickheal thing", true), "Q") == "quickheal thing", "a stock bind is ours to take");
            failed += Check(KFConfig.GetIni(KFConfig.SwapBind("Q=quickheal thing\n", "Q", "QuickHeal", "quickheal thing", false), "Q") == "QuickHeal", "and to give back");

            // ini patching only touches whole keys at the start of a line
            string ini = "[Engine]\r\nMaxClientFrameRate=60\r\nQuality=3\r\nQ=QuickHeal\r\nMouseSamplingTime = 0.05\r\n";
            failed += Check(KFConfig.PatchIni(ini, "MaxClientFrameRate", "200").Contains("MaxClientFrameRate=200"), "key patched");
            failed += Check(KFConfig.PatchIni(ini, "MouseSamplingTime", "0.001").Contains("MouseSamplingTime=0.001"), "spaced key patched");

            string patched = KFConfig.PatchIni(ini, "Q", "getweapon syringe");
            failed += Check(patched.Contains("Q=getweapon syringe"), "short key patched");
            failed += Check(patched.Contains("Quality=3"), "longer key with the same prefix left alone");
            failed += Check(KFConfig.PatchIni(ini, "NotThere", "1") == ini, "missing key changes nothing");

            failed += PatchRun();

            Console.WriteLine(failed == 0 ? "selftest OK" : $"selftest FAILED ({failed})");

            return failed == 0 ? 0 : 1;
        }

        /// <summary>
        /// The whole patcher over a throwaway copy of the stock config: on, then off again, which
        /// is the part that used to quietly do nothing for keys the game had not written yet.
        /// </summary>
        private static int PatchRun()
        {
            int failed = 0;
            string game = Path.Combine(Path.GetTempPath(), $"kflauncher-selftest-{Environment.ProcessId}");
            Directory.CreateDirectory(Path.Combine(game, "System"));

            try
            {
                File.WriteAllText(Path.Combine(game, "System", "KillingFloor.ini"), DefaultConfigs.KillingFloorIni);
                File.WriteAllText(Path.Combine(game, "System", "User.ini"), DefaultConfigs.UserIni);

                JsonConfig settings = new() { GamePath = game, SetFov = true, Fov = "95", BetterAudio = true, DisableBlur = true, NoSwitchOnPickup = true };
                KFConfig patcher = new(settings);

                failed += Check(patcher.ApplySetPatches().Length == 0, "a sane config patches without complaint");

                string kf = patcher.KillingFloorIni;
                string user = patcher.UserIni;

                failed += Check(KFConfig.GetIni(kf, "ReduceMouseLag") == "False", "the mouse lag flush is turned off, not on");
                failed += Check(KFConfig.GetIni(kf, "MaxClientRate") == "15000", "the rate caps are left at stock");
                failed += Check(KFConfig.GetIni(kf, "MaxInternetClientRate") == "10000", "both of them, ticked or not");
                failed += Check(KFConfig.GetIni(user, "ConfiguredInternetSpeed") == "15000", "and the speed asks for what the caps allow");
                failed += Check(KFConfig.GetIni(user, "MouseSmoothingMode") == "0", "a key the game has not written yet is still set");
                failed += Check(KFConfig.GetIni(user, "bUseBlurEffect") == "False", "a key in a section that does not exist yet is still set");
                failed += Check(KFConfig.GetIni(user, "bNeverSwitchOnPickup") == "True", "pickup switching off");
                failed += Check(KFConfig.GetIni(kf, "UseEAX") == "True" && KFConfig.GetIni(kf, "Channels") == "64", "audio opened up");
                failed += Check(KFConfig.GetIni(user, "W") == "MoveForward | fov 95", $"the fov rides on the forward bind, got {KFConfig.GetIni(user, "W")}");
                failed += Check(KFConfig.GetIni(user, "LeftMouse") == "Fire | netspeed 30000", "and the netspeed on the mouse");

                // now every toggle the other way, which has to put the stock values back
                settings.SetFov = false;
                settings.BetterAudio = false;
                settings.DisableBlur = false;
                settings.NoSwitchOnPickup = false;
                settings.FixMouseInput = false;
                settings.ImproveNetcode = false;
                settings.QuickHeal = false;
                patcher.ApplySetPatches();

                kf = patcher.KillingFloorIni;
                user = patcher.UserIni;

                failed += Check(KFConfig.GetIni(kf, "ReduceMouseLag") == "True", "unticking puts the renderer back");
                failed += Check(KFConfig.GetIni(user, "ConfiguredInternetSpeed") == "9636", "and the configured speed");
                failed += Check(KFConfig.GetIni(user, "W") == "MoveForward", "and takes the fov back off the bind");
                failed += Check(KFConfig.GetIni(user, "LeftMouse") == "Fire", "and the netspeed off the mouse");
                failed += Check(KFConfig.GetIni(user, "Q") == "QuickHeal", "and the quickheal bind");
                failed += Check(KFConfig.GetIni(kf, "UseEAX") == "False", "and the audio");

                // a number nobody can use is left out, and said out loud
                settings.SetFov = true;
                settings.Fov = "wide";
                string warning = patcher.ApplySetPatches();
                failed += Check(warning.Contains("field of view"), $"a fov that is not a number is refused, got \"{warning}\"");
                failed += Check(KFConfig.GetIni(patcher.UserIni, "DesiredFOV") == "85.000000", "and the stock value used instead");
                failed += Check(KFConfig.GetIni(patcher.UserIni, "W") == "MoveForward", "and nothing chained onto the bind either");

                // a key that means one thing in one section and something else in another
                failed += Check(KFConfig.GetIni(patcher.KillingFloorIni, "MaxClientRate") == "15000", "the net driver rate is the one that was set");
                string demo = KFConfig.SetIni("[IpDrv.TcpNetDriver]\nMaxClientRate=15000\n\n[Engine.DemoRecDriver]\nMaxClientRate=25000\n", "[IpDrv.TcpNetDriver]", "MaxClientRate", "20000");
                failed += Check(demo.Contains("MaxClientRate=20000") && demo.Contains("MaxClientRate=25000"), "the demo recorder keeps its own");
            }
            finally
            {
                Directory.Delete(game, recursive: true);
            }

            return failed;
        }

        private static void Str(List<byte> buffer, string value)
        {
            buffer.AddRange(Encoding.UTF8.GetBytes(value));
            buffer.Add(0);
        }

        private static int Check(bool condition, string what)
        {
            Console.WriteLine($"{(condition ? "ok  " : "FAIL")} {what}");
            return condition ? 0 : 1;
        }
    }
}
