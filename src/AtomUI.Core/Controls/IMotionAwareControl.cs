using Avalonia;

namespace AtomUI.Controls;

public interface IMotionAwareControl
{
    bool IsMotionEnabled { get; }
}

public abstract class MotionAwareControlProperty
{
    public const string IsMotionEnabledPropertyName = "IsMotionEnabled";
    public const string MotionDurationPropertyName = "MotionDuration";

    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        AvaloniaProperty.Register<StyledElement, bool>(IsMotionEnabledPropertyName);
    
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty = 
        AvaloniaProperty.Register<StyledElement, TimeSpan>(MotionDurationPropertyName, TimeSpan.FromMilliseconds(200));
}