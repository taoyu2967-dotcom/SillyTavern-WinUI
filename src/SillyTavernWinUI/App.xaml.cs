using Microsoft.UI.Xaml;
using SillyTavernWinUI.Services;

namespace SillyTavernWinUI;

public partial class App : Application
{
    private MainWindow? _mainWindow;

    public static AppSettings Settings { get; private set; } = AppSettings.Load();

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }

    /// <summary>设置变更后由设置窗口调用，重新加载全局设置。</summary>
    public static void ReloadSettings()
    {
        Settings = AppSettings.Load();
    }
}
