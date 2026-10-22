using System.ComponentModel;

namespace AtomUI.Theme.DesignTokens;

/// <summary>Generated entry point for a dependency-defaults terminal definition.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IControlTokenDefaults
{
    static abstract void EvaluateDefaults(AbstractControlDesignToken token,
        in ControlTokenDefaultContext context, ReadOnlySpan<ulong> explicitOwnSlots);
}

[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ControlTokenDefaultsEvaluator(AbstractControlDesignToken token,
    in ControlTokenDefaultContext context, ReadOnlySpan<ulong> explicitOwnSlots);
