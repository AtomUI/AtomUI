using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

using AvaloniaScrollBar = Avalonia.Controls.Primitives.ScrollBar;

/// <summary>
/// Base class of the AtomUI scroll bars. It coerces <see cref="RangeBase.Maximum" /> down to <see
/// cref="RangeBase.Minimum" /> while the scroll range spans at most one physical pixel, copies the expanded state that
/// the theme sets through <c>IsEffectiveExpanded</c> to the <see cref="ScrollBar.IsExpanded" /> property of the base
/// class, and reports thumb drags to the owning <see cref="AbstractScrollViewer" />.
/// </summary>
[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public abstract class AbstractScrollBar : AvaloniaScrollBar, IMotionAwareControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractScrollBar>();

    /// <summary>
    /// Enables the transitions of the thumb that the template places in the track: the background and the thickness
    /// of the thumb then change over the <c>MotionDurationMid</c> token instead of at once. A scrollbar inside a
    /// scroll viewer takes the value of the scroll viewer; when the value is not set, the theme supplies it from
    /// the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsEffectiveExpandedProperty =
        AvaloniaProperty.Register<AbstractScrollBar, bool>(nameof(IsEffectiveExpanded));
    
    /// <summary>
    /// Expanded state that the scrollbar shares with the thumb of its template: the theme binds
    /// <see cref="AbstractScrollBarThumb.IsExpanded" /> of the thumb to it in both directions, and every change is
    /// copied to <see cref="ScrollBar.IsExpanded" />. While <see cref="ScrollBar.AllowAutoHide" /> is off, it is
    /// reported as <see langword="false" />.
    /// </summary>
    internal bool IsEffectiveExpanded
    {
        get => GetValue(IsEffectiveExpandedProperty);
        set => SetValue(IsEffectiveExpandedProperty, value);
    }

    #endregion

    static AbstractScrollBar()
    {
        // A fractional Wayland resize can leave an extent that is less than one
        // physical pixel larger than the viewport.  Avalonia's default Auto
        // visibility check uses Maximum > 0, so that sub-pixel remainder can
        // expose a scrollbar which has no meaningful scroll range.  Coerce the
        // range used by the scrollbar itself while leaving the owner's logical
        // extent and viewport untouched.
        MaximumProperty.OverrideMetadata<AbstractScrollBar>(
            new StyledPropertyMetadata<double>(coerce: CoerceMaximum));

        Thumb.DragStartedEvent.AddClassHandler<AbstractScrollBar>(
            (x, e) => x.NotifyThumbDragStarted(e),
            RoutingStrategies.Bubble);
        Thumb.DragCompletedEvent.AddClassHandler<AbstractScrollBar>(
            (x, e) => x.NotifyThumbDragCompleted(e),
            RoutingStrategies.Bubble);
    }

    /// <summary>
    /// Calls the base method and then coerces <see cref="RangeBase.Maximum" /> again with the layout scale of the
    /// visual tree the scrollbar was attached to. An override must call the base method to keep this.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // The render scale is only available once the scrollbar is attached.
        // Re-coercing here also covers a template that received Maximum before
        // it entered a TopLevel.
        CoerceValue(MaximumProperty);
    }

    private static double CoerceMaximum(AvaloniaObject sender, double value)
    {
        if (!double.IsFinite(value))
        {
            return value;
        }

        var minimum = sender.GetValue(MinimumProperty);
        if (value <= minimum)
        {
            return minimum;
        }

        var scale = LayoutHelper.GetLayoutScale((Layoutable)sender);
        if (scale <= 0 || double.IsNaN(scale) || double.IsInfinity(scale))
        {
            scale = 1;
        }

        // Do not create a scrollbar for a range that cannot expose a complete
        // additional physical pixel.  The one-pixel tolerance is intentional:
        // Wayland fractional client sizes are quantized in physical pixels and
        // otherwise make Auto visibility oscillate around the threshold.
        return (value - minimum) * scale <= 1 + LayoutHelper.LayoutEpsilon
            ? minimum
            : value;
    }
    
    /// <summary>
    /// Answers a change of <see cref="ScrollBar.AllowAutoHide" /> with <see cref="UpdateIsExpandedState" /> and does not pass
    /// that change to the base class, whose own handling sets <see cref="ScrollBar.IsExpanded" /> directly. Every other
    /// change is passed to the base class first; a change of <c>IsEffectiveExpanded</c> is then copied to
    /// <see cref="ScrollBar.IsExpanded" />.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == AllowAutoHideProperty)
        {
            UpdateIsExpandedState();
        }
        else
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsEffectiveExpandedProperty)
            {
                this.SetIsExpanded(IsEffectiveExpanded);
            }
        }
    }

    /// <summary>
    /// Clears the dragging state on the owning <see cref="AbstractScrollViewer" /> before calling the base method,
    /// so that a scrollbar detached during a thumb drag does not leave the owner in the dragging state.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetOwnerScrollBarDragging(false);
        base.OnDetachedFromVisualTree(e);
    }
    
    /// <summary>
    /// Called when <see cref="ScrollBar.AllowAutoHide" /> changes. While auto-hide is off, the default implementation stops
    /// the timer of the base class that delays expanding and collapsing and reports the scrollbar as not expanded
    /// through <c>IsEffectiveExpanded</c>; it does nothing while auto-hide is on. The handling of the base class
    /// for this change is bypassed, so an override that replaces this method must still stop that timer while
    /// auto-hide is off.
    /// </summary>
    protected virtual void UpdateIsExpandedState()
    {
        if (!AllowAutoHide)
        {
            var timer = this.GetTimer();
            timer?.Stop();
            IsEffectiveExpanded = false;
        }
    }

    /// <summary>
    /// Called when a thumb inside the scrollbar starts to be dragged. The default implementation sets the dragging
    /// state of the owning <see cref="AbstractScrollViewer" />, which its theme uses to keep the scroll bars
    /// visible in lite mode. An override that does not call the base method must set that state itself.
    /// </summary>
    /// <param name="e">
    /// The drag event raised by the thumb. The default implementation does not read it.
    /// </param>
    protected virtual void NotifyThumbDragStarted(VectorEventArgs e)
    {
        SetOwnerScrollBarDragging(true);
    }

    /// <summary>
    /// Called when the drag of a thumb inside the scrollbar is completed. The default implementation clears the
    /// dragging state of the owning <see cref="AbstractScrollViewer" />. An override that does not call the base
    /// method must clear that state itself.
    /// </summary>
    /// <param name="e">
    /// The drag event raised by the thumb. The default implementation does not read it.
    /// </param>
    protected virtual void NotifyThumbDragCompleted(VectorEventArgs e)
    {
        SetOwnerScrollBarDragging(false);
    }

    private void SetOwnerScrollBarDragging(bool isDragging)
    {
        if (TemplatedParent is AbstractScrollViewer scrollViewer)
        {
            scrollViewer.IsScrollBarDragging = isDragging;
        }
    }
}
