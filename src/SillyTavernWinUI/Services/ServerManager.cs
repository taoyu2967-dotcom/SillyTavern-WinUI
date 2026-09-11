using System.Diagnostics;

namespace SillyTavernWinUI.Services;

/// <summary>
/// 管理 SillyTavern 服务端生命周期：探测已运行实例（附加）、后台启动、轮询等待就绪、结束进程树。
/// 实际进程启动由 <see cref="ServerProcessLauncher"/> 完成（固定命令，不经 shell）。
/// </summary>
public class ServerManager
{
    private Process? _serverProcess;

    public bool StartedByUs => _serverProcess is { HasExited: false };
    public int Port { get; private set; }

    public event Action<string>? StatusChanged;

    private void Report(string message) => StatusChanged?.Invoke(message);

    public static async Task<bool> ProbeAsync(int port, int timeoutMs = 800)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeoutMs) };
            using var cts = new CancellationTokenSource(timeoutMs);
            // 任何 HTTP 响应（包括 401/403）都说明端口上是酒馆服务
            using var response = await client.GetAsync($"http://127.0.0.1:{port}/", cts.Token);
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>确保酒馆服务在运行。返回最终可访问的端口；失败抛出异常。</summary>
    public async Task<int> EnsureRunningAsync(int port, string tavernPath, CancellationToken ct = default)
    {
        Port = port;

        if (await ProbeAsync(port))
        {
            Report($"检测到酒馆已在运行（端口 {port}），直接附加");
            return port;
        }

        Report("正在启动酒馆服务端…");
        _serverProcess = ServerProcessLauncher.Launch(tavernPath);

        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            if (_serverProcess is { HasExited: true })
            {
                throw new InvalidOperationException(
                    $"酒馆服务端进程意外退出（退出码 {_serverProcess.ExitCode}），可在酒馆目录下手动运行 node server.js 查看报错");
            }
            if (await ProbeAsync(port, 500))
            {
                Report($"酒馆服务端已就绪（端口 {port}）");
                return port;
            }
            await Task.Delay(400, ct);
        }
        throw new TimeoutException("等待酒馆服务端就绪超时（90 秒）");
    }

    /// <summary>结束由外壳拉起的服务端进程树。附加的外部实例不会被结束。</summary>
    public void StopIfStartedByUs()
    {
        try
        {
            if (_serverProcess is { HasExited: false })
            {
                Report("正在停止酒馆服务端…");
                _serverProcess.Kill(entireProcessTree: true);
                _serverProcess.WaitForExit(5000);
            }
        }
        catch
        {
            // 退出阶段的清理失败不阻塞关闭
        }
        finally
        {
            _serverProcess?.Dispose();
            _serverProcess = null;
        }
    }
}
