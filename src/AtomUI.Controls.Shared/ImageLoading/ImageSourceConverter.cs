using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// Converts a string to an <see cref="ImageSource" />, so that a string can be assigned to a source in XAML.
/// </summary>
public sealed class ImageSourceConverter : TypeConverter
{
    /// <summary>
    /// Returns whether an object of type <paramref name="sourceType" /> can be converted to an
    /// <see cref="ImageSource" />.
    /// </summary>
    /// <param name="context">
    /// The format context of the conversion.
    /// </param>
    /// <param name="sourceType">
    /// The type to convert from.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="sourceType" /> is <see cref="string" /> or the base converter
    /// accepts it; otherwise <see langword="false" />.
    /// </returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    /// <summary>
    /// Converts <paramref name="value" /> to an <see cref="ImageSource" /> with <see cref="ImageSource.Parse" />
    /// when it is a string.
    /// </summary>
    /// <param name="context">
    /// The format context of the conversion.
    /// </param>
    /// <param name="culture">
    /// The culture of the conversion.
    /// </param>
    /// <param name="value">
    /// The value to convert.
    /// </param>
    /// <returns>
    /// The converted source, or the result of the base converter for any other type.
    /// </returns>
    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        return value is string source
            ? ImageSource.Parse(source)
            : base.ConvertFrom(context, culture, value);
    }
}
