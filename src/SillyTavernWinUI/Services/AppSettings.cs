using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SillyTavernWinUI.Services;

public class AppSettings
{
    public string TavernPath { get; set; } = "";
    /// <summary>0 表示自动从酒馆 config.yaml 读取端口。</summary>
    public int PortOverride { get; set; } = 0;
    public bool AutoStartServer { get; set; } = true;
    public bool CloseToTray { get; set; } = false;
    /// <summary>界面缩放（0.8 ~ 1.2，Chromium 页面缩放），缩小可提升流畅度。</summary>
    public double RenderScale { get; set; } = 1.0;
    public bool LaunchMaximized { get; set; } = true;

    [JsonIgnore]
    public static string SettingsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SillyTavernWinUI");

    [JsonIgnore]
    public static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    [JsonIgnore]
    public static string WebView2UserDataFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SillyTavernWinUI", "WebView2Data");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static AppSettings Load()
    {
        AppSettings settings;
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            else
            {
                settings = new AppSettings();
            }
        }
        catch
        {
            // 设置损坏时回退默认值
            settings = new AppSettings();
        }

        if (string.IsNullOrWhiteSpace(settings.TavernPath))
        {
            settings.TavernPath = TryDetectTavernPath();
        }
        return settings;
    }

    /// <summary>在常见安装位置自动探测包含 server.js 的 SillyTavern 目录；找不到返回空字符串。</summary>
    public static string TryDetectTavernPath()
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidates = new[]
        {
            Path.Combine(profile, "SillyTavern"),
            Path.Combine(profile, "SillyTavern-Launcher", "SillyTavern"),
            @"C:\SillyTavern",
            @"D:\SillyTavern",
            @"E:\SillyTavern",
        };
        foreach (var path in candidates)
        {
            if (File.Exists(Path.Combine(path, "server.js")))
            {
                return path;
            }
        }
        return "";
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, JsonOptions));
    }

    /// <summary>解析酒馆目录下的实际端口：优先用户覆盖值，其次 config.yaml 的 port 字段，最后默认 8000。</summary>
    public int ResolvePort()
    {
        if (PortOverride > 0)
        {
            return PortOverride;
        }
        try
        {
            var configPath = Path.Combine(TavernPath, "config.yaml");
            if (File.Exists(configPath))
            {
                foreach (var rawLine in File.ReadLines(configPath))
                {
                    var line = rawLine.Trim();
                    if (line.StartsWith('#'))
                    {
                        continue;
                    }
                    // 只匹配顶层 "port: 1234"，避免命中 listenAddress 等嵌套键
                    if (line.StartsWith("port:", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = line["port:".Length..].Trim();
                        if (int.TryParse(value, out var port) && port > 0)
                        {
                            return port;
                        }
                    }
                }
            }
        }
        catch
        {
            // 读取失败回退默认端口
        }
        return 8000;
    }
}
