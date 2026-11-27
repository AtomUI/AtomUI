using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One color at one position in the gradient of a <see cref="BorderBeam" /> beam. A stop takes effect once it
/// is in <see cref="BorderBeam.ColorStops" />.
/// </summary>
public class BorderBeamColorStop : AvaloniaObject
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Color" /> property.
    /// </summary>
    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<BorderBeamColorStop, Color>(nameof(Color));

    /// <summary>
    /// Identifies the <see cref="Percent" /> property.
    /// </summary>
    public static readonly StyledProperty<double> PercentProperty =
        AvaloniaProperty.Register<BorderBeamColorStop, double>(nameof(Percent));

    /// <summary>
    /// Color of the beam at this stop. A stop that was never given a color is fully transparent.
    /// </summary>
    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// Position of this stop within the colored part of the beam, from 0 to 100. Values outside that range are
    /// clamped to it, and NaN or infinity counts as 0.
    /// </summary>
    public double Percent
    {
        get => GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    #endregion
}
