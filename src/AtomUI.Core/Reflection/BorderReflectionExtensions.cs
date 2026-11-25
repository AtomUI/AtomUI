using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace AtomUI.Reflection;

internal static class BorderReflectionExtensions
{
    private static readonly Lazy<FieldInfo> CompositionCache = new(ResolveCompositionCache);

    [DynamicDependency("_borderVisual", typeof(Border))]
    private static FieldInfo ResolveCompositionCache()
    {
        var field = FixedMemberReflection.RequireField(
            typeof(Border).GetField("_borderVisual", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(Border), "_borderVisual");
        if (!typeof(CompositionVisual).IsAssignableFrom(field.FieldType))
        {
            throw new NotSupportedException("The framework Border composition cache contract changed.");
        }
        return field;
    }

    internal static void ReleaseDetachedCompositionCache(this Border border)
    {
        // Revalidate this fixed compatibility boundary on framework upgrades.
        // Border retains its old visual after Visual has detached; a reusable
        // presenter can otherwise retain the discarded popup's ancestor chain.
        // Attach recreates this cache. Never touch an attached/current visual.
        if (!border.IsAttachedToVisualTree() && ElementComposition.GetElementVisual(border) is null)
        {
            CompositionCache.Value.SetValue(border, null);
        }
    }
}
