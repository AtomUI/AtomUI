using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Tests.DataGrid.Theme;
using AtomUI.Desktop.Controls.Rendering.Tests;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using GridControl = AtomUI.Desktop.Controls.DataGrid;
using ButtonControl = AtomUI.Desktop.Controls.Button;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public class DataGridFamilyScopeTests
{
    [Fact]
    public void Local_Control_Override_Root_And_Nested_Public_Controls_Keep_Their_Boundaries()
    {
        var grid = DataGridThemeTestFixture.CreateGrid("ColorPrimary");
        var nested = new ButtonControl { ButtonType = ButtonType.Primary, Content = "Add product" };
        var unrelated = new ButtonControl { ButtonType = ButtonType.Primary, Content = "Unrelated" };
        grid.Title = nested;
        grid.Columns.Insert(0, new DataGridSelectionColumn());
        var panel = new StackPanel { Children = { grid, unrelated } };
        var provider = new ThemeConfigProvider { Config = Config(), Child = panel };
        var window = new Avalonia.Controls.Window { Width = 800, Height = 650, Content = provider };
        try
        {
            window.Show();
            DataGridThemeTestFixture.Arrange(grid, "ColorPrimary");
            grid.Selection = new DataGridSelectionState([DataGridRowKey.FromInt64(1)], null, [], []);
            FrameCapture.Refresh(window);
            var selector = DataGridThemeTestFixture.Rows(grid)[0].Cells[0].Content.ShouldNotBeNull().ShouldBeAssignableTo<CheckBox>()!;
            using var before = FrameCapture.Capture(window);
            provider.Config = Config("#FFFF0000");
            WaitForColor(grid, Colors.Red);
            FrameCapture.Refresh(window);
            using var own = FrameCapture.Capture(window);
            Differences(before, own, grid, window).ShouldBeGreaterThan(0);
            Differences(before, own, nested, window).ShouldBe(0);
            Differences(before, own, selector, window).ShouldBe(0);
            ThemeScope.GetTokenOwner(selector).ShouldBe(CheckBoxTokens.Identity);
            Differences(before, own, unrelated, window).ShouldBe(0);
            ThemeScope.GetTokenOwner(grid.GetVisualDescendants().OfType<DataGridRow>().First()).ShouldBe(DataGridTokens.Identity);
            ThemeScope.GetTokenOwner(grid.GetVisualDescendants().OfType<Spin>().Single()).ShouldBe(SpinTokens.Identity);
            ThemeScope.GetTokenOwner(nested).ShouldBe(ButtonTokens.Identity);

            provider.Config = Config(root: "#FF008000");
            WaitForColor(grid, Colors.Green);
            FrameCapture.Refresh(window);
            using var root = FrameCapture.Capture(window);
            Differences(own, root, grid, window).ShouldBeGreaterThan(0);
            Differences(own, root, nested, window).ShouldBeGreaterThan(0);
            Differences(own, root, selector, window).ShouldBeGreaterThan(0);
            Differences(own, root, unrelated, window).ShouldBeGreaterThan(0);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Content_Owner_And_Real_Row_Transitions_Follow_Publish_Detach_And_Remount()
    {
        var grid = DataGridThemeTestFixture.CreateGrid();
        var provider = new ThemeConfigProvider { Config = MotionConfig("00:00:00.4000000"), Child = grid };
        var window = new Avalonia.Controls.Window { Width = 800, Height = 600, Content = provider };
        try
        {
            window.Show();
            DataGridThemeTestFixture.PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            AssertDuration(grid, TimeSpan.FromMilliseconds(400));
            provider.Config = MotionConfig("00:00:00.6000000");
            DataGridThemeTestFixture.PumpUntil(() => ((TransitionBase)DataGridThemeTestFixture.Rows(grid)[0].Transitions![0]).Duration == TimeSpan.FromMilliseconds(600));
            AssertDuration(grid, TimeSpan.FromMilliseconds(600));
            provider.Config = new ThemeConfigBuilder().WithToken("MotionDurationMid", "00:00:00.8000000").Build();
            DataGridThemeTestFixture.PumpUntil(() => ((TransitionBase)DataGridThemeTestFixture.Rows(grid)[0].Transitions![0]).Duration == TimeSpan.FromMilliseconds(800));
            AssertDuration(grid, TimeSpan.FromMilliseconds(800));
            provider.Config = MotionConfig("00:00:00.6000000");
            Dispatcher.UIThread.RunJobs();
            var oldContent = grid.GetVisualDescendants().OfType<DockPanel>().Single(p => p.Name == "GridContent");
            provider.Child = null;
            Dispatcher.UIThread.RunJobs();
            oldContent.IsSet(ThemeScope.TokenOwnerProperty).ShouldBeFalse("Detach must release the owner forwarding binding.");
            provider.Child = grid;
            DataGridThemeTestFixture.PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            AssertDuration(grid, TimeSpan.FromMilliseconds(600));
            grid.Template = (IControlTemplate)new global::AtomUI.Desktop.Controls.Themes.DataGridTheme { TargetType = typeof(GridControl) }.Setters
                .OfType<Setter>().Single(setter => setter.Property == TemplatedControl.TemplateProperty).Value!;
            FrameCapture.Refresh(window);
            oldContent.IsSet(ThemeScope.TokenOwnerProperty).ShouldBeFalse("Re-template must release the old content's owner binding.");
            AssertDuration(grid, TimeSpan.FromMilliseconds(600));
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static void AssertDuration(GridControl grid, TimeSpan duration)
    {
        foreach (var row in DataGridThemeTestFixture.Rows(grid))
        {
            ThemeScope.GetTokenOwner(row).ShouldBe(DataGridTokens.Identity);
            row.Transitions.ShouldNotBeNull().ShouldNotBeEmpty();
            row.Transitions.Cast<TransitionBase>().ShouldAllBe(transition => transition.Duration == duration);
        }
    }

    private static ThemeConfig MotionConfig(string duration) => new ThemeConfigBuilder()
        .WithControl(DataGridTokens.Identity, new ControlThemeConfigBuilder().WithToken("MotionDurationMid", duration).Build()).Build();

    private static ThemeConfig Config(string? own = null, string? root = null)
    {
        var builder = new ThemeConfigBuilder().WithToken("EnableMotion", "false");
        if (own is not null) { builder.WithControl(DataGridTokens.Identity, new ControlThemeConfigBuilder().WithToken("ColorPrimary", own).Build()); }
        if (root is not null) { builder.WithToken("ColorPrimary", root); }
        return builder.Build();
    }

    private static void WaitForColor(GridControl grid, Color color) => DataGridThemeTestFixture.PumpUntil(() =>
        ControlTokenAccessor.Capture<GridControl>(grid).GetEffectiveGlobal<Color>(SharedTokenKind.ColorPrimary) == color);

    private static int Differences(SKBitmap before, SKBitmap after, Control control, Avalonia.Controls.Window window)
    {
        var point = control.TranslatePoint(default, window)!.Value;
        return FrameCapture.CountDifferentPixels(before, after, new SKRectI((int)point.X, (int)point.Y,
            (int)Math.Ceiling(point.X + control.Bounds.Width), (int)Math.Ceiling(point.Y + control.Bounds.Height)), 8);
    }
}
