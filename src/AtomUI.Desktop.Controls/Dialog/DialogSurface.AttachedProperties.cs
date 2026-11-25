using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class DialogSurface
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> HeaderBackgroundProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, IBrush?>("HeaderBackground", inherits: true);

    public static IBrush? GetHeaderBackground(StyledElement element) => element.GetValue(HeaderBackgroundProperty);
    public static void SetHeaderBackground(StyledElement element, IBrush? value) => element.SetValue(HeaderBackgroundProperty, value);

    public static readonly AttachedProperty<Thickness> HeaderBottomMarginProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("HeaderBottomMargin", inherits: true);

    public static Thickness GetHeaderBottomMargin(StyledElement element) => element.GetValue(HeaderBottomMarginProperty);
    public static void SetHeaderBottomMargin(StyledElement element, Thickness value) => element.SetValue(HeaderBottomMarginProperty, value);

    public static readonly AttachedProperty<double> LogoSizeProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("LogoSize", inherits: true);

    public static double GetLogoSize(StyledElement element) => element.GetValue(LogoSizeProperty);
    public static void SetLogoSize(StyledElement element, double value) => element.SetValue(LogoSizeProperty, value);

    public static readonly AttachedProperty<double> HeaderFontSizeProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("HeaderFontSize", inherits: true);

    public static double GetHeaderFontSize(StyledElement element) => element.GetValue(HeaderFontSizeProperty);
    public static void SetHeaderFontSize(StyledElement element, double value) => element.SetValue(HeaderFontSizeProperty, value);

    public static readonly AttachedProperty<IBrush?> HeaderForegroundProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, IBrush?>("HeaderForeground", inherits: true);

    public static IBrush? GetHeaderForeground(StyledElement element) => element.GetValue(HeaderForegroundProperty);
    public static void SetHeaderForeground(StyledElement element, IBrush? value) => element.SetValue(HeaderForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> ContentBackgroundProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, IBrush?>("ContentBackground", inherits: true);

    public static IBrush? GetContentBackground(StyledElement element) => element.GetValue(ContentBackgroundProperty);
    public static void SetContentBackground(StyledElement element, IBrush? value) => element.SetValue(ContentBackgroundProperty, value);

    public static readonly AttachedProperty<Thickness> HeaderPaddingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("HeaderPadding", inherits: true);

    public static Thickness GetHeaderPadding(StyledElement element) => element.GetValue(HeaderPaddingProperty);
    public static void SetHeaderPadding(StyledElement element, Thickness value) => element.SetValue(HeaderPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("ContentPadding", inherits: true);

    public static Thickness GetContentPadding(StyledElement element) => element.GetValue(ContentPaddingProperty);
    public static void SetContentPadding(StyledElement element, Thickness value) => element.SetValue(ContentPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> WindowContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("WindowContentPadding", inherits: true);

    public static Thickness GetWindowContentPadding(StyledElement element) => element.GetValue(WindowContentPaddingProperty);
    public static void SetWindowContentPadding(StyledElement element, Thickness value) => element.SetValue(WindowContentPaddingProperty, value);

    public static readonly AttachedProperty<IBrush?> FooterBackgroundProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, IBrush?>("FooterBackground", inherits: true);

    public static IBrush? GetFooterBackground(StyledElement element) => element.GetValue(FooterBackgroundProperty);
    public static void SetFooterBackground(StyledElement element, IBrush? value) => element.SetValue(FooterBackgroundProperty, value);

    public static readonly AttachedProperty<Thickness> FooterPaddingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("FooterPadding", inherits: true);

    public static Thickness GetFooterPadding(StyledElement element) => element.GetValue(FooterPaddingProperty);
    public static void SetFooterPadding(StyledElement element, Thickness value) => element.SetValue(FooterPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> FooterTopMarginProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("FooterTopMargin", inherits: true);

    public static Thickness GetFooterTopMargin(StyledElement element) => element.GetValue(FooterTopMarginProperty);
    public static void SetFooterTopMargin(StyledElement element, Thickness value) => element.SetValue(FooterTopMarginProperty, value);

    public static readonly AttachedProperty<double> CloseButtonSizeProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("CloseButtonSize", inherits: true);

    public static double GetCloseButtonSize(StyledElement element) => element.GetValue(CloseButtonSizeProperty);
    public static void SetCloseButtonSize(StyledElement element, double value) => element.SetValue(CloseButtonSizeProperty, value);

    public static readonly AttachedProperty<double> DefaultViewportMinHeightProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("DefaultViewportMinHeight", inherits: true);

    public static double GetDefaultViewportMinHeight(StyledElement element) => element.GetValue(DefaultViewportMinHeightProperty);
    public static void SetDefaultViewportMinHeight(StyledElement element, double value) => element.SetValue(DefaultViewportMinHeightProperty, value);

    public static readonly AttachedProperty<double> DefaultViewportMinWidthProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("DefaultViewportMinWidth", inherits: true);

    public static double GetDefaultViewportMinWidth(StyledElement element) => element.GetValue(DefaultViewportMinWidthProperty);
    public static void SetDefaultViewportMinWidth(StyledElement element, double value) => element.SetValue(DefaultViewportMinWidthProperty, value);

    public static readonly AttachedProperty<double> ButtonGroupSpacingProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, double>("ButtonGroupSpacing", inherits: true);

    public static double GetButtonGroupSpacing(StyledElement element) => element.GetValue(ButtonGroupSpacingProperty);
    public static void SetButtonGroupSpacing(StyledElement element, double value) => element.SetValue(ButtonGroupSpacingProperty, value);

    public static readonly AttachedProperty<Thickness> LoadingIndicatorMarginProperty =
        AvaloniaProperty.RegisterAttached<DialogSurface, StyledElement, Thickness>("LoadingIndicatorMargin", inherits: true);

    public static Thickness GetLoadingIndicatorMargin(StyledElement element) => element.GetValue(LoadingIndicatorMarginProperty);
    public static void SetLoadingIndicatorMargin(StyledElement element, Thickness value) => element.SetValue(LoadingIndicatorMarginProperty, value);

    #endregion
}
