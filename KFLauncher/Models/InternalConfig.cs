using System;
using System.Diagnostics;
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
            catch (JsonException)
            {
                Debug.WriteLine("Malformed config.json, using defaults");
                return new();
            }
        }

        public static void WriteConfig(JsonConfig config)
        {
            WriteFile("config.json", JsonSerializer.Serialize(config, Options));
        }

        public static void WriteFile(string name, string data)
        {
            Directory.CreateDirectory(AppDataPath);
            File.WriteAllText(Path.Combine(AppDataPath, name), data);
        }

        public static string ReadFile(string name)
        {
            string path = Path.Combine(AppDataPath, name);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        public static bool AppFileExists(string name)
        {
            return File.Exists(Path.Combine(AppDataPath, name));
        }
    }
}
