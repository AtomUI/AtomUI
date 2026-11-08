using AtomUI.Controls.Commons;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FloatButton;

/// <summary>
/// FloatButton badge colours: a preset <c>BadgeColor</c> is the theme snapshot's palette step 6 (as for the standalone
/// badges), resolved when the button attaches (so a XAML value set before attach applies) and again on every theme
/// publish (so it follows dark mode); a custom colour is used as given.
/// </summary>
public class FloatButtonBadgeColorTests
{
    static FloatButtonBadgeColorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Preset_Badge_Color_Set_Before_Attach_Follows_The_Palette_And_Dark_Mode(bool isDot)
    {
        var button = new AtomUI.Desktop.Controls.FloatButton
        {
            IsBadgeEnabled = true,
            IsDotBadge     = isDot,
            BadgeCount     = 5,
            BadgeColor     = "blue"
        };
        var provider = new ThemeConfigProvider { Child = button, Config = new ThemeConfigBuilder().Build() };
        Show(provider, () =>
        {
            ReadIndicator(button, isDot).ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Blue).ColorSequence[5]);

            provider.Config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build();
            Dispatcher.UIThread.RunJobs();
            ReadIndicator(button, isDot)
                .ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Blue, isDark: true).ColorSequence[5]);
        });
    }

    [Fact]
    public void Custom_Badge_Color_Is_Used_As_Given()
    {
        var button = new AtomUI.Desktop.Controls.FloatButton { IsBadgeEnabled = true, BadgeCount = 5, BadgeColor = "#ff00b96b" };
        var provider = new ThemeConfigProvider { Child = button, Config = new ThemeConfigBuilder().Build() };
        Show(provider, () =>
        {
            ReadIndicator(button, false).ShouldBe(Color.Parse("#ff00b96b"));

            provider.Config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build();
            Dispatcher.UIThread.RunJobs();
            ReadIndicator(button, false).ShouldBe(Color.Parse("#ff00b96b"));
        });
    }

    [Fact]
    public void Clearing_The_Badge_Color_Restores_ColorError()
    {
        var button = new AtomUI.Desktop.Controls.FloatButton { IsBadgeEnabled = true, BadgeCount = 5, BadgeColor = "blue" };
        Show(button, () =>
        {
            button.BadgeColor = null;
            Dispatcher.UIThread.RunJobs();
            ReadIndicator(button, false)
                .ShouldBe(ControlTokenAccessor.Capture(button).GetEffectiveGlobal<Color>(SharedTokenKind.ColorError));
        });
    }

    private static Color ReadIndicator(AtomUI.Desktop.Controls.FloatButton button, bool isDot)
    {
        var brush = isDot
            ? button.GetVisualDescendants().OfType<AbstractDotBadgeAdorner>().Single().BadgeDotColor
            : button.GetVisualDescendants().OfType<AbstractCountBadgeAdorner>().Single().BadgeColor;
        return brush.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
    }

    private static void Show(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 400, Height = 300, Content = content };
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
