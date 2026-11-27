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
    
    /// <summary>
    /// The border width of the content surface when it draws its own border (a bordered arrow box). While it is not
    /// zero the outline rings of the frame shadow (<c>0 0 0 N</c> entries that stand in for a border) are not painted,
    /// so the surface keeps one outline.
    /// </summary>
    internal static readonly StyledProperty<Thickness> SurfaceBorderThicknessProperty =
        AvaloniaProperty.Register<ShadowsAwareContainer, Thickness>(nameof(SurfaceBorderThickness));

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
    
    internal Thickness SurfaceBorderThickness
    {
        get => GetValue(SurfaceBorderThicknessProperty);
        set => SetValue(SurfaceBorderThicknessProperty, value);
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
            if (_arrowDecoratedBox?.ContentDecorator is { } body &&
                body.TransformToVisual(this) is { } bodyTransform)
            {
                // The presenter can round its bounds wider than the actual body. Using those bounds for the
                // shadow cutout leaves a transparent strip beyond the body's outline.
                shadowBounds = new Rect(body.Bounds.Size).TransformToAABB(bodyTransform);
            }
            else if (IsArrowVisible)
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
    /// Content shape (rounded rectangle and arrow drawn by the frame renderer, without the shadow), as DIP
    /// geometry in the coordinate system of <paramref name="relativeTo" />, or <c>null</c> when it cannot be
    /// computed. Use it only while no motion transform is applied: the corner radius is treated as unscaled, and
    /// the transform contributes only the bounding rectangle.
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
        // ArrowIndicatorLayoutBounds is relative to the arrow positioning panel in the template (for bottom and right
        // arrows its origin differs from the ArrowDecoratedBox origin), so the arrow layout part itself is transformed
        // into the target coordinate system.
        if (_frameRenderer is not null &&
            IsArrowVisible &&
            _arrowDecoratedBox?.ArrowIndicatorLayoutPart is { } arrowLayout &&
            arrowLayout.TransformToVisual(relativeTo) is { } arrowTransform)
        {
            arrowBounds = new Rect(arrowLayout.Bounds.Size).TransformToAABB(arrowTransform);
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
        _frameRenderer[!PopupFrameRenderer.SurfaceBorderThicknessProperty] = this[!SurfaceBorderThicknessProperty];
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

    private void HandleContentDecoratorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty && _frameRenderer is not null)
        {
            InvalidateArrange();
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
                // Force the instantiation of the ArrowDecoratedBox template so that the container Border is ready.
                // The shadow corner radius must follow the content decorator that is actually visible (the container
                // Border): a semantic part style may override only the container Border's corner radius, and then the
                // CornerRadius of the ArrowDecoratedBox itself differs from the visible shape, and binding to it
                // directly leaves gaps that show the white background at the corners.
                arrowDecoratedBox.ApplyTemplate();
                if (arrowDecoratedBox.ContentDecorator is { } contentDecorator)
                {
                    _surfaceBindings.Add(BindUtils.RelayBind(
                        contentDecorator, Border.CornerRadiusProperty, this, CornerRadiusProperty));
                    // The shadow cutout follows the body's bounds, but the body can be re-arranged on its own (a
                    // descendant's arrange pass) while this container keeps its size and is not re-arranged; the
                    // stale cutout then leaves a transparent strip between the border and the shadow.
                    contentDecorator.PropertyChanged += HandleContentDecoratorPropertyChanged;
                    _surfaceBindings.Add(Disposable.Create(() =>
                        contentDecorator.PropertyChanged -= HandleContentDecoratorPropertyChanged));
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
                _surfaceBindings.Add(BindUtils.RelayBind(
                    arrowDecoratedBox, ArrowDecoratedBox.BorderThicknessProperty, this, SurfaceBorderThicknessProperty));
            }
            else
            {
                // The arrow part is not ready yet (the template is not applied, or the theme does not provide
                // PART_ArrowDecorator): degrade to no arrow first. It is probed again when the template is (re)applied
                // later, and the arrow bindings are restored once the part is ready.
                SetCurrentValue(IsArrowVisibleProperty, false);
                SetCurrentValue(CornerRadiusProperty, default);
                SetCurrentValue(SurfaceBorderThicknessProperty, default(Thickness));
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
            SetCurrentValue(SurfaceBorderThicknessProperty, default(Thickness));
            _surfaceBindings.Add(BindUtils.RelayBind(
                bordered, Border.CornerRadiusProperty, this, CornerRadiusProperty));
        }
        else if (child is TemplatedControl templatedControl)
        {
            SetCurrentValue(IsArrowVisibleProperty, false);
            SetCurrentValue(SurfaceBorderThicknessProperty, default(Thickness));
            _surfaceBindings.Add(BindUtils.RelayBind(
                templatedControl, TemplatedControl.CornerRadiusProperty, this, CornerRadiusProperty));
        }
        else
        {
            SetCurrentValue(IsArrowVisibleProperty, false);
            SetCurrentValue(CornerRadiusProperty, default);
            SetCurrentValue(SurfaceBorderThicknessProperty, default(Thickness));
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

        public static readonly StyledProperty<Thickness> SurfaceBorderThicknessProperty =
            AvaloniaProperty.Register<PopupFrameRenderer, Thickness>(nameof(SurfaceBorderThickness));

        // The painted shadows: BoxShadow without its outline rings while the surface has its own border; rebuilt only when
        // either changes. Layout keeps using BoxShadow, the value Popup places the surface with.
        private BoxShadows _paintedShadow;

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

        public Thickness SurfaceBorderThickness
        {
            get => GetValue(SurfaceBorderThicknessProperty);
            set => SetValue(SurfaceBorderThicknessProperty, value);
        }

        static PopupFrameRenderer()
        {
            ClipToBoundsProperty.OverrideDefaultValue<PopupFrameRenderer>(false);
            AffectsRender<PopupFrameRenderer>(BoxShadowProperty, CornerRadiusProperty, SurfaceBackgroundProperty,
                SurfaceBorderThicknessProperty);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == BoxShadowProperty || change.Property == SurfaceBorderThicknessProperty)
            {
                _paintedShadow = SurfaceBorderThickness != default ? WithoutOutlineRings(BoxShadow) : BoxShadow;
            }
        }

        /// <summary>
        /// Drops the outline rings, the outer shadows with no offset, no blur and a positive spread, which stand in
        /// for a border on surfaces that have none.
        /// </summary>
        internal static BoxShadows WithoutOutlineRings(BoxShadows shadows)
        {
            var kept = new List<BoxShadow>(shadows.Count);
            foreach (var shadow in shadows)
            {
                var isRing = !shadow.IsInset && shadow.OffsetX == 0 && shadow.OffsetY == 0 && shadow.Blur == 0 &&
                             shadow.Spread > 0;
                if (!isRing)
                {
                    kept.Add(shadow);
                }
            }

            if (kept.Count == shadows.Count)
            {
                return shadows;
            }

            return kept.Count switch
            {
                0 => default,
                1 => new BoxShadows(kept[0]),
                _ => new BoxShadows(kept[0], kept.GetRange(1, kept.Count - 1).ToArray())
            };
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
            context.DrawRectangle(fill, null, roundedRect, _paintedShadow);
        }
    }
}
