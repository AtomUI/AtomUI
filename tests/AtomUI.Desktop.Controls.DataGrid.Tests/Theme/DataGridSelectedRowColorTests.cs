using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Theme;

/// <summary>
/// The selected row's text: the DataGrid <c>RowSelectedColor</c> Own token, which defaults to the global
/// <c>ControlItemColorActive</c> (itself <c>ColorText</c> by default), drawn on <c>RowSelectedBg</c>.
/// </summary>
public class DataGridSelectedRowColorTests
{
    static DataGridSelectedRowColorTests() => AvaloniaTestApp.EnsureInitialized();

    public sealed record Item(string Name);

    [Fact]
    public async Task Selected_Row_Text_Follows_ControlItemColorActive()
    {
        var manager = Application.Current!.GetThemeManager().ShouldBeOfType<ThemeManager>();
        await Apply(manager, null);
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = new TestDataGridSource<Item>(new[] { new Item("one"), new Item("two") }),
            Width = 360,
            Height = 220
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Item.Name)) });
        var window = new AvaloniaWindow { Width = 420, Height = 280, Content = grid };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            grid.GetVisualDescendants().OfType<DataGridRow>().First().IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            var accessor = ControlTokenAccessor.Capture<global::AtomUI.Desktop.Controls.DataGrid>(grid);
            var text = accessor.GetEffectiveGlobal<Color>(SharedTokenKind.ColorText);
            accessor.GetEffectiveGlobal<Color>(SharedTokenKind.ControlItemColorActive).ShouldBe(text);
            accessor.GetOwn<Color>(DataGridTokenKind.RowSelectedColor).ShouldBe(text);
            SelectedRowForeground(grid).ShouldBe(text);

            await Apply(manager, new ThemeConfigBuilder().WithToken("ControlItemColorActive", "#FF123456").Build());
            Dispatcher.UIThread.RunJobs();
            ControlTokenAccessor.Capture<global::AtomUI.Desktop.Controls.DataGrid>(grid)
                                .GetOwn<Color>(DataGridTokenKind.RowSelectedColor).ShouldBe(Color.Parse("#FF123456"));
            SelectedRowForeground(grid).ShouldBe(Color.Parse("#FF123456"));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            await Apply(manager, null);
        }
    }

    private static Color? SelectedRowForeground(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        (grid.GetVisualDescendants().OfType<DataGridRow>().Single(row => row.IsSelected).Foreground as ISolidColorBrush)?.Color;

    private static async Task Apply(ThemeManager manager, ThemeConfig? config)
    {
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
    }
}
