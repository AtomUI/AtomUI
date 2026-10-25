using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

internal static class ColorPickerInputThemeBindings
{
    // An unresolved DynamicResource masks the preceding Width setter. Preserve the
    // rendered 48 DIP default, rather than the historically unused 44 DIP token.
    public static readonly FuncMultiValueConverter<object, double> AlphaWidth =
        new(values => values.Count == 1 && values[0] is double width ? width : 48);

    // A wider visible Alpha input must not overlap the fixed-width RGB/HSV fields.
    public static readonly FuncMultiValueConverter<object, double> InputWidth =
        new(values => values.Count == 2 && values[0] is double width && values[1] is true
            ? 262 + Math.Max(0, width - 48) : 262);
}
