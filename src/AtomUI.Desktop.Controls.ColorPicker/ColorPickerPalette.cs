using AtomUI.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One named group of preset colors in the palette area of a color picker.
/// </summary>
public class ColorPickerPalette : IReadOnlyHeadered
{
    /// <summary>
    /// The name of the group.
    /// </summary>
    public string Label { get; }
    /// <summary>
    /// Whether the group is expanded. The value initializes the collapse item that shows the group.
    /// </summary>
    public bool IsOpen { get; }
    /// <summary>
    /// The preset colors of the group, in the order in which they are shown.
    /// </summary>
    public IEnumerable<Color> Colors { get; }

    /// <summary>
    /// The <see cref="Label" /> of the group, exposed for controls that show a header.
    /// </summary>
    public object Header => Label;

    /// <summary>
    /// Creates a group of preset colors.
    /// </summary>
    /// <param name="label">
    /// The name of the group.
    /// </param>
    /// <param name="isOpen">
    /// Whether the group is expanded.
    /// </param>
    /// <param name="colors">
    /// The preset colors of the group, in the order in which they are shown.
    /// </param>
    public ColorPickerPalette(string label, bool isOpen, IEnumerable<Color> colors)
    {
        Label = label;
        IsOpen = isOpen;
        Colors = colors;
    }
}