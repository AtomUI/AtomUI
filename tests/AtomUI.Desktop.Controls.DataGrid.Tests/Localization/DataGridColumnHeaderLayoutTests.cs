using System.Reflection;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Localization;

public sealed class DataGridColumnHeaderLayoutTests
{
    private const string HeaderResourceKey = "DataGridColumnHeaderLayoutTests.Header";

    static DataGridColumnHeaderLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Auto_Column_Width_Recalculates_When_Header_Resource_Gets_Wider()
    {
        Application.Current!.Resources[HeaderResourceKey] = "ID";

        var column = new DataGridTextColumn
        {
            Width   = DataGridLength.Auto,
            Binding = new Binding(nameof(Row.Name))
        };
        BindDynamicResource(column, DataGridColumn.HeaderProperty, HeaderResourceKey, Application.Current);

        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            HeadersVisibility   = DataGridHeadersVisibility.Column,
            Width               = 640,
            Height              = 240,
            ItemsSource              = new TestDataGridSource<Row>([new Row("Alice")])
        };
        grid.Columns.Add(column);

        var window = new Window
        {
            Width   = 700,
            Height  = 340,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var initialWidth = column.ActualWidth;
            var headerCell   = column.HeaderCell;
            headerCell.Content.ShouldBe("ID");

            Application.Current.Resources[HeaderResourceKey] =
                "A localized column title that is substantially wider";
            Dispatcher.UIThread.RunJobs();

            headerCell.Content.ShouldBe("A localized column title that is substantially wider");
            column.ActualWidth.ShouldBeGreaterThan(initialWidth);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void BindDynamicResource(
        AvaloniaObject target,
        AvaloniaProperty property,
        object key,
        object? anchor)
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

    private sealed record Row(string Name);
}
