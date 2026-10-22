namespace AtomUI.Theme.DesignTokens;

/// <summary>Read-only Global Token projection; getters are emitted from the complete Global schema.</summary>
public readonly ref partial struct ControlTokenGlobalValues
{
    private readonly DesignToken? _source;

    internal ControlTokenGlobalValues(DesignToken source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    private DesignToken Source => _source ??
        throw new InvalidOperationException("The Global Token projection has not been initialized.");
}
