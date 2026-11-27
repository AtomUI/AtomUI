namespace AtomUI.Theme;

// Part of ThemeManager: the validation of theme assets. It materializes the assets of every selected asset
// factory so that a validation run executes them through their real owner lifecycle. The type summary is on the
// main declaration.
internal partial class ThemeManager
{
    /// <summary>
    /// CI validation entry: executes every selected asset factory through its real owner lifecycle.
    /// </summary>
    /// <returns>
    /// The number of theme assets that were made to load: every asset of the startup package index plus every
    /// distinct deferred provider of the static resource table.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the application is not initialized.
    /// </exception>
    internal int MaterializeThemeAssets()
    {
        VerifyTransitionAccess();
        if (!_applicationInitialized)
        {
            throw new InvalidOperationException("Theme asset validation requires an initialized application.");
        }
        return (_startupResourceProvider?.MaterializeAssets() ?? 0) + (_staticResourceProvider?.MaterializeAssets() ?? 0);
    }
}
