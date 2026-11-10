using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class DialogSurface
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> ThemeHeaderBgProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, IBrush?>("ThemeHeaderBg", inherits: true);

    public static IBrush? GetThemeHeaderBg(StyledElement element) => element.GetValue(ThemeHeaderBgProperty);
    public static void SetThemeHeaderBg(StyledElement element, IBrush? value) => element.SetValue(ThemeHeaderBgProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHeaderMarginBottomProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("ThemeHeaderMarginBottom", inherits: true);

    public static Thickness GetThemeHeaderMarginBottom(StyledElement element) => element.GetValue(ThemeHeaderMarginBottomProperty);
    public static void SetThemeHeaderMarginBottom(StyledElement element, Thickness value) => element.SetValue(ThemeHeaderMarginBottomProperty, value);

    public static readonly AttachedProperty<double> ThemeLogoSizeProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("ThemeLogoSize", inherits: true);

    public static double GetThemeLogoSize(StyledElement element) => element.GetValue(ThemeLogoSizeProperty);
    public static void SetThemeLogoSize(StyledElement element, double value) => element.SetValue(ThemeLogoSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeHeaderFontSizeProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("ThemeHeaderFontSize", inherits: true);

    public static double GetThemeHeaderFontSize(StyledElement element) => element.GetValue(ThemeHeaderFontSizeProperty);
    public static void SetThemeHeaderFontSize(StyledElement element, double value) => element.SetValue(ThemeHeaderFontSizeProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeHeaderColorProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, IBrush?>("ThemeHeaderColor", inherits: true);

    public static IBrush? GetThemeHeaderColor(StyledElement element) => element.GetValue(ThemeHeaderColorProperty);
    public static void SetThemeHeaderColor(StyledElement element, IBrush? value) => element.SetValue(ThemeHeaderColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeContentBgProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, IBrush?>("ThemeContentBg", inherits: true);

    public static IBrush? GetThemeContentBg(StyledElement element) => element.GetValue(ThemeContentBgProperty);
    public static void SetThemeContentBg(StyledElement element, IBrush? value) => element.SetValue(ThemeContentBgProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHeaderPaddingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("ThemeHeaderPadding", inherits: true);

    public static Thickness GetThemeHeaderPadding(StyledElement element) => element.GetValue(ThemeHeaderPaddingProperty);
    public static void SetThemeHeaderPadding(StyledElement element, Thickness value) => element.SetValue(ThemeHeaderPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("ThemeContentPadding", inherits: true);

    public static Thickness GetThemeContentPadding(StyledElement element) => element.GetValue(ThemeContentPaddingProperty);
    public static void SetThemeContentPadding(StyledElement element, Thickness value) => element.SetValue(ThemeContentPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeWindowContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("ThemeWindowContentPadding", inherits: true);

    public static Thickness GetThemeWindowContentPadding(StyledElement element) => element.GetValue(ThemeWindowContentPaddingProperty);
    public static void SetThemeWindowContentPadding(StyledElement element, Thickness value) => element.SetValue(ThemeWindowContentPaddingProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeFooterBgProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, IBrush?>("ThemeFooterBg", inherits: true);

    public static IBrush? GetThemeFooterBg(StyledElement element) => element.GetValue(ThemeFooterBgProperty);
    public static void SetThemeFooterBg(StyledElement element, IBrush? value) => element.SetValue(ThemeFooterBgProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeFooterPaddingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("ThemeFooterPadding", inherits: true);

    public static Thickness GetThemeFooterPadding(StyledElement element) => element.GetValue(ThemeFooterPaddingProperty);
    public static void SetThemeFooterPadding(StyledElement element, Thickness value) => element.SetValue(ThemeFooterPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeFooterMarginTopProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("ThemeFooterMarginTop", inherits: true);

    public static Thickness GetThemeFooterMarginTop(StyledElement element) => element.GetValue(ThemeFooterMarginTopProperty);
    public static void SetThemeFooterMarginTop(StyledElement element, Thickness value) => element.SetValue(ThemeFooterMarginTopProperty, value);

    public static readonly AttachedProperty<double> ThemeCloseBtnSizeProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("ThemeCloseBtnSize", inherits: true);

    public static double GetThemeCloseBtnSize(StyledElement element) => element.GetValue(ThemeCloseBtnSizeProperty);
    public static void SetThemeCloseBtnSize(StyledElement element, double value) => element.SetValue(ThemeCloseBtnSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeMinHeightProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("ThemeMinHeight", inherits: true);

    public static double GetThemeMinHeight(StyledElement element) => element.GetValue(ThemeMinHeightProperty);
    public static void SetThemeMinHeight(StyledElement element, double value) => element.SetValue(ThemeMinHeightProperty, value);

    public static readonly AttachedProperty<double> ThemeMinWidthProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("ThemeMinWidth", inherits: true);

    public static double GetThemeMinWidth(StyledElement element) => element.GetValue(ThemeMinWidthProperty);
    public static void SetThemeMinWidth(StyledElement element, double value) => element.SetValue(ThemeMinWidthProperty, value);

    public static readonly AttachedProperty<double> ThemeButtonGroupSpacingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("ThemeButtonGroupSpacing", inherits: true);

    public static double GetThemeButtonGroupSpacing(StyledElement element) => element.GetValue(ThemeButtonGroupSpacingProperty);
    public static void SetThemeButtonGroupSpacing(StyledElement element, double value) => element.SetValue(ThemeButtonGroupSpacingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeLoadingIndicatorMarginProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("ThemeLoadingIndicatorMargin", inherits: true);

    public static Thickness GetThemeLoadingIndicatorMargin(StyledElement element) => element.GetValue(ThemeLoadingIndicatorMarginProperty);
    public static void SetThemeLoadingIndicatorMargin(StyledElement element, Thickness value) => element.SetValue(ThemeLoadingIndicatorMarginProperty, value);

    #endregion
}
