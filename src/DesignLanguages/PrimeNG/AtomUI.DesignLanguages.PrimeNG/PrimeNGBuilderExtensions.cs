using AtomUI.Theme.Languages;

namespace AtomUI.DesignLanguages.PrimeNG;

public static class PrimeNGBuilderExtensions
{
    /// <summary>
    /// Enables the PrimeNG language (styles Aura, Lara and Nora, chosen by the theme selection). The value packages of
    /// the enabled control packages are added in <paramref name="configure"/>, for example
    /// <c>builder.UsePrimeNG(language =&gt; language.AddDesktopControls())</c>.
    /// </summary>
    public static IAtomUIBuilder UsePrimeNG(this IAtomUIBuilder builder,
        Action<DesignLanguageBuilder<PrimeNGLanguage>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIDesignLanguagesPrimeNG.GeneratedDesignLanguageRegistration.Register(builder.Theme);
        configure?.Invoke(new DesignLanguageBuilder<PrimeNGLanguage>(builder));
        return builder;
    }
}
