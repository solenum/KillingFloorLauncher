using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Linq;
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

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Slots))]
        private int maxPlayers;

        /// <summary>The port the game listens on, which is not always the one we query.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Address))]
        private ushort gamePort;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FavoriteIcon))]
        private bool isFavorite;

        /// <summary>Handed to the game as a url option when the server asks for one.</summary>
        [ObservableProperty]
        private string password = string.Empty;

        /// <summary>0 to 4 off the server list tag, beginner up to hell on earth.  -1 is unknown.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DifficultyText))]
        private int difficulty = -1;

        /// <summary>Off unreals own query, not steam.  0 is unknown.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WaveText))]
        private int wave;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WaveText))]
        private int finalWave;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Details))]
        private string version = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Details))]
        private bool dedicated = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Details))]
        private bool secure;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Details))]
        private int bots;

        /// <summary>"l" or "w", as steam reports the box the server runs on.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Details))]
        private string os = string.Empty;

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

        public string WaveText => this.FinalWave > 0 ? $"{this.Wave}/{this.FinalWave}" : string.Empty;

        public string DifficultyText => this.Difficulty switch
        {
            0 => "Beginner",
            1 => "Normal",
            2 => "Hard",
            3 => "Suicidal",
            4 => "Hell on Earth",
            _ => string.Empty,
        };

        /// <summary>The small print for the details panel, everything steam knows and we do not show.</summary>
        public string Details => string.Join("  ·  ", new[]
        {
            this.Version.Length > 0 ? $"v{this.Version}" : string.Empty,
            this.Dedicated ? "dedicated" : "listen server",
            this.Secure ? "VAC" : "no VAC",
            this.Os switch { "l" => "linux", "w" => "windows", _ => string.Empty },
            this.Bots > 0 ? $"{this.Bots} bots" : string.Empty,
        }.Where(part => part.Length > 0));

        /// <summary>The star in the first column, filled once the server is a favorite.</summary>
        public string FavoriteIcon => this.IsFavorite ? "\u2605" : "\u2606";

        /// <summary>Copy for the favorites tab, so it refreshes on its own without the big list.</summary>
        public ServerInfo Clone() => new()
        {
            Query = this.Query,
            Name = this.Name,
            Map = this.Map,
            GamePort = this.GamePort,
            Players = this.Players,
            MaxPlayers = this.MaxPlayers,
            Ping = this.Ping,
            Passworded = this.Passworded,
            IsFavorite = this.IsFavorite,
            Password = this.Password,
            Difficulty = this.Difficulty,
            Wave = this.Wave,
            FinalWave = this.FinalWave,
            Version = this.Version,
            Dedicated = this.Dedicated,
            Secure = this.Secure,
            Bots = this.Bots,
            Os = this.Os,
        };

        /// <summary>Fold a live A2S reply in, keeping what the reply does not carry.</summary>
        internal void Apply(A2SInfo live)
        {
            this.Name = live.Name.Length > 0 ? live.Name : this.Name;
            this.Map = live.Map;
            this.Players = live.Players;
            this.MaxPlayers = live.MaxPlayers;
            this.Ping = live.Ping;
            this.Passworded = live.Passworded;
            this.Bots = live.Bots;
            this.Secure = live.Vac;
            this.Wave = live.Wave;
            this.FinalWave = live.FinalWave;

            if (live.Version.Length > 0)
            {
                this.Version = live.Version;
            }

            // only the optional extra data field carries it, so 0 means "keep what we had"
            if (live.GamePort != 0)
            {
                this.GamePort = live.GamePort;
            }
        }

        /// <summary>A padlock for the grid, so nobody wastes a connect on a server they cannot enter.</summary>
        public string Locked => this.Passworded ? "🔒" : string.Empty;

        public string PingText => this.Ping < 0 ? "—" : this.Ping.ToString();

        public string Address => $"{this.Query.Address}:{this.GamePort}";

        /// <summary>Unreal takes the password as a url option, which is the only way past a padlock.</summary>
        private string Option => this.Password.Length > 0 ? $"?password={this.Password}" : string.Empty;

        /// <summary>Starts the game on the server.  Ignored by steam if the game already runs.</summary>
        // steam://connect makes steam query the server for the app id, and asking the game port for
        // it gets "app id specified by server is invalid", so name the app ourselves and hand the
        // address over as a launch argument, which is how unreal opens a url.
        public string LaunchUri => $"steam://run/{ServerBrowser.AppId}//{this.Query.Address}:{this.GamePort}{this.Option}/";

        /// <summary>For a game that is already up: press ~ and paste.  Steam has no route in,
        /// steam://connect asks the server for its app id and KF servers do not report a usable
        /// one, on either port ("app id specified by server is invalid").</summary>
        public string ConsoleCommand => $"open {this.Query.Address}:{this.GamePort}{this.Option}";
    }

    /// <summary>One player on a server, from A2S_PLAYER.</summary>
    public record PlayerInfo(string Name, int Score, TimeSpan Time)
    {
        public string TimeText => this.Time.TotalHours >= 1
            ? $"{(int)this.Time.TotalHours}:{this.Time.Minutes:00}:{this.Time.Seconds:00}"
            : $"{this.Time.Minutes}:{this.Time.Seconds:00}";
    }

    /// <summary>Live values straight off a server, used to refresh a <see cref="ServerInfo"/>.</summary>
    internal record A2SInfo(string Name, string Map, int Players, int MaxPlayers, int Bots, bool Passworded, bool Vac, string Version, ushort GamePort, int Ping)
    {
        /// <summary>Not A2S at all, filled in from unreals query when it answers.  0 is unknown.</summary>
        public int Wave { get; init; }

        public int FinalWave { get; init; }
    }

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

        private static readonly Encoding StrictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private const uint Single = 0xFFFFFFFF;
        private const uint Split = 0xFFFFFFFE;

        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(20) };
        private static readonly byte[] InfoRequest = [0xFF, 0xFF, 0xFF, 0xFF, 0x54, .. Encoding.ASCII.GetBytes("Source Engine Query\0")];
        private static readonly byte[] PlayerRequest = [0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF];
        private static readonly byte[] UnrealInfoRequest = [0x80, 0x00, 0x00, 0x00, 0x00];

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
                    Bots = server.TryGetProperty("bots", out JsonElement bots) ? bots.GetInt32() : 0,
                    Version = (server.TryGetProperty("version", out JsonElement version) ? version.GetString() : null) ?? string.Empty,
                    Secure = server.TryGetProperty("secure", out JsonElement secure) && secure.GetBoolean(),
                    Dedicated = !server.TryGetProperty("dedicated", out JsonElement dedicated) || dedicated.GetBoolean(),
                    Os = (server.TryGetProperty("os", out JsonElement os) ? os.GetString() : null) ?? string.Empty,
                    Difficulty = ParseDifficulty(server.TryGetProperty("gametype", out JsonElement tags) ? tags.GetString() : null),
                });
            }

            return servers;
        }

        /// <summary>
        /// KF puts its difficulty in the server list tag, as "d;0;2": dedicated or listen, a flag
        /// we have no use for, and the difficulty from 0 (beginner) to 4 (hell on earth).
        /// </summary>
        internal static int ParseDifficulty(string? gametype)
        {
            string[] parts = (gametype ?? string.Empty).Split(';');

            return parts.Length > 2 && int.TryParse(parts[2], out int difficulty) && difficulty is >= 0 and <= 4
                ? difficulty
                : -1;
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

                A2SInfo? info = reply is null ? null : ParseInfo(reply, server, (int)sw.ElapsedMilliseconds);

                // steam knows nothing of waves, unreals own query one above the game port does.  the
                // server has just answered, so a quiet port there is a firewall and not worth the
                // full timeout on every refresh
                if (info is { GamePort: > 0 and < ushort.MaxValue })
                {
                    (int wave, int finalWave) = await QueryWaveAsync(new IPEndPoint(server.Address, info.GamePort + 1), Math.Min(timeoutMs, (info.Ping * 3) + 250), ct);
                    info = info with { Wave = wave, FinalWave = finalWave };
                }

                return info;
            }
            catch (Exception ex) when (ex is SocketException or OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>Unreal 2's native info query.  (0, 0) when it does not answer.</summary>
        private static async Task<(int Wave, int FinalWave)> QueryWaveAsync(IPEndPoint unreal, int timeoutMs, CancellationToken ct)
        {
            try
            {
                using UdpClient udp = new();
                udp.Connect(unreal);

                using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(timeoutMs);

                // not ReceiveAsync, unreals replies do not carry the A2S header it looks for
                await udp.SendAsync(UnrealInfoRequest, timeout.Token);
                return ParseWave((await udp.ReceiveAsync(timeout.Token)).Buffer);
            }
            catch (Exception ex) when (ex is SocketException or OperationCanceledException)
            {
                return default;
            }
        }

        /// <summary>
        /// Unreal's info reply: header, server id, ip, port, query port, name, map and game type,
        /// then players and max players, which KF follows with the wave it is on and the last one.
        /// </summary>
        internal static (int Wave, int FinalWave) ParseWave(ReadOnlySpan<byte> data)
        {
            if (data.Length < 5 || BinaryPrimitives.ReadUInt32LittleEndian(data) != 0x80 || data[4] != 0x00)
            {
                return default;
            }

            try
            {
                int i = 9;                              // header, command, server id
                SkipUnrealString(data, ref i);          // ip
                i += 8;                                 // port, query port
                SkipUnrealString(data, ref i);          // name
                SkipUnrealString(data, ref i);          // map
                SkipUnrealString(data, ref i);          // game type
                i += 8;                                 // players, max players

                return (BinaryPrimitives.ReadInt32LittleEndian(data.Slice(i, 4)), BinaryPrimitives.ReadInt32LittleEndian(data.Slice(i + 4, 4)));
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException or IndexOutOfRangeException)
            {
                return default;
            }
        }

        /// <summary>
        /// Unreal writes the length first as a compact index: sign in the top bit of the first byte
        /// (negative is utf16), 6 bits of value, and 7 more from each byte after while bit 0x40 then
        /// 0x80 says there is more.  Server names with colour codes run past 63 bytes, so it matters.
        /// </summary>
        private static void SkipUnrealString(ReadOnlySpan<byte> data, ref int i)
        {
            byte first = data[i++];
            int length = first & 0x3F;
            bool more = (first & 0x40) != 0;
            for (int shift = 6; more && shift < 32; shift += 7)
            {
                byte next = data[i++];
                length |= (next & 0x7F) << shift;
                more = (next & 0x80) != 0;
            }

            i += (first & 0x80) != 0 ? length * 2 : length;
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
                string version = ReadString(data, ref i);

                // the game port only comes through in the optional extra data field, and 0 tells
                // the caller to keep the port it already had rather than connect to the query one
                ushort gamePort = 0;
                if (i < data.Length)
                {
                    byte edf = data[i++];
                    if ((edf & 0x80) != 0 && i + 2 <= data.Length)
                    {
                        gamePort = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(i, 2));
                    }
                }

                return new A2SInfo(name, map, players, maxPlayers, bots, passworded, vac, version, gamePort, ping);
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException or IndexOutOfRangeException)
            {
                // a truncated or lying reply is just one dead server, not a dead refresh
                return null;
            }
        }

        /// <summary>
        /// A null terminated string, minus unreals inline colour codes: an escape and three bytes
        /// of rgb, which are not text and turn into a row of boxes in the grid if you keep them.
        /// </summary>
        private static string ReadString(ReadOnlySpan<byte> data, ref int i)
        {
            List<byte> value = new();
            while (i < data.Length && data[i] != 0)
            {
                if (data[i] == 0x1B)
                {
                    i += 4;
                    continue;
                }

                value.Add(data[i++]);
            }

            i++;

            return Decode(value.ToArray()).Trim();
        }

        /// <summary>
        /// Server names are whatever the host typed.  Most are utf8, plenty are not, and decoding
        /// one of those as utf8 leaves a row of replacement diamonds where the box drawing and the
        /// accents were, so anything that is not valid utf8 is read as latin1 instead.
        /// </summary>
        private static string Decode(byte[] text)
        {
            try
            {
                return StrictUtf8.GetString(text);
            }
            catch (DecoderFallbackException)
            {
                return Encoding.Latin1.GetString(text);
            }
        }

        /// <summary>
        /// One reply, which is not always one packet: a player list off a full server does not fit
        /// in a datagram and comes back in numbered pieces to be glued together.
        /// </summary>
        private static async Task<byte[]?> ReceiveAsync(UdpClient udp, int timeoutMs, CancellationToken ct)
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(timeoutMs);

            Dictionary<byte, byte[]> pieces = new();

            try
            {
                while (true)
                {
                    byte[] packet = (await udp.ReceiveAsync(timeout.Token)).Buffer;
                    if (packet.Length < 4)
                    {
                        continue;
                    }

                    uint header = BinaryPrimitives.ReadUInt32LittleEndian(packet);
                    if (header == Single)
                    {
                        return packet;
                    }

                    // a split reply: an id, how many pieces there are and which one this is
                    if (header != Split || packet.Length < 12)
                    {
                        continue;
                    }

                    // the top bit of the id means the payload is compressed, which no KF server does
                    if ((BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(4)) & 0x80000000) != 0)
                    {
                        return null;
                    }

                    pieces[packet[9]] = packet;
                    if (pieces.Count >= packet[8])
                    {
                        return Reassemble([.. pieces.OrderBy(piece => piece.Key).Select(piece => piece.Value)]);
                    }
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException or SocketException)
            {
                return null;
            }
        }

        /// <summary>
        /// Glue the pieces of a split reply back into the single packet it would have been.  Most
        /// games put a payload size after the piece number, some do not, and the giveaway is
        /// whether the first piece carries the ordinary reply header where it should.
        /// </summary>
        internal static byte[]? Reassemble(IReadOnlyList<byte[]> pieces)
        {
            byte[]? first = pieces.FirstOrDefault(piece => piece.Length > 9 && piece[9] == 0);
            if (first is null || pieces.Count != first[8])
            {
                return null;
            }

            int offset = first.Length >= 16 && BinaryPrimitives.ReadUInt32LittleEndian(first.AsSpan(12)) == Single ? 12 : 10;
            if (pieces.Any(piece => piece.Length < offset))
            {
                return null;
            }

            return [.. pieces.SelectMany(piece => piece.Skip(offset))];
        }
    }
}
