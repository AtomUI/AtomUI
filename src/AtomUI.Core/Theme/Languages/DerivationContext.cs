using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>What a derivation reads besides the token it writes: the variant and the language tokens.</summary>
public readonly ref struct DerivationContext<TLanguageToken>
    where TLanguageToken : LanguageDesignToken
{
    public DerivationContext(DesignVariant variant, TLanguageToken language)
    {
        Variant  = variant;
        Language = language;
    }

    public DesignVariant Variant { get; }

    public TLanguageToken Language { get; }
}
