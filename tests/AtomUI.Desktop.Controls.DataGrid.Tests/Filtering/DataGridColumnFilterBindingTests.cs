using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Data;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Filtering;

public class DataGridColumnFilterBindingTests
{
    private static readonly DataGridFieldId AddressField = new("address");
    private static readonly DataGridOperatorId EqualsOperator = new("equals");

    static DataGridColumnFilterBindingTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Filters_Can_Be_Assigned_From_ViewModel_Source()
    {
        var filters = CreateFilters();
        var column = CreateColumn(filters: filters);

        column.Filters.ShouldBeSameAs(filters);
    }

    [Fact]
    public void Filters_Use_Explicit_Filter_Items_For_Hierarchies()
    {
        var parent = new DataGridFilterItem { Text = "Europe", Value = "Europe" };
        parent.Children.Add(new DataGridFilterItem { Text = "London", Value = "London" });
        var column = CreateColumn(filters: new[] { parent });

        var item = column.GetEffectiveFilterItems().Single();

        item.ShouldBeSameAs(parent);
        item.Children.Single().Text.ShouldBe("London");
    }

    [Fact]
    public void Selected_Filter_Values_Project_To_Immutable_Query()
    {
        var selectedValues = new ObservableCollection<object>();
        var column = CreateColumn(CreateFilters(), selectedValues);
        var (window, grid, source) = Show(column);
        try
        {
            selectedValues.Add("London");
            Dispatcher.UIThread.RunJobs();

            GetSingleFilter(grid).ShouldBe(new DataGridFilter(
                AddressField,
                EqualsOperator,
                ImmutableArray.Create(DataGridScalar.FromString("London"))));

            selectedValues.Clear();
            Dispatcher.UIThread.RunJobs();
            grid.Query.Filters.ShouldBeEmpty();
        }
        finally
        {
            Close(window, source);
        }
    }

    [Fact]
    public void Filter_Item_Removal_Prunes_Query_Without_Member_Path_Reflection()
    {
        var filters = CreateFilters();
        var selectedValues = new ObservableCollection<object> { "London", "New York" };
        var column = CreateColumn(filters, selectedValues);
        var (window, grid, source) = Show(column);
        try
        {
            filters.RemoveAt(0);
            Dispatcher.UIThread.RunJobs();

            GetSingleFilter(grid).Values.ShouldBe([
                DataGridScalar.FromString("New York")]);

            filters.Clear();
            Dispatcher.UIThread.RunJobs();
            grid.Query.Filters.ShouldBeEmpty();
        }
        finally
        {
            Close(window, source);
        }
    }

    [Fact]
    public void Column_Filter_Method_Updates_Query_And_Clear_Removes_Projection()
    {
        var column = CreateColumn(CreateFilters());
        var (window, grid, source) = Show(column);
        try
        {
            column.Filter(["New York"]);
            Dispatcher.UIThread.RunJobs();
            GetSingleFilter(grid).Values.ShouldBe([
                DataGridScalar.FromString("New York")]);

            column.ClearFilter();
            Dispatcher.UIThread.RunJobs();
            grid.Query.Filters.ShouldBeEmpty();
        }
        finally
        {
            Close(window, source);
        }
    }

    [Fact]
    public void Column_Filter_Bindings_Use_OwningGrid_DataContext()
    {
        var firstFilters = CreateFilters();
        var firstSelectedValues = new ObservableCollection<object> { "London" };
        var column = CreateColumn();
        column.Bind(DataGridColumn.FiltersProperty, new Binding(nameof(FilterBindingViewModel.AddressFilters)));
        column.Bind(
            DataGridColumn.SelectedFilterValuesProperty,
            new Binding(nameof(FilterBindingViewModel.SelectedAddresses)));
        var firstViewModel = new FilterBindingViewModel
        {
            AddressFilters = firstFilters,
            SelectedAddresses = firstSelectedValues
        };
        var (window, grid, source) = Show(column, firstViewModel);
        try
        {
            column.Filters.ShouldBeSameAs(firstFilters);
            GetSingleFilter(grid).Values.ShouldBe([
                DataGridScalar.FromString("London")]);

            var secondFilters = new ObservableCollection<DataGridFilterItem>
            {
                new() { Text = "Tokyo", Value = "Tokyo" }
            };
            var secondSelectedValues = new ObservableCollection<object> { "Tokyo" };
            grid.DataContext = new FilterBindingViewModel
            {
                AddressFilters = secondFilters,
                SelectedAddresses = secondSelectedValues
            };
            Dispatcher.UIThread.RunJobs();

            column.Filters.ShouldBeSameAs(secondFilters);
            column.SelectedFilterValues.ShouldBeSameAs(secondSelectedValues);
            GetSingleFilter(grid).Values.ShouldBe([
                DataGridScalar.FromString("Tokyo")]);
        }
        finally
        {
            Close(window, source);
        }
    }

    [Fact]
    public void Column_Removal_Releases_Filter_Collection_Subscriptions()
    {
        var filters = new TrackedObservableCollection<DataGridFilterItem>
        {
            new() { Text = "London", Value = "London" }
        };
        var selectedValues = new TrackedObservableCollection<object> { "London" };
        var column = CreateColumn(filters, selectedValues);
        var (window, grid, source) = Show(column);
        try
        {
            var filterSubscriptionCount = filters.CollectionChangedHandlerCount;
            filterSubscriptionCount.ShouldBeGreaterThan(0);
            selectedValues.CollectionChangedHandlerCount.ShouldBe(1);

            grid.Columns.Remove(column);
            Dispatcher.UIThread.RunJobs();

            filters.CollectionChangedHandlerCount.ShouldBeLessThan(filterSubscriptionCount);
            selectedValues.CollectionChangedHandlerCount.ShouldBe(0);
        }
        finally
        {
            Close(window, source);
        }
    }

    private static DataGridTextColumn CreateColumn(
        IEnumerable? filters = null,
        IList? selectedValues = null) => new()
    {
        Header = "Address",
        FieldId = AddressField,
        Binding = new Binding(nameof(FilterRow.Address)),
        Filters = filters,
        SelectedFilterValues = selectedValues
    };

    private static ObservableCollection<DataGridFilterItem> CreateFilters() =>
    [
        new() { Text = "London", Value = "London" },
        new() { Text = "New York", Value = "New York" }
    ];

    private static (Window Window, global::AtomUI.Desktop.Controls.DataGrid Grid, DataGridLocalSource<FilterRow> Source)
        Show(DataGridTextColumn column, object? dataContext = null)
    {
        var rows = new[] { new FilterRow(1, "London"), new FilterRow(2, "New York"), new FilterRow(3, "Tokyo") };
        var filter = new DataGridLocalFilter<string>(
            EqualsOperator,
            1,
            16,
            DataGridScalarKinds.String,
            static (value, values) => values.Contains(DataGridScalar.FromString(value)));
        var descriptor = DataGridLocalSourceDescriptor.For<FilterRow>(
                static row => DataGridRowKey.FromInt64(row.Id))
            .Field(
                AddressField,
                static row => row.Address,
                filters: ImmutableArray.Create(filter));
        var source = DataGridLocalSource.Create(rows, descriptor);
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            CanUserFilterColumns = true,
            DataContext = dataContext,
            ItemsSource = source,
            Width = 480,
            Height = 240
        };
        grid.Columns.Add(column);
        var window = new Window { Width = 520, Height = 280, Content = grid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, grid, source);
    }

    private static DataGridFilter GetSingleFilter(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        grid.Query.Filters.Length.ShouldBe(1);
        return grid.Query.Filters[0];
    }

    private static void Close(Window window, IDisposable source)
    {
        window.Close();
        Dispatcher.UIThread.RunJobs();
        source.Dispose();
    }
}

internal sealed record FilterRow(long Id, string Address);

internal sealed class FilterBindingViewModel
{
    public ObservableCollection<DataGridFilterItem>? AddressFilters { get; set; }

    public IList? SelectedAddresses { get; set; }
}

internal sealed class TrackedObservableCollection<T> : ObservableCollection<T>
{
    public int CollectionChangedHandlerCount { get; private set; }

    public override event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add
        {
            CollectionChangedHandlerCount++;
            base.CollectionChanged += value;
        }
        remove
        {
            CollectionChangedHandlerCount--;
            base.CollectionChanged -= value;
        }
    }
}
