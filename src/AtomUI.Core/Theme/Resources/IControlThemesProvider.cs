using Avalonia.Controls;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Supplies the control themes of one theme package to the theme manager.
/// </summary>
public interface IControlThemesProvider
{
    /// <summary>
    /// The id of the theme package the provider belongs to. It has to be unique: registering a second provider
    /// with the same id fails.
    /// </summary>
    string Id { get; }
    /// <summary>
    /// The resource providers that hold the control themes. The theme manager reads the list when it loads the
    /// package, and may clear it and refill it with a lazy provider for each registered theme asset.
    /// </summary>
    IList<IResourceProvider> ControlThemes { get; }
}
