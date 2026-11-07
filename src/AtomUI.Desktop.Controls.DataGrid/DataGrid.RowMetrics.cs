using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    #region 运行时字段

    private static readonly object[] RowMetricResourceKeys =
    [
        DataGridTokenKind.CellPadding, DataGridTokenKind.CellPaddingMD, DataGridTokenKind.CellPaddingSM,
        DataGridTokenKind.CellFontSize, DataGridTokenKind.CellFontSizeMD, DataGridTokenKind.CellFontSizeSM,
        new ExplicitTokenResourceKey(DataGridTokenKind.SelectionColumnWidth),
        new ExplicitTokenResourceKey(DataGridTokenKind.ExpandIconSize),
        new ExplicitTokenResourceKey(DataGridTokenKind.ExpandIconMargin),
        DataGridTokenKind.RowReorderIndicatorSize,
        ControlTokenResourceKey.Global(typeof(DataGrid), SharedTokenKind.BorderThickness),
        ControlTokenResourceKey.Global(typeof(DataGrid), SharedTokenKind.SizeMS),
        ControlTokenResourceKey.Global(typeof(DataGrid), SharedTokenKind.IconSize),
        ControlTokenResourceKey.Global(typeof(DataGrid), SharedTokenKind.FontFamily),
        ControlTokenResourceKey.Global(typeof(DataGridCell), SharedTokenKind.FontFamily),
        ControlTokenResourceKey.Global(typeof(DataGridRow), SharedTokenKind.FontFamily),
        ControlTokenResourceKey.Global(typeof(DataGridRowHeader), SharedTokenKind.FontFamily),
        ControlTokenResourceKey.Global(typeof(DataGridRowGroupHeader), SharedTokenKind.FontFamily)
    ];

    private readonly ThemeTokenResolver _rowMetricResolver = new();
    private IDisposable? _rowMetricSubscription;
    private object?[]? _rowMetricInputs;
    private DispatcherOperation? _rowMetricInvalidation;
    private bool _rowMetricsChanging;
    private DataGridViewportAnchor? _rowMetricAnchor;

    #endregion

    #region 控件生命周期

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _rowMetricSubscription?.Dispose();
        _rowMetricInputs = CaptureRowMetricInputs(_rowMetricResolver.Capture(this));
        _rowMetricSubscription = _rowMetricResolver.Subscribe(this, RefreshRowMetricInputs);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _rowMetricSubscription?.Dispose();
        _rowMetricSubscription = null;
        _rowMetricInvalidation?.Abort();
        _rowMetricInvalidation = null;
        _rowMetricInputs = null;
        _rowMetricsChanging = false;
        _rowMetricAnchor = null;
        LayoutUpdated -= CompleteRowMetricLayout;
        base.OnDetachedFromLogicalTree(e);
    }

    #endregion

    #region 私有实现

    private object?[] CaptureRowMetricInputs(ThemeSnapshot snapshot)
    {
        // This read-only provider captures one committed snapshot. It does not
        // create a resource host, binding, or context, and does not retain it.
        var resources = new ThemeTokenResourceProvider(snapshot);
        var values = new object?[RowMetricResourceKeys.Length + 1];
        values[0] = SizeType;
        for (var index = 0; index < RowMetricResourceKeys.Length; index++)
        {
            resources.TryGetResource(RowMetricResourceKeys[index], null, out values[index + 1]);
        }
        return values;
    }

    private void RefreshRowMetricInputs(ThemeSnapshot snapshot)
    {
        var values = CaptureRowMetricInputs(snapshot);
        if (_rowMetricInputs is not null && _rowMetricInputs.SequenceEqual(values))
        {
            return;
        }
        _rowMetricInputs = values;
        if (!_rowMetricsChanging)
        {
            _rowMetricAnchor = CaptureFirstCompleteRangeAnchor();
            _rowMetricsChanging = true;
        }

        // Publication queues compiled/stream bindings as well. Retire measurements
        // once that batch has applied; measuring halfway through it seeds old sizes.
        if (_rowMetricInvalidation is null)
        {
            _rowMetricInvalidation = Dispatcher.UIThread.InvokeAsync(InvalidateRowMetrics, DispatcherPriority.Loaded);
        }
    }

    private void InvalidateRowMetrics()
    {
        _rowMetricInvalidation = null;
        if (_rowMetricSubscription is null)
        {
            return;
        }

        // Measured range/detail heights belong to the typography and geometry
        // that produced them. Their monotonic guards remain valid within that
        // revision, but must not impose old heights after an Own/Global change.
        _rangeMeasuredDetailsHeights.Clear();
        _rowDetailsHeightEstimateTable.Clear();
        RowDetailsHeightEstimate = 0;
        _rangeMeasurementScrollDirection = 0;
        InvalidateRowHeightEstimate();
        RebaseRangeHeightIndex(discardMeasuredDataHeights: true);
        LayoutUpdated -= CompleteRowMetricLayout;
        LayoutUpdated += CompleteRowMetricLayout;
        foreach (var row in GetAllRows())
        {
            row.InvalidateThemeMeasurements();
        }
        InvalidateRowsMeasure(invalidateIndividualElements: true);
        InvalidateMeasure();
    }

    private void CompleteRowMetricLayout(object? sender, EventArgs e)
    {
        LayoutUpdated -= CompleteRowMetricLayout;
        _rowMetricsChanging = false;
        var anchor = _rowMetricAnchor;
        _rowMetricAnchor = null;
        if (anchor.HasValue && _rangePresentationIndex is not null && DisplayData.FirstScrollingSlot >= 0)
        {
            // Metric changes rebuild the height index; reconcile the restored anchor through the
            // committed-range path, which clamps, normalizes displayed rows and publishes the offset.
            ReconcileCommittedRangeOffset(_rangePresentationIndex.RestoreAnchor(anchor.Value));
            InvalidateMeasure();
        }
    }

    #endregion
}
