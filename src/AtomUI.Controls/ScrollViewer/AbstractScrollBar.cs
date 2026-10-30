using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

using AvaloniaScrollBar = Avalonia.Controls.Primitives.ScrollBar;

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public abstract class AbstractScrollBar : AvaloniaScrollBar, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractScrollBar>();

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsEffectiveExpandedProperty =
        AvaloniaProperty.Register<AbstractScrollBar, bool>(nameof(IsEffectiveExpanded));
    
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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetOwnerScrollBarDragging(false);
        base.OnDetachedFromVisualTree(e);
    }
    
    protected virtual void UpdateIsExpandedState()
    {
        if (!AllowAutoHide)
        {
            var timer = this.GetTimer();
            timer?.Stop();
            IsEffectiveExpanded = false;
        }
    }

    protected virtual void NotifyThumbDragStarted(VectorEventArgs e)
    {
        SetOwnerScrollBarDragging(true);
    }

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
