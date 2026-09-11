using System.Management;

namespace SillyTavernWinUI.Services;

public enum GpuVendor
{
    Unknown,
    Nvidia,
    Amd,
    Intel,
}

public record GpuInfo(string Name, GpuVendor Vendor)
{
    /// <summary>该 GPU 厂商对应的驱动层超分/帧生成技术名称。</summary>
    public string UpscalingTechLabel => Vendor switch
    {
        GpuVendor.Nvidia => "DLSS / DLSS 帧生成（仅支持的游戏生效）",
        GpuVendor.Amd => "FSR / AFMF 流体移动帧（驱动层，仅支持的游戏生效）",
        GpuVendor.Intel => "XeSS / Intel Smooth Motion（仅支持的游戏生效）",
        _ => "未识别的 GPU",
    };
}

/// <summary>通过 WMI 枚举显卡并识别厂商。</summary>
public static class GpuDetector
{
    public static IReadOnlyList<GpuInfo> GetGpus()
    {
        var result = new List<GpuInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"] as string;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }
                result.Add(new GpuInfo(name, Classify(name)));
            }
        }
        catch
        {
            // WMI 不可用时返回空列表，由界面显示"未识别"
        }
        return result;
    }

    private static GpuVendor Classify(string name)
    {
        if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("GeForce", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("RTX", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("GTX", StringComparison.OrdinalIgnoreCase))
        {
            return GpuVendor.Nvidia;
        }
        if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Radeon", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("RX ", StringComparison.OrdinalIgnoreCase))
        {
            return GpuVendor.Amd;
        }
        if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Arc", StringComparison.OrdinalIgnoreCase))
        {
            return GpuVendor.Intel;
        }
        return GpuVendor.Unknown;
    }
}
