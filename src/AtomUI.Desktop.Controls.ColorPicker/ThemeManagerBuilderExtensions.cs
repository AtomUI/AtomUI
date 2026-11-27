using AtomUI.Generated.AtomUIDesktopControlsColorPicker;
namespace AtomUI.Desktop.Controls;

/// <summary>
/// Registers the color picker control package with an AtomUI builder.
/// </summary>
public static class ColorPickerThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls.ColorPicker";

    /// <summary>
    /// Registers the color picker package with <paramref name="builder" />. The desktop controls package is
    /// registered as well, because the color picker depends on it.
    /// </summary>
    public static IAtomUIBuilder UseDesktopColorPicker(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return PackageMetadata.Definition.Register(builder, static package => package
            .DependsOn(static builder => builder.UseDesktopControls()));
    }
}
