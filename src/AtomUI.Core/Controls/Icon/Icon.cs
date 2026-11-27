// Reimplementation reference: https://github.com/irihitech/Irihi.Iconica.IconPark

using System.Diagnostics;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

/// <summary>
/// Base class of the vector icons. A derived icon class supplies the shapes of its glyph through
/// <see cref="DrawingInstructions" />, in the units of <see cref="ViewBox" />, and <see cref="Render" /> scales
/// them to the layout size of the icon. The <see cref="Icon" /> theme sets the size, brushes and stroke of every
/// icon and provides no template. A concrete icon class assigns <see cref="IconTheme" /> and <see cref="ViewBox" />
/// in its constructor and overrides <see cref="DrawingInstructions" /> and <see cref="CreateInstance" />.
/// </summary>
public abstract class Icon : PathIcon, ICustomHitTest
{
    /// <summary>
    /// Routes every derived icon class to the <see cref="Icon" /> theme, so the generated icon classes need no
    /// theme of their own.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(Icon);

    /// <summary>
    /// Identifies the <see cref="LoadingAnimation" /> property.
    /// </summary>
    public static readonly StyledProperty<IconAnimation> LoadingAnimationProperty =
        AvaloniaProperty.Register<Icon, IconAnimation>(
            nameof(LoadingAnimation), IconAnimation.None);
    
    /// <summary>
    /// Identifies the <see cref="StrokeBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StrokeBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(StrokeBrush));
    
    /// <summary>
    /// Identifies the <see cref="FillBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FillBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(FillBrush));
    
    /// <summary>
    /// Identifies the <see cref="SecondaryStrokeBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SecondaryStrokeBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(SecondaryStrokeBrush));
    
    /// <summary>
    /// Identifies the <see cref="SecondaryFillBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SecondaryFillBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(SecondaryFillBrush));
    
    /// <summary>
    /// Identifies the <see cref="FallbackBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FallbackBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(FallbackBrush), defaultValue: Brushes.White);
    
    /// <summary>
    /// Identifies the <see cref="IconTheme" /> property.
    /// </summary>
    public static readonly StyledProperty<IconThemeType> IconThemeProperty =
        AvaloniaProperty.Register<Icon, IconThemeType>(
            nameof(IconTheme), IconThemeType.Filled);
    
    /// <summary>
    /// Identifies the <see cref="StrokeWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<Icon, double>(
            nameof(StrokeWidth), 4);

    /// <summary>
    /// Identifies the <see cref="StrokeLineCap" /> property.
    /// </summary>
    public static readonly StyledProperty<PenLineCap> StrokeLineCapProperty =
        AvaloniaProperty.Register<Icon, PenLineCap>(
            nameof(StrokeLineCap), PenLineCap.Round);

    /// <summary>
    /// Identifies the <see cref="StrokeLineJoin" /> property.
    /// </summary>
    public static readonly StyledProperty<PenLineJoin> StrokeLineJoinProperty =
        AvaloniaProperty.Register<Icon, PenLineJoin>(
            nameof(StrokeLineJoin), PenLineJoin.Round);

    /// <summary>
    /// Identifies the <see cref="LoadingAnimationDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> LoadingAnimationDurationProperty =
        AvaloniaProperty.Register<Icon, TimeSpan>(
            nameof(LoadingAnimationDuration), TimeSpan.FromSeconds(1));
    
    /// <summary>
    /// The brush stored under <see cref="IconBrushType.Stroke" />, which a drawing instruction can select for its
    /// pen or its fill. The icon theme sets it to the <c>ColorIconHover</c> token, or to <c>ColorPrimary</c> for
    /// <see cref="IconThemeType.TwoTone" />. <see langword="null" /> paints nothing.
    /// </summary>
    public IBrush? StrokeBrush
    {
        get => GetValue(StrokeBrushProperty);
        set => SetValue(StrokeBrushProperty, value);
    }
    
    /// <summary>
    /// The brush stored under <see cref="IconBrushType.Fill" />, which a drawing instruction can select for its pen
    /// or its fill. The icon theme sets it to the <c>ColorIconHover</c> token, to <c>ColorInfoBg</c> for
    /// <see cref="IconThemeType.TwoTone" /> and to <c>ColorPrimary</c> for <see cref="IconThemeType.MultiColor" />.
    /// <see langword="null" /> paints nothing.
    /// </summary>
    public IBrush? FillBrush
    {
        get => GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }
    
    /// <summary>
    /// The brush stored under <see cref="IconBrushType.SecondaryStroke" />, which a drawing instruction can select
    /// for its pen or its fill. The icon theme sets it to the <c>SecondaryStrokeColor</c> token.
    /// <see langword="null" /> paints nothing.
    /// </summary>
    public IBrush? SecondaryStrokeBrush
    {
        get => GetValue(SecondaryStrokeBrushProperty);
        set => SetValue(SecondaryStrokeBrushProperty, value);
    }
    
    /// <summary>
    /// The brush stored under <see cref="IconBrushType.SecondaryFill" />, which a drawing instruction can select
    /// for its pen or its fill. The icon theme sets it to the <c>SecondaryFillColor</c> token.
    /// <see langword="null" /> paints nothing.
    /// </summary>
    public IBrush? SecondaryFillBrush
    {
        get => GetValue(SecondaryFillBrushProperty);
        set => SetValue(SecondaryFillBrushProperty, value);
    }
    
    /// <summary>
    /// The brush stored under <see cref="IconBrushType.Fallback" />, which a drawing instruction can select for its
    /// pen or its fill. The icon theme sets it to the <c>FallbackColor</c> token; without a value from the theme it
    /// is <see cref="Brushes.White" />. <see langword="null" /> paints nothing.
    /// </summary>
    public IBrush? FallbackBrush
    {
        get => GetValue(FallbackBrushProperty);
        set => SetValue(FallbackBrushProperty, value);
    }
    
    /// <summary>
    /// The width of the stroke that a drawing instruction takes from the icon, in the units of <see cref="ViewBox" />,
    /// so it scales with the icon. The icon theme sets it to the <c>StrokeWidth</c> token.
    /// </summary>
    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    /// <summary>
    /// The cap at the end of the stroke that a drawing instruction takes from the icon. The icon theme sets it to
    /// the <c>StrokeLineCap</c> token.
    /// </summary>
    public PenLineCap StrokeLineCap
    {
        get => GetValue(StrokeLineCapProperty);
        set => SetValue(StrokeLineCapProperty, value);
    }

    /// <summary>
    /// The join at the corners of the stroke that a drawing instruction takes from the icon. The icon theme sets it
    /// to the <c>StrokeLineJoin</c> token.
    /// </summary>
    public PenLineJoin StrokeLineJoin
    {
        get => GetValue(StrokeLineJoinProperty);
        set => SetValue(StrokeLineJoinProperty, value);
    }
    
    /// <summary>
    /// Names the glyph family of the icon. Each generated icon class assigns it in its constructor, and the default
    /// is <see cref="IconThemeType.Filled" />. The icon theme picks <see cref="StrokeBrush" /> and
    /// <see cref="FillBrush" /> from it: <c>ColorIconHover</c> for both, except that
    /// <see cref="IconThemeType.TwoTone" /> takes <c>ColorPrimary</c> and <c>ColorInfoBg</c>, and
    /// <see cref="IconThemeType.MultiColor" /> takes <c>ColorIconHover</c> and <c>ColorPrimary</c>.
    /// </summary>
    public IconThemeType IconTheme
    {
        get => GetValue(IconThemeProperty);
        set => SetValue(IconThemeProperty, value);
    }

    /// <summary>
    /// The duration of one cycle of <see cref="LoadingAnimation" />. A value shorter than one millisecond is treated
    /// as one millisecond. Changing it on a loaded icon restarts the animation when one is configured.
    /// </summary>
    public TimeSpan LoadingAnimationDuration
    {
        get => GetValue(LoadingAnimationDurationProperty);
        set => SetValue(LoadingAnimationDurationProperty, value);
    }

    /// <summary>
    /// The looping rotation the icon plays as a loading indicator; the default, <see cref="IconAnimation.None" />,
    /// keeps it still. <see cref="IconAnimation.Spin" /> and <see cref="IconAnimation.Pulse" /> turn the icon by one
    /// full cycle per <see cref="LoadingAnimationDuration" />, and only while the icon is loaded and visible together
    /// with all its visual ancestors.
    /// </summary>
    public IconAnimation LoadingAnimation
    {
        get => GetValue(LoadingAnimationProperty);
        set => SetValue(LoadingAnimationProperty, value);
    }

    private const float FULL_ROTATION_RADIANS = (float)(Math.PI * 2);
    private const string ROTATION_PROPERTY    = "RotationAngle";

    private CompositeDisposable? _effectiveVisibilitySubscriptions;

    /// <summary>
    /// The shapes of the glyph in the order <see cref="Render" /> draws them, in the units of <see cref="ViewBox" />.
    /// A derived icon class overrides the getter; the default is empty and paints no glyph.
    /// </summary>
    protected virtual IList<DrawingInstruction> DrawingInstructions { get; } = Array.Empty<DrawingInstruction>();
    /// <summary>
    /// The size of the coordinate system of <see cref="DrawingInstructions" />. <see cref="Render" /> scales it to
    /// the layout size of the icon, with a separate factor for each axis. A derived icon class that supplies shapes
    /// must assign it in its constructor.
    /// </summary>
    protected Rect ViewBox;
    
    /// <summary>
    /// The brushes indexed by <see cref="IconBrushType" /> value; <see cref="FindIconBrush" /> reads them while the
    /// icon renders.
    /// </summary>
    protected readonly IBrush?[] DrawBrushes = new IBrush[5];
    /// <summary>
    /// One pen per <see cref="IconBrushType" /> value, built from the brush of <see cref="DrawBrushes" />,
    /// <see cref="StrokeWidth" />, <see cref="StrokeLineCap" /> and <see cref="StrokeLineJoin" />, and updated when
    /// those change. <see cref="Render" /> does not read them: every drawing instruction builds its own pen.
    /// </summary>
    protected readonly Pen?[] DrawPens = new Pen?[5];

    static Icon()
    {
        AffectsMeasure<Icon>(HeightProperty, WidthProperty);
        AffectsRender<Icon>(
            StrokeBrushProperty,
            FillBrushProperty,
            SecondaryStrokeBrushProperty,
            SecondaryFillBrushProperty,
            FallbackBrushProperty,
            StrokeLineCapProperty,
            StrokeLineJoinProperty,
            StrokeWidthProperty);
    }

    /// <summary>
    /// Fills <see cref="DrawBrushes" /> and <see cref="DrawPens" /> from the current brush and stroke properties.
    /// An override must call the base member, otherwise <see cref="FindIconBrush" /> returns <see langword="null" />
    /// until a brush property changes.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        var strokeIndex          = (int)IconBrushType.Stroke;
        var fillIndex            = (int)IconBrushType.Fill;
        var secondaryStrokeIndex = (int)IconBrushType.SecondaryStroke;
        var secondaryFillIndex   = (int)IconBrushType.SecondaryFill;
        var fallbackIndex        = (int)IconBrushType.Fallback;
        
        var strokeBrush          = ProcessBrush(StrokeBrush);
        var fillBrush            = ProcessBrush(FillBrush);
        var secondaryStrokeBrush = ProcessBrush(SecondaryStrokeBrush);
        var secondaryFillBrush   = ProcessBrush(SecondaryFillBrush);
        var fallbackBrush        = ProcessBrush(FallbackBrush);
        
        DrawBrushes[strokeIndex]          = strokeBrush;
        DrawBrushes[fillIndex]            = fillBrush;
        DrawBrushes[secondaryStrokeIndex] = secondaryStrokeBrush;
        DrawBrushes[secondaryFillIndex]   = secondaryFillBrush;
        DrawBrushes[fallbackIndex]        = fallbackBrush;
        
        DrawPens[strokeIndex]          = new Pen(strokeBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
        DrawPens[fillIndex]            = new Pen(fillBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
        DrawPens[secondaryStrokeIndex] = new Pen(secondaryStrokeBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
        DrawPens[secondaryFillIndex]   = new Pen(secondaryFillBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
        DrawPens[fallbackIndex]        = new Pen(fallbackBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
    }

    /// <summary>
    /// Lets a derived icon class replace a brush before the icon caches it in <see cref="DrawBrushes" /> and in the
    /// pen of <see cref="DrawPens" /> for that brush type. It runs for all five brushes when the icon is initialized,
    /// and again when a brush property changes at a priority numerically greater than
    /// <see cref="BindingPriority.Animation" />; a value set at that priority or a numerically lower one is cached
    /// unchanged.
    /// The default returns the brush unchanged.
    /// </summary>
    protected virtual IBrush? ProcessBrush(IBrush? brush)
    {
        return brush;
    }

    /// <summary>
    /// Keeps <see cref="DrawBrushes" /> and <see cref="DrawPens" /> in step with changes of the brush and stroke
    /// properties. On a loaded icon it also re-evaluates the loading animation when <see cref="LoadingAnimation" />
    /// or <see cref="Visual.IsVisible" /> changes, restarts the animation after a change of
    /// <see cref="LoadingAnimationDuration" /> if one is configured, and recenters a running animation when
    /// <see cref="Visual.Bounds" /> changes. An override must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StrokeBrushProperty)
        {
            HandleBrushChanged(IconBrushType.Stroke, StrokeBrush, change.Priority);
        }
        else if (change.Property == FillBrushProperty)
        {
            HandleBrushChanged(IconBrushType.Fill, FillBrush, change.Priority);
        }
        else if (change.Property == SecondaryStrokeBrushProperty)
        {
            HandleBrushChanged(IconBrushType.SecondaryStroke, SecondaryStrokeBrush, change.Priority);
        }
        else if (change.Property == SecondaryFillBrushProperty)
        {
            HandleBrushChanged(IconBrushType.SecondaryFill, SecondaryFillBrush, change.Priority);
        }
        else if (change.Property == FallbackBrushProperty)
        {
            HandleBrushChanged(IconBrushType.Fallback, FallbackBrush, change.Priority);
        }
        else if (change.Property == StrokeWidthProperty)
        {
            HandleStrokeWidthChanged(StrokeWidth);
        }
        else if (change.Property == StrokeLineCapProperty)
        {
            HandleLineCapChanged(StrokeLineCap);
        }
        else if (change.Property == StrokeLineJoinProperty)
        {
            HandleLineJoinChanged(StrokeLineJoin);
        }

        if (IsLoaded)
        {
            if (change.Property == LoadingAnimationProperty ||
                change.Property == IsVisibleProperty)
            {
                ConfigureLoadingAnimationVisibilityTracking();
            }
            else if (change.Property == LoadingAnimationDurationProperty)
            {
                if (IsLoadingAnimationConfigured())
                {
                    RestartLoadingAnimation();
                }
            }
            else if (change.Property == BoundsProperty)
            {
                if (CanRunLoadingAnimation())
                {
                    UpdateLoadingAnimationCenterPoint();
                }
            }
        }
    }

    private void HandleBrushChanged(IconBrushType brushType, IBrush? brush, BindingPriority priority)
    {
        if (brushType == IconBrushType.None)
        {
            return;
        }
        var brushIndex = (int)brushType;
        if (priority > BindingPriority.Animation)
        {
            DrawBrushes[brushIndex] = ProcessBrush(brush);
        }
        else
        {
            DrawBrushes[brushIndex] = brush;
        }

        DrawPens[brushIndex] = new Pen(DrawBrushes[brushIndex], StrokeWidth, lineCap: StrokeLineCap,
            lineJoin: StrokeLineJoin);
    }

    private void HandleStrokeWidthChanged(double strokeWidth)
    {
        foreach (var pen in DrawPens)
        {
            if (pen != null)
            {
                pen.Thickness = strokeWidth;
            }
        }
    }
    
    private void HandleLineCapChanged(PenLineCap lineCap)
    {
        foreach (var pen in DrawPens)
        {
            if (pen != null)
            {
                pen.LineCap = lineCap;
            }
        }
    }
    
    private void HandleLineJoinChanged(PenLineJoin lineJoin)
    {
        foreach (var pen in DrawPens)
        {
            if (pen != null)
            {
                pen.LineJoin = lineJoin;
            }
        }
    }
    
    private void StartLoadingAnimation()
    {
        if (!CanRunLoadingAnimation())
        {
            return;
        }

        var visual = ElementComposition.GetElementVisual(this);
        if (visual?.Compositor is null)
        {
            return;
        }

        visual.StopAnimation(ROTATION_PROPERTY);
        UpdateLoadingAnimationCenterPoint(visual);

        Easing easing = LoadingAnimation == IconAnimation.Pulse
            ? new PulseEasing()
            : new LinearEasing();
        var rotationAnimation = visual.Compositor.CreateScalarKeyFrameAnimation();
        rotationAnimation.Target            = ROTATION_PROPERTY;
        rotationAnimation.Duration          = GetCompositionAnimationDuration();
        rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
        rotationAnimation.StopBehavior      = AnimationStopBehavior.SetToInitialValue;
        rotationAnimation.InsertKeyFrame(0, 0, easing);
        rotationAnimation.InsertKeyFrame(1, FULL_ROTATION_RADIANS, easing);
        visual.StartAnimation(ROTATION_PROPERTY, rotationAnimation);
        IsLoadingAnimationRunning = true;
    }

    private void StopLoadingAnimation()
    {
        // Icons that never started the rotation keep the default composition state, so skip the
        // composition update that would otherwise run for every static icon on load and detach.
        if (!IsLoadingAnimationRunning)
        {
            return;
        }

        var visual = ElementComposition.GetElementVisual(this);
        visual?.StopAnimation(ROTATION_PROPERTY);
        if (visual is not null)
        {
            visual.RotationAngle = 0;
        }
        IsLoadingAnimationRunning = false;
    }

    /// <summary>
    /// Gets whether the composition rotation animation is currently started on this icon.
    /// </summary>
    internal bool IsLoadingAnimationRunning { get; private set; }

    /// <summary>
    /// Gets whether this icon currently subscribes to visibility changes of its visual chain.
    /// </summary>
    internal bool IsTrackingEffectiveVisibility => _effectiveVisibilitySubscriptions is not null;

    private void RestartLoadingAnimation()
    {
        StopLoadingAnimation();
        StartLoadingAnimation();
    }

    private bool CanRunLoadingAnimation()
    {
        return IsEffectivelyVisible &&
               IsLoaded &&
               IsLoadingAnimationConfigured();
    }

    private void ConfigureLoadingAnimationVisibilityTracking()
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;

        // The rotation can only start once loaded. While the icon itself is hidden no ancestor can make it
        // visible, so the visual chain is watched only for a visible icon; OnPropertyChanged re-runs this
        // method when the icon's own IsVisible changes, after Visual has refreshed IsEffectivelyVisible.
        if (!IsLoaded ||
            !IsVisible ||
            !IsLoadingAnimationConfigured() ||
            this.GetVisualParent() is not { } visualParent)
        {
            StopLoadingAnimation();
            return;
        }

        var subscriptions = new CompositeDisposable();
        _effectiveVisibilitySubscriptions = subscriptions;
        visualParent.TrackEffectiveVisibility(HandleEffectiveVisibilityChanged, subscriptions);
    }

    private void HandleEffectiveVisibilityChanged(bool isEffectivelyVisible)
    {
        if (isEffectivelyVisible)
        {
            StartLoadingAnimation();
        }
        else
        {
            StopLoadingAnimation();
        }
    }

    private bool IsLoadingAnimationConfigured()
    {
        return LoadingAnimation == IconAnimation.Spin ||
               LoadingAnimation == IconAnimation.Pulse;
    }

    private void UpdateLoadingAnimationCenterPoint()
    {
        var visual = ElementComposition.GetElementVisual(this);
        if (visual is not null)
        {
            UpdateLoadingAnimationCenterPoint(visual);
        }
    }

    private void UpdateLoadingAnimationCenterPoint(CompositionVisual visual)
    {
        var size = Bounds.Size;
        visual.CenterPoint = new Vector3D(size.Width / 2, size.Height / 2, 0);
    }

    private TimeSpan GetCompositionAnimationDuration()
    {
        return LoadingAnimationDuration < TimeSpan.FromMilliseconds(1)
            ? TimeSpan.FromMilliseconds(1)
            : LoadingAnimationDuration;
    }

    /// <summary>
    /// Subscribes to the effective visibility of the icon's visual ancestors, so that a configured
    /// <see cref="LoadingAnimation" /> runs only while the icon and its ancestors are all visible. An override must
    /// call the base member.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureLoadingAnimationVisibilityTracking();
    }

    /// <summary>
    /// Disposes the subscriptions to the visibility of the visual ancestors and stops a running loading animation
    /// before the base member is called. An override must call the base member.
    /// </summary>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        StopLoadingAnimation();
        base.OnUnloaded(e);
    }

    /// <summary>
    /// Disposes the subscriptions to the visibility of the visual ancestors and stops a running loading animation
    /// before the base member is called. An override must call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        StopLoadingAnimation();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Paints the whole icon. It fills the bounds with
    /// <see cref="Avalonia.Controls.Primitives.TemplatedControl.Background" /> or a transparent brush, scales
    /// <see cref="DrawingInstructions" /> from <see cref="ViewBox" /> to the layout size of the icon with a separate
    /// factor per axis, draws them in order, and finally paints
    /// <see cref="Avalonia.Controls.Primitives.TemplatedControl.Background" /> above the glyph unless it is fully
    /// transparent.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(Background ?? Brushes.Transparent, Bounds);
        if (DrawingInstructions.Count > 0)
        {
            var       realSize             = DesiredSize.Deflate(Margin);
            var       scale                = new Vector(realSize.Width / ViewBox.Width, realSize.Height / ViewBox.Height);
            var       globalGeometryMatrix = CalculateGlobalGeometryMatrix();
            using var transformState       = context.PushTransform(Matrix.CreateScale(scale));
            foreach (var instruction in DrawingInstructions)
            {
                instruction.Draw(context, in globalGeometryMatrix, this);
            }
        }

        // Icon used to have a template Border bound to Background, which painted the local bounds above the
        // glyph. Keep that output now that the template is gone; a fully transparent fill paints nothing.
        if (Background is { } background && !IsFullyTransparent(background))
        {
            context.FillRectangle(background, new Rect(Bounds.Size));
        }
    }

    private static bool IsFullyTransparent(IBrush brush)
    {
        return MathUtils.IsZero(brush.Opacity) || brush is ISolidColorBrush { Color.A: 0 };
    }

    /// <summary>
    /// Returns the transform that <see cref="Render" /> applies to every shape of the glyph after the shape's own
    /// transform, in the units of <see cref="ViewBox" />. It is read once per render. The default is
    /// <see cref="Matrix.Identity" />; <c>AntDesignIcon</c> overrides it to zoom the glyph to the bounds of its shapes.
    /// </summary>
    protected virtual Matrix CalculateGlobalGeometryMatrix()
    {
        return Matrix.Identity;
    }

    /// <summary>
    /// Creates a new instance of the same icon type in its default state. It takes the place of reflection, so that
    /// code which builds a copy of an icon stays AOT-safe. Nothing in <see cref="Icon" /> calls it.
    /// </summary>
    /// <returns>
    /// A new instance of the same type as this icon.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// The type of this icon does not override this method.
    /// </exception>
    public virtual Icon CreateInstance()
    {
        throw new NotSupportedException(
            $"{GetType().FullName} must override {nameof(CreateInstance)} for AOT-safe cloning.");
    }
    
    /// <summary>
    /// Builds a geometry from every item of <see cref="DrawingInstructions" />, gives each one the item's own
    /// transform, and returns the bounds of the group in the units of <see cref="ViewBox" />. Strokes and
    /// <see cref="CalculateGlobalGeometryMatrix" /> are not included. <see cref="Icon" /> does not call it;
    /// <c>AntDesignIcon</c> uses the result to zoom the glyph to its shapes.
    /// </summary>
    protected Rect CalculateGeometryBounds()
    {
        var group = new GeometryGroup();
        foreach (var instruction in DrawingInstructions)
        {
            if (instruction is RectDrawingInstruction rectDrawingInstruction)
            {
                var rectangleGeometry = new RectangleGeometry(rectDrawingInstruction.Rect,
                    rectDrawingInstruction.RadiusX, rectDrawingInstruction.RadiusY);
                if (rectDrawingInstruction.Transform != null)
                {
                    rectangleGeometry.Transform = new MatrixTransform(rectDrawingInstruction.Transform.Value);
                }

                group.Children.Add(rectangleGeometry);
            }
            else if (instruction is CircleDrawingInstruction circleDrawingInstruction)
            {
                var circleGeometry = new EllipseGeometry
                {
                    Center  = circleDrawingInstruction.Center,
                    RadiusX = circleDrawingInstruction.Radius,
                    RadiusY = circleDrawingInstruction.Radius
                };
                if (circleDrawingInstruction.Transform != null)
                {
                    circleGeometry.Transform = new MatrixTransform(circleDrawingInstruction.Transform.Value);
                }

                group.Children.Add(circleGeometry);
            }
            else if (instruction is EllipseDrawingInstruction ellipseDrawingInstruction)
            {
                var ellipseGeometry = new EllipseGeometry
                {
                    Center  = ellipseDrawingInstruction.Center,
                    RadiusX = ellipseDrawingInstruction.RadiusX,
                    RadiusY = ellipseDrawingInstruction.RadiusY
                };
                if (ellipseDrawingInstruction.Transform != null)
                {
                    ellipseGeometry.Transform = new MatrixTransform(ellipseDrawingInstruction.Transform.Value);
                }

                group.Children.Add(ellipseGeometry);
            }
            else if (instruction is LineDrawingInstruction lineDrawingInstruction)
            {
                var lineGeometry = new LineGeometry()
                {
                    StartPoint = lineDrawingInstruction.StartPoint,
                    EndPoint   = lineDrawingInstruction.EndPoint
                };
                if (lineDrawingInstruction.Transform != null)
                {
                    lineGeometry.Transform = new MatrixTransform(lineDrawingInstruction.Transform.Value);
                }

                group.Children.Add(lineGeometry);
            }
            else if (instruction is PolylineDrawingInstruction polylineDrawingInstruction)
            {
                var polylineGeometry = new PolylineGeometry()
                {
                    Points   = polylineDrawingInstruction.Points,
                    IsFilled = false
                };
                if (polylineDrawingInstruction.Transform != null)
                {
                    polylineGeometry.Transform = new MatrixTransform(polylineDrawingInstruction.Transform.Value);
                }

                group.Children.Add(polylineGeometry);
            }
            else if (instruction is PolygonDrawingInstruction polygonDrawingInstruction)
            {
                var polygonGeometry = new PolylineGeometry()
                {
                    Points   = polygonDrawingInstruction.Points,
                    IsFilled = true
                };
                if (polygonDrawingInstruction.Transform != null)
                {
                    polygonGeometry.Transform = new MatrixTransform(polygonDrawingInstruction.Transform.Value);
                }

                group.Children.Add(polygonGeometry);
            }
            else if (instruction is PathDrawingInstruction pathDrawingInstruction)
            {
                var geometry = pathDrawingInstruction.Data?.Clone();
                if (geometry != null)
                {
                    if (pathDrawingInstruction.Transform != null)
                    {
                        geometry.Transform = new MatrixTransform(pathDrawingInstruction.Transform.Value);
                    }

                    group.Children.Add(geometry);
                }
            }
        }

        return group.Bounds;
    }

    /// <summary>
    /// Hook that a derived icon class can call before it switches <see cref="IconTheme" /> on a live icon. The base
    /// implementation refuses every switch. Nothing in <see cref="Icon" /> calls it.
    /// </summary>
    /// <exception cref="InvalidOleVariantTypeException">
    /// Always, in the base implementation. An override that allows a switch must not call the base member.
    /// </exception>
    protected virtual void ValidateThemeChange()
    {
        throw new InvalidOleVariantTypeException("Icon theme type switching is not supported.");
    }

    /// <summary>
    /// Returns <see langword="true" /> for every point, so the whole icon rectangle is hit-testable, including the
    /// areas where the glyph paints nothing.
    /// </summary>
    /// <param name="point">
    /// The point to test. It is ignored.
    /// </param>
    public bool HitTest(Point point)
    {
        return true;
    }

    /// <summary>
    /// Returns the brush the icon cached for the given brush type; drawing instructions call it while the icon
    /// renders. The result is <see langword="null" /> for <see cref="IconBrushType.None" /> and when the icon has no
    /// brush for the type.
    /// </summary>
    public virtual IBrush? FindIconBrush(IconBrushType brushType)
    {
        if (brushType == IconBrushType.None)
        {
            return null;
        }
        var index = (int)brushType;
        Debug.Assert(index >= 0 && index < DrawBrushes.Length);
        return DrawBrushes[index];
    }
}
