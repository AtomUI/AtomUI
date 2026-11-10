using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class GroupHeaderItem
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> ThemeContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, Thickness>("ThemeContentPadding", inherits: true);

    public static Thickness GetThemeContentPadding(StyledElement element) => element.GetValue(ThemeContentPaddingProperty);
    public static void SetThemeContentPadding(StyledElement element, Thickness value) => element.SetValue(ThemeContentPaddingProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemColorProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, IBrush?>("ThemeItemColor", inherits: true);

    public static IBrush? GetThemeItemColor(StyledElement element) => element.GetValue(ThemeItemColorProperty);
    public static void SetThemeItemColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemHoverColorProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, IBrush?>("ThemeItemHoverColor", inherits: true);

    public static IBrush? GetThemeItemHoverColor(StyledElement element) => element.GetValue(ThemeItemHoverColorProperty);
    public static void SetThemeItemHoverColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemHoverColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemSelectedColorProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, IBrush?>("ThemeItemSelectedColor", inherits: true);

    public static IBrush? GetThemeItemSelectedColor(StyledElement element) => element.GetValue(ThemeItemSelectedColorProperty);
    public static void SetThemeItemSelectedColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemSelectedColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemDisabledColorProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, IBrush?>("ThemeItemDisabledColor", inherits: true);

    public static IBrush? GetThemeItemDisabledColor(StyledElement element) => element.GetValue(ThemeItemDisabledColorProperty);
    public static void SetThemeItemDisabledColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemDisabledColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemBgColorProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, IBrush?>("ThemeItemBgColor", inherits: true);

    public static IBrush? GetThemeItemBgColor(StyledElement element) => element.GetValue(ThemeItemBgColorProperty);
    public static void SetThemeItemBgColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemBgColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemHoverBgColorProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, IBrush?>("ThemeItemHoverBgColor", inherits: true);

    public static IBrush? GetThemeItemHoverBgColor(StyledElement element) => element.GetValue(ThemeItemHoverBgColorProperty);
    public static void SetThemeItemHoverBgColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemHoverBgColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemSelectedBgColorProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, IBrush?>("ThemeItemSelectedBgColor", inherits: true);

    public static IBrush? GetThemeItemSelectedBgColor(StyledElement element) => element.GetValue(ThemeItemSelectedBgColorProperty);
    public static void SetThemeItemSelectedBgColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemSelectedBgColorProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemPaddingSMProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, Thickness>("ThemeItemPaddingSM", inherits: true);

    public static Thickness GetThemeItemPaddingSM(StyledElement element) => element.GetValue(ThemeItemPaddingSMProperty);
    public static void SetThemeItemPaddingSM(StyledElement element, Thickness value) => element.SetValue(ThemeItemPaddingSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemPaddingProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, Thickness>("ThemeItemPadding", inherits: true);

    public static Thickness GetThemeItemPadding(StyledElement element) => element.GetValue(ThemeItemPaddingProperty);
    public static void SetThemeItemPadding(StyledElement element, Thickness value) => element.SetValue(ThemeItemPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemPaddingLGProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, Thickness>("ThemeItemPaddingLG", inherits: true);

    public static Thickness GetThemeItemPaddingLG(StyledElement element) => element.GetValue(ThemeItemPaddingLGProperty);
    public static void SetThemeItemPaddingLG(StyledElement element, Thickness value) => element.SetValue(ThemeItemPaddingLGProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemMarginProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, Thickness>("ThemeItemMargin", inherits: true);

    public static Thickness GetThemeItemMargin(StyledElement element) => element.GetValue(ThemeItemMarginProperty);
    public static void SetThemeItemMargin(StyledElement element, Thickness value) => element.SetValue(ThemeItemMarginProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeGroupHeaderColorProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, IBrush?>("ThemeGroupHeaderColor", inherits: true);

    public static IBrush? GetThemeGroupHeaderColor(StyledElement element) => element.GetValue(ThemeGroupHeaderColorProperty);
    public static void SetThemeGroupHeaderColor(StyledElement element, IBrush? value) => element.SetValue(ThemeGroupHeaderColorProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeGroupHeaderPaddingProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, Thickness>("ThemeGroupHeaderPadding", inherits: true);

    public static Thickness GetThemeGroupHeaderPadding(StyledElement element) => element.GetValue(ThemeGroupHeaderPaddingProperty);
    public static void SetThemeGroupHeaderPadding(StyledElement element, Thickness value) => element.SetValue(ThemeGroupHeaderPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeSelectedIndicatorMarginProperty =
        AvaloniaProperty.RegisterAttached<GroupHeaderItem, StyledElement, Thickness>("ThemeSelectedIndicatorMargin", inherits: true);

    public static Thickness GetThemeSelectedIndicatorMargin(StyledElement element) => element.GetValue(ThemeSelectedIndicatorMarginProperty);
    public static void SetThemeSelectedIndicatorMargin(StyledElement element, Thickness value) => element.SetValue(ThemeSelectedIndicatorMarginProperty, value);

    #endregion
}
