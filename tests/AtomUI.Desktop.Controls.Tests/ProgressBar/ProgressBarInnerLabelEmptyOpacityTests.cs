using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIProgressBar = AtomUI.Desktop.Controls.ProgressBar;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ProgressBar;

/// <summary>
/// <c>ProgressBarToken.InnerLabelEmptyOpacity</c>: the inner percentage label at value 0 (on the empty track) keeps its
/// default opacity 1, and a theme can hide it (PrimeNG renders no label at 0). Other values and outer labels are not
/// affected.
/// </summary>
public class ProgressBarInnerLabelEmptyOpacityTests
{
    static ProgressBarInnerLabelEmptyOpacityTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(null, 1d)]
    [InlineData("0", 0d)]
    [InlineData("0.4", 0.4d)]
    public async Task Inner_Label_At_Zero_Takes_The_Token_Opacity(string? tokenValue, double expected)
    {
        var manager = Application.Current!.GetThemeManager().ShouldBeOfType<ThemeManager>();
        var config = tokenValue is null
            ? null
            : new ThemeConfigBuilder()
                // Value 上的 DoubleTransition（MotionDurationVerySlow）在 headless 下不会走完，
                // 离开 0 的断言必须禁用动效让属性值立即生效
                .WithToken("EnableMotion", "false")
                .WithControl(ProgressBarTokens.Identity,
                    new ControlThemeConfigBuilder().WithToken("InnerLabelEmptyOpacity", tokenValue).Build()).Build();
        await Apply(manager, config);
        try
        {
            var empty = Inner(0);
            var half  = Inner(50);
            var outer = new AtomUIProgressBar { Value = 0, Width = 300 };
            var window = new AvaloniaWindow
            {
                Width = 400, Height = 200,
                Content = new StackPanel { Children = { empty, half, outer } }
            };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();

                Label(empty).Opacity.ShouldBe(expected);
                Label(half).Opacity.ShouldBe(1d);
                Label(outer).Opacity.ShouldBe(1d);

                // Leaving 0 shows the label again; returning to 0 applies the token again.
                empty.Value = 10;
                Dispatcher.UIThread.RunJobs();
                Label(empty).Opacity.ShouldBe(1d);
                empty.Value = 0;
                Dispatcher.UIThread.RunJobs();
                Label(empty).Opacity.ShouldBe(expected);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            await Apply(manager, null);
        }
    }

    private static AtomUIProgressBar Inner(double value) => new()
    {
        Value = value, Width = 300,
        PercentPosition = new PercentPosition { IsInner = true }
    };

    private static Avalonia.Controls.Label Label(AtomUIProgressBar progress) =>
        progress.GetVisualDescendants().OfType<Avalonia.Controls.Label>().Single(label => label.Name == "PART_PercentageLabel");

    private static async Task Apply(ThemeManager manager, ThemeConfig? config)
    {
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(AntDesignThemes.DaybreakBlue, config, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
    }
}
