using AtomUI.Generated.AtomUIDesktopControlsDataGrid;
namespace AtomUI.Desktop.Controls;

public static class DataGridThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls.DataGrid";

    public static IAtomUIBuilder UseDesktopDataGrid(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Definition.Register(builder, static package => package
            .DependsOn(static builder => builder.UseDesktopControls()));
    }
}
