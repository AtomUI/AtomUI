using System.Collections;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    private readonly record struct ItemsSourceReplayState(
        object[] SelectedItems,
        object[] CheckedItems,
        TreeNodePath? SelectedItemPath,
        List<TreeNodePath>? SelectedItemPaths,
        List<TreeNodePath>? CheckedItemPaths);

    private ItemsSourceReplayState? _itemsSourceReplayState;
    private int _itemsSourceReplayRevision;
    private int _itemsSourceReplayInternalDepth;
    private bool _hasExplicitSelectionUpdateDuringReplay;
    private bool _hasExplicitSelectionAssignmentDuringReplay;
    private bool _hasExplicitCheckedUpdateDuringReplay;
    private bool _hasExplicitCheckedAssignmentDuringReplay;
    private TreeNodePath? _explicitSelectedItemPath;
    private List<TreeNodePath>? _explicitSelectedItemPaths;
    private List<TreeNodePath>? _explicitCheckedItemPaths;

    private bool IsItemsSourceReplayPending => _itemsSourceReplayState.HasValue;

    private int BeginItemsSourceReplay()
    {
        var revision = ++_itemsSourceReplayRevision;
        if (IsLoaded && (_itemsSourceReplayState == null || _itemsSourceReplayInternalDepth > 0))
        {
            var previous = _itemsSourceReplayState;
            _hasExplicitSelectionUpdateDuringReplay = false;
            _hasExplicitSelectionAssignmentDuringReplay = false;
            _hasExplicitCheckedUpdateDuringReplay = false;
            _hasExplicitCheckedAssignmentDuringReplay = false;
            _explicitSelectedItemPath = null;
            _explicitSelectedItemPaths = null;
            _explicitCheckedItemPaths = null;
            _itemsSourceReplayState = new ItemsSourceReplayState(
                previous?.SelectedItems ?? CaptureSelectedItems(),
                previous?.CheckedItems ?? CheckedItems.Cast<object>().ToArray(),
                BuildNodeIdentityPath(SelectedItem as ITreeItemNode) ?? previous?.SelectedItemPath,
                BuildNodeIdentityPaths(SelectedItems) ?? previous?.SelectedItemPaths,
                BuildNodeIdentityPaths(CheckedItems) ?? previous?.CheckedItemPaths);
        }
        return revision;
    }

    private void CancelItemsSourceReplay()
    {
        _itemsSourceReplayRevision++;
        _itemsSourceReplayState = null;
        _hasExplicitSelectionUpdateDuringReplay = false;
        _hasExplicitSelectionAssignmentDuringReplay = false;
        _hasExplicitCheckedUpdateDuringReplay = false;
        _hasExplicitCheckedAssignmentDuringReplay = false;
        _explicitSelectedItemPath = null;
        _explicitSelectedItemPaths = null;
        _explicitCheckedItemPaths = null;
    }

    private void CompleteItemsSourceReplay(int revision)
    {
        if (revision != _itemsSourceReplayRevision || _itemsSourceReplayState is not { } state)
        {
            return;
        }
        if (!IsLoaded)
        {
            CancelItemsSourceReplay();
            return;
        }

        _itemsSourceReplayInternalDepth++;
        try
        {
            if (_hasExplicitSelectionAssignmentDuringReplay)
            {
                // A caller may mutate a plain IList after assigning SelectedItems. Such lists do
                // not raise collection notifications, so read the assigned value at replay time.
                _explicitSelectedItemPath = BuildNodeIdentityPath(SelectedItem as ITreeItemNode);
                _explicitSelectedItemPaths = BuildNodeIdentityPaths(SelectedItems);
            }
            if (_hasExplicitCheckedAssignmentDuringReplay)
            {
                // CheckedItems likewise accepts a plain IList that may change without notification.
                _explicitCheckedItemPaths = BuildNodeIdentityPaths(CheckedItems);
            }
            ConfigureStateAfterItemsSourceChanged(
                state,
                _hasExplicitSelectionUpdateDuringReplay,
                _hasExplicitCheckedUpdateDuringReplay);
        }
        finally
        {
            _itemsSourceReplayInternalDepth--;
            if (revision == _itemsSourceReplayRevision)
            {
                _itemsSourceReplayState = null;
                _hasExplicitSelectionUpdateDuringReplay = false;
                _hasExplicitSelectionAssignmentDuringReplay = false;
                _hasExplicitCheckedUpdateDuringReplay = false;
                _hasExplicitCheckedAssignmentDuringReplay = false;
                _explicitSelectedItemPath = null;
                _explicitSelectedItemPaths = null;
                _explicitCheckedItemPaths = null;
            }
        }

        if (revision != _itemsSourceReplayRevision)
        {
            // A property observer replaced ItemsSource during this replay. Its newer replay owns
            // the snapshot and will publish the final difference after the nested reset settles.
            return;
        }

        var selectedItems = CaptureSelectedItems();
        var previousSet = new HashSet<object>(state.SelectedItems, ReferenceEqualityComparer.Instance);
        var currentSet = new HashSet<object>(selectedItems, ReferenceEqualityComparer.Instance);
        var removed = state.SelectedItems.Where(item =>
            !currentSet.Contains(item)).ToArray();
        var added = selectedItems.Where(item =>
            !previousSet.Contains(item)).ToArray();
        if (removed.Length > 0 || added.Length > 0)
        {
            RaiseEvent(new SelectionChangedEventArgs(
                Avalonia.Controls.Primitives.SelectingItemsControl.SelectionChangedEvent,
                removed,
                added));
            NotifyFormValueChanged((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple
                ? SelectedItems
                : SelectedItem);
        }

        var checkedItems = CheckedItems.Cast<object>().ToArray();
        var previousCheckedSet = new HashSet<object>(state.CheckedItems, ReferenceEqualityComparer.Instance);
        var currentCheckedSet = new HashSet<object>(checkedItems, ReferenceEqualityComparer.Instance);
        var removedChecked = state.CheckedItems.Where(item =>
            !currentCheckedSet.Contains(item)).ToArray();
        var addedChecked = checkedItems.Where(item =>
            !previousCheckedSet.Contains(item)).ToArray();
        if (removedChecked.Length > 0 || addedChecked.Length > 0)
        {
            CheckedItemsChanged?.Invoke(this, new TreeViewCheckedItemsChangedEventArgs(
                removedChecked,
                addedChecked));
        }
    }

    private object[] CaptureSelectedItems()
    {
        var items = SelectedItems.Cast<object>().ToArray();
        return items.Length == 0 && SelectedItem is { } selectedItem
            ? [selectedItem]
            : items;
    }

    private void HandleSelectionChangedDuringItemsSourceReplay(SelectionChangedEventArgs args)
    {
        if (IsItemsSourceReplayPending && ReferenceEquals(args.Source, this))
        {
            if (_itemsSourceReplayInternalDepth == 0)
            {
                RecordExplicitSelectionUpdateDuringReplay(false);
            }
            args.Handled = true;
        }
    }

    private void RecordExplicitSelectionUpdateDuringReplay(bool propertyAssignment)
    {
        if (IsItemsSourceReplayPending && _itemsSourceReplayInternalDepth == 0 &&
            (propertyAssignment || !SyncingSelectedItems))
        {
            _hasExplicitSelectionUpdateDuringReplay = true;
            _hasExplicitSelectionAssignmentDuringReplay |= propertyAssignment;
            _explicitSelectedItemPath = BuildNodeIdentityPath(SelectedItem as ITreeItemNode);
            _explicitSelectedItemPaths = BuildNodeIdentityPaths(SelectedItems);
        }
    }

    private void RecordExplicitCheckedUpdateDuringReplay(bool propertyAssignment)
    {
        if (IsItemsSourceReplayPending && _itemsSourceReplayInternalDepth == 0 &&
            (propertyAssignment || !SyncingCheckedItems))
        {
            _hasExplicitCheckedUpdateDuringReplay = true;
            _hasExplicitCheckedAssignmentDuringReplay |= propertyAssignment;
            _explicitCheckedItemPaths = BuildNodeIdentityPaths(CheckedItems);
        }
    }

    internal void CaptureOwnerSelectionForItemsSourceReplay()
    {
        if (!IsItemsSourceReplayPending)
        {
            return;
        }

        _hasExplicitSelectionUpdateDuringReplay = true;
        _explicitSelectedItemPath = BuildNodeIdentityPath(SelectedItem as ITreeItemNode);
        _explicitSelectedItemPaths = BuildNodeIdentityPaths(SelectedItems);
        _hasExplicitCheckedUpdateDuringReplay = true;
        _explicitCheckedItemPaths = BuildNodeIdentityPaths(CheckedItems);
    }

    private void ReplayLoadedState()
    {
        ConfigureDefaultSelectedPaths();
        ConfigureDefaultCheckedPaths();

        FilterTreeNode();

        if (IsDefaultExpandAll)
        {
            Dispatcher.Post(() => ExpandAll(false));
        }
        else
        {
            ConfigureDefaultExpandedPaths();
        }
    }

    private void ConfigureStateAfterItemsSourceChanged(
        ItemsSourceReplayState state,
        bool hasExplicitSelectionUpdate,
        bool hasExplicitCheckedUpdate)
    {
        if (!IsLoaded)
        {
            return;
        }

        var isMultiple        = (SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple;
        var selectedItemPath  = hasExplicitSelectionUpdate ? _explicitSelectedItemPath : state.SelectedItemPath;
        var selectedItemPaths = hasExplicitSelectionUpdate ? _explicitSelectedItemPaths : state.SelectedItemPaths;
        var checkedItemPaths  = hasExplicitCheckedUpdate ? _explicitCheckedItemPaths : state.CheckedItemPaths;

        // Preserve realized selected containers when they still represent the same item instances.
        // A changed path or item instance must still be remapped from the pre-reset snapshot.
        var reuseSelection = CanReuseCurrentSelection(isMultiple, selectedItemPath, selectedItemPaths);
        if (!reuseSelection)
        {
            SetCurrentValue(SelectedItemProperty, null);
            SelectedItems.Clear();
        }

        CheckedItems.Clear();

        if (!reuseSelection)
        {
            var selectionRestored = false;
            if (isMultiple && selectedItemPaths is { Count: > 0 })
            {
                foreach (var path in selectedItemPaths)
                {
                    selectionRestored |= TrySelectNodePath(path);
                }
            }
            if (!selectionRestored && selectedItemPath != null)
            {
                selectionRestored = TrySelectNodePath(selectedItemPath);
            }
            if (!selectionRestored && selectedItemPaths != null)
            {
                foreach (var path in selectedItemPaths)
                {
                    selectionRestored |= TrySelectNodePath(path);
                }
            }
            if (!selectionRestored && !hasExplicitSelectionUpdate)
            {
                ConfigureDefaultSelectedPaths();
            }
        }

        var checkedRestored = false;
        if (checkedItemPaths != null)
        {
            foreach (var path in checkedItemPaths)
            {
                checkedRestored |= TryCheckNodePath(path);
            }
        }
        if (!checkedRestored && !hasExplicitCheckedUpdate)
        {
            ConfigureDefaultCheckedPaths();
        }

        if (IsDefaultExpandAll)
        {
            ExpandAll(false);
        }
        else
        {
            ConfigureDefaultExpandedPaths();
        }
    }

    private bool CanReuseCurrentSelection(
        bool isMultiple,
        TreeNodePath? selectedItemPath,
        List<TreeNodePath>? selectedItemPaths)
    {
        if (isMultiple)
        {
            if (selectedItemPaths == null ||
                selectedItemPaths.Count == 0 ||
                selectedItemPaths.Count != SelectedItems.Count)
            {
                return false;
            }

            var selectedInstances = new HashSet<object>(
                SelectedItems.Cast<object>(), ReferenceEqualityComparer.Instance);
            foreach (var path in selectedItemPaths)
            {
                if (!TryResolveSelectedNodePath(path, out var item) || !selectedInstances.Contains(item!))
                {
                    return false;
                }
            }

            return true;
        }

        if (SelectedItem == null)
        {
            // Nothing to reuse; let the replay reapply DefaultSelectedPaths as before.
            return false;
        }

        return selectedItemPath != null &&
               TryResolveSelectedNodePath(selectedItemPath, out var selectedItem) &&
               ReferenceEquals(selectedItem, SelectedItem);
    }

    private bool TryResolveSelectedNodePath(TreeNodePath path, out object? item)
    {
        object? resolved     = null;
        var     containerSet = false;
        TraverseDefaultStatePath(path, (treeViewItem, i) =>
        {
            if (i == path.Length - 1)
            {
                resolved     = TreeItemFromContainer(treeViewItem);
                containerSet = treeViewItem.IsSelected;
            }
        });
        item = resolved;
        return resolved != null && containerSet;
    }

    private static List<TreeNodePath>? BuildNodeIdentityPaths(IList? nodes)
    {
        if (nodes == null)
        {
            return null;
        }

        var paths = new List<TreeNodePath>(nodes.Count);
        foreach (var node in nodes)
        {
            if (node is ITreeItemNode treeItemNode)
            {
                var path = BuildNodeIdentityPath(treeItemNode);
                if (path != null)
                {
                    paths.Add(path);
                }
            }
        }
        return paths;
    }

    private static TreeNodePath? BuildNodeIdentityPath(ITreeItemNode? node)
    {
        if (node == null)
        {
            return null;
        }

        var depth    = CountTreeItemPathDepth(node);
        var segments = new string[depth];
        var current  = node;
        for (var i = segments.Length - 1; current != null; i--)
        {
            var segment = current.ItemKey?.ToString() ?? current.Value?.ToString();
            if (string.IsNullOrEmpty(segment))
            {
                return null;
            }

            segments[i] = segment;
            current     = current.ParentNode as ITreeItemNode;
        }

        return new TreeNodePath(segments);
    }

    private static int CountTreeItemPathDepth(ITreeItemNode itemData)
    {
        var count   = 0;
        var current = itemData;
        while (current != null)
        {
            count++;
            current = current.ParentNode as ITreeItemNode;
        }

        return count;
    }

    private bool TrySelectNodePath(TreeNodePath path)
    {
        var selected = false;
        TraverseDefaultStatePath(path, (treeViewItem, i) =>
        {
            if (i == path.Length - 1)
            {
                var item = TreeItemFromContainer(treeViewItem);
                if (item != null)
                {
                    treeViewItem.SetCurrentValue(TreeViewItem.IsSelectedProperty, true);
                    if (!SelectedItems.Contains(item))
                    {
                        SelectedItems.Add(item);
                    }
                    if (SelectionMode == SelectionMode.Single)
                    {
                        SetCurrentValue(SelectedItemProperty, item);
                    }
                    selected = true;
                }
            }
        });
        return selected;
    }

    private bool TryCheckNodePath(TreeNodePath path)
    {
        var isChecked = false;
        TraverseDefaultStatePath(path, (treeViewItem, i) =>
        {
            if (i == path.Length - 1)
            {
                treeViewItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
                isChecked = true;
            }
        });
        return isChecked;
    }

    private void ConfigureDefaultCheckedPaths()
    {
        if (DefaultCheckedPaths != null)
        {
            foreach (var checkedPath in DefaultCheckedPaths)
            {
                TraverseDefaultStatePath(checkedPath, (treeViewItem, i) =>
                {
                    if (i == checkedPath.Length - 1)
                    {
                        treeViewItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
                    }
                });
            }
        }
    }

    private void ConfigureDefaultExpandedPaths()
    {
        if (DefaultExpandedPaths != null)
        {
            foreach (var path in DefaultExpandedPaths)
            {
                TraverseTreeViewPath(
                    path,
                    TreePathTraversalOptions.DisableMotion |
                    TreePathTraversalOptions.ExpandMatchedNodes |
                    TreePathTraversalOptions.EnsureContainers);
            }
        }
    }

    private void ConfigureDefaultSelectedPaths()
    {
        if (!IsSelectable)
        {
            return;
        }

        if (SelectedItems.Count == 0 && SelectedItem == null)
        {
            if (DefaultSelectedPaths != null)
            {
                foreach (var selectedPath in DefaultSelectedPaths)
                {
                    TraverseDefaultStatePath(selectedPath, (treeViewItem, i) =>
                    {
                        if (i == selectedPath.Length - 1)
                        {
                            var item = TreeItemFromContainer(treeViewItem);
                            if (!SelectedItems.Contains(item))
                            {
                                SelectedItems.Add(item);
                            }
                        }
                    });
                }
            }
        }
        else if ((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple &&
                 SelectedItems.Count > 0)
        {
            ConfigureSelectedItems();
        }
        else if (SelectedItem != null)
        {
            var paths = GetTreePathFromItem(SelectedItem);
            SelectTreeItemByPath(paths);
        }
    }

    private void ConfigureSelectedItems()
    {
        var selectedPaths = BuildNodeIdentityPaths(SelectedItems);
        if (selectedPaths != null)
        {
            foreach (var selectedPath in selectedPaths)
            {
                TrySelectNodePath(selectedPath);
            }
        }
    }

    private List<object> GetTreePathFromItem(object item)
    {
        List<object> paths;
        if (item is ITreeItemNode itemData)
        {
            var pathDepth = CountTreeItemPathDepth(itemData);
            paths = new List<object>(pathDepth);
            for (var i = 0; i < pathDepth; i++)
            {
                paths.Add(null!);
            }

            var current = itemData;
            for (var i = pathDepth - 1; current != null; i--)
            {
                paths[i] = current;
                current  = current.ParentNode as ITreeItemNode;
            }
        }
        else if (item is TreeViewItem treeViewItem)
        {
            var pathDepth = CountTreeViewItemPathDepth(treeViewItem);
            paths = new List<object>(pathDepth);
            for (var i = 0; i < pathDepth; i++)
            {
                paths.Add(null!);
            }

            var current = treeViewItem;
            for (var i = pathDepth - 1; current != null; i--)
            {
                paths[i] = current;
                current  = current.Parent as TreeViewItem;
            }
        }
        else
        {
            throw new ArgumentException("Invalid item type, Must ITreeItemNode or TreeItem.");
        }

        return paths;
    }

    private void SelectTreeItemByPath(IList paths)
    {
        if (paths.Count == 0)
        {
            return;
        }
        ItemsControl current             = this;
        bool         originMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            for (var i = 0; i < paths.Count; i++)
            {
                var pathNode = paths[i];
                if (pathNode != null)
                {
                    TreeViewItem? child          = null;
                    bool?         originExpanded = null;
                    try
                    {
                        if (current is TreeViewItem item)
                        {
                            originExpanded = item.IsExpanded;
                            item.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
                        }

                        child = GetTreeViewItemContainer(pathNode, current);
                    }
                    finally
                    {
                        if (current is TreeViewItem item)
                        {
                            if (originExpanded != null)
                            {
                                item.SetCurrentValue(TreeViewItem.IsExpandedProperty, originExpanded.Value);
                            }
                        }
                    }

                    if (child != null)
                    {
                        current = child;
                    }
                }
            }

            if (current is TreeViewItem treeViewItem)
            {
                var item = TreeItemFromContainer(treeViewItem);
                if (item != null && !SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
                treeViewItem.SetCurrentValue(TreeViewItem.IsSelectedProperty, true);
            }
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
        }
    }

    private TreeViewItem? GetTreeViewItemContainer(object childNode, ItemsControl current)
    {
        if (current.Presenter?.Panel == null)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                topLevel.UpdateLayout();
            }
        }
        if (current.Presenter?.Panel is { })
        {
            return current.ContainerFromItem(childNode) as TreeViewItem;
        }
        return null;
    }

    private List<TreeViewItem>? TraverseDefaultStatePath(TreeNodePath path, Action<TreeViewItem, int>? action)
    {
        return TraverseTreeViewPath(
            path,
            TreePathTraversalOptions.DisableMotion |
            TreePathTraversalOptions.ExpandMatchedNodes |
            TreePathTraversalOptions.RestoreExpandedState |
            TreePathTraversalOptions.EnsureContainers,
            action);
    }
}
