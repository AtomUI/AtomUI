using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class TabOverflowEdgeIndicator
{
    #region 公共属性定义

    public static readonly AttachedProperty<double> ThemeTitleFontSizeProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeTitleFontSize", inherits: true);

    public static double GetThemeTitleFontSize(StyledElement element) => element.GetValue(ThemeTitleFontSizeProperty);
    public static void SetThemeTitleFontSize(StyledElement element, double value) => element.SetValue(ThemeTitleFontSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeTitleFontSizeLGProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeTitleFontSizeLG", inherits: true);

    public static double GetThemeTitleFontSizeLG(StyledElement element) => element.GetValue(ThemeTitleFontSizeLGProperty);
    public static void SetThemeTitleFontSizeLG(StyledElement element, double value) => element.SetValue(ThemeTitleFontSizeLGProperty, value);

    public static readonly AttachedProperty<double> ThemeTitleFontSizeSMProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeTitleFontSizeSM", inherits: true);

    public static double GetThemeTitleFontSizeSM(StyledElement element) => element.GetValue(ThemeTitleFontSizeSMProperty);
    public static void SetThemeTitleFontSizeSM(StyledElement element, double value) => element.SetValue(ThemeTitleFontSizeSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHorizontalItemMarginProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeHorizontalItemMargin", inherits: true);

    public static Thickness GetThemeHorizontalItemMargin(StyledElement element) => element.GetValue(ThemeHorizontalItemMarginProperty);
    public static void SetThemeHorizontalItemMargin(StyledElement element, Thickness value) => element.SetValue(ThemeHorizontalItemMarginProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemColorProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, IBrush?>("ThemeItemColor", inherits: true);

    public static IBrush? GetThemeItemColor(StyledElement element) => element.GetValue(ThemeItemColorProperty);
    public static void SetThemeItemColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemHoverColorProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, IBrush?>("ThemeItemHoverColor", inherits: true);

    public static IBrush? GetThemeItemHoverColor(StyledElement element) => element.GetValue(ThemeItemHoverColorProperty);
    public static void SetThemeItemHoverColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemHoverColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemSelectedColorProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, IBrush?>("ThemeItemSelectedColor", inherits: true);

    public static IBrush? GetThemeItemSelectedColor(StyledElement element) => element.GetValue(ThemeItemSelectedColorProperty);
    public static void SetThemeItemSelectedColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemSelectedColorProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeItemIconMarginProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeItemIconMargin", inherits: true);

    public static Thickness GetThemeItemIconMargin(StyledElement element) => element.GetValue(ThemeItemIconMarginProperty);
    public static void SetThemeItemIconMargin(StyledElement element, Thickness value) => element.SetValue(ThemeItemIconMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMenuIndicatorPaddingHorizontalProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeMenuIndicatorPaddingHorizontal", inherits: true);

    public static Thickness GetThemeMenuIndicatorPaddingHorizontal(StyledElement element) => element.GetValue(ThemeMenuIndicatorPaddingHorizontalProperty);
    public static void SetThemeMenuIndicatorPaddingHorizontal(StyledElement element, Thickness value) => element.SetValue(ThemeMenuIndicatorPaddingHorizontalProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMenuIndicatorPaddingVerticalProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeMenuIndicatorPaddingVertical", inherits: true);

    public static Thickness GetThemeMenuIndicatorPaddingVertical(StyledElement element) => element.GetValue(ThemeMenuIndicatorPaddingVerticalProperty);
    public static void SetThemeMenuIndicatorPaddingVertical(StyledElement element, Thickness value) => element.SetValue(ThemeMenuIndicatorPaddingVerticalProperty, value);


    public static readonly AttachedProperty<BoxShadows> ThemeBoxShadowTabsOverflowLeftProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, BoxShadows>("ThemeBoxShadowTabsOverflowLeft", inherits: true);

    public static BoxShadows GetThemeBoxShadowTabsOverflowLeft(StyledElement element) => element.GetValue(ThemeBoxShadowTabsOverflowLeftProperty);
    public static void SetThemeBoxShadowTabsOverflowLeft(StyledElement element, BoxShadows value) => element.SetValue(ThemeBoxShadowTabsOverflowLeftProperty, value);

    public static readonly AttachedProperty<BoxShadows> ThemeBoxShadowTabsOverflowRightProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, BoxShadows>("ThemeBoxShadowTabsOverflowRight", inherits: true);

    public static BoxShadows GetThemeBoxShadowTabsOverflowRight(StyledElement element) => element.GetValue(ThemeBoxShadowTabsOverflowRightProperty);
    public static void SetThemeBoxShadowTabsOverflowRight(StyledElement element, BoxShadows value) => element.SetValue(ThemeBoxShadowTabsOverflowRightProperty, value);

    public static readonly AttachedProperty<BoxShadows> ThemeBoxShadowTabsOverflowTopProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, BoxShadows>("ThemeBoxShadowTabsOverflowTop", inherits: true);

    public static BoxShadows GetThemeBoxShadowTabsOverflowTop(StyledElement element) => element.GetValue(ThemeBoxShadowTabsOverflowTopProperty);
    public static void SetThemeBoxShadowTabsOverflowTop(StyledElement element, BoxShadows value) => element.SetValue(ThemeBoxShadowTabsOverflowTopProperty, value);

    public static readonly AttachedProperty<BoxShadows> ThemeBoxShadowTabsOverflowBottomProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, BoxShadows>("ThemeBoxShadowTabsOverflowBottom", inherits: true);

    public static BoxShadows GetThemeBoxShadowTabsOverflowBottom(StyledElement element) => element.GetValue(ThemeBoxShadowTabsOverflowBottomProperty);
    public static void SetThemeBoxShadowTabsOverflowBottom(StyledElement element, BoxShadows value) => element.SetValue(ThemeBoxShadowTabsOverflowBottomProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeCloseIconMarginProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeCloseIconMargin", inherits: true);

    public static Thickness GetThemeCloseIconMargin(StyledElement element) => element.GetValue(ThemeCloseIconMarginProperty);
    public static void SetThemeCloseIconMargin(StyledElement element, Thickness value) => element.SetValue(ThemeCloseIconMarginProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeInkBarColorProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, IBrush?>("ThemeInkBarColor", inherits: true);

    public static IBrush? GetThemeInkBarColor(StyledElement element) => element.GetValue(ThemeInkBarColorProperty);
    public static void SetThemeInkBarColor(StyledElement element, IBrush? value) => element.SetValue(ThemeInkBarColorProperty, value);

    public static readonly AttachedProperty<double> ThemeInkBarThicknessProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeInkBarThickness", inherits: true);

    public static double GetThemeInkBarThickness(StyledElement element) => element.GetValue(ThemeInkBarThicknessProperty);
    public static void SetThemeInkBarThickness(StyledElement element, double value) => element.SetValue(ThemeInkBarThicknessProperty, value);

    public static readonly AttachedProperty<double> ThemeHorizontalItemGutterProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeHorizontalItemGutter", inherits: true);

    public static double GetThemeHorizontalItemGutter(StyledElement element) => element.GetValue(ThemeHorizontalItemGutterProperty);
    public static void SetThemeHorizontalItemGutter(StyledElement element, double value) => element.SetValue(ThemeHorizontalItemGutterProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHorizontalItemPaddingProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeHorizontalItemPadding", inherits: true);

    public static Thickness GetThemeHorizontalItemPadding(StyledElement element) => element.GetValue(ThemeHorizontalItemPaddingProperty);
    public static void SetThemeHorizontalItemPadding(StyledElement element, Thickness value) => element.SetValue(ThemeHorizontalItemPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHorizontalItemPaddingLGProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeHorizontalItemPaddingLG", inherits: true);

    public static Thickness GetThemeHorizontalItemPaddingLG(StyledElement element) => element.GetValue(ThemeHorizontalItemPaddingLGProperty);
    public static void SetThemeHorizontalItemPaddingLG(StyledElement element, Thickness value) => element.SetValue(ThemeHorizontalItemPaddingLGProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHorizontalItemPaddingSMProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeHorizontalItemPaddingSM", inherits: true);

    public static Thickness GetThemeHorizontalItemPaddingSM(StyledElement element) => element.GetValue(ThemeHorizontalItemPaddingSMProperty);
    public static void SetThemeHorizontalItemPaddingSM(StyledElement element, Thickness value) => element.SetValue(ThemeHorizontalItemPaddingSMProperty, value);

    public static readonly AttachedProperty<double> ThemeVerticalItemGutterProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeVerticalItemGutter", inherits: true);

    public static double GetThemeVerticalItemGutter(StyledElement element) => element.GetValue(ThemeVerticalItemGutterProperty);
    public static void SetThemeVerticalItemGutter(StyledElement element, double value) => element.SetValue(ThemeVerticalItemGutterProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeCardBgProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, IBrush?>("ThemeCardBg", inherits: true);

    public static IBrush? GetThemeCardBg(StyledElement element) => element.GetValue(ThemeCardBgProperty);
    public static void SetThemeCardBg(StyledElement element, IBrush? value) => element.SetValue(ThemeCardBgProperty, value);

    public static readonly AttachedProperty<double> ThemeCardSizeProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeCardSize", inherits: true);

    public static double GetThemeCardSize(StyledElement element) => element.GetValue(ThemeCardSizeProperty);
    public static void SetThemeCardSize(StyledElement element, double value) => element.SetValue(ThemeCardSizeProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeCardPaddingProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeCardPadding", inherits: true);

    public static Thickness GetThemeCardPadding(StyledElement element) => element.GetValue(ThemeCardPaddingProperty);
    public static void SetThemeCardPadding(StyledElement element, Thickness value) => element.SetValue(ThemeCardPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeCardPaddingSMProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeCardPaddingSM", inherits: true);

    public static Thickness GetThemeCardPaddingSM(StyledElement element) => element.GetValue(ThemeCardPaddingSMProperty);
    public static void SetThemeCardPaddingSM(StyledElement element, Thickness value) => element.SetValue(ThemeCardPaddingSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeCardPaddingLGProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeCardPaddingLG", inherits: true);

    public static Thickness GetThemeCardPaddingLG(StyledElement element) => element.GetValue(ThemeCardPaddingLGProperty);
    public static void SetThemeCardPaddingLG(StyledElement element, Thickness value) => element.SetValue(ThemeCardPaddingLGProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeVerticalItemPaddingProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeVerticalItemPadding", inherits: true);

    public static Thickness GetThemeVerticalItemPadding(StyledElement element) => element.GetValue(ThemeVerticalItemPaddingProperty);
    public static void SetThemeVerticalItemPadding(StyledElement element, Thickness value) => element.SetValue(ThemeVerticalItemPaddingProperty, value);

    public static readonly AttachedProperty<double> ThemeCardGutterProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeCardGutter", inherits: true);

    public static double GetThemeCardGutter(StyledElement element) => element.GetValue(ThemeCardGutterProperty);
    public static void SetThemeCardGutter(StyledElement element, double value) => element.SetValue(ThemeCardGutterProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeAddTabButtonMarginHorizontalProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeAddTabButtonMarginHorizontal", inherits: true);

    public static Thickness GetThemeAddTabButtonMarginHorizontal(StyledElement element) => element.GetValue(ThemeAddTabButtonMarginHorizontalProperty);
    public static void SetThemeAddTabButtonMarginHorizontal(StyledElement element, Thickness value) => element.SetValue(ThemeAddTabButtonMarginHorizontalProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeAddTabButtonMarginVerticalProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, Thickness>("ThemeAddTabButtonMarginVertical", inherits: true);

    public static Thickness GetThemeAddTabButtonMarginVertical(StyledElement element) => element.GetValue(ThemeAddTabButtonMarginVerticalProperty);
    public static void SetThemeAddTabButtonMarginVertical(StyledElement element, Thickness value) => element.SetValue(ThemeAddTabButtonMarginVerticalProperty, value);

    public static readonly AttachedProperty<double> ThemeTabAndContentGutterProperty =
        AvaloniaProperty.RegisterAttached<TabOverflowEdgeIndicator, StyledElement, double>("ThemeTabAndContentGutter", inherits: true);

    public static double GetThemeTabAndContentGutter(StyledElement element) => element.GetValue(ThemeTabAndContentGutterProperty);
    public static void SetThemeTabAndContentGutter(StyledElement element, double value) => element.SetValue(ThemeTabAndContentGutterProperty, value);

    #endregion
}
