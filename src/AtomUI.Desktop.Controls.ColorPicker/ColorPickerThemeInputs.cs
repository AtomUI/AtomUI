using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inherited style inputs; each public terminal supplies its own values in AXAML.
internal sealed class ColorPickerThemeInputs
{
    #region 公共属性定义

    public static readonly AttachedProperty<double> ThemeColorPickerWidthProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerWidth", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorSpectrumHeightProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorSpectrumHeight", inherits: true);

    public static readonly AttachedProperty<BoxShadows> ThemeColorPickerInsetShadowProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, BoxShadows>("ThemeColorPickerInsetShadow", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerHandlerSizeLGProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerHandlerSizeLG", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerHandlerSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerHandlerSize", inherits: true);

    public static readonly AttachedProperty<IBrush?> ThemeColorPickerHandlerLightColorProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, IBrush?>("ThemeColorPickerHandlerLightColor", inherits: true);

    public static readonly AttachedProperty<IBrush?> ThemeColorPickerHandlerDarkColorProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, IBrush?>("ThemeColorPickerHandlerDarkColor", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerSliderSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerSliderSize", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerSliderTrackSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerSliderTrackSize", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerSliderThumbSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerSliderThumbSize", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerPreviewSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerPreviewSize", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerAlphaInputWidthProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerAlphaInputWidth", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerInputNumberHandleWidthProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerInputNumberHandleWidth", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerPresetColorSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerPresetColorSize", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorPickerPresetPanelWidthProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorPickerPresetPanelWidth", inherits: true);

    public static readonly AttachedProperty<double> ThemeCheckedMarkSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeCheckedMarkSize", inherits: true);

    public static readonly AttachedProperty<Thickness> ThemeColorPickerPresetColorGroupPaddingProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, Thickness>("ThemeColorPickerPresetColorGroupPadding", inherits: true);

    public static readonly AttachedProperty<BoxShadows> ThemeColorBlockInnerShadowsProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, BoxShadows>("ThemeColorBlockInnerShadows", inherits: true);

    public static readonly AttachedProperty<Thickness> ThemeTriggerPaddingProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, Thickness>("ThemeTriggerPadding", inherits: true);

    public static readonly AttachedProperty<Thickness> ThemeTriggerTextMarginProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, Thickness>("ThemeTriggerTextMargin", inherits: true);

    public static readonly AttachedProperty<Thickness> ThemeSliderContainerMarginProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, Thickness>("ThemeSliderContainerMargin", inherits: true);

    public static readonly AttachedProperty<double> ThemeTransparentBgSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeTransparentBgSize", inherits: true);

    public static readonly AttachedProperty<double> ThemeColorBlockDisabledOpacityProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeColorBlockDisabledOpacity", inherits: true);

    public static readonly AttachedProperty<double> ThemeEditorFontSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, double>("ThemeEditorFontSize", inherits: true);

    public static readonly AttachedProperty<Thickness> ThemeSeparatorThicknessProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerThemeInputs, StyledElement, Thickness>("ThemeSeparatorThickness", inherits: true);

    #endregion

    private ColorPickerThemeInputs() { }

    #region 公共方法定义

    public static double GetThemeColorPickerWidth(StyledElement element) => element.GetValue(ThemeColorPickerWidthProperty);
    public static void SetThemeColorPickerWidth(StyledElement element, double value) => element.SetValue(ThemeColorPickerWidthProperty, value);
    public static double GetThemeColorSpectrumHeight(StyledElement element) => element.GetValue(ThemeColorSpectrumHeightProperty);
    public static void SetThemeColorSpectrumHeight(StyledElement element, double value) => element.SetValue(ThemeColorSpectrumHeightProperty, value);
    public static BoxShadows GetThemeColorPickerInsetShadow(StyledElement element) => element.GetValue(ThemeColorPickerInsetShadowProperty);
    public static void SetThemeColorPickerInsetShadow(StyledElement element, BoxShadows value) => element.SetValue(ThemeColorPickerInsetShadowProperty, value);
    public static double GetThemeColorPickerHandlerSizeLG(StyledElement element) => element.GetValue(ThemeColorPickerHandlerSizeLGProperty);
    public static void SetThemeColorPickerHandlerSizeLG(StyledElement element, double value) => element.SetValue(ThemeColorPickerHandlerSizeLGProperty, value);
    public static double GetThemeColorPickerHandlerSize(StyledElement element) => element.GetValue(ThemeColorPickerHandlerSizeProperty);
    public static void SetThemeColorPickerHandlerSize(StyledElement element, double value) => element.SetValue(ThemeColorPickerHandlerSizeProperty, value);
    public static IBrush? GetThemeColorPickerHandlerLightColor(StyledElement element) => element.GetValue(ThemeColorPickerHandlerLightColorProperty);
    public static void SetThemeColorPickerHandlerLightColor(StyledElement element, IBrush? value) => element.SetValue(ThemeColorPickerHandlerLightColorProperty, value);
    public static IBrush? GetThemeColorPickerHandlerDarkColor(StyledElement element) => element.GetValue(ThemeColorPickerHandlerDarkColorProperty);
    public static void SetThemeColorPickerHandlerDarkColor(StyledElement element, IBrush? value) => element.SetValue(ThemeColorPickerHandlerDarkColorProperty, value);
    public static double GetThemeColorPickerSliderSize(StyledElement element) => element.GetValue(ThemeColorPickerSliderSizeProperty);
    public static void SetThemeColorPickerSliderSize(StyledElement element, double value) => element.SetValue(ThemeColorPickerSliderSizeProperty, value);
    public static double GetThemeColorPickerSliderTrackSize(StyledElement element) => element.GetValue(ThemeColorPickerSliderTrackSizeProperty);
    public static void SetThemeColorPickerSliderTrackSize(StyledElement element, double value) => element.SetValue(ThemeColorPickerSliderTrackSizeProperty, value);
    public static double GetThemeColorPickerSliderThumbSize(StyledElement element) => element.GetValue(ThemeColorPickerSliderThumbSizeProperty);
    public static void SetThemeColorPickerSliderThumbSize(StyledElement element, double value) => element.SetValue(ThemeColorPickerSliderThumbSizeProperty, value);
    public static double GetThemeColorPickerPreviewSize(StyledElement element) => element.GetValue(ThemeColorPickerPreviewSizeProperty);
    public static void SetThemeColorPickerPreviewSize(StyledElement element, double value) => element.SetValue(ThemeColorPickerPreviewSizeProperty, value);
    public static double GetThemeColorPickerAlphaInputWidth(StyledElement element) => element.GetValue(ThemeColorPickerAlphaInputWidthProperty);
    public static void SetThemeColorPickerAlphaInputWidth(StyledElement element, double value) => element.SetValue(ThemeColorPickerAlphaInputWidthProperty, value);
    public static double GetThemeColorPickerInputNumberHandleWidth(StyledElement element) => element.GetValue(ThemeColorPickerInputNumberHandleWidthProperty);
    public static void SetThemeColorPickerInputNumberHandleWidth(StyledElement element, double value) => element.SetValue(ThemeColorPickerInputNumberHandleWidthProperty, value);
    public static double GetThemeColorPickerPresetColorSize(StyledElement element) => element.GetValue(ThemeColorPickerPresetColorSizeProperty);
    public static void SetThemeColorPickerPresetColorSize(StyledElement element, double value) => element.SetValue(ThemeColorPickerPresetColorSizeProperty, value);
    public static double GetThemeColorPickerPresetPanelWidth(StyledElement element) => element.GetValue(ThemeColorPickerPresetPanelWidthProperty);
    public static void SetThemeColorPickerPresetPanelWidth(StyledElement element, double value) => element.SetValue(ThemeColorPickerPresetPanelWidthProperty, value);
    public static double GetThemeCheckedMarkSize(StyledElement element) => element.GetValue(ThemeCheckedMarkSizeProperty);
    public static void SetThemeCheckedMarkSize(StyledElement element, double value) => element.SetValue(ThemeCheckedMarkSizeProperty, value);
    public static Thickness GetThemeColorPickerPresetColorGroupPadding(StyledElement element) => element.GetValue(ThemeColorPickerPresetColorGroupPaddingProperty);
    public static void SetThemeColorPickerPresetColorGroupPadding(StyledElement element, Thickness value) => element.SetValue(ThemeColorPickerPresetColorGroupPaddingProperty, value);
    public static BoxShadows GetThemeColorBlockInnerShadows(StyledElement element) => element.GetValue(ThemeColorBlockInnerShadowsProperty);
    public static void SetThemeColorBlockInnerShadows(StyledElement element, BoxShadows value) => element.SetValue(ThemeColorBlockInnerShadowsProperty, value);
    public static Thickness GetThemeTriggerPadding(StyledElement element) => element.GetValue(ThemeTriggerPaddingProperty);
    public static void SetThemeTriggerPadding(StyledElement element, Thickness value) => element.SetValue(ThemeTriggerPaddingProperty, value);
    public static Thickness GetThemeTriggerTextMargin(StyledElement element) => element.GetValue(ThemeTriggerTextMarginProperty);
    public static void SetThemeTriggerTextMargin(StyledElement element, Thickness value) => element.SetValue(ThemeTriggerTextMarginProperty, value);
    public static Thickness GetThemeSliderContainerMargin(StyledElement element) => element.GetValue(ThemeSliderContainerMarginProperty);
    public static void SetThemeSliderContainerMargin(StyledElement element, Thickness value) => element.SetValue(ThemeSliderContainerMarginProperty, value);
    public static double GetThemeTransparentBgSize(StyledElement element) => element.GetValue(ThemeTransparentBgSizeProperty);
    public static void SetThemeTransparentBgSize(StyledElement element, double value) => element.SetValue(ThemeTransparentBgSizeProperty, value);
    public static double GetThemeColorBlockDisabledOpacity(StyledElement element) => element.GetValue(ThemeColorBlockDisabledOpacityProperty);
    public static void SetThemeColorBlockDisabledOpacity(StyledElement element, double value) => element.SetValue(ThemeColorBlockDisabledOpacityProperty, value);
    public static double GetThemeEditorFontSize(StyledElement element) => element.GetValue(ThemeEditorFontSizeProperty);
    public static void SetThemeEditorFontSize(StyledElement element, double value) => element.SetValue(ThemeEditorFontSizeProperty, value);
    public static Thickness GetThemeSeparatorThickness(StyledElement element) => element.GetValue(ThemeSeparatorThicknessProperty);
    public static void SetThemeSeparatorThickness(StyledElement element, Thickness value) => element.SetValue(ThemeSeparatorThicknessProperty, value);

    #endregion
}
