using Avalonia;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class DatePickerPresenter
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> PanelContentPaddingProperty =
        AvaloniaProperty.RegisterAttached<DatePickerPresenter, StyledElement, Thickness>("PanelContentPadding", inherits: true);

    public static Thickness GetPanelContentPadding(StyledElement element) => element.GetValue(PanelContentPaddingProperty);
    public static void SetPanelContentPadding(StyledElement element, Thickness value) => element.SetValue(PanelContentPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ButtonsPanelMarginProperty =
        AvaloniaProperty.RegisterAttached<DatePickerPresenter, StyledElement, Thickness>("ButtonsPanelMargin", inherits: true);

    public static Thickness GetButtonsPanelMargin(StyledElement element) => element.GetValue(ButtonsPanelMarginProperty);
    public static void SetButtonsPanelMargin(StyledElement element, Thickness value) => element.SetValue(ButtonsPanelMarginProperty, value);

    public static readonly AttachedProperty<Thickness> HeaderBorderThicknessProperty =
        AvaloniaProperty.RegisterAttached<DatePickerPresenter, StyledElement, Thickness>("HeaderBorderThickness", inherits: true);

    public static Thickness GetHeaderBorderThickness(StyledElement element) => element.GetValue(HeaderBorderThicknessProperty);
    public static void SetHeaderBorderThickness(StyledElement element, Thickness value) => element.SetValue(HeaderBorderThicknessProperty, value);

    #endregion
}
