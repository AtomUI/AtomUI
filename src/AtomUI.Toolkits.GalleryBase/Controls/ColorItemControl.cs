using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// A swatch of one palette color: the color fills the background of the control, and it shows the name and
/// the hexadecimal value of the color.
/// </summary>
public class ColorItemControl : TemplatedControl
{
    /// <summary>
    /// Identifies the <see cref="ColorName" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ColorNameProperty =
        AvaloniaProperty.Register<ColorItemControl, string?>(nameof(ColorName));

    /// <summary>
    /// Identifies the <see cref="Hex" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> HexProperty =
        AvaloniaProperty.Register<ColorItemControl, string?>(nameof(Hex));

    /// <summary>
    /// The display name of the color, shown on the swatch. The default is <see langword="null" />.
    /// </summary>
    public string? ColorName
    {
        get => GetValue(ColorNameProperty);
        set => SetValue(ColorNameProperty, value);
    }

    /// <summary>
    /// The hexadecimal value of the color, shown on the swatch. The default is <see langword="null" />.
    /// </summary>
    public string? Hex
    {
        get => GetValue(HexProperty);
        set => SetValue(HexProperty, value);
    }
}
