using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Badge;

/// <summary>
/// Badge preset colours (control-token-guidelines consumption rule 4): a preset name is antd's genPresetColor
/// <c>darkColor</c> (palette step 6) of the badge's theme snapshot, so it follows the dark palettes, is not derived from
/// ColorPrimary, and clearing the colour restores the theme's default indicator colour.
/// </summary>
public class BadgePresetColorTests
{
    static BadgePresetColorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public static TheoryData<string> Badges() => ["CountBadge", "DotBadge", "RibbonBadge"];

    [Theory]
    [MemberData(nameof(Badges))]
    public void Preset_Color_Follows_The_Dark_Palette(string kind)
    {
        var (badge, read) = Create(kind, "volcano");
        var provider = new ThemeConfigProvider { Child = badge, Config = new ThemeConfigBuilder().Build() };
        Show(provider, () =>
        {
            var light = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Volcano).ColorSequence[5];
            var dark  = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Volcano, isDark: true).ColorSequence[5];
            dark.ShouldNotBe(light, "the dark palette must differ for the test to observe the theme.");
            read().ShouldBe(light);

            provider.Config = new ThemeConfigBuilder().WithAppearance(ThemeAppearance.Dark).Build();
            Dispatcher.UIThread.RunJobs();
            read().ShouldBe(dark, $"{kind} preset colour did not follow the dark palette.");

            provider.Config = new ThemeConfigBuilder().Build();
            Dispatcher.UIThread.RunJobs();
            read().ShouldBe(light);
        });
    }

    [Theory]
    [MemberData(nameof(Badges))]
    public void Preset_Color_Does_Not_Follow_ColorPrimary(string kind)
    {
        var (badge, read) = Create(kind, "blue");
        var provider = new ThemeConfigProvider { Child = badge, Config = new ThemeConfigBuilder().Build() };
        Show(provider, () =>
        {
            var blue = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Blue).ColorSequence[5];
            read().ShouldBe(blue);

            provider.Config = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.ColorPrimary), "#ff00b96b").Build();
            Dispatcher.UIThread.RunJobs();
            read().ShouldBe(blue, $"{kind} preset colour changed with ColorPrimary.");
        });
    }

    [Theory]
    [MemberData(nameof(Badges))]
    public void Preset_Color_Is_Re_Resolved_When_Re_Attached_After_A_Theme_Change(string kind)
    {
        var (badge, read) = Create(kind, "volcano");
        var host     = new StackPanel { Children = { badge } };
        var provider = new ThemeConfigProvider { Child = host, Config = new ThemeConfigBuilder().Build() };
        Show(provider, () =>
        {
            read().ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Volcano).ColorSequence[5]);

            host.Children.Remove(badge);
            Dispatcher.UIThread.RunJobs();
            provider.Config = new ThemeConfigBuilder().WithAppearance(ThemeAppearance.Dark).Build();
            Dispatcher.UIThread.RunJobs();
            host.Children.Add(badge);
            Dispatcher.UIThread.RunJobs();

            read().ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Volcano, isDark: true).ColorSequence[5],
                $"{kind} kept the preset colour of the theme it was detached from.");
        });
    }

    [Theory]
    [MemberData(nameof(Badges))]
    public void Preset_Color_Follows_The_Provider_It_Moves_Into(string kind)
    {
        var (badge, read) = Create(kind, "volcano");
        var lightHost = new StackPanel { Children = { badge } };
        var darkHost  = new StackPanel();
        var light     = new ThemeConfigProvider { Child = lightHost, Config = new ThemeConfigBuilder().Build() };
        var dark = new ThemeConfigProvider
        {
            Child  = darkHost,
            Config = new ThemeConfigBuilder().WithAppearance(ThemeAppearance.Dark).Build()
        };
        Show(new StackPanel { Children = { light, dark } }, () =>
        {
            read().ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Volcano).ColorSequence[5]);

            lightHost.Children.Remove(badge);
            darkHost.Children.Add(badge);
            Dispatcher.UIThread.RunJobs();

            read().ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Volcano, isDark: true).ColorSequence[5],
                $"{kind} kept the preset colour of the provider it left.");
        });
    }

    [Fact]
    public void Clearing_The_Count_Badge_Color_Restores_The_Indicator_Color()
    {
        var (badge, read) = Create("CountBadge", "red");
        Show(badge, () =>
        {
            read().ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Red).ColorSequence[5]);
            var expected = ControlTokenAccessor.Capture(badge).GetOwn<Color>(CountBadgeTokenKind.IndicatorColor);

            ((CountBadge)badge).BadgeColor = null;
            Dispatcher.UIThread.RunJobs();
            read().ShouldBe(expected);

            ((CountBadge)badge).BadgeColor = "not-a-color";
            Dispatcher.UIThread.RunJobs();
            read().ShouldBe(expected);
        });
    }

    [Fact]
    public void Clearing_The_Dot_Badge_Color_Restores_The_Status_Color()
    {
        var (badge, read) = Create("DotBadge", "red");
        ((DotBadge)badge).Status = DotBadgeStatus.Success;
        Show(badge, () =>
        {
            read().ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Red).ColorSequence[5]);

            ((DotBadge)badge).DotColor = null;
            Dispatcher.UIThread.RunJobs();
            read().ShouldBe(ControlTokenAccessor.Capture(badge).GetEffectiveGlobal<Color>(SharedTokenKind.ColorSuccess));
        });
    }

    [Fact]
    public void Clearing_The_Ribbon_Color_Restores_ColorPrimary()
    {
        var (badge, read) = Create("RibbonBadge", "red");
        Show(badge, () =>
        {
            read().ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Red).ColorSequence[5]);

            ((RibbonBadge)badge).RibbonColor = null;
            Dispatcher.UIThread.RunJobs();
            read().ShouldBe(ControlTokenAccessor.Capture(badge).GetEffectiveGlobal<Color>(SharedTokenKind.ColorPrimary));
        });
    }

    private static (Control Badge, Func<Color> Read) Create(string kind, string color)
    {
        switch (kind)
        {
            case "CountBadge":
            {
                var badge = new CountBadge { Count = 5, BadgeColor = color, IsMotionEnabled = false };
                return (badge, () => BrushColor(Adorner<AbstractCountBadgeAdorner>(badge).BadgeColor));
            }
            case "DotBadge":
            {
                var badge = new DotBadge { DotColor = color, Text = "Dot", IsMotionEnabled = false };
                return (badge, () => BrushColor(Adorner<AbstractDotBadgeAdorner>(badge).BadgeDotColor));
            }
            default:
            {
                var badge = new RibbonBadge { RibbonColor = color, Text = "Ribbon" };
                return (badge, () => BrushColor(Adorner<AbstractRibbonBadgeAdorner>(badge).RibbonColor));
            }
        }
    }

    private static TAdorner Adorner<TAdorner>(Control badge)
        where TAdorner : Control =>
        badge.GetVisualDescendants().OfType<TAdorner>().Single();

    private static Color BrushColor(IBrush? brush) => brush.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;

    private static void Show(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 400, Height = 200, Content = content };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
