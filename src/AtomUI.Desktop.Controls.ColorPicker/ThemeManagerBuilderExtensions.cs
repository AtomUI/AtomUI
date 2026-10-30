using AtomUI.Generated.AtomUIDesktopControlsColorPicker;
namespace AtomUI.Desktop.Controls;

public static class ColorPickerThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls.ColorPicker";

    public static IAtomUIBuilder UseDesktopColorPicker(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Definition.Register(builder, static package => package
            .DependsOn(static builder => builder.UseDesktopControls()));
    }
}
