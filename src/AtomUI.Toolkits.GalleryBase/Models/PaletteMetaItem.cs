using AtomUI.Theme.Derivation;

namespace AtomUI.Toolkits.GalleryBase.Models;

public class PaletteMetaItem
{
    public string Title;
    public string Desc;
    public PresetPrimaryColor PresetPrimaryColor;

    public PaletteMetaItem(string title, string desc, PresetPrimaryColor presetPrimaryColor)
    {
        Title              = title;
        Desc               = desc;
        PresetPrimaryColor = presetPrimaryColor;
    }
}
