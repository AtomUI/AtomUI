using System.Collections.Immutable;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateDataGridScenarios()
    {
        return
        [
            new PerfScenario("DataGrid.Basic", _ => CreateBasicDataGrid()),
            new PerfScenario("DataGrid.Filter.Menu.Closed", _ => CreateFilterDataGrid(hierarchical: false)),
            new PerfScenario("DataGrid.Filter.Tree.Closed", _ => CreateFilterDataGrid(hierarchical: true)),
            new PerfScenario("DataGrid.RowHeaders", _ => CreateRowHeadersDataGrid()),
            new PerfScenario("DataGrid.RowDetails.Collapsed", _ => CreateRowDetailsDataGrid()),
            new PerfScenario("DataGrid.GroupHeaders", _ => CreateColumnGroupDataGrid()),
            new PerfScenario("DataGrid.RowGroups", _ => CreateRowGroupDataGrid()),
            new PerfScenario("DataGrid.RemoteMillion", _ => CreateRemoteMillionDataGrid()),
            new PerfScenario("DataGrid.GalleryShape", _ => CreateDataGridGalleryShape())
        ];
    }

    private static DataGrid CreateBasicDataGrid(int rowCount = 8, int columnCount = 4)
    {
        var grid = CreateDataGridShell(rowCount);
        for (var i = 0; i < columnCount; i++)
        {
            var field = GetDataGridField(i);
            grid.Columns.Add(new DataGridTextColumn
            {
                Header  = $"Column {i + 1}",
                FieldId = field.Id,
                Binding = new Binding(field.Path)
            });
        }
        return grid;
    }

    private static DataGrid CreateFilterDataGrid(bool hierarchical)
    {
        var grid = CreateDataGridShell(8);
        grid.CanUserFilterColumns = true;
        var nameFilters = new List<DataGridFilterItem>
        {
            new() { Text = "Joe", Value = "Joe" },
            new() { Text = "Jim", Value = "Jim" }
        };
        if (hierarchical)
        {
            nameFilters.Add(new DataGridFilterItem
            {
                Text = "Nested",
                Value = "Nested",
                Children =
                {
                    new DataGridFilterItem { Text = "Green", Value = "Green" },
                    new DataGridFilterItem { Text = "Black", Value = "Black" }
                }
            });
        }
        grid.Columns.Add(CreateFilterColumn(
            "Name",
            NameField,
            nameof(PerfDataGridRow.Name),
            nameFilters));
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Age",
            FieldId = AgeField,
            Binding = new Binding(nameof(PerfDataGridRow.Age))
        });
        grid.Columns.Add(CreateFilterColumn("Address", AddressField, nameof(PerfDataGridRow.Address),
        [
            new DataGridFilterItem { Text = "London", Value = "London" },
            new DataGridFilterItem { Text = "New York", Value = "New York" }
        ]));
        return grid;
    }

    private static DataGrid CreateRowHeadersDataGrid()
    {
        var grid = CreateBasicDataGrid();
        grid.HeadersVisibility = DataGridHeadersVisibility.All;
        return grid;
    }

    private static Control CreateDataGridGalleryShape()
    {
        var panel = new StackPanel
        {
            Spacing = 8
        };

        panel.Children.Add(CreateBasicDataGrid(rowCount: 8, columnCount: 4));
        panel.Children.Add(CreateFilterDataGrid(hierarchical: false));
        panel.Children.Add(CreateFilterDataGrid(hierarchical: true));
        panel.Children.Add(CreateBasicDataGrid(rowCount: 20, columnCount: 8));

        return panel;
    }

    private static DataGrid CreateRowDetailsDataGrid()
    {
        var grid = CreateDataGridShell(8);
        grid.Columns.Add(new DataGridDetailExpanderColumn());
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Name",
            FieldId = NameField,
            Binding = new Binding(nameof(PerfDataGridRow.Name))
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Age",
            FieldId = AgeField,
            Binding = new Binding(nameof(PerfDataGridRow.Age))
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Address",
            FieldId = AddressField,
            Binding = new Binding(nameof(PerfDataGridRow.Address))
        });
        grid.RowDetailsTemplate = new FuncDataTemplate<PerfDataGridRow>((row, _) =>
            new Avalonia.Controls.TextBlock
            {
                Text         = row?.Address,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });
        return grid;
    }

    private static DataGrid CreateColumnGroupDataGrid()
    {
        var grid = CreateDataGridShell(8);
        grid.CanUserResizeColumns = true;
        grid.LeftFrozenColumnCount = 1;
        grid.RightFrozenColumnCount = 1;

        var addressGroup = new DataGridColumnGroupItem { Header = "Address" };
        addressGroup.GroupChildren.Add(new DataGridTextColumn
        {
            Header  = "Address",
            FieldId = AddressField,
            Binding = new Binding(nameof(PerfDataGridRow.Address))
        });
        addressGroup.GroupChildren.Add(new DataGridTextColumn
        {
            Header  = "Score",
            FieldId = ScoreField,
            Binding = new Binding(nameof(PerfDataGridRow.Score))
        });

        var profileGroup = new DataGridColumnGroupItem { Header = "Profile" };
        profileGroup.GroupChildren.Add(new DataGridTextColumn
        {
            Header  = "Name",
            FieldId = NameField,
            Binding = new Binding(nameof(PerfDataGridRow.Name))
        });
        profileGroup.GroupChildren.Add(new DataGridTextColumn
        {
            Header  = "Age",
            FieldId = AgeField,
            Binding = new Binding(nameof(PerfDataGridRow.Age))
        });
        profileGroup.GroupChildren.Add(addressGroup);

        grid.ColumnGroups.Add(profileGroup);
        grid.ColumnGroups.Add(new DataGridTextColumn
        {
            Header  = "Score",
            FieldId = ScoreField,
            Binding = new Binding(nameof(PerfDataGridRow.Score))
        });
        return grid;
    }

    private static DataGrid CreateRowGroupDataGrid()
    {
        var grid = CreateDataGridShell(
            CreateDataGridRows(12),
            DataGridQuery.Empty.WithGroups(
                [new DataGridGroup(NameField, DataGridSortDirection.Ascending)]));
        grid.SelectionMode = DataGridSelectionMode.Single;
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Name",
            FieldId = NameField,
            Binding = new Binding(nameof(PerfDataGridRow.Name))
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Age",
            FieldId = AgeField,
            Binding = new Binding(nameof(PerfDataGridRow.Age))
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Address",
            FieldId = AddressField,
            Binding = new Binding(nameof(PerfDataGridRow.Address))
        });
        return grid;
    }

    private static DataGrid CreateDataGridShell(int rowCount)
    {
        return CreateDataGridShell(CreateDataGridRows(rowCount));
    }

    private static DataGrid CreateRemoteMillionDataGrid()
    {
        var source = new PerfRemoteDataGridSource(1_000_000);
        var grid = new DataGrid
        {
            Width  = 720,
            Height = 220,
            ItemsSource = source
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Name",
            FieldId = NameField,
            Binding = new Binding(nameof(PerfDataGridRow.Name))
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Age",
            FieldId = AgeField,
            Binding = new Binding(nameof(PerfDataGridRow.Age))
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Address",
            FieldId = AddressField,
            Binding = new Binding(nameof(PerfDataGridRow.Address))
        });
        grid.DetachedFromVisualTree += ReleaseOwnedSource;
        return grid;

        void ReleaseOwnedSource(object? sender, VisualTreeAttachmentEventArgs args)
        {
            grid.DetachedFromVisualTree -= ReleaseOwnedSource;
            source.Dispose();
        }
    }

    private static DataGrid CreateDataGridShell(
        IReadOnlyList<PerfDataGridRow> rows,
        DataGridQuery? query = null)
    {
        var source = DataGridLocalSource.Create(rows, PerfDataGridRowDescriptor);
        var grid = new DataGrid
        {
            Width       = 720,
            Height      = 220,
            Query       = query ?? DataGridQuery.Empty,
            ItemsSource      = source
        };
        grid.DetachedFromVisualTree += ReleaseOwnedSource;
        return grid;

        void ReleaseOwnedSource(object? sender, VisualTreeAttachmentEventArgs args)
        {
            grid.DetachedFromVisualTree -= ReleaseOwnedSource;
            source.Dispose();
        }
    }

    private static DataGridTextColumn CreateFilterColumn(
        string header,
        DataGridFieldId fieldId,
        string bindingPath,
        IEnumerable<DataGridFilterItem> filters)
    {
        var column = new DataGridTextColumn
        {
            Header           = header,
            FieldId          = fieldId,
            Binding          = new Binding(bindingPath),
            Filters          = filters
        };
        return column;
    }

    private static IReadOnlyList<PerfDataGridRow> CreateDataGridRows(int count)
    {
        var rows = new PerfDataGridRow[count];
        for (var i = 0; i < count; i++)
        {
            rows[i] = new PerfDataGridRow
            {
                Id = i,
                Name = i % 2 == 0 ? "Joe" : "Jim",
                Age = 20 + i % 30,
                Address = i % 3 == 0 ? "London" : "New York",
                Score = 60 + i % 40
            };
        }
        return rows;
    }

    private static (DataGridFieldId Id, string Path) GetDataGridField(int index)
    {
        return (index % 4) switch
        {
            0 => (NameField, nameof(PerfDataGridRow.Name)),
            1 => (AgeField, nameof(PerfDataGridRow.Age)),
            2 => (AddressField, nameof(PerfDataGridRow.Address)),
            _ => (ScoreField, nameof(PerfDataGridRow.Score))
        };
    }

    private static readonly DataGridFieldId NameField = new("name");
    private static readonly DataGridFieldId AgeField = new("age");
    private static readonly DataGridFieldId AddressField = new("address");
    private static readonly DataGridFieldId ScoreField = new("score");

    private static readonly ImmutableArray<DataGridLocalFilter<string>> TextFilters =
    [
        new(
            new DataGridOperatorId("contains-any"),
            1,
            16,
            DataGridScalarKinds.String,
            static (value, values) => values.Any(candidate =>
                value.Contains(candidate.GetString(), StringComparison.OrdinalIgnoreCase)))
    ];

    private static readonly DataGridLocalSourceDescriptor<PerfDataGridRow> PerfDataGridRowDescriptor =
        DataGridLocalSourceDescriptor.For<PerfDataGridRow>(static row => DataGridRowKey.FromInt64(row.Id))
            .Field(NameField, static row => row.Name ?? string.Empty, StringComparer.Ordinal, filters: TextFilters, canGroup: true)
            .Field(AgeField, static row => row.Age)
            .Field(AddressField, static row => row.Address ?? string.Empty, StringComparer.Ordinal, filters: TextFilters)
            .Field(ScoreField, static row => row.Score);

    private sealed class PerfDataGridRow
    {
        public long Id { get; init; }
        public string? Name { get; init; }
        public int Age { get; init; }
        public string? Address { get; init; }
        public int Score { get; init; }
    }

    private sealed class PerfRemoteDataGridSource : IDataGridSource, IDisposable
    {
        private static readonly DataGridSnapshotId Snapshot = new("perf-remote-v1");
        private readonly object _gate = new();
        private readonly long _totalDataCount;
        private EventHandler? _invalidated;
        private int _requestCount;
        private int _generatedRowCount;
        private int _maximumReturnedRangeCount;
        private int _activeRequestCount;
        private int _peakConcurrentRequestCount;
        private bool _isDisposed;

        public PerfRemoteDataGridSource(long totalDataCount)
        {
            _totalDataCount = totalDataCount;
            Schema = new DataGridSourceSchema(
                typeof(PerfDataGridRow),
                [
                    new DataGridFieldSchema(
                        NameField,
                        typeof(string),
                        DataGridSortDirections.None,
                        [],
                        canGroup: false),
                    new DataGridFieldSchema(
                        AgeField,
                        typeof(int),
                        DataGridSortDirections.None,
                        [],
                        canGroup: false),
                    new DataGridFieldSchema(
                        AddressField,
                        typeof(string),
                        DataGridSortDirections.None,
                        [],
                        canGroup: false)
                ],
                preferredRangeSize: 64,
                maximumRangeSize: 64);
        }

        public DataGridSourceSchema Schema { get; }

        public int RequestCount => Volatile.Read(ref _requestCount);

        public int GeneratedRowCount => Volatile.Read(ref _generatedRowCount);

        public int MaximumReturnedRangeCount => Volatile.Read(ref _maximumReturnedRangeCount);

        public int ActiveRequestCount => Volatile.Read(ref _activeRequestCount);

        public int PeakConcurrentRequestCount => Volatile.Read(ref _peakConcurrentRequestCount);

        public int InvalidatedSubscriberCount
        {
            get
            {
                lock (_gate)
                {
                    return _invalidated?.GetInvocationList().Length ?? 0;
                }
            }
        }

        public event EventHandler? Invalidated
        {
            add
            {
                lock (_gate)
                {
                    ThrowIfDisposed();
                    _invalidated += value;
                }
            }
            remove
            {
                lock (_gate)
                {
                    _invalidated -= value;
                }
            }
        }

        public ValueTask<DataGridRangeResult> FetchAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                ThrowIfDisposed();
            }

            Interlocked.Increment(ref _requestCount);
            var active = Interlocked.Increment(ref _activeRequestCount);
            RecordMaximum(ref _peakConcurrentRequestCount, active);
            try
            {
                if (request.ExpectedSnapshot is { } expectedSnapshot && expectedSnapshot != Snapshot)
                {
                    throw new DataGridSnapshotExpiredException(
                        $"Snapshot '{expectedSnapshot}' is not current.");
                }

                var pageStart = request.PageRequest?.DataStartIndex ?? 0;
                var availableDataCount = Math.Max(0L, _totalDataCount - pageStart);
                var requestedWindowCount = request.PageRequest?.DataCount ??
                                           (int)Math.Min(int.MaxValue, _totalDataCount);
                var windowDataCount = (int)Math.Min(requestedWindowCount, availableDataCount);
                var start = Math.Min(request.Range.StartIndex, windowDataCount);
                var count = Math.Min(request.Range.Count, windowDataCount - start);
                var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
                for (var offset = 0; offset < count; offset++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var windowIndex = start + offset;
                    var dataIndex = checked(pageStart + windowIndex);
                    entries.Add(DataGridSourceEntry.CreateData(
                        DataGridRowKey.FromInt64(dataIndex + 1),
                        new PerfDataGridRow
                        {
                            Id = dataIndex,
                            Name = $"Remote {dataIndex + 1}",
                            Age = 18 + (int)(dataIndex % 48),
                            Address = $"Shard {dataIndex % 32:D2}"
                        },
                        windowIndex,
                        dataIndex));
                }

                Interlocked.Add(ref _generatedRowCount, count);
                RecordMaximum(ref _maximumReturnedRangeCount, count);
                return new ValueTask<DataGridRangeResult>(new DataGridRangeResult(
                    start,
                    entries.MoveToImmutable(),
                    windowDataCount,
                    windowDataCount,
                    _totalDataCount,
                    Snapshot));
            }
            finally
            {
                Interlocked.Decrement(ref _activeRequestCount);
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                _isDisposed = true;
                _invalidated = null;
            }
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
        }

        private static void RecordMaximum(ref int target, int candidate)
        {
            var observed = Volatile.Read(ref target);
            while (candidate > observed)
            {
                var previous = Interlocked.CompareExchange(ref target, candidate, observed);
                if (previous == observed)
                {
                    return;
                }
                observed = previous;
            }
        }
    }
}
