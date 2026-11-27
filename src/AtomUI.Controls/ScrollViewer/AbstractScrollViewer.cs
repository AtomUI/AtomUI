using Avalonia;

namespace AtomUI.Controls.Commons;

using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;

/// <summary>
/// Base class of the AtomUI scroll viewers. It adds the motion switch that the template passes on to the scroll
/// bars, and the internal opacity and dragging state that the theme binds the scroll bars and the corner between
/// them to.
/// </summary>
public abstract class AbstractScrollViewer : AvaloniaScrollViewer, IMotionAwareControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractScrollViewer>();

    /// <summary>
    /// Enables the transitions of the scroll bars. The theme then animates the opacity of the bars and of the
    /// corner between them, and the template passes the value to the bars, which pass it to their thumbs for their
    /// own transitions. Set it on the scroll viewer; when the value is not set, the theme supplies it from the
    /// <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ScrollBarsSeparatorOpacityProperty =
        AvaloniaProperty.Register<AbstractScrollViewer, double>(nameof(ScrollBarsSeparatorOpacity));

    internal static readonly StyledProperty<double> ScrollBarOpacityProperty =
        AvaloniaProperty.Register<AbstractScrollViewer, double>(nameof(ScrollBarOpacity));

    internal static readonly StyledProperty<bool> IsScrollBarDraggingProperty =
        AvaloniaProperty.Register<AbstractScrollViewer, bool>(nameof(IsScrollBarDragging));

    /// <summary>
    /// The opacity of the corner panel between the scroll bars, from 0 (invisible) to 1. The theme of the scroll
    /// viewer binds the opacity of that panel to it and sets it from <c>IsExpanded</c>. The themes animate it with
    /// a transition while motion is enabled. This class only stores the value.
    /// </summary>
    internal double ScrollBarsSeparatorOpacity
    {
        get => GetValue(ScrollBarsSeparatorOpacityProperty);
        set => SetValue(ScrollBarsSeparatorOpacityProperty, value);
    }

    /// <summary>
    /// The opacity of the scroll bars, from 0 (invisible) to 1. The themes bind the opacity of the bars to it. The
    /// theme of the scroll viewer animates it with a transition while motion is enabled, and sets it to 1 outside
    /// lite mode and, in lite mode, while the pointer is over the viewer or a bar is dragged, and to 0 otherwise.
    /// This class only stores the value.
    /// </summary>
    internal double ScrollBarOpacity
    {
        get => GetValue(ScrollBarOpacityProperty);
        set => SetValue(ScrollBarOpacityProperty, value);
    }

    /// <summary>
    /// Whether a scroll bar of the viewer is being dragged. The theme selects on it together with lite mode
    /// (<c>^[IsLiteMode=True][IsScrollBarDragging=True]</c>) to show the bars while a bar is dragged. This class
    /// only stores the value.
    /// </summary>
    internal bool IsScrollBarDragging
    {
        get => GetValue(IsScrollBarDraggingProperty);
        set => SetValue(IsScrollBarDraggingProperty, value);
    }

    #endregion

}
