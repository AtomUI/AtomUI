using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class TextAreaDecoratedBox : AddOnDecoratedBox
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsAllowAutoHideProperty =
        AvaloniaProperty.Register<TextAreaDecoratedBox, bool>(
            nameof(IsAllowAutoHide));
    
    public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<TextAreaDecoratedBox, ScrollBarVisibility>(
            nameof(HorizontalScrollBarVisibility), ScrollBarVisibility.Disabled);
    
    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<TextAreaDecoratedBox, ScrollBarVisibility>(
            nameof(VerticalScrollBarVisibility), ScrollBarVisibility.Auto);
    
    public static readonly StyledProperty<bool> IsScrollChainingEnabledProperty =
        AvaloniaProperty.Register<TextAreaDecoratedBox, bool>(
            nameof(IsScrollChainingEnabled), true);
    
    public static readonly StyledProperty<IBrush?> ResizeIndicatorLineBrushProperty =
        AvaloniaProperty.Register<TextAreaDecoratedBox, IBrush?>(
            nameof(ResizeIndicatorLineBrush));
    
    public bool IsAllowAutoHide
    {
        get => GetValue(IsAllowAutoHideProperty);
        set => SetValue(IsAllowAutoHideProperty, value);
    }
    
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }
    
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }
    
    public bool IsScrollChainingEnabled
    {
        get => GetValue(IsScrollChainingEnabledProperty);
        set => SetValue(IsScrollChainingEnabledProperty, value);
    }
    
    public IBrush? ResizeIndicatorLineBrush
    {
        get => GetValue(ResizeIndicatorLineBrushProperty);
        set => SetValue(ResizeIndicatorLineBrushProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<TextAreaDecoratedBox, bool> IsResizableProperty =
        AvaloniaProperty.RegisterDirect<TextAreaDecoratedBox, bool>(nameof(IsResizable),
            o => o.IsResizable,
            (o, v) => o.IsResizable = v);
    
    internal static readonly StyledProperty<Thickness> TextAreaFramePaddingProperty =
        AvaloniaProperty.Register<TextAreaDecoratedBox, Thickness>(
            nameof(TextAreaFramePadding));
    
    internal static readonly StyledProperty<Thickness> ScrollerPaddingProperty =
        AvaloniaProperty.Register<TextAreaDecoratedBox, Thickness>(
            nameof(ScrollerPadding));
    
    private bool _isResizable;

    internal bool IsResizable
    {
        get => _isResizable;
        set => SetAndRaise(IsResizableProperty, ref _isResizable, value);
    }
    
    internal Thickness TextAreaFramePadding
    {
        get => GetValue(TextAreaFramePaddingProperty);
        set => SetValue(TextAreaFramePaddingProperty, value);
    }
    
    internal Thickness ScrollerPadding
    {
        get => GetValue(ScrollerPaddingProperty);
        set => SetValue(ScrollerPaddingProperty, value);
    }
    #endregion

    static TextAreaDecoratedBox()
    {
        AffectsRender<TextAreaDecoratedBox>(ResizeIndicatorLineBrushProperty, IsResizableProperty);
    }
    
    internal ScrollViewer? ScrollViewer { get; set; }
    internal TextArea? Owner { get; set; }
    private Control? _rightContentAddOn;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ScrollViewer = e.NameScope.Find<ScrollViewer>(TextAreaThemeConstants.ScrollViewerPart);
        
        if (_rightContentAddOn != null)
        {
            _rightContentAddOn.SizeChanged -= HandleRightContentAddOnSizeChanged;
        }
        
        _rightContentAddOn = e.NameScope.Find<Control>(AddOnDecoratedBoxThemeConstants.ContentRightAddOnPart);
        if (ScrollViewer != null)
        {
            Owner?.NotifyScrollViewerCreated(ScrollViewer);
        }

        if (_rightContentAddOn != null)
        {
            _rightContentAddOn.SizeChanged += HandleRightContentAddOnSizeChanged;
        }
    }

    private void HandleRightContentAddOnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ScrollerPadding = new Thickness(0, 0, e.NewSize.Width, 0);
    }

    public override void Render(DrawingContext context)
    {
        if (IsResizable)
        {
            var bounds     = Bounds.Deflate(new Thickness(2));
            var sideLength = 7d;
            var (line1Start, line1End) = CalculateResizeLine(bounds, sideLength);
            var (line2Start, line2End) = CalculateResizeLine(bounds, sideLength - 3);
            var pen = new Pen(ResizeIndicatorLineBrush);
        
            context.DrawLine(pen, line1Start, line1End);
            context.DrawLine(pen, line2Start, line2End);
        }
    }

    private (Point, Point) CalculateResizeLine(Rect bounds, double sideLength)
    {
        double right       = bounds.Right;
        double bottom      = bounds.Bottom;
        Point rightSidePoint = new Point(right, bottom - sideLength);
        Point bottomSidePoint = new Point(right - sideLength, bottom);
        
        return (rightSidePoint, bottomSidePoint);
    }
}