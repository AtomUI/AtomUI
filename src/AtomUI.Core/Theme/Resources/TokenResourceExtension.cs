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
    /// <summary>
    /// The kind of Token to read. It is <see langword="null" /> until it is assigned, and <see cref="ProvideValue" />
    /// throws while it is.
    /// </summary>
    public TTokenKind? Kind { get; set; }

    public TokenResourceExtension()
    {
    }

    /// <summary>
    /// Creates an extension that reads the Token <paramref name="kind" />.
    /// </summary>
    /// <param name="kind">
    /// The Token to read.
    /// </param>
    public TokenResourceExtension(TTokenKind kind)
    {
        Kind = kind;
    }

    /// <summary>
    /// Returns the dynamic resource read of <see cref="Kind" />. Inside a control template the read is created
    /// through <see cref="DynamicResourceExtension" /> with Template priority; outside a control template the
    /// <see cref="DynamicResourceExtension" /> that holds the resource key is returned unchanged.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider of the markup extension. Only <see cref="IAvaloniaXamlIlControlTemplateProvider" />
    /// is requested from it, to detect a control template.
    /// </param>
    /// <returns>
    /// The value provided to the target property.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Kind" /> is <see langword="null" />.
    /// </exception>
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
            // The synthetic provider answers only IAvaloniaXamlIlControlTemplateProvider and returns null for
            // IProvideValueTarget and every other service. Verified against Avalonia 12.1.5, whose
            // DynamicResourceExtension.ProvideValue tolerates both (it only reads the template context and the
            // optional anchor); TokenResourceExtensionTests.ProvideValue_In_Control_Template_Uses_Template_Priority
            // is the contract that must go red if an Avalonia upgrade starts requiring either service.
            return extension.ProvideValue(new ControlTemplateContext(templateProvider));
        }
        return extension;
    }

    /// <summary>
    /// Returns the resource key that addresses the Token kind. <see cref="ProvideValue" /> calls it once for
    /// each read.
    /// </summary>
    /// <param name="kind">
    /// The Token kind to look up; never <see langword="null" />.
    /// </param>
    /// <returns>
    /// The resource key. The base implementation returns <paramref name="kind" /> itself; the generated Token
    /// resource extensions override it.
    /// </returns>
    protected virtual object GetResourceKey(TTokenKind kind) => kind;

    private sealed class ControlTemplateContext(object templateProvider) : IServiceProvider
    {
        public object? GetService(Type serviceType) =>
            serviceType == typeof(IAvaloniaXamlIlControlTemplateProvider) ? templateProvider : null;
    }
}
