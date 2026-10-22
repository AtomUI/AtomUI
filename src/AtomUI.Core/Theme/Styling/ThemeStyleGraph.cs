using System.Runtime.CompilerServices;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AtomUI.Theme.Styling;

internal static class ThemeStyleGraph
{
    // Weak keys detect graph reuse without retaining retired styles or their assets.
    private static readonly ConditionalWeakTable<Style, object> s_prepared = new();
    private static readonly object s_marker = new();

    internal static void Prepare(Style root, Selector qualifiedSelector)
    {
        Dispatcher.UIThread.VerifyAccess();
        var nodes = new List<Style>();
        var visited = new HashSet<Style>(ReferenceEqualityComparer.Instance);
        Validate(root, null, nodes, visited);

        var originalSelector = root.Selector;
        root.Selector = qualifiedSelector;
        try
        {
            foreach (var node in nodes)
            {
                for (var index = 0; index < node.Children.Count; index++)
                {
                    // Children may have been authored before the root had a selector.
                    // Reassignment invokes native SetParent/nesting validation. It does
                    // not attach an owner: the entire graph is still unmounted here.
                    node.Children[index] = node.Children[index];
                }
            }
        }
        catch
        {
            root.Selector = originalSelector;
            throw;
        }

        // UI-thread preparation cannot race another claim; failed validation claims none.
        foreach (var node in nodes)
        {
            s_prepared.Add(node, s_marker);
        }
    }

    private static void Validate(
        Style node,
        Style? expectedParent,
        List<Style> nodes,
        HashSet<Style> visited)
    {
        if (!visited.Add(node))
        {
            throw new InvalidOperationException("A managed style graph cannot share or cycle through a node.");
        }
        if (node.Owner is not null || !ReferenceEquals(node.Parent, expectedParent))
        {
            throw new InvalidOperationException("A managed style graph must be unmounted with consistent parents.");
        }
        if (s_prepared.TryGetValue(node, out _))
        {
            throw new InvalidOperationException("A managed style node can only be prepared once.");
        }

        var resources = node.Resources;
        if (resources.Count != 0 || resources.MergedDictionaries.Count != 0 || resources.ThemeDictionaries.Count != 0)
        {
            throw new InvalidOperationException("A candidate style cannot publish public resources before activation.");
        }

        nodes.Add(node);
        foreach (var child in node.Children)
        {
            if (child is not Style childStyle)
            {
                throw new InvalidOperationException("The managed style profile only supports native Style nodes.");
            }
            Validate(childStyle, node, nodes, visited);
        }
    }
}
