namespace AtomUI.Theme;

internal partial class ThemeManager
{
    /// <summary>CI validation entry: executes every selected asset factory through its real owner lifecycle.</summary>
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
