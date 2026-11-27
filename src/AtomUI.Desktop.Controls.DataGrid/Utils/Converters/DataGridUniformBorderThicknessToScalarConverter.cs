using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Converters;

/// <summary>
/// Converts a <see cref="Thickness" /> into a single scalar by returning its left edge, for bindings that
/// expect one value for a uniform border width. Any other value is returned unchanged, and the conversion
/// cannot be reversed.
/// </summary>
public class DataGridUniformBorderThicknessToScalarConverter : IValueConverter
{
    /// <summary>
    /// Returns the left edge of a <see cref="Thickness" /> value, and any other value unchanged.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // TODO Uniform thickness: decide whether Convert must require the four edges of the Thickness to be equal
        // and what to return when they are not; today the left edge is returned regardless.
        // Remove this TODO when that decision is implemented.
        if (value is Thickness thickness)
        {
            return thickness.Left;
        }
        return value;
    }

    /// <summary>
    /// Throws <see cref="NotImplementedException" />; the converter works in one direction only.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Always thrown.
    /// </exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}