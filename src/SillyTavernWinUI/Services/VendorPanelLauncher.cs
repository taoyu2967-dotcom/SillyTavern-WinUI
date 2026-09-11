using System.Diagnostics;
using System.IO;

namespace SillyTavernWinUI.Services;

/// <summary>
/// 打开显卡厂商控制面板（均为固定 URI / 固定路径，best-effort）。
/// </summary>
internal static class VendorPanelLauncher
{
    public static bool OpenNvidia() =>
        TryUri("nvidiaapp://") ||
        TryUri("geforceexperience://") ||
        TryExe(@"C:\Program Files\NVIDIA Corporation\NVIDIA app\CEF\NVIDIA app.exe");

    public static bool OpenAmd() => TryUri("amdsoftware://");

    public static bool OpenIntel() =>
        TryUri("igcc://") ||
        TryUri("intelgraphicscontrolpanel://");

    private static bool TryUri(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryExe(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
