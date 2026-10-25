using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Reflection;

// Avalonia 12.1.4 clears replaced parts' TemplatedParent without retiring their
// TemplatedParentTheme frames. Ancestor activators can then retain the old tree.
// Track only explicitly retiring templates; remove after verified upstream cleanup.
internal static class TemplatePartThemeCompatibility
{
    private static readonly ConditionalWeakTable<StyledElement, object> s_tracked = new();
    private static readonly object s_marker = new();
    private static readonly Lazy<MethodInfo> s_invalidateTheme = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(StyledElement).GetMethod("OnTemplatedParentControlThemeChanged",
                BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(StyledElement), "OnTemplatedParentControlThemeChanged"));

    internal static void TrackTemplateReplacement(TemplatedControl owner)
    {
        owner.Dispatcher.VerifyAccess();
        foreach (var part in owner.GetVisualDescendants().OfType<StyledElement>())
        {
            if (part.TemplatedParent is not null && !s_tracked.TryGetValue(part, out _))
            {
                s_tracked.Add(part, s_marker);
                part.PropertyChanged += HandleTemplatedParentChanged;
            }
        }
    }

    internal static void SetTemplatedParent(StyledElement part, AvaloniaObject? parent)
    {
        if (ReferenceEquals(part.TemplatedParent, parent))
        {
            return;
        }

        part.SetTemplatedParent(parent);
        InvalidateTheme(part);
        if (((ILogical)part).IsAttachedToLogicalTree)
        {
            part.ApplyStyling();
        }
    }

    private static void HandleTemplatedParentChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == StyledElement.TemplatedParentProperty &&
            change.OldValue is not null && sender is StyledElement part)
        {
            part.PropertyChanged -= HandleTemplatedParentChanged;
            s_tracked.Remove(part);
            InvalidateTheme(part);
        }
    }

    [DynamicDependency("OnTemplatedParentControlThemeChanged", typeof(StyledElement))]
    private static void InvalidateTheme(StyledElement part) => s_invalidateTheme.Value.Invoke(part, null);
}
