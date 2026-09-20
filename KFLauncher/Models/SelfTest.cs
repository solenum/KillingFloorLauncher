using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

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

            // A2S_INFO reply with the extra data field carrying the game port
            List<byte> info = [0xFF, 0xFF, 0xFF, 0xFF, 0x49, 0x11];
            Str(info, "KF Server");
            Str(info, "KF-BioticsLab");
            Str(info, "KFGame");
            Str(info, "Killing Floor");
            info.AddRange([0xE2, 0x04, 5, 6, 0, (byte)'d', (byte)'w', 0, 1]);
            Str(info, "1.0");
            info.AddRange([0x80, 0x1B, 0x1E]);

            A2SInfo? parsed = ServerBrowser.ParseInfo(info.ToArray(), new IPEndPoint(IPAddress.Loopback, 27015), 42);
            failed += Check(parsed is not null, "info reply parses");
            failed += Check(parsed?.Name == "KF Server", $"name parsed, got {parsed?.Name}");
            failed += Check(parsed?.Map == "KF-BioticsLab", $"map parsed, got {parsed?.Map}");
            failed += Check(parsed?.Players == 5 && parsed?.MaxPlayers == 6, $"players parsed, got {parsed?.Players}/{parsed?.MaxPlayers}");
            failed += Check(parsed?.GamePort == 7707, $"game port from extra data, got {parsed?.GamePort}");
            failed += Check(ServerBrowser.ParseInfo([0xFF, 0xFF], new IPEndPoint(IPAddress.Loopback, 1), 0) is null, "short reply rejected");

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

            // ini patching only touches whole keys at the start of a line
            string ini = "[Engine]\r\nMaxClientFrameRate=60\r\nQuality=3\r\nQ=QuickHeal\r\nMouseSamplingTime = 0.05\r\n";
            failed += Check(KFConfig.PatchIni(ini, "MaxClientFrameRate", "200").Contains("MaxClientFrameRate=200"), "key patched");
            failed += Check(KFConfig.PatchIni(ini, "MouseSamplingTime", "0.001").Contains("MouseSamplingTime=0.001"), "spaced key patched");

            string patched = KFConfig.PatchIni(ini, "Q", "getweapon syringe");
            failed += Check(patched.Contains("Q=getweapon syringe"), "short key patched");
            failed += Check(patched.Contains("Quality=3"), "longer key with the same prefix left alone");
            failed += Check(KFConfig.PatchIni(ini, "NotThere", "1") == ini, "missing key changes nothing");

            Console.WriteLine(failed == 0 ? "selftest OK" : $"selftest FAILED ({failed})");

            return failed == 0 ? 0 : 1;
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
