using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.RadioButton;

public class OptionButtonTokenTests
{
    static OptionButtonTokenTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public async Task Size_Paddings_Use_Their_Own_Height_Font_Size_And_Line_Height()
    {
        await WithOption(null, accessor =>
        {
            var lineWidth = accessor.GetEffectiveGlobal<double>(SharedTokenKind.LineWidth);
            foreach (var (padding, height, fontSize, lineHeight) in new[]
                     {
                         (OptionButtonTokenKind.ContentPadding, SharedTokenKind.ControlHeight,
                             OptionButtonTokenKind.ContentFontSize, OptionButtonTokenKind.ContentLineHeight),
                         (OptionButtonTokenKind.ContentPaddingLG, SharedTokenKind.ControlHeightLG,
                             OptionButtonTokenKind.ContentFontSizeLG, OptionButtonTokenKind.ContentLineHeightLG),
                         (OptionButtonTokenKind.ContentPaddingSM, SharedTokenKind.ControlHeightSM,
                             OptionButtonTokenKind.ContentFontSizeSM, OptionButtonTokenKind.ContentLineHeightSM)
                     })
            {
                var expected = Math.Max(
                    (accessor.GetEffectiveGlobal<double>(height) -
                     accessor.GetOwn<double>(fontSize) * accessor.GetOwn<double>(lineHeight)) / 2 - lineWidth,
                    0);
                accessor.GetOwn<Thickness>(padding).Top.ShouldBe(expected, 1e-9, padding.ToString());
            }

            // The large size has room for a vertical padding; it no longer borrows the small size's operands.
            accessor.GetOwn<Thickness>(OptionButtonTokenKind.ContentPaddingLG).Top.ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public async Task Derived_Font_Sizes_And_Line_Heights_Follow_A_New_Base_Font_Size()
    {
        await WithOption("14", accessor =>
        {
            accessor.GetOwn<double>(OptionButtonTokenKind.ContentFontSize).ShouldBe(14d);
            accessor.GetOwn<double>(OptionButtonTokenKind.ContentLineHeight)
                    .ShouldBe(CalculatorUtils.CalculateLineHeight(14d), 1e-9);
        }, "16", accessor =>
        {
            accessor.GetOwn<double>(OptionButtonTokenKind.ContentFontSize).ShouldBe(16d);
            accessor.GetOwn<double>(OptionButtonTokenKind.ContentLineHeight)
                    .ShouldBe(CalculatorUtils.CalculateLineHeight(16d), 1e-9);
        });
    }

    private static async Task WithOption(
        string? fontSize,
        Action<ControlTokenAccessor> assertion,
        string? nextFontSize = null,
        Action<ControlTokenAccessor>? nextAssertion = null)
    {
        var manager = AvaloniaLocator.Current.GetService(typeof(ThemeManager)).ShouldBeOfType<ThemeManager>();
        var group   = new OptionButtonGroup();
        var option  = new OptionButton { Content = "Option" };
        group.Items.Add(option);
        var window = new AvaloniaWindow { Content = group };
        try
        {
            await Apply(manager, fontSize);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(ControlTokenAccessor.Capture<OptionButton>(option));
            if (nextAssertion is not null)
            {
                await Apply(manager, nextFontSize);
                Dispatcher.UIThread.RunJobs();
                nextAssertion(ControlTokenAccessor.Capture<OptionButton>(option));
            }
        }
        finally
        {
            window.Close();
            await Apply(manager, null);
        }
    }

    private static async Task Apply(ThemeManager manager, string? fontSize)
    {
        var config = fontSize is null ? null : new ThemeConfigBuilder().WithToken("FontSize", fontSize).Build();
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
    }
}
