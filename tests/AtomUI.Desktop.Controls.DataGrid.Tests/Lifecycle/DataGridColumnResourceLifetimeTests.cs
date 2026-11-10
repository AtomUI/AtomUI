using System.Reflection;
using AtomUI.Desktop.Controls.Tests.DataGrid.Theme;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Lifecycle;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DataGridColumnResourceLifetimeCollection
{
    public const string Name = nameof(DataGridColumnResourceLifetimeCollection);
}

[Collection(DataGridColumnResourceLifetimeCollection.Name)]
public class DataGridColumnResourceLifetimeTests
{
    private const string HeaderResourceKey = "DataGridColumnResourceLifetimeTests.Header";

    static DataGridColumnResourceLifetimeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(typeof(DataGridTextColumn))]
    [InlineData(typeof(DataGridSelectionColumn))]
    [InlineData(typeof(DataGridDetailExpanderColumn))]
    public void Dynamic_Resource_Header_Does_Not_Root_Unreferenced_DataGrid_Column(Type columnType)
    {
        var columnReference = CreateUnrootedColumnReference(CreateResourceKey(), columnType);

        CollectGarbage();

        columnReference.IsAlive.ShouldBeFalse(
            "DataGridColumn dynamic resources must not be rooted by Application.ResourcesChanged after the page releases the column");
    }

    [Fact]
    public void Dynamic_Resource_Header_Updates_When_Application_Resource_Changes()
    {
        var resourceKey = CreateResourceKey();
        Application.Current!.Resources[resourceKey] = "Initial header";

        var column = new DataGridTextColumn();
        BindDynamicResource(column, DataGridColumn.HeaderProperty, resourceKey, Application.Current);

        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            Width               = 500,
            Height              = 220
        };
        grid.Columns.Add(column);

        var window = new Window
        {
            Width   = 640,
            Height  = 320,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            column.Header.ShouldBe("Initial header");

            Application.Current.Resources[resourceKey] = "Updated header";
            Dispatcher.UIThread.RunJobs();

            column.Header.ShouldBe("Updated header");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Dynamic_Resource_Header_Does_Not_Root_Unreferenced_DataGrid_Column_Group_Item()
    {
        var groupItemReference = CreateUnrootedColumnGroupItemReference(CreateResourceKey());

        CollectGarbage();

        groupItemReference.IsAlive.ShouldBeFalse(
            "DataGridColumnGroupItem dynamic resources must not be rooted by Application.ResourcesChanged after the page releases the group item");
    }

    [Fact]
    public void Column_And_Group_Use_Local_Provider_Resources_Through_Edit_Publish_And_Owner_Replacement()
    {
        var key = CreateResourceKey();
        Application.Current!.Resources[key] = Brushes.Purple;
        var first = DataGridThemeTestFixture.CreateGrid();
        var second = DataGridThemeTestFixture.CreateGrid();
        var column = first.Columns.OfType<DataGridTextColumn>().First();
        column.IsReadOnly = false;
        BindDynamicResource(column, DataGridAbstractTextColumn.ForegroundProperty, key, Application.Current);
        var group = new DataGridColumnGroupItem { OwningGrid = first };
        var firstScope = new ThemeConfigProvider { Child = first };
        var secondScope = new ThemeConfigProvider { Child = second };
        firstScope.Resources[key] = Brushes.Blue;
        secondScope.Resources[key] = Brushes.Green;
        var window = new Avalonia.Controls.Window { Width = 1200, Height = 600,
            Content = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal,
                Children = { firstScope, secondScope } } };
        try
        {
            // Source/columns can attach logically before any live TopLevel theme context, without eager snapshot capture.
            window.Show();
            DataGridThemeTestFixture.PumpUntil(() => first.LoadState == DataGridLoadState.Ready && second.LoadState == DataGridLoadState.Ready);
            column.Foreground.ShouldBe(Brushes.Blue);
            group.TryGetResource(key, null, out var groupValue).ShouldBeTrue();
            groupValue.ShouldBe(Brushes.Blue);
            var row = DataGridThemeTestFixture.Rows(first)[0];
            column.GetCellContent(row).ShouldBeOfType<global::AtomUI.Desktop.Controls.TextBlock>()
                .Foreground.ShouldBe(Brushes.Blue);
            first.UpdateSelectionAndCurrency(column.Index, row.Slot, DataGridSelectionAction.SelectCurrent, false).ShouldBeTrue();
            first.BeginEdit(new RoutedEventArgs()).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();
            var editor = column.GetCellContent(row).ShouldBeOfType<LineEdit>();
            editor.Foreground.ShouldBe(Brushes.Blue);
            ThemeScope.GetTokenOwner(editor).ShouldBe(LineEditTokens.Identity);
            firstScope.Resources[key] = Brushes.Orange;
            Dispatcher.UIThread.RunJobs();
            column.GetCellContent(row).ShouldBeOfType<LineEdit>().Foreground.ShouldBe(Brushes.Orange);
            first.CancelEdit(DataGridEditingUnit.Row, true).ShouldBeTrue();
            firstScope.Config = new ThemeConfigBuilder().WithControl(DataGridTokens.Identity,
                new ControlThemeConfigBuilder().WithToken("CellFontSize", "22").Build()).Build();
            DataGridThemeTestFixture.PumpUntil(() =>
                ControlTokenAccessor.Capture<global::AtomUI.Desktop.Controls.DataGrid>(first)
                    .GetOwn<double>(DataGridTokenKind.CellFontSize) == 22);
            column.GetCellContent(DataGridThemeTestFixture.Rows(first)[0]).ShouldBeOfType<global::AtomUI.Desktop.Controls.TextBlock>()
                .FontSize.ShouldBe(22);
            first.Columns.Remove(column);
            second.Columns.Insert(0, column);
            group.OwningGrid = second;
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            column.Foreground.ShouldBe(Brushes.Green);
            second.ItemsSource = first.ItemsSource;
            DataGridThemeTestFixture.PumpUntil(() => second.LoadState == DataGridLoadState.Ready);
            column.GetCellContent(DataGridThemeTestFixture.Rows(second)[0]).ShouldBeOfType<global::AtomUI.Desktop.Controls.TextBlock>()
                .Foreground.ShouldBe(Brushes.Green);
            group.TryGetResource(key, null, out groupValue).ShouldBeTrue();
            groupValue.ShouldBe(Brushes.Green);
            firstScope.Resources[key] = Brushes.Red;
            Dispatcher.UIThread.RunJobs();
            column.Foreground.ShouldBe(Brushes.Green, "The old owner's resource events must be released.");
            second.Columns.Remove(column);
            group.OwningGrid = null;
            Dispatcher.UIThread.RunJobs();
            column.Foreground.ShouldBe(Brushes.Purple, "A detached resource host keeps the documented application fallback.");
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateUnrootedColumnReference(string resourceKey, Type columnType)
    {
        Application.Current!.Resources[resourceKey] = "Localized header";

        var column = (DataGridColumn)Activator.CreateInstance(columnType)!;
        var grid = new global::AtomUI.Desktop.Controls.DataGrid { AutoGenerateColumns = false };
        grid.Columns.Add(column);
        BindDynamicResource(column, DataGridColumn.HeaderProperty, resourceKey, Application.Current);

        column.Header.ShouldBe("Localized header");
        return new WeakReference(column);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateUnrootedColumnGroupItemReference(string resourceKey)
    {
        Application.Current!.Resources[resourceKey] = "Localized group header";

        var groupItem = new DataGridColumnGroupItem();
        BindDynamicResource(groupItem, DataGridColumnGroupItem.HeaderProperty, resourceKey, Application.Current);

        groupItem.Header.ShouldBe("Localized group header");
        return new WeakReference(groupItem);
    }

    private static string CreateResourceKey()
    {
        return $"{HeaderResourceKey}.{Guid.NewGuid():N}";
    }

    private static void BindDynamicResource(AvaloniaObject target, AvaloniaProperty property, object key, object? anchor)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bindMethod = typeof(AvaloniaObject).GetMethod(
            "Bind",
            flags,
            binder: null,
            types: new[] { typeof(AvaloniaProperty), typeof(BindingBase), typeof(object) },
            modifiers: null);

        bindMethod.ShouldNotBeNull();

        var extension = new DynamicResourceExtension(key);
        var anchorField = typeof(DynamicResourceExtension).GetField("_anchor", flags);
        anchorField.ShouldNotBeNull();
        anchorField.SetValue(extension, anchor);

        bindMethod.Invoke(target, new object?[] { property, extension, anchor });
    }

    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Dispatcher.UIThread.RunJobs();
    }
}
