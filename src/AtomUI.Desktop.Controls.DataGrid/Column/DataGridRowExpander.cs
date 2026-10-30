using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class DataGridRowExpander : ToggleButton
{
    private static readonly Func<bool, bool?> s_detailsVisibleToIsChecked = ConvertDetailsVisibleToIsChecked;

    internal static readonly DirectProperty<DataGridRowExpander, double> IndicatorThicknessProperty =
        AvaloniaProperty.RegisterDirect<DataGridRowExpander, double>(
            nameof(IndicatorThickness),
            o => o.IndicatorThickness,
            (o, v) => o.IndicatorThickness = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGridRowExpander>();
    
    internal static readonly StyledProperty<ITransform?> VerticalIndicatorRenderTransformProperty = 
        AvaloniaProperty.Register<DataGridRowExpander, ITransform?>(nameof (VerticalIndicatorRenderTransform));
    
    internal static readonly StyledProperty<ITransform?> HorizontalIndicatorRenderTransformProperty = 
        AvaloniaProperty.Register<DataGridRowExpander, ITransform?>(nameof (HorizontalIndicatorRenderTransform));
    
    internal static readonly StyledProperty<IBrush?> IndicatorFillProperty = 
        AvaloniaProperty.Register<DataGridRowExpander, IBrush?>(nameof (IndicatorFill));

    internal double IndicatorThickness
    {
        get => _indicatorThickness;
        set => SetAndRaise(IndicatorThicknessProperty, ref _indicatorThickness, value);
    }

    private double _indicatorThickness;
    private DataGridRow? _owningRow;

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    internal ITransform? VerticalIndicatorRenderTransform
    {
        get => GetValue(VerticalIndicatorRenderTransformProperty);
        set => SetValue(VerticalIndicatorRenderTransformProperty, value);
    }
    
    internal ITransform? HorizontalIndicatorRenderTransform
    {
        get => GetValue(HorizontalIndicatorRenderTransformProperty);
        set => SetValue(HorizontalIndicatorRenderTransformProperty, value);
    }

    internal IBrush? IndicatorFill
    {
        get => GetValue(IndicatorFillProperty);
        set => SetValue(IndicatorFillProperty, value);
    }

    private Rectangle? _horizontalIndicator;
    private Rectangle? _verticalIndicator;
    private IDisposable? _detailsVisibilitySubscription;

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size  = base.ArrangeOverride(finalSize);
        var delta = IndicatorThickness;
        if (_verticalIndicator != null)
        {
            var indicatorSize = _verticalIndicator.DesiredSize;
            var x             = (finalSize.Width - indicatorSize.Width) / 2 - delta;
            var y             = (finalSize.Height - indicatorSize.Height) / 2 - delta;
            Canvas.SetLeft(_verticalIndicator, x);
            Canvas.SetTop(_verticalIndicator, y);
        }

        if (_horizontalIndicator != null)
        {
            var indicatorSize = _horizontalIndicator.DesiredSize;
            var x             = (finalSize.Width - indicatorSize.Width) / 2 - delta;
            var y             = (finalSize.Height - indicatorSize.Height) / 2 - delta;
            Canvas.SetLeft(_horizontalIndicator, x);
            Canvas.SetTop(_horizontalIndicator, y);
        }

        return size;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderThicknessProperty)
        {
            IndicatorThickness = BorderThickness.Left;
        }
    }

    protected override void Toggle()
    {
        base.Toggle();
        if (_owningRow != null)
        {
            _owningRow.IsDetailsVisible = IsChecked == true;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _verticalIndicator   = e.NameScope.Find<Rectangle>(DataGridRowExpanderThemeConstants.VerticalIndicatorPart);
        _horizontalIndicator = e.NameScope.Find<Rectangle>(DataGridRowExpanderThemeConstants.HorizontalIndicatorPart);
    }

    internal void NotifyLoadingRow(DataGridRow row)
    {
        ReleaseDetailsVisibilitySubscription();
        _owningRow = row;
        _detailsVisibilitySubscription = this.Bind(
            IsCheckedProperty,
            row.GetObservable(DataGridRow.IsDetailsVisibleProperty, s_detailsVisibleToIsChecked));
    }

    internal void NotifyUnLoadingRow(DataGridRow row)
    {
        ReleaseDetailsVisibilitySubscription();
    }

    private void ReleaseDetailsVisibilitySubscription()
    {
        _owningRow = null;
        _detailsVisibilitySubscription?.Dispose();
        _detailsVisibilitySubscription = null;
    }

    private static bool? ConvertDetailsVisibleToIsChecked(bool isDetailsVisible)
    {
        return isDetailsVisible;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ReleaseDetailsVisibilitySubscription();
    }
}
