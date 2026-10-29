using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

internal static class CascaderThemeBindings
{
    // XAML cannot resolve the internal registration field directly. Keep the
    // existing property object and the binding in ControlTheme.
    public static readonly AvaloniaProperty PopupHeightBudgetProperty = AbstractSelect.MaxPopupHeightProperty;

    public static readonly IMultiValueConverter PopupMaxHeight =
        new FuncMultiValueConverter<object?, object>(static (IReadOnlyList<object?> values) =>
            values.Count == 2 && values[0] is double budget && values[1] is double cap
                ? Math.Min(budget, cap)
                : BindingOperations.DoNothing);
}
