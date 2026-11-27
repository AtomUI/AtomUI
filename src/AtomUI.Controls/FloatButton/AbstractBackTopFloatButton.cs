using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

using AvaScrollViewer = Avalonia.Controls.ScrollViewer;

/// <summary>
/// Base class of the back-to-top button: a float button that appears once the scroll viewer given by
/// <see cref="Target" /> is scrolled down by <see cref="VisibilityHeight" />, and scrolls it back to the top
/// when clicked. Its default icon is <c>VerticalAlignTopOutlined</c>, and it can draw a ring around the button
/// that shows the scroll progress.
/// </summary>
public abstract class AbstractBackTopFloatButton : AbstractFloatButton
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ToTopDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> ToTopDurationProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, TimeSpan>(nameof(ToTopDuration), TimeSpan.FromMilliseconds(450));

    /// <summary>
    /// Identifies the <see cref="Target" /> property.
    /// </summary>
    public static readonly StyledProperty<AvaScrollViewer?> TargetProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, AvaScrollViewer?>(nameof(Target));

    /// <summary>
    /// Identifies the <see cref="VisibilityHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> VisibilityHeightProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, double>(nameof(VisibilityHeight), 400d);

    /// <summary>
    /// Identifies the <see cref="MotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractBackTopFloatButton>();

    /// <summary>
    /// Identifies the <see cref="IsShowProgress" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowProgressProperty =
        AvaloniaProperty.Register<AbstractBackTopFloatButton, bool>(nameof(IsShowProgress));

    /// <summary>
    /// Gets or sets how long the animated scroll to the top takes when the button is clicked. A click while a
    /// scroll animation is still running restarts it from the current offset. Unset means 450 milliseconds.
    /// </summary>
    public TimeSpan ToTopDuration
    {
        get => GetValue(ToTopDurationProperty);
        set => SetValue(ToTopDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the scroll viewer that the button watches and scrolls to the top. The button becomes visible
    /// once the vertical offset of the viewer reaches <see cref="VisibilityHeight" />. With
    /// <see langword="null" />, the button does not react to scrolling and a click does not scroll.
    /// </summary>
    public AvaScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scroll offset of <see cref="Target" />, in device-independent pixels, at which the
    /// button becomes visible. The button is visible once the offset reaches this value and hidden below it.
    /// Unset means 400.
    /// </summary>
    public double VisibilityHeight
    {
        get => GetValue(VisibilityHeightProperty);
        set => SetValue(VisibilityHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets how long the fade in and fade out of the button take when its visibility changes. The fade
    /// runs only while <see cref="AbstractFloatButton.IsMotionEnabled" /> is <see langword="true" />. The theme of
    /// the derived controls sets the value from the <c>MotionDurationMid</c> token; without it the value is
    /// 200 milliseconds.
    /// </summary>
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a ring is drawn around the edge of the button that shows how far <see cref="Target" />
    /// is scrolled. The ring fills clockwise from the 12 o'clock position as the scroll offset goes from the top
    /// to the bottom of the content. Unset hides the ring.
    /// </summary>
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

    /// <summary>
    /// Whether the back-top button is shown. The button sets it when <see cref="Target" /> is scrolled and when the
    /// button is loaded: it is <see langword="true" /> while the vertical offset of the target has reached
    /// <see cref="VisibilityHeight" />. The theme binds the animated visibility of the root panel to it; until the
    /// button sets the value, the <see langword="true" /> of the theme applies.
    /// </summary>
    internal bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// How far <see cref="Target" /> is scrolled, from 0 at the top to 1 at the bottom; 0 while the target cannot
    /// scroll. The button updates it together with <see cref="IsActive" />, and the theme binds the progress ring of
    /// the button to it.
    /// </summary>
    internal double ScrollProgress
    {
        get => GetValue(ScrollProgressProperty);
        set => SetValue(ScrollProgressProperty, value);
    }

    #endregion

    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Called when the button is attached to a visual tree. It starts listening to the scroll events of
    /// <see cref="Target" />. Overrides must call the base implementation, otherwise the button does not follow
    /// the scroll position.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Target != null)
        {
            Target.ScrollChanged -= HandleScrollChanged;
            Target.ScrollChanged += HandleScrollChanged;
        }
    }

    /// <summary>
    /// Called when the button is detached from its visual tree. It stops listening to the scroll events of
    /// <see cref="Target" /> and cancels a scroll animation that is still running. Overrides must call the base
    /// implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (Target != null)
        {
            Target?.ScrollChanged -= HandleScrollChanged;
        }
        CancelScrollAnimation();
    }

    /// <summary>
    /// Sets <c>VerticalAlignTopOutlined</c> as the default icon when the button is initialized without an icon.
    /// </summary>
    protected override void ConfigureDefaultIcon()
    {
        if (Icon == null)
        {
            SetCurrentValue(IconProperty, new VerticalAlignTopOutlined());
        }
    }

    /// <summary>
    /// Called when the value of a property of the button changes. When <see cref="Target" /> changes, it cancels a
    /// running scroll animation and moves the scroll event subscription to the new viewer. Overrides must call the
    /// base implementation, otherwise the button keeps following the previous viewer.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TargetProperty)
        {
            CancelScrollAnimation();
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
            SetCurrentValue(IsActiveProperty, scrollViewer.Offset.Y >= VisibilityHeight);
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

    /// <summary>
    /// Called when the button is loaded. It updates the visibility and the progress ring from the current scroll
    /// position of <see cref="Target" />. Overrides must call the base implementation, otherwise the button does
    /// not reflect the initial scroll position.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Target != null)
        {
            SetCurrentValue(IsActiveProperty, Target.Offset.Y >= VisibilityHeight);
            UpdateScrollProgress(Target);
        }
    }

    /// <summary>
    /// Called when the button is clicked. While the button is enabled and <see cref="Target" /> is set, it starts
    /// an animation that scrolls <see cref="Target" /> to the top over <see cref="ToTopDuration" />. It then calls
    /// the base implementation, which raises the click event and runs the command. Overrides must call the base
    /// implementation, otherwise the click event and the command are not raised.
    /// </summary>
    protected override void OnClick()
    {
        if (IsEffectivelyEnabled && Target is { } target)
        {
            CancelScrollAnimation();
            var cancellation = new CancellationTokenSource();
            _cancellationTokenSource = cancellation;
            _ = ScrollToTopAsync(target, cancellation);
        }
        base.OnClick();
    }

    private void CancelScrollAnimation()
    {
        var cancellation = _cancellationTokenSource;
        _cancellationTokenSource = null;
        cancellation?.Cancel();
        cancellation?.Dispose();
    }

    private async Task ScrollToTopAsync(AvaScrollViewer target, CancellationTokenSource cancellation)
    {
        var token = cancellation.Token;
        var currentOffset = target.Offset;
        var destination = currentOffset.WithY(0);
        var animation = new Animation
        {
            Easing = new CubicEaseInOut(),
            Duration = ToTopDuration,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Setter(AvaScrollViewer.OffsetProperty, currentOffset) },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters = { new Setter(AvaScrollViewer.OffsetProperty, destination) },
                    Cue = new Cue(1d)
                }
            }
        };
        try
        {
            await animation.RunAsync(target, token);
            // Commit after the animation priority value is released so the scroll viewer keeps the exact end
            // position even when the final render tick passed the last keyframe. Keep the target's Offset binding.
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!token.IsCancellationRequested && ReferenceEquals(Target, target) && target.IsAttachedToVisualTree())
                {
                    target.SetCurrentValue(AvaScrollViewer.OffsetProperty, destination);
                }
            }, DispatcherPriority.Background);
        }
        finally
        {
            if (ReferenceEquals(_cancellationTokenSource, cancellation))
            {
                _cancellationTokenSource = null;
                cancellation.Dispose();
            }
        }
    }
}
