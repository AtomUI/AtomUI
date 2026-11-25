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

public abstract class AbstractSpinIndicator : TemplatedControl, ICustomizableSizeTypeAware
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSpinIndicator>();

    public static readonly StyledProperty<object?> CustomIndicatorProperty =
        AbstractSpin.CustomIndicatorProperty.AddOwner<AbstractSpinIndicator>();

    public static readonly StyledProperty<IDataTemplate?> CustomIndicatorTemplateProperty =
        AbstractSpin.CustomIndicatorTemplateProperty.AddOwner<AbstractSpinIndicator>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSpinIndicator>();

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractSpinIndicator>();

    public static readonly StyledProperty<Easing?> MotionEasingCurveProperty =
        AbstractSpin.MotionEasingCurveProperty.AddOwner<AbstractSpinIndicator>();

    public static readonly StyledProperty<double> IndicatorSizeProperty =
        AvaloniaProperty.Register<AbstractSpinIndicator, double>(nameof(IndicatorSize), double.NaN);

    public static readonly StyledProperty<double> DotSizeProperty =
        AvaloniaProperty.Register<AbstractSpinIndicator, double>(nameof(DotSize), double.NaN);

    /// <summary>
    /// 内置指示器圆点的填充画刷。
    /// </summary>
    public static readonly StyledProperty<IBrush?> DotBgBrushProperty =
        AvaloniaProperty.Register<AbstractSpinIndicator, IBrush?>(nameof(DotBgBrush));

    /// <summary>
    /// 动画模式；默认 <see cref="IndicatorAnimationMode.Rotate"/>，<see cref="IndicatorAnimationMode.Custom"/>
    /// 时由子类通过 <see cref="StartCustomAnimation"/> 与 <see cref="StopCustomAnimation"/> 提供动画。
    /// </summary>
    public static readonly StyledProperty<IndicatorAnimationMode> AnimationModeProperty =
        AvaloniaProperty.Register<AbstractSpinIndicator, IndicatorAnimationMode>(nameof(AnimationMode));

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    [DependsOn(nameof(CustomIndicatorTemplate))]
    public object? CustomIndicator
    {
        get => GetValue(CustomIndicatorProperty);
        set => SetValue(CustomIndicatorProperty, value);
    }

    public IDataTemplate? CustomIndicatorTemplate
    {
        get => GetValue(CustomIndicatorTemplateProperty);
        set => SetValue(CustomIndicatorTemplateProperty, value);
    }

    /// <summary>Usually bound to the <c>EnableMotion</c> token; when false the indicator rests on its initial frame.</summary>
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

    public IndicatorAnimationMode AnimationMode
    {
        get => GetValue(AnimationModeProperty);
        set => SetValue(AnimationModeProperty, value);
    }

    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    public double DotSize
    {
        get => GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }

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

    /// <summary>True while a composition animation runs on the active indicator.</summary>
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        StartIndicatorAnimation();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopIndicatorAnimation();
        base.OnDetachedFromVisualTree(e);
    }

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
    /// <see cref="IndicatorAnimationMode.Custom"/> 模式下在门控放行且已加载时调用；<paramref name="target"/> 为模板部件
    /// <c>PART_IndicatorLayout</c>，找不到时为内置或自定义指示器目标。返回是否已启动；返回 false 时不视为运行中。
    /// </summary>
    protected virtual bool StartCustomAnimation(Visual target)
    {
        return false;
    }

    /// <summary>
    /// 每次成功的 <see cref="StartCustomAnimation"/> 恰好对应一次调用（门控关闭、脱离、模板重应用、模式切换、重启）。
    /// </summary>
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

    /// <summary>几何等变化后由子类调用，先停止再重新开始当前动画。</summary>
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
