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

    public static readonly AttachedProperty<double> ThemeMultipleItemHeightSMProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, double>("ThemeMultipleItemHeightSM", inherits: true);

    public static double GetThemeMultipleItemHeightSM(StyledElement element) => element.GetValue(ThemeMultipleItemHeightSMProperty);
    public static void SetThemeMultipleItemHeightSM(StyledElement element, double value) => element.SetValue(ThemeMultipleItemHeightSMProperty, value);

    public static readonly AttachedProperty<double> ThemeMultipleItemHeightLGProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, double>("ThemeMultipleItemHeightLG", inherits: true);

    public static double GetThemeMultipleItemHeightLG(StyledElement element) => element.GetValue(ThemeMultipleItemHeightLGProperty);
    public static void SetThemeMultipleItemHeightLG(StyledElement element, double value) => element.SetValue(ThemeMultipleItemHeightLGProperty, value);

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

    public static readonly AttachedProperty<Thickness> ThemeMultiModePaddingSMProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeMultiModePaddingSM", inherits: true);

    public static Thickness GetThemeMultiModePaddingSM(StyledElement element) => element.GetValue(ThemeMultiModePaddingSMProperty);
    public static void SetThemeMultiModePaddingSM(StyledElement element, Thickness value) => element.SetValue(ThemeMultiModePaddingSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMultiModePaddingLGProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeMultiModePaddingLG", inherits: true);

    public static Thickness GetThemeMultiModePaddingLG(StyledElement element) => element.GetValue(ThemeMultiModePaddingLGProperty);
    public static void SetThemeMultiModePaddingLG(StyledElement element, Thickness value) => element.SetValue(ThemeMultiModePaddingLGProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMultiModePrefixIndentProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeMultiModePrefixIndent", inherits: true);

    public static Thickness GetThemeMultiModePrefixIndent(StyledElement element) => element.GetValue(ThemeMultiModePrefixIndentProperty);
    public static void SetThemeMultiModePrefixIndent(StyledElement element, Thickness value) => element.SetValue(ThemeMultiModePrefixIndentProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMultiModePrefixIndentSMProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeMultiModePrefixIndentSM", inherits: true);

    public static Thickness GetThemeMultiModePrefixIndentSM(StyledElement element) => element.GetValue(ThemeMultiModePrefixIndentSMProperty);
    public static void SetThemeMultiModePrefixIndentSM(StyledElement element, Thickness value) => element.SetValue(ThemeMultiModePrefixIndentSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMultiModePrefixIndentLGProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeMultiModePrefixIndentLG", inherits: true);

    public static Thickness GetThemeMultiModePrefixIndentLG(StyledElement element) => element.GetValue(ThemeMultiModePrefixIndentLGProperty);
    public static void SetThemeMultiModePrefixIndentLG(StyledElement element, Thickness value) => element.SetValue(ThemeMultiModePrefixIndentLGProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeSingleModePaddingProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeSingleModePadding", inherits: true);

    public static Thickness GetThemeSingleModePadding(StyledElement element) => element.GetValue(ThemeSingleModePaddingProperty);
    public static void SetThemeSingleModePadding(StyledElement element, Thickness value) => element.SetValue(ThemeSingleModePaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeSingleModePaddingSMProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeSingleModePaddingSM", inherits: true);

    public static Thickness GetThemeSingleModePaddingSM(StyledElement element) => element.GetValue(ThemeSingleModePaddingSMProperty);
    public static void SetThemeSingleModePaddingSM(StyledElement element, Thickness value) => element.SetValue(ThemeSingleModePaddingSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeSingleModePaddingLGProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("ThemeSingleModePaddingLG", inherits: true);

    public static Thickness GetThemeSingleModePaddingLG(StyledElement element) => element.GetValue(ThemeSingleModePaddingLGProperty);
    public static void SetThemeSingleModePaddingLG(StyledElement element, Thickness value) => element.SetValue(ThemeSingleModePaddingLGProperty, value);

    #endregion
}
