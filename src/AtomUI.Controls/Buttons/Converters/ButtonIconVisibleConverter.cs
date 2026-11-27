using System.Globalization;
using Avalonia.Data.Converters;

namespace AtomUI.Controls.Converters;

/// <summary>
/// Converts the icon, the icon visibility flag and the loading flag of a button into one value that reports
/// whether the icon is shown.
/// </summary>
public class ButtonIconVisibleConverter : IMultiValueConverter
{
    /// <summary>
    /// Returns whether a button shows its icon.
    /// </summary>
    /// <param name="values">
    /// The bound values, in this order: the icon, the flag that requests the icon to be visible,
    /// and the flag that marks the button as loading.
    /// </param>
    /// <param name="targetType">
    /// The type of the binding target. It is not used.
    /// </param>
    /// <param name="parameter">
    /// The converter parameter. It is not used.
    /// </param>
    /// <param name="culture">
    /// The culture of the conversion. It is not used.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when exactly three values are passed, the first is an <see cref="Icon" />, the
    /// second is <see langword="true" /> and the third is not <see langword="true" />. Otherwise
    /// <see langword="false" />. For either flag, <see langword="null" />, an unset value or a value of another
    /// type counts as not <see langword="true" />.
    /// </returns>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3)
        {
            return false;
        }
        var icon        = values[0] as Icon;
        var iconVisible = values[1] as bool?;
        var isLoading = values[2] as bool?;
        return icon != null && iconVisible == true && isLoading != true;
    }
}