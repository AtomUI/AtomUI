using AtomUI.Theme.Definitions.Compiled;

namespace AtomUI.Theme.Definitions;

/// <summary>Explicit source union. Compiled packages never impersonate a byte source or resolver.</summary>
internal sealed class ThemeDefinitionRegistration
{
    internal ThemeDefinitionRegistration(IThemeDefinitionResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        Resolver = resolver;
        Id = resolver.Id;
        SupportsReload = resolver.SupportsReload;
    }

    internal ThemeDefinitionRegistration(CompiledThemePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        Package = package;
        Id = package.Id;
    }

    internal string Id { get; }
    internal bool SupportsReload { get; }
    internal IThemeDefinitionResolver? Resolver { get; }
    internal CompiledThemePackage? Package { get; }
    internal bool IsUserDirectory => Resolver is UserDirectoryThemeDefinitionResolver;
}
