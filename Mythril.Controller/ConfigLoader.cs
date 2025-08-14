using System.IO;
using Newtonsoft.Json;

namespace Mythril.Controller
{
    public class AppConfig
    {
        public string Transport { get; set; } = "stdio";
        public string ScreenshotMode { get; set; } = "path";
        public string PipeName { get; set; } = "mythril_ai";
        public int TcpPort { get; set; } = 5555;
    }

    public static class ConfigLoader
    {
        private const string ConfigFileName = "ai_config.json";

        public static AppConfig LoadConfig()
        {
            if (File.Exists(ConfigFileName))
            {
                var json = File.ReadAllText(ConfigFileName);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            return new AppConfig(); // Return default if file not found
        }

        public static void SaveConfig(AppConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFileName, json);
        }
    }
}
