using AtomUI.Generated.AtomUIDesktopControlsDataGrid;
namespace AtomUI.Desktop.Controls;

/// <summary>
/// Extension methods that register the DataGrid package on an AtomUI builder.
/// </summary>
public static class DataGridThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls.DataGrid";

    /// <summary>
    /// Registers the DataGrid package on the builder. The desktop controls package is registered with it as a
    /// dependency.
    /// </summary>
    /// <param name="builder">
    /// The builder that assembles the application.
    /// </param>
    /// <returns>
    /// The same builder, for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    public static IAtomUIBuilder UseDesktopDataGrid(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return PackageMetadata.Definition.Register(builder, static package => package
            .DependsOn(static builder => builder.UseDesktopControls()));
    }
}
