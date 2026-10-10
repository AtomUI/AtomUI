using System.Diagnostics.CodeAnalysis;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

public sealed class ControlTokenDescriptor
{
    private readonly Func<AbstractControlDesignToken>? _factory;
    private readonly Action<AbstractControlDesignToken, ThemeAppearance>? _evaluator;

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

    private ControlTokenDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        int slot,
        IEnumerable<TokenDescriptor> ownTokens,
        Func<AbstractControlDesignToken>? factory,
        Action<AbstractControlDesignToken, ThemeAppearance>? evaluator)
    {
        ArgumentNullException.ThrowIfNull(controlType);
        ArgumentNullException.ThrowIfNull(ownTokens);
        if (!typeof(Control).IsAssignableFrom(controlType))
        {
            throw new ArgumentException(
                $"Control Token type '{controlType.FullName}' must derive from Avalonia.Controls.Control.",
                nameof(controlType));
        }

        var ownTokenArray = ownTokens.OrderBy(static token => token.Slot).ToArray();
        if (ownTokenArray.Length == 0)
        {
            if (factory is not null || evaluator is not null)
            {
                throw new ArgumentException(
                    "A Control without Own Tokens cannot define a Token factory or evaluator.",
                    nameof(factory));
            }
        }
        else if (factory is null || evaluator is null)
        {
            throw new ArgumentException(
                "A Control with Own Tokens requires a Token factory and evaluator.",
                nameof(factory));
        }

        ControlType = controlType;
        Identity    = identity;
        Slot        = slot;
        OwnTokens   = Array.AsReadOnly(ownTokenArray);
        _factory    = factory;
        _evaluator  = evaluator;
    }

    private ControlTokenDescriptor(ControlTokenDescriptor source, int slot)
    {
        ControlType = source.ControlType;
        Identity    = source.Identity;
        Slot        = slot;
        // Public construction already copied, ordered and sealed this metadata.
        OwnTokens   = source.OwnTokens;
        _factory    = source._factory;
        _evaluator  = source._evaluator;
    }

    public Type ControlType { get; }
    public ControlTokenIdentity Identity { get; }
    public int Slot { get; }
    public IReadOnlyList<TokenDescriptor> OwnTokens { get; }
    public bool HasOwnTokens => _factory is not null;

    public AbstractControlDesignToken? CreateBuilder() => _factory?.Invoke();

    public void Evaluate(AbstractControlDesignToken token, ThemeAppearance appearance)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (_evaluator is null)
        {
            throw new InvalidOperationException($"Control '{Identity}' does not define Own Tokens.");
        }
        _evaluator(token, appearance);
    }

    internal ControlTokenDescriptor Bind(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        return new ControlTokenDescriptor(this, slot);
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
