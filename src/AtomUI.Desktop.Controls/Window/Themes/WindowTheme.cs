using Avalonia.Controls.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

internal class WindowTheme : ControlTheme
{
    public static readonly CornerRadiusFilterConverter  CornerRadiusConverter = new CornerRadiusFilterConverter()
    {
        Filter = Corners.BottomLeft | Corners.BottomRight,
    }; 
}