using AtomUI.Theme.Schema;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Markup.Xaml.Templates;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Builds the <see cref="Transitions" /> of one <see cref="TokenTransitions" /> markup for resolved Token durations.
/// </summary>
/// <remarks>
/// <para>
/// The declared Tokens are discovered from the first build. Each distinct duration vector gets one built instance
/// that every target resolving the same durations shares; Avalonia keeps transition state per animated control, and
/// a built instance is never mutated after it is handed out. At most <see cref="MaxSharedVariants" /> vectors are
/// kept (a default theme needs one); further vectors are built per request and owned by their target alone.
/// </para>
/// <para>
/// The factory belongs to the markup, i.e. to the theme, and holds no reference to any target. It is used on the UI
/// thread only, like the styles that own it.
/// </para>
/// </remarks>
internal sealed class TokenTransitionsFactory
{
    internal const int MaxSharedVariants = 8;

    private readonly object _content;
    private readonly List<Variant> _variants = [];
    private readonly Dictionary<ControlTokenIdentity, object[]> _ownerKeys = [];
    private TokenInput[]? _inputs;
    private object[]? _globalKeys;
    private Transitions? _prototype;

    internal TokenTransitionsFactory(object content)
    {
        _content = content;
    }

    /// <summary>
    /// The distinct declared Tokens, in declaration order.
    /// </summary>
    internal TokenInput[] Inputs => _inputs ?? DiscoverInputs();

    /// <summary>
    /// The plain global resource keys of the declared inputs, boxed once.
    /// </summary>
    internal object[] GlobalKeys
    {
        get
        {
            if (_globalKeys is null)
            {
                var inputs = Inputs;
                var keys = new object[inputs.Length];
                for (var index = 0; index < inputs.Length; index++)
                {
                    keys[index] = inputs[index].Kind;
                }
                _globalKeys = keys;
            }
            return _globalKeys;
        }
    }

    internal int SharedVariantCount => _variants.Count;

    /// <summary>
    /// The owner's unbound Effective Global keys of the declared inputs, created once per owner.
    /// </summary>
    internal object[] GetOwnerKeys(ControlTokenIdentity owner)
    {
        if (!_ownerKeys.TryGetValue(owner, out var keys))
        {
            var inputs = Inputs;
            keys = new object[inputs.Length];
            for (var index = 0; index < inputs.Length; index++)
            {
                keys[index] = OwnerTokenBinding.GetResourceKey(owner, inputs[index].Kind);
            }
            _ownerKeys.Add(owner, keys);
        }
        return keys;
    }

    /// <summary>
    /// Returns the transitions for <paramref name="values" /> (one value per declared input).
    /// Finding an existing vector does not allocate.
    /// </summary>
    internal Transitions GetTransitions(ReadOnlySpan<TokenValue> values)
    {
        foreach (var variant in _variants)
        {
            if (Matches(variant.Values, values))
            {
                return variant.Transitions;
            }
        }

        var transitions = _prototype ?? Build();
        _prototype = null;
        ApplyValues(transitions, values);
        if (_variants.Count < MaxSharedVariants && TryCreateKey(values, out var key))
        {
            _variants.Add(new Variant(key, transitions));
        }
        return transitions;
    }

    private static bool Matches(ReadOnlySpan<TokenKey> stored, ReadOnlySpan<TokenValue> expected)
    {
        for (var index = 0; index < stored.Length; index++)
        {
            if (stored[index].Duration != expected[index].Duration ||
                !TokenValueOperations.TryGetEasingKey(expected[index].Easing, out var easing) ||
                stored[index].Easing != easing)
            {
                return false;
            }
        }
        return true;
    }

    private static bool TryCreateKey(ReadOnlySpan<TokenValue> values, out TokenKey[] key)
    {
        key = new TokenKey[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            if (!TokenValueOperations.TryGetEasingKey(values[index].Easing, out var easing))
            {
                return false;
            }
            key[index] = new TokenKey(values[index].Duration, easing);
        }
        return true;
    }

    private TokenInput[] DiscoverInputs()
    {
        _prototype = Build();
        var inputs = new List<TokenInput>();
        foreach (var transition in _prototype.OfType<TransitionBase>())
        {
            if (TokenTransitions.GetDuration(transition) is { } duration &&
                !inputs.Contains(new TokenInput(duration, IsEasing: false)))
            {
                inputs.Add(new TokenInput(duration, IsEasing: false));
            }
            if (TokenTransitions.GetEasing(transition) is { } easing &&
                !inputs.Contains(new TokenInput(easing, IsEasing: true)))
            {
                inputs.Add(new TokenInput(easing, IsEasing: true));
            }
        }
        _inputs = inputs.ToArray();
        return _inputs;
    }

    private void ApplyValues(Transitions transitions, ReadOnlySpan<TokenValue> values)
    {
        var inputs = Inputs;
        foreach (var transition in transitions)
        {
            if (transition is not TransitionBase target)
            {
                continue;
            }
            if (TokenTransitions.GetDuration(target) is { } duration)
            {
                target.Duration = values[Array.IndexOf(inputs, new TokenInput(duration, IsEasing: false))].Duration;
            }
            if (TokenTransitions.GetEasing(target) is { } easing &&
                values[Array.IndexOf(inputs, new TokenInput(easing, IsEasing: true))].Easing is { } resolved)
            {
                target.Easing = (Easing)ThemeResourceValue.CloneForConsumer(resolved)!;
            }
        }
    }

    private Transitions Build() =>
        TemplateContent.Load<Transitions>(_content)?.Result ??
        throw new InvalidOperationException("TokenTransitions content did not build a Transitions instance.");

    internal readonly record struct TokenInput(SharedTokenKind Kind, bool IsEasing);
    internal readonly record struct TokenValue(TimeSpan Duration, Easing? Easing);
    private readonly record struct TokenKey(TimeSpan Duration, TokenValueOperations.EasingKey Easing);
    private sealed record Variant(TokenKey[] Values, Transitions Transitions);
}
