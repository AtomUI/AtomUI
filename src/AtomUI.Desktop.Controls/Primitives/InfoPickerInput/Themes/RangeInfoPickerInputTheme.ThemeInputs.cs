using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Primitives.Themes;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class RangeInfoPickerInputTheme
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> ThemeRangePickerArrowMarginProperty =
        AvaloniaProperty.RegisterAttached<RangeInfoPickerInputTheme, StyledElement, Thickness>("ThemeRangePickerArrowMargin", inherits: true);

    public static Thickness GetThemeRangePickerArrowMargin(StyledElement element) => element.GetValue(ThemeRangePickerArrowMarginProperty);
    public static void SetThemeRangePickerArrowMargin(StyledElement element, Thickness value) => element.SetValue(ThemeRangePickerArrowMarginProperty, value);

    public static readonly AttachedProperty<double> ThemeRangePickerIndicatorThicknessProperty =
        AvaloniaProperty.RegisterAttached<RangeInfoPickerInputTheme, StyledElement, double>("ThemeRangePickerIndicatorThickness", inherits: true);

    public static double GetThemeRangePickerIndicatorThickness(StyledElement element) => element.GetValue(ThemeRangePickerIndicatorThicknessProperty);
    public static void SetThemeRangePickerIndicatorThickness(StyledElement element, double value) => element.SetValue(ThemeRangePickerIndicatorThicknessProperty, value);

    #endregion
}
