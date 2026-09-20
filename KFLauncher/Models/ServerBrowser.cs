using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KFLauncher.Models
{
    /// <summary>One server in the browser.  Player count, map and ping refresh in place.</summary>
    public partial class ServerInfo : ObservableObject
    {
        public required IPEndPoint Query { get; init; }

        public required string Name { get; init; }

        public required int MaxPlayers { get; init; }

        public required ushort GamePort { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Slots))]
        private int players;

        [ObservableProperty]
        private string map = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Locked))]
        private bool passworded;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PingText))]
        private int ping = -1;

        public string Slots => $"{this.Players}/{this.MaxPlayers}";

        /// <summary>A padlock for the grid, so nobody wastes a connect on a server they cannot enter.</summary>
        public string Locked => this.Passworded ? "🔒" : string.Empty;

        public string PingText => this.Ping < 0 ? "—" : this.Ping.ToString();

        public string Address => $"{this.Query.Address}:{this.GamePort}";

        /// <summary>Starts the game on the server.  Ignored by steam if the game already runs.</summary>
        // steam://connect makes steam query the server for the app id, and asking the game port for
        // it gets "app id specified by server is invalid", so name the app ourselves and hand the
        // address over as a launch argument, which is how unreal opens a url.
        public string LaunchUri => $"steam://run/{ServerBrowser.AppId}//{this.Query.Address}:{this.GamePort}/";

        /// <summary>For a game that is already up: press ~ and paste.  Steam has no route in,
        /// steam://connect asks the server for its app id and KF servers do not report a usable
        /// one, on either port ("app id specified by server is invalid").</summary>
        public string ConsoleCommand => $"open {this.Query.Address}:{this.GamePort}";
    }

    /// <summary>One player on a server, from A2S_PLAYER.</summary>
    public record PlayerInfo(string Name, int Score, TimeSpan Time)
    {
        public string TimeText => this.Time.TotalHours >= 1
            ? $"{(int)this.Time.TotalHours}:{this.Time.Minutes:00}:{this.Time.Seconds:00}"
            : $"{this.Time.Minutes}:{this.Time.Seconds:00}";
    }

    /// <summary>Live values straight off a server, used to refresh a <see cref="ServerInfo"/>.</summary>
    internal record A2SInfo(string Name, string Map, int Players, int MaxPlayers, int Bots, bool Passworded, bool Vac, ushort GamePort, int Ping);

    /// <summary>
    /// Server list from the steam web api, live player counts and ping over A2S.
    /// The old keyless udp master (hl2master.steampowered.com) no longer resolves, hence the api key.
    /// </summary>
    internal static class ServerBrowser
    {
        public const int AppId = 1250;
        public const string ApiKeyUrl = "https://steamcommunity.com/dev/apikey";

        /// <summary>
        /// Point this at a host running tools/kf-serverlist.sh and everyone who downloads the
        /// launcher gets the server list without an api key of their own.  Empty ships the api key
        /// route, see the README.
        /// </summary>
        public const string DefaultListUrl = "https://everparser.com/kf-servers.json";

        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(20) };
        private static readonly byte[] InfoRequest = [0xFF, 0xFF, 0xFF, 0xFF, 0x54, .. Encoding.ASCII.GetBytes("Source Engine Query\0")];
        private static readonly byte[] PlayerRequest = [0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF];

        /// <summary>Every KF server steam knows about.  Throws <see cref="HttpRequestException"/> on a bad key.</summary>
        public static async Task<List<ServerInfo>> FetchListAsync(string listUrl, string apiKey, CancellationToken ct = default)
        {
            string json = await Http.GetStringAsync(BuildListUrl(listUrl, apiKey), ct);

            return ParseServerList(json);
        }

        /// <summary>A relay serves steams reply unchanged, so both routes parse the same.</summary>
        internal static string BuildListUrl(string listUrl, string apiKey)
        {
            if (listUrl.Length > 0)
            {
                return listUrl;
            }

            string filter = Uri.EscapeDataString($"\\appid\\{AppId}");

            return $"https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={Uri.EscapeDataString(apiKey)}&limit=5000&filter={filter}";
        }

        internal static List<ServerInfo> ParseServerList(string json)
        {
            List<ServerInfo> servers = new();

            using JsonDocument document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("response", out JsonElement response)
                || !response.TryGetProperty("servers", out JsonElement list))
            {
                return servers;
            }

            foreach (JsonElement server in list.EnumerateArray())
            {
                string? address = server.TryGetProperty("addr", out JsonElement addr) ? addr.GetString() : null;
                if (address is null || !IPEndPoint.TryParse(address, out IPEndPoint? query))
                {
                    continue;
                }

                servers.Add(new ServerInfo
                {
                    Query = query,
                    Name = (server.TryGetProperty("name", out JsonElement name) ? name.GetString() : null) ?? address,
                    MaxPlayers = server.TryGetProperty("max_players", out JsonElement max) ? max.GetInt32() : 0,
                    GamePort = server.TryGetProperty("gameport", out JsonElement port) ? (ushort)port.GetInt32() : (ushort)query.Port,
                    Players = server.TryGetProperty("players", out JsonElement players) ? players.GetInt32() : 0,
                    Map = (server.TryGetProperty("map", out JsonElement map) ? map.GetString() : null) ?? string.Empty,
                });
            }

            return servers;
        }

        /// <summary>Query one server directly for its current state, timing the round trip as ping.</summary>
        public static async Task<A2SInfo?> QueryAsync(IPEndPoint server, int timeoutMs = 1500, CancellationToken ct = default)
        {
            try
            {
                using UdpClient udp = new();
                udp.Connect(server);

                Stopwatch sw = Stopwatch.StartNew();
                await udp.SendAsync(InfoRequest, ct);
                byte[]? reply = await ReceiveAsync(udp, timeoutMs, ct);

                // servers answer with a challenge that has to be echoed back
                if (reply is { Length: >= 9 } && reply[4] == 0x41)
                {
                    byte[] challenged = [.. InfoRequest, reply[5], reply[6], reply[7], reply[8]];
                    await udp.SendAsync(challenged, ct);
                    reply = await ReceiveAsync(udp, timeoutMs, ct);
                }

                return reply is null ? null : ParseInfo(reply, server, (int)sw.ElapsedMilliseconds);
            }
            catch (Exception ex) when (ex is SocketException or OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>Who is on the server right now.  Null when it does not answer.</summary>
        public static async Task<List<PlayerInfo>?> QueryPlayersAsync(IPEndPoint server, int timeoutMs = 2000, CancellationToken ct = default)
        {
            try
            {
                using UdpClient udp = new();
                udp.Connect(server);

                await udp.SendAsync(PlayerRequest, ct);
                byte[]? reply = await ReceiveAsync(udp, timeoutMs, ct);

                if (reply is { Length: >= 9 } && reply[4] == 0x41)
                {
                    byte[] challenged = [0xFF, 0xFF, 0xFF, 0xFF, 0x55, reply[5], reply[6], reply[7], reply[8]];
                    await udp.SendAsync(challenged, ct);
                    reply = await ReceiveAsync(udp, timeoutMs, ct);
                }

                return reply is null ? null : ParsePlayers(reply);
            }
            catch (Exception ex) when (ex is SocketException or OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>A2S_PLAYER reply: a count, then index, name, score and seconds connected each.</summary>
        internal static List<PlayerInfo>? ParsePlayers(ReadOnlySpan<byte> data)
        {
            if (data.Length < 6 || data[0] != 0xFF || data[4] != 0x44)
            {
                return null;
            }

            List<PlayerInfo> players = new();
            int count = data[5];
            int i = 6;

            for (int player = 0; player < count; player++)
            {
                // the index byte is meaningless on most servers, and the count itself can lie
                if (i + 1 >= data.Length)
                {
                    break;
                }

                i++;
                string name = ReadString(data, ref i);
                if (i + 8 > data.Length)
                {
                    break;
                }

                int score = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(i, 4));
                i += 4;
                float seconds = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(i, 4));
                i += 4;

                // servers have been seen sending nan and negative times
                players.Add(new PlayerInfo(name, score, TimeSpan.FromSeconds(float.IsFinite(seconds) && seconds > 0 ? seconds : 0)));
            }

            return players;
        }

        /// <summary>A2S_INFO reply, see https://developer.valvesoftware.com/wiki/Server_queries </summary>
        internal static A2SInfo? ParseInfo(ReadOnlySpan<byte> data, IPEndPoint server, int ping)
        {
            // ponytail: split replies (0xFFFFFFFE) are dropped, KF1's info payload fits one packet
            if (data.Length < 20 || data[0] != 0xFF || data[4] != 0x49)
            {
                return null;
            }

            try
            {
                int i = 5;
                i++;                                    // protocol
                string name = ReadString(data, ref i);
                string map = ReadString(data, ref i);
                ReadString(data, ref i);                // folder
                ReadString(data, ref i);                // game
                i += 2;                                 // appid
                int players = data[i++];
                int maxPlayers = data[i++];
                int bots = data[i++];
                i += 2;                                 // server type, environment
                bool passworded = data[i++] != 0;
                bool vac = data[i++] != 0;
                ReadString(data, ref i);                // version

                // the game port only comes through in the optional extra data field
                ushort gamePort = (ushort)server.Port;
                if (i < data.Length)
                {
                    byte edf = data[i++];
                    if ((edf & 0x80) != 0 && i + 2 <= data.Length)
                    {
                        gamePort = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(i, 2));
                    }
                }

                return new A2SInfo(name, map, players, maxPlayers, bots, passworded, vac, gamePort, ping);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private static string ReadString(ReadOnlySpan<byte> data, ref int i)
        {
            int start = i;
            while (i < data.Length && data[i] != 0)
            {
                i++;
            }

            string value = Encoding.UTF8.GetString(data.Slice(start, i - start));
            i++;

            return value;
        }

        private static async Task<byte[]?> ReceiveAsync(UdpClient udp, int timeoutMs, CancellationToken ct)
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(timeoutMs);

            try
            {
                UdpReceiveResult result = await udp.ReceiveAsync(timeout.Token);
                return result.Buffer;
            }
            catch (Exception ex) when (ex is OperationCanceledException or SocketException)
            {
                return null;
            }
        }
    }
}
