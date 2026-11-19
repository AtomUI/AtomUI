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

    public static readonly AttachedProperty<Thickness> ThemeContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, Thickness>("ThemeContentPadding", inherits: true);
    public static Thickness GetThemeContentPadding(StyledElement element) => element.GetValue(ThemeContentPaddingProperty);
    public static void SetThemeContentPadding(StyledElement element, Thickness value) => element.SetValue(ThemeContentPaddingProperty, value);

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

    public static readonly AttachedProperty<double> ThemeOnlyIconSizeProperty =
        AvaloniaProperty.RegisterAttached<ButtonThemeInputs, StyledElement, double>("ThemeOnlyIconSize", inherits: true);
    public static double GetThemeOnlyIconSize(StyledElement element) => element.GetValue(ThemeOnlyIconSizeProperty);
    public static void SetThemeOnlyIconSize(StyledElement element, double value) => element.SetValue(ThemeOnlyIconSizeProperty, value);

}
