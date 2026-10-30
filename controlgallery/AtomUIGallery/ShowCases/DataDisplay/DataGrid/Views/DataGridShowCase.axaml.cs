using System.Collections;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AtomDataGrid = AtomUI.Desktop.Controls.DataGrid;

namespace AtomUIGallery.ShowCases.DataGrid;

public partial class DataGridShowCase : GalleryReactiveUserControl<DataGridViewModel>
{
    public const string LanguageId = nameof(DataGridShowCase);

    private static int s_cellsEditableNewRowIndex = 1;

    public DataGridShowCase()
    {
        InitializeComponent();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is DataGridViewModel viewModel)
        {
            DataGridShowCaseDataSources.ClearAll(viewModel);
        }
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RefreshMaterializedExampleDataGrids();
    }

    private void HandleExampleDataGridAttached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is AtomDataGrid dataGrid)
        {
            SetExampleDataGridSource(dataGrid);
        }
    }

    private void RefreshMaterializedExampleDataGrids()
    {
        foreach (var dataGrid in ExamplesContent.GetVisualDescendants().OfType<AtomDataGrid>())
        {
            SetExampleDataGridSource(dataGrid);
        }
    }

    private void SetExampleDataGridSource(AtomDataGrid dataGrid)
    {
        if (DataContext is not DataGridViewModel viewModel)
        {
            dataGrid.ItemsSource = null;
            ConfigureFilterColumns(dataGrid, null);
            return;
        }

        switch (dataGrid.Name)
        {
            case "SemanticCaseGrid":
            case "SemanticStyleCaseGridA":
            case "SemanticStyleCaseGridB":
                DataGridShowCaseDataSources.EnsureSemanticDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.SemanticCaseDataSource?.Source;
                break;

            case "BasicCaseGrid":
            case "SelectionDataGrid":
            case "DragResizeColumn":
            case "LargeSizeDataGrid":
            case "MiddleSizeDataGrid":
            case "SmallSizeDataGrid":
            case "CustomHeaderAndFooterDataGrid":
            case "ResetFilterAndSortGrid":
            case "HideColumnDataGrid":
                DataGridShowCaseDataSources.EnsureBasicDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.BasicCaseDataSource?.Source;
                break;
            case "FilterAndSortGrid":
            case "FilterInTreeGrid":
                DataGridShowCaseDataSources.EnsureFilterAndSorterDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.FilterAndSorterDataSource?.Source;
                break;
            case "MultiSorterDataGrid":
                DataGridShowCaseDataSources.EnsureMultiSorterDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.MultiSorterDataSource?.Source;
                break;
            case "ExpandableDataGrid":
            case "OrderSpecificColumnDataGrid":
            case "RowAndColumnHeaderDataGrid":
                DataGridShowCaseDataSources.EnsureExpandableRowDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.ExpandableRowDataSource?.Source;
                break;
            case "GroupHeaderDataGrid":
                DataGridShowCaseDataSources.EnsureGroupHeaderDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.GroupHeaderDataSource?.Source;
                break;
            case "FixedHeaderDataGrid":
                DataGridShowCaseDataSources.EnsureFixedHeaderDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.FixedHeaderDataSource?.Source;
                break;
            case "FixedColumnsDataGrid1":
            case "FixedColumnsDataGrid2":
                DataGridShowCaseDataSources.EnsureFixedColumnsDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.FixedColumnsDataSource?.Source;
                break;
            case "FixedColumnsAndHeadersDataGrid":
                DataGridShowCaseDataSources.EnsureFixedColumnsAndHeadersDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.FixedColumnsAndHeadersDataSource?.Source;
                break;
            case "DragColumnDataGrid1":
            case "DragColumnDataGrid2":
            case "DragColumnDataGrid3":
                DataGridShowCaseDataSources.EnsureDragColumnDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.DragColumnDataSource?.Source;
                break;
            case "DragRowDataGrid1":
                DataGridShowCaseDataSources.EnsureDragRowDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.DragRowDataSource?.Source;
                break;
            case "DragRowDataGrid2":
                DataGridShowCaseDataSources.EnsureDragRowDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.DragRowManyDataSource?.Source;
                break;
            case "CustomEmptyDataGrid":
                DataGridShowCaseDataSources.EnsureCustomEmptyDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.CustomEmptyDataSource?.Source;
                break;
            case "EditableCellsDataGrid":
                DataGridShowCaseDataSources.EnsureEditableCellsDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.EditableCellsDataSource?.Source;
                break;
            case "BasicPagingCaseGrid":
                DataGridShowCaseDataSources.EnsurePagingGridDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.PagingGridDataSource?.Source;
                break;
            case "RemoteRangeDataGrid":
                DataGridShowCaseDataSources.EnsureRemoteDataSource(viewModel);
                dataGrid.ItemsSource = viewModel.RemoteDataSource;
                break;
        }

        ConfigureFilterColumns(dataGrid, viewModel);
    }

    private static void ConfigureFilterColumns(AtomDataGrid dataGrid, DataGridViewModel? viewModel)
    {
        switch (dataGrid.Name)
        {
            case "FilterAndSortGrid":
                ConfigureFilterColumn(dataGrid, 0, viewModel?.NameFilters, viewModel?.FilterAndSorterSelectedNames);
                ConfigureFilterColumn(dataGrid, 2, viewModel?.AddressFilters, viewModel?.FilterAndSorterSelectedAddresses);
                break;
            case "FilterInTreeGrid":
                ConfigureFilterColumn(dataGrid, 0, viewModel?.NameFilters, viewModel?.TreeFilterSelectedNames);
                ConfigureFilterColumn(dataGrid, 2, viewModel?.AddressFilters, viewModel?.TreeFilterSelectedAddresses);
                break;
            case "ResetFilterAndSortGrid":
                ConfigureFilterColumn(dataGrid, 0, viewModel?.NameFilters, viewModel?.ResetSelectedNames);
                ConfigureFilterColumn(dataGrid, 2, viewModel?.AddressFilters, viewModel?.ResetSelectedAddresses);
                break;
        }
    }

    private static void ConfigureFilterColumn(AtomDataGrid dataGrid, int columnIndex, IEnumerable? filters, IList? selectedValues)
    {
        if (columnIndex >= dataGrid.Columns.Count)
        {
            return;
        }

        var column = dataGrid.Columns[columnIndex];
        column.SelectedFilterValues = selectedValues;
        column.Filters              = filters;
    }

    private void HandleSelectionModeCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not AtomUIRadioButton radioButton ||
            radioButton.IsChecked != true)
        {
            return;
        }

        var selectionDataGrid = FindNamedControlNearSender<AtomDataGrid>(sender, "SelectionDataGrid");
        if (selectionDataGrid is null)
        {
            return;
        }

        selectionDataGrid.SelectionMode = radioButton.Name == "ExtendedSelection"
            ? DataGridSelectionMode.Extended
            : DataGridSelectionMode.Single;
    }

    private void HandleSortAgeBtnClick(object? sender, RoutedEventArgs? eventArgs)
    {
        FindNamedControlNearSender<AtomDataGrid>(sender, "ResetFilterAndSortGrid")
            ?.SetSort(DataGridShowCaseFields.Age, DataGridSortDirection.Descending);
    }

    private void HandleClearFiltersBtnClick(object? sender, RoutedEventArgs? eventArgs)
    {
        FindNamedControlNearSender<AtomDataGrid>(sender, "ResetFilterAndSortGrid")
            ?.ClearFilters();
    }

    private void HandleClearFiltersAndSortersBtnClick(object? sender, RoutedEventArgs? eventArgs)
    {
        var dataGrid = FindNamedControlNearSender<AtomDataGrid>(sender, "ResetFilterAndSortGrid");
        dataGrid?.ClearFilters();
        dataGrid?.ClearSorts();
    }

    private void HandleColumnVisibleChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not AtomUICheckBox checkBox)
        {
            return;
        }

        var dataGrid = FindNamedControlNearSender<AtomDataGrid>(sender, "HideColumnDataGrid");
        if (dataGrid is null)
        {
            return;
        }

        var columnIndex = checkBox.Name switch
        {
            "ColumnCheckBox1" => 0,
            "ColumnCheckBox2" => 1,
            "ColumnCheckBox3" => 2,
            "ColumnCheckBox4" => 3,
            "ColumnCheckBox5" => 4,
            "ColumnCheckBox6" => 5,
            _                 => -1
        };

        if (columnIndex >= 0 && columnIndex < dataGrid.Columns.Count)
        {
            dataGrid.Columns[columnIndex].IsVisible = checkBox.IsChecked == true;
        }
    }

    private void HandleToggleEmptyGridSource(object? sender, RoutedEventArgs? eventArgs)
    {
        var dataGrid = FindNamedControlNearSender<AtomDataGrid>(sender, "CustomEmptyDataGrid");
        if (dataGrid is null)
        {
            return;
        }

        if (dataGrid.ItemsSource is not null)
        {
            dataGrid.ItemsSource = null;
        }
        else if (DataContext is DataGridViewModel viewModel)
        {
            DataGridShowCaseDataSources.EnsureCustomEmptyDataSource(viewModel);
            dataGrid.ItemsSource = viewModel.CustomEmptyDataSource?.Source;
        }
    }

    private void HandleToggleLoadingState(object? sender, RoutedEventArgs? eventArgs)
    {
        var dataGrid = FindNamedControlNearSender<AtomDataGrid>(sender, "CustomEmptyDataGrid");
        if (dataGrid is not null)
        {
            dataGrid.IsOperating = !dataGrid.IsOperating;
        }
    }

    private void HandleAddARowToCellsEditableGrid(object? sender, RoutedEventArgs? eventArgs)
    {
        if (DataContext is not DataGridViewModel viewModel)
        {
            return;
        }

        DataGridShowCaseDataSources.EnsureEditableCellsDataSource(viewModel);
        viewModel.EditableCellsDataSource?.Rows.Add(new DataGridBaseInfo
        {
            Key     = $"editable-new-{s_cellsEditableNewRowIndex}",
            Address = $"London, Park Lane no. {s_cellsEditableNewRowIndex}",
            Name    = $"Edward King {s_cellsEditableNewRowIndex}",
            Age     = 32
        });
        s_cellsEditableNewRowIndex++;
    }

    private void HandleReloadRemoteRangeSource(object? sender, RoutedEventArgs? eventArgs)
    {
        if (DataContext is DataGridViewModel viewModel)
        {
            DataGridShowCaseDataSources.EnsureRemoteDataSource(viewModel);
            viewModel.RemoteDataSource?.Reload();
        }
    }

    private void HandleFailRemoteRangeSource(object? sender, RoutedEventArgs? eventArgs)
    {
        if (DataContext is DataGridViewModel viewModel)
        {
            DataGridShowCaseDataSources.EnsureRemoteDataSource(viewModel);
            viewModel.RemoteDataSource?.ReloadWithFailure();
        }
    }

    private void HandleExpireRemoteRangeSnapshot(object? sender, RoutedEventArgs? eventArgs)
    {
        if (DataContext is DataGridViewModel viewModel)
        {
            DataGridShowCaseDataSources.EnsureRemoteDataSource(viewModel);
            viewModel.RemoteDataSource?.ExpireSnapshot();
        }
    }

    private void HandleRemoveRowCellsEditableGrid(object? sender, RoutedEventArgs? eventArgs)
    {
        if (sender is not AtomUIPopupConfirm { DataContext: int index })
        {
            return;
        }

        if (DataContext is DataGridViewModel viewModel &&
            viewModel.EditableCellsDataSource is { } dataSource &&
            index >= 0 &&
            index < dataSource.Rows.Count)
        {
            dataSource.Rows.RemoveAt(index);
        }
    }

    private void HandleTopPaginationAlignChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        var dataGrid = FindNamedControlNearSender<AtomDataGrid>(sender, "BasicPagingCaseGrid");
        if (dataGrid is not null)
        {
            dataGrid.TopPaginationAlign = ToPaginationAlign(args.Index);
        }
    }

    private void HandleBottomPaginationAlignChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        var dataGrid = FindNamedControlNearSender<AtomDataGrid>(sender, "BasicPagingCaseGrid");
        if (dataGrid is not null)
        {
            dataGrid.BottomPaginationAlign = ToPaginationAlign(args.Index);
        }
    }

    private void HandleShowTopPaginationCheckBoxChanged(object? sender, RoutedEventArgs args)
    {
        UpdatePaginationVisibility(sender, DataGridPaginationVisibility.Top);
    }

    private void HandleShowBottomPaginationCheckBoxChanged(object? sender, RoutedEventArgs args)
    {
        UpdatePaginationVisibility(sender, DataGridPaginationVisibility.Bottom);
    }

    private static PaginationAlign ToPaginationAlign(int index)
    {
        return index switch
        {
            0 => PaginationAlign.Start,
            1 => PaginationAlign.Center,
            _ => PaginationAlign.End
        };
    }

    private static T? FindNamedControlNearSender<T>(object? sender, string name)
        where T : Control
    {
        if (sender is not Visual visual)
        {
            return null;
        }

        var showCaseItem = visual.FindAncestorOfType<ShowCaseItem>(includeSelf: true);
        return showCaseItem?.GetVisualDescendants()
                            .OfType<T>()
                            .FirstOrDefault(control => control.Name == name);
    }

    private void UpdatePaginationVisibility(object? sender, DataGridPaginationVisibility position)
    {
        if (sender is not AtomUICheckBox checkBox)
        {
            return;
        }

        var dataGrid = FindNamedControlNearSender<AtomDataGrid>(sender, "BasicPagingCaseGrid");
        if (dataGrid is null)
        {
            return;
        }

        if (checkBox.IsChecked == true)
        {
            dataGrid.PaginationVisibility |= position;
        }
        else
        {
            dataGrid.PaginationVisibility &= ~position;
        }
    }
}

internal static class DataGridShowCaseDataSources
{
    public static void ClearAll(DataGridViewModel viewModel)
    {
        viewModel.BasicCaseDataSource?.Dispose();
        viewModel.FilterAndSorterDataSource?.Dispose();
        viewModel.MultiSorterDataSource?.Dispose();
        viewModel.ExpandableRowDataSource?.Dispose();
        viewModel.GroupHeaderDataSource?.Dispose();
        viewModel.FixedHeaderDataSource?.Dispose();
        viewModel.FixedColumnsDataSource?.Dispose();
        viewModel.FixedColumnsAndHeadersDataSource?.Dispose();
        viewModel.DragColumnDataSource?.Dispose();
        viewModel.DragRowDataSource?.Dispose();
        viewModel.DragRowManyDataSource?.Dispose();
        viewModel.CustomEmptyDataSource?.Dispose();
        viewModel.EditableCellsDataSource?.Dispose();
        viewModel.EditableRowsDataSource?.Dispose();
        viewModel.PagingGridDataSource?.Dispose();
        viewModel.RemoteDataSource?.Dispose();
        viewModel.SemanticCaseDataSource?.Dispose();
        viewModel.BasicCaseDataSource              = null;
        viewModel.SemanticCaseDataSource           = null;
        viewModel.FilterAndSorterDataSource        = null;
        viewModel.MultiSorterDataSource            = null;
        viewModel.ExpandableRowDataSource          = null;
        viewModel.GroupHeaderDataSource            = null;
        viewModel.FixedHeaderDataSource            = null;
        viewModel.FixedColumnsDataSource           = null;
        viewModel.FixedColumnsAndHeadersDataSource = null;
        viewModel.DragColumnDataSource             = null;
        viewModel.DragRowDataSource                = null;
        viewModel.DragRowManyDataSource            = null;
        viewModel.CustomEmptyDataSource            = null;
        viewModel.EditableCellsDataSource          = null;
        viewModel.EditableRowsDataSource           = null;
        viewModel.PagingGridDataSource             = null;
        viewModel.RemoteDataSource                 = null;
    }

    public static void EnsureBasicDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.BasicCaseDataSource is not null)
        {
            return;
        }

        List<DataGridBaseInfo> items =
        [
            new DataGridBaseInfo
            {
                Key   = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park",
                Money = "￥300,000.00",
                Tags =
                [
                    new TagInfo { Name = "NICE", Color      = "green" },
                    new TagInfo { Name = "DEVELOPER", Color = "geekblue" }
                ]
            },
            new DataGridBaseInfo
            {
                Key   = "2", Name = "Jim Green", Age = 42, Address = "London No. 1 Lake Park",
                Money = "￥1,256,000.00",
                Tags =
                [
                    new TagInfo { Name = "LOSER", Color = "volcano" }
                ]
            },
            new DataGridBaseInfo
            {
                Key   = "3", Name = "Joe Black", Age = 32, Address = "Sydney No. 1 Lake Park",
                Money = "￥120,000.00",
                Tags =
                [
                    new TagInfo { Name = "COOL", Color    = "green" },
                    new TagInfo { Name = "TEACHER", Color = "geekblue" }
                ]
            }
        ];
        viewModel.BasicCaseDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureSemanticDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.SemanticCaseDataSource is not null)
        {
            return;
        }

        // 对齐上游 style-class 演示数据:Name/Age/Address/Description,pageSize 3 时首屏 3 行。
        List<ExpandableRowDataType> items =
        [
            new ExpandableRowDataType
            {
                Key = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park",
                Description = "My name is John Brown, I am 32 years old, living in New York No. 1 Lake Park."
            },
            new ExpandableRowDataType
            {
                Key = "2", Name = "Jim Green", Age = 42, Address = "London No. 1 Lake Park",
                Description = "My name is Jim Green, I am 42 years old, living in London No. 1 Lake Park."
            },
            new ExpandableRowDataType
            {
                Key = "3", Name = "Joe Black", Age = 32, Address = "Sydney No. 1 Lake Park",
                Description = "My name is Joe Black, I am 32 years old, living in Sydney No. 1 Lake Park."
            },
            new ExpandableRowDataType
            {
                Key = "4", Name = "Disabled User", Age = 99, Address = "Sydney No. 2 Lake Park",
                Description = "This user is disabled."
            }
        ];
        viewModel.SemanticCaseDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Semantic);
    }

    public static void EnsureFilterAndSorterDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.FilterAndSorterDataSource is not null)
        {
            return;
        }

        List<DataGridBaseInfo> items =
        [
            new DataGridBaseInfo { Key = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "2", Name = "Jim Green", Age  = 42, Address = "London No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "3", Name = "Joe Black", Age  = 32, Address = "Sydney No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "4", Name = "Joe Red", Age    = 32, Address = "London No. 2 Lake Park" }
        ];
        viewModel.FilterAndSorterDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureMultiSorterDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.MultiSorterDataSource is not null)
        {
            return;
        }

        List<MultiSorterDataType> items =
        [
            new MultiSorterDataType { Key = "1", Name = "John Brown", Chinese = 98, Math = 60, English = 70 },
            new MultiSorterDataType { Key = "2", Name = "Jim Green", Chinese  = 98, Math = 66, English = 89 },
            new MultiSorterDataType { Key = "3", Name = "Joe Black", Chinese  = 98, Math = 90, English = 70 },
            new MultiSorterDataType { Key = "4", Name = "Jim Red", Chinese    = 88, Math = 99, English = 89 },
        ];
        viewModel.MultiSorterDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.MultiSorter);
    }

    public static void EnsureExpandableRowDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.ExpandableRowDataSource is not null)
        {
            return;
        }

        List<ExpandableRowDataType> items =
        [
            new ExpandableRowDataType
            {
                Key         = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park",
                Description = "My name is John Brown, I am 32 years old, living in New York No. 1 Lake Park."
            },
            new ExpandableRowDataType
            {
                Key         = "2", Name = "Jim Green", Age = 42, Address = "London No. 1 Lake Park",
                Description = "London No. 1 Lake Park"
            },
            new ExpandableRowDataType
            {
                Key         = "3", Name = "Joe Black", Age = 32, Address = "Sydney No. 1 Lake Park",
                Description = "My name is Joe Black, I am 32 years old, living in Sydney No. 1 Lake Park."
            },
            new ExpandableRowDataType
            {
                Key         = "5", Name = "Joe Red", Age = 78, Address = "London No. 2 Lake Park",
                Description = "My name is Joe Black, I am 78 years old, London No. 2 Lake Park"
            }
        ];
        viewModel.ExpandableRowDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Expandable);
    }

    public static void EnsureGroupHeaderDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.GroupHeaderDataSource is not null)
        {
            return;
        }

        var items = new List<GroupHeaderDataType>();
        for (var i = 0; i < 6; i++)
        {
            items.Add(new GroupHeaderDataType
            {
                Key            = i.ToString(),
                Name           = "John Brown",
                Age            = i + 1,
                Street         = "Lake Park",
                Building       = "C",
                Number         = 2035,
                CompanyAddress = "Lake Street 42",
                CompanyName    = "SoftLake Co",
                Gender         = "M"
            });
        }

        viewModel.GroupHeaderDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.GroupHeader);
    }

    public static void EnsureFixedHeaderDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.FixedHeaderDataSource is not null)
        {
            return;
        }

        var items = new List<DataGridBaseInfo>();
        for (var i = 0; i < 30; i++)
        {
            items.Add(new DataGridBaseInfo
            {
                Key     = i.ToString(),
                Name    = $"Edward King {i}",
                Age     = 32,
                Address = $"London No. 1 Lake Park {i}",
            });
        }

        viewModel.FixedHeaderDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureFixedColumnsDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.FixedColumnsDataSource is not null)
        {
            return;
        }

        List<DataGridBaseInfo> items =
        [
            new DataGridBaseInfo { Key = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "2", Name = "Jim Green", Age  = 42, Address = "London No. 1 Lake Park" },
        ];
        viewModel.FixedColumnsDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureFixedColumnsAndHeadersDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.FixedColumnsAndHeadersDataSource is not null)
        {
            return;
        }

        var items = new List<DataGridBaseInfo>();
        for (var i = 0; i < 30; i++)
        {
            items.Add(new DataGridBaseInfo
            {
                Key     = i.ToString(),
                Name    = $"Edward King {i}",
                Age     = 32,
                Address = $"London No. 1 Lake Park {i}",
            });
        }

        viewModel.FixedColumnsAndHeadersDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureDragColumnDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.DragColumnDataSource is not null)
        {
            return;
        }

        var items = new List<DragColumnDataType>
        {
            new()
            {
                Key     = "1",
                Name    = "John Brown",
                Gender  = "male",
                Age     = 32,
                Email   = "John Brown@example.com",
                Address = "London No. 1 Lake Park"
            },
            new()
            {
                Key     = "2",
                Name    = "Jim Green",
                Gender  = "female",
                Age     = 42,
                Email   = "jimGreen@example.com",
                Address = "London No. 1 Lake Park"
            },
            new()
            {
                Key     = "3",
                Name    = "Joe Black",
                Gender  = "female",
                Age     = 32,
                Email   = "JoeBlack@example.com",
                Address = "Sidney No. 1 Lake Park"
            },
            new()
            {
                Key     = "4",
                Name    = "George Hcc",
                Gender  = "male",
                Age     = 20,
                Email   = "george@example.com",
                Address = "Sidney No. 1 Lake Park"
            }
        };
        viewModel.DragColumnDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.DragColumn);
    }

    public static void EnsureDragRowDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.DragRowDataSource is null)
        {
            List<DataGridBaseInfo> items =
            [
                new DataGridBaseInfo { Key = "drag-1", Name = "John Brown", Age  = 32, Address = "London No. 1 Lake Park" },
                new DataGridBaseInfo { Key = "drag-2", Name = "Jim Green", Age   = 42, Address = "London No. 1 Lake Park" },
                new DataGridBaseInfo { Key = "drag-3", Name = "Joe Black", Age   = 32, Address = "Sidney No. 1 Lake Park" },
                new DataGridBaseInfo { Key = "drag-4", Name = "George Hcc", Age  = 20, Address = "Sidney No. 1 Lake Park" }
            ];
            viewModel.DragRowDataSource = new(
                items,
                DataGridShowCaseSourceDescriptors.Base);
        }

        if (viewModel.DragRowManyDataSource is not null)
        {
            return;
        }

        var manyItems = new List<DataGridBaseInfo>();
        for (var i = 0; i < 30; i++)
        {
            manyItems.Add(new DataGridBaseInfo
            {
                Key     = $"drag-many-{i}",
                Name    = "John Brown",
                Age     = 32,
                Address = $"London No. {i + 1} Lake Park"
            });
        }
        viewModel.DragRowManyDataSource = new(
            manyItems,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureCustomEmptyDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.CustomEmptyDataSource is not null)
        {
            return;
        }

        List<DataGridBaseInfo> items =
        [
            new DataGridBaseInfo { Key = "empty-1", Name = "John Brown", Age = 32, Address = "London No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "empty-2", Name = "Jim Green", Age  = 42, Address = "London No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "empty-3", Name = "Joe Black", Age  = 32, Address = "Sidney No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "empty-4", Name = "George Hcc", Age = 18, Address = "Sidney No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "empty-5", Name = "Joe Black", Age  = 32, Address = "Sidney No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "empty-6", Name = "George Hcc", Age = 44, Address = "Sidney No. 2 Lake Park" }
        ];
        viewModel.CustomEmptyDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureEditableCellsDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.EditableCellsDataSource is not null)
        {
            return;
        }

        List<DataGridBaseInfo> items =
        [
            new DataGridBaseInfo { Key = "editable-1", Name = "John Brown", Age = 32, Address = "London No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "editable-2", Name = "Jim Green", Age  = 42, Address = "London No. 3 Lake Park" }
        ];
        viewModel.EditableCellsDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureEditableRowsDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.EditableRowsDataSource is not null)
        {
            return;
        }

        var items = new List<DataGridBaseInfo>();
        for (var i = 0; i < 30; i++)
        {
            items.Add(new DataGridBaseInfo
            {
                Key     = $"editable-row-{i + 1}",
                Name    = $"Edward {i + 1}",
                Age     = 32,
                Address = $"London Park no. {i + 1}"
            });
        }
        viewModel.EditableRowsDataSource = new(
            items,
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsurePagingGridDataSource(DataGridViewModel viewModel)
    {
        if (viewModel.PagingGridDataSource is not null)
        {
            return;
        }

        viewModel.PagingGridDataSource = new(
            RandomDataGenerator.GenerateRandomData(100),
            DataGridShowCaseSourceDescriptors.Base);
    }

    public static void EnsureRemoteDataSource(DataGridViewModel viewModel)
    {
        viewModel.RemoteDataSource ??= new GalleryRemoteDataGridSource(
            totalDataCount: 1_000_000,
            latency: TimeSpan.FromMilliseconds(180));
    }
}

internal static class RandomDataGenerator
{
    private static readonly Random Random = new();

    private static readonly string[] FirstNames = ["James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth"];
    private static readonly string[] LastNames = ["Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez"];
    private static readonly string[] Cities = ["New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose"];
    private static readonly string[] Streets = ["Main St", "Park Ave", "Elm St", "Oak St", "Pine Rd", "Maple Ave", "Cedar Ln", "Washington Blvd", "Lake Shore Dr", "Sunset Blvd"];
    private static readonly string[] TagNames = ["VIP", "NEW", "STAFF", "MANAGER", "DEVELOPER", "DESIGNER", "ANALYST", "LEADER", "EXPERT", "TRAINEE", "VIP", "SPECIAL"];
    private static readonly string[] Colors = ["red", "volcano", "orange", "gold", "yellow", "lime", "green", "cyan", "blue", "geekblue", "purple"];

    public static List<DataGridBaseInfo> GenerateRandomData(int count = 10)
    {
        return Enumerable.Range(1, count).Select(i => new DataGridBaseInfo
        {
            Key     = i.ToString(),
            Name    = $"{FirstNames[Random.Next(FirstNames.Length)]} {LastNames[Random.Next(LastNames.Length)]}",
            Age     = Random.Next(18, 66),
            Address = $"{Random.Next(1, 10000)} {Streets[Random.Next(Streets.Length)]}, {Cities[Random.Next(Cities.Length)]}",
            Money   = $"￥{Random.Next(1000, 10000000):N2}",
            Tags    = GenerateRandomTags(Random.Next(1, 4))
        }).ToList();
    }

    private static List<TagInfo> GenerateRandomTags(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new TagInfo
        {
            Name  = TagNames[Random.Next(TagNames.Length)],
            Color = Colors[Random.Next(Colors.Length)]
        }).ToList();
    }
}
