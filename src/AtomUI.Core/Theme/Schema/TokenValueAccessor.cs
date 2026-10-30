using System.ComponentModel;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Generated per-token-class accessor. <c>slot</c> is the owning descriptor's <see cref="TokenDescriptor.Slot"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TokenValueAccessor
{
    public abstract object? GetValue(AbstractDesignToken token, int slot);

    public abstract void SetValue(AbstractDesignToken token, int slot, object? value);

    public abstract object? ProjectResourceValue(AbstractDesignToken token, int slot);
}

internal sealed class DelegateTokenValueAccessor : TokenValueAccessor
{
    private readonly Func<AbstractDesignToken, object?> _getter;
    private readonly Action<AbstractDesignToken, object?> _setter;
    private readonly Func<AbstractDesignToken, object?> _resourceProjector;

    internal DelegateTokenValueAccessor(
        Func<AbstractDesignToken, object?> getter,
        Action<AbstractDesignToken, object?> setter,
        Func<AbstractDesignToken, object?> resourceProjector)
    {
        _getter            = getter;
        _setter            = setter;
        _resourceProjector = resourceProjector;
    }

    public override object? GetValue(AbstractDesignToken token, int slot) => _getter(token);

    public override void SetValue(AbstractDesignToken token, int slot, object? value) => _setter(token, value);

    public override object? ProjectResourceValue(AbstractDesignToken token, int slot) => _resourceProjector(token);
}
