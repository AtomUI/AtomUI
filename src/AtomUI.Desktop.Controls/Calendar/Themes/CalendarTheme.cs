using Avalonia.Styling;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Themes;

internal class CalendarControlTheme : ControlTheme
{
    public static readonly IValueConverter TopLineThickness =
        new FuncValueConverter<double, Thickness>(static width => new Thickness(0, width, 0, 0));
}
