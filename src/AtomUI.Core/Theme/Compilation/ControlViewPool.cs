using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// The global view a control derivation starts from, shared between the controls of one evaluation unit (spec 12.2).
/// A control whose derivation writes no global token and that has no global override returns its view unchanged, and
/// the next control reuses it instead of copying the scope's global tokens again. A view that was written is dropped.
/// </summary>
internal sealed class ControlViewPool
{
    private readonly DesignToken _source;
    private readonly IReadOnlyList<TokenDescriptor> _descriptors;
    private DesignToken? _spare;

    internal ControlViewPool(DesignToken source, IReadOnlyList<TokenDescriptor> descriptors)
    {
        _source      = source;
        _descriptors = descriptors;
    }

    /// <summary>An unwritten copy of the scope's global tokens; the caller owns it until it returns it.</summary>
    internal DesignToken Rent() => Interlocked.Exchange(ref _spare, null) ?? ThemeCompiler.CopyBuilder(_source, _descriptors);

    /// <summary>Hands back a view that still equals the scope's global tokens.</summary>
    internal void Return(DesignToken view) => Interlocked.CompareExchange(ref _spare, view, null);
}
