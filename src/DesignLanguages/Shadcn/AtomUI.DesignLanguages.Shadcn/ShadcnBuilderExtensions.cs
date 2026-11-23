using AtomUI.Theme.Languages;

namespace AtomUI.DesignLanguages.Shadcn;

public static class ShadcnBuilderExtensions
{
    /// <summary>
    /// Enables the shadcn language. The value packages of the enabled control packages are added in
    /// <paramref name="configure"/>, for example <c>builder.UseShadcn(language =&gt; language.AddDesktopControls())</c>.
    /// </summary>
    public static IAtomUIBuilder UseShadcn(this IAtomUIBuilder builder,
        Action<DesignLanguageBuilder<ShadcnLanguage>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIDesignLanguagesShadcn.GeneratedDesignLanguageRegistration.Register(builder.Theme);
        configure?.Invoke(new DesignLanguageBuilder<ShadcnLanguage>(builder));
        return builder;
    }
}
