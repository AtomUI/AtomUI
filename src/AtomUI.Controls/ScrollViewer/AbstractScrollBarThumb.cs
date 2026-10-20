using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

public abstract class AbstractScrollBarThumb : Thumb, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<AbstractScrollBarThumb>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractScrollBarThumb>();
    
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AbstractScrollBarThumb, bool>(nameof(IsExpanded));
    
    internal bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    #endregion

}