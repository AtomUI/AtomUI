using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// CheckedItems owns the checked state. A cascade is resolved over the node data (ITreeItemNode.Children and
// ParentNode, or the logical TreeViewItem children), not over the realized containers, so it also reaches the
// nodes a virtualizing panel has not realized. Its result is applied to CheckedItems, written to the node data
// and projected onto the realized containers; a container prepared later takes it from the node data.
public partial class TreeView
{
    // Reference-identity mirror of CheckedItems: constant-time lookups, and on a Reset the items that were
    // checked before it. Node records compare by value and hash their mutable state, so membership must not
    // depend on Equals/GetHashCode.
    private readonly HashSet<object> _checkedItemSet = new(ReferenceEqualityComparer.Instance);

    // A plain IList assigned to CheckedItems changes without notification; the mirror is then re-read from it at
    // each entry point.
    private bool IsCheckedItemsTracked => _checkedItems is null or INotifyCollectionChanged;

    // Container IsChecked changes made while a checked state is applied are projections, not toggles.
    internal bool IsApplyingCheckedState { get; private set; }

    private bool IsCascadeCheckMode => ToggleType == ItemToggleType.CheckBox && !IsCheckStrictly;

    private void ApplyCheckedSubTree(TreeViewItem viewItem)
    {
        if (viewItem.IsEffectiveCheckable() && ResolveCheckedStateItem(viewItem) is { } item)
        {
            ApplyCheckedSubTrees([item], true, viewItem);
        }
    }

    private void ApplyUnCheckedSubTree(TreeViewItem viewItem)
    {
        if (viewItem.IsEffectiveCheckable() && ResolveCheckedStateItem(viewItem) is { } item)
        {
            ApplyCheckedSubTrees([item], false, viewItem);
        }
    }

    private ISet<object> CollectUnCheckedSubTreeItems(TreeViewItem treeViewItem)
    {
        var unCheckedItems = new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (ResolveCheckedStateItem(treeViewItem) is not { } item)
        {
            return unCheckedItems;
        }

        RefreshUntrackedCheckedItemSet();
        var plan = new CheckedStatePlan(this);
        plan.RegisterContainer(item, treeViewItem);
        try
        {
            PlanUnCheckedSubTree(plan, item);
            if (!IsCheckStrictly)
            {
                PlanAncestorStates(plan, [item]);
            }
            CommitCheckedStatePlan(plan, false);
        }
        finally
        {
            plan.RestoreExpandedStates();
        }

        foreach (var planItem in plan.Items)
        {
            if (plan.GetState(planItem) != true)
            {
                unCheckedItems.Add(planItem);
            }
        }
        return unCheckedItems;
    }

    /// <summary>
    /// Checks or unchecks the subtree of a node by its data, whether or not its container is realized.
    /// </summary>
    internal void SetSubTreeChecked(object item, bool isChecked)
    {
        if (ToggleType != ItemToggleType.None && IsCheckableNode(item, ResolveRealizedContainer(item)))
        {
            ApplyCheckedSubTrees([item], isChecked, null);
        }
    }

    private void ApplyCheckedSubTrees(IReadOnlyList<object> items, bool isChecked, TreeViewItem? rootContainer)
    {
        RefreshUntrackedCheckedItemSet();
        var plan = new CheckedStatePlan(this);
        if (rootContainer != null && items.Count == 1)
        {
            plan.RegisterContainer(items[0], rootContainer);
        }

        try
        {
            foreach (var item in items)
            {
                if (isChecked)
                {
                    PlanCheckedSubTree(plan, item, plan.ContainerOf(item));
                }
                else
                {
                    PlanUnCheckedSubTree(plan, item);
                }
            }

            if (!IsCheckStrictly)
            {
                PlanAncestorStates(plan, items);
            }
            CommitCheckedStatePlan(plan, true);
        }
        finally
        {
            plan.RestoreExpandedStates();
        }
    }

    private void PlanCheckedSubTree(CheckedStatePlan plan, object item, TreeViewItem? container)
    {
        if (AllowsRecursiveCheck(item, container, true))
        {
            plan.SetState(item, true);
        }

        foreach (var child in EnumerateCheckChildren(plan, item, container, true))
        {
            var childContainer = plan.ContainerOf(child);
            if (IsCheckableNode(child, childContainer))
            {
                PlanCheckedSubTree(plan, child, childContainer);
            }
        }
    }

    private void PlanUnCheckedSubTree(CheckedStatePlan plan, object item)
    {
        // The toggled node itself always leaves CheckedItems; the recursive predicate only guards descendants.
        plan.SetState(item, false);
        var container = plan.ContainerOf(item);
        foreach (var child in EnumerateCheckChildren(plan, item, container, true))
        {
            var childContainer = plan.ContainerOf(child);
            if (IsCheckableNode(child, childContainer))
            {
                PlanUnCheckedDescendant(plan, child, childContainer);
            }
        }
    }

    private void PlanUnCheckedDescendant(CheckedStatePlan plan, object item, TreeViewItem? container)
    {
        if (GetCheckedState(plan, item, container) != false && AllowsRecursiveCheck(item, container, false))
        {
            plan.SetState(item, false);
        }

        foreach (var child in EnumerateCheckChildren(plan, item, container, true))
        {
            var childContainer = plan.ContainerOf(child);
            if (IsCheckableNode(child, childContainer))
            {
                PlanUnCheckedDescendant(plan, child, childContainer);
            }
        }
    }

    // Recomputes the parents of the changed nodes, deepest first and each once, so a batch costs one pass per
    // parent. Walking up stops at a disabled parent.
    private void PlanAncestorStates(CheckedStatePlan plan, IEnumerable<object> changedItems)
    {
        var scheduled = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var queue     = new PriorityQueue<(object Item, TreeViewItem? Container, int Depth), int>();

        void ScheduleParentOf(object item, TreeViewItem? container, int depth)
        {
            var (parent, parentContainer) = GetCheckParent(plan, item, container);
            if (parent != null && scheduled.Add(parent))
            {
                queue.Enqueue((parent, parentContainer, depth - 1), -(depth - 1));
            }
        }

        foreach (var item in changedItems)
        {
            var container = plan.ContainerOf(item);
            ScheduleParentOf(item, container, GetCheckDepth(plan, item, container));
        }

        while (queue.TryDequeue(out var entry, out _))
        {
            var (parent, parentContainer, depth) = entry;
            if (!IsCheckParentEnabled(parent, parentContainer))
            {
                continue;
            }

            GetChildCheckStatus(plan, parent, parentContainer, out var isAllChecked, out var isAnyChecked);
            plan.SetState(parent, isAllChecked ? true : isAnyChecked ? null : false);
            ScheduleParentOf(parent, parentContainer, depth);
        }
    }

    private void GetChildCheckStatus(CheckedStatePlan plan, object parent, TreeViewItem? parentContainer,
                                     out bool isAllChecked, out bool isAnyChecked)
    {
        var hasChild = false;
        isAllChecked = true;
        isAnyChecked = false;

        foreach (var child in EnumerateCheckChildren(plan, parent, parentContainer, false))
        {
            hasChild = true;
            var childContainer = plan.ContainerOf(child);
            if (IsCheckableNode(child, childContainer))
            {
                var state = GetCheckedState(plan, child, childContainer);
                if (state != true)
                {
                    isAllChecked = false;
                }
                if (state != false)
                {
                    isAnyChecked = true;
                }
            }

            if (!isAllChecked && isAnyChecked)
            {
                break;
            }
        }

        if (!hasChild)
        {
            isAllChecked = false;
        }
    }

    private void CommitCheckedStatePlan(CheckedStatePlan plan, bool updateCheckedItems)
    {
        if (plan.Items.Count == 0)
        {
            return;
        }

        var originIsMotionEnabled  = IsMotionEnabled;
        var wasApplyingCheckedState = IsApplyingCheckedState;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            IsApplyingCheckedState = true;
            using (BeginCheckedItemsSync())
            {
                foreach (var item in plan.Items)
                {
                    var state = plan.GetState(item);
                    if (updateCheckedItems)
                    {
                        if (state == true)
                        {
                            AddCheckedItemCore(item);
                        }
                        else
                        {
                            RemoveCheckedItemCore(item);
                        }
                    }
                    ProjectCheckedState(item, plan.ContainerOf(item), state);
                }
            }
        }
        finally
        {
            IsApplyingCheckedState = wasApplyingCheckedState;
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }

    private static void ProjectCheckedState(object item, TreeViewItem? container, bool? state)
    {
        if (item is ITreeItemNode node and not TreeViewItem && node.IsChecked != state)
        {
            node.IsChecked = state;
        }

        if (container != null && container.IsChecked != state)
        {
            container.SetCurrentValue(TreeViewItem.IsCheckedProperty, state);
        }
    }

    // The checked state of a node that is not part of the plan: CheckedItems decides whether it is checked,
    // the container or node data whether an unchecked node is indeterminate.
    private bool? GetCheckedState(CheckedStatePlan plan, object item, TreeViewItem? container)
    {
        if (plan.TryGetState(item, out var planned))
        {
            return planned;
        }

        if (_checkedItemSet.Contains(item))
        {
            return true;
        }

        bool? stored = false;
        if (container != null)
        {
            stored = container.IsChecked;
        }
        else if (item is ITreeItemNode node)
        {
            stored = node.IsChecked;
        }
        return stored is null ? null : false;
    }

    private IEnumerable<object> EnumerateCheckChildren(CheckedStatePlan plan, object item, TreeViewItem? container,
                                                       bool materialize)
    {
        if (item is TreeViewItem treeViewItem)
        {
            return EnumerateNonNull(treeViewItem.Items);
        }

        if (item is ITreeItemNode node)
        {
            return node.Children;
        }

        // Data without a node model only exposes its children through a realized container.
        if (container == null)
        {
            return [];
        }

        if (materialize && container.Presenter?.Panel == null && !container.IsExpanded)
        {
            plan.RecordExpandedState(container);
            container.SetCurrentValue(Avalonia.Controls.TreeViewItem.IsExpandedProperty, true);
            TopLevel.GetTopLevel(this)?.UpdateLayout();
            plan.InvalidateContainers();
        }
        return EnumerateNonNull(container.Items).ToList();
    }

    private static IEnumerable<object> EnumerateNonNull(IEnumerable items)
    {
        foreach (var item in items)
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }

    private static (object? Parent, TreeViewItem? ParentContainer) GetCheckParent(
        CheckedStatePlan plan, object item, TreeViewItem? container)
    {
        // A realized container knows its parent exactly; otherwise follow the node data.
        if (container?.Parent is TreeViewItem parentContainer)
        {
            return (ResolveCheckedStateItem(parentContainer) ?? parentContainer, parentContainer);
        }

        if (item is TreeViewItem treeViewItem)
        {
            return treeViewItem.Parent is TreeViewItem logicalParent ? (logicalParent, logicalParent) : (null, null);
        }

        if (item is ITreeItemNode { ParentNode: ITreeItemNode parentNode })
        {
            return (parentNode, plan.ContainerOf(parentNode));
        }

        return (null, null);
    }

    private static int GetCheckDepth(CheckedStatePlan plan, object item, TreeViewItem? container)
    {
        var depth   = 1;
        var current = GetCheckParent(plan, item, container);
        while (current.Parent != null)
        {
            depth++;
            current = GetCheckParent(plan, current.Parent, current.ParentContainer);
        }
        return depth;
    }

    private static bool IsCheckableNode(object item, TreeViewItem? container)
    {
        if (container != null)
        {
            return container.IsEnabled && container.IsIndicatorEnabled;
        }
        if (item is ITreeItemNode node)
        {
            return node.IsEnabled && node.IsIndicatorEnabled;
        }
        return true;
    }

    private static bool IsCheckParentEnabled(object parent, TreeViewItem? parentContainer)
    {
        return parentContainer?.IsEnabled ?? (parent as ITreeItemNode)?.IsEnabled ?? true;
    }

    private bool AllowsRecursiveCheck(object item, TreeViewItem? container, bool isChecked)
    {
        if (container != null)
        {
            return isChecked ? RecursiveCheckNodePredicate(container) : RecursiveUnCheckNodePredicate(container);
        }
        return isChecked ? RecursiveCheckNodeDataPredicate(item) : RecursiveUnCheckNodeDataPredicate(item);
    }

    /// <summary>
    /// Counterpart of <see cref="RecursiveCheckNodePredicate" /> for a node whose container is not realized.
    /// </summary>
    private protected virtual bool RecursiveCheckNodeDataPredicate(object item) => true;

    /// <summary>
    /// Counterpart of <see cref="RecursiveUnCheckNodePredicate" /> for a node whose container is not realized.
    /// </summary>
    private protected virtual bool RecursiveUnCheckNodeDataPredicate(object item) => true;

    private static object? ResolveCheckedStateItem(TreeViewItem treeViewItem)
    {
        // A container a virtualizing panel is still realizing has no index yet; its node is its header.
        return (treeViewItem.Parent as ItemsControl)?.ItemFromContainer(treeViewItem) ??
               treeViewItem.Header as ITreeItemNode;
    }

    private TreeViewItem? ResolveRealizedContainer(object item)
    {
        return item as TreeViewItem ?? TreeContainerFromItem(item) as TreeViewItem;
    }

    /// <summary>
    /// Brings a container that was just prepared from node data in line with CheckedItems. Node data that is
    /// checked although CheckedItems does not know it is taken over like a toggle of a realized container.
    /// </summary>
    internal void ReconcilePreparedCheckedState(TreeViewItem container, object item)
    {
        if (ToggleType != ItemToggleType.CheckBox)
        {
            return;
        }

        RefreshUntrackedCheckedItemSet();
        if (_checkedItemSet.Contains(item))
        {
            if (container.IsChecked != true)
            {
                var wasApplyingCheckedState = IsApplyingCheckedState;
                IsApplyingCheckedState = true;
                try
                {
                    ProjectCheckedState(item, container, true);
                }
                finally
                {
                    IsApplyingCheckedState = wasApplyingCheckedState;
                }
            }
            return;
        }

        if (container.IsChecked != true || !IsCheckableNode(item, container))
        {
            return;
        }

        if (IsCheckStrictly)
        {
            using (BeginCheckedItemsSync())
            {
                AddCheckedItemCore(item);
            }
        }
        else
        {
            ApplyCheckedSubTrees([item], true, container);
        }
    }

    internal void AddCheckedItemCore(object item)
    {
        var isChecked = IsCheckedItemsTracked ? _checkedItemSet.Contains(item) : CheckedItems.Contains(item);
        if (!isChecked)
        {
            CheckedItems.Add(item);
        }
        _checkedItemSet.Add(item);
    }

    internal void RemoveCheckedItemCore(object item)
    {
        if (!_checkedItemSet.Remove(item) && IsCheckedItemsTracked)
        {
            return;
        }
        CheckedItems.Remove(item);
    }

    private void RefreshUntrackedCheckedItemSet()
    {
        if (!IsCheckedItemsTracked)
        {
            _checkedItemSet.Clear();
            _checkedItemSet.UnionWith(EnumerateNonNull(_checkedItems!));
        }
    }

    /// <summary>
    /// A node left the data tree: drop it from CheckedItems without touching its data, which the caller may
    /// still own or insert elsewhere.
    /// </summary>
    internal void RemoveCheckedItemOfRemovedNode(object? item)
    {
        RefreshUntrackedCheckedItemSet();
        if (item != null && !ShouldPreserveRemovedTreeItem(item) && _checkedItemSet.Contains(item))
        {
            using (BeginCheckedItemsSync())
            {
                RemoveCheckedItemCore(item);
            }
        }
    }

    /// <summary>
    /// A container's children collection was replaced by a new one: checked nodes of the old collection that the
    /// new one does not carry left the tree, together with their subtrees.
    /// </summary>
    internal void RemoveCheckedItemsOfReplacedChildren(IEnumerable oldChildren, IEnumerable? newChildren)
    {
        RefreshUntrackedCheckedItemSet();
        if (_checkedItemSet.Count == 0)
        {
            return;
        }

        var kept = new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (newChildren != null)
        {
            kept.UnionWith(EnumerateNonNull(newChildren));
        }

        var removed = new List<object>();
        var pending = new Stack<object>(EnumerateNonNull(oldChildren).Where(child => !kept.Contains(child)));
        while (pending.TryPop(out var item))
        {
            if (ShouldPreserveRemovedTreeItem(item))
            {
                continue;
            }
            if (_checkedItemSet.Contains(item))
            {
                removed.Add(item);
            }
            if (item is ITreeItemNode node and not TreeViewItem)
            {
                foreach (var child in node.Children)
                {
                    pending.Push(child);
                }
            }
        }

        RemoveCheckedItemsOfRemovedNodes(removed);
    }

    /// <summary>
    /// A children collection was reset: checked nodes that can no longer be reached from the roots left the tree.
    /// Data without a node model cannot be traced and stays.
    /// </summary>
    internal void RemoveDetachedCheckedItems()
    {
        RefreshUntrackedCheckedItemSet();
        if (_checkedItemSet.Count == 0)
        {
            return;
        }

        var roots        = new HashSet<object>(EnumerateNonNull(Items), ReferenceEqualityComparer.Instance);
        var childrenSets = new Dictionary<object, HashSet<object>>(ReferenceEqualityComparer.Instance);
        var detached     = new List<object>();
        foreach (var item in _checkedItemSet)
        {
            if (!IsReachableFromRoots(item, roots, childrenSets) && !ShouldPreserveRemovedTreeItem(item))
            {
                detached.Add(item);
            }
        }

        RemoveCheckedItemsOfRemovedNodes(detached);
    }

    private static bool IsReachableFromRoots(object item, HashSet<object> roots,
                                             Dictionary<object, HashSet<object>> childrenSets)
    {
        HashSet<object> ChildrenOf(object parent, IEnumerable children)
        {
            if (!childrenSets.TryGetValue(parent, out var set))
            {
                set = new HashSet<object>(EnumerateNonNull(children), ReferenceEqualityComparer.Instance);
                childrenSets.Add(parent, set);
            }
            return set;
        }

        if (item is TreeViewItem treeViewItem)
        {
            var current = treeViewItem;
            while (current.Parent is TreeViewItem parent)
            {
                if (!ChildrenOf(parent, parent.Items).Contains(current))
                {
                    return false;
                }
                current = parent;
            }
            return roots.Contains(current);
        }

        if (item is ITreeItemNode node)
        {
            var current = node;
            while (current.ParentNode is ITreeItemNode parentNode)
            {
                if (!ChildrenOf(parentNode, parentNode.Children).Contains(current))
                {
                    return false;
                }
                current = parentNode;
            }
            return roots.Contains(current);
        }

        return true;
    }

    private void RemoveCheckedItemsOfRemovedNodes(List<object> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        using (BeginCheckedItemsSync())
        {
            foreach (var item in items)
            {
                RemoveCheckedItemCore(item);
            }
        }
    }

    /// <summary>
    /// The root items were reset: their nodes left the tree, so CheckedItems is emptied without projecting the
    /// change onto data that no longer belongs to it.
    /// </summary>
    private void ClearCheckedItemsOfResetRoots()
    {
        if (CheckedItems.Count == 0 && _checkedItemSet.Count == 0)
        {
            return;
        }

        using (BeginCheckedItemsSync())
        {
            CheckedItems.Clear();
            _checkedItemSet.Clear();
        }
    }

    private void SubscribeToCheckedItems()
    {
        if (_checkedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += HandleCheckedItemsCollectionChanged;
        }

        HandleCheckedItemsCollectionChanged(
            _checkedItems,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void UnsubscribeFromCheckedItems()
    {
        if (_checkedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= HandleCheckedItemsCollectionChanged;
        }
    }

    private void HandleCheckedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IList? added   = null;
        IList? removed = null;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    MirrorCheckedItems(e.NewItems, true);
                    if (!SyncingCheckedItems)
                    {
                        ApplyExternalCheckedItems(e.NewItems, true);
                    }
                    added = e.NewItems;
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    MirrorCheckedItems(e.OldItems, false);
                    if (!SyncingCheckedItems)
                    {
                        ApplyExternalCheckedItems(e.OldItems, false);
                    }
                }

                removed = e.OldItems;
                break;
            case NotifyCollectionChangedAction.Reset:
                var previous = new HashSet<object>(_checkedItemSet, ReferenceEqualityComparer.Instance);
                _checkedItemSet.Clear();
                if (_checkedItems != null)
                {
                    _checkedItemSet.UnionWith(EnumerateNonNull(_checkedItems));
                }

                if (!SyncingCheckedItems)
                {
                    ApplyCheckedItemsReset(previous);
                }

                if (e.NewItems?.Count > 0)
                {
                    added = new List<object>(CheckedItems.Count);
                    foreach (var item in CheckedItems)
                    {
                        added.Add(item);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                {
                    MirrorCheckedItems(e.OldItems, false);
                }
                if (e.NewItems != null)
                {
                    MirrorCheckedItems(e.NewItems, true);
                }

                if (!SyncingCheckedItems)
                {
                    if (e.OldItems != null)
                    {
                        ApplyExternalCheckedItems(e.OldItems, false);
                    }

                    if (e.NewItems != null)
                    {
                        ApplyExternalCheckedItems(e.NewItems, true);
                    }
                }

                added   = e.NewItems;
                removed = e.OldItems;
                break;
        }
        RecordExplicitCheckedUpdateDuringReplay(false);
        if (!IsItemsSourceReplayPending && (added?.Count > 0 || removed?.Count > 0))
        {
            CheckedItemsChanged?.Invoke(this, new TreeViewCheckedItemsChangedEventArgs(
                removed ?? Empty,
                added ?? Empty));
        }
    }

    private void MirrorCheckedItems(IList items, bool isChecked)
    {
        foreach (var item in items)
        {
            if (item == null)
            {
                continue;
            }
            if (isChecked)
            {
                _checkedItemSet.Add(item);
            }
            else
            {
                _checkedItemSet.Remove(item);
            }
        }
    }

    // Items added to or removed from CheckedItems by code: the same as toggling them, including for nodes whose
    // containers are not realized.
    private void ApplyExternalCheckedItems(IList items, bool isChecked)
    {
        if (items.Count == 0)
        {
            return;
        }

        if (IsCascadeCheckMode)
        {
            var cascadeItems = new List<object>(items.Count);
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                var container = ResolveRealizedContainer(item);
                if (IsCheckableNode(item, container))
                {
                    cascadeItems.Add(item);
                }
                else
                {
                    ProjectCheckedState(item, container, isChecked);
                }
            }

            if (cascadeItems.Count > 0)
            {
                ApplyCheckedSubTrees(cascadeItems, isChecked, null);
            }
            return;
        }

        foreach (var item in items)
        {
            if (item != null)
            {
                ProjectCheckedState(item, ResolveRealizedContainer(item), isChecked);
            }
        }
    }

    // CheckedItems was reset or replaced: uncheck what left it, check what came in, and in cascade mode recompute
    // the parents around both. Nodes that were never in CheckedItems keep their own data.
    private void ApplyCheckedItemsReset(HashSet<object> previous)
    {
        var plan    = new CheckedStatePlan(this);
        var changed = new List<object>();
        try
        {
            foreach (var item in previous)
            {
                if (!_checkedItemSet.Contains(item))
                {
                    plan.SetState(item, false);
                    changed.Add(item);
                }
            }

            var isCascade = IsCascadeCheckMode;
            if (_checkedItems != null)
            {
                foreach (var item in EnumerateNonNull(_checkedItems))
                {
                    if (previous.Contains(item))
                    {
                        continue;
                    }

                    changed.Add(item);
                    var container = plan.ContainerOf(item);
                    if (isCascade && IsCheckableNode(item, container))
                    {
                        PlanCheckedSubTree(plan, item, container);
                    }
                    plan.SetState(item, true);
                }
            }

            if (changed.Count == 0)
            {
                return;
            }

            if (isCascade)
            {
                PlanAncestorStates(plan, changed);
            }
            CommitCheckedStatePlan(plan, isCascade);
        }
        finally
        {
            plan.RestoreExpandedStates();
        }
    }

    internal IDisposable BeginCheckedItemsSync()
    {
        return new CheckedItemsSyncScope(this);
    }

    private sealed class CheckedItemsSyncScope : IDisposable
    {
        private readonly TreeView _treeView;
        private readonly bool _wasSyncing;

        public CheckedItemsSyncScope(TreeView treeView)
        {
            _treeView                    = treeView;
            _wasSyncing                  = treeView.SyncingCheckedItems;
            treeView.SyncingCheckedItems = true;
        }

        public void Dispose()
        {
            _treeView.SyncingCheckedItems = _wasSyncing;
        }
    }

    // The target state of the nodes one checked-state change touches, in the order they were planned, with the
    // realized containers they map to.
    private sealed class CheckedStatePlan
    {
        private readonly TreeView _owner;
        private readonly Dictionary<object, bool?> _states = new(ReferenceEqualityComparer.Instance);
        private readonly List<object> _items = [];
        private readonly Dictionary<object, TreeViewItem> _registeredContainers = new(ReferenceEqualityComparer.Instance);
        private Dictionary<object, TreeViewItem>? _realizedContainers;
        private Dictionary<TreeViewItem, bool>? _expandedStates;

        public CheckedStatePlan(TreeView owner)
        {
            _owner = owner;
        }

        public IReadOnlyList<object> Items => _items;

        public void SetState(object item, bool? state)
        {
            if (_states.TryAdd(item, state))
            {
                _items.Add(item);
            }
            else
            {
                _states[item] = state;
            }
        }

        public bool? GetState(object item) => _states[item];

        public bool TryGetState(object item, out bool? state) => _states.TryGetValue(item, out state);

        public void RegisterContainer(object item, TreeViewItem container)
        {
            _registeredContainers[item] = container;
        }

        public TreeViewItem? ContainerOf(object item)
        {
            if (item is TreeViewItem treeViewItem)
            {
                return treeViewItem;
            }

            if (_registeredContainers.TryGetValue(item, out var registered))
            {
                return registered;
            }

            _realizedContainers ??= BuildRealizedContainers();
            return _realizedContainers.GetValueOrDefault(item);
        }

        public void InvalidateContainers()
        {
            _realizedContainers = null;
        }

        public void RecordExpandedState(TreeViewItem container)
        {
            _expandedStates ??= new Dictionary<TreeViewItem, bool>();
            _expandedStates.TryAdd(container, container.IsExpanded);
        }

        public void RestoreExpandedStates()
        {
            if (_expandedStates == null)
            {
                return;
            }

            foreach (var (container, wasExpanded) in _expandedStates)
            {
                container.SetCurrentValue(Avalonia.Controls.TreeViewItem.IsExpandedProperty, wasExpanded);
            }
            _expandedStates = null;
        }

        private Dictionary<object, TreeViewItem> BuildRealizedContainers()
        {
            var containers = new Dictionary<object, TreeViewItem>(ReferenceEqualityComparer.Instance);
            foreach (var container in _owner.GetRealizedTreeContainers())
            {
                if (container is TreeViewItem treeViewItem &&
                    (treeViewItem.Parent as ItemsControl)?.ItemFromContainer(treeViewItem) is { } item)
                {
                    containers.TryAdd(item, treeViewItem);
                }
            }
            return containers;
        }
    }
}
