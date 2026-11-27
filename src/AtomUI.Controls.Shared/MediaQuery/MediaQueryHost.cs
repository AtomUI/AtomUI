using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

/// <summary>
/// Finds the control whose breakpoints a responsive control has to follow; <c>GalleryShellView</c> in
/// AtomUI.Toolkits.GalleryBase uses it to attach its media owner.
/// </summary>
/// <remarks>
/// <c>FindOwner</c> walks from the visual up through its ancestors and returns the first one that implements
/// <c>IMediaBreakAwareControl</c>. If none does, it returns the top level of the visual when that implements the
/// interface, and otherwise <see langword="null" />.
/// </remarks>
internal static class MediaQueryHost
{
    internal static IMediaBreakAwareControl? FindOwner(Visual visual)
    {
        foreach (var ancestor in visual.GetSelfAndVisualAncestors())
        {
            if (ancestor is IMediaBreakAwareControl owner)
            {
                return owner;
            }
        }

        return TopLevel.GetTopLevel(visual) as IMediaBreakAwareControl;
    }
}
