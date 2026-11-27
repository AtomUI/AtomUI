using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of the control that shows which step of a tour is active. Assign an instance to
/// <see cref="Tour.Indicator" />; the tour then keeps <see cref="StepCount" />, <see cref="ActiveIndex" />,
/// <see cref="StyleType" /> and <see cref="IsMotionEnabled" /> in sync with its own state.
/// </summary>
public abstract class TourIndicator : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="StepCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> StepCountProperty = 
        AvaloniaProperty.Register<TourIndicator, int>(nameof(StepCount));
    
    /// <summary>
    /// Identifies the <see cref="ActiveIndex" /> property.
    /// </summary>
    public static readonly StyledProperty<int> ActiveIndexProperty = 
        AvaloniaProperty.Register<TourIndicator, int>(nameof(ActiveIndex));

    /// <summary>
    /// Identifies the <see cref="StyleType" /> property.
    /// </summary>
    public static readonly StyledProperty<TourStyleType> StyleTypeProperty =
        Tour.StyleTypeProperty.AddOwner<TourIndicator>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TourIndicator>();

    /// <summary>
    /// Number of steps in the tour. The hosting tour keeps this value in sync with its own step count.
    /// </summary>
    public int StepCount
    {
        get => GetValue(StepCountProperty);
        set => SetValue(StepCountProperty, value);
    }
    
    /// <summary>
    /// Zero-based index of the step that is currently shown. The hosting tour keeps this value in sync with its own
    /// current index.
    /// </summary>
    public int ActiveIndex
    {
        get => GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }
    
    /// <summary>
    /// Style variant of the indicator, which selects its colors. The hosting tour sets it to the style type of the
    /// current step, or to <see cref="Tour.StyleType" /> when the step sets none.
    /// </summary>
    public TourStyleType StyleType
    {
        get => GetValue(StyleTypeProperty);
        set => SetValue(StyleTypeProperty, value);
    }
    
    /// <summary>
    /// Whether motion is enabled for the indicator. The hosting tour relays its own
    /// <see cref="Tour.IsMotionEnabled" /> value to this property.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion

    static TourIndicator()
    {
        AffectsRender<TourIndicator>(ActiveIndexProperty);
        AffectsMeasure<TourIndicator>( StepCountProperty);
    }
}