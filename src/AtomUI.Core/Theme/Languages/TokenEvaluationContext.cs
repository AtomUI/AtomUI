using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// The variant and language tokens used during global or control token evaluation.
/// </summary>
public readonly ref struct TokenEvaluationContext<TLanguageToken>
    where TLanguageToken : LanguageDesignToken
{
    /// <summary>
    /// Creates a context for evaluating the tokens of <paramref name="variant" /> with the language package
    /// <paramref name="language" />.
    /// </summary>
    /// <param name="variant">
    /// The design variant being evaluated, with its style, density and idiom.
    /// </param>
    /// <param name="language">
    /// The language token package the evaluation reads.
    /// </param>
    public TokenEvaluationContext(DesignVariant variant, TLanguageToken language)
    {
        Variant  = variant;
        Language = language;
    }

    /// <summary>
    /// The design variant being evaluated, with its style, density and idiom.
    /// </summary>
    public DesignVariant Variant { get; }

    /// <summary>
    /// The language token package the evaluation reads its language-specific values from.
    /// </summary>
    public TLanguageToken Language { get; }
}
