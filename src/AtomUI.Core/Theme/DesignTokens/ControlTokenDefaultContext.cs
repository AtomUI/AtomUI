namespace AtomUI.Theme.DesignTokens;

public readonly ref struct ControlTokenDefaultContext
{
    private readonly DesignToken? _effectiveGlobal;
    private readonly ThemeAppearance _appearance;

    internal ControlTokenDefaultContext(DesignToken effectiveGlobal, ThemeAppearance appearance)
    {
        ArgumentNullException.ThrowIfNull(effectiveGlobal);
        _effectiveGlobal = effectiveGlobal;
        _appearance = appearance;
    }

    public ThemeAppearance Appearance
    {
        get
        {
            EnsureInitialized();
            return _appearance;
        }
    }

    public ControlTokenGlobalValues Global
    {
        get
        {
            EnsureInitialized();
            return new ControlTokenGlobalValues(_effectiveGlobal!);
        }
    }

    internal void EnsureInitialized()
    {
        if (_effectiveGlobal is null)
        {
            throw new InvalidOperationException("The Control default context has not been initialized.");
        }
    }
}
