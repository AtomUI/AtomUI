using System.ComponentModel;
using System.Runtime.CompilerServices;
using AtomUI.Controls.Commons;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

/// <summary>
/// Base of every skeleton. A group of nested skeletons shares one shimmer: only the outermost skeleton of the group,
/// the <i>shimmer host</i>, runs it. Blocks are <see cref="Border"/>s marked with <see cref="IsShimmerBlockProperty"/>;
/// each registers with its host. While the shimmer runs, the host hides the blocks' own backgrounds and a custom
/// composition visual on the host's shimmer layer paints every block with one band on the render thread. Whether it
/// runs is decided by <see cref="MotionGate"/> together with <see cref="IsActive"/>, <see cref="IsMotionEnabled"/> and
/// <see cref="MotionDuration"/>.
/// </summary>
public abstract class AbstractSkeleton : TemplatedControl
{
    /// <summary>Template part the shimmer visual hangs on; a composite skeleton looks it up in its placeholder.</summary>
    internal const string ShimmerLayerPart = "PART_ShimmerLayer";

    /// <summary>Background layer of the element templates; the shimmer layer when no <see cref="ShimmerLayerPart"/>
    /// exists.</summary>
    internal const string ContentLayerPart = "PART_ContentLayer";

    #region 公共属性定义

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<AbstractSkeleton, bool>(nameof(IsActive));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSkeleton>();

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractSkeleton>();

    public static readonly StyledProperty<Easing?> MotionEasingCurveProperty =
        AvaloniaProperty.Register<AbstractSkeleton, Easing?>(nameof(MotionEasingCurve));

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>Usually bound to the <c>EnableMotion</c> token; when false the skeleton rests on its base colour.</summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    public Easing? MotionEasingCurve
    {
        get => GetValue(MotionEasingCurveProperty);
        set => SetValue(MotionEasingCurveProperty, value);
    }

    #endregion

    #region 内部属性定义

    /// <summary>Base colour of the band; the theme sets it to the blocks' background colour. Colour tokens are
    /// published as solid brushes, so the property takes the brush and the scene uses its colour.</summary>
    internal static readonly StyledProperty<IBrush?> ShimmerFromColorProperty =
        AvaloniaProperty.Register<AbstractSkeleton, IBrush?>(nameof(ShimmerFromColor));

    /// <summary>Highlight colour of the band, as a solid brush.</summary>
    internal static readonly StyledProperty<IBrush?> ShimmerToColorProperty =
        AvaloniaProperty.Register<AbstractSkeleton, IBrush?>(nameof(ShimmerToColor));

    internal static readonly StyledProperty<SkeletonShimmerColorTokenResourceKeys?> ShimmerColorTokenResourceKeysProperty =
        AvaloniaProperty.Register<AbstractSkeleton, SkeletonShimmerColorTokenResourceKeys?>(nameof(ShimmerColorTokenResourceKeys));

    /// <summary>
    /// Marks a <see cref="Border"/> as a shimmer block. A marked, attached border registers with its host: the
    /// outermost <see cref="AbstractSkeleton"/> among its visual ancestors, where a composite skeleton counts only
    /// for visuals inside its placeholder. Template infrastructure: public only because the XAML compiler resolves
    /// attached properties through public members, so skeleton templates in other assemblies can mark blocks.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static readonly AttachedProperty<bool> IsShimmerBlockProperty =
        AvaloniaProperty.RegisterAttached<AbstractSkeleton, Border, bool>("IsShimmerBlock");

    internal static readonly DirectProperty<AbstractSkeleton, bool> IsShimmerRunningProperty =
        AvaloniaProperty.RegisterDirect<AbstractSkeleton, bool>(
            nameof(IsShimmerRunning),
            o => o.IsShimmerRunning);

    internal IBrush? ShimmerFromColor
    {
        get => GetValue(ShimmerFromColorProperty);
        set => SetValue(ShimmerFromColorProperty, value);
    }

    internal IBrush? ShimmerToColor
    {
        get => GetValue(ShimmerToColorProperty);
        set => SetValue(ShimmerToColorProperty, value);
    }

    internal SkeletonShimmerColorTokenResourceKeys? ShimmerColorTokenResourceKeys
    {
        get => GetValue(ShimmerColorTokenResourceKeysProperty);
        set => SetValue(ShimmerColorTokenResourceKeysProperty, value);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool GetIsShimmerBlock(Border border) => border.GetValue(IsShimmerBlockProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetIsShimmerBlock(Border border, bool value) => border.SetValue(IsShimmerBlockProperty, value);

    private bool _isShimmerRunning;

    /// <summary>True while this skeleton's shimmer visual is sweeping.</summary>
    internal bool IsShimmerRunning
    {
        get => _isShimmerRunning;
        private set => SetAndRaise(IsShimmerRunningProperty, ref _isShimmerRunning, value);
    }

    #endregion

    internal static readonly Easing DefaultShimmerEasing = new SplineEasing(0.25, 0.1, 0.25, 1.0);

    private static readonly ConditionalWeakTable<Border, AbstractSkeleton> s_blockHosts = new();
    private static readonly EventHandler<VisualTreeAttachmentEventArgs> s_blockAttachedHandler = HandleBlockAttached;
    private static readonly EventHandler<VisualTreeAttachmentEventArgs> s_blockDetachedHandler = HandleBlockDetached;

    private readonly List<ShimmerBlock> _shimmerBlocks = [];
    private readonly List<RoundedRect> _blockScratch = [];
    private MotionGate? _motionGate;
    private Control? _templateShimmerLayer;
    private Control? _shimmerLayer;
    private CompositionCustomVisual? _shimmerVisual;
    private Control? _shimmerVisualLayer;
    private SkeletonShimmerVisualHandler? _shimmerHandler;
    private RoundedRect[] _sceneBlocks = [];
    private Size _sceneLayerSize;
    private SkeletonShimmerScene? _lastScene;
    private OwnerTokenSet? _shimmerColorTokens;
    private bool _isShimmerHost;
    private bool _isTemplateApplied;

    /// <summary>True when this skeleton is attached and no qualifying skeleton ancestor hosts it.</summary>
    internal bool IsShimmerHost => _isShimmerHost;

    /// <summary>Blocks registered with this host.</summary>
    internal int ShimmerBlockCount => _shimmerBlocks.Count;

    /// <summary>Where the shimmer visual hangs: <see cref="ShimmerLayerPart"/>, else <see cref="ContentLayerPart"/>,
    /// else the skeleton itself.</summary>
    internal Control? ShimmerLayer => _shimmerLayer;

    internal CompositionCustomVisual? ShimmerVisual => _shimmerVisual;

    internal SkeletonShimmerVisualHandler? ShimmerHandler => _shimmerHandler;

    internal SkeletonShimmerScene? LastShimmerScene => _lastScene;

    internal int ShimmerVisualCreateCount { get; private set; }

    internal int ShimmerVisualReleaseCount { get; private set; }

    /// <summary>How many times the block geometry was rebuilt; the scene is re-sent without a rebuild when only
    /// colours or timing change.</summary>
    internal int ShimmerSceneBuildCount { get; private set; }

    internal Easing EffectiveShimmerEasing => MotionEasingCurve ?? DefaultShimmerEasing;

    /// <summary>Extra run condition of a skeleton kind; a composite skeleton shimmers only while loading.</summary>
    internal virtual bool CanRunShimmer => true;

    /// <summary>True once the parts the shimmer hangs on exist: the template is applied (and, for a composite that
    /// should show its placeholder, the placeholder is built). Starting earlier would hang the visual on the wrong
    /// layer only to move it again.</summary>
    internal virtual bool IsShimmerReady => _isTemplateApplied;

    static AbstractSkeleton()
    {
        // Desktop skeletons keep animating unless a theme binds this to EnableMotion.
        IsMotionEnabledProperty.OverrideDefaultValue<AbstractSkeleton>(true);
        IsShimmerBlockProperty.Changed.AddClassHandler<Border>(HandleIsShimmerBlockChanged);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsActiveProperty)
        {
            UpdateShimmer();
        }
        else if (change.Property == IsMotionEnabledProperty)
        {
            if (_motionGate is not null)
            {
                _motionGate.IsMotionEnabled = IsMotionEnabled;
            }
            UpdateShimmer();
        }
        else if (change.Property == MotionDurationProperty)
        {
            UpdateShimmer();
            ResendShimmerScene();
        }
        else if (change.Property == ShimmerColorTokenResourceKeysProperty)
        {
            ReleaseShimmerColorTokenSubscription();
            ResendShimmerScene();
        }
        else if (change.Property == MotionEasingCurveProperty ||
                 change.Property == ShimmerFromColorProperty ||
                 change.Property == ShimmerToColorProperty)
        {
            ResendShimmerScene();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isShimmerHost = FindShimmerHost(this) is null;
        if (_isShimmerHost)
        {
            _motionGate = new MotionGate(this, HandleShouldRunChanged)
            {
                IsMotionEnabled = IsMotionEnabled
            };
        }
        UpdateShimmer();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // The layer's composition visual exists only once the whole subtree is attached.
        UpdateShimmer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopShimmer();
        ReleaseShimmerVisual();
        _motionGate?.Dispose();
        _motionGate    = null;
        _isShimmerHost = false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _isTemplateApplied    = true;
        _templateShimmerLayer = e.NameScope.Find<Control>(ShimmerLayerPart) ??
                                e.NameScope.Find<Control>(ContentLayerPart);
        RefreshShimmerLayer();
    }

    /// <summary>Asks the shimmer host of this skeleton to re-evaluate whether the shimmer runs.</summary>
    protected void StartActiveAnimation()
    {
        RequestShimmerUpdate();
    }

    /// <summary>Asks the shimmer host of this skeleton to re-evaluate whether the shimmer runs.</summary>
    protected void StopActiveAnimation()
    {
        RequestShimmerUpdate();
    }

    /// <summary>The shimmer layer this skeleton would use now.</summary>
    internal virtual Control ResolveShimmerLayer() => _templateShimmerLayer ?? this;

    /// <summary>Re-resolves the shimmer layer; a running shimmer moves to the new layer.</summary>
    internal void RefreshShimmerLayer()
    {
        var layer = ResolveShimmerLayer();
        if (!ReferenceEquals(layer, _shimmerLayer))
        {
            StopShimmer();
            ReleaseShimmerVisual();
            _shimmerLayer = layer;
        }
        UpdateShimmer();
    }

    private void RequestShimmerUpdate()
    {
        if (_isShimmerHost)
        {
            UpdateShimmer();
        }
        else if (this.IsAttachedToVisualTree())
        {
            FindShimmerHost(this)?.UpdateShimmer();
        }
    }

    private void HandleShouldRunChanged(bool shouldRun) => UpdateShimmer();

    /// <summary>Starts or stops the shimmer to match the run conditions.</summary>
    internal void UpdateShimmer()
    {
        var shouldRun = _isShimmerHost &&
                        IsActive &&
                        IsMotionEnabled &&
                        MotionDuration > TimeSpan.Zero &&
                        _motionGate is { ShouldRun: true } &&
                        CanRunShimmer &&
                        IsShimmerReady;
        if (shouldRun)
        {
            StartShimmer();
        }
        else
        {
            StopShimmer();
        }
    }

    /// <summary>Stops a running shimmer, restoring the blocks' backgrounds.</summary>
    internal void StopShimmer()
    {
        if (!_isShimmerRunning)
        {
            return;
        }
        LayoutUpdated -= HandleHostLayoutUpdated;
        Send(new SkeletonShimmerStop());
        for (var i = 0; i < _shimmerBlocks.Count; i++)
        {
            _shimmerBlocks[i] = _shimmerBlocks[i].Release();
        }
        ReleaseShimmerColorTokenSubscription();
        IsShimmerRunning = false;
    }

    private void StartShimmer()
    {
        if (_isShimmerRunning)
        {
            return;
        }
        _shimmerLayer ??= ResolveShimmerLayer();
        AttachShimmerVisual(_shimmerLayer);
        if (_shimmerVisual is null)
        {
            // No compositor yet; OnLoaded or the next template or placeholder change retries.
            return;
        }

        for (var i = 0; i < _shimmerBlocks.Count; i++)
        {
            _shimmerBlocks[i] = _shimmerBlocks[i].Suppress();
        }
        IsShimmerRunning = true;
        LayoutUpdated += HandleHostLayoutUpdated;
        RefreshShimmerScene(true);
        Send(new SkeletonShimmerStart());
    }

    // Hangs the shimmer visual on layer, creating it when layer has a compositor. A visual on another layer is taken
    // off first, so a host owns at most one handler.
    private void AttachShimmerVisual(Control layer)
    {
        if (!ReferenceEquals(layer, _shimmerVisualLayer))
        {
            ReleaseShimmerVisual();
        }
        if (_shimmerVisual is not null ||
            ElementComposition.GetElementVisual(layer)?.Compositor is not { } compositor)
        {
            return;
        }

        var handler = new SkeletonShimmerVisualHandler();
        _shimmerVisual      = compositor.CreateCustomVisual(handler);
        _shimmerHandler     = handler;
        _shimmerVisualLayer = layer;
        _sceneBlocks        = [];
        _sceneLayerSize     = default;
        ShimmerVisualCreateCount++;
        ElementComposition.SetElementChildVisual(layer, _shimmerVisual);
    }

    private void ReleaseShimmerVisual()
    {
        if (_shimmerVisual is null)
        {
            return;
        }
        if (_shimmerVisualLayer is not null &&
            ReferenceEquals(ElementComposition.GetElementChildVisual(_shimmerVisualLayer), _shimmerVisual))
        {
            ElementComposition.SetElementChildVisual(_shimmerVisualLayer, null);
        }
        _shimmerVisual      = null;
        _shimmerHandler     = null;
        _shimmerVisualLayer = null;
        _lastScene          = null;
        ShimmerVisualReleaseCount++;
    }

    private void HandleHostLayoutUpdated(object? sender, EventArgs e) => RefreshShimmerScene(false);

    // Collects the visible blocks in layer coordinates and sends a new scene only when they or the layer size differ
    // from the last scene. Runs after every layout pass while the shimmer runs; the block buffer is reused, and a new
    // array is made only for a scene that is actually sent.
    private void RefreshShimmerScene(bool force)
    {
        if (_shimmerVisual is null || _shimmerLayer is not { } layer)
        {
            return;
        }
        _blockScratch.Clear();
        for (var i = 0; i < _shimmerBlocks.Count; i++)
        {
            var block = _shimmerBlocks[i].Border;
            if (!block.IsEffectivelyVisible || block.Bounds.Width <= 0 || block.Bounds.Height <= 0 ||
                block.TransformToVisual(layer) is not { } transform)
            {
                continue;
            }
            // The shimmer is drawn above the block, so it fills only the area inside the border: a border set by a
            // custom style keeps its stroke visible while the band moves.
            var thickness = block.BorderThickness;
            var inner     = new Rect(block.Bounds.Size).Deflate(thickness);
            if (inner.Width <= 0 || inner.Height <= 0)
            {
                continue;
            }
            var rect = inner.TransformToAABB(transform);
            _blockScratch.Add(CreateBlockShape(rect, DeflateCornerRadius(block.CornerRadius, thickness)));
        }

        var layerSize = layer.Bounds.Size;
        if (!force && layerSize == _sceneLayerSize && SameBlocks(_sceneBlocks, _blockScratch))
        {
            return;
        }

        _sceneBlocks        = _blockScratch.ToArray();
        _sceneLayerSize     = layerSize;
        _shimmerVisual.Size = new Vector(layerSize.Width, layerSize.Height);
        ShimmerSceneBuildCount++;
        SendScene();
    }

    private void ResendShimmerScene()
    {
        if (_isShimmerRunning)
        {
            SendScene();
        }
    }

    private void SendScene()
    {
        var colors = ReadShimmerColors();
        var scene = new SkeletonShimmerScene(_sceneBlocks, _sceneLayerSize.Width,
            colors.From, colors.To, MotionDuration,
            CreateRenderEasing(EffectiveShimmerEasing));
        _lastScene = scene;
        Send(scene);
    }

    private (Color From, Color To) ReadShimmerColors()
    {
        if (_shimmerColorTokens is null && ShimmerColorTokenResourceKeys is { } keys)
        {
            // Only the active compositor host submits a scene. Nested and inactive skeletons do not observe the palette.
            _shimmerColorTokens = new OwnerTokenSet(this, 2, ResendShimmerScene);
            _shimmerColorTokens.SetResourceKey(0, keys.From);
            _shimmerColorTokens.SetResourceKey(1, keys.To);
        }

        var from = _shimmerColorTokens is null || IsSet(ShimmerFromColorProperty)
            ? ToShimmerColor(ShimmerFromColor)
            : ReadShimmerColor(_shimmerColorTokens.GetValue(0), _lastScene?.From ?? Colors.Transparent);
        var to = _shimmerColorTokens is null || IsSet(ShimmerToColorProperty)
            ? ToShimmerColor(ShimmerToColor)
            : ReadShimmerColor(_shimmerColorTokens.GetValue(1), _lastScene?.To ?? Colors.Transparent);
        return (from, to);
    }

    private static Color ReadShimmerColor(object? value, Color previous) => value switch
    {
        IBrush brush => ToShimmerColor(brush),
        Color color => color,
        string text when Color.TryParse(text, out var color) => color,
        _ when ReferenceEquals(value, BindingOperations.DoNothing) => previous,
        _ => Colors.Transparent
    };

    private void ReleaseShimmerColorTokenSubscription()
    {
        _shimmerColorTokens?.Dispose();
        _shimmerColorTokens = null;
    }

    /// <summary>
    /// The easing the render thread evaluates. A <see cref="SplineEasing"/> keeps mutable state (its key spline caches
    /// parameters and rebuilds lazily, and its control points are settable), so the render thread gets a private copy
    /// per scene instead of an instance the UI thread may also use. Avalonia's other built-in easings hold no state
    /// and pass through. A custom <see cref="Easing"/> passes through as well and must therefore be pure: no state
    /// written by <c>Ease</c> and none changed after it is assigned.
    /// </summary>
    internal static Easing CreateRenderEasing(Easing easing)
    {
        if (easing is SplineEasing spline)
        {
            return new SplineEasing(spline.X1, spline.Y1, spline.X2, spline.Y2);
        }
        return easing;
    }

    // A solid brush's colour with the brush opacity folded into its alpha; any other brush shimmers transparent.
    internal static Color ToShimmerColor(IBrush? brush)
    {
        if (brush is not ISolidColorBrush solid)
        {
            return Colors.Transparent;
        }
        var color = solid.Color;
        return Color.FromArgb((byte)Math.Round(color.A * Math.Clamp(solid.Opacity, 0, 1)), color.R, color.G, color.B);
    }

    private void Send(object message)
    {
        _shimmerVisual?.SendHandlerMessage(message);
    }

    private static bool SameBlocks(RoundedRect[] scene, List<RoundedRect> current)
    {
        if (scene.Length != current.Count)
        {
            return false;
        }
        for (var i = 0; i < scene.Length; i++)
        {
            if (!scene[i].Equals(current[i]))
            {
                return false;
            }
        }
        return true;
    }

    // Corner radii scale down together when adjacent corners would overlap, as a Border draws them.
    // The inner edge's radius at each corner: the outer radius less the larger of the two borders meeting there.
    private static CornerRadius DeflateCornerRadius(CornerRadius radius, Thickness thickness)
    {
        if (thickness == default)
        {
            return radius;
        }
        return new CornerRadius(
            Math.Max(0, radius.TopLeft - Math.Max(thickness.Left, thickness.Top)),
            Math.Max(0, radius.TopRight - Math.Max(thickness.Right, thickness.Top)),
            Math.Max(0, radius.BottomRight - Math.Max(thickness.Right, thickness.Bottom)),
            Math.Max(0, radius.BottomLeft - Math.Max(thickness.Left, thickness.Bottom)));
    }

    private static RoundedRect CreateBlockShape(Rect rect, CornerRadius radius)
    {
        var scale = 1d;
        scale = ShrinkFactor(scale, rect.Width, radius.TopLeft + radius.TopRight);
        scale = ShrinkFactor(scale, rect.Width, radius.BottomLeft + radius.BottomRight);
        scale = ShrinkFactor(scale, rect.Height, radius.TopLeft + radius.BottomLeft);
        scale = ShrinkFactor(scale, rect.Height, radius.TopRight + radius.BottomRight);
        return new RoundedRect(rect,
            radius.TopLeft * scale,
            radius.TopRight * scale,
            radius.BottomRight * scale,
            radius.BottomLeft * scale);
    }

    private static double ShrinkFactor(double scale, double length, double sum)
    {
        return sum > length && sum > 0 ? Math.Min(scale, length / sum) : scale;
    }

    #region 微光块登记

    /// <summary>
    /// The shimmer host for a visual: the outermost qualifying <see cref="AbstractSkeleton"/> among its visual
    /// ancestors, or null. Any skeleton qualifies except a composite one, which qualifies only when the visual lies
    /// inside its placeholder; a composite's content is outside its shimmer.
    /// </summary>
    internal static AbstractSkeleton? FindShimmerHost(Visual visual)
    {
        AbstractSkeleton? host = null;
        AbstractCompositeSkeleton? placeholderOwner = null;
        var current = visual;
        while (current is not null)
        {
            if (current is Control control &&
                control.TemplatedParent is AbstractCompositeSkeleton owner &&
                ReferenceEquals(owner.Placeholder, control))
            {
                placeholderOwner = owner;
            }
            var parent = current.GetVisualParent();
            if (parent is AbstractSkeleton skeleton &&
                (skeleton is not AbstractCompositeSkeleton composite || ReferenceEquals(composite, placeholderOwner)))
            {
                host = skeleton;
            }
            current = parent;
        }
        return host;
    }

    private static void HandleIsShimmerBlockChanged(Border border, AvaloniaPropertyChangedEventArgs e)
    {
        border.AttachedToVisualTree   -= s_blockAttachedHandler;
        border.DetachedFromVisualTree -= s_blockDetachedHandler;
        UnregisterBlock(border);
        if (e.GetNewValue<bool>())
        {
            border.AttachedToVisualTree   += s_blockAttachedHandler;
            border.DetachedFromVisualTree += s_blockDetachedHandler;
            if (border.IsAttachedToVisualTree())
            {
                RegisterBlock(border);
            }
        }
    }

    private static void HandleBlockAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Border border)
        {
            RegisterBlock(border);
        }
    }

    private static void HandleBlockDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Border border)
        {
            UnregisterBlock(border);
        }
    }

    private static void RegisterBlock(Border border)
    {
        UnregisterBlock(border);
        if (FindShimmerHost(border) is not { } host)
        {
            return;
        }
        s_blockHosts.AddOrUpdate(border, host);
        host.AddShimmerBlock(border);
    }

    private static void UnregisterBlock(Border border)
    {
        if (s_blockHosts.TryGetValue(border, out var host))
        {
            s_blockHosts.Remove(border);
            host.RemoveShimmerBlock(border);
        }
    }

    private void AddShimmerBlock(Border border)
    {
        var block = new ShimmerBlock(border, null);
        if (_isShimmerRunning)
        {
            block = block.Suppress();
        }
        _shimmerBlocks.Add(block);
        if (_isShimmerRunning)
        {
            RefreshShimmerScene(false);
        }
    }

    private void RemoveShimmerBlock(Border border)
    {
        for (var i = 0; i < _shimmerBlocks.Count; i++)
        {
            if (ReferenceEquals(_shimmerBlocks[i].Border, border))
            {
                _shimmerBlocks[i].Release();
                _shimmerBlocks.RemoveAt(i);
                break;
            }
        }
        if (_isShimmerRunning)
        {
            RefreshShimmerScene(false);
        }
    }

    // A registered block and, while the shimmer runs, the animation-priority null that hides its own background so
    // the base colour is not painted twice under the band.
    private readonly record struct ShimmerBlock(Border Border, IDisposable? Suppression)
    {
        public ShimmerBlock Suppress()
        {
            if (Suppression is not null)
            {
                return this;
            }
            return this with { Suppression = Border.SetValue(Border.BackgroundProperty, null, BindingPriority.Animation) };
        }

        public ShimmerBlock Release()
        {
            Suppression?.Dispose();
            return this with { Suppression = null };
        }
    }

    #endregion
}
