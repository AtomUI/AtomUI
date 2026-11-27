using Avalonia;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

using AvaScrollViewer = Avalonia.Controls.ScrollViewer;

/// <summary>
/// Base class of hosts that place a back-to-top button in the scoped overlay layer. The properties set on the
/// host are forwarded to the button that it creates.
/// </summary>
public abstract class AbstractBackTopFloatButtonHost : AbstractFloatButtonHost
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="ToTopDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> ToTopDurationProperty =
        AbstractBackTopFloatButton.ToTopDurationProperty.AddOwner<AbstractBackTopFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="Target" /> property.
    /// </summary>
    public static readonly StyledProperty<AvaScrollViewer?> TargetProperty =
        AbstractBackTopFloatButton.TargetProperty.AddOwner<AbstractBackTopFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="VisibilityHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> VisibilityHeightProperty =
        AbstractBackTopFloatButton.VisibilityHeightProperty.AddOwner<AbstractBackTopFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="MotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractBackTopFloatButtonHost>();

    /// <summary>
    /// Identifies the <see cref="IsShowProgress" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowProgressProperty =
        AbstractBackTopFloatButton.IsShowProgressProperty.AddOwner<AbstractBackTopFloatButtonHost>();

    /// <summary>
    /// Gets or sets how long the animated scroll to the top takes when the button is clicked. The value is
    /// forwarded to the button. Unset means 450 milliseconds.
    /// </summary>
    public TimeSpan ToTopDuration
    {
        get => GetValue(ToTopDurationProperty);
        set => SetValue(ToTopDurationProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the scroll viewer that the button watches and scrolls to the top. The value is forwarded to
    /// the button. When the host is attached and <see cref="Target" /> is <see langword="null" />, the nearest
    /// ancestor scroll viewer is used.
    /// </summary>
    public AvaScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the vertical scroll offset of <see cref="Target" />, in device-independent pixels, at which the
    /// button becomes visible. The value is forwarded to the button. Unset means 400.
    /// </summary>
    public double VisibilityHeight
    {
        get => GetValue(VisibilityHeightProperty);
        set => SetValue(VisibilityHeightProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how long the fade in and fade out of the button take when its visibility changes. The value is
    /// forwarded to the button. Unset, the theme of the derived host sets it from the <c>MotionDurationMid</c>
    /// token.
    /// </summary>
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a ring that shows the scroll progress of <see cref="Target" /> is drawn around the
    /// button. The value is forwarded to the button. Unset hides the ring.
    /// </summary>
    public bool IsShowProgress
    {
        get => GetValue(IsShowProgressProperty);
        set => SetValue(IsShowProgressProperty, value);
    }

    #endregion

    /// <summary>
    /// Called when the host is attached to a visual tree. It creates the button, and when
    /// <see cref="Target" /> is <see langword="null" />, resolves the target to the nearest ancestor scroll viewer.
    /// Overrides must call the base implementation, otherwise no button is created.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Target ??= this.FindAncestorOfType<AvaScrollViewer>();
    }
}