using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class MenuPopupScrollHost
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> ThemeMenuPopupContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, Thickness>("ThemeMenuPopupContentPadding", inherits: true);

    public static Thickness GetThemeMenuPopupContentPadding(StyledElement element) => element.GetValue(ThemeMenuPopupContentPaddingProperty);
    public static void SetThemeMenuPopupContentPadding(StyledElement element, Thickness value) => element.SetValue(ThemeMenuPopupContentPaddingProperty, value);

    public static readonly AttachedProperty<double> ThemeMenuPopupMinWidthProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ThemeMenuPopupMinWidth", inherits: true);

    public static double GetThemeMenuPopupMinWidth(StyledElement element) => element.GetValue(ThemeMenuPopupMinWidthProperty);
    public static void SetThemeMenuPopupMinWidth(StyledElement element, double value) => element.SetValue(ThemeMenuPopupMinWidthProperty, value);

    public static readonly AttachedProperty<double> ThemeMenuPopupMaxWidthProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ThemeMenuPopupMaxWidth", inherits: true);

    public static double GetThemeMenuPopupMaxWidth(StyledElement element) => element.GetValue(ThemeMenuPopupMaxWidthProperty);
    public static void SetThemeMenuPopupMaxWidth(StyledElement element, double value) => element.SetValue(ThemeMenuPopupMaxWidthProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeMenuPopupBgColorProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeMenuPopupBgColor", inherits: true);

    public static IBrush? GetThemeMenuPopupBgColor(StyledElement element) => element.GetValue(ThemeMenuPopupBgColorProperty);
    public static void SetThemeMenuPopupBgColor(StyledElement element, IBrush? value) => element.SetValue(ThemeMenuPopupBgColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemColorProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeItemColor", inherits: true);

    public static IBrush? GetThemeItemColor(StyledElement element) => element.GetValue(ThemeItemColorProperty);
    public static void SetThemeItemColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeKeyGestureColorProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeKeyGestureColor", inherits: true);

    public static IBrush? GetThemeKeyGestureColor(StyledElement element) => element.GetValue(ThemeKeyGestureColorProperty);
    public static void SetThemeKeyGestureColor(StyledElement element, IBrush? value) => element.SetValue(ThemeKeyGestureColorProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemMarginProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, Thickness>("ThemeItemMargin", inherits: true);

    public static Thickness GetThemeItemMargin(StyledElement element) => element.GetValue(ThemeItemMarginProperty);
    public static void SetThemeItemMargin(StyledElement element, Thickness value) => element.SetValue(ThemeItemMarginProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemHoverColorProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeItemHoverColor", inherits: true);

    public static IBrush? GetThemeItemHoverColor(StyledElement element) => element.GetValue(ThemeItemHoverColorProperty);
    public static void SetThemeItemHoverColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemHoverColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemDisabledColorProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeItemDisabledColor", inherits: true);

    public static IBrush? GetThemeItemDisabledColor(StyledElement element) => element.GetValue(ThemeItemDisabledColorProperty);
    public static void SetThemeItemDisabledColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemDisabledColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeDangerItemColorProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeDangerItemColor", inherits: true);

    public static IBrush? GetThemeDangerItemColor(StyledElement element) => element.GetValue(ThemeDangerItemColorProperty);
    public static void SetThemeDangerItemColor(StyledElement element, IBrush? value) => element.SetValue(ThemeDangerItemColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeDangerItemHoverColorProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeDangerItemHoverColor", inherits: true);

    public static IBrush? GetThemeDangerItemHoverColor(StyledElement element) => element.GetValue(ThemeDangerItemHoverColorProperty);
    public static void SetThemeDangerItemHoverColor(StyledElement element, IBrush? value) => element.SetValue(ThemeDangerItemHoverColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemBgProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeItemBg", inherits: true);

    public static IBrush? GetThemeItemBg(StyledElement element) => element.GetValue(ThemeItemBgProperty);
    public static void SetThemeItemBg(StyledElement element, IBrush? value) => element.SetValue(ThemeItemBgProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemHoverBgProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ThemeItemHoverBg", inherits: true);

    public static IBrush? GetThemeItemHoverBg(StyledElement element) => element.GetValue(ThemeItemHoverBgProperty);
    public static void SetThemeItemHoverBg(StyledElement element, IBrush? value) => element.SetValue(ThemeItemHoverBgProperty, value);

    public static readonly AttachedProperty<double> ThemeItemHeightProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ThemeItemHeight", inherits: true);

    public static double GetThemeItemHeight(StyledElement element) => element.GetValue(ThemeItemHeightProperty);
    public static void SetThemeItemHeight(StyledElement element, double value) => element.SetValue(ThemeItemHeightProperty, value);

    public static readonly AttachedProperty<double> ThemeItemIconSizeProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ThemeItemIconSize", inherits: true);

    public static double GetThemeItemIconSize(StyledElement element) => element.GetValue(ThemeItemIconSizeProperty);
    public static void SetThemeItemIconSize(StyledElement element, double value) => element.SetValue(ThemeItemIconSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeItemIconMarginInlineEndProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ThemeItemIconMarginInlineEnd", inherits: true);

    public static double GetThemeItemIconMarginInlineEnd(StyledElement element) => element.GetValue(ThemeItemIconMarginInlineEndProperty);
    public static void SetThemeItemIconMarginInlineEnd(StyledElement element, double value) => element.SetValue(ThemeItemIconMarginInlineEndProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeGroupTitlePaddingProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, Thickness>("ThemeGroupTitlePadding", inherits: true);

    public static Thickness GetThemeGroupTitlePadding(StyledElement element) => element.GetValue(ThemeGroupTitlePaddingProperty);
    public static void SetThemeGroupTitlePadding(StyledElement element, Thickness value) => element.SetValue(ThemeGroupTitlePaddingProperty, value);

    public static readonly AttachedProperty<CornerRadius> ThemeItemBorderRadiusProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, CornerRadius>("ThemeItemBorderRadius", inherits: true);

    public static CornerRadius GetThemeItemBorderRadius(StyledElement element) => element.GetValue(ThemeItemBorderRadiusProperty);
    public static void SetThemeItemBorderRadius(StyledElement element, CornerRadius value) => element.SetValue(ThemeItemBorderRadiusProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemPaddingInlineProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, Thickness>("ThemeItemPaddingInline", inherits: true);

    public static Thickness GetThemeItemPaddingInline(StyledElement element) => element.GetValue(ThemeItemPaddingInlineProperty);
    public static void SetThemeItemPaddingInline(StyledElement element, Thickness value) => element.SetValue(ThemeItemPaddingInlineProperty, value);

    public static readonly AttachedProperty<double> ThemeSeparatorItemHeightProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ThemeSeparatorItemHeight", inherits: true);

    public static double GetThemeSeparatorItemHeight(StyledElement element) => element.GetValue(ThemeSeparatorItemHeightProperty);
    public static void SetThemeSeparatorItemHeight(StyledElement element, double value) => element.SetValue(ThemeSeparatorItemHeightProperty, value);

    public static readonly AttachedProperty<BoxShadows> ThemeMenuPopupShadowsProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, BoxShadows>("ThemeMenuPopupShadows", inherits: true);

    public static BoxShadows GetThemeMenuPopupShadows(StyledElement element) => element.GetValue(ThemeMenuPopupShadowsProperty);
    public static void SetThemeMenuPopupShadows(StyledElement element, BoxShadows value) => element.SetValue(ThemeMenuPopupShadowsProperty, value);

    #endregion
}
