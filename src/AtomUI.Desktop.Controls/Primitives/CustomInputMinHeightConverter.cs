using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Retains the input's nominal minimum until the owner supplies a smaller height budget.
/// </summary>
internal sealed class CustomInputMinHeightConverter : IMultiValueConverter
{
    public static CustomInputMinHeightConverter Instance { get; } = new();

    private CustomInputMinHeightConverter()
    {
    }

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 4 || values[0] is not double preset || values[1] is not double height ||
            values[2] is not double minimum || values[3] is not double maximum)
        {
            return AvaloniaProperty.UnsetValue;
        }
        var budget = double.IsNaN(height) ? maximum : Math.Min(height, maximum);
        return Math.Max(minimum, Math.Min(preset, budget));
    }
}
