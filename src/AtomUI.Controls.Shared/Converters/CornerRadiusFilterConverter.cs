using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Controls.Converters;

/// <summary>
/// Keeps the enabled corners of a <see cref="CornerRadius" /> and replaces the other corners with 0.
/// All four flags default to <see langword="true" />, which passes the radius through unchanged.
/// </summary>
public class CornerRadiusFilterConverter : IValueConverter
{
    /// <summary>
    /// Keeps the top-left corner of the radius when <see langword="true" />; replaces it with 0 otherwise.
    /// </summary>
    public bool TopLeft { get; set; } = true;

    /// <summary>
    /// Keeps the top-right corner of the radius when <see langword="true" />; replaces it with 0 otherwise.
    /// </summary>
    public bool TopRight { get; set; } = true;

    /// <summary>
    /// Keeps the bottom-left corner of the radius when <see langword="true" />; replaces it with 0 otherwise.
    /// </summary>
    public bool BottomLeft { get; set; } = true;

    /// <summary>
    /// Keeps the bottom-right corner of the radius when <see langword="true" />; replaces it with 0 otherwise.
    /// </summary>
    public bool BottomRight { get; set; } = true;
    
    /// <summary>
    /// Returns a <see cref="CornerRadius" /> that keeps the enabled corners of <paramref name="value" /> and has 0
    /// on every other corner.
    /// </summary>
    /// <param name="value">
    /// The source radius. A value of any other type, including <see langword="null" />, yields a zero radius.
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
        if (value is CornerRadius cornerRadius)
        {
            return new CornerRadius(
                TopLeft ? cornerRadius.TopLeft : 0,
                TopRight? cornerRadius.TopRight : 0, 
                BottomRight ? cornerRadius.BottomRight : 0, 
                BottomLeft ? cornerRadius.BottomLeft : 0);
        }
        return new CornerRadius(0);
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