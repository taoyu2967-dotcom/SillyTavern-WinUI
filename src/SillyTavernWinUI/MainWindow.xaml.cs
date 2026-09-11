using System.IO;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.Web.WebView2.Core;
using SillyTavernWinUI.Services;
using Windows.Graphics;

namespace SillyTavernWinUI;

public sealed partial class MainWindow : Window
{
    private readonly ServerManager _server = new();
    private SettingsWindow? _settingsWindow;
    private bool _exitRequested;
    private bool _webViewReady;

    public MainWindow()
    {
        InitializeComponent();
        Title = "SillyTavern";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // WebView2 Loader 支持通过环境变量指定用户数据目录（WinUI 控件无对应属性）
        Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", AppSettings.WebView2UserDataFolder);
        Directory.CreateDirectory(AppSettings.WebView2UserDataFolder);

        AppWindow.Resize(new SizeInt32(1440, 900));
        if (App.Settings.LaunchMaximized && AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.Maximize();
        }
        TrySetWindowIcon();

        _server.StatusChanged += OnServerStatus;
        Closed += MainWindow_Closed;
        RootGrid.Loaded += async (_, _) => await InitializeAsync();

        // 托盘事件在代码中挂接（XAML 事件属性会导致 XamlCompiler 崩溃；
        // H.NotifyIcon 2.3.x 未公开双击事件，通过 DoubleClickCommand 挂接）
        TrayIcon.DoubleClickCommand = new RelayCommand(_ => ShowFromTray());
        TrayOpenItem.Click += TrayOpen_Click;
        TrayExitItem.Click += TrayExit_Click;
    }

    private void TrySetWindowIcon()
    {
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
            if (File.Exists(iconPath))
            {
                AppWindow.SetIcon(iconPath);
            }
        }
        catch
        {
            // 图标失败不影响功能
        }
    }

    private void OnServerStatus(string message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusText.Text = message;
            StartupStatusText.Text = message;
        });
    }

    private async Task InitializeAsync()
    {
        try
        {
            await Browser.EnsureCoreWebView2Async();

            var core = Browser.CoreWebView2;
            core.Settings.AreDevToolsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.AreBrowserAcceleratorKeysEnabled = true;
            core.Settings.IsZoomControlEnabled = true;
            core.Settings.IsSwipeNavigationEnabled = false;
            // 每次页面加载完成后应用界面缩放（CSS zoom）
            core.NavigationCompleted += (_, _) => ApplyRenderScale();
            _webViewReady = true;
            ApplyRenderScale();
        }
        catch (Exception ex)
        {
            ShowStartupError($"WebView2 初始化失败：{ex.Message}\n请确认已安装 Microsoft Edge WebView2 Runtime。");
            return;
        }

        await StartServerFlowAsync();
    }

    private async Task StartServerFlowAsync()
    {
        if (!_webViewReady)
        {
            return;
        }

        StartupOverlay.Visibility = Visibility.Visible;
        StartupProgress.IsActive = true;
        StartupErrorText.Visibility = Visibility.Collapsed;
        ErrorButtons.Visibility = Visibility.Collapsed;

        try
        {
            int port;
            if (App.Settings.AutoStartServer)
            {
                port = await Task.Run(() =>
                    _server.EnsureRunningAsync(App.Settings.ResolvePort(), App.Settings.TavernPath));
            }
            else
            {
                port = App.Settings.ResolvePort();
                if (!await ServerManager.ProbeAsync(port, 1500))
                {
                    ShowStartupError($"未检测到运行中的酒馆服务（端口 {port}）。" +
                        "已关闭自动启动：请先手动启动酒馆后点\"重试\"，或在设置中开启自动启动。");
                    return;
                }
                StatusText.Text = $"已附加到运行中的酒馆（端口 {port}）";
            }
            Browser.Source = new Uri($"http://127.0.0.1:{port}/");
            StartupOverlay.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            ShowStartupError(ex.Message);
        }
    }

    private void ShowStartupError(string message)
    {
        StartupProgress.IsActive = false;
        StartupStatusText.Text = "启动失败";
        StartupErrorText.Text = message;
        StartupErrorText.Visibility = Visibility.Visible;
        ErrorButtons.Visibility = Visibility.Visible;
    }

    /// <summary>设置保存后回调：实时应用渲染缩放，端口/路径变更在下次重试或重启时生效。</summary>
    public void OnSettingsSaved()
    {
        App.ReloadSettings();
        ApplyRenderScale();
    }

    private void ApplyRenderScale()
    {
        try
        {
            // 界面缩放（CSS zoom）：缩小后页面以更少像素渲染，滚动与动画更流畅
            if (Browser.CoreWebView2 is not null)
            {
                var scale = Math.Clamp(App.Settings.RenderScale, 0.8, 1.2)
                    .ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                _ = Browser.CoreWebView2.ExecuteScriptAsync($"document.body.style.zoom='{scale}'");
            }
        }
        catch
        {
            // WebView 未就绪时忽略，下次保存设置会再应用
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e) => OpenSettings();

    private void OpenSettingsFromError_Click(object sender, RoutedEventArgs e) => OpenSettings();

    public void OpenSettings()
    {
        if (_settingsWindow is null)
        {
            _settingsWindow = new SettingsWindow(this);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }
        _settingsWindow.Activate();
    }

    private async void Retry_Click(object sender, RoutedEventArgs e)
    {
        App.ReloadSettings();
        await StartServerFlowAsync();
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if (App.Settings.CloseToTray && !_exitRequested)
        {
            args.Handled = true;
            AppWindow.Hide();
            return;
        }
        Cleanup();
    }

    private void TrayOpen_Click(object sender, RoutedEventArgs e) => ShowFromTray();

    private void ShowFromTray()
    {
        AppWindow.Show();
        Activate();
    }

    private void TrayExit_Click(object sender, RoutedEventArgs e)
    {
        _exitRequested = true;
        Close();
    }

    private void Cleanup()
    {
        _server.StatusChanged -= OnServerStatus;
        _server.StopIfStartedByUs();
        try
        {
            TrayIcon.Dispose();
        }
        catch
        {
            // 托盘清理失败不阻塞退出
        }
    }
}
