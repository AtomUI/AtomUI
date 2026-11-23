using AtomUI.Theme.Languages;

namespace AtomUI.DesignLanguages.AntDesign;

public static class AntDesignBuilderExtensions
{
    /// <summary>
    /// Enables the Ant Design language and its official themes (Daybreak Blue). The value packages of the enabled
    /// control packages are added in <paramref name="configure"/>, for example
    /// <c>builder.UseAntDesign(language =&gt; language.AddDesktopControls())</c>.
    /// </summary>
    public static IAtomUIBuilder UseAntDesign(this IAtomUIBuilder builder,
        Action<DesignLanguageBuilder<AntDesignLanguage>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIDesignLanguagesAntDesign.GeneratedDesignLanguageRegistration.Register(builder.Theme);
        if (s_registeredThemes.TryAdd(builder.Theme, s_marker))
        {
            Generated.AtomUIDesignLanguagesAntDesign.GeneratedThemePackageRegistration.Register(builder.Theme);
        }
        configure?.Invoke(new DesignLanguageBuilder<AntDesignLanguage>(builder));
        return builder;
    }

    // The compiled theme package registers once per theme manager builder: a second UseAntDesign call adds value
    // packages only.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, object> s_registeredThemes = new();
    private static readonly object s_marker = new();
}
