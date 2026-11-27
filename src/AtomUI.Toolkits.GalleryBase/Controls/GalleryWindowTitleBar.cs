using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// The title bar of the Gallery workspace window. It adds a <see cref="Menu" /> area on the left of the bar.
/// </summary>
public class GalleryWindowTitleBar : WindowTitleBar
{
    /// <summary>
    /// Identifies the <see cref="Menu" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> MenuProperty =
        AvaloniaProperty.Register<GalleryWindowTitleBar, Control?>(nameof(Menu));

    /// <summary>
    /// Content shown in the left add-on area of the title bar, normally the menu of the Gallery workspace. The
    /// value is forwarded to the inherited <c>LeftAddOn</c> property whenever it changes. The default is
    /// <see langword="null" />, which shows nothing in that area.
    /// </summary>
    public Control? Menu
    {
        get => GetValue(MenuProperty);
        set => SetValue(MenuProperty, value);
    }

    static GalleryWindowTitleBar()
    {
        MenuProperty.Changed.AddClassHandler<GalleryWindowTitleBar>((titleBar, args) =>
        {
            titleBar.LeftAddOn = args.GetNewValue<Control?>();
        });
    }
}
