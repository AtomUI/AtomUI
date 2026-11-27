using AtomUI.Controls;

namespace AtomUI.Toolkits.GalleryBase.Models;

/// <summary>
/// One icon of an icon package in the Gallery icon list: its name, the type recorded for it and the
/// factory that creates the icon when the list loads it.
/// </summary>
public record PackageIconItem
{
    /// <summary>
    /// Name of the icon, shown in the icon list.
    /// </summary>
    public string IconName { get; set; }
    /// <summary>
    /// The icon instance of this entry. It stays <see langword="null" /> until the icon gallery loads the
    /// entry and creates it through <see cref="Creator" />.
    /// </summary>
    public Icon? Icon { get; set; }
    /// <summary>
    /// The type recorded for the icon by the icon package catalog, passed on from the catalog entry.
    /// </summary>
    public Type IconType { get; set; }
    /// <summary>
    /// Factory that creates <see cref="Icon" />. The icon gallery invokes it when it loads this entry;
    /// <see langword="null" /> leaves <see cref="Icon" /> unset.
    /// </summary>
    public Func<Icon>? Creator { get; set; }

    /// <summary>
    /// Creates an entry whose <see cref="Icon" /> stays <see langword="null" /> until the icon gallery
    /// loads it.
    /// </summary>
    /// <param name="iconName">
    /// Name of the icon, shown in the icon list.
    /// </param>
    /// <param name="iconType">
    /// The type recorded for the icon by the icon package catalog.
    /// </param>
    /// <param name="creator">
    /// Factory that creates <see cref="Icon" />, or <see langword="null" /> for none.
    /// </param>
    public PackageIconItem(string iconName, Type iconType, Func<Icon>? creator)
    {
        IconName = iconName;
        IconType = iconType;
        Creator  = creator;
    }
}
