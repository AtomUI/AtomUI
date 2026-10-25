using Avalonia;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace AtomUI.Utils;

// Avalonia 12.1.4 Border retains its detached composition visual, including its
// managed parent chain. Retire the disappearing popup scene before native detach
// clears Visual.CompositionVisual, so reused content cannot retain the old host.
internal static class CompositionTreeRetirement
{
    internal static void Retire(Visual root)
    {
        root.Dispatcher.VerifyAccess();
        foreach (var element in root.GetVisualDescendants().Prepend(root))
        {
            if (ElementComposition.GetElementVisual(element) is CompositionContainerVisual visual)
            {
                // Clear only framework visuals owned by actual Visual nodes.
                // Application ChildCompositionVisual subtrees keep their children.
                visual.Children.Clear();
            }
        }
    }
}
