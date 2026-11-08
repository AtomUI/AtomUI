using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

public class ButtonTokenExplicitOverrideTests
{
    static ButtonTokenExplicitOverrideTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public async Task Overriding_ContentFontSize_Drives_ContentLineHeight_And_Keeps_The_Override()
    {
        var manager = GetThemeManager();
        var control = new ControlThemeConfigBuilder().WithToken("ContentFontSize", "20").Build();
        var config = new ThemeConfigBuilder().WithControl(ButtonTokens.Identity, control).Build();
        await Apply(manager, config);

        var button = new Button { Content = "Explicit" };
        var window = new AvaloniaWindow { Content = button };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var accessor = ControlTokenAccessor.Capture<Button>(button);
            accessor.GetOwn<double>(ButtonTokenKind.ContentFontSize).ShouldBe(20d);
            // LineHeight follows the explicit font size: (fontSize + 8) / fontSize * fontSize.
            accessor.GetOwn<double>(ButtonTokenKind.ContentLineHeight).ShouldBe(28d, 1e-9);
            // Untouched sibling sizes keep their default derivation from the global font size.
            var globalFontSize = accessor.GetEffectiveGlobal<double>(SharedTokenKind.FontSize);
            accessor.GetOwn<double>(ButtonTokenKind.ContentFontSizeSM).ShouldBe(globalFontSize);
            accessor.GetOwn<double>(ButtonTokenKind.ContentLineHeightSM).ShouldBe(globalFontSize + 8, 1e-9);
        }
        finally
        {
            window.Close();
            await Apply(manager, null);
        }
    }

    [Fact]
    public async Task Explicit_ContentLineHeight_Wins_Over_The_Derived_Value()
    {
        var manager = GetThemeManager();
        var control = new ControlThemeConfigBuilder()
                      .WithToken("ContentFontSize", "20")
                      .WithToken("ContentLineHeight", "40")
                      .Build();
        var config = new ThemeConfigBuilder().WithControl(ButtonTokens.Identity, control).Build();
        await Apply(manager, config);

        var button = new Button { Content = "Explicit" };
        var window = new AvaloniaWindow { Content = button };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var accessor = ControlTokenAccessor.Capture<Button>(button);
            accessor.GetOwn<double>(ButtonTokenKind.ContentFontSize).ShouldBe(20d);
            accessor.GetOwn<double>(ButtonTokenKind.ContentLineHeight).ShouldBe(40d);
        }
        finally
        {
            window.Close();
            await Apply(manager, null);
        }
    }

    [Fact]
    public async Task Explicit_Zero_Is_Not_Treated_As_Unset()
    {
        var manager = GetThemeManager();
        var control = new ControlThemeConfigBuilder().WithToken("ContentLineHeight", "0").Build();
        var config = new ThemeConfigBuilder().WithControl(ButtonTokens.Identity, control).Build();
        await Apply(manager, config);

        var button = new Button { Content = "Explicit" };
        var window = new AvaloniaWindow { Content = button };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            ControlTokenAccessor.Capture<Button>(button)
                                .GetOwn<double>(ButtonTokenKind.ContentLineHeight)
                                .ShouldBe(0d);
        }
        finally
        {
            window.Close();
            await Apply(manager, null);
        }
    }

    [Fact]
    public async Task Reapplying_An_Override_With_A_New_Base_Font_Size_Recomputes_The_Derived_Line_Height()
    {
        var manager = GetThemeManager();
        var button  = new Button { Content = "Explicit" };
        var window  = new AvaloniaWindow { Content = button };
        try
        {
            await Apply(manager, Config(fontSize: "14"));
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var accessor = ControlTokenAccessor.Capture<Button>(button);
            accessor.GetOwn<double>(ButtonTokenKind.ContentFontSizeLG).ShouldBe(20d);
            accessor.GetOwn<double>(ButtonTokenKind.ContentFontSize).ShouldBe(14d);
            accessor.GetOwn<double>(ButtonTokenKind.ContentLineHeight).ShouldBe(22d, 1e-9);

            // Same explicit Own override, different base font size: derived values track the new base, the override stays.
            await Apply(manager, Config(fontSize: "16"));
            Dispatcher.UIThread.RunJobs();
            accessor = ControlTokenAccessor.Capture<Button>(button);
            accessor.GetOwn<double>(ButtonTokenKind.ContentFontSizeLG).ShouldBe(20d);
            accessor.GetOwn<double>(ButtonTokenKind.ContentFontSize).ShouldBe(16d);
            accessor.GetOwn<double>(ButtonTokenKind.ContentLineHeight).ShouldBe(24d, 1e-9);
            accessor.GetOwn<double>(ButtonTokenKind.ContentLineHeightLG).ShouldBe(28d, 1e-9);
        }
        finally
        {
            window.Close();
            await Apply(manager, null);
        }

        static ThemeConfig Config(string fontSize) =>
            new ThemeConfigBuilder()
                .WithToken("FontSize", fontSize)
                .WithControl(ButtonTokens.Identity, new ControlThemeConfigBuilder().WithToken("ContentFontSizeLG", "20").Build())
                .Build();
    }

    private static ThemeManager GetThemeManager()
    {
        return AvaloniaLocator.Current.GetService(typeof(ThemeManager)).ShouldBeOfType<ThemeManager>();
    }

    private static async Task Apply(ThemeManager manager, ThemeConfig? config)
    {
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
    }
}
