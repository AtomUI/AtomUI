using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Resolves a global Design Token through the Effective Global Token of the control that owns the
/// target element, for shared primitives that have no Token identity of their own.
/// </summary>
/// <remarks>
/// <para>
/// AXAML usage: <c>&lt;Setter Property="BorderBrush" Value="{atom:OwnerTokenResource ColorBorder}" /&gt;</c>.
/// The owner is the nearest registered public control above the target, read from the inherited
/// <c>ThemeScope.TokenOwner</c> value, so a per-control override such as
/// <c>ThemeConfig.Controls[identity].Tokens["ColorBorder"]</c> reaches every primitive in that control's
/// template, including popup content whose logical parent chain leads back to the owner. Without an owner
/// (or with an owner the active schema does not register) the plain global value is used, exactly like
/// <see cref="SharedTokenResourceExtension" />.
/// </para>
/// <para>
/// The returned binding is instantiated per target element; the owner and the resource are resolved at
/// that point, never while the markup is being loaded. The service provider is only asked whether the
/// markup sits in a control template, as <c>DynamicResourceExtension</c> does, so direct use on a template
/// element gets Template priority. Only targets that are <see cref="Avalonia.StyledElement" /> instances can
/// resolve a value.
/// </para>
/// </remarks>
public sealed class OwnerTokenResourceExtension : MarkupExtension
{
    /// <summary>
    /// The global Design Token to resolve.
    /// </summary>
    public SharedTokenKind? Kind { get; set; }

    /// <summary>
    /// Creates an extension whose <see cref="Kind" /> is assigned later.
    /// </summary>
    public OwnerTokenResourceExtension()
    {
    }

    /// <summary>
    /// Creates an extension for the given global Design Token.
    /// </summary>
    public OwnerTokenResourceExtension(SharedTokenKind kind)
    {
        Kind = kind;
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Kind is not { } kind)
        {
            throw new InvalidOperationException("A Token resource key is required.");
        }
        // Same template-context check as DynamicResourceExtension: values inside a ControlTemplate use
        // Template priority so triggered styles can still override them.
        var priority = serviceProvider.GetService(typeof(IAvaloniaXamlIlControlTemplateProvider)) is not null
            ? BindingPriority.Template
            : BindingPriority.LocalValue;
        return OwnerTokenBinding.Create(kind, priority);
    }
}
