using System.Globalization;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Converters;

public class DataGridPaginationVisibilityConvertor : IValueConverter
{
    public bool IsTop { get; set; }
    
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
                return paginationVisibility.HasFlag(DataGridPaginationVisibility.Top);
            }
            
            return paginationVisibility.HasFlag(DataGridPaginationVisibility.Bottom);
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}