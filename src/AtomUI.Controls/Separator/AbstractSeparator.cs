using System.Globalization;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Metadata;

namespace AtomUI.Controls.Commons;

using AvaloniaSeparator = Avalonia.Controls.Separator;

[PseudoClasses(SeparatorPseudoClass.HasTitleText)]
public abstract class AbstractSeparator : AvaloniaSeparator, ICustomizableSizeTypeAware
{
    private const double SEPARATOR_LINE_MIN_PROPORTION = 0.25;
    
    #region 公共属性定义

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AbstractSeparator, string?>(nameof(Title));

    public static readonly StyledProperty<SeparatorTitlePosition> TitlePositionProperty =
        AvaloniaProperty.Register<AbstractSeparator, SeparatorTitlePosition>(nameof(TitlePosition),
            SeparatorTitlePosition.Center);

    public static readonly StyledProperty<IBrush?> TitleColorProperty =
        AvaloniaProperty.Register<AbstractSeparator, IBrush?>(nameof(TitleColor));

    public static readonly StyledProperty<IBrush?> LineColorProperty =
        AvaloniaProperty.Register<AbstractSeparator, IBrush?>(nameof(LineColor));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AbstractSeparator, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<double> OrientationMarginProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(nameof(OrientationMargin), double.NaN);
    
    public static readonly StyledProperty<SeparatorVariant> VariantProperty =
        AvaloniaProperty.Register<AbstractSeparator, SeparatorVariant>(nameof(Variant), SeparatorVariant.Solid);

    public static readonly StyledProperty<double> LineWidthProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(nameof(LineWidth), 1);
    
    public static readonly StyledProperty<bool> IsPlainProperty =
        AvaloniaProperty.Register<AbstractSeparator, bool>(nameof(IsPlain), false);
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSeparator>();

    /// <summary>
    /// 分割线的标题
    /// </summary>
    [Content]
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// 分割线标题的位置
    /// </summary>
    public SeparatorTitlePosition TitlePosition
    {
        get => GetValue(TitlePositionProperty);
        set => SetValue(TitlePositionProperty, value);
    }

    /// <summary>
    /// 分割线标题的颜色
    /// </summary>
    public IBrush? TitleColor
    {
        get => GetValue(TitleColorProperty);
        set => SetValue(TitleColorProperty, value);
    }

    /// <summary>
    /// 分割线标题的颜色
    /// </summary>
    public IBrush? LineColor
    {
        get => GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    /// <summary>
    /// 分割线的方向，垂直和水平分割线
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    
    /// <summary>
    /// 分割线是虚线、点线还是实线
    /// </summary>
    public SeparatorVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>
    /// The margin-left/right between the title and its closest border, while the orientation must be left or right,
    /// If a numeric value of type string is provided without a unit, it is assumed to be in pixels (px) by default.
    /// </summary>
    /// <returns></returns>
    public double OrientationMargin
    {
        get => GetValue(OrientationMarginProperty);
        set => SetValue(OrientationMarginProperty, value);
    }
    
    /// <summary>
    /// 分割线的设计宽度，实际绘制厚度会根据当前 render scale 调整
    /// </summary>
    public double LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }
    
    /// <summary>
    /// 文字是否显示为普通正文样式
    /// </summary>
    public bool IsPlain
    {
        get => GetValue(IsPlainProperty);
        set => SetValue(IsPlainProperty, value);
    }
    
    /// <summary>
    /// The size of divider. Only valid for horizontal layout. Custom lets the caller own the margin.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> TextPaddingInlineProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(
            nameof(TextPaddingInline));

    internal static readonly StyledProperty<double> OrientationMarginPercentProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(
            nameof(OrientationMarginPercent));

    internal static readonly StyledProperty<double> VerticalMarginInlineProperty =
        AvaloniaProperty.Register<AbstractSeparator, double>(
            nameof(VerticalMarginInline));

    internal static readonly StyledProperty<bool> UseSeparatorLayoutTokenResourcesProperty =
        AvaloniaProperty.Register<AbstractSeparator, bool>(nameof(UseSeparatorLayoutTokenResources));

    internal double TextPaddingInline
    {
        get => GetValue(TextPaddingInlineProperty);
        set => SetValue(TextPaddingInlineProperty, value);
    }

    internal double OrientationMarginPercent
    {
        get => GetValue(OrientationMarginPercentProperty);
        set => SetValue(OrientationMarginPercentProperty, value);
    }

    internal double VerticalMarginInline
    {
        get => GetValue(VerticalMarginInlineProperty);
        set => SetValue(VerticalMarginInlineProperty, value);
    }

    internal bool UseSeparatorLayoutTokenResources
    {
        get => GetValue(UseSeparatorLayoutTokenResourcesProperty);
        set => SetValue(UseSeparatorLayoutTokenResourcesProperty, value);
    }

    #endregion
    
    private TextBlock? _titleLabel;
    private SeparatorRail? _railStart;
    private SeparatorRail? _railEnd;
    private double _currentEdgeDistance;
    private OwnerTokenSet? _layoutTokens;
    private (double TextPadding, double EdgeRatio, double VerticalMargin) _layoutTokensValues;

    private protected virtual OwnerTokenSet? CreateSeparatorLayoutTokenSet(Action changed) => null;

    private double EffectiveTextPadding =>
        IsSet(TextPaddingInlineProperty) ? TextPaddingInline : _layoutTokensValues.TextPadding;

    private double EffectiveEdgeRatio =>
        IsSet(OrientationMarginPercentProperty) ? OrientationMarginPercent : _layoutTokensValues.EdgeRatio;

    private double EffectiveVerticalMargin =>
        IsSet(VerticalMarginInlineProperty) ? VerticalMarginInline : _layoutTokensValues.VerticalMargin;

    static AbstractSeparator()
    {
        // Shared themes may set the layout properties directly.
        AffectsMeasure<AbstractSeparator>(OrientationProperty,
            LineWidthProperty,
            TitleProperty,
            VerticalMarginInlineProperty);
        AffectsArrange<AbstractSeparator>(TitlePositionProperty,
            OrientationMarginProperty,
            TextPaddingInlineProperty,
            OrientationMarginPercentProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty)
        {
            UpdatePseudoClasses();
        }
        if (change.Property == UseSeparatorLayoutTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateLayoutTokenSubscription();
        }
        else if (_layoutTokens is not null && (change.Property == OrientationProperty || change.Property == TitleProperty))
        {
            RefreshLayoutMetrics();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateLayoutTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _layoutTokens?.Dispose();
        _layoutTokens = null;
        _layoutTokensValues = default;
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateLayoutTokenSubscription()
    {
        _layoutTokens?.Dispose();
        _layoutTokens = UseSeparatorLayoutTokenResources ? CreateSeparatorLayoutTokenSet(RefreshLayoutMetrics) : null;
        RefreshLayoutMetrics();
    }

    private void RefreshLayoutMetrics()
    {
        var previous = (TextPadding: EffectiveTextPadding, EdgeRatio: EffectiveEdgeRatio, VerticalMargin: EffectiveVerticalMargin);
        var vertical = Orientation == Orientation.Vertical;
        var titled = !vertical && !string.IsNullOrEmpty(Title);
        _layoutTokensValues = (
            titled ? ReadLayoutToken(0, _layoutTokensValues.TextPadding) : 0,
            titled ? ReadLayoutToken(1, _layoutTokensValues.EdgeRatio) : 0,
            vertical ? ReadLayoutToken(2, _layoutTokensValues.VerticalMargin) : 0);

        if (vertical && !previous.VerticalMargin.Equals(EffectiveVerticalMargin))
        {
            InvalidateMeasure();
        }
        else if (titled && (!previous.TextPadding.Equals(EffectiveTextPadding) || !previous.EdgeRatio.Equals(EffectiveEdgeRatio)))
        {
            InvalidateArrange();
        }
    }

    private double ReadLayoutToken(int index, double previous)
    {
        var value = _layoutTokens?.GetValue(index);
        if (value is double metric)
        {
            return metric;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _titleLabel = e.NameScope.Find<TextBlock>("PART_Title");
        _railStart  = e.NameScope.Find<SeparatorRail>("PART_RailStart");
        _railEnd    = e.NameScope.Find<SeparatorRail>("PART_RailEnd");
        UpdatePseudoClasses();
    }
    
    // 当为水平分隔线的时候，我们设置最小的高度，当为垂直分割线的时候我们设置一个合适宽度
    // 然后保持尽可能保持文字尽可能的显示，如果小于最小分隔部分的两倍的时候，文字隐藏。
    protected override Size MeasureOverride(Size availableSize)
    {
        var size         = base.MeasureOverride(availableSize);
        var targetHeight = size.Height;
        var targetWidth  = size.Width;
        if (Orientation == Orientation.Horizontal)
        {
            if (Title is null || Title?.Length == 0)
            {
                targetHeight = LineWidth * 3;
            }

            if (!double.IsInfinity(availableSize.Width))
            {
                targetWidth = Math.Max(availableSize.Width, targetWidth);
            }
        }
        else
        {
            targetWidth  = Math.Max(1, LineWidth) + EffectiveVerticalMargin;
            targetHeight = FontUtils.ConvertEmToPixel(1, FontSize, TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0);
            if (!double.IsInfinity(availableSize.Height))
            {
                targetHeight = Math.Max(availableSize.Height, targetHeight);
            }
        }

        return new Size(targetWidth, targetHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        ArrangeRails(finalSize);
        if (Orientation == Orientation.Horizontal && _titleLabel?.IsVisible == true)
        {
            _titleLabel.Arrange(GetTitleRect(finalSize));
        }

        return size;
    }

    private void ArrangeRails(Size finalSize)
    {
        if (_railStart is null || _railEnd is null)
        {
            return;
        }

        if (Orientation == Orientation.Horizontal)
        {
            if (Title?.Length > 0)
            {
                var titleRect  = GetTitleRect(finalSize);
                var textPadding = GetTextPaddingInline();
                var drawStart  = TitlePosition != SeparatorTitlePosition.Left ||
                                 double.IsNaN(OrientationMargin);
                var drawEnd    = TitlePosition != SeparatorTitlePosition.Right ||
                                 double.IsNaN(OrientationMargin);

                var startWidth = drawStart
                    ? Math.Max(0, titleRect.Left - textPadding)
                    : 0;
                var endLeft   = titleRect.Right + textPadding;
                var endWidth  = drawEnd
                    ? Math.Max(0, finalSize.Width - endLeft)
                    : 0;

                _railStart.Arrange(new Rect(0, 0, startWidth, finalSize.Height));
                _railEnd.Arrange(new Rect(endLeft, 0, endWidth, finalSize.Height));
            }
            else
            {
                _railStart.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
                _railEnd.Arrange(new Rect(0, 0, 0, 0));
            }
        }
        else
        {
            var inset      = finalSize.Height * 0.2;
            var railHeight = Math.Max(0, finalSize.Height - inset * 2);
            _railStart.Arrange(new Rect(0, inset, finalSize.Width, railHeight));
            _railEnd.Arrange(new Rect(0, 0, 0, 0));
        }
    }

    private double GetTextPaddingInline()
    {
        return FontSize * EffectiveTextPadding;
    }

    private Rect GetTitleRect(Size finalSize)
    {
        Rect titleRect = default;
        if (Orientation == Orientation.Horizontal && Title?.Length > 0)
        {
            // 线最小得占到 25 %，拍脑袋
            var lineMinWidth = finalSize.Width * SEPARATOR_LINE_MIN_PROPORTION;
            var titleWidth   = (_titleLabel?.DesiredSize.Width ?? 0) + 2;
            var remainWidth  = finalSize.Width - titleWidth - GetTextPaddingInline() * 2;
            if (lineMinWidth > remainWidth)
            {
                // 字过多
                titleWidth = Math.Max(finalSize.Width - lineMinWidth, lineMinWidth);
            }

            // 处理完成之后，字的宽度一定在 width 范围内
            // 计算位置
            if (TitlePosition == SeparatorTitlePosition.Left)
            {
                if (!double.IsNaN(OrientationMargin))
                {
                    _currentEdgeDistance = Math.Min((finalSize.Width - titleWidth) / 2, OrientationMargin);
                }
                else
                {
                    _currentEdgeDistance = finalSize.Width * EffectiveEdgeRatio;
                }

                titleRect = new Rect(new Point(_currentEdgeDistance + GetTextPaddingInline(), 0),
                    new Size(titleWidth, finalSize.Height));
                var rightDelta = titleRect.Right - finalSize.Width;
                if (MathUtils.GreaterThan(rightDelta, 0))
                {
                    titleRect = titleRect.WithWidth(Math.Max(finalSize.Width - titleRect.Left, lineMinWidth));
                }
            }
            else if (TitlePosition == SeparatorTitlePosition.Right)
            {
                if (!double.IsNaN(OrientationMargin))
                {
                    _currentEdgeDistance = Math.Min((finalSize.Width - titleWidth) / 2, OrientationMargin);
                }
                else
                {
                    _currentEdgeDistance = finalSize.Width * EffectiveEdgeRatio;
                }

                titleRect = new Rect(
                    new Point(finalSize.Width - _currentEdgeDistance - titleWidth - GetTextPaddingInline() * 2, 0),
                    new Size(titleWidth, finalSize.Height));
                var leftDelta = titleRect.Left - 0;
                if (leftDelta < 0)
                {
                    titleRect = titleRect.WithX(0);
                }
            }
            else
            {
                // 居中
                titleRect = new Rect(new Point((finalSize.Width - titleWidth) / 2, 0),
                    new Size(titleWidth, finalSize.Height));
            }
        }

        return titleRect;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(SeparatorPseudoClass.HasTitleText, !string.IsNullOrEmpty(Title));
    }
}
