using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal sealed class ButtonThemeInputs
{
    private ButtonThemeInputs() { }

    public static readonly AttachedProperty<IBrush?> ThemeBorderColorDisabledProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, IBrush?>("ThemeBorderColorDisabled", inherits: true);
    public static IBrush? GetThemeBorderColorDisabled(StyledElement element) => element.GetValue(ThemeBorderColorDisabledProperty);
    public static void SetThemeBorderColorDisabled(StyledElement element, IBrush? value) => element.SetValue(ThemeBorderColorDisabledProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeCirclePaddingProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeCirclePadding", inherits: true);
    public static Thickness GetThemeCirclePadding(StyledElement element) => element.GetValue(ThemeCirclePaddingProperty);
    public static void SetThemeCirclePadding(StyledElement element, Thickness value) => element.SetValue(ThemeCirclePaddingProperty, value);

    public static readonly AttachedProperty<double> ThemeContentFontSizeProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, double>("ThemeContentFontSize", inherits: true);
    public static double GetThemeContentFontSize(StyledElement element) => element.GetValue(ThemeContentFontSizeProperty);
    public static void SetThemeContentFontSize(StyledElement element, double value) => element.SetValue(ThemeContentFontSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeContentFontSizeLGProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, double>("ThemeContentFontSizeLG", inherits: true);
    public static double GetThemeContentFontSizeLG(StyledElement element) => element.GetValue(ThemeContentFontSizeLGProperty);
    public static void SetThemeContentFontSizeLG(StyledElement element, double value) => element.SetValue(ThemeContentFontSizeLGProperty, value);

    public static readonly AttachedProperty<double> ThemeContentFontSizeSMProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, double>("ThemeContentFontSizeSM", inherits: true);
    public static double GetThemeContentFontSizeSM(StyledElement element) => element.GetValue(ThemeContentFontSizeSMProperty);
    public static void SetThemeContentFontSizeSM(StyledElement element, double value) => element.SetValue(ThemeContentFontSizeSMProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeContentPadding", inherits: true);
    public static Thickness GetThemeContentPadding(StyledElement element) => element.GetValue(ThemeContentPaddingProperty);
    public static void SetThemeContentPadding(StyledElement element, Thickness value) => element.SetValue(ThemeContentPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeContentPaddingLGProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeContentPaddingLG", inherits: true);
    public static Thickness GetThemeContentPaddingLG(StyledElement element) => element.GetValue(ThemeContentPaddingLGProperty);
    public static void SetThemeContentPaddingLG(StyledElement element, Thickness value) => element.SetValue(ThemeContentPaddingLGProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeContentPaddingSMProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeContentPaddingSM", inherits: true);
    public static Thickness GetThemeContentPaddingSM(StyledElement element) => element.GetValue(ThemeContentPaddingSMProperty);
    public static void SetThemeContentPaddingSM(StyledElement element, Thickness value) => element.SetValue(ThemeContentPaddingSMProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeDefaultBorderColorDisabledProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, IBrush?>("ThemeDefaultBorderColorDisabled", inherits: true);
    public static IBrush? GetThemeDefaultBorderColorDisabled(StyledElement element) => element.GetValue(ThemeDefaultBorderColorDisabledProperty);
    public static void SetThemeDefaultBorderColorDisabled(StyledElement element, IBrush? value) => element.SetValue(ThemeDefaultBorderColorDisabledProperty, value);

    public static readonly AttachedProperty<FontWeight> ThemeFontWeightProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, FontWeight>("ThemeFontWeight", inherits: true);
    public static FontWeight GetThemeFontWeight(StyledElement element) => element.GetValue(ThemeFontWeightProperty);
    public static void SetThemeFontWeight(StyledElement element, FontWeight value) => element.SetValue(ThemeFontWeightProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeIconEndMarginProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeIconEndMargin", inherits: true);
    public static Thickness GetThemeIconEndMargin(StyledElement element) => element.GetValue(ThemeIconEndMarginProperty);
    public static void SetThemeIconEndMargin(StyledElement element, Thickness value) => element.SetValue(ThemeIconEndMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeIconMarginProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeIconMargin", inherits: true);
    public static Thickness GetThemeIconMargin(StyledElement element) => element.GetValue(ThemeIconMarginProperty);
    public static void SetThemeIconMargin(StyledElement element, Thickness value) => element.SetValue(ThemeIconMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeIconOnyPaddingProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeIconOnyPadding", inherits: true);
    public static Thickness GetThemeIconOnyPadding(StyledElement element) => element.GetValue(ThemeIconOnyPaddingProperty);
    public static void SetThemeIconOnyPadding(StyledElement element, Thickness value) => element.SetValue(ThemeIconOnyPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeIconOnyPaddingLGProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeIconOnyPaddingLG", inherits: true);
    public static Thickness GetThemeIconOnyPaddingLG(StyledElement element) => element.GetValue(ThemeIconOnyPaddingLGProperty);
    public static void SetThemeIconOnyPaddingLG(StyledElement element, Thickness value) => element.SetValue(ThemeIconOnyPaddingLGProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeIconOnyPaddingSMProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeIconOnyPaddingSM", inherits: true);
    public static Thickness GetThemeIconOnyPaddingSM(StyledElement element) => element.GetValue(ThemeIconOnyPaddingSMProperty);
    public static void SetThemeIconOnyPaddingSM(StyledElement element, Thickness value) => element.SetValue(ThemeIconOnyPaddingSMProperty, value);

    public static readonly AttachedProperty<double> ThemeOnlyIconSizeProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, double>("ThemeOnlyIconSize", inherits: true);
    public static double GetThemeOnlyIconSize(StyledElement element) => element.GetValue(ThemeOnlyIconSizeProperty);
    public static void SetThemeOnlyIconSize(StyledElement element, double value) => element.SetValue(ThemeOnlyIconSizeProperty, value);

    public static readonly AttachedProperty<double> ThemeOnlyIconSizeLGProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, double>("ThemeOnlyIconSizeLG", inherits: true);
    public static double GetThemeOnlyIconSizeLG(StyledElement element) => element.GetValue(ThemeOnlyIconSizeLGProperty);
    public static void SetThemeOnlyIconSizeLG(StyledElement element, double value) => element.SetValue(ThemeOnlyIconSizeLGProperty, value);

    public static readonly AttachedProperty<double> ThemeOnlyIconSizeSMProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, double>("ThemeOnlyIconSizeSM", inherits: true);
    public static double GetThemeOnlyIconSizeSM(StyledElement element) => element.GetValue(ThemeOnlyIconSizeSMProperty);
    public static void SetThemeOnlyIconSizeSM(StyledElement element, double value) => element.SetValue(ThemeOnlyIconSizeSMProperty, value);

}
