using System.Diagnostics;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace AtomUI.Theme.Resources;

public abstract class TokenResourceExtension<TTokenKind> : MarkupExtension
    where TTokenKind : Enum
{
    public TTokenKind? Kind { get; set; }

    /// <summary>Apply a registered Control Global or Own resource only when normalized configuration explicitly supplies its field.</summary>
    public bool WhenExplicit { get; set; }
    
    public TokenResourceExtension()
    {
    }

    public TokenResourceExtension(TTokenKind kind)
    {
        Kind = kind;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        Debug.Assert(Kind != null);
        if (Kind is not { } kind)
        {
            throw new InvalidOperationException("A Token resource key is required.");
        }
        var key = GetResourceKey(kind);
        return new DynamicResourceExtension(WhenExplicit ? new ExplicitTokenResourceKey(key) : key);
    }

    protected virtual object GetResourceKey(TTokenKind kind) => kind;
}
