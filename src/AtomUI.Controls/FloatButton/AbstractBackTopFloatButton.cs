using System.Reactive.Linq;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace AtomUI.Controls.Commons;

using AvaScrollViewer = Avalonia.Controls.ScrollViewer;

public abstract class AbstractBackTopFloatButton : AbstractFloatButton
{
    #region 公共属性定义

    public static readonly StyledProperty<TimeSpan> ToTopDurationProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, TimeSpan>(nameof(ToTopDuration), TimeSpan.FromMilliseconds(450));

    public static readonly StyledProperty<AvaScrollViewer?> TargetProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, AvaScrollViewer?>(nameof(Target));

    public static readonly StyledProperty<double> VisibilityHeightProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, double>(nameof(VisibilityHeight), 400d);

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractBackTopFloatButton>();

    public static readonly StyledProperty<bool> IsShowProgressProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, bool>(nameof(IsShowProgress));

    public TimeSpan ToTopDuration
    {
        get => GetValue(ToTopDurationProperty);
        set => SetValue(ToTopDurationProperty, value);
    }

    public AvaScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public double VisibilityHeight
    {
        get => GetValue(VisibilityHeightProperty);
        set => SetValue(VisibilityHeightProperty, value);
    }

    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    public bool IsShowProgress
    {
        get => GetValue(IsShowProgressProperty);
        set => SetValue(IsShowProgressProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, bool>(nameof(IsActive));

    internal static readonly StyledProperty<double> ScrollProgressProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, double>(nameof(ScrollProgress));

    internal bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    internal double ScrollProgress
    {
        get => GetValue(ScrollProgressProperty);
        set => SetValue(ScrollProgressProperty, value);
    }

    #endregion

    private bool _isAttachedToVisualTree;
    private CancellationTokenSource? _scrollCancellation;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        UpdateTargetState();
        if (Target != null)
        {
            Target.ScrollChanged -= HandleScrollChanged;
            Target.ScrollChanged += HandleScrollChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        CancelScrollMotion();
        base.OnDetachedFromVisualTree(e);
        if (Target != null)
        {
            Target?.ScrollChanged -= HandleScrollChanged;
        }
    }

    protected override void ConfigureDefaultIcon()
    {
        if (Icon == null)
        {
            SetCurrentValue(IconProperty, new VerticalAlignTopOutlined());
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TargetProperty)
        {
            ConfigureTarget(change.OldValue as AvaScrollViewer, change.NewValue as AvaScrollViewer);
        }
        else if (change.Property == VisibilityHeightProperty)
        {
            UpdateTargetState();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateTargetState();
    }

    protected override void OnClick()
    {
        if (IsEffectivelyEnabled && Target != null)
        {
            _ = ScrollToTopAsync(Target);
        }
        base.OnClick();
    }

    private void ConfigureTarget(AvaScrollViewer? oldScrollViewer, AvaScrollViewer? newScrollViewer)
    {
        CancelScrollMotion();
        if (oldScrollViewer != null)
        {
            oldScrollViewer.ScrollChanged -= HandleScrollChanged;
        }

        if (_isAttachedToVisualTree && newScrollViewer != null)
        {
            newScrollViewer.ScrollChanged += HandleScrollChanged;
        }
        UpdateTargetState();
    }

    private void HandleScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is AvaScrollViewer scrollViewer)
        {
            IsActive = scrollViewer.Offset.Y >= VisibilityHeight;
            UpdateScrollProgress(scrollViewer);
        }
    }

    private void UpdateScrollProgress(AvaScrollViewer scrollViewer)
    {
        var maxScroll = Math.Max(scrollViewer.Extent.Height - scrollViewer.Viewport.Height, 0d);
        var progress  = maxScroll > 0d
            ? Math.Clamp(scrollViewer.Offset.Y / maxScroll, 0d, 1d)
            : 0d;
        SetCurrentValue(ScrollProgressProperty, progress);
    }

    private void UpdateTargetState()
    {
        // Without a target, preserve the existing standalone/template visibility contract.
        if (Target != null)
        {
            SetCurrentValue(IsActiveProperty, Target.Offset.Y >= VisibilityHeight);
            UpdateScrollProgress(Target);
        }
    }

    private void CancelScrollMotion()
    {
        _scrollCancellation?.Cancel();
        _scrollCancellation?.Dispose();
        _scrollCancellation = null;
    }

    private async Task ScrollToTopAsync(AvaScrollViewer target)
    {
        CancelScrollMotion();
        var cancellation = new CancellationTokenSource();
        _scrollCancellation = cancellation;
        var token = cancellation.Token;
        var currentOffset = target.Offset;
        var destination = new Vector(currentOffset.X, 0);
        try
        {
            // Offset is a runtime target outside this control's template. Commit only this
            // request's terminal value after releasing Animation priority, preserving caller bindings.
            var animation = new Animation
            {
                Easing = new CubicEaseInOut(),
                Duration = ToTopDuration,
                FillMode = FillMode.None,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter(ScrollOffsetAnimation.ValueProperty, currentOffset) }, Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters = { new Setter(ScrollOffsetAnimation.ValueProperty, destination) }, Cue = new Cue(1d)
                    }
                }
            };
            // Direct Offset animation can leave this two-way scroll target at Animation
            // priority after completion. Animate an owned value, then project to the runtime
            // target with SetCurrentValue so the target's binding and scrolling remain writable.
            var value = new ScrollOffsetAnimation();
            value.SetCurrentValue(ScrollOffsetAnimation.ValueProperty, currentOffset);
            using (value.GetObservable(ScrollOffsetAnimation.ValueProperty).Subscribe(offset =>
                   {
                       if (!token.IsCancellationRequested && ReferenceEquals(_scrollCancellation, cancellation) &&
                           ReferenceEquals(Target, target) && _isAttachedToVisualTree)
                       {
                           target.SetCurrentValue(AvaScrollViewer.OffsetProperty, offset);
                       }
                   }))
            {
                await animation.RunAsync(value, token);
            }
            await Dispatcher.InvokeAsync(() =>
            {
                if (!token.IsCancellationRequested && ReferenceEquals(_scrollCancellation, cancellation) &&
                    ReferenceEquals(Target, target) && _isAttachedToVisualTree)
                {
                    target.SetCurrentValue(AvaScrollViewer.OffsetProperty, destination);
                    UpdateTargetState();
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Target replacement, detach or another click supersedes this request.
        }
        finally
        {
            if (ReferenceEquals(_scrollCancellation, cancellation))
            {
                _scrollCancellation = null;
                cancellation.Dispose();
            }
        }
    }

    private sealed class ScrollOffsetAnimation : Animatable
    {
        #region 内部属性定义

        internal static readonly StyledProperty<Vector> ValueProperty =
            AvaloniaProperty.Register<ScrollOffsetAnimation, Vector>("Value");

        #endregion
    }
}
