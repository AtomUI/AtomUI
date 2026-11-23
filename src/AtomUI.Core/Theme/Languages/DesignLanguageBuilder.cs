namespace AtomUI.Theme.Languages;

/// <summary>
/// What <c>Use&lt;Language&gt;(language =&gt; …)</c> hands to its configuration (spec 8): the value packages of the language
/// can register their derivations explicitly through extensions such as <c>language.AddDesktopControls()</c>.
/// </summary>
/// <typeparam name="TLanguage">The design language being configured.</typeparam>
public sealed class DesignLanguageBuilder<TLanguage>
    where TLanguage : DesignLanguage
{
    public DesignLanguageBuilder(IAtomUIBuilder root)
    {
        ArgumentNullException.ThrowIfNull(root);
        Root = root;
    }

    /// <summary>The application builder.</summary>
    public IAtomUIBuilder Root { get; }

    /// <summary>The theme manager builder: value packages register their derivations through it.</summary>
    public IThemeManagerBuilder Theme => Root.Theme;

    /// <summary>Sets this language's default named theme for startup and selection.</summary>
    public DesignLanguageBuilder<TLanguage> WithDefaultTheme(string themeId)
    {
        Theme.SetDefaultTheme<TLanguage>(themeId);
        return this;
    }

    /// <summary>Chooses this value package when more than one package covers the same pair.</summary>
    public DesignLanguageBuilder<TLanguage> UseValues(ControlValuePackage package)
    {
        Theme.SelectValuePackage<TLanguage>(package);
        return this;
    }
}
