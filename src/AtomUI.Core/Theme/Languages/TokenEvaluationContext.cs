using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>The variant and language tokens used during global or control token evaluation.</summary>
public readonly ref struct TokenEvaluationContext<TLanguageToken>
    where TLanguageToken : LanguageDesignToken
{
    public TokenEvaluationContext(DesignVariant variant, TLanguageToken language)
    {
        Variant  = variant;
        Language = language;
    }

    public DesignVariant Variant { get; }

    public TLanguageToken Language { get; }
}
