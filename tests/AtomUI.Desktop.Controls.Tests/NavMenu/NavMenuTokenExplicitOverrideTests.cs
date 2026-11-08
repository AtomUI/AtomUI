using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuTokenExplicitOverrideTests
{
    static NavMenuTokenExplicitOverrideTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public async Task Explicit_Zero_ItemContentMargin_Is_Not_Replaced_By_The_Default()
    {
        var manager = AvaloniaLocator.Current.GetService(typeof(ThemeManager)).ShouldBeOfType<ThemeManager>();
        var control = new ControlThemeConfigBuilder().WithToken("ItemContentMargin", "0,0,0,0").Build();
        await Apply(manager, new ThemeConfigBuilder().WithControl(NavMenuTokens.Identity, control).Build());

        var menu = new global::AtomUI.Desktop.Controls.NavMenu();
        var window = new AvaloniaWindow { Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            ControlTokenAccessor.Capture<global::AtomUI.Desktop.Controls.NavMenu>(menu)
                                .GetOwn<Thickness>(NavMenuTokenKind.ItemContentMargin)
                                .ShouldBe(new Thickness(0));
        }
        finally
        {
            window.Close();
            await Apply(manager, null);
        }
    }

    [Fact]
    public async Task Unset_ItemContentMargin_Uses_The_Default_Derivation()
    {
        var manager = AvaloniaLocator.Current.GetService(typeof(ThemeManager)).ShouldBeOfType<ThemeManager>();
        await Apply(manager, null);

        var menu = new global::AtomUI.Desktop.Controls.NavMenu();
        var window = new AvaloniaWindow { Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var accessor = ControlTokenAccessor.Capture<global::AtomUI.Desktop.Controls.NavMenu>(menu);
            var marginXXS = accessor.GetEffectiveGlobal<double>(SharedTokenKind.UniformlyMarginXXS);
            accessor.GetOwn<Thickness>(NavMenuTokenKind.ItemContentMargin)
                    .ShouldBe(new Thickness(marginXXS, 0, marginXXS, marginXXS));
        }
        finally
        {
            window.Close();
        }
    }

    private static async Task Apply(ThemeManager manager, ThemeConfig? config)
    {
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
    }
}
