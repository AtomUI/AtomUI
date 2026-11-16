using System.Reflection;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using GridControl = AtomUI.Desktop.Controls.DataGrid;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Theme;

// One real typed-source fixture, compiled into the original headless tests and the existing Skia rendering host.
internal static class DataGridThemeTestFixture
{
    private static readonly DataGridFieldId NameField = new("name");

    internal static GridControl CreateGrid(string token = "")
    {
        var source = DataGridLocalSource.Create(
            Enumerable.Range(1, 18).Select(index => new EffectRow(index, $"Product {index:00} description")).ToArray(),
            DataGridLocalSourceDescriptor.For<EffectRow>(row => DataGridRowKey.FromInt64(row.Id))
                .Field(NameField, row => row.Name, filters:
                [new DataGridLocalFilter<string>(new DataGridOperatorId("equals"), 1, 16, DataGridScalarKinds.String,
                    (value, args) => args.Any(arg => arg.GetString() == value))], canGroup: true, setter: (row, value) => row.Name = value));
        var grid = new GridControl
        {
            AutoGenerateColumns = false, Width = 560, Height = 440, RowHeaderWidth = 40,
            ItemsSource = source, CanUserSortColumns = true, CanUserFilterColumns = true, CanUserReorderRows = true, IsFrameBorderVisible = true,
            HeadersVisibility = DataGridHeadersVisibility.All, GridLinesVisibility = DataGridGridLinesVisibility.All,
            SizeType = (token.EndsWith("SM", StringComparison.Ordinal) && token != "PaddingSM") || token.Contains("Small", StringComparison.Ordinal) || token == "PaddingXS"
                ? global::AtomUI.CustomizableSizeType.Small : token.EndsWith("MD", StringComparison.Ordinal) || token.Contains("Middle", StringComparison.Ordinal) || token == "PaddingSM"
                    ? global::AtomUI.CustomizableSizeType.Middle : global::AtomUI.CustomizableSizeType.Large,
            Title = "Product inventory", Footer = "Inventory summary",
            PaginationVisibility = DataGridPaginationVisibility.All, PageSize = 10,
            SelectionMode = DataGridSelectionMode.Extended,
            RowDetailsTemplate = new FuncDataTemplate<EffectRow>((row, _) => new global::AtomUI.Desktop.Controls.TextBlock
                { Text = $"Details for {row?.Name}" })
        };
        if (ExpanderToken(token) && !token.Contains("Expanded", StringComparison.Ordinal))
        {
            grid.Columns.Add(new DataGridDetailExpanderColumn());
        }
        if (token is "RowReorderIndicatorSize" or "SizeMD" or "IconSize" or "ColorBgTextHover" or "ColorBgTextActive")
        {
            grid.Columns.Add(new DataGridRowReorderColumn { Width = new DataGridLength(70) });
        }
        if (token == "SpacingXS")
        {
            grid.Columns.Add(new DataGridOperationColumn { Width = new DataGridLength(150) });
        }
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Product", FieldId = NameField, Binding = new Binding(nameof(EffectRow.Name)),
            Width = new DataGridLength(180), CanUserSort = true,
            Filters = new[] { new DataGridFilterItem { Text = "Products", Value = "Product" } }
        });
        for (var index = 0; index < 3; index++)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = $"Warehouse {index + 1}", Binding = new Binding(nameof(EffectRow.Name)),
                Width = new DataGridLength(180)
            });
        }
        if (token is "LeftFrozenShadows" or "RightFrozenShadows" or "ColorSplit")
        {
            grid.LeftFrozenColumnCount = 1;
            grid.RightFrozenColumnCount = 1;
        }
        if (token is "MarginSM" or "MarginXXS")
        {
            grid.Query = grid.Query.WithGroups([new DataGridGroup(NameField, DataGridSortDirection.Ascending)]);
        }
        if (token.Contains("Expanded", StringComparison.Ordinal))
        {
            grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible;
        }
        return grid;
    }

    internal static bool ExpanderToken(string token) => token.Contains("Expand", StringComparison.Ordinal) ||
        token is "LineWidth" or "SizeMS" or "BorderRadiusSM" or "ColorBorder" or "ColorTextSecondary" or
            "ColorPrimaryHover" or "ColorPrimaryActive";

    internal static bool PopupToken(string token) => token.Contains("Dropdown", StringComparison.Ordinal) ||
        token.StartsWith("TableFilter", StringComparison.Ordinal) || token is "ColorText" or "BorderRadius" or "UniformlyMarginXS";

    internal static Control Host(GridControl grid)
    {
        var layers = new VisualLayerManager { Child = new Avalonia.Controls.Border { Background = Brushes.DimGray, Child = grid } };
        typeof(VisualLayerManager).GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(layers, true);
        return layers;
    }

    internal static void Arrange(GridControl grid, string token)
    {
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && Rows(grid).Length > 0);
        var row = Rows(grid)[0];
        var header = grid.GetVisualDescendants().OfType<DataGridColumnHeader>().First(h => h.OwningColumn == grid.Columns.OfType<DataGridTextColumn>().First());
        if (PopupToken(token))
        {
            var indicator = grid.GetVisualDescendants().OfType<DataGridFilterIndicator>().First();
            grid.Columns.OfType<DataGridTextColumn>().First().FilterPresenterMode = token == "FilterDropdownMenuBg"
                ? DataGridFilterPresenterMode.Menu : DataGridFilterPresenterMode.Tree;
            grid.IsPopupPinnedOpen = true;
            PumpUntil(() => TopLevel.GetTopLevel(grid)!.GetVisualDescendants().OfType<Control>()
                .Any(p => p is DataGridTreeFilterFlyoutPresenter or DataGridMenuFilterFlyoutPresenter && p.IsEffectivelyVisible));
        }
        if (token is "LeftFrozenShadows" or "RightFrozenShadows" or "ColorSplit")
        {
            grid.UpdateHorizontalOffset(grid.HorizontalMaximizeOffset / 2).ShouldBeTrue();
        }
        if (token == "TableTopLeftColumnCornerRadius")
        {
            grid.GetVisualDescendants().OfType<DataGridTopLeftColumnHeader>().Single().Background = Brushes.Red;
        }
        if (token == "PaddingXL")
        {
            grid.ItemsSource = null;
        }
        if (token == "LineWidthBold")
        {
            row.DropPosition = DataGridRowDropPosition.Before;
        }
        if (token.Contains("Hover", StringComparison.Ordinal) || token == "ColorPrimaryActive")
        {
            foreach (var expander in grid.GetVisualDescendants().OfType<DataGridRowExpander>())
            {
                ((IPseudoClasses)expander.Classes).Add(token == "ColorPrimaryActive" ? ":pressed" : ":pointerover");
            }
        }
        if (token is "ColorBgTextHover" or "ColorBgTextActive")
        {
            ((IPseudoClasses)grid.GetVisualDescendants().OfType<RowReorderButton>().First().Classes)
                .Add(token == "ColorBgTextActive" ? ":pressed" : ":pointerover");
        }
        if (token is "HeaderFilterHoverBg" or "TableHeaderFilterActiveBg" or "ColorIconHover")
        {
            ((IPseudoClasses)grid.GetVisualDescendants().OfType<DataGridFilterIndicator>().First().Classes)
                .Add(token == "ColorBgTextActive" ? ":pressed" : ":pointerover");
        }
        if (token.Contains("Selected", StringComparison.Ordinal) || token.Contains("ItemBgActive", StringComparison.Ordinal) ||
            token.Contains("ItemColorActive", StringComparison.Ordinal))
        {
            grid.Selection = new DataGridSelectionState([DataGridRowKey.FromInt64(1)], null, [], []);
        }
        if (token.Contains("Hover", StringComparison.Ordinal) || token == "ColorIconHover")
        {
            ((IPseudoClasses)row.Classes).Add(":row-hover");
            ((IPseudoClasses)header.Classes).Add(":pointerover");
        }
        if (token.Contains("Sort", StringComparison.Ordinal) || token == "ColorFillSecondary" || token == "ColorPrimary")
        {
            grid.SetSort(NameField, DataGridSortDirection.Ascending);
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && grid.AppliedQuery == grid.Query);
        }
        if (token.Contains("Expanded", StringComparison.Ordinal))
        {
            Rows(grid)[0].IsDetailsVisible = true;
        }
        if (token == "ColumnReorderActiveBg" || token == "ColorFillContent")
        {
            row.Cells[grid.Columns.OfType<DataGridTextColumn>().First().Index].OwningColumnDragging = true;
        }
    }

    internal static DataGridRow[] Rows(GridControl grid) => grid.GetVisualDescendants().OfType<DataGridRow>()
        .Where(row => row.DataContext is EffectRow).OrderBy(row => row.Index).ToArray();

    internal static void PumpUntil(Func<bool> condition)
    {
        SpinWait.SpinUntil(() => { Dispatcher.UIThread.RunJobs(); return condition(); }, TimeSpan.FromSeconds(5))
            .ShouldBeTrue("The real source must have produced its displayed rows.");
    }

    private sealed class EffectRow(long id, string name)
    {
        public long Id { get; } = id;
        public string Name { get; set; } = name;
    }
}
