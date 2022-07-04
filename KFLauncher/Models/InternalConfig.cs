using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFLauncher.Models
{
    internal class InternalConfig
    {
        private readonly string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KFLauncher\\";
        private readonly string configFileName = "config.json";
        
        public InternalConfig()
        {
            // create the KFLauncher dir in appdata if it doesn't exist
            Directory.CreateDirectory(appDataPath);
        }

        public JsonConfig ReadConfig(JsonConfig j)
        {
            // read json config
            JObject? jsonFile = ReadJsonFile(this.GetAppFilePath(this.configFileName));
            if (jsonFile is null)
            {
                return j;
            }

            // merge with current config
            JObject jsonConfig = JObject.Parse(JsonConvert.SerializeObject(j));
            jsonConfig = this.MergeJson(jsonConfig, jsonFile);
            string json = jsonConfig.ToString();

            // update config object
            if (json.Length > 0)
            {
                JsonConfig? c = JsonConvert.DeserializeObject<JsonConfig>(json);
                if (c is not null)
                {
                    return c;
                }
            }

            return j;
        }

        public void WriteConfig(JsonConfig j)
        {
            // merge configs first
            JObject? jsonFile = ReadJsonFile(this.GetAppFilePath(this.configFileName));
            JObject jsonConfig = JObject.Parse(JsonConvert.SerializeObject(j));
            if (jsonFile is not null)
            {
                jsonConfig = this.MergeJson(jsonFile, jsonConfig);
            }

            // delete config if it exists
            if (File.Exists(this.GetAppFilePath(this.configFileName)))
            {
                try
                {
                    File.Delete(this.GetAppFilePath(this.configFileName));
                }
                catch (Exception ex)
                {
                    // TODO:
                }
            }

            // write to json config file
            File.WriteAllText(this.GetAppFilePath(this.configFileName), jsonConfig.ToString(Formatting.Indented));
        }

        private JObject? ReadJsonFile(string path)
        {
            // read a file into a JObject
            if (File.Exists(path))
            {
                using (StreamReader file = File.OpenText(path))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    try
                    {
                        JObject? jsonFile = (JObject)JToken.ReadFrom(reader);
                        return jsonFile;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        private JObject MergeJson(JObject a, JObject b)
        {
            // merge, replacing A with B
            a.Merge(b, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace
            });

            return a;
        }

        private string GetAppFilePath(string filePath)
        {
            return this.appDataPath + filePath;
        }
    }
}
