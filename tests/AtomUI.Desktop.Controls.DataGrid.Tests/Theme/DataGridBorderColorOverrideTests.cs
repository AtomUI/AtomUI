using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Theme;

public class DataGridBorderColorOverrideTests
{
    static DataGridBorderColorOverrideTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public async Task Overriding_BorderColor_Reaches_TableBorderColor_And_The_Rendered_Frame()
    {
        var manager = AvaloniaLocator.Current.GetService(typeof(ThemeManager)).ShouldBeOfType<ThemeManager>();
        var red = Color.Parse("#FF0000");
        var control = new ControlThemeConfigBuilder().WithToken("BorderColor", "#FF0000").Build();
        var config = new ThemeConfigBuilder().WithControl(DataGridTokens.Identity, control).Build();
        await Apply(manager, config);

        var grid = new global::AtomUI.Desktop.Controls.DataGrid { AutoGenerateColumns = false, Width = 460, Height = 240 };
        var window = new AvaloniaWindow { Width = 540, Height = 320, Content = grid };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var accessor = ControlTokenAccessor.Capture<global::AtomUI.Desktop.Controls.DataGrid>(grid);
            accessor.GetOwn<Color>(DataGridTokenKind.BorderColor).ShouldBe(red);
            // TableBorderColor is derived from BorderColor inside CalculateTokenValues.
            accessor.GetOwn<Color>(DataGridTokenKind.TableBorderColor).ShouldBe(red);

            var frame = grid.GetVisualDescendants()
                            .OfType<PixelAlignedBorder>()
                            .Single(static border => border.Name == "Frame");
            var brush = frame.BorderBrush.ShouldNotBeNull().ShouldBeAssignableTo<ISolidColorBrush>();
            brush.Color.ShouldBe(red);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            await Apply(manager, null);
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
