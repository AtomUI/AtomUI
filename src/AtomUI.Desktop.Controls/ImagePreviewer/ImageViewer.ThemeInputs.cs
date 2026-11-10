using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class ImageViewer
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> ThemeColorBgLayoutProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeColorBgLayout", inherits: true);

    public static IBrush? GetThemeColorBgLayout(StyledElement element) => element.GetValue(ThemeColorBgLayoutProperty);
    public static void SetThemeColorBgLayout(StyledElement element, IBrush? value) => element.SetValue(ThemeColorBgLayoutProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeColorBgMaskProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeColorBgMask", inherits: true);

    public static IBrush? GetThemeColorBgMask(StyledElement element) => element.GetValue(ThemeColorBgMaskProperty);
    public static void SetThemeColorBgMask(StyledElement element, IBrush? value) => element.SetValue(ThemeColorBgMaskProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeColorBgTextActiveProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeColorBgTextActive", inherits: true);

    public static IBrush? GetThemeColorBgTextActive(StyledElement element) => element.GetValue(ThemeColorBgTextActiveProperty);
    public static void SetThemeColorBgTextActive(StyledElement element, IBrush? value) => element.SetValue(ThemeColorBgTextActiveProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeColorBgTextHoverProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeColorBgTextHover", inherits: true);

    public static IBrush? GetThemeColorBgTextHover(StyledElement element) => element.GetValue(ThemeColorBgTextHoverProperty);
    public static void SetThemeColorBgTextHover(StyledElement element, IBrush? value) => element.SetValue(ThemeColorBgTextHoverProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeColorFillTertiaryProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeColorFillTertiary", inherits: true);

    public static IBrush? GetThemeColorFillTertiary(StyledElement element) => element.GetValue(ThemeColorFillTertiaryProperty);
    public static void SetThemeColorFillTertiary(StyledElement element, IBrush? value) => element.SetValue(ThemeColorFillTertiaryProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeColorTextLightSolidProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeColorTextLightSolid", inherits: true);

    public static IBrush? GetThemeColorTextLightSolid(StyledElement element) => element.GetValue(ThemeColorTextLightSolidProperty);
    public static void SetThemeColorTextLightSolid(StyledElement element, IBrush? value) => element.SetValue(ThemeColorTextLightSolidProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeColorTextTertiaryProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeColorTextTertiary", inherits: true);

    public static IBrush? GetThemeColorTextTertiary(StyledElement element) => element.GetValue(ThemeColorTextTertiaryProperty);
    public static void SetThemeColorTextTertiary(StyledElement element, IBrush? value) => element.SetValue(ThemeColorTextTertiaryProperty, value);

    public static readonly AttachedProperty<double> ThemeControlHeightSMProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeControlHeightSM", inherits: true);

    public static double GetThemeControlHeightSM(StyledElement element) => element.GetValue(ThemeControlHeightSMProperty);
    public static void SetThemeControlHeightSM(StyledElement element, double value) => element.SetValue(ThemeControlHeightSMProperty, value);

    public static readonly AttachedProperty<double> ThemeCoverImageWidthProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeCoverImageWidth", inherits: true);

    public static double GetThemeCoverImageWidth(StyledElement element) => element.GetValue(ThemeCoverImageWidthProperty);
    public static void SetThemeCoverImageWidth(StyledElement element, double value) => element.SetValue(ThemeCoverImageWidthProperty, value);

    public static readonly AttachedProperty<double> ThemeDialogMinHeightProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeDialogMinHeight", inherits: true);

    public static double GetThemeDialogMinHeight(StyledElement element) => element.GetValue(ThemeDialogMinHeightProperty);
    public static void SetThemeDialogMinHeight(StyledElement element, double value) => element.SetValue(ThemeDialogMinHeightProperty, value);

    public static readonly AttachedProperty<double> ThemeDialogMinWidthProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeDialogMinWidth", inherits: true);

    public static double GetThemeDialogMinWidth(StyledElement element) => element.GetValue(ThemeDialogMinWidthProperty);
    public static void SetThemeDialogMinWidth(StyledElement element, double value) => element.SetValue(ThemeDialogMinWidthProperty, value);

    public static readonly AttachedProperty<bool> ThemeEnableMotionProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, bool>("ThemeEnableMotion", inherits: true);

    public static bool GetThemeEnableMotion(StyledElement element) => element.GetValue(ThemeEnableMotionProperty);
    public static void SetThemeEnableMotion(StyledElement element, bool value) => element.SetValue(ThemeEnableMotionProperty, value);

    public static readonly AttachedProperty<double> ThemeErrorIconSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeErrorIconSize", inherits: true);

    public static double GetThemeErrorIconSize(StyledElement element) => element.GetValue(ThemeErrorIconSizeProperty);
    public static void SetThemeErrorIconSize(StyledElement element, double value) => element.SetValue(ThemeErrorIconSizeProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeFloatToolbarIndicatorPaddingProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("ThemeFloatToolbarIndicatorPadding", inherits: true);

    public static Thickness GetThemeFloatToolbarIndicatorPadding(StyledElement element) => element.GetValue(ThemeFloatToolbarIndicatorPaddingProperty);
    public static void SetThemeFloatToolbarIndicatorPadding(StyledElement element, Thickness value) => element.SetValue(ThemeFloatToolbarIndicatorPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeFloatToolbarPaddingProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("ThemeFloatToolbarPadding", inherits: true);

    public static Thickness GetThemeFloatToolbarPadding(StyledElement element) => element.GetValue(ThemeFloatToolbarPaddingProperty);
    public static void SetThemeFloatToolbarPadding(StyledElement element, Thickness value) => element.SetValue(ThemeFloatToolbarPaddingProperty, value);

    public static readonly AttachedProperty<double> ThemeIconSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeIconSize", inherits: true);

    public static double GetThemeIconSize(StyledElement element) => element.GetValue(ThemeIconSizeProperty);
    public static void SetThemeIconSize(StyledElement element, double value) => element.SetValue(ThemeIconSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeIconSizeLGProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeIconSizeLG", inherits: true);

    public static double GetThemeIconSizeLG(StyledElement element) => element.GetValue(ThemeIconSizeLGProperty);
    public static void SetThemeIconSizeLG(StyledElement element, double value) => element.SetValue(ThemeIconSizeLGProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMarginSMProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("ThemeMarginSM", inherits: true);

    public static Thickness GetThemeMarginSM(StyledElement element) => element.GetValue(ThemeMarginSMProperty);
    public static void SetThemeMarginSM(StyledElement element, Thickness value) => element.SetValue(ThemeMarginSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeMarginXLProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("ThemeMarginXL", inherits: true);

    public static Thickness GetThemeMarginXL(StyledElement element) => element.GetValue(ThemeMarginXLProperty);
    public static void SetThemeMarginXL(StyledElement element, Thickness value) => element.SetValue(ThemeMarginXLProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeMaskBgColorProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeMaskBgColor", inherits: true);

    public static IBrush? GetThemeMaskBgColor(StyledElement element) => element.GetValue(ThemeMaskBgColorProperty);
    public static void SetThemeMaskBgColor(StyledElement element, IBrush? value) => element.SetValue(ThemeMaskBgColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeNavButtonBgColorProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeNavButtonBgColor", inherits: true);

    public static IBrush? GetThemeNavButtonBgColor(StyledElement element) => element.GetValue(ThemeNavButtonBgColorProperty);
    public static void SetThemeNavButtonBgColor(StyledElement element, IBrush? value) => element.SetValue(ThemeNavButtonBgColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeNavButtonBgHoverColorProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeNavButtonBgHoverColor", inherits: true);

    public static IBrush? GetThemeNavButtonBgHoverColor(StyledElement element) => element.GetValue(ThemeNavButtonBgHoverColorProperty);
    public static void SetThemeNavButtonBgHoverColor(StyledElement element, IBrush? value) => element.SetValue(ThemeNavButtonBgHoverColorProperty, value);

    public static readonly AttachedProperty<Thickness> ThemePaddingSMProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("ThemePaddingSM", inherits: true);

    public static Thickness GetThemePaddingSM(StyledElement element) => element.GetValue(ThemePaddingSMProperty);
    public static void SetThemePaddingSM(StyledElement element, Thickness value) => element.SetValue(ThemePaddingSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemePaddingXSProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("ThemePaddingXS", inherits: true);

    public static Thickness GetThemePaddingXS(StyledElement element) => element.GetValue(ThemePaddingXSProperty);
    public static void SetThemePaddingXS(StyledElement element, Thickness value) => element.SetValue(ThemePaddingXSProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemePreviewOperationColorProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemePreviewOperationColor", inherits: true);

    public static IBrush? GetThemePreviewOperationColor(StyledElement element) => element.GetValue(ThemePreviewOperationColorProperty);
    public static void SetThemePreviewOperationColor(StyledElement element, IBrush? value) => element.SetValue(ThemePreviewOperationColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemePreviewOperationColorDisabledProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemePreviewOperationColorDisabled", inherits: true);

    public static IBrush? GetThemePreviewOperationColorDisabled(StyledElement element) => element.GetValue(ThemePreviewOperationColorDisabledProperty);
    public static void SetThemePreviewOperationColorDisabled(StyledElement element, IBrush? value) => element.SetValue(ThemePreviewOperationColorDisabledProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemePreviewOperationHoverColorProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemePreviewOperationHoverColor", inherits: true);

    public static IBrush? GetThemePreviewOperationHoverColor(StyledElement element) => element.GetValue(ThemePreviewOperationHoverColorProperty);
    public static void SetThemePreviewOperationHoverColor(StyledElement element, IBrush? value) => element.SetValue(ThemePreviewOperationHoverColorProperty, value);

    public static readonly AttachedProperty<double> ThemePreviewOperationSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemePreviewOperationSize", inherits: true);

    public static double GetThemePreviewOperationSize(StyledElement element) => element.GetValue(ThemePreviewOperationSizeProperty);
    public static void SetThemePreviewOperationSize(StyledElement element, double value) => element.SetValue(ThemePreviewOperationSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeSpacingProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeSpacing", inherits: true);

    public static double GetThemeSpacing(StyledElement element) => element.GetValue(ThemeSpacingProperty);
    public static void SetThemeSpacing(StyledElement element, double value) => element.SetValue(ThemeSpacingProperty, value);

    public static readonly AttachedProperty<double> ThemeSpacingXXSProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ThemeSpacingXXS", inherits: true);

    public static double GetThemeSpacingXXS(StyledElement element) => element.GetValue(ThemeSpacingXXSProperty);
    public static void SetThemeSpacingXXS(StyledElement element, double value) => element.SetValue(ThemeSpacingXXSProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeTitleBarBackgroundColorProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ThemeTitleBarBackgroundColor", inherits: true);

    public static IBrush? GetThemeTitleBarBackgroundColor(StyledElement element) => element.GetValue(ThemeTitleBarBackgroundColorProperty);
    public static void SetThemeTitleBarBackgroundColor(StyledElement element, IBrush? value) => element.SetValue(ThemeTitleBarBackgroundColorProperty, value);

    #endregion
}
