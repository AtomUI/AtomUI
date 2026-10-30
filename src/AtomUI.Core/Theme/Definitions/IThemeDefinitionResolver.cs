namespace AtomUI.Theme.Definitions;

public interface IThemeDefinitionResolver
{
    string Id { get; }
    bool SupportsReload { get; }

    ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context);
}

/// <summary>
/// Opts a resolver into startup background preparation. Implementations must make Resolve
/// and every returned source's OpenRead safe to call from a worker thread, including a
/// concurrent UI-thread fallback if preparation times out.
/// </summary>
public interface IBackgroundThemeDefinitionResolver : IThemeDefinitionResolver
{
}
