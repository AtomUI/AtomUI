using Avalonia;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class TimePickerPresenter
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> ButtonsMarginProperty =
        AvaloniaProperty.RegisterAttached<TimePickerPresenter, StyledElement, Thickness>("ButtonsMargin", inherits: true);

    public static Thickness GetButtonsMargin(StyledElement element) => element.GetValue(ButtonsMarginProperty);
    public static void SetButtonsMargin(StyledElement element, Thickness value) => element.SetValue(ButtonsMarginProperty, value);

    #endregion
}
