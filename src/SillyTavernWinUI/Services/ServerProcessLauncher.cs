using System.Diagnostics;
using System.IO;

namespace SillyTavernWinUI.Services;

/// <summary>
/// 以固定命令（PATH 中的 node + 常量参数 server.js）在指定目录后台启动酒馆服务端。
/// 工作目录先经 Path.GetFullPath 规范化并校验存在性与 server.js 存在性。
/// </summary>
internal static class ServerProcessLauncher
{
    public static Process Launch(string tavernDirectory)
    {
        var fullPath = Path.GetFullPath(tavernDirectory);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"未找到酒馆目录：{fullPath}");
        }
        if (!File.Exists(Path.Combine(fullPath, "server.js")))
        {
            throw new FileNotFoundException($"酒馆目录中没有 server.js：{fullPath}");
        }

        var psi = new ProcessStartInfo
        {
            FileName = "node",
            WorkingDirectory = fullPath,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("server.js");
        return Process.Start(psi)
            ?? throw new InvalidOperationException("无法启动 node 进程");
    }
}
