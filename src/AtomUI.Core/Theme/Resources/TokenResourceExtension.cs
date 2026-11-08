using System.Diagnostics;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.XamlIl.Runtime;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Base markup extension for Design Token reads: resolves the token's resource key as a dynamic resource.
/// </summary>
/// <remarks>
/// Like <c>DynamicResource</c>, a read written directly on a ControlTemplate element uses Template priority,
/// so triggered styles (<c>:pointerover</c>, <c>:disabled</c>, classes) on the same property override it,
/// while plain (non-triggered) style setters do not. Outside templates the read keeps the priority of the
/// place it is written in (LocalValue on an element, the style's priority in a Setter). The service
/// provider is only asked whether the markup sits in a control template.
/// </remarks>
public abstract class TokenResourceExtension<TTokenKind> : MarkupExtension
    where TTokenKind : Enum
{
    public TTokenKind? Kind { get; set; }

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
        var extension = new DynamicResourceExtension(GetResourceKey(kind));
        if (serviceProvider.GetService(typeof(IAvaloniaXamlIlControlTemplateProvider)) is { } templateProvider)
        {
            // Let DynamicResourceExtension pick Template priority itself; it sees only the template
            // context, so it captures no anchor and no dictionary theme variant, exactly as before.
            return extension.ProvideValue(new ControlTemplateContext(templateProvider));
        }
        return extension;
    }

    protected virtual object GetResourceKey(TTokenKind kind) => kind;

    private sealed class ControlTemplateContext(object templateProvider) : IServiceProvider
    {
        public object? GetService(Type serviceType) =>
            serviceType == typeof(IAvaloniaXamlIlControlTemplateProvider) ? templateProvider : null;
    }
}
