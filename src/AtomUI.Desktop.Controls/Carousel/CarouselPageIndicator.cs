using AtomUI.Theme.Resources;
using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class CarouselPageIndicator : ContentControl, ISelectable, IControlThemeGlobalOwnerSource
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<CarouselPageIndicator>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CarouselPageIndicator>();

    public static readonly StyledProperty<bool> IsShowTransitionProgressProperty = 
        Carousel.IsShowTransitionProgressProperty.AddOwner<CarouselPageIndicator>();

    public static readonly StyledProperty<TimeSpan> AutoPlaySpeedProperty = 
        Carousel.AutoPlaySpeedProperty.AddOwner<CarouselPageIndicator>();

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> FrameOpacityProperty =
        AvaloniaProperty.Register<CarouselPageIndicator, double>(nameof(FrameOpacity));

    internal static readonly DirectProperty<CarouselPageIndicator, double> ProgressValueProperty =
        AvaloniaProperty.RegisterDirect<CarouselPageIndicator, double>(
            nameof(ProgressValue),
            o => o.ProgressValue,
            (o, v) => o.ProgressValue = v);

    internal static readonly DirectProperty<CarouselPageIndicator, double> EffectiveProgressWidthProperty =
        AvaloniaProperty.RegisterDirect<CarouselPageIndicator, double>(
            nameof(EffectiveProgressWidth),
            o => o.EffectiveProgressWidth,
            (o, v) => o.EffectiveProgressWidth = v);

    #endregion

    private static readonly Easing DefaultProgressEasing = new LinearEasing();

    private Animation? _animation;

    private CancellationTokenSource? _cancellationTokenSource;

    private CompositeDisposable? _effectiveVisibilitySubscriptions;

    private Border? _frame;

    private readonly TransitionInitializationSession _transitionInitialization;

    #region 公共属性定义

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsShowTransitionProgress
    {
        get => GetValue(IsShowTransitionProgressProperty);
        set => SetValue(IsShowTransitionProgressProperty, value);
    }

    public TimeSpan AutoPlaySpeed
    {
        get => GetValue(AutoPlaySpeedProperty);
        set => SetValue(AutoPlaySpeedProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal double FrameOpacity
    {
        get => GetValue(FrameOpacityProperty);
        set => SetValue(FrameOpacityProperty, value);
    }

    private double _progressValue;

    internal double ProgressValue
    {
        get => _progressValue;
        set => SetAndRaise(ProgressValueProperty, ref _progressValue, value);
    }

    private double _effectiveProgressWidth;

    internal double EffectiveProgressWidth
    {
        get => _effectiveProgressWidth;
        set => SetAndRaise(EffectiveProgressWidthProperty, ref _effectiveProgressWidth, value);
    }

    #endregion

    static CarouselPageIndicator()
    {
        SelectableMixin.Attach<CarouselPageIndicator>(IsSelectedProperty);
        PressedMixin.Attach<CarouselPageIndicator>();
        FocusableProperty.OverrideDefaultValue<CarouselPageIndicator>(true);
    }

    public CarouselPageIndicator()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        SetCurrentValue(CornerRadiusProperty, new CornerRadius(e.NewSize.Height));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsShowTransitionProgressProperty)
        {
            BuildProgressAnimation(true);
            ConfigureProgressVisibilityTracking();
        }
        else if (change.Property == AutoPlaySpeedProperty)
        {
            ConfigureProgressAnimation();
            UpdateProgressAnimationState(IsEffectivelyVisible);
        }
        else if (change.Property == IsSelectedProperty)
        {
            ConfigureProgressVisibilityTracking();
        }
        else if (change.Property == ProgressValueProperty)
        {
            ConfigureProgressWidth();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _frame = e.NameScope.Find<Border>("PART_Frame");
        if (IsShowTransitionProgress)
        {
            BuildProgressAnimation(false);
        }
        UpdateProgressAnimationState(IsEffectivelyVisible);
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureProgressVisibilityTracking();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        StopProgressAnimation();
        base.OnDetachedFromVisualTree(e);
    }

    #region 实现 IControlThemeGlobalOwnerSource 接口

    Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => CarouselThemeOwner.Resolve(this);

    #endregion

    private void BuildProgressAnimation(bool force = false)
    {
        if (force || _animation is null)
        {
            StopProgressAnimation();
            _animation = new Animation
            {
                Easing           = DefaultProgressEasing,
                Duration         = AutoPlaySpeed,
                PlaybackBehavior = PlaybackBehavior.OnlyIfVisible,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter(ProgressValueProperty, 0.0) }, 
                        Cue     = new Cue(0.0d)
                    },
                    new KeyFrame
                    {
                        Setters = { new Setter(ProgressValueProperty, 1.0) }, 
                        Cue     = new Cue(1.0d)
                    }
                }
            };
            ConfigureProgressAnimation();
        }
    }

    private void ConfigureProgressAnimation()
    {
        if (_animation != null)
        {
            _animation.Duration = AutoPlaySpeed;
        }
    }


    private void ConfigureProgressVisibilityTracking()
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        StopProgressAnimation();

        if (!IsSelected ||
            !IsShowTransitionProgress ||
            !this.IsAttachedToVisualTree())
        {
            return;
        }

        var subscriptions = new CompositeDisposable();
        _effectiveVisibilitySubscriptions = subscriptions;
        this.TrackEffectiveVisibility(UpdateProgressAnimationState, subscriptions);
    }

    private void UpdateProgressAnimationState(bool isEffectivelyVisible)
    {
        StopProgressAnimation();
        if (!isEffectivelyVisible ||
            !IsSelected ||
            !IsShowTransitionProgress ||
            _animation is null ||
            !this.IsAttachedToVisualTree())
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _animation.RunAsync(this, _cancellationTokenSource.Token);
    }

    private void StopProgressAnimation()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        SetCurrentValue(ProgressValueProperty, 0d);
        ConfigureProgressWidth();
    }

    private void ConfigureProgressWidth()
    {
        var width = IsSelected && IsShowTransitionProgress
            ? (_frame?.Bounds.Width ?? 0.0) * ProgressValue
            : 0.0;
        SetCurrentValue(EffectiveProgressWidthProperty, width);
    }
}
