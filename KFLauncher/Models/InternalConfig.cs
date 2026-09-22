using System;
using System.IO;
using System.Text.Json;

namespace KFLauncher.Models
{
    /// <summary>Our own settings + config backups, stored next to the users application data.</summary>
    internal static class InternalConfig
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KFLauncher");

        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        public static JsonConfig ReadConfig()
        {
            try
            {
                string json = ReadFile("config.json");
                // missing keys fall back to the property defaults, no merging needed
                return json.Length > 0 ? JsonSerializer.Deserialize<JsonConfig>(json) ?? new() : new();
            }
            catch (Exception ex)
            {
                // malformed, unreadable, whatever: defaults beat a launcher that will not open
                TraceLog.Error("reading config.json", ex);
                return new();
            }
        }

        public static void WriteConfig(JsonConfig config)
        {
            WriteFile("config.json", JsonSerializer.Serialize(config, Options));
        }

        /// <summary>Saving settings is a side effect of typing, so it must never throw at the ui.</summary>
        public static void WriteFile(string name, string data)
        {
            try
            {
                Directory.CreateDirectory(AppDataPath);
                string path = Path.Combine(AppDataPath, name);

                // via a temp file, so a crash mid write cannot cost someone their config backup
                File.WriteAllText(path + ".tmp", data);
                File.Move(path + ".tmp", path, true);
            }
            catch (Exception ex)
            {
                TraceLog.Error($"writing {name}", ex);
            }
        }

        public static string ReadFile(string name)
        {
            try
            {
                string path = Path.Combine(AppDataPath, name);
                return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            }
            catch (Exception ex)
            {
                TraceLog.Error($"reading {name}", ex);
                return string.Empty;
            }
        }
    }
}
