using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base of the loading indicator that <see cref="AbstractSpin" /> shows over its content. It switches between the
/// built-in four-dot indicator and the custom indicator, and runs the animation of the shown one while the control
/// is loaded, visible and <see cref="IsMotionEnabled" /> is true. A derived control supplies the template; in
/// <see cref="IndicatorAnimationMode.Custom" /> mode it also supplies the animation.
/// </summary>
public abstract class AbstractSpinIndicator : TemplatedControl, ICustomizableSizeTypeAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSpinIndicator>();

    /// <summary>
    /// Identifies the <see cref="CustomIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CustomIndicatorProperty =
        AbstractSpin.CustomIndicatorProperty.AddOwner<AbstractSpinIndicator>();

    /// <summary>
    /// Identifies the <see cref="CustomIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CustomIndicatorTemplateProperty =
        AbstractSpin.CustomIndicatorTemplateProperty.AddOwner<AbstractSpinIndicator>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSpinIndicator>();

    /// <summary>
    /// Identifies the <see cref="MotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractSpinIndicator>();

    /// <summary>
    /// Identifies the <see cref="MotionEasingCurve" /> property.
    /// </summary>
    public static readonly StyledProperty<Easing?> MotionEasingCurveProperty =
        AbstractSpin.MotionEasingCurveProperty.AddOwner<AbstractSpinIndicator>();

    /// <summary>
    /// Identifies the <see cref="IndicatorSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorSizeProperty =
        AvaloniaProperty.Register<AbstractSpinIndicator, double>(nameof(IndicatorSize), double.NaN);

    /// <summary>
    /// Identifies the <see cref="DotSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> DotSizeProperty =
        AvaloniaProperty.Register<AbstractSpinIndicator, double>(nameof(DotSize), double.NaN);

    /// <summary>
    /// Identifies the <see cref="DotBgBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> DotBgBrushProperty =
        AvaloniaProperty.Register<AbstractSpinIndicator, IBrush?>(nameof(DotBgBrush));

    /// <summary>
    /// Identifies the <see cref="AnimationMode" /> property.
    /// </summary>
    public static readonly StyledProperty<IndicatorAnimationMode> AnimationModeProperty =
        AvaloniaProperty.Register<AbstractSpinIndicator, IndicatorAnimationMode>(nameof(AnimationMode));

    /// <summary>
    /// Size step of the indicator; the Spin control forwards its own step to it. The SpinIndicator theme takes the
    /// width and height of the indicator area and of its dots from the tokens of that step, and
    /// <see cref="CustomizableSizeType.Custom" /> uses the middle values unless <see cref="IndicatorSize" /> and
    /// <see cref="DotSize" /> are set explicitly. The default is <see cref="CustomizableSizeType.Middle" />.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Content shown in place of the built-in dots, or <see langword="null" /> to show the dots. Use
    /// <see cref="CustomIndicatorTemplate" /> to build the content from a data object. When the content is a
    /// <see cref="PathIcon" />, it gets the width and height of <see cref="IndicatorSize" />. A change restarts the
    /// animation.
    /// </summary>
    [DependsOn(nameof(CustomIndicatorTemplate))]
    public object? CustomIndicator
    {
        get => GetValue(CustomIndicatorProperty);
        set => SetValue(CustomIndicatorProperty, value);
    }

    /// <summary>
    /// Data template that builds the content of <see cref="CustomIndicator" />. It is used only while
    /// <see cref="CustomIndicator" /> is not <see langword="null" />, so a template on its own leaves the built-in dots
    /// shown. A change restarts the animation.
    /// </summary>
    public IDataTemplate? CustomIndicatorTemplate
    {
        get => GetValue(CustomIndicatorTemplateProperty);
        set => SetValue(CustomIndicatorTemplateProperty, value);
    }

    /// <summary>
    /// Switches the indicator animation on or off; while false, the running animation is stopped and the built-in
    /// indicator rests on its initial frame. The default is true, and <see cref="AbstractSpin" /> does not forward its
    /// own motion setting, so following the <c>EnableMotion</c> token needs a theme that binds this property to it.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Duration of one turn of the built-in indicator and of one fade-in of its dots. The SpinIndicator theme binds it
    /// to the <c>IndicatorDuration</c> token. Values below 1 ms are treated as 1 ms, and a change restarts the
    /// animation.
    /// </summary>
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>
    /// Easing of the rotation and of the dot opacity animation of the built-in indicator.
    /// <see langword="null" /> animates linearly, and the SpinIndicator theme sets <see cref="LinearEasing" />. A
    /// change restarts the animation.
    /// </summary>
    public Easing? MotionEasingCurve
    {
        get => GetValue(MotionEasingCurveProperty);
        set => SetValue(MotionEasingCurveProperty, value);
    }

    /// <summary>
    /// Selects who animates the indicator. In <see cref="IndicatorAnimationMode.Rotate" />, the default, the base class
    /// rotates the shown indicator and fades the dots of the built-in one. In
    /// <see cref="IndicatorAnimationMode.Custom" />, a derived control animates through
    /// <see cref="StartCustomAnimation" /> and <see cref="StopCustomAnimation" />, while the base class still decides
    /// when the animation starts and stops. A change restarts the animation.
    /// </summary>
    public IndicatorAnimationMode AnimationMode
    {
        get => GetValue(AnimationModeProperty);
        set => SetValue(AnimationModeProperty, value);
    }

    /// <summary>
    /// Width and height of the indicator area. The built-in dots are laid out in it, and a custom indicator is centered
    /// in it; a custom indicator that is a <see cref="PathIcon" /> also gets it as its own width and height. The
    /// SpinIndicator theme sets it from the token of the <see cref="SizeType" /> step. The registered default is
    /// <see cref="double.NaN" />, so clearing an explicit value returns to that token.
    /// </summary>
    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    /// <summary>
    /// Width and height of each dot of the built-in indicator; it has no effect on a custom indicator. The
    /// SpinIndicator theme sets it from the token of the <see cref="SizeType" /> step. The registered default is
    /// <see cref="double.NaN" />, so clearing an explicit value returns to that token.
    /// </summary>
    public double DotSize
    {
        get => GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }

    /// <summary>
    /// Fill of the four dots of the built-in indicator; it has no effect on a custom indicator. The SpinIndicator theme
    /// binds it to the <c>ColorPrimary</c> token, and <see langword="null" /> leaves the dots unfilled.
    /// </summary>
    public IBrush? DotBgBrush
    {
        get => GetValue(DotBgBrushProperty);
        set => SetValue(DotBgBrushProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractSpinIndicator, bool> IsCustomIndicatorProperty =
        AvaloniaProperty.RegisterDirect<AbstractSpinIndicator, bool>(
            nameof(IsCustomIndicator),
            o => o.IsCustomIndicator,
            (o, v) => o.IsCustomIndicator = v);

    private bool _isCustomIndicator;

    /// <summary>
    /// Whether the indicator shows the custom indicator instead of the built-in dots. The indicator sets it from
    /// <c>CustomIndicator</c>. While <see langword="true" />, the theme hides the dot panel and shows the custom
    /// indicator presenter, and the loading animation targets the presenter instead of the dots.
    /// </summary>
    internal bool IsCustomIndicator
    {
        get => _isCustomIndicator;
        set => SetAndRaise(IsCustomIndicatorProperty, ref _isCustomIndicator, value);
    }

    #endregion

    private const double DOT_START_OPACITY     = 0.3;
    private const float FULL_ROTATION_RADIANS  = (float)(Math.PI * 2);
    private const string ROTATION_PROPERTY     = "RotationAngle";
    private const string OPACITY_PROPERTY      = "Opacity";

    private SpinIndicatorDotPanel? _builtInIndicatorLayout;
    private ContentPresenter? _customIndicatorPresenter;
    private Control? _indicatorLayoutPart;
    private Control? _animatedIndicatorTarget;
    private bool _isCustomAnimationRunning;
    // A start runs derived code (StartCustomAnimation) that may change a property and request a restart or stop
    // before the running state is recorded. Such requests are deferred and replayed once the start is recorded, so
    // every successful start is still paired with exactly one stop.
    private bool _isStartingIndicatorAnimation;
    private bool _isIndicatorAnimationStale;
    private readonly MotionGate _motionGate;

    /// <summary>
    /// True while an animation runs on the active indicator: the built-in one, or the custom one started through
    /// <see cref="StartCustomAnimation" />.
    /// </summary>
    internal bool IsIndicatorAnimationRunning => _animatedIndicatorTarget is not null;

    internal event EventHandler? IndicatorAnimationStarted;

    static AbstractSpinIndicator()
    {
        AffectsMeasure<AbstractSpinIndicator>(SizeTypeProperty,
            CustomIndicatorProperty,
            CustomIndicatorTemplateProperty,
            IndicatorSizeProperty,
            DotSizeProperty);
        // Desktop indicators keep animating unless a theme binds this to EnableMotion.
        IsMotionEnabledProperty.OverrideDefaultValue<AbstractSpinIndicator>(true);
    }

    protected AbstractSpinIndicator()
    {
        _motionGate = new MotionGate(this, HandleShouldRunChanged);
    }

    /// <summary>
    /// Starts the indicator animation when the control is loaded. Overrides must call the base member.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        StartIndicatorAnimation();
    }

    /// <summary>
    /// Stops the indicator animation when the control is detached from the visual tree. Overrides must call the base
    /// member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopIndicatorAnimation();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Stops the running animation, looks up the template parts <c>BuiltInIndicatorLayout</c>,
    /// <c>PART_CustomIndicatorPresenter</c> and <c>PART_IndicatorLayout</c>, and starts the animation again when the
    /// control is already loaded. Overrides must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        StopIndicatorAnimation();
        if (_customIndicatorPresenter is not null)
        {
            _customIndicatorPresenter.PropertyChanged -= HandleIndicatorPresenterPropertyChanged;
        }

        _builtInIndicatorLayout   = null;
        _customIndicatorPresenter = null;
        _indicatorLayoutPart      = null;

        _builtInIndicatorLayout = e.NameScope.Find<SpinIndicatorDotPanel>("BuiltInIndicatorLayout");
        _customIndicatorPresenter = e.NameScope.Find<ContentPresenter>("PART_CustomIndicatorPresenter");
        _indicatorLayoutPart      = e.NameScope.Find<Control>("PART_IndicatorLayout");

        if (_customIndicatorPresenter is not null)
        {
            _customIndicatorPresenter.PropertyChanged += HandleIndicatorPresenterPropertyChanged;
            UpdateCustomIndicatorSize();
        }

        SyncCustomIndicatorState();
        if (IsLoaded)
        {
            StartIndicatorAnimation();
        }
    }

    /// <summary>
    /// Updates the animation after property changes: <see cref="IsMotionEnabled" /> opens or closes the motion gate,
    /// <see cref="IndicatorSize" /> updates the size of a <see cref="PathIcon" /> indicator and the rotation center,
    /// and a change of <see cref="CustomIndicator" />, <see cref="CustomIndicatorTemplate" />,
    /// <see cref="AnimationMode" />, <see cref="MotionDuration" /> or <see cref="MotionEasingCurve" /> restarts the
    /// animation. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsMotionEnabledProperty)
        {
            _motionGate.IsMotionEnabled = IsMotionEnabled;
        }
        else if (change.Property == IndicatorSizeProperty)
        {
            UpdateCustomIndicatorSize();
            UpdateAnimatedTargetCenterPoint();
        }
        else if (change.Property == CustomIndicatorProperty ||
                 change.Property == CustomIndicatorTemplateProperty)
        {
            SyncCustomIndicatorState();
            RestartIndicatorAnimation();
        }
        else if (change.Property == AnimationModeProperty)
        {
            RestartIndicatorAnimation();
        }
        else if (change.Property == MotionDurationProperty ||
                 change.Property == MotionEasingCurveProperty)
        {
            RestartIndicatorAnimation();
        }
    }

    private void SyncCustomIndicatorState()
    {
        SetCurrentValue(IsCustomIndicatorProperty, CustomIndicator is not null);
    }

    private void StartIndicatorAnimation()
    {
        if (_isStartingIndicatorAnimation)
        {
            _isIndicatorAnimationStale = true;
            return;
        }

        if (!_motionGate.ShouldRun || !IsLoaded)
        {
            return;
        }

        var isCustomMode = AnimationMode == IndicatorAnimationMode.Custom;
        var target = isCustomMode
            ? _indicatorLayoutPart ?? GetActiveAnimationTarget()
            : GetActiveAnimationTarget();
        if (target is null)
        {
            return;
        }

        if (ReferenceEquals(_animatedIndicatorTarget, target) && _isCustomAnimationRunning == isCustomMode)
        {
            return;
        }

        StopIndicatorAnimation();
        bool started;
        _isStartingIndicatorAnimation = true;
        _isIndicatorAnimationStale    = false;
        try
        {
            started = isCustomMode ? StartCustomAnimation(target) : StartTargetAnimation(target);
        }
        finally
        {
            _isStartingIndicatorAnimation = false;
        }

        if (started)
        {
            _animatedIndicatorTarget  = target;
            _isCustomAnimationRunning = isCustomMode;
        }

        if (_isIndicatorAnimationStale)
        {
            // Replay what was requested during the start against the recorded state.
            _isIndicatorAnimationStale = false;
            RestartIndicatorAnimation();
            return;
        }

        if (started)
        {
            IndicatorAnimationStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Starts the animation in <see cref="IndicatorAnimationMode.Custom" /> mode. The base class calls it whenever the
    /// animation should run: when the control is loaded, when the motion gate opens, after the template is applied, and
    /// after <see cref="CustomIndicator" />, <see cref="CustomIndicatorTemplate" />,
    /// <see cref="AnimationMode" />, <see cref="MotionDuration" /> or <see cref="MotionEasingCurve" /> changes. The
    /// base implementation starts nothing and returns <see langword="false" />.
    /// </summary>
    /// <param name="target">
    /// The <c>PART_IndicatorLayout</c> part of the template, or, when the template has no such part, the built-in dot
    /// panel or the presenter of the custom indicator.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the animation runs. After <see langword="false" /> the indicator does not count as
    /// animating, so no <see cref="StopCustomAnimation" /> call follows.
    /// </returns>
    protected virtual bool StartCustomAnimation(Visual target)
    {
        return false;
    }

    /// <summary>
    /// Stops the animation that <see cref="StartCustomAnimation" /> started. It is called exactly once for each start
    /// that returned <see langword="true" />: when the motion gate closes, when the control is detached, when the
    /// template is applied, and before the animation restarts. The base implementation does nothing.
    /// </summary>
    /// <param name="target">
    /// The target that was passed to <see cref="StartCustomAnimation" />.
    /// </param>
    protected virtual void StopCustomAnimation(Visual target)
    {
    }

    private void StopIndicatorAnimation()
    {
        if (_isStartingIndicatorAnimation)
        {
            _isIndicatorAnimationStale = true;
            return;
        }

        if (_animatedIndicatorTarget is not null)
        {
            var target = _animatedIndicatorTarget;
            var wasCustom = _isCustomAnimationRunning;
            _animatedIndicatorTarget  = null;
            _isCustomAnimationRunning = false;
            if (wasCustom)
            {
                StopCustomAnimation(target);
            }
            else
            {
                StopTargetAnimation(target);
            }
            return;
        }

        StopTargetAnimation(_builtInIndicatorLayout);
        StopTargetAnimation(_customIndicatorPresenter);
    }

    /// <summary>
    /// Stops the running animation and starts it again, for use by derived controls after a change that affects a
    /// running custom animation, such as its geometry. The animation starts again only while the control is loaded and
    /// the motion gate is open.
    /// </summary>
    protected void RestartIndicatorAnimation()
    {
        StopIndicatorAnimation();
        StartIndicatorAnimation();
    }

    private void HandleShouldRunChanged(bool shouldRun)
    {
        if (shouldRun)
        {
            StartIndicatorAnimation();
        }
        else
        {
            StopIndicatorAnimation();
        }
    }

    private Control? GetActiveAnimationTarget()
    {
        return IsCustomIndicator ? _customIndicatorPresenter : _builtInIndicatorLayout;
    }

    private bool StartTargetAnimation(Control target)
    {
        var visual = ElementComposition.GetElementVisual(target);
        if (visual?.Compositor is null)
        {
            return false;
        }

        UpdateTargetCenterPoint(target, visual);
        var easing = MotionEasingCurve ?? new LinearEasing();
        var rotationAnimation = visual.Compositor.CreateScalarKeyFrameAnimation();
        rotationAnimation.Target            = ROTATION_PROPERTY;
        rotationAnimation.Duration          = GetCompositionDuration();
        rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
        rotationAnimation.StopBehavior      = AnimationStopBehavior.SetToInitialValue;
        rotationAnimation.InsertKeyFrame(0, 0, easing);
        rotationAnimation.InsertKeyFrame(1, FULL_ROTATION_RADIANS, easing);
        visual.StartAnimation(ROTATION_PROPERTY, rotationAnimation);

        if (ReferenceEquals(target, _builtInIndicatorLayout))
        {
            StartBuiltInDotOpacityAnimations(visual.Compositor);
        }

        return true;
    }

    private void StopTargetAnimation(Control? target)
    {
        if (target is null)
        {
            return;
        }

        var visual = ElementComposition.GetElementVisual(target);
        visual?.StopAnimation(ROTATION_PROPERTY);
        if (visual is not null)
        {
            visual.RotationAngle = 0;
        }

        if (ReferenceEquals(target, _builtInIndicatorLayout))
        {
            StopBuiltInDotOpacityAnimations();
            ResetBuiltInDotOpacities();
        }
    }

    private void UpdateAnimatedTargetCenterPoint()
    {
        if (_animatedIndicatorTarget is null || _isCustomAnimationRunning)
        {
            return;
        }

        var visual = ElementComposition.GetElementVisual(_animatedIndicatorTarget);
        if (visual is not null)
        {
            UpdateTargetCenterPoint(_animatedIndicatorTarget, visual);
        }
    }

    private void UpdateTargetCenterPoint(Control target, CompositionVisual visual)
    {
        var size = GetAnimationTargetSize(target);
        visual.CenterPoint = new Vector3D(size.Width / 2, size.Height / 2, 0);
    }

    private Size GetAnimationTargetSize(Control target)
    {
        if (ReferenceEquals(target, _builtInIndicatorLayout) && !double.IsNaN(IndicatorSize))
        {
            return new Size(IndicatorSize, IndicatorSize);
        }

        if (ReferenceEquals(target, _customIndicatorPresenter) && !double.IsNaN(IndicatorSize))
        {
            return new Size(IndicatorSize, IndicatorSize);
        }

        return target.Bounds.Size;
    }

    private TimeSpan GetCompositionDuration()
    {
        return MotionDuration < TimeSpan.FromMilliseconds(1)
            ? TimeSpan.FromMilliseconds(1)
            : MotionDuration;
    }

    private void StartBuiltInDotOpacityAnimations(Compositor compositor)
    {
        if (_builtInIndicatorLayout is null)
        {
            return;
        }

        var easing = MotionEasingCurve ?? new LinearEasing();
        var index = 0;
        foreach (var dot in _builtInIndicatorLayout.Children)
        {
            if (index >= 4)
            {
                break;
            }

            var dotVisual = ElementComposition.GetElementVisual(dot);
            if (dotVisual is null)
            {
                index++;
                continue;
            }

            var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.Target            = OPACITY_PROPERTY;
            opacityAnimation.Duration          = GetCompositionDuration();
            opacityAnimation.DelayBehavior     = AnimationDelayBehavior.SetInitialValueBeforeDelay;
            opacityAnimation.DelayTime         = GetDotOpacityDelay(index);
            opacityAnimation.Direction         = PlaybackDirection.Alternate;
            opacityAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            opacityAnimation.StopBehavior      = AnimationStopBehavior.SetToInitialValue;
            opacityAnimation.InsertKeyFrame(0, (float)DOT_START_OPACITY, easing);
            opacityAnimation.InsertKeyFrame(1, 1, easing);

            dotVisual.StartAnimation(OPACITY_PROPERTY, opacityAnimation);
            index++;
        }
    }

    private void StopBuiltInDotOpacityAnimations()
    {
        if (_builtInIndicatorLayout is null)
        {
            return;
        }

        foreach (var dot in _builtInIndicatorLayout.Children)
        {
            ElementComposition.GetElementVisual(dot)?.StopAnimation(OPACITY_PROPERTY);
        }
    }

    private void ResetBuiltInDotOpacities()
    {
        if (_builtInIndicatorLayout is null)
        {
            return;
        }

        var index = 0;
        foreach (var dot in _builtInIndicatorLayout.Children)
        {
            if (index >= 4)
            {
                break;
            }

            dot.SetCurrentValue(OpacityProperty, DOT_START_OPACITY);
            index++;
        }
    }

    private TimeSpan GetDotOpacityDelay(int index)
    {
        return TimeSpan.FromTicks(GetCompositionDuration().Ticks * index / 3);
    }

    private void HandleIndicatorPresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            UpdateCustomIndicatorSize();
        }
    }

    private void UpdateCustomIndicatorSize()
    {
        if (_customIndicatorPresenter?.Child is not PathIcon child)
        {
            return;
        }

        var size = IndicatorSize;
        if (!double.IsNaN(size))
        {
            child.SetValue(WidthProperty, size);
            child.SetValue(HeightProperty, size);
        }
    }
}
