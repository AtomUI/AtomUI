using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;

namespace AtomUI.Theme.Resources;

// Internal shared-primitives input: resolve the registered public owner while
// Avalonia's native binding/resource hosts retain priority and subscription lifetime.
internal sealed class OwnerTokenResourceExtension : MarkupExtension
{
    public SharedTokenKind Kind { get; set; }
    public bool WhenControlExplicit { get; set; }

    public OwnerTokenResourceExtension() { }

    public OwnerTokenResourceExtension(SharedTokenKind kind)
    {
        Kind = kind;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        CreateBinding(Kind, WhenControlExplicit, WhenControlExplicit);

    internal static BindingBase CreateBinding(SharedTokenKind kind, bool whenExplicit = false, bool controlOnly = false)
    {
        var source = new ClrPropertyInfo("OwnerTokenValue", value =>
        {
            var owner = (Control)value;
            _ = ThemeScope.ResolveContext(owner);
            // Content add-ons are logical children with no TemplatedParent. Re-evaluate
            // their route on attachment/reparenting without subscribing to every ancestor.
            return Observable.Merge(
                    owner.GetObservable(ThemeScope.ContextProperty).Select(_ => 0),
                    owner.GetObservable(StyledElement.ParentProperty).Select(_ => 0),
                    owner.GetObservable(StyledElement.TemplatedParentProperty).Select(_ => 0),
                    Observable.FromEventPattern<LogicalTreeAttachmentEventArgs>(
                        handler => owner.AttachedToLogicalTree += handler,
                        handler => owner.AttachedToLogicalTree -= handler).Select(_ => 0))
                .Select(_ =>
                {
                    var context = owner.GetValue(ThemeScope.ContextProperty) ?? ThemeScope.ResolveContext(owner);
                    return (Context: context, Key: ResolveResourceKey(owner, context, kind));
                })
                .DistinctUntilChanged()
                .Select(source => source.Context is null || (whenExplicit && source.Key is SharedTokenKind)
                    ? Observable.Return<object?>(AvaloniaProperty.UnsetValue)
                    : owner.GetResourceObservable(whenExplicit ? new ExplicitTokenResourceKey(source.Key, controlOnly) : source.Key))
                .Switch();
        }, null, typeof(IObservable<object?>));
        return new CompiledBindingExtension(new CompiledBindingPathBuilder().Self()
            .Property(source, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
            .StreamObservable<object?>().Build());
    }

    private static object ResolveResourceKey(Control owner, ThemeContext? context, SharedTokenKind kind)
    {
        if (context is null)
        {
            return kind;
        }
        for (Control? candidate = owner; candidate is not null;
             candidate = candidate.TemplatedParent as Control ?? candidate.Parent as Control)
        {
            var registry = context.Snapshot.Registry;
            if (registry.TryGetControl(candidate.GetType(), out var descriptor) ||
                registry.TryGetControl(candidate.StyleKey, out descriptor))
            {
                return ControlTokenResourceKey.Global(descriptor.Identity, kind);
            }
        }
        return kind;
    }
}
