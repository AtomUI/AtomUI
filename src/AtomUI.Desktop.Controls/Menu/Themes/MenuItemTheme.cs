using AtomUI.Controls.Converters;
using AtomUI.Desktop.Controls.Converters;
using Avalonia;
using Avalonia.Controls.Converters;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

internal class MenuItemTheme : ControlTheme
{
    public static readonly StringToTextBlockConverter StringToTextBlockConverter = new()
    {
        VerticalAlignment = VerticalAlignment.Center
    };

    public static readonly PlatformKeyGestureConverter KeyGestureConverter = new();
    public static readonly ToggleItemsLayoutVisibleConverter ToggleItemsLayoutVisibleConverter = new();

    /// <summary>Projects the icon's inline-end spacing (ItemIconMarginInlineEnd) onto the icon margin.</summary>
    public static readonly IValueConverter InlineEndMarginConverter =
        new FuncValueConverter<double, Thickness>(static value => new Thickness(0, 0, value, 0));
}
