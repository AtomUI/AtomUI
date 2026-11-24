using Avalonia;

namespace AtomUI.Desktop.Controls.Primitives.Themes;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class RangeInfoPickerInputTheme
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> RangeArrowMarginProperty =
        AvaloniaProperty.RegisterAttached<RangeInfoPickerInputTheme, StyledElement, Thickness>("RangeArrowMargin", inherits: true);

    public static Thickness GetRangeArrowMargin(StyledElement element) => element.GetValue(RangeArrowMarginProperty);
    public static void SetRangeArrowMargin(StyledElement element, Thickness value) => element.SetValue(RangeArrowMarginProperty, value);

    public static readonly AttachedProperty<double> RangeIndicatorThicknessProperty =
        AvaloniaProperty.RegisterAttached<RangeInfoPickerInputTheme, StyledElement, double>("RangeIndicatorThickness", inherits: true);

    public static double GetRangeIndicatorThickness(StyledElement element) => element.GetValue(RangeIndicatorThicknessProperty);
    public static void SetRangeIndicatorThickness(StyledElement element, double value) => element.SetValue(RangeIndicatorThicknessProperty, value);

    #endregion
}
