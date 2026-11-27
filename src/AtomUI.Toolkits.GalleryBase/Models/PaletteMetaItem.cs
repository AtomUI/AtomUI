using AtomUI.Theme.Derivation;

namespace AtomUI.Toolkits.GalleryBase.Models;

/// <summary>
/// Describes one preset palette for the Gallery palette showcase: a title, a description and the preset
/// primary color that selects the palette.
/// </summary>
public class PaletteMetaItem
{
    /// <summary>
    /// Title of the palette.
    /// </summary>
    public string Title;
    /// <summary>
    /// Description of the palette.
    /// </summary>
    public string Desc;
    /// <summary>
    /// The preset primary color whose palette the color list shows when the item is set as its
    /// <c>PaletteMetaInfo</c>.
    /// </summary>
    public PresetPrimaryColor PresetPrimaryColor;

    /// <summary>
    /// Creates a palette description.
    /// </summary>
    /// <param name="title">
    /// Title of the palette.
    /// </param>
    /// <param name="desc">
    /// Description of the palette.
    /// </param>
    /// <param name="presetPrimaryColor">
    /// The preset primary color whose palette is shown.
    /// </param>
    public PaletteMetaItem(string title, string desc, PresetPrimaryColor presetPrimaryColor)
    {
        Title              = title;
        Desc               = desc;
        PresetPrimaryColor = presetPrimaryColor;
    }
}
