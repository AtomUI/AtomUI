using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.DataGrid;

public partial class DataGridViewModel : ReactiveObject, IRoutableViewModel, IDisposable
{
    public static EntityKey ID = "DataGrid";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    internal GalleryLocalDataGridSource<DataGridBaseInfo>? BasicCaseDataSource { get; set; }
    internal GalleryLocalDataGridSource<ExpandableRowDataType>? SemanticCaseDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? FilterAndSorterDataSource { get; set; }
    internal GalleryLocalDataGridSource<MultiSorterDataType>? MultiSorterDataSource { get; set; }
    internal GalleryLocalDataGridSource<ExpandableRowDataType>? ExpandableRowDataSource { get; set; }
    internal GalleryLocalDataGridSource<GroupHeaderDataType>? GroupHeaderDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? FixedHeaderDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? FixedColumnsDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? FixedColumnsAndHeadersDataSource { get; set; }
    internal GalleryLocalDataGridSource<DragColumnDataType>? DragColumnDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? DragRowDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? DragRowManyDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? CustomEmptyDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? EditableCellsDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? EditableRowsDataSource { get; set; }
    internal GalleryLocalDataGridSource<DataGridBaseInfo>? PagingGridDataSource { get; set; }
    internal GalleryRemoteDataGridSource? RemoteDataSource { get; set; }

    public ObservableCollection<DataGridFilterItem> NameFilters { get; }
    public ObservableCollection<DataGridFilterItem> AddressFilters { get; }

    private IList? _filterAndSorterSelectedNames = new ObservableCollection<object>();
    private IList? _filterAndSorterSelectedAddresses = new ObservableCollection<object>();
    private IList? _treeFilterSelectedNames = new ObservableCollection<object>();
    private IList? _treeFilterSelectedAddresses = new ObservableCollection<object>();
    private IList? _resetSelectedNames = new ObservableCollection<object>();
    private IList? _resetSelectedAddresses = new ObservableCollection<object>();

    public IList? FilterAndSorterSelectedNames
    {
        get => _filterAndSorterSelectedNames;
        set => this.RaiseAndSetIfChanged(ref _filterAndSorterSelectedNames, value);
    }

    public IList? FilterAndSorterSelectedAddresses
    {
        get => _filterAndSorterSelectedAddresses;
        set => this.RaiseAndSetIfChanged(ref _filterAndSorterSelectedAddresses, value);
    }

    public IList? TreeFilterSelectedNames
    {
        get => _treeFilterSelectedNames;
        set => this.RaiseAndSetIfChanged(ref _treeFilterSelectedNames, value);
    }

    public IList? TreeFilterSelectedAddresses
    {
        get => _treeFilterSelectedAddresses;
        set => this.RaiseAndSetIfChanged(ref _treeFilterSelectedAddresses, value);
    }

    public IList? ResetSelectedNames
    {
        get => _resetSelectedNames;
        set => this.RaiseAndSetIfChanged(ref _resetSelectedNames, value);
    }

    public IList? ResetSelectedAddresses
    {
        get => _resetSelectedAddresses;
        set => this.RaiseAndSetIfChanged(ref _resetSelectedAddresses, value);
    }

    public DataGridViewModel(IScreen screen)
    {
        HostScreen     = screen;
        NameFilters    = CreateNameFilters();
        AddressFilters = CreateAddressFilters();
    }

    public void Dispose() => DataGridShowCaseDataSources.ClearAll(this);

    private static ObservableCollection<DataGridFilterItem> CreateNameFilters()
    {
        return
        [
            new DataGridFilterItem { Text = Lang(DataGridShowCaseLangResourceKind.P2TextJoe), Value = "Joe" },
            new DataGridFilterItem { Text = Lang(DataGridShowCaseLangResourceKind.P2TextJim), Value = "Jim" },
            new DataGridFilterItem
            {
                Text  = Lang(DataGridShowCaseLangResourceKind.P2TextSubmenu),
                Value = "Submenu",
                Children =
                [
                    new DataGridFilterItem { Text = Lang(DataGridShowCaseLangResourceKind.P2TextGreen), Value = "Green" },
                    new DataGridFilterItem { Text = Lang(DataGridShowCaseLangResourceKind.P2TextBlack), Value = "Black" }
                ]
            }
        ];
    }

    private static ObservableCollection<DataGridFilterItem> CreateAddressFilters()
    {
        return
        [
            new DataGridFilterItem { Text = Lang(DataGridShowCaseLangResourceKind.P2TextLondon), Value = "London" },
            new DataGridFilterItem { Text = Lang(DataGridShowCaseLangResourceKind.P2TextNewYork), Value = "New York" }
        ];
    }

    private static string Lang(DataGridShowCaseLangResourceKind kind)
    {
        return GalleryLocalization.Get(kind, kind.ToString());
    }
}

[GenerateDataMemberAccessors]
public partial class DataGridBaseInfo
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Money { get; set; } = string.Empty;
    public List<TagInfo> Tags { get; set; } = new();
}

public class TagInfo
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

[GenerateDataMemberAccessors]
public partial class MultiSorterDataType
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Chinese { get; set; }
    public int Math { get; set; }
    public int English { get; set; }
}

public class ExpandableRowDataType : DataGridBaseInfo
{
    public string Description { get; set; } = string.Empty;
}

public class GroupHeaderDataType
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int Number { get; set; }
}

public class DragColumnDataType
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public static class DataGridShowCaseFields
{
    public static DataGridFieldId Key { get; } = new("key");
    public static DataGridFieldId Name { get; } = new("name");
    public static DataGridFieldId Age { get; } = new("age");
    public static DataGridFieldId Address { get; } = new("address");
    public static DataGridFieldId Description { get; } = new("description");
    public static DataGridFieldId Money { get; } = new("money");
    public static DataGridFieldId Chinese { get; } = new("chinese");
    public static DataGridFieldId Math { get; } = new("math");
    public static DataGridFieldId English { get; } = new("english");
    public static DataGridFieldId Street { get; } = new("street");
    public static DataGridFieldId Building { get; } = new("building");
    public static DataGridFieldId Number { get; } = new("number");
    public static DataGridFieldId CompanyAddress { get; } = new("company-address");
    public static DataGridFieldId CompanyName { get; } = new("company-name");
    public static DataGridFieldId Gender { get; } = new("gender");
    public static DataGridFieldId Email { get; } = new("email");
}

internal sealed class GalleryLocalDataGridSource<T> : IDisposable
{
    public GalleryLocalDataGridSource(
        IEnumerable<T> rows,
        DataGridLocalSourceDescriptor<T> descriptor)
    {
        Rows = new ObservableCollection<T>(rows);
        Source = DataGridLocalSource.Create(Rows, descriptor);
    }

    public ObservableCollection<T> Rows { get; }

    public DataGridLocalSource<T> Source { get; }

    public void Dispose() => Source.Dispose();
}

internal sealed class GalleryRemoteDataGridSource : IDataGridSource, IDisposable
{
    private readonly object _gate = new();
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly int _totalDataCount;
    private EventHandler? _invalidated;
    private int _snapshotVersion;
    private int _generatedRowCount;
    private int _maximumGeneratedRangeCount;
    private int _cancellationCount;
    private bool _failNextRequest;
    private bool _isDisposed;

    public GalleryRemoteDataGridSource(int totalDataCount, TimeSpan latency)
    {
        if (totalDataCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalDataCount));
        }
        if (latency < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(latency));
        }

        _totalDataCount = totalDataCount;
        Latency = latency;
        Schema = new DataGridSourceSchema(
            typeof(DataGridBaseInfo),
            [
                new DataGridFieldSchema(
                    DataGridShowCaseFields.Name,
                    typeof(string),
                    DataGridSortDirections.None,
                    [],
                    canGroup: false),
                new DataGridFieldSchema(
                    DataGridShowCaseFields.Age,
                    typeof(int),
                    DataGridSortDirections.None,
                    [],
                    canGroup: false),
                new DataGridFieldSchema(
                    DataGridShowCaseFields.Address,
                    typeof(string),
                    DataGridSortDirections.None,
                    [],
                    canGroup: false)
            ],
            preferredRangeSize: 64,
            maximumRangeSize: 256);
    }

    public DataGridSourceSchema Schema { get; }

    public TimeSpan Latency { get; set; }

    public int GeneratedRowCount => Volatile.Read(ref _generatedRowCount);

    public int MaximumGeneratedRangeCount => Volatile.Read(ref _maximumGeneratedRangeCount);

    public int CancellationCount => Volatile.Read(ref _cancellationCount);

    public bool IsDisposed
    {
        get
        {
            lock (_gate)
            {
                return _isDisposed;
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

    public async ValueTask<DataGridRangeResult> FetchAsync(
        DataGridFetchRequest request,
        CancellationToken cancellationToken)
    {
        CancellationToken disposeToken;
        lock (_gate)
        {
            ThrowIfDisposed();
            disposeToken = _disposeCancellation.Token;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            disposeToken);
        try
        {
            if (Latency > TimeSpan.Zero)
            {
                await Task.Delay(Latency, linkedCancellation.Token).ConfigureAwait(false);
            }
            linkedCancellation.Token.ThrowIfCancellationRequested();

            int snapshotVersion;
            lock (_gate)
            {
                ThrowIfDisposed();
                if (_failNextRequest)
                {
                    _failNextRequest = false;
                    throw new InvalidOperationException("The deterministic Gallery remote request failed.");
                }
                snapshotVersion = _snapshotVersion;
            }

            var snapshot = new DataGridSnapshotId($"gallery-remote-{snapshotVersion}");
            if (request.ExpectedSnapshot is { } expectedSnapshot && expectedSnapshot != snapshot)
            {
                throw new DataGridSnapshotExpiredException(
                    $"Snapshot '{expectedSnapshot}' expired; current snapshot is '{snapshot}'.");
            }

            var pageStart = request.PageRequest?.DataStartIndex ?? 0;
            var availableDataCount = Math.Max(0, _totalDataCount - pageStart);
            var windowDataCount = (int)Math.Min(
                request.PageRequest?.DataCount ?? _totalDataCount,
                availableDataCount);
            var start = Math.Min(request.Range.StartIndex, windowDataCount);
            var count = Math.Min(request.Range.Count, windowDataCount - start);
            var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
            for (var offset = 0; offset < count; offset++)
            {
                linkedCancellation.Token.ThrowIfCancellationRequested();
                var windowIndex = start + offset;
                var dataIndex = checked(pageStart + windowIndex);
                entries.Add(DataGridSourceEntry.CreateData(
                    DataGridRowKey.FromInt64(dataIndex + 1),
                    CreateRow(dataIndex),
                    windowIndex,
                    dataIndex));
            }

            Interlocked.Add(ref _generatedRowCount, count);
            RecordMaximumGeneratedRange(count);
            return new DataGridRangeResult(
                start,
                entries.MoveToImmutable(),
                windowDataCount,
                windowDataCount,
                _totalDataCount,
                snapshot);
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref _cancellationCount);
            throw;
        }
    }

    public void Reload()
    {
        EventHandler? invalidated;
        lock (_gate)
        {
            ThrowIfDisposed();
            _snapshotVersion = checked(_snapshotVersion + 1);
            invalidated = _invalidated;
        }
        invalidated?.Invoke(this, EventArgs.Empty);
    }

    public void FailNextRequest()
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            _failNextRequest = true;
        }
    }

    public void ReloadWithFailure()
    {
        EventHandler? invalidated;
        lock (_gate)
        {
            ThrowIfDisposed();
            _failNextRequest = true;
            invalidated = _invalidated;
        }
        invalidated?.Invoke(this, EventArgs.Empty);
    }

    public void ExpireSnapshot()
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            _snapshotVersion = checked(_snapshotVersion + 1);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _invalidated = null;
        }
        _disposeCancellation.Cancel();
        _disposeCancellation.Dispose();
    }

    private static DataGridBaseInfo CreateRow(long dataIndex)
    {
        var ordinal = dataIndex + 1;
        return new DataGridBaseInfo
        {
            Key = $"remote-{ordinal}",
            Name = $"Remote row {ordinal:N0}",
            Age = 18 + (int)(dataIndex % 48),
            Address = $"Shard {dataIndex % 32:D2} · record {ordinal:N0}"
        };
    }

    private void RecordMaximumGeneratedRange(int count)
    {
        var observed = Volatile.Read(ref _maximumGeneratedRangeCount);
        while (count > observed)
        {
            var previous = Interlocked.CompareExchange(
                ref _maximumGeneratedRangeCount,
                count,
                observed);
            if (previous == observed)
            {
                return;
            }
            observed = previous;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(GalleryRemoteDataGridSource));
        }
    }
}

internal static class DataGridShowCaseSourceDescriptors
{
    private static readonly ImmutableArray<DataGridLocalFilter<string>> ContainsAny =
    [
        new(
            new DataGridOperatorId("contains-any"),
            1,
            64,
            DataGridScalarKinds.String,
            static (value, values) => values.Any(candidate =>
                value.Contains(candidate.GetString(), StringComparison.OrdinalIgnoreCase)))
    ];

    public static DataGridLocalSourceDescriptor<DataGridBaseInfo> Base { get; } =
        DataGridLocalSourceDescriptor.For<DataGridBaseInfo>(static row => DataGridRowKey.FromString(row.Key))
            .Field(DataGridShowCaseFields.Key, static row => row.Key, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Name, static row => row.Name, StringComparer.Ordinal, filters: ContainsAny)
            .Field(DataGridShowCaseFields.Age, static row => row.Age)
            .Field(DataGridShowCaseFields.Address, static row => row.Address, StringComparer.Ordinal, filters: ContainsAny)
            .Field(DataGridShowCaseFields.Money, static row => row.Money, StringComparer.Ordinal);

    public static DataGridLocalSourceDescriptor<ExpandableRowDataType> Expandable { get; } =
        DataGridLocalSourceDescriptor.For<ExpandableRowDataType>(static row => DataGridRowKey.FromString(row.Key))
            .Field(DataGridShowCaseFields.Key, static row => row.Key, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Name, static row => row.Name, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Age, static row => row.Age)
            .Field(DataGridShowCaseFields.Address, static row => row.Address, StringComparer.Ordinal);

    public static DataGridLocalSourceDescriptor<ExpandableRowDataType> Semantic { get; } =
        DataGridLocalSourceDescriptor.For<ExpandableRowDataType>(static row => DataGridRowKey.FromString(row.Key))
            .Field(DataGridShowCaseFields.Key, static row => row.Key, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Name, static row => row.Name, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Age, static row => row.Age)
            .Field(DataGridShowCaseFields.Address, static row => row.Address, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Description, static row => row.Description, StringComparer.Ordinal);

    public static DataGridLocalSourceDescriptor<MultiSorterDataType> MultiSorter { get; } =
        DataGridLocalSourceDescriptor.For<MultiSorterDataType>(static row => DataGridRowKey.FromString(row.Key))
            .Field(DataGridShowCaseFields.Name, static row => row.Name, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Chinese, static row => row.Chinese)
            .Field(DataGridShowCaseFields.Math, static row => row.Math)
            .Field(DataGridShowCaseFields.English, static row => row.English);

    public static DataGridLocalSourceDescriptor<GroupHeaderDataType> GroupHeader { get; } =
        DataGridLocalSourceDescriptor.For<GroupHeaderDataType>(static row => DataGridRowKey.FromString(row.Key))
            .Field(DataGridShowCaseFields.Name, static row => row.Name, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Age, static row => row.Age)
            .Field(DataGridShowCaseFields.Street, static row => row.Street, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Building, static row => row.Building, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Number, static row => row.Number)
            .Field(DataGridShowCaseFields.CompanyAddress, static row => row.CompanyAddress, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.CompanyName, static row => row.CompanyName, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Gender, static row => row.Gender, StringComparer.Ordinal);

    public static DataGridLocalSourceDescriptor<DragColumnDataType> DragColumn { get; } =
        DataGridLocalSourceDescriptor.For<DragColumnDataType>(static row => DataGridRowKey.FromString(row.Key))
            .Field(DataGridShowCaseFields.Name, static row => row.Name, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Age, static row => row.Age)
            .Field(DataGridShowCaseFields.Address, static row => row.Address, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Gender, static row => row.Gender, StringComparer.Ordinal)
            .Field(DataGridShowCaseFields.Email, static row => row.Email, StringComparer.Ordinal);
}
