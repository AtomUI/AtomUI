using System.Diagnostics.CodeAnalysis;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Registers one control with the schema of its control Tokens: which Tokens its Token class declares and the
/// factory that creates instances of that class. The generated registration of a control package creates one per
/// control, with or without own Tokens, and the theme schema registry assigns each its <see cref="Slot" />.
/// </summary>
public sealed class ControlTokenDescriptor
{
    private readonly Func<AbstractControlDesignToken>? _factory;

    /// <summary>
    /// Creates the descriptor of a control that declares no own Tokens, so it has no Token factory.
    /// </summary>
    /// <param name="controlType">
    /// The control type; it must derive from <see cref="Control" />.
    /// </param>
    /// <param name="identity">
    /// The identity of the control's Token owner.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="controlType" /> does not derive from <see cref="Control" />.
    /// </exception>
    public ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity)
        : this(
            controlType,
            identity,
            -1,
            Array.Empty<TokenDescriptor>(),
            null)
    {
    }

    /// <summary>
    /// Creates the descriptor of a control that declares own Tokens.
    /// </summary>
    /// <param name="controlType">
    /// The control type; it must derive from <see cref="Control" />.
    /// </param>
    /// <param name="identity">
    /// The identity of the control's Token owner.
    /// </param>
    /// <param name="ownTokens">
    /// One descriptor per Token the class declares. The descriptor stores them in <see cref="OwnTokens" /> as a
    /// read-only list ordered by slot.
    /// </param>
    /// <param name="factory">
    /// Creates a new instance of the Token class on every call.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="controlType" /> does not derive from <see cref="Control" />, or
    /// <paramref name="ownTokens" /> and <paramref name="factory" /> do not agree: own Tokens require a factory
    /// and a control without own Tokens must not have one.
    /// </exception>
    public ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken> factory)
        : this(controlType, identity, -1, ownTokens, factory)
    {
    }

    private ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        int slot,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken>? factory)
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
            if (factory is not null)
            {
                throw new ArgumentException(
                    "A Control without control tokens cannot define a Token factory.",
                    nameof(factory));
            }
        }
        else if (factory is null)
        {
            throw new ArgumentException(
                "A Control with control tokens requires a Token factory.",
                nameof(factory));
        }

        ControlType = controlType;
        CompilationIdentity = new object();
        Identity    = identity;
        Slot        = slot;
        OwnTokens   = Array.AsReadOnly(ownTokenArray);
        _factory    = factory;
    }

    private ControlTokenDescriptor(ControlTokenDescriptor source, int slot)
    {
        ControlType = source.ControlType;
        CompilationIdentity = source.CompilationIdentity;
        Identity    = source.Identity;
        Slot        = slot;
        // Public construction already copied, ordered and sealed this metadata.
        OwnTokens   = source.OwnTokens;
        _factory    = source._factory;
    }

    /// <summary>
    /// The type of the control the Tokens belong to. The registry looks controls up by it and matches it
    /// with the owner recorded by the generated registration.
    /// </summary>
    public Type ControlType { get; }
    internal object CompilationIdentity { get; }
    /// <summary>
    /// The catalog and id of the control's Token owner; the key the registry, value packages and theme assets use
    /// to name the control.
    /// </summary>
    public ControlTokenIdentity Identity { get; }
    /// <summary>
    /// The index of the control in the registry's control array, or -1 until the registry assigns it. The
    /// slots of the control's own Tokens are relative to the control and start at zero.
    /// </summary>
    public int Slot { get; }
    /// <summary>
    /// The descriptors of the Tokens the control's Token class declares, ordered by slot; the global Tokens it
    /// reads are not listed. The registry requires the slots to be contiguous from zero.
    /// </summary>
    public IReadOnlyList<TokenDescriptor> OwnTokens { get; }
    /// <summary>
    /// Whether the control declares own Tokens. Only then does <see cref="CreateBuilder" /> return an instance.
    /// </summary>
    public bool HasOwnTokens => _factory is not null;

    /// <summary>
    /// Creates a new instance of the control's Token class for the theme compiler to fill with the control's Token
    /// values.
    /// </summary>
    /// <returns>
    /// A new instance on every call, or <see langword="null" /> when the control declares no own Tokens.
    /// </returns>
    public AbstractControlDesignToken? CreateBuilder() => _factory?.Invoke();

    internal ControlTokenDescriptor Bind(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        return new ControlTokenDescriptor(this, slot);
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
