using System.Globalization;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Converters;

/// <summary>
/// Converts a <see cref="DataGridPaginationVisibility" /> value into the visibility of the pager above or
/// the pager below the grid, for binding to <c>IsVisible</c>.
/// </summary>
public class DataGridPaginationVisibilityConvertor : IValueConverter
{
    /// <summary>
    /// Selects the pager this instance reports on: <see langword="true" /> for the pager above the grid,
    /// <see langword="false" /> for the pager below it.
    /// </summary>
    public bool IsTop { get; set; }
    
    /// <summary>
    /// Returns whether the pager selected by <see cref="IsTop" /> is included in the visibility value.
    /// </summary>
    /// <param name="value">
    /// The <see cref="DataGridPaginationVisibility" /> value to read. Any other value, including
    /// <see langword="null" />, yields <see langword="false" />.
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
    /// <returns>
    /// <see langword="true" /> when <paramref name="value" /> has the <c>Top</c> flag and <see cref="IsTop" /> is
    /// <see langword="true" />, or has the <c>Bottom</c> flag and <see cref="IsTop" /> is
    /// <see langword="false" />; otherwise <see langword="false" />.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DataGridPaginationVisibility paginationVisibility)
        {
            if (paginationVisibility == DataGridPaginationVisibility.None)
            {
                return false;
            }

            if (IsTop)
            {
                return (paginationVisibility & DataGridPaginationVisibility.Top) == DataGridPaginationVisibility.Top;
            }
            
            return (paginationVisibility & DataGridPaginationVisibility.Bottom) == DataGridPaginationVisibility.Bottom;
        }
        return false;
    }

    /// <summary>
    /// Is not implemented; the converter works in the source-to-target direction only.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Always thrown.
    /// </exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
