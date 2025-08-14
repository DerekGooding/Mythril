using Newtonsoft.Json;

namespace Mythril.Controller;

public class AppConfig
{
    public string Transport { get; set; } = "stdio";
    public string ScreenshotMode { get; set; } = "path";
    public string PipeName { get; set; } = "mythril_ai";
    public int TcpPort { get; set; } = 5555;
}

public static class ConfigLoader
{
    private const string _configFileName = "ai_config.json";

    public static AppConfig LoadConfig()
    {
        if (File.Exists(_configFileName))
        {
            var json = File.ReadAllText(_configFileName);
            return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
        }
        return new AppConfig(); // Return default if file not found
    }

    public static void SaveConfig(AppConfig config)
    {
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(_configFileName, json);
    }
}
