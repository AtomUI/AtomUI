using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class PaginationNavItem
{
    #region 公共属性定义

    public static readonly AttachedProperty<double> ThemeItemSizeProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemeItemSize", inherits: true);

    public static double GetThemeItemSize(StyledElement element) => element.GetValue(ThemeItemSizeProperty);
    public static void SetThemeItemSize(StyledElement element, double value) => element.SetValue(ThemeItemSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeItemSizeSMProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemeItemSizeSM", inherits: true);

    public static double GetThemeItemSizeSM(StyledElement element) => element.GetValue(ThemeItemSizeSMProperty);
    public static void SetThemeItemSizeSM(StyledElement element, double value) => element.SetValue(ThemeItemSizeSMProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemActiveColorDisabledProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, IBrush?>("ThemeItemActiveColorDisabled", inherits: true);

    public static IBrush? GetThemeItemActiveColorDisabled(StyledElement element) => element.GetValue(ThemeItemActiveColorDisabledProperty);
    public static void SetThemeItemActiveColorDisabled(StyledElement element, IBrush? value) => element.SetValue(ThemeItemActiveColorDisabledProperty, value);

    public static readonly AttachedProperty<double> ThemePaginationLayoutSpacingProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemePaginationLayoutSpacing", inherits: true);

    public static double GetThemePaginationLayoutSpacing(StyledElement element) => element.GetValue(ThemePaginationLayoutSpacingProperty);
    public static void SetThemePaginationLayoutSpacing(StyledElement element, double value) => element.SetValue(ThemePaginationLayoutSpacingProperty, value);

    public static readonly AttachedProperty<double> ThemePaginationLayoutMiniSpacingProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemePaginationLayoutMiniSpacing", inherits: true);

    public static double GetThemePaginationLayoutMiniSpacing(StyledElement element) => element.GetValue(ThemePaginationLayoutMiniSpacingProperty);
    public static void SetThemePaginationLayoutMiniSpacing(StyledElement element, double value) => element.SetValue(ThemePaginationLayoutMiniSpacingProperty, value);

    public static readonly AttachedProperty<double> ThemePaginationQuickJumperInputWidthProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemePaginationQuickJumperInputWidth", inherits: true);

    public static double GetThemePaginationQuickJumperInputWidth(StyledElement element) => element.GetValue(ThemePaginationQuickJumperInputWidthProperty);
    public static void SetThemePaginationQuickJumperInputWidth(StyledElement element, double value) => element.SetValue(ThemePaginationQuickJumperInputWidthProperty, value);

    public static readonly AttachedProperty<double> ThemePaginationMiniQuickJumperInputWidthProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemePaginationMiniQuickJumperInputWidth", inherits: true);

    public static double GetThemePaginationMiniQuickJumperInputWidth(StyledElement element) => element.GetValue(ThemePaginationMiniQuickJumperInputWidthProperty);
    public static void SetThemePaginationMiniQuickJumperInputWidth(StyledElement element, double value) => element.SetValue(ThemePaginationMiniQuickJumperInputWidthProperty, value);

    public static readonly AttachedProperty<Thickness> ThemePaginationItemPaddingInlineProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, Thickness>("ThemePaginationItemPaddingInline", inherits: true);

    public static Thickness GetThemePaginationItemPaddingInline(StyledElement element) => element.GetValue(ThemePaginationItemPaddingInlineProperty);
    public static void SetThemePaginationItemPaddingInline(StyledElement element, Thickness value) => element.SetValue(ThemePaginationItemPaddingInlineProperty, value);

    #endregion
}
