using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls;

public enum ArrowPosition
{
    /// <summary>
    /// Preferred location is below the target element.
    /// </summary>
    Bottom,

    /// <summary>
    /// Preferred location is to the right of the target element.
    /// </summary>
    Right,

    /// <summary>
    /// Preferred location is to the left of the target element.
    /// </summary>
    Left,

    /// <summary>
    /// Preferred location is above the target element.
    /// </summary>
    Top,

    /// <summary>
    /// Preferred location is above the target element, with the left edge of the popup
    /// aligned with the left edge of the target element.
    /// </summary>
    TopEdgeAlignedLeft,

    /// <summary>
    /// Preferred location is above the target element, with the right edge of popup aligned with right edge of the target
    /// element.
    /// </summary>
    TopEdgeAlignedRight,

    /// <summary>
    /// Preferred location is below the target element, with the left edge of popup aligned with left edge of the target
    /// element.
    /// </summary>
    BottomEdgeAlignedLeft,

    /// <summary>
    /// Preferred location is below the target element, with the right edge of popup aligned with right edge of the target
    /// element.
    /// </summary>
    BottomEdgeAlignedRight,

    /// <summary>
    /// Preferred location is to the left of the target element, with the top edge of popup aligned with top edge of the
    /// target element.
    /// </summary>
    LeftEdgeAlignedTop,

    /// <summary>
    /// Preferred location is to the left of the target element, with the bottom edge of popup aligned with bottom edge of
    /// the target element.
    /// </summary>
    LeftEdgeAlignedBottom,

    /// <summary>
    /// Preferred location is to the right of the target element, with the top edge of popup aligned with top edge of the
    /// target element.
    /// </summary>
    RightEdgeAlignedTop,

    /// <summary>
    /// Preferred location is to the right of the target element, with the bottom edge of popup aligned with bottom edge of
    /// the target element.
    /// </summary>
    RightEdgeAlignedBottom
}

[TemplatePart("PART_ContentDecorator", typeof(Border))]
[TemplatePart("PART_ArrowIndicatorLayout", typeof(Control))]
[TemplatePart("PART_ArrowIndicator", typeof(ArrowIndicator))]
[PseudoClasses(BorderedPseudoClass)]
public abstract class AbstractArrowDecoratedBox : ContentControl,
                                                  IArrowAwareShadowMaskInfoProvider,
                                                  IMotionAwareControl
{
    public const string ArrowDecoratorPart = "PART_ArrowDecorator";

    /// <summary>
    /// BorderThickness 非默认值时置位;内置主题按该状态隐藏浮动箭头,
    /// 因为当前内置视觉不支持箭头与边框的融合呈现。
    /// </summary>
    public const string BorderedPseudoClass = ":bordered";

    protected AbstractArrowDecoratedBox()
    {
        PseudoClasses.Set(BorderedPseudoClass, BorderThickness != default);
    }

    #region 公共属性定义

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, bool>(nameof(IsArrowVisible), true);

    public static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, ArrowPosition>(
            nameof(ArrowPosition));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractArrowDecoratedBox>();

    /// <summary>
    /// 是否显示指示箭头
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// 箭头渲染的位置
    /// </summary>
    public ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ArrowSizeProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowSize));

    internal static readonly StyledProperty<Direction> ArrowDirectionProperty = 
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, Direction>(nameof(ArrowDirection));
    
    internal static readonly StyledProperty<double> ArrowOpacityProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowOpacity), 1.0);
    
    internal static readonly DirectProperty<AbstractArrowDecoratedBox, Rect> ArrowIndicatorBoundsProperty =
        AvaloniaProperty.RegisterDirect<AbstractArrowDecoratedBox, Rect>(
            nameof(ArrowIndicatorBounds),
            o => o.ArrowIndicatorBounds,
            (o, v) => o.ArrowIndicatorBounds = v);
    
    internal static readonly DirectProperty<AbstractArrowDecoratedBox, Rect> ArrowIndicatorLayoutBoundsProperty =
        AvaloniaProperty.RegisterDirect<AbstractArrowDecoratedBox, Rect>(
            nameof(ArrowIndicatorLayoutBounds),
            o => o.ArrowIndicatorLayoutBounds,
            (o, v) => o.ArrowIndicatorLayoutBounds = v);
    
    internal double ArrowSize
    {
        get => GetValue(ArrowSizeProperty);
        set => SetValue(ArrowSizeProperty, value);
    }

    internal Direction ArrowDirection
    {
        get => GetValue(ArrowDirectionProperty);
        set => SetValue(ArrowDirectionProperty, value);
    }
    
    internal double ArrowOpacity
    {
        get => GetValue(ArrowOpacityProperty);
        set => SetValue(ArrowOpacityProperty, value);
    }
    
    private Rect _arrowIndicatorBounds;

    internal Rect ArrowIndicatorBounds
    {
        get => _arrowIndicatorBounds;
        set => SetAndRaise(ArrowIndicatorBoundsProperty, ref _arrowIndicatorBounds, value);
    }
    
    private Rect _arrowIndicatorLayoutBounds;

    internal Rect ArrowIndicatorLayoutBounds
    {
        get => _arrowIndicatorLayoutBounds;
        set => SetAndRaise(ArrowIndicatorLayoutBoundsProperty, ref _arrowIndicatorLayoutBounds, value);
    }

    #endregion

    // 指针最顶点位置
    // 相对坐标
    internal (double, double) ArrowVertexPoint => GetArrowVertexPoint();
    private Border? _contentDecorator;

    internal Border? ContentDecorator => _contentDecorator;
    private protected Control? ArrowIndicatorLayout;
    // ArrowIndicatorLayoutBounds 相对模板内的箭头定位面板，不是本控件；需要真实位置时按此部件自身做坐标变换
    internal Control? ArrowIndicatorLayoutPart => ArrowIndicatorLayout;
    private protected ArrowIndicator? ArrowIndicator;
    private protected bool ArrowPlacementFlipped;

    static AbstractArrowDecoratedBox()
    {
        AffectsMeasure<AbstractArrowDecoratedBox>(IsArrowVisibleProperty, ArrowDirectionProperty);
        AffectsArrange<AbstractArrowDecoratedBox>(ArrowPositionProperty);
        AffectsRender<AbstractArrowDecoratedBox>(BackgroundProperty, ArrowOpacityProperty);
    }

    public static Direction GetDirection(ArrowPosition arrowPosition)
    {
        return arrowPosition switch
        {
            ArrowPosition.Left => Direction.Left,
            ArrowPosition.LeftEdgeAlignedBottom => Direction.Left,
            ArrowPosition.LeftEdgeAlignedTop => Direction.Left,

            ArrowPosition.Top => Direction.Top,
            ArrowPosition.TopEdgeAlignedLeft => Direction.Top,
            ArrowPosition.TopEdgeAlignedRight => Direction.Top,

            ArrowPosition.Right => Direction.Right,
            ArrowPosition.RightEdgeAlignedBottom => Direction.Right,
            ArrowPosition.RightEdgeAlignedTop => Direction.Right,

            ArrowPosition.Bottom => Direction.Bottom,
            ArrowPosition.BottomEdgeAlignedLeft => Direction.Bottom,
            ArrowPosition.BottomEdgeAlignedRight => Direction.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(arrowPosition), arrowPosition,
                "Invalid value for ArrowPosition")
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderThicknessProperty)
        {
            PseudoClasses.Set(BorderedPseudoClass, BorderThickness != default);
        }

        if (change.Property == ArrowPositionProperty)
        {
            SetArrowDirectionIfChanged(GetDirection(ArrowPosition));
        }
        else if (change.Property == ArrowDirectionProperty)
        {
            // 因为属性更新比布局更新快，我们计算 GetMaskBounds 时候等不及布局更新就要计算坐标了
            var oldDirection = change.GetOldValue<Direction>();
            var newDirection = change.GetNewValue<Direction>();
            if ((oldDirection == Direction.Left && newDirection == Direction.Right) ||
                (oldDirection == Direction.Right && newDirection == Direction.Left) ||
                (oldDirection == Direction.Top && newDirection == Direction.Bottom) ||
                (oldDirection == Direction.Bottom && newDirection == Direction.Top))
            {
                ArrowPlacementFlipped = true;
            }
        }
    }

    public CornerRadius GetMaskCornerRadius()
    {
        // 阴影蒙版圆角必须跟随真正可见的内容装饰器（容器 Border）：语义部件样式可以单独
        // 覆盖容器 Border 的圆角，此时 ArrowDecoratedBox 自身的 CornerRadius 与可见形状
        // 不一致，按可见形状取圆角才能避免弹层角落出现白底空隙。
        return _contentDecorator?.CornerRadius ?? CornerRadius;
    }

    public IBrush? GetMaskBackground()
    {
        return Background;
    }

    public Rect GetMaskBounds()
    {
        Debug.Assert(ArrowIndicatorLayout != null && _contentDecorator != null);
        var targetRect = _contentDecorator.Bounds;
        var arrowSize  = ArrowIndicatorLayout.DesiredSize;
        if (ArrowPlacementFlipped)
        {
            if (ArrowDirection == Direction.Top)
            {
                targetRect = targetRect.WithY(targetRect.Y + arrowSize.Height);
            }
            else if (ArrowDirection == Direction.Bottom)
            {
                targetRect = targetRect.WithY(targetRect.Y - arrowSize.Height);
            }
            else if (ArrowDirection == Direction.Left)
            {
                targetRect = targetRect.WithX(targetRect.X + arrowSize.Width);
            }
            else
            {
                targetRect = targetRect.WithX(targetRect.X - arrowSize.Width);
            }
        }

        return targetRect;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _contentDecorator    = e.NameScope.Get<Border>("PART_ContentDecorator");
        ArrowIndicatorLayout = e.NameScope.Get<Control>("PART_ArrowIndicatorLayout");
        ArrowIndicator       = e.NameScope.Get<ArrowIndicator>("PART_ArrowIndicator");
        SetArrowDirectionIfChanged(GetDirection(ArrowPosition));
    }

    private (double, double) GetArrowVertexPoint()
    {
        if (ArrowIndicatorLayout is null)
        {
            return default;
        }

        if (!ArrowIndicatorLayout.IsMeasureValid)
        {
            LayoutHelper.MeasureChild(this, Size.Infinity, Padding);
        }

        if (!ArrowIndicatorLayout.IsArrangeValid)
        {
            Arrange(new Rect(DesiredSize));
        }
        
        var targetRect  = ArrowIndicatorLayout.Bounds;
        var center      = targetRect.Center;
        var controlSize = Bounds.Size;

        // 计算中点
        var direction = GetDirection(ArrowPosition);
        if (direction == Direction.Left || direction == Direction.Right)
        {
            return (center.Y, controlSize.Height - center.Y);
        }
    
        return (center.X, controlSize.Width - center.X);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (IsArrowVisible)
        {
            ArrangeArrow(finalSize);
        }

        return size;
    }

    private void ArrangeArrow(Size finalSize)
    {
        if (ArrowIndicatorLayout is null)
        {
            return;
        }

        var offsetX  = 0d;
        var offsetY  = 0d;
        var position = ArrowPosition;
        var size     = ArrowIndicatorLayout.DesiredSize;

        var minValue = Math.Min(size.Width, size.Height);
        var maxValue = Math.Max(size.Width, size.Height);
        if (position == ArrowPosition.Left ||
            position == ArrowPosition.LeftEdgeAlignedTop ||
            position == ArrowPosition.LeftEdgeAlignedBottom)
        {
            offsetX = 0.5d;
            if (position == ArrowPosition.Left)
            {
                offsetY = (finalSize.Height - maxValue) / 2;
            }
            else if (position == ArrowPosition.LeftEdgeAlignedTop)
            {
                if (maxValue * 2 > finalSize.Height / 2)
                {
                    offsetY = minValue;
                }
                else
                {
                    offsetY = maxValue;
                }
            }
            else
            {
                if (maxValue * 2 > finalSize.Height / 2)
                {
                    offsetY = finalSize.Height - minValue - maxValue;
                }
                else
                {
                    offsetY = finalSize.Height - maxValue * 2;
                }
            }
        }
        else if (position == ArrowPosition.Top ||
                 position == ArrowPosition.TopEdgeAlignedLeft ||
                 position == ArrowPosition.TopEdgeAlignedRight)
        {
            offsetY = 0.5d;
            if (position == ArrowPosition.TopEdgeAlignedLeft)
            {
                offsetX = maxValue;
            }
            else if (position == ArrowPosition.Top)
            {
                offsetX = (finalSize.Width - maxValue) / 2;
            }
            else
            {
                offsetX = finalSize.Width - maxValue * 2;
            }
        }
        else if (position == ArrowPosition.Right ||
                 position == ArrowPosition.RightEdgeAlignedTop ||
                 position == ArrowPosition.RightEdgeAlignedBottom)
        {
            offsetX = -0.5d;
            if (position == ArrowPosition.Right)
            {
                offsetY = (finalSize.Height - maxValue) / 2;
            }
            else if (position == ArrowPosition.RightEdgeAlignedTop)
            {
                if (maxValue * 2 > finalSize.Height / 2)
                {
                    offsetY = minValue;
                }
                else
                {
                    offsetY = maxValue;
                }
            }
            else
            {
                if (maxValue * 2 > finalSize.Height / 2)
                {
                    offsetY = finalSize.Height - minValue - maxValue;
                }
                else
                {
                    offsetY = finalSize.Height - maxValue * 2;
                }
            }
        }
        else
        {
            offsetY = -0.5d;
            if (position == ArrowPosition.BottomEdgeAlignedLeft)
            {
                offsetX = maxValue;
            }
            else if (position == ArrowPosition.Bottom)
            {
                offsetX = (finalSize.Width - maxValue) / 2;
            }
            else
            {
                offsetX = finalSize.Width - maxValue * 2;
            }
        }

        var adjustedOffset = AdjustArrowOffset(new Point(offsetX, offsetY), finalSize, size);
        ArrowIndicatorLayout.Arrange(new Rect(adjustedOffset, size));
        if (ArrowIndicator != null)
        {
            SetArrowIndicatorBoundsIfChanged(ArrowIndicator.Bounds);
        }

        SetArrowIndicatorLayoutBoundsIfChanged(ArrowIndicatorLayout.Bounds);
        ArrowPlacementFlipped     = false;
    }

    private void SetArrowDirectionIfChanged(Direction direction)
    {
        if (ArrowDirection != direction)
        {
            ArrowDirection = direction;
        }
    }

    private void SetArrowIndicatorBoundsIfChanged(Rect bounds)
    {
        if (ArrowIndicatorBounds != bounds)
        {
            ArrowIndicatorBounds = bounds;
        }
    }

    private void SetArrowIndicatorLayoutBoundsIfChanged(Rect bounds)
    {
        if (ArrowIndicatorLayoutBounds != bounds)
        {
            ArrowIndicatorLayoutBounds = bounds;
        }
    }

    protected virtual Point AdjustArrowOffset(Point offset, Size finalSize, Size arrowSize)
    {
        return offset;
    }

    ArrowPosition IArrowAwareShadowMaskInfoProvider.GetArrowPosition()
    {
        return ArrowPosition;
    }
    
    bool IArrowAwareShadowMaskInfoProvider.IsArrowVisible()
    {
        return IsArrowVisible;
    }

    void IArrowAwareShadowMaskInfoProvider.SetArrowOpacity(double opacity)
    {
        if (Math.Abs(ArrowOpacity - opacity) > 0.0001)
        {
            ArrowOpacity = opacity;
        }
    }

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorBounds()
    {
        return ArrowIndicatorBounds;
    }
    
    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorLayoutBounds()
    {
        return ArrowIndicatorLayoutBounds;
    }

    AbstractArrowDecoratedBox? IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()
    {
        return this;
    }
}
