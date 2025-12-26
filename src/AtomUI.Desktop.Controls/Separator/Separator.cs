using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaSeparator = Avalonia.Controls.Separator;

public enum SeparatorTitlePosition
{
    Left,
    Right,
    Center
}

public enum SeparatorVariant
{
    Solid,
    Dotted,
    Dashed
}

[PseudoClasses(SeparatorPseudoClass.HasTitleText)]
public class Separator : AvaloniaSeparator,
                         ISizeTypeAware,
                         IControlSharedTokenResourcesHost
{
    private const double SEPARATOR_LINE_MIN_PROPORTION = 0.25;
    
    #region 公共属性定义

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Separator, string?>(nameof(Title));

    public static readonly StyledProperty<SeparatorTitlePosition> TitlePositionProperty =
        AvaloniaProperty.Register<Separator, SeparatorTitlePosition>(nameof(TitlePosition),
            SeparatorTitlePosition.Center);

    public static readonly StyledProperty<IBrush?> TitleColorProperty =
        AvaloniaProperty.Register<Separator, IBrush?>(nameof(TitleColor));

    public static readonly StyledProperty<IBrush?> LineColorProperty =
        AvaloniaProperty.Register<Separator, IBrush?>(nameof(LineColor));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Separator, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<double> OrientationMarginProperty =
        AvaloniaProperty.Register<Separator, double>(nameof(OrientationMargin), double.NaN);
    
    public static readonly StyledProperty<SeparatorVariant> VariantProperty =
        AvaloniaProperty.Register<Separator, SeparatorVariant>(nameof(Variant), SeparatorVariant.Solid);

    public static readonly StyledProperty<double> LineWidthProperty =
        AvaloniaProperty.Register<Separator, double>(nameof(LineWidth), 1);
    
    public static readonly StyledProperty<bool> IsPlainProperty =
        AvaloniaProperty.Register<Separator, bool>(nameof(IsPlain), false);
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Separator>();

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
    /// 分割线的宽度，这里的宽度是 RenderScaling 中立的像素值
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
    /// The size of divider. Only valid for horizontal layout
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> TextPaddingInlineProperty =
        AvaloniaProperty.Register<Separator, double>(
            nameof(TextPaddingInline));

    internal static readonly StyledProperty<double> OrientationMarginPercentProperty =
        AvaloniaProperty.Register<Separator, double>(
            nameof(OrientationMarginPercent));

    internal static readonly StyledProperty<double> VerticalMarginInlineProperty =
        AvaloniaProperty.Register<Separator, double>(
            nameof(VerticalMarginInline));

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

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SeparatorToken.ID;

    #endregion
    
    private TextBlock? _titleLabel;
    private double _currentEdgeDistance;
    private static ImmutableDashStyle? s_dash;
    private static ImmutableDashStyle? s_dot;

    static Separator()
    {
        AffectsMeasure<Separator>(OrientationProperty,
            LineWidthProperty,
            TitleProperty);
        AffectsArrange<Separator>(TitlePositionProperty);
        AffectsRender<Separator>(TitleColorProperty,
            LineColorProperty,
            IsPlainProperty);
    }

    public Separator()
    {
        this.RegisterResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty)
        {
            UpdatePseudoClasses();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _titleLabel = e.NameScope.Find<TextBlock>(SeparatorThemeConstants.TitlePart);
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
            targetWidth  = Math.Max(1, LineWidth) + VerticalMarginInline;
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
        if (Orientation == Orientation.Horizontal && _titleLabel?.IsVisible == true)
        {
            var titleRect = GetTitleRect(DesiredSize.Deflate(Margin));
            _titleLabel.Arrange(titleRect);
        }

        return size;
    }

    private double GetTextPaddingInline()
    {
        return FontSize * TextPaddingInline;
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
                    _currentEdgeDistance = finalSize.Width * OrientationMarginPercent;
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
                    _currentEdgeDistance = finalSize.Width * OrientationMarginPercent;
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

    public override void Render(DrawingContext context)
    {
        using var state = context.PushRenderOptions(new RenderOptions
        {
            EdgeMode = EdgeMode.Aliased
        });
        IDashStyle? lineStyle   = null;
        
        if (Variant == SeparatorVariant.Dashed)
        {
            lineStyle = DashStyle;
        }
        else if (Variant == SeparatorVariant.Dotted)
        {
            lineStyle = DotStyle;
        }
        
        var linePen     = new Pen(LineColor, LineWidth, lineStyle);
        var controlRect = new Rect(DesiredSize.Deflate(Margin));

        if (Orientation == Orientation.Horizontal)
        {
            var offsetY = controlRect.Height / 2.0;
            if (Title?.Length > 0)
            {
                // 画两个线段
                var titleRect = GetTitleRect(controlRect.Size);
                if (TitlePosition == SeparatorTitlePosition.Left)
                {
                    if (double.IsNaN(OrientationMargin))
                    {
                        context.DrawLine(linePen, new Point(0, offsetY),
                            new Point(titleRect.Left - GetTextPaddingInline(), offsetY));
                    }

                    context.DrawLine(linePen, new Point(titleRect.Right + GetTextPaddingInline(), offsetY),
                        new Point(controlRect.Right, offsetY));
                }
                else if (TitlePosition == SeparatorTitlePosition.Right)
                {
                    context.DrawLine(linePen, new Point(0, offsetY),
                        new Point(titleRect.Left - GetTextPaddingInline(), offsetY));
                    if (double.IsNaN(OrientationMargin))
                    {
                        context.DrawLine(linePen, new Point(titleRect.Right + GetTextPaddingInline(), offsetY),
                            new Point(controlRect.Right, offsetY));
                    }
                }
                else
                {
                    context.DrawLine(linePen, new Point(0, offsetY),
                        new Point(titleRect.Left - GetTextPaddingInline(), offsetY));
                    context.DrawLine(linePen, new Point(titleRect.Right + GetTextPaddingInline(), offsetY),
                        new Point(controlRect.Right, offsetY));
                }
            }
            else
            {
                context.DrawLine(linePen, new Point(0, offsetY), new Point(controlRect.Right, offsetY));
            }
        }
        else
        {
            var offsetX = controlRect.Width / 2.0;
            var offsetY = controlRect.Height * 0.2;
            context.DrawLine(linePen, new Point(offsetX, offsetY), new Point(offsetX, controlRect.Bottom - offsetY));
        }
    }
    
    public static IDashStyle DashStyle => s_dash ??= new ImmutableDashStyle([4, 2], 0);
    public static IDashStyle DotStyle => s_dot ??= new ImmutableDashStyle([1, 1], 0);
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(SeparatorPseudoClass.HasTitleText, !string.IsNullOrEmpty(Title));
    }
}

public class VerticalSeparator : Separator
{
    static VerticalSeparator()
    {
        OrientationProperty.OverrideDefaultValue<VerticalSeparator>(Orientation.Vertical);
    }

    protected override Type StyleKeyOverride => typeof(Separator);
}