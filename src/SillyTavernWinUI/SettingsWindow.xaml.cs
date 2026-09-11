using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SillyTavernWinUI.Services;
using Windows.Graphics;
using Windows.Storage.Pickers;

namespace SillyTavernWinUI;

public sealed partial class SettingsWindow : Window
{
    private readonly MainWindow _owner;

    public SettingsWindow(MainWindow owner)
    {
        InitializeComponent();
        _owner = owner;

        Title = "SillyTavern 设置";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(SettingsTitleBar);
        AppWindow.Resize(new SizeInt32(780, 760));

        LoadValues();
        PopulateGpuSection();
    }

    private void LoadValues()
    {
        var s = App.Settings;
        TavernPathBox.Text = s.TavernPath;
        PortBox.Value = s.PortOverride;
        AutoStartSwitch.IsOn = s.AutoStartServer;
        CloseToTraySwitch.IsOn = s.CloseToTray;
        LaunchMaximizedSwitch.IsOn = s.LaunchMaximized;
        RenderScaleSlider.Value = Math.Round(Math.Clamp(s.RenderScale, 0.8, 1.2) * 100);
        UpdateRenderScaleHeader();
    }

    private void UpdateRenderScaleHeader() =>
        RenderScaleHeader.Text = $"渲染缩放：{(int)RenderScaleSlider.Value}%";

    private void RenderScaleSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (RenderScaleHeader is not null)
        {
            UpdateRenderScaleHeader();
        }
    }

    private async void Browse_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            TavernPathBox.Text = folder.Path;
        }
    }

    private void PopulateGpuSection()
    {
        var gpus = GpuDetector.GetGpus();
        var vendors = new HashSet<GpuVendor>();

        if (gpus.Count == 0)
        {
            GpuList.Children.Add(new TextBlock
            {
                Text = "未能检测到显卡信息（WMI 不可用）。",
                TextWrapping = TextWrapping.Wrap,
            });
        }

        foreach (var gpu in gpus)
        {
            vendors.Add(gpu.Vendor);
            GpuList.Children.Add(new TextBlock
            {
                Text = $"• {gpu.Name} — {gpu.UpscalingTechLabel}",
                TextWrapping = TextWrapping.Wrap,
            });
        }

        if (vendors.Contains(GpuVendor.Nvidia))
        {
            AddVendorButton("打开 NVIDIA App", VendorPanelLauncher.OpenNvidia);
        }
        if (vendors.Contains(GpuVendor.Amd))
        {
            AddVendorButton("打开 AMD Software", VendorPanelLauncher.OpenAmd);
        }
        if (vendors.Contains(GpuVendor.Intel))
        {
            AddVendorButton("打开 Intel 显卡控制中心", VendorPanelLauncher.OpenIntel);
        }
    }

    private void AddVendorButton(string label, Func<bool> openAction)
    {
        var button = new Button { Content = label };
        button.Click += async (_, _) =>
        {
            if (!openAction())
            {
                var dialog = new ContentDialog
                {
                    Title = "无法打开",
                    Content = $"未能启动 {label.Replace("打开 ", "")}，请从开始菜单手动打开。",
                    CloseButtonText = "知道了",
                    XamlRoot = Content.XamlRoot,
                };
                await dialog.ShowAsync();
            }
        };
        VendorButtons.Children.Add(button);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var s = App.Settings;
        s.TavernPath = TavernPathBox.Text.Trim();
        s.PortOverride = (int)PortBox.Value;
        s.AutoStartServer = AutoStartSwitch.IsOn;
        s.CloseToTray = CloseToTraySwitch.IsOn;
        s.LaunchMaximized = LaunchMaximizedSwitch.IsOn;
        s.RenderScale = Math.Clamp(RenderScaleSlider.Value / 100.0, 0.8, 1.2);
        s.Save();

        _owner.OnSettingsSaved();
        SaveHint.Text = "已保存。服务端路径/端口变更将在下次启动或点\"重试\"时生效。";
    }
}
