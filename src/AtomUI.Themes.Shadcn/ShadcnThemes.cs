using AtomUI.Theme;

namespace AtomUI.Themes.Shadcn;

public static class ShadcnThemeManagerBuilderExtensions
{
    /// <summary>
    /// Registers the Shadcn style algorithms (<c>Shadcn:Nova</c>, <c>Shadcn:Nova.Dark</c>, …), their control token
    /// stages and the Shadcn style theme profiles. The generated <c>GeneratedThemeAlgorithmRegistration</c> adds
    /// both the algorithms and the control stages.
    /// </summary>
    public static IThemeManagerBuilder UseShadcnThemes(this IThemeManagerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIThemesShadcn.GeneratedThemeAlgorithmRegistration.Register(builder);
        Generated.AtomUIThemesShadcn.GeneratedThemePackageRegistration.Register(builder);
        return builder;
    }
}
