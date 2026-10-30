using System.Collections;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    [Flags]
    private enum TreePathTraversalOptions
    {
        None = 0,
        DisableMotion = 1,
        ExpandMatchedNodes = 1 << 1,
        RestoreExpandedState = 1 << 2,
        EnsureContainers = 1 << 3
    }

    private List<TreeViewItem>? TraverseTreeViewPath(
        TreeNodePath treeNodePath,
        TreePathTraversalOptions options,
        Action<TreeViewItem, int>? action = null)
    {
        if (treeNodePath.Length == 0)
        {
            return null;
        }

        var originIsMotionEnabled = IsMotionEnabled;
        var shouldDisableMotion    = (options & TreePathTraversalOptions.DisableMotion) != 0;
        var shouldRestoreExpanded  = (options & TreePathTraversalOptions.RestoreExpandedState) != 0;
        var shouldExpandMatched    = (options & TreePathTraversalOptions.ExpandMatchedNodes) != 0;
        var shouldEnsureContainers = (options & TreePathTraversalOptions.EnsureContainers) != 0;

        try
        {
            if (shouldDisableMotion)
            {
                SetCurrentValue(IsMotionEnabledProperty, false);
            }

            var segments             = treeNodePath.Segments;
            IList items              = Items;
            var pathNodes            = new List<TreeViewItem>(segments.Count);
            var pathNodeExpandStates = shouldRestoreExpanded ? new List<bool>(segments.Count) : null;
            try
            {
                for (var i = 0; i < segments.Count; i++)
                {
                    var treeViewItem = FindPathSegmentItem(items, segments[i]);
                    if (treeViewItem == null)
                    {
                        return null;
                    }

                    if (shouldRestoreExpanded)
                    {
                        pathNodeExpandStates!.Add(treeViewItem.IsExpanded);
                    }

                    if (shouldExpandMatched)
                    {
                        treeViewItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
                    }

                    if (shouldEnsureContainers)
                    {
                        EnsureTreeViewItemContainers(treeViewItem);
                    }

                    items = treeViewItem.Items;
                    pathNodes.Add(treeViewItem);
                    action?.Invoke(treeViewItem, i);
                }

                return pathNodes;
            }
            finally
            {
                if (pathNodeExpandStates != null)
                {
                    RestorePathExpandedStates(pathNodes, pathNodeExpandStates);
                }
            }
        }
        finally
        {
            if (shouldDisableMotion)
            {
                SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
            }
        }
    }

    private TreeViewItem? FindPathSegmentItem(IList items, string segment)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item == null)
            {
                continue;
            }

            if (TreeContainerFromItem(item) is not TreeViewItem treeViewItem)
            {
                return null;
            }

            if (IsPathSegmentMatched(treeViewItem, segment))
            {
                return treeViewItem;
            }
        }

        return null;
    }

    private static bool IsPathSegmentMatched(TreeViewItem treeViewItem, string segment)
    {
        if (treeViewItem.ItemKey != null && treeViewItem.ItemKey.Value == segment)
        {
            return true;
        }
        return treeViewItem.Value?.ToString() == segment;
    }

    private void EnsureTreeViewItemContainers(TreeViewItem treeViewItem)
    {
        if (treeViewItem.Presenter?.Panel != null)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.UpdateLayout();
    }

    private static void RestorePathExpandedStates(
        IReadOnlyList<TreeViewItem> pathNodes,
        IReadOnlyList<bool> pathNodeExpandStates)
    {
        for (var i = pathNodes.Count - 1; i >= 0; --i)
        {
            pathNodes[i].SetCurrentValue(TreeViewItem.IsExpandedProperty, pathNodeExpandStates[i]);
        }
    }
}
