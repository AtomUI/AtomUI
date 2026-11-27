using AtomUI.Controls;

namespace AtomUI.Icons;

/// <summary>
/// Describes one SVG source file of an icon package while the package is generated.
/// </summary>
public record IconFileInfo
{
    /// <summary>
    /// Group the icon belongs to, or <see langword="null" /> when it has none.
    /// <see cref="DefaultIconPackageGenerator" /> leaves it <see langword="null" />.
    /// </summary>
    public string? Category;
    /// <summary>
    /// Path of the SVG source file that the generator reads.
    /// </summary>
    public string FilePath = string.Empty;
    /// <summary>
    /// Class name of the generated icon without the theme suffix.
    /// </summary>
    public string Name = string.Empty;
    /// <summary>
    /// Theme of the icon. It is appended to <see cref="Name" /> to form the generated class name and the
    /// member name in the icon kind enum.
    /// </summary>
    public IconThemeType ThemeType;
}