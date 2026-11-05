using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Converters;
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
    public bool WhenExplicit { get; set; }
    public bool WhenControlExplicit { get; set; }
    public Type? SourceControlType { get; set; }
    public Type? AnchorControlType { get; set; }
    public BindingPriority Priority { get; set; } = BindingPriority.LocalValue;
    public IValueConverter? Converter { get; set; }

    public OwnerTokenResourceExtension() { }

    public OwnerTokenResourceExtension(SharedTokenKind kind)
    {
        Kind = kind;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        CreateBinding(Kind, WhenExplicit || WhenControlExplicit, WhenControlExplicit, SourceControlType, AnchorControlType, Priority, Converter);

    internal static BindingBase CreateBinding(SharedTokenKind kind, bool whenExplicit = false, bool controlOnly = false, Type? sourceControlType = null, Type? anchorControlType = null, BindingPriority priority = BindingPriority.LocalValue, IValueConverter? converter = null)
    {
        // Re-evaluating a compiled accessor for the same target must retain its
        // stream identity. The weak key owns only the observable definition;
        // current routes/values and subscription disposal stay binding-owned.
        var sources = new ConditionalWeakTable<Control, IObservable<object?>>();
        ConditionalWeakTable<Control, IObservable<object?>>.CreateValueCallback createSource = owner =>
        {
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
                    return (Context: context, Key: ResolveResourceKey(owner, context, kind, sourceControlType, anchorControlType));
                })
                .DistinctUntilChanged()
                .Select(source => source.Context is null || (whenExplicit && source.Key is SharedTokenKind)
                    ? Observable.Return<object?>(AvaloniaProperty.UnsetValue)
                    : owner.GetResourceObservable(whenExplicit ? new ExplicitTokenResourceKey(source.Key, controlOnly) : source.Key))
                .Switch();
        };
        var source = new ClrPropertyInfo("OwnerTokenValue", value =>
        {
            var owner = (Control)value;
            _ = ThemeScope.ResolveContext(owner);
            return sources.GetValue(owner, createSource);
        }, null, typeof(IObservable<object?>));
        return new CompiledBindingExtension(new CompiledBindingPathBuilder().Self()
            .Property(source, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
            .StreamObservable<object?>().Build()) { Priority = priority, Converter = converter };
    }

    private static object ResolveResourceKey(Control owner, ThemeContext? context, SharedTokenKind kind, Type? sourceControlType, Type? anchorControlType)
    {
        if (context is null)
        {
            return kind;
        }
        var registry = context.Snapshot.Registry;
        // A composition theme owns its template inputs even when the target is a
        // separately registered primitive. Start lookup at that theme's actual
        // instance; derived internal themes retain the concrete public parent.
        var anchorReached = anchorControlType is null;
        for (Control? candidate = owner; candidate is not null;
             candidate = candidate.TemplatedParent as Control ?? candidate.Parent as Control)
        {
            anchorReached |= anchorControlType?.IsInstanceOfType(candidate) == true;
            if (!anchorReached)
            {
                continue;
            }
            if (sourceControlType is not null && candidate.GetType() != sourceControlType && candidate.StyleKey != sourceControlType)
            {
                continue;
            }
            if (candidate is IControlThemeGlobalOwnerSource { ThemeGlobalOwner: { } actualOwner } &&
                (registry.TryGetControl(actualOwner.GetType(), out var actual) || registry.TryGetControl(actualOwner.StyleKey, out actual)))
            {
                return ControlTokenResourceKey.Global(actual.Identity, kind);
            }
            if (registry.TryGetControl(candidate.GetType(), out var descriptor) ||
                registry.TryGetControl(candidate.StyleKey, out descriptor))
            {
                return ControlTokenResourceKey.Global(descriptor.Identity, kind);
            }
        }
        if (sourceControlType is not null)
        {
            // A standalone use of a composition theme retains its declared
            // resource identity when no matching owner exists in the tree.
            return ControlTokenResourceKey.Global(sourceControlType, kind);
        }
        return kind;
    }
}
