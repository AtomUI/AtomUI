using AtomUI.Theme.Schema;
using Avalonia.Animation;
using Avalonia.Markup.Xaml.Templates;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Builds the <see cref="Transitions"/> of one <see cref="TokenTransitions"/> markup for resolved Token durations.
/// </summary>
/// <remarks>
/// <para>
/// The declared Tokens are discovered from the first build. Each distinct duration vector gets one built instance
/// that every target resolving the same durations shares; Avalonia keeps transition state per animated control, and
/// a built instance is never mutated after it is handed out. At most <see cref="MaxSharedVariants"/> vectors are
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
    private SharedTokenKind[]? _kinds;
    private object[]? _globalKeys;
    private Transitions? _prototype;

    internal TokenTransitionsFactory(object content)
    {
        _content = content;
    }

    /// <summary>The distinct declared Tokens, in declaration order.</summary>
    internal SharedTokenKind[] Kinds => _kinds ?? DiscoverKinds();

    /// <summary>The plain global resource keys of <see cref="Kinds"/>, boxed once.</summary>
    internal object[] GlobalKeys
    {
        get
        {
            if (_globalKeys is null)
            {
                var kinds = Kinds;
                var keys  = new object[kinds.Length];
                for (var index = 0; index < kinds.Length; index++)
                {
                    keys[index] = kinds[index];
                }
                _globalKeys = keys;
            }
            return _globalKeys;
        }
    }

    internal int SharedVariantCount => _variants.Count;

    /// <summary>The owner's unbound Effective Global keys of <see cref="Kinds"/>, created once per owner.</summary>
    internal object[] GetOwnerKeys(ControlTokenIdentity owner)
    {
        if (!_ownerKeys.TryGetValue(owner, out var keys))
        {
            var kinds = Kinds;
            keys = new object[kinds.Length];
            for (var index = 0; index < kinds.Length; index++)
            {
                keys[index] = OwnerTokenBinding.GetResourceKey(owner, kinds[index]);
            }
            _ownerKeys.Add(owner, keys);
        }
        return keys;
    }

    /// <summary>
    /// Returns the transitions for <paramref name="durations"/> (one value per entry of <see cref="Kinds"/>).
    /// Finding an existing vector does not allocate.
    /// </summary>
    internal Transitions GetTransitions(ReadOnlySpan<TimeSpan> durations)
    {
        foreach (var variant in _variants)
        {
            if (durations.SequenceEqual(variant.Durations))
            {
                return variant.Transitions;
            }
        }

        var transitions = _prototype ?? Build();
        _prototype = null;
        ApplyDurations(transitions, durations);
        if (_variants.Count < MaxSharedVariants)
        {
            _variants.Add(new Variant(durations.ToArray(), transitions));
        }
        return transitions;
    }

    private SharedTokenKind[] DiscoverKinds()
    {
        _prototype = Build();
        var kinds = new List<SharedTokenKind>();
        foreach (var transition in _prototype)
        {
            if (transition is TransitionBase transitionBase &&
                transitionBase.GetValue(TokenTransitions.DurationProperty) is { } kind &&
                !kinds.Contains(kind))
            {
                kinds.Add(kind);
            }
        }
        _kinds = kinds.ToArray();
        return _kinds;
    }

    private Transitions Build() =>
        TemplateContent.Load<Transitions>(_content)?.Result ??
        throw new InvalidOperationException("TokenTransitions content did not build a Transitions instance.");

    private void ApplyDurations(Transitions transitions, ReadOnlySpan<TimeSpan> durations)
    {
        var kinds = Kinds;
        foreach (var transition in transitions)
        {
            if (transition is TransitionBase transitionBase &&
                transitionBase.GetValue(TokenTransitions.DurationProperty) is { } kind)
            {
                transitionBase.Duration = durations[Array.IndexOf(kinds, kind)];
            }
        }
    }

    private sealed record Variant(TimeSpan[] Durations, Transitions Transitions);
}
