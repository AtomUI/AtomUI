using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public abstract class AbstractSkeleton : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<AbstractSkeleton, bool>(nameof(IsActive));
    
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractSkeleton>();

    public static readonly StyledProperty<Easing?> MotionEasingCurveProperty =
        AvaloniaProperty.Register<AbstractSkeleton, Easing?>(nameof(MotionEasingCurve));
    
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
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
    
    internal static readonly StyledProperty<IBrush?> LoadingBackgroundStartProperty =
        AvaloniaProperty.Register<AbstractSkeleton, IBrush?>(nameof(LoadingBackgroundStart));
    
    internal static readonly StyledProperty<IBrush?> LoadingBackgroundMiddleProperty =
        AvaloniaProperty.Register<AbstractSkeleton, IBrush?>(nameof(LoadingBackgroundMiddle));
    
    internal static readonly StyledProperty<IBrush?> LoadingBackgroundEndProperty =
        AvaloniaProperty.Register<AbstractSkeleton, IBrush?>(nameof(LoadingBackgroundEnd));

    internal IBrush? LoadingBackgroundStart
    {
        get => GetValue(LoadingBackgroundStartProperty);
        set => SetValue(LoadingBackgroundStartProperty, value);
    }
    
    internal IBrush? LoadingBackgroundMiddle
    {
        get => GetValue(LoadingBackgroundMiddleProperty);
        set => SetValue(LoadingBackgroundMiddleProperty, value);
    }
    
    internal IBrush? LoadingBackgroundEnd
    {
        get => GetValue(LoadingBackgroundEndProperty);
        set => SetValue(LoadingBackgroundEndProperty, value);
    }
    #endregion

    private static readonly Easing DefaultActiveMotionEasing = new SplineEasing
    {
        X1 = 0.25,
        Y1 = 0.1,
        X2 = 0.25,
        Y2 = 1.0
    };
    
    private Animation? _animation;
    private Style? _activeAnimationStyle;
    private Border? _activeAnimationLayer;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsActiveProperty)
        {
            UpdateActiveAnimationState();
        }
        else if (change.Property == LoadingBackgroundStartProperty ||
                 change.Property == LoadingBackgroundMiddleProperty ||
                 change.Property == LoadingBackgroundEndProperty ||
                 change.Property == MotionDurationProperty ||
                 change.Property == MotionEasingCurveProperty)
        {
            RestartActiveAnimation();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateActiveAnimationState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopActiveAnimation();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        StopActiveAnimation();
        _activeAnimationLayer = e.NameScope.Find<Border>("PART_ActiveAnimationLayer");
        UpdateActiveAnimationState();
    }

    protected void StartActiveAnimation()
    {
        if (!ShouldRunActiveAnimation())
        {
            StopActiveAnimation();
            return;
        }

        if (_activeAnimationStyle is not null)
        {
            return;
        }

        _animation ??= BuildActiveAnimation();

        // Avalonia 的 Animation.RunAsync 禁止 IterationCount.Infinite（返回 faulted Task，
        // 未观察时会在 GC 阶段以 TaskScheduler.UnobservedTaskException 爆发）。
        // 单实例持续动画必须由样式动画承载：样式动画原生支持 Infinite，且不创建 Task。
        var animationStyle = new Style(selector => selector.Is<Border>());
        animationStyle.Animations.Add(_animation);
        _activeAnimationLayer!.Styles.Add(animationStyle);
        _activeAnimationStyle = animationStyle;
    }

    protected void StopActiveAnimation()
    {
        if (_activeAnimationStyle is null)
        {
            return;
        }

        _activeAnimationLayer?.Styles.Remove(_activeAnimationStyle);
        _activeAnimationStyle = null;
    }

    private void UpdateActiveAnimationState()
    {
        if (ShouldRunActiveAnimation())
        {
            StartActiveAnimation();
        }
        else
        {
            StopActiveAnimation();
        }
    }

    private void RestartActiveAnimation()
    {
        var shouldRestart = _activeAnimationStyle is not null;
        StopActiveAnimation();
        _animation = null;
        if (shouldRestart)
        {
            StartActiveAnimation();
        }
    }

    private Animation BuildActiveAnimation()
    {
        return new Animation
        {
            Easing         = MotionEasingCurve ?? DefaultActiveMotionEasing,
            Duration       = MotionDuration,
            IterationCount = IterationCount.Infinite,
            PlaybackBehavior = PlaybackBehavior.OnlyIfVisible,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Setter(Border.BackgroundProperty, LoadingBackgroundStart) },
                    Cue     = new Cue(0.0d)
                },
                new KeyFrame
                {
                    Setters = { new Setter(Border.BackgroundProperty, LoadingBackgroundMiddle) },
                    Cue     = new Cue(0.5d)
                },
                new KeyFrame
                {
                    Setters = { new Setter(Border.BackgroundProperty, LoadingBackgroundEnd) },
                    Cue     = new Cue(1.0d)
                }
            }
        };
    }

    private bool ShouldRunActiveAnimation()
    {
        return IsActive &&
               _activeAnimationLayer is not null &&
               MotionDuration > TimeSpan.Zero &&
               this.IsAttachedToVisualTree();
    }
}
