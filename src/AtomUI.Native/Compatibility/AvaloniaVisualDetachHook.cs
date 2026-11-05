using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Native;

/// <summary>
/// Releases Avalonia 12.1.4's cached Border composition visual after detachment.
/// The cache otherwise retains the retired composition parent and its window.
/// </summary>
internal static class AvaloniaVisualDetachHook
{
    private static readonly FieldInfo BorderVisual =
        typeof(Border).GetField("_borderVisual", BindingFlags.Instance | BindingFlags.NonPublic) ??
        throw new NotSupportedException("Unsupported Avalonia Border composition cache shape.");

    internal static IDisposable Attach() =>
        Visual.VisualParentProperty.Changed.AddClassHandler<Visual>(HandleVisualParentChanged);

    private static void HandleVisualParentChanged(Visual visual, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.OldValue is not Visual || visual.IsAttachedToVisualTree())
        {
            return;
        }

        ReleaseBorderCache(visual);
        // Only the detached subtree root changes VisualParent. Descendants have
        // also detached from their compositor, while keeping their logical tree.
        foreach (var child in visual.GetVisualDescendants())
        {
            ReleaseBorderCache(child);
        }
    }

    private static void ReleaseBorderCache(Visual visual)
    {
        if (visual is Border border && !border.IsAttachedToVisualTree())
        {
            // Visual.DetachFromCompositor has released CompositionVisual already.
            // Border's additional cache is recreated by CreateCompositionVisual
            // on reattachment; no control property or live composition is changed.
            BorderVisual.SetValue(border, null);
        }
    }
}
