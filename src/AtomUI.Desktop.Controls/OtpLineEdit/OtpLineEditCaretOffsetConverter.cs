using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

internal sealed class OtpLineEditCaretOffsetConverter : IMultiValueConverter
{
    public static OtpLineEditCaretOffsetConverter Instance { get; } = new();

    private OtpLineEditCaretOffsetConverter()
    {
    }

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || values[1] is not double fontSize ||
            (values[0] is not null && values[0] is not string))
        {
            return AvaloniaProperty.UnsetValue;
        }

        // Preserve the existing digit-advance approximation and default position.
        return values[0] is string { Length: > 0 } ? fontSize * 0.30 : 0d;
    }
}
