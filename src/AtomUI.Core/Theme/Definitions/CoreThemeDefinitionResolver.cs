namespace AtomUI.Theme.Definitions;

internal static class CoreThemeDefinitionResolver
{
    internal const string ResolverId = "AtomUI.Core.BuiltInThemes";

    internal static IThemeDefinitionResolver Create()
    {
        return new BuiltInResolver();
    }

    private sealed class BuiltInResolver : IThemeDefinitionResolver
    {
        private readonly IThemeDefinitionSource[] _sources = [new BuiltInThemeDefinitionSource(
            typeof(CoreThemeDefinitionResolver).Assembly.GetName().Version?.ToString() ?? "0")];

        public string Id => ResolverId;
        public bool SupportsReload => false;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return new ThemeDefinitionResolveResult(_sources, Array.Empty<ThemeDiagnostic>());
        }
    }
}
