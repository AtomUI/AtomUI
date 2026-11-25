using System.Diagnostics.CodeAnalysis;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

public sealed class ControlTokenDescriptor
{
    private readonly Func<AbstractControlDesignToken>? _factory;

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

    public Type ControlType { get; }
    internal object CompilationIdentity { get; }
    public ControlTokenIdentity Identity { get; }
    public int Slot { get; }
    public IReadOnlyList<TokenDescriptor> OwnTokens { get; }
    public bool HasOwnTokens => _factory is not null;

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
