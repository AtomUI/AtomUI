using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class TimeView
{
    #region 公共属性定义

    public static readonly AttachedProperty<double> ThemeItemHeightProperty =
        AvaloniaProperty.RegisterAttached<TimeView, StyledElement, double>("ThemeItemHeight", inherits: true);

    public static double GetThemeItemHeight(StyledElement element) => element.GetValue(ThemeItemHeightProperty);
    public static void SetThemeItemHeight(StyledElement element, double value) => element.SetValue(ThemeItemHeightProperty, value);

    public static readonly AttachedProperty<double> ThemeItemWidthProperty =
        AvaloniaProperty.RegisterAttached<TimeView, StyledElement, double>("ThemeItemWidth", inherits: true);

    public static double GetThemeItemWidth(StyledElement element) => element.GetValue(ThemeItemWidthProperty);
    public static void SetThemeItemWidth(StyledElement element, double value) => element.SetValue(ThemeItemWidthProperty, value);

    public static readonly AttachedProperty<double> ThemePeriodHostWidthProperty =
        AvaloniaProperty.RegisterAttached<TimeView, StyledElement, double>("ThemePeriodHostWidth", inherits: true);

    public static double GetThemePeriodHostWidth(StyledElement element) => element.GetValue(ThemePeriodHostWidthProperty);
    public static void SetThemePeriodHostWidth(StyledElement element, double value) => element.SetValue(ThemePeriodHostWidthProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemPaddingProperty =
        AvaloniaProperty.RegisterAttached<TimeView, StyledElement, Thickness>("ThemeItemPadding", inherits: true);

    public static Thickness GetThemeItemPadding(StyledElement element) => element.GetValue(ThemeItemPaddingProperty);
    public static void SetThemeItemPadding(StyledElement element, Thickness value) => element.SetValue(ThemeItemPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemMarginProperty =
        AvaloniaProperty.RegisterAttached<TimeView, StyledElement, Thickness>("ThemeItemMargin", inherits: true);

    public static Thickness GetThemeItemMargin(StyledElement element) => element.GetValue(ThemeItemMarginProperty);
    public static void SetThemeItemMargin(StyledElement element, Thickness value) => element.SetValue(ThemeItemMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHeaderMarginProperty =
        AvaloniaProperty.RegisterAttached<TimeView, StyledElement, Thickness>("ThemeHeaderMargin", inherits: true);

    public static Thickness GetThemeHeaderMargin(StyledElement element) => element.GetValue(ThemeHeaderMarginProperty);
    public static void SetThemeHeaderMargin(StyledElement element, Thickness value) => element.SetValue(ThemeHeaderMarginProperty, value);

    #endregion
}
