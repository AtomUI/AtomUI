using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the thumb that the template of an <see cref="AbstractScrollBar" /> places in its track. The
/// scrollbar passes its <see cref="Orientation" /> and <see cref="IsMotionEnabled" /> to the thumb, and the theme
/// of the thumb takes its thickness from the orientation.
/// </summary>
public abstract class AbstractScrollBarThumb : Thumb, IMotionAwareControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<AbstractScrollBarThumb>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractScrollBarThumb>();
    
    /// <summary>
    /// Axis of the scrollbar that owns the thumb. The template of the scrollbar copies its own orientation here, so
    /// the value is normally not set on the thumb itself. <see cref="Avalonia.Layout.Orientation.Vertical" />, the
    /// default, makes the thickness of the thumb its width, and
    /// <see cref="Avalonia.Layout.Orientation.Horizontal" /> makes it its height.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    
    /// <summary>
    /// Enables the transitions of the thumb between its normal, hovered and pressed background and between its
    /// thickness values. A thumb in the template of a scrollbar receives the value of the scrollbar; the default of
    /// <see langword="false" /> shows every change at once.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AbstractScrollBarThumb, bool>(nameof(IsExpanded));
    
    /// <summary>
    /// Expanded state of the thumb in lite mode. The theme sets it to <see langword="true" /> while a lite-mode thumb
    /// is hovered or pressed, and then gives the thumb its expanded thickness. The template of the scrollbar binds
    /// it to the expanded state of the scrollbar in both directions.
    /// </summary>
    internal bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    #endregion

}