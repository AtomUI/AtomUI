using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Layout;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateWindowTitleBarScenarios()
    {
        return
        [
            new PerfScenario("WindowTitleBar.Windows.TitleOnly", _ => CreateWindowTitleBar(OsType.Windows)),
            new PerfScenario("WindowTitleBar.Windows.TitleLogo", _ => CreateWindowTitleBar(OsType.Windows, hasLogo: true)),
            new PerfScenario("WindowTitleBar.Windows.AddOns", _ => CreateWindowTitleBar(
                OsType.Windows,
                hasLogo: true,
                hasAddOns: true)),
            new PerfScenario("WindowTitleBar.Linux.TitleOnly", _ => CreateWindowTitleBar(OsType.Linux)),
            new PerfScenario("WindowTitleBar.macOS.TitleOnly", _ => CreateWindowTitleBar(OsType.macOS)),
            new PerfScenario("CaptionButtonGroup.Windows.Default", _ => CreateCaptionButtonGroup(OsType.Windows)),
            new PerfScenario("CaptionButtonGroup.Windows.FullScreen", _ => CreateCaptionButtonGroup(
                OsType.Windows,
                isFullScreen: true)),
            new PerfScenario("CaptionButtonGroup.Windows.Maximized", _ => CreateCaptionButtonGroup(
                OsType.Windows,
                isMaximized: true)),
            new PerfScenario("CaptionButtonGroup.Linux.Default", _ => CreateCaptionButtonGroup(OsType.Linux)),
            new PerfScenario("CaptionButtonGroup.macOS.Default", _ => CreateCaptionButtonGroup(OsType.macOS)),
            new PerfScenario("WindowTitleBar.Batch.Windows8", _ => CreateWindowTitleBarBatch())
        ];
    }

    private static WindowTitleBar CreateWindowTitleBar(
        OsType osType,
        bool hasLogo = false,
        bool hasAddOns = false)
    {
        var titleBar = new WindowTitleBar
        {
            Title = "AtomUI Workspace",
            Width = 640
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, osType);

        if (hasLogo)
        {
            titleBar.Logo = new AppstoreOutlined();
        }

        if (hasAddOns)
        {
            titleBar.LeftAddOn = new AtomTextBlock
            {
                Text              = "Project",
                VerticalAlignment = VerticalAlignment.Center
            };
            titleBar.RightAddOn = new AtomButton
            {
                Content    = "Action",
                ButtonType = ButtonType.Primary
            };
        }

        return titleBar;
    }

    private static CaptionButtonGroup CreateCaptionButtonGroup(
        OsType osType,
        bool isFullScreen = false,
        bool isMaximized = false)
    {
        var group = new CaptionButtonGroup
        {
            Width                            = 260,
            Height                           = 32,
            IsFullScreenCaptionButtonVisible = true,
            IsPinCaptionButtonVisible        = true,
            IsCloseCaptionButtonVisible      = true,
            IsMaximizeCaptionButtonVisible   = true,
            IsMinimizeCaptionButtonVisible   = true,
            IsWindowActive                   = true,
            HostWindowState                  = isFullScreen
                ? WindowState.FullScreen
                : isMaximized
                    ? WindowState.Maximized
                    : WindowState.Normal
        };
        group.SetValue(CaptionButtonGroup.OsTypeProperty, osType);
        return group;
    }

    private static Control CreateWindowTitleBarBatch()
    {
        var root = new StackPanel
        {
            Spacing = 8,
            Width   = 720
        };

        for (var i = 0; i < 8; i++)
        {
            root.Children.Add(CreateWindowTitleBar(
                OsType.Windows,
                hasLogo: i % 2 == 0,
                hasAddOns: i % 3 == 0));
        }

        return root;
    }

}
