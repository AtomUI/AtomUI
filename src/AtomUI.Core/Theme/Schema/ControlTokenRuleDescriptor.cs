using System.ComponentModel;
using System.Reflection;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Registers how one language token type derives one control's view: the pair (<see cref="LanguageTokenType" />,
/// <see cref="Control" />) and its typed evaluator. The generated registration of a value package
/// creates one per <see cref="ControlTokenRuleAttribute" /> class.
/// </summary>
public sealed class ControlTokenRuleDescriptor
{
    private readonly Evaluator _evaluator;
    private readonly string? _expectedTokenSchemaDigest;

    private ControlTokenRuleDescriptor(Type languageTokenType, Type tokenType, ControlTokenIdentity control,
        Evaluator evaluator, Assembly valuePackage, string? expectedTokenSchemaDigest)
    {
        LanguageTokenType          = languageTokenType;
        TokenType                  = tokenType;
        Control                    = control;
        _evaluator                 = evaluator;
        _expectedTokenSchemaDigest = expectedTokenSchemaDigest;
        ValuePackage               = valuePackage;
        Revision                   = valuePackage.ManifestModule.ModuleVersionId;
    }

    /// <summary>
    /// Creates the descriptor of <typeparamref name="TRule" /> for the control of <typeparamref name="TToken" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <typeparamref name="TToken" /> reports a control identity without a catalog.
    /// </exception>
    public static ControlTokenRuleDescriptor Create<TLanguageToken, TToken, TRule>()
        where TLanguageToken : LanguageDesignToken
        where TToken : AbstractControlDesignToken, IControlToken
        where TRule : class, IControlTokenRule<TLanguageToken, TToken>, new() =>
        Create<TLanguageToken, TToken, TRule>(null);

    /// <summary>
    /// Creates a cross-package rule using the control schema digest observed when the rule was compiled.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ControlTokenRuleDescriptor Create<TLanguageToken, TToken, TRule>(string? expectedTokenSchemaDigest)
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
            new Evaluator<TLanguageToken, TToken, TRule>(),
            typeof(TRule).Assembly, expectedTokenSchemaDigest);
    }

    /// <summary>
    /// The language token type the rule is bound to; derived languages find it along their base types.
    /// </summary>
    public Type LanguageTokenType { get; }

    /// <summary>
    /// The control token type the rule writes.
    /// </summary>
    public Type TokenType { get; }

    /// <summary>
    /// The control whose view the rule computes.
    /// </summary>
    public ControlTokenIdentity Control { get; }

    /// <summary>
    /// The identity of the value package's code; part of the registry revision.
    /// </summary>
    public Guid Revision { get; }

    /// <summary>
    /// The value package (assembly) that holds the rule.
    /// </summary>
    internal Assembly ValuePackage { get; }

    /// <summary>
    /// Identifies this registration in compilation caches.
    /// </summary>
    internal object CompilationIdentity { get; } = new();

    internal void ValidateTokenSchema()
    {
        if (_expectedTokenSchemaDigest is { } expected && _evaluator.TokenSchemaDigest is { } actual)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new ThemeSchemaException(TokenSchemaDigests.Describe(
                    ValuePackage.GetName().Name!, TokenType.Assembly.GetName().Name!, expected, actual)!);
            }
            return;
        }

        // Older or hand-written registrations can still declare their schema evidence on the assemblies.
        TokenSchemaDigests.Validate(ValuePackage, TokenType.Assembly);
    }

    internal void Apply(in DesignVariant variant, LanguageDesignToken language, AbstractControlDesignToken token,
        DesignToken global) =>
        _evaluator.Apply(in variant, language, token, global, Control);

    private abstract class Evaluator
    {
        internal abstract string? TokenSchemaDigest { get; }

        internal abstract void Apply(in DesignVariant variant, LanguageDesignToken language,
            AbstractControlDesignToken token, DesignToken global, ControlTokenIdentity control);
    }

    private sealed class Evaluator<TLanguageToken, TToken, TRule> : Evaluator
        where TLanguageToken : LanguageDesignToken
        where TToken : AbstractControlDesignToken, IControlToken
        where TRule : class, IControlTokenRule<TLanguageToken, TToken>, new()
    {
        private TRule? _rule;

        internal override string? TokenSchemaDigest => TToken.TokenSchemaDigest;

        internal override void Apply(in DesignVariant variant, LanguageDesignToken language,
            AbstractControlDesignToken token, DesignToken global, ControlTokenIdentity control)
        {
            if (token is not TToken typed || language is not TLanguageToken typedLanguage)
            {
                throw new ArgumentException(
                    $"Control rule for '{control}' expects '{typeof(TLanguageToken).Name}' and '{typeof(TToken).Name}', " +
                    $"not '{language.GetType().Name}' and '{token.GetType().Name}'.");
            }

            // Registration needs only identity and type metadata. Cold controls may never evaluate their rule.
            var rule = Volatile.Read(ref _rule);
            if (rule is null)
            {
                lock (this)
                {
                    rule = _rule;
                    if (rule is null)
                    {
                        rule = new TRule();
                        Volatile.Write(ref _rule, rule);
                    }
                }
            }
            rule.Apply(new TokenEvaluationContext<TLanguageToken>(variant, typedLanguage),
                new ControlTokenTarget<TToken>(typed, global));
        }
    }

    /// <summary>
    /// Returns the name of <see cref="LanguageTokenType" />, an arrow and <see cref="Control" />, for diagnostics.
    /// </summary>
    public override string ToString() => $"{LanguageTokenType.Name} -> {Control}";
}
