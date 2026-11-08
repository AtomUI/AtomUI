using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Tooltip;

/// <summary>
/// ToolTip preset and custom colours (control-token-guidelines consumption rules 4 and 7): the colour is written to an
/// internal property the ControlTheme projects onto Background through an untriggered setter, so app styles and local
/// values keep precedence; a preset is antd's genPresetColor <c>darkColor</c> (palette step 6) of the tooltip's theme
/// snapshot, so it follows dark palettes and re-resolves when the theme changes.
/// </summary>
public class ToolTipPresetColorTests
{
    static ToolTipPresetColorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void App_Style_Background_Overrides_Preset_Color()
    {
        var host = CreateHost();
        ToolTip.SetPresetColor(host, PresetColorType.Red);
        var tip = new ToolTip { Content = "tip" };
        ToolTip.SetTip(host, tip);
        var window = ShowInWindow(host);
        window.Styles.Add(new Style(x => x.OfType<ToolTip>())
        {
            Setters = { new Setter(TemplatedControl.BackgroundProperty, Brushes.DarkViolet) }
        });
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            tip.Background.ShouldBeSameAs(Brushes.DarkViolet);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Preset_Color_Uses_The_Dark_Snapshot_Palette()
    {
        var host = CreateHost();
        ToolTip.SetPresetColor(host, PresetColorType.Blue);
        ToolTip.SetTip(host, "tip");
        var provider = new ThemeConfigProvider { Child = host, Config = DarkConfig() };
        var window = ShowInWindow(provider);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            var expected = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Blue, isDark: true).ColorSequence[5];
            expected.ShouldNotBe(PresetPrimaryColor.Blue.Color());
            BackgroundOf(OpenedTip(window)).ShouldBe(expected);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Preset_Color_Follows_A_Theme_Change_While_Open()
    {
        var host = CreateHost();
        ToolTip.SetPresetColor(host, PresetColorType.Volcano);
        ToolTip.SetTip(host, "tip");
        var provider = new ThemeConfigProvider { Child = host, Config = new ThemeConfigBuilder().Build() };
        var window = ShowInWindow(provider);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();
            var tip = OpenedTip(window);
            BackgroundOf(tip).ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Volcano).ColorSequence[5]);

            provider.Config = DarkConfig();
            Dispatcher.UIThread.RunJobs();
            BackgroundOf(tip).ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Volcano, isDark: true).ColorSequence[5]);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Custom_Color_Is_Used_As_Given_And_No_Color_Falls_Back_To_The_Token()
    {
        var custom = Color.Parse("#ff7a3e9d");
        var host   = CreateHost();
        ToolTip.SetColor(host, custom);
        var tip = new ToolTip { Content = "tip" };
        ToolTip.SetTip(host, tip);
        var window = ShowInWindow(host);
        try
        {
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();
            BackgroundOf(tip).ShouldBe(custom);

            ToolTip.SetIsOpen(host, false);
            Dispatcher.UIThread.RunJobs();
            ToolTip.SetColor(host, null);
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();
            BackgroundOf(tip).ShouldBe(
                ControlTokenAccessor.Capture(tip).GetOwn<Color>(DesignTokens.ToolTipTokenKind.ToolTipBackground),
                "the tooltip kept the previous host colour instead of the ToolTipBackground Token.");
        }
        finally
        {
            window.Close();
        }
    }

    private static Color BackgroundOf(ToolTip tip) => tip.Background.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;

    private static ToolTip OpenedTip(Visual window) =>
        window.GetVisualDescendants().OfType<ToolTip>().Single(static tip => tip.Classes.Contains(ToolTipPseudoClass.Open));

    private static ThemeConfig DarkConfig() =>
        new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build();

    private static Button CreateHost()
    {
        var host = new Button { Content = "Host" };
        // headless 平台没有原生 popup 窗口实现，弹层只能走 overlay host
        ToolTip.SetIsUseOverlayHost(host, true);
        host.HorizontalAlignment = HorizontalAlignment.Center;
        host.VerticalAlignment   = VerticalAlignment.Center;
        return host;
    }

    private static AtomUIWindow ShowInWindow(Control content)
    {
        var window = new AtomUIWindow
        {
            Width   = 800,
            Height  = 600,
            Content = new Avalonia.Controls.Grid { Children = { content } }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
