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

    public static readonly AttachedProperty<IBrush?> ThemeItemActiveColorDisabledProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, IBrush?>("ThemeItemActiveColorDisabled", inherits: true);

    public static IBrush? GetThemeItemActiveColorDisabled(StyledElement element) => element.GetValue(ThemeItemActiveColorDisabledProperty);
    public static void SetThemeItemActiveColorDisabled(StyledElement element, IBrush? value) => element.SetValue(ThemeItemActiveColorDisabledProperty, value);

    public static readonly AttachedProperty<double> ThemePaginationLayoutSpacingProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemePaginationLayoutSpacing", inherits: true);

    public static double GetThemePaginationLayoutSpacing(StyledElement element) => element.GetValue(ThemePaginationLayoutSpacingProperty);
    public static void SetThemePaginationLayoutSpacing(StyledElement element, double value) => element.SetValue(ThemePaginationLayoutSpacingProperty, value);

    public static readonly AttachedProperty<double> ThemeQuickJumperSpacingProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemeQuickJumperSpacing", inherits: true);

    public static double GetThemeQuickJumperSpacing(StyledElement element) => element.GetValue(ThemeQuickJumperSpacingProperty);
    public static void SetThemeQuickJumperSpacing(StyledElement element, double value) => element.SetValue(ThemeQuickJumperSpacingProperty, value);

    public static readonly AttachedProperty<double> ThemePaginationQuickJumperInputWidthProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, double>("ThemePaginationQuickJumperInputWidth", inherits: true);

    public static double GetThemePaginationQuickJumperInputWidth(StyledElement element) => element.GetValue(ThemePaginationQuickJumperInputWidthProperty);
    public static void SetThemePaginationQuickJumperInputWidth(StyledElement element, double value) => element.SetValue(ThemePaginationQuickJumperInputWidthProperty, value);

    public static readonly AttachedProperty<Thickness> ThemePaginationItemPaddingInlineProperty =
        AvaloniaProperty.RegisterAttached<PaginationNavItem, StyledElement, Thickness>("ThemePaginationItemPaddingInline", inherits: true);

    public static Thickness GetThemePaginationItemPaddingInline(StyledElement element) => element.GetValue(ThemePaginationItemPaddingInlineProperty);
    public static void SetThemePaginationItemPaddingInline(StyledElement element, Thickness value) => element.SetValue(ThemePaginationItemPaddingInlineProperty, value);

    #endregion
}
