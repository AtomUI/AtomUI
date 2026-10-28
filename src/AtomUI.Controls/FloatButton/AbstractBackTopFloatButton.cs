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

    private CancellationTokenSource? _cancellationTokenSource;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Target != null)
        {
            Target.ScrollChanged -= HandleScrollChanged;
            Target.ScrollChanged += HandleScrollChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (Target != null)
        {
            Target?.ScrollChanged -= HandleScrollChanged;
        }
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
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
    }

    private void ConfigureTarget(AvaScrollViewer? oldScrollViewer, AvaScrollViewer? newScrollViewer)
    {
        if (oldScrollViewer != null)
        {
            oldScrollViewer.ScrollChanged -= HandleScrollChanged;
        }

        if (newScrollViewer != null)
        {
            newScrollViewer.ScrollChanged += HandleScrollChanged;
        }
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Target != null)
        {
            IsActive = Target.Offset.Y >= VisibilityHeight;
            UpdateScrollProgress(Target);
        }
    }

    protected override void OnClick()
    {
        if (IsEffectivelyEnabled)
        {
            if (Target != null)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                var currentOffset = Target.Offset;
                var animation = new Animation
                {
                    Easing   = new CubicEaseInOut(),
                    Duration = ToTopDuration,
                    Children =
                    {
                        new KeyFrame
                        {
                            Setters = { new Setter(AvaScrollViewer.OffsetProperty, currentOffset) }, Cue = new Cue(0.0d)
                        },
                        new KeyFrame
                        {
                            Setters = { new Setter(AvaScrollViewer.OffsetProperty, new Vector(currentOffset.X, 0)) }, Cue = new Cue(1.0d)
                        }
                    }
                };
                animation.RunAsync(Target, _cancellationTokenSource.Token);
            }
        }
        base.OnClick();
    }
}
