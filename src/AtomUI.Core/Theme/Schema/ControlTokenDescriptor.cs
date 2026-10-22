using System.Diagnostics.CodeAnalysis;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

public sealed class ControlTokenDescriptor
{
    private readonly Func<AbstractControlDesignToken>? _factory;
    private readonly Action<AbstractControlDesignToken, ThemeAppearance>? _evaluator;
    private readonly ControlTokenDefaultsEvaluator? _defaultsEvaluator;
    private readonly (TokenDescriptor Alias, TokenDescriptor Canonical)[] _aliases;

    public ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity)
        : this(
            controlType,
            identity,
            -1,
            Array.Empty<TokenDescriptor>(),
            null,
            null)
    {
    }

    public ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken> factory,
        Action<AbstractControlDesignToken, ThemeAppearance> evaluator)
        : this(controlType, identity, -1, ownTokens, factory, evaluator)
    {
    }

    public ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken> factory,
        Action<AbstractControlDesignToken, ThemeAppearance> evaluator,
        IEnumerable<ControlTokenAlias> aliases)
        : this(controlType, identity, -1, ownTokens, factory, evaluator, null, 0,
            aliases ?? throw new ArgumentNullException(nameof(aliases)))
    {
    }

    public ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken> factory,
        ControlTokenDefaultsEvaluator evaluator,
        ulong evaluatorRevision,
        IEnumerable<ControlTokenAlias> aliases)
        : this(controlType, identity, -1, ownTokens, factory, null,
            evaluator ?? throw new ArgumentNullException(nameof(evaluator)), evaluatorRevision,
            aliases ?? throw new ArgumentNullException(nameof(aliases)))
    {
    }

    public ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken> factory,
        ControlTokenDefaultsEvaluator evaluator,
        ulong evaluatorRevision)
        : this(controlType, identity, -1, ownTokens, factory, null,
            evaluator ?? throw new ArgumentNullException(nameof(evaluator)), evaluatorRevision)
    {
    }

    private ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        int slot,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken>? factory,
        Action<AbstractControlDesignToken, ThemeAppearance>? evaluator)
        : this(controlType, identity, slot, ownTokens, factory, evaluator, null, 0)
    {
    }

    private ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        int slot,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken>? factory,
        Action<AbstractControlDesignToken, ThemeAppearance>? evaluator,
        ControlTokenDefaultsEvaluator? defaultsEvaluator,
        ulong evaluatorRevision,
        IEnumerable<ControlTokenAlias>? aliases = null)
    {
        ArgumentNullException.ThrowIfNull(controlType);
        ArgumentNullException.ThrowIfNull(ownTokens);
        if (!typeof(Control).IsAssignableFrom(controlType))
        {
            throw new ArgumentException(
                $"Control Token type '{controlType.FullName}' must derive from Avalonia.Controls.Control.",
                nameof(controlType));
        }

        var ownTokenArray = OrderOwnTokens(ownTokens);
        if (ownTokenArray.Length == 0)
        {
            if (factory is not null || evaluator is not null || defaultsEvaluator is not null)
            {
                throw new ArgumentException(
                    "A Control without Own Tokens cannot define a Token factory or evaluator.",
                    nameof(factory));
            }
        }
        else if (factory is null || (evaluator is null && defaultsEvaluator is null) ||
                 (evaluator is not null && defaultsEvaluator is not null))
        {
            throw new ArgumentException(
                "A Control with Own Tokens requires a Token factory and evaluator.",
                nameof(factory));
        }

        if (defaultsEvaluator is not null && evaluatorRevision == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(evaluatorRevision),
                "A dependency-defaults evaluator requires a nonzero revision.");
        }

        ControlType = controlType;
        CompilationIdentity = new object();
        Identity    = identity;
        Slot        = slot;
        OwnTokens   = Array.AsReadOnly(ownTokenArray);
        var aliasArray = aliases?.ToArray() ?? Array.Empty<ControlTokenAlias>();
        _aliases = BindAliases(ownTokenArray, aliasArray);
        OwnTokenAliases = aliasArray.Length == 0
            ? Array.Empty<ControlTokenAlias>() : Array.AsReadOnly(aliasArray);
        _factory    = factory;
        _evaluator  = evaluator;
        _defaultsEvaluator = defaultsEvaluator;
        EvaluationVersion = defaultsEvaluator is null
            ? ControlTokenEvaluationVersion.WholeEvaluatorV1
            : ControlTokenEvaluationVersion.DependencyDefaultsV2;
        EvaluatorRevision = evaluatorRevision;
    }

    private ControlTokenDescriptor(ControlTokenDescriptor source, int slot)
    {
        ControlType = source.ControlType;
        CompilationIdentity = source.CompilationIdentity;
        Identity    = source.Identity;
        Slot        = slot;
        // Public construction already copied, ordered and sealed this metadata.
        OwnTokens   = source.OwnTokens;
        OwnTokenAliases = source.OwnTokenAliases;
        _aliases = source._aliases;
        _factory    = source._factory;
        _evaluator  = source._evaluator;
        _defaultsEvaluator = source._defaultsEvaluator;
        EvaluationVersion = source.EvaluationVersion;
        EvaluatorRevision = source.EvaluatorRevision;
    }

    public Type ControlType { get; }
    internal object CompilationIdentity { get; }
    public ControlTokenIdentity Identity { get; }
    public int Slot { get; }
    public IReadOnlyList<TokenDescriptor> OwnTokens { get; }
    public IReadOnlyList<ControlTokenAlias> OwnTokenAliases { get; }
    public bool HasOwnTokens => _factory is not null;
    public ControlTokenEvaluationVersion EvaluationVersion { get; }
    public ulong EvaluatorRevision { get; }

    public AbstractControlDesignToken? CreateBuilder() => _factory?.Invoke();

    public void Evaluate(AbstractControlDesignToken token, ThemeAppearance appearance)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (_evaluator is null)
        {
            throw new InvalidOperationException(HasOwnTokens
                ? $"Control '{Identity}' requires the dependency-defaults evaluation entry."
                : $"Control '{Identity}' does not define Own Tokens.");
        }
        _evaluator(token, appearance);
    }

    public void EvaluateDefaults(AbstractControlDesignToken token,
        in ControlTokenDefaultContext context, ReadOnlySpan<ulong> explicitOwnSlots)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (_defaultsEvaluator is null)
        {
            throw new InvalidOperationException($"Control '{Identity}' does not define a dependency-defaults evaluator.");
        }
        context.EnsureInitialized();
        if (!explicitOwnSlots.IsEmpty)
        {
            var wordCount = (OwnTokens.Count + 63L) >> 6;
            var finalBits = OwnTokens.Count & 63;
            if (explicitOwnSlots.Length != wordCount ||
                (finalBits != 0 && (explicitOwnSlots[^1] >> finalBits) != 0))
            {
                throw new ArgumentException("Explicit Own presence does not match the Control schema.",
                    nameof(explicitOwnSlots));
            }
        }
        _defaultsEvaluator(token, in context, explicitOwnSlots);
    }

    internal ControlTokenDescriptor Bind(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        return new ControlTokenDescriptor(this, slot);
    }

    internal TokenDescriptor GetCanonicalOwnToken(TokenDescriptor token)
    {
        foreach (var pair in _aliases)
        {
            if (ReferenceEquals(pair.Alias, token))
            {
                return pair.Canonical;
            }
        }
        return token;
    }

    internal void ApplyAliases(AbstractControlDesignToken token)
    {
        foreach (var pair in _aliases)
        {
            pair.Alias.SetValue(token, pair.Canonical.CopyValue(pair.Canonical.GetValue(token)));
        }
    }

    private static (TokenDescriptor Alias, TokenDescriptor Canonical)[] BindAliases(
        TokenDescriptor[] tokens, ControlTokenAlias[] aliases)
    {
        if (aliases.Length == 0)
        {
            return Array.Empty<(TokenDescriptor, TokenDescriptor)>();
        }
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var alias in aliases)
        {
            if (alias is null || !names.Add(alias.Name))
            {
                throw new ArgumentException("Own Token aliases must be nonnull and unique.", nameof(aliases));
            }
        }
        var own = tokens.ToDictionary(static token => token.Name, StringComparer.Ordinal);
        var result = new (TokenDescriptor, TokenDescriptor)[aliases.Length];
        for (var index = 0; index < aliases.Length; index++)
        {
            var alias = aliases[index];
            if (!own.TryGetValue(alias.Name, out var source) ||
                !own.TryGetValue(alias.CanonicalName, out var canonical) ||
                names.Contains(alias.CanonicalName) || source.ValueType != canonical.ValueType ||
                source.Stage != TokenStage.Control || canonical.Stage != TokenStage.Control)
            {
                throw new ArgumentException(
                    $"Own Token alias '{alias.Name}' requires a direct canonical Own Token '{alias.CanonicalName}' of the same type; alias chains and cycles are not supported.",
                    nameof(aliases));
            }
            result[index] = (source, canonical);
        }
        return result;
    }

    // Generated factories emit own Tokens slot-ascending (slot == emission index); the O(n) scan lets
    // that input skip the OrderBy wrapper while unsorted public input keeps the stable-sort contract.
    private static TokenDescriptor[] OrderOwnTokens(IEnumerable<TokenDescriptor> ownTokens)
    {
        var array = ownTokens.ToArray();
        for (var index = 1; index < array.Length; index++)
        {
            if (array[index - 1].Slot > array[index].Slot)
            {
                return array.OrderBy(static token => token.Slot).ToArray();
            }
        }
        return array;
    }

    internal bool TryGetOwnToken(
        string name,
        [NotNullWhen(true)] out TokenDescriptor? descriptor)
    {
        foreach (var candidate in OwnTokens)
        {
            if (string.Equals(candidate.Name, name, StringComparison.Ordinal))
            {
                descriptor = candidate;
                return true;
            }
        }

        descriptor = null;
        return false;
    }
}
