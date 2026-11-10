using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class TimePickerPresenter
{
    #region 公共属性定义

    public static readonly AttachedProperty<Thickness> ThemeButtonsMarginProperty =
        AvaloniaProperty.RegisterAttached<TimePickerPresenter, StyledElement, Thickness>("ThemeButtonsMargin", inherits: true);

    public static Thickness GetThemeButtonsMargin(StyledElement element) => element.GetValue(ThemeButtonsMarginProperty);
    public static void SetThemeButtonsMargin(StyledElement element, Thickness value) => element.SetValue(ThemeButtonsMarginProperty, value);

    #endregion
}
