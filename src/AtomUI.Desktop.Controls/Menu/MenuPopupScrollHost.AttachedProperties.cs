using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class MenuPopupScrollHost
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, Thickness>("PopupContentPadding", inherits: true);

    public static Thickness GetPopupContentPadding(StyledElement element) => element.GetValue(PopupContentPaddingProperty);
    public static void SetPopupContentPadding(StyledElement element, Thickness value) => element.SetValue(PopupContentPaddingProperty, value);

    public static readonly AttachedProperty<double> PopupMinWidthProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("PopupMinWidth", inherits: true);

    public static double GetPopupMinWidth(StyledElement element) => element.GetValue(PopupMinWidthProperty);
    public static void SetPopupMinWidth(StyledElement element, double value) => element.SetValue(PopupMinWidthProperty, value);

    public static readonly AttachedProperty<double> PopupMaxWidthProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("PopupMaxWidth", inherits: true);

    public static double GetPopupMaxWidth(StyledElement element) => element.GetValue(PopupMaxWidthProperty);
    public static void SetPopupMaxWidth(StyledElement element, double value) => element.SetValue(PopupMaxWidthProperty, value);

    public static readonly AttachedProperty<IBrush?> PopupBackgroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("PopupBackground", inherits: true);

    public static IBrush? GetPopupBackground(StyledElement element) => element.GetValue(PopupBackgroundProperty);
    public static void SetPopupBackground(StyledElement element, IBrush? value) => element.SetValue(PopupBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> ItemForegroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ItemForeground", inherits: true);

    public static IBrush? GetItemForeground(StyledElement element) => element.GetValue(ItemForegroundProperty);
    public static void SetItemForeground(StyledElement element, IBrush? value) => element.SetValue(ItemForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> KeyGestureForegroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("KeyGestureForeground", inherits: true);

    public static IBrush? GetKeyGestureForeground(StyledElement element) => element.GetValue(KeyGestureForegroundProperty);
    public static void SetKeyGestureForeground(StyledElement element, IBrush? value) => element.SetValue(KeyGestureForegroundProperty, value);

    public static readonly AttachedProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, Thickness>("ItemMargin", inherits: true);

    public static Thickness GetItemMargin(StyledElement element) => element.GetValue(ItemMarginProperty);
    public static void SetItemMargin(StyledElement element, Thickness value) => element.SetValue(ItemMarginProperty, value);

    public static readonly AttachedProperty<IBrush?> ItemHoverForegroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ItemHoverForeground", inherits: true);

    public static IBrush? GetItemHoverForeground(StyledElement element) => element.GetValue(ItemHoverForegroundProperty);
    public static void SetItemHoverForeground(StyledElement element, IBrush? value) => element.SetValue(ItemHoverForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> ItemDisabledForegroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ItemDisabledForeground", inherits: true);

    public static IBrush? GetItemDisabledForeground(StyledElement element) => element.GetValue(ItemDisabledForegroundProperty);
    public static void SetItemDisabledForeground(StyledElement element, IBrush? value) => element.SetValue(ItemDisabledForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> DangerItemForegroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("DangerItemForeground", inherits: true);

    public static IBrush? GetDangerItemForeground(StyledElement element) => element.GetValue(DangerItemForegroundProperty);
    public static void SetDangerItemForeground(StyledElement element, IBrush? value) => element.SetValue(DangerItemForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> DangerItemHoverForegroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("DangerItemHoverForeground", inherits: true);

    public static IBrush? GetDangerItemHoverForeground(StyledElement element) => element.GetValue(DangerItemHoverForegroundProperty);
    public static void SetDangerItemHoverForeground(StyledElement element, IBrush? value) => element.SetValue(DangerItemHoverForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> ItemBackgroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ItemBackground", inherits: true);

    public static IBrush? GetItemBackground(StyledElement element) => element.GetValue(ItemBackgroundProperty);
    public static void SetItemBackground(StyledElement element, IBrush? value) => element.SetValue(ItemBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> ItemHoverBackgroundProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, IBrush?>("ItemHoverBackground", inherits: true);

    public static IBrush? GetItemHoverBackground(StyledElement element) => element.GetValue(ItemHoverBackgroundProperty);
    public static void SetItemHoverBackground(StyledElement element, IBrush? value) => element.SetValue(ItemHoverBackgroundProperty, value);

    public static readonly AttachedProperty<double> ItemHeightProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ItemHeight", inherits: true);

    public static double GetItemHeight(StyledElement element) => element.GetValue(ItemHeightProperty);
    public static void SetItemHeight(StyledElement element, double value) => element.SetValue(ItemHeightProperty, value);

    public static readonly AttachedProperty<double> ItemIconSizeProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ItemIconSize", inherits: true);

    public static double GetItemIconSize(StyledElement element) => element.GetValue(ItemIconSizeProperty);
    public static void SetItemIconSize(StyledElement element, double value) => element.SetValue(ItemIconSizeProperty, value);

    public static readonly AttachedProperty<double> ItemIconEndMarginProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("ItemIconEndMargin", inherits: true);

    public static double GetItemIconEndMargin(StyledElement element) => element.GetValue(ItemIconEndMarginProperty);
    public static void SetItemIconEndMargin(StyledElement element, double value) => element.SetValue(ItemIconEndMarginProperty, value);

    public static readonly AttachedProperty<Thickness> GroupTitlePaddingProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, Thickness>("GroupTitlePadding", inherits: true);

    public static Thickness GetGroupTitlePadding(StyledElement element) => element.GetValue(GroupTitlePaddingProperty);
    public static void SetGroupTitlePadding(StyledElement element, Thickness value) => element.SetValue(GroupTitlePaddingProperty, value);

    public static readonly AttachedProperty<CornerRadius> ItemCornerRadiusProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, CornerRadius>("ItemCornerRadius", inherits: true);

    public static CornerRadius GetItemCornerRadius(StyledElement element) => element.GetValue(ItemCornerRadiusProperty);
    public static void SetItemCornerRadius(StyledElement element, CornerRadius value) => element.SetValue(ItemCornerRadiusProperty, value);

    public static readonly AttachedProperty<Thickness> ItemInlinePaddingProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, Thickness>("ItemInlinePadding", inherits: true);

    public static Thickness GetItemInlinePadding(StyledElement element) => element.GetValue(ItemInlinePaddingProperty);
    public static void SetItemInlinePadding(StyledElement element, Thickness value) => element.SetValue(ItemInlinePaddingProperty, value);

    public static readonly AttachedProperty<double> SeparatorItemHeightProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, double>("SeparatorItemHeight", inherits: true);

    public static double GetSeparatorItemHeight(StyledElement element) => element.GetValue(SeparatorItemHeightProperty);
    public static void SetSeparatorItemHeight(StyledElement element, double value) => element.SetValue(SeparatorItemHeightProperty, value);

    public static readonly AttachedProperty<BoxShadows> PopupShadowsProperty =
        AvaloniaProperty.RegisterAttached<MenuPopupScrollHost, StyledElement, BoxShadows>("PopupShadows", inherits: true);

    public static BoxShadows GetPopupShadows(StyledElement element) => element.GetValue(PopupShadowsProperty);
    public static void SetPopupShadows(StyledElement element, BoxShadows value) => element.SetValue(PopupShadowsProperty, value);

    #endregion
}
