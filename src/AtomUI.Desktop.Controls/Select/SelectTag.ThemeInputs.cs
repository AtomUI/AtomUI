using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class SelectTag
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> ThemeMultipleItemBgProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, IBrush?>("ThemeMultipleItemBg", inherits: true);

    public static IBrush? GetThemeMultipleItemBg(StyledElement element) => element.GetValue(ThemeMultipleItemBgProperty);
    public static void SetThemeMultipleItemBg(StyledElement element, IBrush? value) => element.SetValue(ThemeMultipleItemBgProperty, value);

    public static readonly AttachedProperty<double> ThemeMultipleItemHeightProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, double>("ThemeMultipleItemHeight", inherits: true);

    public static double GetThemeMultipleItemHeight(StyledElement element) => element.GetValue(ThemeMultipleItemHeightProperty);
    public static void SetThemeMultipleItemHeight(StyledElement element, double value) => element.SetValue(ThemeMultipleItemHeightProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeMultipleSelectorBgDisabledProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, IBrush?>("ThemeMultipleSelectorBgDisabled", inherits: true);

    public static IBrush? GetThemeMultipleSelectorBgDisabled(StyledElement element) => element.GetValue(ThemeMultipleSelectorBgDisabledProperty);
    public static void SetThemeMultipleSelectorBgDisabled(StyledElement element, IBrush? value) => element.SetValue(ThemeMultipleSelectorBgDisabledProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeMultipleItemColorDisabledProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, IBrush?>("ThemeMultipleItemColorDisabled", inherits: true);

    public static IBrush? GetThemeMultipleItemColorDisabled(StyledElement element) => element.GetValue(ThemeMultipleItemColorDisabledProperty);
    public static void SetThemeMultipleItemColorDisabled(StyledElement element, IBrush? value) => element.SetValue(ThemeMultipleItemColorDisabledProperty, value);

    public static readonly AttachedProperty<double> ThemeOptionFontSizeProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, double>("ThemeOptionFontSize", inherits: true);

    public static double GetThemeOptionFontSize(StyledElement element) => element.GetValue(ThemeOptionFontSizeProperty);
    public static void SetThemeOptionFontSize(StyledElement element, double value) => element.SetValue(ThemeOptionFontSizeProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMultiModePaddingProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeMultiModePadding", inherits: true);

    public static Thickness GetThemeMultiModePadding(StyledElement element) => element.GetValue(ThemeMultiModePaddingProperty);
    public static void SetThemeMultiModePadding(StyledElement element, Thickness value) => element.SetValue(ThemeMultiModePaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeCompactMultiModePaddingProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeCompactMultiModePadding", inherits: true);

    public static Thickness GetThemeCompactMultiModePadding(StyledElement element) => element.GetValue(ThemeCompactMultiModePaddingProperty);
    public static void SetThemeCompactMultiModePadding(StyledElement element, Thickness value) => element.SetValue(ThemeCompactMultiModePaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMultiModePrefixIndentProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeMultiModePrefixIndent", inherits: true);

    public static Thickness GetThemeMultiModePrefixIndent(StyledElement element) => element.GetValue(ThemeMultiModePrefixIndentProperty);
    public static void SetThemeMultiModePrefixIndent(StyledElement element, Thickness value) => element.SetValue(ThemeMultiModePrefixIndentProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeSingleModePaddingProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeSingleModePadding", inherits: true);

    public static Thickness GetThemeSingleModePadding(StyledElement element) => element.GetValue(ThemeSingleModePaddingProperty);
    public static void SetThemeSingleModePadding(StyledElement element, Thickness value) => element.SetValue(ThemeSingleModePaddingProperty, value);

    #endregion
}
