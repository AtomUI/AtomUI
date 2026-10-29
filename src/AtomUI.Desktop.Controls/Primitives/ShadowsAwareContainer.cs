using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Media;
using AtomUI.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class ShadowsAwareContainer : Decorator
{
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<ShadowsAwareContainer>();
    
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner<ShadowsAwareContainer>();

    internal static readonly StyledProperty<IBrush?> SurfaceBackgroundProperty =
        Popup.SurfaceBackgroundProperty.AddOwner<ShadowsAwareContainer>();
    
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<ShadowsAwareContainer>();
    
    public static readonly StyledProperty<double> ArrowSizeProperty =
        ArrowDecoratedBox.ArrowSizeProperty.AddOwner<ShadowsAwareContainer>();
    
    public static readonly StyledProperty<Direction> ArrowDirectionProperty = 
        ArrowDecoratedBox.ArrowDirectionProperty.AddOwner<ShadowsAwareContainer>();
    
    public static readonly StyledProperty<bool> IsOverlayModeProperty = 
        AvaloniaProperty.Register<ShadowsAwareContainer, bool>(nameof (IsOverlayMode), false);
    
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    internal IBrush? SurfaceBackground
    {
        get => GetValue(SurfaceBackgroundProperty);
        set => SetValue(SurfaceBackgroundProperty, value);
    }
    
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }
    
    public double ArrowSize
    {
        get => GetValue(ArrowSizeProperty);
        set => SetValue(ArrowSizeProperty, value);
    }

    public Direction ArrowDirection
    {
        get => GetValue(ArrowDirectionProperty);
        set => SetValue(ArrowDirectionProperty, value);
    }
    
    public bool IsOverlayMode
    {
        get => GetValue(IsOverlayModeProperty);
        set => SetValue(IsOverlayModeProperty, value);
    }

    #region 内部属性定义
    internal static readonly DirectProperty<ShadowsAwareContainer, Rect> ArrowIndicatorLayoutBoundsProperty =
        AvaloniaProperty.RegisterDirect<ShadowsAwareContainer, Rect>(
            nameof(ArrowIndicatorLayoutBounds),
            o => o.ArrowIndicatorLayoutBounds,
            (o, v) => o.ArrowIndicatorLayoutBounds = v);
    
    private Rect _arrowIndicatorLayoutBounds;

    internal Rect ArrowIndicatorLayoutBounds
    {
        get => _arrowIndicatorLayoutBounds;
        set => SetAndRaise(ArrowIndicatorLayoutBoundsProperty, ref _arrowIndicatorLayoutBounds, value);
    }
    #endregion
    
    private PopupFrameRenderer? _frameRenderer;
    private CompositeDisposable? _frameRenderBindings;
    private readonly List<ContentPresenter> _contentPresenterChain = [];
    private CompositeDisposable? _surfaceBindings;
    private AbstractArrowDecoratedBox? _arrowDecoratedBox;
    private bool _isContentSurfaceTrackingActive;

    private bool HasBoxShadow => BoxShadow.Count != 0;
    private bool HasSurfaceBackground => SurfaceBackground is not null;
    
    static ShadowsAwareContainer()
    {
        ClipToBoundsProperty.OverrideDefaultValue<ShadowsAwareContainer>(false);
        ChildProperty.Changed.AddClassHandler<ShadowsAwareContainer>((x, e) => x.ChildChanged(e));
        BoxShadowProperty.Changed.AddClassHandler<ShadowsAwareContainer>((x, _) =>
        {
            if (x.HasBoxShadow)
            {
                x.EnsureFrameRenderer();
            }
        });
        SurfaceBackgroundProperty.Changed.AddClassHandler<ShadowsAwareContainer>((x, _) =>
        {
            if (x.HasSurfaceBackground)
            {
                x.EnsureFrameRenderer();
            }
        });
        AffectsMeasure<ShadowsAwareContainer>(
            BoxShadowProperty,
            ArrowDirectionProperty,
            ArrowSizeProperty,
            IsArrowVisibleProperty,
            IsOverlayModeProperty,
            ArrowIndicatorLayoutBoundsProperty);
        AffectsRender<ShadowsAwareContainer>(CornerRadiusProperty);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _isContentSurfaceTrackingActive = true;
        ConfigureContentSurface();
        var popup = this.FindLogicalAncestorOfType<Popup>();
        if (popup != null)
        {
            _frameRenderBindings?.Dispose();
            _frameRenderBindings = new CompositeDisposable(2)
            {
                BindUtils.RelayBind(popup, Popup.FrameShadowProperty, this, BoxShadowProperty),
                BindUtils.RelayBind(popup, Popup.SurfaceBackgroundProperty, this, SurfaceBackgroundProperty)
            };
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _isContentSurfaceTrackingActive = false;
        ClearContentSurfaceTracking();
        _frameRenderBindings?.Dispose();
        _frameRenderBindings = null;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var shadowThickness = !IsOverlayMode && HasBoxShadow
            ? GetEffectiveShadowThickness(BoxShadow.Thickness())
            : default;
        var size = base.MeasureOverride(availableSize.Deflate(shadowThickness));
        return size.Inflate(shadowThickness);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (!IsOverlayMode)
        {
            var shadowThickness = HasBoxShadow
                ? GetEffectiveShadowThickness(BoxShadow.Thickness())
                : default;
            Child?.Arrange(new Rect(finalSize).Deflate(shadowThickness).Deflate(Padding));
        }
        else
        {
            finalSize = base.ArrangeOverride(finalSize);
        }
        
        if (_frameRenderer != null)
        {
            var shadowBounds = Child?.Bounds ?? default;
            if (IsArrowVisible)
            {
                var effectiveDirection = ArrowDirection;
                if (effectiveDirection == Direction.Top)
                {
                    shadowBounds = shadowBounds.WithY(shadowBounds.Y + ArrowIndicatorLayoutBounds.Height)
                                               .WithHeight(shadowBounds.Height - ArrowIndicatorLayoutBounds.Height);
                }
                else if (effectiveDirection == Direction.Bottom)
                {
                    shadowBounds = shadowBounds.WithHeight(shadowBounds.Height - ArrowIndicatorLayoutBounds.Height);
                }
                else if (effectiveDirection == Direction.Left)
                {
                    shadowBounds = shadowBounds.WithX(shadowBounds.X + ArrowIndicatorLayoutBounds.Width)
                                               .WithWidth(shadowBounds.Width - ArrowIndicatorLayoutBounds.Width);
                }
                else
                {
                    shadowBounds = shadowBounds.WithWidth(shadowBounds.Width - ArrowIndicatorLayoutBounds.Width);
                }
            }
            _frameRenderer.Arrange(shadowBounds);
        }
        return finalSize;
    }

    /// <summary>
    /// 内容形状（frame renderer 绘制的圆角矩形 + 箭头，不含阴影），<paramref name="relativeTo"/> 坐标系下的 DIP 几何；
    /// 无法计算时返回 <c>null</c>。只在无动效变换时使用：圆角按未缩放处理，变换只取外包矩形。
    /// </summary>
    internal PopupContentShape? GetContentShape(Visual relativeTo)
    {
        if (this.TransformToVisual(relativeTo) is not { } transform)
        {
            return null;
        }

        var body = _frameRenderer?.Bounds ?? Child?.Bounds ?? default;
        if (body.Width <= 0 || body.Height <= 0)
        {
            return null;
        }

        var arrowBounds = default(Rect);
        var arrowSide   = PopupArrowSide.None;
        if (_frameRenderer is not null &&
            IsArrowVisible &&
            _arrowDecoratedBox?.TranslatePoint(default, this) is { } arrowOrigin)
        {
            arrowBounds = ArrowIndicatorLayoutBounds.Translate(arrowOrigin).TransformToAABB(transform);
            arrowSide = ArrowDirection switch
            {
                Direction.Left => PopupArrowSide.Left,
                Direction.Top => PopupArrowSide.Top,
                Direction.Right => PopupArrowSide.Right,
                _ => PopupArrowSide.Bottom
            };
        }

        return new PopupContentShape(body.TransformToAABB(transform), CornerRadius, arrowBounds, arrowSide);
    }

    private void ChildChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (HasBoxShadow || HasSurfaceBackground)
        {
            EnsureFrameRenderer();
        }

        if (_isContentSurfaceTrackingActive)
        {
            ConfigureContentSurface();
        }
    }

    private Thickness GetEffectiveShadowThickness(Thickness shadowThickness)
    {
        if (!IsArrowVisible)
        {
            return shadowThickness;
        }

        var left   = shadowThickness.Left;
        var top    = shadowThickness.Top;
        var right  = shadowThickness.Right;
        var bottom = shadowThickness.Bottom;
        
        switch (ArrowDirection)
        {
            case Direction.Left:
                left = Math.Max(left - ArrowIndicatorLayoutBounds.Width, 0);
                break;
            case Direction.Right:
                right = Math.Max(right - ArrowIndicatorLayoutBounds.Width, 0);
                break;
            case Direction.Top:
                top = Math.Max(top - ArrowIndicatorLayoutBounds.Height, 0);
                break;
            case Direction.Bottom:
                bottom = Math.Max(bottom - ArrowIndicatorLayoutBounds.Height, 0);
                break;
        }

        return new Thickness(left, top, right, bottom);
    }

    private void EnsureFrameRenderer()
    {
        if (_frameRenderer != null)
        {
            return;
        }

        _frameRenderer = new PopupFrameRenderer();
        _frameRenderer[!PopupFrameRenderer.BoxShadowProperty]         = this[!BoxShadowProperty];
        _frameRenderer[!PopupFrameRenderer.CornerRadiusProperty]      = this[!CornerRadiusProperty];
        _frameRenderer[!PopupFrameRenderer.SurfaceBackgroundProperty] = this[!SurfaceBackgroundProperty];
        ((ISetLogicalParent)_frameRenderer).SetParent(this);
        VisualChildren.Insert(0, _frameRenderer);
        LogicalChildren.Insert(0, _frameRenderer);
    }

    private void ConfigureContentSurface()
    {
        ClearContentSurfaceTracking();

        Control? surface = Child;
        while (surface is ContentPresenter presenter)
        {
            presenter.PropertyChanged += HandleContentPresenterPropertyChanged;
            _contentPresenterChain.Add(presenter);
            surface = presenter.Child;
        }

        ConfigureShadowsInfo(surface);
    }

    private void ClearContentSurfaceTracking()
    {
        foreach (var presenter in _contentPresenterChain)
        {
            presenter.PropertyChanged -= HandleContentPresenterPropertyChanged;
        }
        _contentPresenterChain.Clear();

        _surfaceBindings?.Dispose();
        _surfaceBindings = null;
        _arrowDecoratedBox = null;
    }

    private void HandleContentPresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            ConfigureContentSurface();
        }
    }

    private void ConfigureShadowsInfo(Control? child)
    {
        _surfaceBindings = new CompositeDisposable();
        if (child is IArrowAwareShadowMaskInfoProvider arrowAwareShadowMaskInfoProvider)
        {
            var arrowDecoratedBox = arrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox();
            if (arrowDecoratedBox is not null)
            {
                _arrowDecoratedBox = arrowDecoratedBox;
                // 强制实例化 ArrowDecoratedBox 自身的模板，保证容器 Border 已就绪；
                // 阴影圆角必须跟随真正可见的内容装饰器（容器 Border）：语义部件样式可能只覆盖
                // 容器 Border 的圆角，此时 ArrowDecoratedBox 自身的 CornerRadius 与可见形状不一致，
                // 直接绑定会导致角落出现白底空隙。
                arrowDecoratedBox.ApplyTemplate();
                if (arrowDecoratedBox.ContentDecorator is { } contentDecorator)
                {
                    _surfaceBindings.Add(BindUtils.RelayBind(
                        contentDecorator, Border.CornerRadiusProperty, this, CornerRadiusProperty));
                }
                else
                {
                    _surfaceBindings.Add(BindUtils.RelayBind(
                        arrowDecoratedBox, ArrowDecoratedBox.CornerRadiusProperty, this, CornerRadiusProperty));
                }
                _surfaceBindings.Add(BindUtils.RelayBind(
                    arrowDecoratedBox, ArrowDecoratedBox.ArrowSizeProperty, this, ArrowSizeProperty));
                _surfaceBindings.Add(BindUtils.RelayBind(
                    arrowDecoratedBox,
                    ArrowDecoratedBox.ArrowIndicatorLayoutBoundsProperty,
                    this,
                    ArrowIndicatorLayoutBoundsProperty));
                _surfaceBindings.Add(BindUtils.RelayBind(
                    arrowDecoratedBox, ArrowDecoratedBox.ArrowDirectionProperty, this, ArrowDirectionProperty));
                _surfaceBindings.Add(BindUtils.RelayBind(
                    arrowDecoratedBox, ArrowDecoratedBox.IsArrowVisibleProperty, this, IsArrowVisibleProperty));
            }
            else
            {
                // 箭头部件尚未就绪（模板未应用或主题未提供 PART_ArrowDecorator）：先按无箭头降级；
                // 模板后续（重新）应用时重新探测，部件就绪即恢复箭头绑定
                SetCurrentValue(IsArrowVisibleProperty, false);
                SetCurrentValue(CornerRadiusProperty, default);
                if (child is TemplatedControl templatedChild)
                {
                    templatedChild.TemplateApplied += HandleSurfaceTemplateApplied;
                    _surfaceBindings.Add(Disposable.Create(() => templatedChild.TemplateApplied -= HandleSurfaceTemplateApplied));
                }
            }
        }
        else if (child is Border bordered)
        {
            SetCurrentValue(IsArrowVisibleProperty, false);
            _surfaceBindings.Add(BindUtils.RelayBind(
                bordered, Border.CornerRadiusProperty, this, CornerRadiusProperty));
        }
        else if (child is TemplatedControl templatedControl)
        {
            SetCurrentValue(IsArrowVisibleProperty, false);
            _surfaceBindings.Add(BindUtils.RelayBind(
                templatedControl, TemplatedControl.CornerRadiusProperty, this, CornerRadiusProperty));
        }
        else
        {
            SetCurrentValue(IsArrowVisibleProperty, false);
            SetCurrentValue(CornerRadiusProperty, default);
        }
    }

    private void HandleSurfaceTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        ConfigureContentSurface();
    }

    private sealed class PopupFrameRenderer : Control
    {
        public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
            AvaloniaProperty.Register<PopupFrameRenderer, BoxShadows>(nameof(BoxShadow));

        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.Register<PopupFrameRenderer, CornerRadius>(nameof(CornerRadius));

        public static readonly StyledProperty<IBrush?> SurfaceBackgroundProperty =
            AvaloniaProperty.Register<PopupFrameRenderer, IBrush?>(nameof(SurfaceBackground));

        public BoxShadows BoxShadow
        {
            get => GetValue(BoxShadowProperty);
            set => SetValue(BoxShadowProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public IBrush? SurfaceBackground
        {
            get => GetValue(SurfaceBackgroundProperty);
            set => SetValue(SurfaceBackgroundProperty, value);
        }

        static PopupFrameRenderer()
        {
            ClipToBoundsProperty.OverrideDefaultValue<PopupFrameRenderer>(false);
            AffectsRender<PopupFrameRenderer>(BoxShadowProperty, CornerRadiusProperty, SurfaceBackgroundProperty);
        }

        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(Bounds.Size);
            if ((BoxShadow.Count == 0 && SurfaceBackground is null) ||
                bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var cornerRadius = CornerRadius;
            var roundedRect = new RoundedRect(
                bounds,
                cornerRadius.TopLeft,
                cornerRadius.TopRight,
                cornerRadius.BottomRight,
                cornerRadius.BottomLeft);
            var fill = SurfaceBackground ?? Brushes.Transparent;
            context.DrawRectangle(fill, null, roundedRect, BoxShadow);
        }
    }
}
