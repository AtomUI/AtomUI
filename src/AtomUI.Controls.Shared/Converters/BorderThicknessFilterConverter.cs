using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Controls.Converters;

/// <summary>
/// Keeps the enabled sides of a <see cref="Thickness" /> and replaces the other sides with 0.
/// All four flags default to <see langword="true" />, which passes the thickness through unchanged.
/// </summary>
public class BorderThicknessFilterConverter : IValueConverter
{
    /// <summary>
    /// Keeps the left side of the thickness when <see langword="true" />; replaces it with 0 otherwise.
    /// </summary>
    public bool Left { get; set; } = true;

    /// <summary>
    /// Keeps the top side of the thickness when <see langword="true" />; replaces it with 0 otherwise.
    /// </summary>
    public bool Top { get; set; } = true;

    /// <summary>
    /// Keeps the right side of the thickness when <see langword="true" />; replaces it with 0 otherwise.
    /// </summary>
    public bool Right { get; set; } = true;

    /// <summary>
    /// Keeps the bottom side of the thickness when <see langword="true" />; replaces it with 0 otherwise.
    /// </summary>
    public bool Bottom { get; set; } = true;

    /// <summary>
    /// Returns a <see cref="Thickness" /> that keeps the enabled sides of <paramref name="value" /> and has 0 on
    /// every other side.
    /// </summary>
    /// <param name="value">
    /// The source thickness. A value of any other type, including <see langword="null" />, yields a zero thickness.
    /// </param>
    /// <param name="targetType">
    /// Not used.
    /// </param>
    /// <param name="parameter">
    /// Not used.
    /// </param>
    /// <param name="culture">
    /// Not used.
    /// </param>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Thickness thickness)
        {
            return new Thickness(
                Left ? thickness.Left : 0,
                Top ? thickness.Top : 0,
                Right ? thickness.Right : 0,
                Bottom ? thickness.Bottom : 0);
        }
        return new Thickness(0);
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown on every call.
    /// </exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}