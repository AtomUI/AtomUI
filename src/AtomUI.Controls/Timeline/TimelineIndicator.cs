using System.Globalization;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Controls.Commons;

[TemplatePart("PART_Rail", typeof(Border))]
[TemplatePart("PART_Dot", typeof(Border))]
[TemplatePart("PART_IconHost", typeof(Border))]
[TemplatePart("PART_IconPresenter", typeof(IconPresenter))]
internal class TimelineIndicator : TemplatedControl
{
    public const string IconPresentPC = ":icon-present";

    #region 公共属性定义

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<TimelineIndicator>();

    public static readonly StyledProperty<PathIcon?> IndicatorIconProperty =
        AvaloniaProperty.Register<TimelineIndicator, PathIcon?>(nameof(IndicatorIcon));

    public static readonly StyledProperty<IBrush?> IndicatorColorProperty =
        AvaloniaProperty.Register<TimelineIndicator, IBrush?>(nameof(IndicatorColor));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public PathIcon? IndicatorIcon
    {
        get => GetValue(IndicatorIconProperty);
        set => SetValue(IndicatorIconProperty, value);
    }

    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> RelativeLineHeightProperty =
        AvaloniaProperty.Register<TimelineIndicator, double>(nameof(RelativeLineHeight));

    internal static readonly StyledProperty<TimelineIndicatorTokenResourceKeys?> IndicatorTokenResourceKeysProperty =
        AvaloniaProperty.Register<TimelineIndicator, TimelineIndicatorTokenResourceKeys?>(nameof(IndicatorTokenResourceKeys));

    internal static readonly DirectProperty<TimelineIndicator, bool> IsFirstProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, bool>(nameof(IsFirst),
            o => o.IsFirst,
            (o, v) => o.IsFirst = v);

    internal static readonly DirectProperty<TimelineIndicator, bool> IsLastProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, bool>(nameof(IsLast),
            o => o.IsLast,
            (o, v) => o.IsLast = v);

    internal static readonly DirectProperty<TimelineIndicator, double> IndicatorMinHeightProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, double>(nameof(IndicatorMinHeight),
            o => o.IndicatorMinHeight,
            (o, v) => o.IndicatorMinHeight = v);
    
    internal static readonly StyledProperty<IBrush?> DefaultIndicatorColorProperty =
        AvaloniaProperty.Register<TimelineIndicator, IBrush?>(nameof(DefaultIndicatorColor));

    // The item colour takes precedence over the theme's default for the dot and icon.
    internal static readonly DirectProperty<TimelineIndicator, IBrush?> EffectiveIndicatorColorProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, IBrush?>(nameof(EffectiveIndicatorColor),
            o => o.EffectiveIndicatorColor);
    
    internal static readonly StyledProperty<double> IndicatorDotBorderWidthProperty =
        AvaloniaProperty.Register<TimelineIndicator, double>(nameof(IndicatorDotBorderWidth));
    
    internal static readonly StyledProperty<IBrush?> IndicatorTailColorProperty =
        AvaloniaProperty.Register<TimelineIndicator, IBrush?>(nameof(IndicatorTailColor));
    
    internal static readonly StyledProperty<double> IndicatorTailWidthProperty =
        AvaloniaProperty.Register<TimelineIndicator, double>(nameof(IndicatorTailWidth));
    
    internal static readonly StyledProperty<double> IndicatorDotSizeProperty =
        AvaloniaProperty.Register<TimelineIndicator, double>(nameof(IndicatorDotSize));
    

    internal static readonly DirectProperty<TimelineIndicator, bool> NextIsPendingProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, bool>(nameof(NextIsPending),
            o => o.NextIsPending,
            (o, v) => o.NextIsPending = v);
    
    internal double RelativeLineHeight
    {
        get => GetValue(RelativeLineHeightProperty);
        set => SetValue(RelativeLineHeightProperty, value);
    }

    internal IBrush? DefaultIndicatorColor
    {
        get => GetValue(DefaultIndicatorColorProperty);
        set => SetValue(DefaultIndicatorColorProperty, value);
    }

    internal TimelineIndicatorTokenResourceKeys? IndicatorTokenResourceKeys
    {
        get => GetValue(IndicatorTokenResourceKeysProperty);
        set => SetValue(IndicatorTokenResourceKeysProperty, value);
    }

    private OwnerTokenSet? _defaultTokens;
    private double _themeLineHeightRatio;
    private IBrush? _themeIndicatorColor;
    private IBrush? _effectiveIndicatorColor;

    internal IBrush? EffectiveIndicatorColor
    {
        get => _effectiveIndicatorColor;
        private set => SetAndRaise(EffectiveIndicatorColorProperty, ref _effectiveIndicatorColor, value);
    }

    private bool _isFirst;

    internal bool IsFirst
    {
        get => _isFirst;
        set => SetAndRaise(IsFirstProperty, ref _isFirst, value);
    }

    private bool _isLast;

    internal bool IsLast
    {
        get => _isLast;
        set => SetAndRaise(IsLastProperty, ref _isLast, value);
    }

    private double _indicatorMinHeight;

    internal double IndicatorMinHeight
    {
        get => _indicatorMinHeight;
        set => SetAndRaise(IndicatorMinHeightProperty, ref _indicatorMinHeight, value);
    }
    
    public double IndicatorDotBorderWidth
    {
        get => GetValue(IndicatorDotBorderWidthProperty);
        set => SetValue(IndicatorDotBorderWidthProperty, value);
    }
    
    public IBrush? IndicatorTailColor
    {
        get => GetValue(IndicatorTailColorProperty);
        set => SetValue(IndicatorTailColorProperty, value);
    }
    
    public double IndicatorTailWidth
    {
        get => GetValue(IndicatorTailWidthProperty);
        set => SetValue(IndicatorTailWidthProperty, value);
    }
    
    public double IndicatorDotSize
    {
        get => GetValue(IndicatorDotSizeProperty);
        set => SetValue(IndicatorDotSizeProperty, value);
    }

    private bool _nextIsPending;

    internal bool NextIsPending
    {
        get => _nextIsPending;
        set => SetAndRaise(NextIsPendingProperty, ref _nextIsPending, value);
    }
    
    #endregion

    private Border? _rail;
    private Border? _dot;
    private Border? _iconHost;
    private IconPresenter? _iconPresenter;

    static TimelineIndicator()
    {
        OrientationProperty.OverrideDefaultValue<TimelineIndicator>(Orientation.Vertical);
        // rail 要连续延伸到下一项节点顶边，会跨出 indicator 自身的底部边界；
        // TemplatedControl 默认 ClipToBounds=true 会把这溢出的 rail 段切断。
        ClipToBoundsProperty.OverrideDefaultValue<TimelineIndicator>(false);
        AffectsMeasure<TimelineIndicator>(
            IsFirstProperty,
            IsLastProperty,
            OrientationProperty,
            IndicatorIconProperty,
            IndicatorMinHeightProperty);
        AffectsArrange<TimelineIndicator>(
            IsFirstProperty,
            IsLastProperty,
            OrientationProperty,
            IndicatorIconProperty,
            IndicatorMinHeightProperty,
            IndicatorDotBorderWidthProperty,
            IndicatorDotSizeProperty,
            IndicatorTailWidthProperty);
        TextElement.FontSizeProperty.Changed.AddClassHandler<TimelineIndicator>((indicator, _) => indicator.UpdateIndicatorMinHeight());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateIndicatorColor();
        _rail          =   e.NameScope.Find<Border>("PART_Rail");
        _dot           =   e.NameScope.Find<Border>("PART_Dot");
        _iconHost      =   e.NameScope.Find<Border>("PART_IconHost");
        _iconPresenter =   e.NameScope.Find<IconPresenter>("PART_IconPresenter");
        UpdatePseudoClasses();
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IndicatorTokenResourceKeysProperty)
        {
            UpdateIndicatorDefaultsTokenSubscription();
        }
        else if (change.Property == IndicatorColorProperty || change.Property == DefaultIndicatorColorProperty)
        {
            UpdateIndicatorColor();
        }
        else if (change.Property == RelativeLineHeightProperty)
        {
            UpdateIndicatorMinHeight();
        }
        else if (change.Property == IndicatorIconProperty)
        {
            UpdatePseudoClasses();
        }
    }

    private void UpdateIndicatorDefaultsTokenSubscription()
    {
        _defaultTokens?.Dispose();
        _defaultTokens = null;
        if (IndicatorTokenResourceKeys is { } keys)
        {
            _defaultTokens = new OwnerTokenSet(this, 2, RefreshIndicatorDefaults);
            _defaultTokens.SetResourceKey(0, keys.LineHeightRatio);
            _defaultTokens.SetResourceKey(1, keys.NodeColor);
        }
        RefreshIndicatorDefaults();
    }

    private void RefreshIndicatorDefaults()
    {
        _themeLineHeightRatio = ReadLineHeightRatio(_defaultTokens?.GetValue(0));
        _themeIndicatorColor = _defaultTokens?.GetValue(1) switch
        {
            IBrush brush => brush,
            Color color => new ImmutableSolidColorBrush(color),
            var value when ReferenceEquals(value, BindingOperations.DoNothing) => _themeIndicatorColor,
            _ => null
        };
        UpdateIndicatorMinHeight();
        UpdateIndicatorColor();
    }

    private double ReadLineHeightRatio(object? value)
    {
        if (value is double ratio)
        {
            return ratio;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return _themeLineHeightRatio;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private void UpdateIndicatorMinHeight()
    {
        var ratio = _defaultTokens is null || IsSet(RelativeLineHeightProperty) ? RelativeLineHeight : _themeLineHeightRatio;
        IndicatorMinHeight = ratio * TextElement.GetFontSize(this);
    }

    private void UpdateIndicatorColor()
    {
        var fallback = _defaultTokens is null || IsSet(DefaultIndicatorColorProperty) ? DefaultIndicatorColor : _themeIndicatorColor;
        EffectiveIndicatorColor = IndicatorColor ?? fallback;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(IconPresentPC, IndicatorIcon != null);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size   = base.MeasureOverride(availableSize);
        var height = Math.Max(size.Height, IndicatorMinHeight);
        return new Size(size.Width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var boxSize = IndicatorDotSize + IndicatorDotBorderWidth;

        if (_iconHost is not null)
        {
            var iconWidth  = _iconPresenter?.DesiredSize.Width ?? 0;
            var iconHeight = _iconPresenter?.DesiredSize.Height ?? 0;
            var offsetX    = (finalSize.Width - iconWidth) / 2;
            var offsetY    = Orientation == Orientation.Horizontal
                ? (finalSize.Height - iconHeight) / 2
                : (_indicatorMinHeight - iconHeight) / 2;
            _iconHost.Arrange(new Rect(offsetX, offsetY, iconWidth, iconHeight));
        }

        double centerX;
        double centerY;
        if (IndicatorIcon != null && _iconHost is not null)
        {
            centerX = _iconHost.Bounds.X + _iconHost.Bounds.Width / 2;
            centerY = _iconHost.Bounds.Y + _iconHost.Bounds.Height / 2;
        }
        else if (Orientation == Orientation.Horizontal)
        {
            centerX = finalSize.Width / 2;
            centerY = finalSize.Height / 2;
        }
        else
        {
            centerX = finalSize.Width / 2;
            centerY = _indicatorMinHeight / 2 - 1;
        }

        if (_dot is not null)
        {
            // The ring width and the circle radius follow the dot Tokens; current values keep application styles on the
            // dot part in charge.
            _dot.SetCurrentValue(Border.BorderThicknessProperty, new Thickness(IndicatorDotBorderWidth));
            _dot.SetCurrentValue(Border.CornerRadiusProperty, new CornerRadius(boxSize / 2));
            _dot.Arrange(new Rect(centerX - boxSize / 2, centerY - boxSize / 2, boxSize, boxSize));
        }

        if (_rail is not null)
        {
            var tailWidth = IndicatorTailWidth;
            Rect railRect;
            if (Orientation == Orientation.Horizontal)
            {
                // 水平方向：节点位于各 item 中心、items 左右并排，单段 rail 只能覆盖
                // 到 item 边缘（即两节点连线的中点）。因此水平 rail 贯穿相邻 item 连成
                // 完整轴线：首项从自身节点起、末项止于自身节点、中间项全宽贯通，与
                // 相邻 item 的 rail 首尾相接，交叉区域由不透明节点掩膜遮盖。
                var left  = IsFirst ? centerX : 0;
                var right = IsLast ? centerX : finalSize.Width;
                railRect = new Rect(
                    left,
                    finalSize.Height / 2 - tailWidth / 2,
                    Math.Max(0, right - left),
                    tailWidth);
            }
            else
            {
                // 垂直方向：对齐上游 rail 的分段语义——每段从自身节点底边延伸到下一
                // 节点顶边（贯穿 item 边界），线在节点处紧贴、由不透明节点掩膜遮盖，
                // 视觉连续且语义框以节点为界；末项无 rail。
                var glyphHalfHeight = Math.Max(
                    boxSize / 2,
                    _iconHost is not null ? _iconHost.Bounds.Height / 2 : 0);
                var top              = centerY + glyphHalfHeight;
                var nextGlyphTopEdge = finalSize.Height + _indicatorMinHeight / 2 - 1 - boxSize / 2;
                var bottom           = IsLast ? top : nextGlyphTopEdge;
                railRect = new Rect(
                    finalSize.Width / 2 - tailWidth / 2,
                    top,
                    tailWidth,
                    Math.Max(0, bottom - top));
            }

            _rail.Arrange(railRect);
        }

        return finalSize;
    }
}

internal sealed record TimelineIndicatorTokenResourceKeys(object LineHeightRatio, object NodeColor);
