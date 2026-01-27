using AtomUI.Desktop.Controls.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

internal class ListTheme : ControlTheme
{
    public static readonly ListPaginationVisibilityConverter TopPaginationVisibilityConverter =
        new()
        {
            IsTop = true
        };
    
    public static readonly ListPaginationVisibilityConverter BottomPaginationVisibilityConverter =
        new()
        {
            IsTop = false
        };
}