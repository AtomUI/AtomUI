using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inherited style inputs; each public terminal supplies its own values in AXAML.
internal sealed class ColorPickerAttachedProperties
{
    #region 公共属性定义

    public static readonly AttachedProperty<double> SpectrumMinWidthProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("SpectrumMinWidth", inherits: true);

    public static readonly AttachedProperty<double> SpectrumHeightProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("SpectrumHeight", inherits: true);

    public static readonly AttachedProperty<BoxShadows> InsetShadowsProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, BoxShadows>("InsetShadows", inherits: true);

    public static readonly AttachedProperty<double> OuterSpectrumHandleSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("OuterSpectrumHandleSize", inherits: true);

    public static readonly AttachedProperty<double> SpectrumHandleSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("SpectrumHandleSize", inherits: true);

    public static readonly AttachedProperty<IBrush?> SpectrumHandleLightBrushProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, IBrush?>("SpectrumHandleLightBrush", inherits: true);

    public static readonly AttachedProperty<IBrush?> SpectrumHandleDarkBrushProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, IBrush?>("SpectrumHandleDarkBrush", inherits: true);

    public static readonly AttachedProperty<double> SliderHeightProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("SliderHeight", inherits: true);

    public static readonly AttachedProperty<double> SliderTrackHeightProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("SliderTrackHeight", inherits: true);

    public static readonly AttachedProperty<double> SliderThumbSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("SliderThumbSize", inherits: true);

    public static readonly AttachedProperty<double> PreviewSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("PreviewSize", inherits: true);

    public static readonly AttachedProperty<double> AlphaInputWidthProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("AlphaInputWidth", inherits: true);

    public static readonly AttachedProperty<double> NumberInputSpinnerHandleWidthProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("NumberInputSpinnerHandleWidth", inherits: true);

    public static readonly AttachedProperty<double> PresetColorSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("PresetColorSize", inherits: true);

    public static readonly AttachedProperty<double> PresetPanelWidthProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("PresetPanelWidth", inherits: true);

    public static readonly AttachedProperty<double> CheckedMarkSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("CheckedMarkSize", inherits: true);

    public static readonly AttachedProperty<Thickness> PresetColorGroupPaddingProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, Thickness>("PresetColorGroupPadding", inherits: true);

    public static readonly AttachedProperty<BoxShadows> ColorBlockInnerShadowsProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, BoxShadows>("ColorBlockInnerShadows", inherits: true);

    public static readonly AttachedProperty<Thickness> TriggerPaddingProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, Thickness>("TriggerPadding", inherits: true);

    public static readonly AttachedProperty<Thickness> TriggerTextMarginProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, Thickness>("TriggerTextMargin", inherits: true);

    public static readonly AttachedProperty<Thickness> SliderContainerMarginProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, Thickness>("SliderContainerMargin", inherits: true);

    public static readonly AttachedProperty<double> CheckerboardTileSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("CheckerboardTileSize", inherits: true);

    public static readonly AttachedProperty<double> DisabledColorBlockOpacityProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("DisabledColorBlockOpacity", inherits: true);

    public static readonly AttachedProperty<double> EditorFontSizeProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, double>("EditorFontSize", inherits: true);

    public static readonly AttachedProperty<Thickness> SeparatorThicknessProperty =
        AvaloniaProperty.RegisterAttached<ColorPickerAttachedProperties, StyledElement, Thickness>("SeparatorThickness", inherits: true);

    #endregion

    private ColorPickerAttachedProperties() { }

    #region 公共方法定义

    public static double GetSpectrumMinWidth(StyledElement element) => element.GetValue(SpectrumMinWidthProperty);
    public static void SetSpectrumMinWidth(StyledElement element, double value) => element.SetValue(SpectrumMinWidthProperty, value);
    public static double GetSpectrumHeight(StyledElement element) => element.GetValue(SpectrumHeightProperty);
    public static void SetSpectrumHeight(StyledElement element, double value) => element.SetValue(SpectrumHeightProperty, value);
    public static BoxShadows GetInsetShadows(StyledElement element) => element.GetValue(InsetShadowsProperty);
    public static void SetInsetShadows(StyledElement element, BoxShadows value) => element.SetValue(InsetShadowsProperty, value);
    public static double GetOuterSpectrumHandleSize(StyledElement element) => element.GetValue(OuterSpectrumHandleSizeProperty);
    public static void SetOuterSpectrumHandleSize(StyledElement element, double value) => element.SetValue(OuterSpectrumHandleSizeProperty, value);
    public static double GetSpectrumHandleSize(StyledElement element) => element.GetValue(SpectrumHandleSizeProperty);
    public static void SetSpectrumHandleSize(StyledElement element, double value) => element.SetValue(SpectrumHandleSizeProperty, value);
    public static IBrush? GetSpectrumHandleLightBrush(StyledElement element) => element.GetValue(SpectrumHandleLightBrushProperty);
    public static void SetSpectrumHandleLightBrush(StyledElement element, IBrush? value) => element.SetValue(SpectrumHandleLightBrushProperty, value);
    public static IBrush? GetSpectrumHandleDarkBrush(StyledElement element) => element.GetValue(SpectrumHandleDarkBrushProperty);
    public static void SetSpectrumHandleDarkBrush(StyledElement element, IBrush? value) => element.SetValue(SpectrumHandleDarkBrushProperty, value);
    public static double GetSliderHeight(StyledElement element) => element.GetValue(SliderHeightProperty);
    public static void SetSliderHeight(StyledElement element, double value) => element.SetValue(SliderHeightProperty, value);
    public static double GetSliderTrackHeight(StyledElement element) => element.GetValue(SliderTrackHeightProperty);
    public static void SetSliderTrackHeight(StyledElement element, double value) => element.SetValue(SliderTrackHeightProperty, value);
    public static double GetSliderThumbSize(StyledElement element) => element.GetValue(SliderThumbSizeProperty);
    public static void SetSliderThumbSize(StyledElement element, double value) => element.SetValue(SliderThumbSizeProperty, value);
    public static double GetPreviewSize(StyledElement element) => element.GetValue(PreviewSizeProperty);
    public static void SetPreviewSize(StyledElement element, double value) => element.SetValue(PreviewSizeProperty, value);
    public static double GetAlphaInputWidth(StyledElement element) => element.GetValue(AlphaInputWidthProperty);
    public static void SetAlphaInputWidth(StyledElement element, double value) => element.SetValue(AlphaInputWidthProperty, value);
    public static double GetNumberInputSpinnerHandleWidth(StyledElement element) => element.GetValue(NumberInputSpinnerHandleWidthProperty);
    public static void SetNumberInputSpinnerHandleWidth(StyledElement element, double value) => element.SetValue(NumberInputSpinnerHandleWidthProperty, value);
    public static double GetPresetColorSize(StyledElement element) => element.GetValue(PresetColorSizeProperty);
    public static void SetPresetColorSize(StyledElement element, double value) => element.SetValue(PresetColorSizeProperty, value);
    public static double GetPresetPanelWidth(StyledElement element) => element.GetValue(PresetPanelWidthProperty);
    public static void SetPresetPanelWidth(StyledElement element, double value) => element.SetValue(PresetPanelWidthProperty, value);
    public static double GetCheckedMarkSize(StyledElement element) => element.GetValue(CheckedMarkSizeProperty);
    public static void SetCheckedMarkSize(StyledElement element, double value) => element.SetValue(CheckedMarkSizeProperty, value);
    public static Thickness GetPresetColorGroupPadding(StyledElement element) => element.GetValue(PresetColorGroupPaddingProperty);
    public static void SetPresetColorGroupPadding(StyledElement element, Thickness value) => element.SetValue(PresetColorGroupPaddingProperty, value);
    public static BoxShadows GetColorBlockInnerShadows(StyledElement element) => element.GetValue(ColorBlockInnerShadowsProperty);
    public static void SetColorBlockInnerShadows(StyledElement element, BoxShadows value) => element.SetValue(ColorBlockInnerShadowsProperty, value);
    public static Thickness GetTriggerPadding(StyledElement element) => element.GetValue(TriggerPaddingProperty);
    public static void SetTriggerPadding(StyledElement element, Thickness value) => element.SetValue(TriggerPaddingProperty, value);
    public static Thickness GetTriggerTextMargin(StyledElement element) => element.GetValue(TriggerTextMarginProperty);
    public static void SetTriggerTextMargin(StyledElement element, Thickness value) => element.SetValue(TriggerTextMarginProperty, value);
    public static Thickness GetSliderContainerMargin(StyledElement element) => element.GetValue(SliderContainerMarginProperty);
    public static void SetSliderContainerMargin(StyledElement element, Thickness value) => element.SetValue(SliderContainerMarginProperty, value);
    public static double GetCheckerboardTileSize(StyledElement element) => element.GetValue(CheckerboardTileSizeProperty);
    public static void SetCheckerboardTileSize(StyledElement element, double value) => element.SetValue(CheckerboardTileSizeProperty, value);
    public static double GetDisabledColorBlockOpacity(StyledElement element) => element.GetValue(DisabledColorBlockOpacityProperty);
    public static void SetDisabledColorBlockOpacity(StyledElement element, double value) => element.SetValue(DisabledColorBlockOpacityProperty, value);
    public static double GetEditorFontSize(StyledElement element) => element.GetValue(EditorFontSizeProperty);
    public static void SetEditorFontSize(StyledElement element, double value) => element.SetValue(EditorFontSizeProperty, value);
    public static Thickness GetSeparatorThickness(StyledElement element) => element.GetValue(SeparatorThicknessProperty);
    public static void SetSeparatorThickness(StyledElement element, Thickness value) => element.SetValue(SeparatorThicknessProperty, value);

    #endregion
}
