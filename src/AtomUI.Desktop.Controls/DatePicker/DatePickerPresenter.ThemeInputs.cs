using Avalonia;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class DatePickerPresenter
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> ThemePanelContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<DatePickerPresenter, StyledElement, Thickness>("ThemePanelContentPadding", inherits: true);

    public static Thickness GetThemePanelContentPadding(StyledElement element) => element.GetValue(ThemePanelContentPaddingProperty);
    public static void SetThemePanelContentPadding(StyledElement element, Thickness value) => element.SetValue(ThemePanelContentPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeButtonsPanelMarginProperty =
        AvaloniaProperty.RegisterAttached<DatePickerPresenter, StyledElement, Thickness>("ThemeButtonsPanelMargin", inherits: true);

    public static Thickness GetThemeButtonsPanelMargin(StyledElement element) => element.GetValue(ThemeButtonsPanelMarginProperty);
    public static void SetThemeButtonsPanelMargin(StyledElement element, Thickness value) => element.SetValue(ThemeButtonsPanelMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHeaderBorderThicknessProperty =
        AvaloniaProperty.RegisterAttached<DatePickerPresenter, StyledElement, Thickness>("ThemeHeaderBorderThickness", inherits: true);

    public static Thickness GetThemeHeaderBorderThickness(StyledElement element) => element.GetValue(ThemeHeaderBorderThicknessProperty);
    public static void SetThemeHeaderBorderThickness(StyledElement element, Thickness value) => element.SetValue(ThemeHeaderBorderThicknessProperty, value);

    #endregion
}
