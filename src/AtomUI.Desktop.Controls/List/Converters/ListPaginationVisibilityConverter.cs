using System.Globalization;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Converters;

internal class ListPaginationVisibilityConverter : IValueConverter
{
    public bool IsTop { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ListPaginationVisibility paginationVisibility)
        {
            if (paginationVisibility == ListPaginationVisibility.None)
            {
                return false;
            }

            if (IsTop)
            {
                return paginationVisibility.HasFlag(ListPaginationVisibility.Top);
            }
            
            return paginationVisibility.HasFlag(ListPaginationVisibility.Bottom);
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}