using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
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
    
    internal static readonly StyledProperty<IBrush?> AnimationLayerFillProperty =
        AvaloniaProperty.Register<AbstractSkeleton, IBrush?>(nameof (AnimationLayerFill));
    
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

    internal IBrush? AnimationLayerFill
    {
        get => GetValue(AnimationLayerFillProperty);
        set => SetValue(AnimationLayerFillProperty, value);
    }
    #endregion

    private Animation? _animation;
    private CancellationTokenSource? _cancellationTokenSource;
    private AbstractSkeleton? _followTarget;
    private CompositeDisposable? _followDisposables;
    
    internal bool IsFollowMode => _followTarget != null;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (!IsFollowMode)
            {
                if (change.Property == IsActiveProperty)
                {
                    if (IsActive)
                    {
                        StartActiveAnimation();
                    }
                    else
                    {
                        StopActiveAnimation();
                    }
                }
            }
            
            if (change.Property == LoadingBackgroundStartProperty ||
                change.Property == LoadingBackgroundMiddleProperty ||
                change.Property == LoadingBackgroundEndProperty ||
                change.Property == MotionDurationProperty)
            {
                if (!IsFollowMode)
                {
                    BuildActiveAnimation(true);
                    if (IsActive)
                    {
                        StartActiveAnimation();
                    }
                }
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!IsFollowMode)
        {
            if (IsActive)
            {
                StartActiveAnimation();
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (!IsFollowMode)
        {
            StopActiveAnimation();
        }
    }

    protected void StartActiveAnimation()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        if (_animation != null)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
                await _animation.RunInfiniteAsync(this, _cancellationTokenSource.Token));
        }
    }

    protected void StopActiveAnimation()
    {
        _cancellationTokenSource?.Cancel();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (!IsFollowMode)
        {
            BuildActiveAnimation();
        }
    }

    private void BuildActiveAnimation(bool force = false)
    {
        if (force || _animation is null)
        {
            _cancellationTokenSource?.Cancel();
            _animation = new Animation
            {
                Easing         = MotionEasingCurve ?? new CubicEaseOut(),
                Duration       = MotionDuration,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter(AnimationLayerFillProperty, LoadingBackgroundStart) }, 
                        Cue     = new Cue(0.0d)
                    },
                    new KeyFrame
                    {
                        Setters = { new Setter(AnimationLayerFillProperty, LoadingBackgroundMiddle) }, 
                        Cue     = new Cue(0.8d)
                    },
                    new KeyFrame
                    {
                        Setters = { new Setter(AnimationLayerFillProperty, LoadingBackgroundEnd) }, 
                        Cue     = new Cue(1.0d)
                    }
                }
            };
            _cancellationTokenSource = null;
        }
    }

    internal void Follow(AbstractSkeleton followTarget)
    {
        if (_followTarget != null)
        {
            _followDisposables?.Dispose();
        }
        _cancellationTokenSource?.Dispose();
        _followTarget = followTarget;

        _followDisposables = new  CompositeDisposable(2);
        _followDisposables.Add(BindUtils.RelayBind(followTarget, AnimationLayerFillProperty, this, AnimationLayerFillProperty));
        _followDisposables.Add(BindUtils.RelayBind(followTarget, IsActiveProperty, this, IsActiveProperty));
    }

    internal void UnFollow()
    {
        _followDisposables?.Dispose();
        _followDisposables = null;
        _followTarget      = null;
        BuildActiveAnimation();
        if (IsActive)
        {
            StartActiveAnimation();
        }
    }
}