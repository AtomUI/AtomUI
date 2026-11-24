using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Registers how one language token type derives one control's view: the pair (<see cref="LanguageTokenType"/>,
/// <see cref="Control"/>) and an instance of the rule class. The generated registration of a value package
/// creates one per <see cref="ControlTokenRuleAttribute"/> class.
/// </summary>
public sealed class ControlTokenRuleDescriptor
{
    private readonly Evaluator _evaluator;

    private ControlTokenRuleDescriptor(Type languageTokenType, Type tokenType, ControlTokenIdentity control,
        Evaluator evaluator, System.Reflection.Assembly valuePackage)
    {
        LanguageTokenType = languageTokenType;
        TokenType         = tokenType;
        Control           = control;
        _evaluator        = evaluator;
        ValuePackage      = valuePackage;
        Revision          = valuePackage.ManifestModule.ModuleVersionId;
    }

    /// <summary>Creates the descriptor of <typeparamref name="TRule"/> for the control of <typeparamref name="TToken"/>.</summary>
    public static ControlTokenRuleDescriptor Create<TLanguageToken, TToken, TRule>()
        where TLanguageToken : LanguageDesignToken
        where TToken : AbstractControlDesignToken, IControlToken
        where TRule : class, IControlTokenRule<TLanguageToken, TToken>, new()
    {
        var control = TToken.ControlIdentity;
        if (control.Catalog is null)
        {
            throw new InvalidOperationException(
                $"Control token type '{typeof(TToken).FullName}' reports an uninitialized control identity.");
        }

        return new ControlTokenRuleDescriptor(typeof(TLanguageToken), typeof(TToken), control,
            new Evaluator<TLanguageToken, TToken>(new TRule()),
            typeof(TRule).Assembly);
    }

    /// <summary>The language token type the rule is bound to; derived languages find it along their base types.</summary>
    public Type LanguageTokenType { get; }

    /// <summary>The control token type the rule writes.</summary>
    public Type TokenType { get; }

    /// <summary>The control whose view the rule computes.</summary>
    public ControlTokenIdentity Control { get; }

    /// <summary>The identity of the value package's code; part of the registry revision.</summary>
    public Guid Revision { get; }

    /// <summary>The value package (assembly) that holds the rule.</summary>
    internal System.Reflection.Assembly ValuePackage { get; }

    /// <summary>Identifies this registration in compilation caches.</summary>
    internal object CompilationIdentity { get; } = new();

    internal void Apply(in DesignVariant variant, LanguageDesignToken language, AbstractControlDesignToken token,
        DesignToken global) =>
        _evaluator.Apply(in variant, language, token, global, Control);

    private abstract class Evaluator
    {
        internal abstract void Apply(in DesignVariant variant, LanguageDesignToken language,
            AbstractControlDesignToken token, DesignToken global, ControlTokenIdentity control);
    }

    private sealed class Evaluator<TLanguageToken, TToken>(IControlTokenRule<TLanguageToken, TToken> rule) : Evaluator
        where TLanguageToken : LanguageDesignToken
        where TToken : AbstractControlDesignToken
    {
        internal override void Apply(in DesignVariant variant, LanguageDesignToken language,
            AbstractControlDesignToken token, DesignToken global, ControlTokenIdentity control)
        {
            if (token is not TToken typed || language is not TLanguageToken typedLanguage)
            {
                throw new ArgumentException(
                    $"Control rule for '{control}' expects '{typeof(TLanguageToken).Name}' and '{typeof(TToken).Name}', " +
                    $"not '{language.GetType().Name}' and '{token.GetType().Name}'.");
            }

            rule.Apply(new TokenEvaluationContext<TLanguageToken>(variant, typedLanguage),
                new ControlTokenTarget<TToken>(typed, global));
        }
    }

    public override string ToString() => $"{LanguageTokenType.Name} -> {Control}";
}
