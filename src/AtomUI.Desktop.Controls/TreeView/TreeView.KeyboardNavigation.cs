using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

// TreeView is the only keyboard focus target of the tree. The keyboard position is an internal
// active item instead of item focus, so item containers stay non-focusable and popup hosts can
// forward keys while focus stays in their own input.
public partial class TreeView : ICustomKeyboardNavigation
{
    private TreeViewItem? _keyboardActiveItem;
    private TreeViewItem? _keyboardSelectionAnchor;
    private bool _isKeyboardActiveVisible;
    private bool _isSyncingKeyboardRangeSelection;

    internal TreeViewItem? KeyboardActiveItem => _keyboardActiveItem;

    /// <summary>
    /// Whether moving the active item also changes selection (single-select follow and Shift range).
    /// Popup hosts turn it off so browsing never commits; Enter / Space still commit.
    /// </summary>
    internal bool IsKeyboardMoveSelectionEnabled { get; set; } = true;

    // Re-implemented so Tab lands on the TreeView itself and leaves it through the default tab
    // order; the base implementation targets item containers, which are not focusable here.
    (bool handled, IInputElement? next) ICustomKeyboardNavigation.GetNext(IInputElement element,
                                                                          NavigationDirection direction)
    {
        return (false, null);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Keys raised by focusable user content inside a header keep their own semantics.
        if (ReferenceEquals(e.Source, this))
        {
            HandleKeyDown(e);
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this))
        {
            return;
        }

        EnsureKeyboardActiveItem();
        if (e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional)
        {
            SetKeyboardActiveVisible(true);
        }
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (ReferenceEquals(e.Source, this))
        {
            SetKeyboardActiveVisible(false);
        }
    }

    /// <summary>
    /// Keyboard navigation entry used by the TreeView itself and by popup hosts that keep focus
    /// in their own input and forward keys to the tree.
    /// </summary>
    internal void HandleKeyDown(KeyEventArgs e)
    {
        if (e.Handled || !IsEffectivelyEnabled || e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return;
        }

        var hotkeys = this.GetPlatformSettings()?.HotkeyConfiguration;
        if (IsSelectable &&
            SelectionMode.HasFlag(SelectionMode.Multiple) &&
            hotkeys is not null &&
            hotkeys.SelectAll.Any(gesture => gesture.Matches(e)))
        {
            SelectAll();
            e.Handled = true;
            return;
        }

        var hadActiveItem = _keyboardActiveItem is not null && IsNavigable(_keyboardActiveItem);
        var activeItem    = EnsureKeyboardActiveItem();
        if (activeItem is null)
        {
            return;
        }

        var handled = false;
        if (!hadActiveItem && IsNavigationKey(e.Key))
        {
            // The first key of a session only establishes the active item.
            handled = true;
        }
        else
        {
            var commandModifiers = hotkeys?.CommandModifiers ?? KeyModifiers.Control;
            var moveOnly         = e.KeyModifiers.HasFlag(commandModifiers);
            var extend           = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            var isForwardKey     = (e.Key == Key.Right) == (FlowDirection != FlowDirection.RightToLeft);

            handled = e.Key switch
            {
                Key.Down => MoveKeyboardActiveItem(GetNextNavigableItem(activeItem), moveOnly, extend),
                Key.Up => MoveKeyboardActiveItem(GetPreviousNavigableItem(activeItem), moveOnly, extend),
                Key.Home => MoveKeyboardActiveItem(GetFirstNavigableItem(), moveOnly, extend),
                Key.End => MoveKeyboardActiveItem(GetLastNavigableItem(), moveOnly, extend),
                Key.Left or Key.Right => isForwardKey
                    ? ExpandOrEnterKeyboardActiveItem(activeItem, moveOnly)
                    : CollapseOrLeaveKeyboardActiveItem(activeItem, moveOnly),
                Key.Enter or Key.Space => CommitKeyboardActiveItem(activeItem),
                _ => false
            };
        }

        if (handled)
        {
            SetKeyboardActiveVisible(true);
            e.Handled = true;
        }
    }

    internal void ClearKeyboardActiveItem()
    {
        SetKeyboardActiveItem(null);
        _keyboardSelectionAnchor = null;
        _isKeyboardActiveVisible = false;
    }

    internal void NotifyTreeViewItemCollapsed(TreeViewItem treeViewItem)
    {
        if (_keyboardActiveItem is not null &&
            !ReferenceEquals(_keyboardActiveItem, treeViewItem) &&
            treeViewItem.IsVisualAncestorOf(_keyboardActiveItem))
        {
            SetKeyboardActiveItem(treeViewItem);
        }
    }

    internal void NotifyTreeViewItemDetached(TreeViewItem treeViewItem)
    {
        if (ReferenceEquals(_keyboardActiveItem, treeViewItem))
        {
            SetKeyboardActiveItem(null);
        }

        if (ReferenceEquals(_keyboardSelectionAnchor, treeViewItem))
        {
            _keyboardSelectionAnchor = null;
        }
    }

    private void NotifyTreeViewItemPointerPressed(TreeViewItem treeViewItem)
    {
        _isKeyboardActiveVisible = false;
        SetKeyboardActiveItem(treeViewItem);
        _keyboardSelectionAnchor = treeViewItem;
    }

    private static bool IsNavigationKey(Key key)
    {
        return key is Key.Up or Key.Down or Key.Left or Key.Right or Key.Home or Key.End;
    }

    private TreeViewItem? EnsureKeyboardActiveItem()
    {
        if (_keyboardActiveItem is { } activeItem)
        {
            if (IsNavigable(activeItem))
            {
                return activeItem;
            }

            // Filtered, disabled or collapsed away: fall back to the nearest navigable ancestor.
            var ancestor = activeItem.Parent as TreeViewItem;
            while (ancestor is not null && !IsNavigable(ancestor))
            {
                ancestor = ancestor.Parent as TreeViewItem;
            }

            if (ancestor is not null)
            {
                SetKeyboardActiveItem(ancestor);
                return ancestor;
            }
        }

        var initialItem = GetInitialKeyboardActiveItem();
        SetKeyboardActiveItem(initialItem);
        return initialItem;
    }

    private TreeViewItem? GetInitialKeyboardActiveItem()
    {
        TreeViewItem? firstChecked = null;
        TreeViewItem? first        = null;
        foreach (var item in EnumerateNavigableItems())
        {
            if (item.IsSelected)
            {
                return item;
            }

            first ??= item;
            if (firstChecked is null && item.IsChecked == true)
            {
                firstChecked = item;
            }
        }

        return firstChecked ?? first;
    }

    private void SetKeyboardActiveItem(TreeViewItem? treeViewItem)
    {
        if (!ReferenceEquals(_keyboardActiveItem, treeViewItem))
        {
            if (_keyboardActiveItem is not null)
            {
                _keyboardActiveItem.IsKeyboardActive              = false;
                _keyboardActiveItem.IsKeyboardFocusOutlineVisible = false;
            }

            _keyboardActiveItem = treeViewItem;
        }

        UpdateKeyboardActiveIndicator();
    }

    private void SetKeyboardActiveVisible(bool isVisible)
    {
        _isKeyboardActiveVisible = isVisible;
        UpdateKeyboardActiveIndicator();
    }

    private void UpdateKeyboardActiveIndicator()
    {
        if (_keyboardActiveItem is not null)
        {
            _keyboardActiveItem.IsKeyboardActive = _isKeyboardActiveVisible;
            // Only the tree's own keyboard focus outlines the item; popup hosts that forward keys keep
            // focus in their input and show the active background alone.
            _keyboardActiveItem.IsKeyboardFocusOutlineVisible = _isKeyboardActiveVisible && IsFocused;
        }
    }

    private bool MoveKeyboardActiveItem(TreeViewItem? target, bool moveOnly, bool extend)
    {
        if (target is null)
        {
            return false;
        }

        var previous = _keyboardActiveItem;
        SetKeyboardActiveItem(target);
        if (!moveOnly)
        {
            ApplyKeyboardSelection(target, previous, extend);
        }

        target.BringHeaderIntoView();
        return true;
    }

    private void ApplyKeyboardSelection(TreeViewItem target, TreeViewItem? previous, bool extend)
    {
        if (!IsKeyboardMoveSelectionEnabled || !IsSelectable || ToggleType != ItemToggleType.None)
        {
            return;
        }

        if (SelectionMode.HasFlag(SelectionMode.Multiple))
        {
            if (extend)
            {
                SelectKeyboardRange(_keyboardSelectionAnchor ?? previous ?? target, target);
            }

            return;
        }

        SelectedItem             = GetTreeItem(target);
        _keyboardSelectionAnchor = target;
    }

    private void SelectKeyboardRange(TreeViewItem anchor, TreeViewItem target)
    {
        var items       = EnumerateNavigableItems().ToList();
        var anchorIndex = items.IndexOf(anchor);
        var targetIndex = items.IndexOf(target);
        if (anchorIndex < 0 || targetIndex < 0)
        {
            return;
        }

        _keyboardSelectionAnchor = anchor;
        var start = Math.Min(anchorIndex, targetIndex);
        var end   = Math.Max(anchorIndex, targetIndex);
        var range = new List<object>(end - start + 1);
        for (var i = start; i <= end; i++)
        {
            range.Add(GetTreeItem(items[i]));
        }

        // Only the delta is synchronized, so SelectedItems never passes through an empty selection. Avalonia
        // TreeView raises SelectionChanged for every SelectedItems notification (AvaloniaList removes one
        // contiguous run per notification, then adds the rest in one), and offers no batch update: those
        // intermediate events are suppressed and the whole change is reported once. Collection observers
        // still see the removal before the addition.
        var rangeSet    = new HashSet<object>(range);
        var selectedSet = new HashSet<object>(SelectedItems.Cast<object>());
        var removed     = SelectedItems.Cast<object>().Where(item => !rangeSet.Contains(item)).ToList();
        var added       = range.Where(item => !selectedSet.Contains(item)).ToList();
        if (removed.Count == 0 && added.Count == 0)
        {
            return;
        }

        _isSyncingKeyboardRangeSelection = true;
        try
        {
            if (SelectedItems is AvaloniaList<object> selectedItems)
            {
                selectedItems.RemoveAll(removed);
                selectedItems.AddRange(added);
            }
            else
            {
                foreach (var item in removed)
                {
                    SelectedItems.Remove(item);
                }

                foreach (var item in added)
                {
                    SelectedItems.Add(item);
                }
            }
        }
        finally
        {
            _isSyncingKeyboardRangeSelection = false;
        }

        RaiseEvent(new SelectionChangedEventArgs(
            Avalonia.Controls.Primitives.SelectingItemsControl.SelectionChangedEvent, removed, added));
    }

    private void HandleSelectionChangedDuringKeyboardRangeSync(SelectionChangedEventArgs args)
    {
        if (_isSyncingKeyboardRangeSelection && ReferenceEquals(args.Source, this))
        {
            args.Handled = true;
        }
    }

    private bool ExpandOrEnterKeyboardActiveItem(TreeViewItem activeItem, bool moveOnly)
    {
        if (!activeItem.IsExpandable())
        {
            return false;
        }

        if (!activeItem.IsExpanded || (activeItem.HasTreeItemDataLoader && !activeItem.AsyncLoaded))
        {
            return activeItem.RequestToggleExpand();
        }

        return MoveKeyboardActiveItem(GetNavigableChild(activeItem, first: true), moveOnly, false);
    }

    private bool CollapseOrLeaveKeyboardActiveItem(TreeViewItem activeItem, bool moveOnly)
    {
        if (activeItem.IsExpanded && activeItem.ItemCount > 0)
        {
            activeItem.SetCurrentValue(Avalonia.Controls.TreeViewItem.IsExpandedProperty, false);
            return true;
        }

        return MoveKeyboardActiveItem(activeItem.Parent as TreeViewItem, moveOnly, false);
    }

    private bool CommitKeyboardActiveItem(TreeViewItem activeItem)
    {
        // A disabled header (for example a host's selection limit) blocks pointer commits too.
        if (!activeItem.IsHeaderEffectivelyEnabled)
        {
            return false;
        }

        if (IsKeyboardCheckable(activeItem))
        {
            if (ToggleType == ItemToggleType.CheckBox)
            {
                activeItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, activeItem.IsChecked != true);
            }
            else if (activeItem.IsChecked != true)
            {
                activeItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
            }

            return true;
        }

        if (!IsSelectable)
        {
            return false;
        }

        var treeItem = GetTreeItem(activeItem);
        if (SelectionMode.HasFlag(SelectionMode.Multiple))
        {
            if (SelectedItems.Contains(treeItem))
            {
                SelectedItems.Remove(treeItem);
            }
            else
            {
                SelectedItems.Add(treeItem);
            }
        }
        else
        {
            SelectedItem = treeItem;
        }

        _keyboardSelectionAnchor = activeItem;
        return true;
    }

    private bool IsKeyboardCheckable(TreeViewItem treeViewItem)
    {
        if (!treeViewItem.IsEffectiveCheckable())
        {
            return false;
        }

        return ToggleType == ItemToggleType.CheckBox ||
               (ToggleType == ItemToggleType.Radio && treeViewItem.IsLeaf);
    }

    private static object GetTreeItem(TreeViewItem treeViewItem)
    {
        return (treeViewItem.Parent as ItemsControl)?.ItemFromContainer(treeViewItem) ?? treeViewItem;
    }

    private bool IsNavigable(TreeViewItem treeViewItem)
    {
        if (!IsSelfNavigable(treeViewItem))
        {
            return false;
        }

        var parent = treeViewItem.Parent;
        while (parent is TreeViewItem parentItem)
        {
            if (!parentItem.IsExpanded || !IsSelfNavigable(parentItem))
            {
                return false;
            }

            parent = parentItem.Parent;
        }

        return ReferenceEquals(parent, this);
    }

    private static bool IsSelfNavigable(TreeViewItem? treeViewItem)
    {
        return treeViewItem is { IsVisible: true, IsEffectivelyEnabled: true } &&
               treeViewItem.IsAttachedToVisualTree();
    }

    // Preorder over realized, navigable containers; the visual order of the tree.
    private IEnumerable<TreeViewItem> EnumerateNavigableItems()
    {
        return EnumerateNavigableChildren(this);

        static IEnumerable<TreeViewItem> EnumerateNavigableChildren(ItemsControl owner)
        {
            for (var i = 0; i < owner.ItemCount; i++)
            {
                if (owner.ContainerFromIndex(i) is not TreeViewItem item || !IsSelfNavigable(item))
                {
                    continue;
                }

                yield return item;
                if (item.IsExpanded)
                {
                    foreach (var descendant in EnumerateNavigableChildren(item))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }

    private TreeViewItem? GetNextNavigableItem(TreeViewItem treeViewItem)
    {
        if (treeViewItem.IsExpanded && GetNavigableChild(treeViewItem, first: true) is { } child)
        {
            return child;
        }

        var current = treeViewItem;
        while (current is not null)
        {
            if (GetNavigableSibling(current, 1) is { } sibling)
            {
                return sibling;
            }

            current = current.Parent as TreeViewItem;
        }

        return null;
    }

    private TreeViewItem? GetPreviousNavigableItem(TreeViewItem treeViewItem)
    {
        if (GetNavigableSibling(treeViewItem, -1) is { } sibling)
        {
            return GetLastNavigableDescendantOrSelf(sibling);
        }

        return treeViewItem.Parent as TreeViewItem;
    }

    private TreeViewItem? GetFirstNavigableItem()
    {
        return GetNavigableChild(this, first: true);
    }

    private TreeViewItem? GetLastNavigableItem()
    {
        return GetNavigableChild(this, first: false) is { } last ? GetLastNavigableDescendantOrSelf(last) : null;
    }

    private TreeViewItem GetLastNavigableDescendantOrSelf(TreeViewItem treeViewItem)
    {
        var current = treeViewItem;
        while (current.IsExpanded && GetNavigableChild(current, first: false) is { } last)
        {
            current = last;
        }

        return current;
    }

    private static TreeViewItem? GetNavigableSibling(TreeViewItem treeViewItem, int step)
    {
        if (treeViewItem.Parent is not ItemsControl owner)
        {
            return null;
        }

        var index = owner.IndexFromContainer(treeViewItem);
        if (index < 0)
        {
            return null;
        }

        for (var i = index + step; i >= 0 && i < owner.ItemCount; i += step)
        {
            if (owner.ContainerFromIndex(i) is TreeViewItem sibling && IsSelfNavigable(sibling))
            {
                return sibling;
            }
        }

        return null;
    }

    private static TreeViewItem? GetNavigableChild(ItemsControl owner, bool first)
    {
        if (owner.ItemCount == 0)
        {
            return null;
        }

        // A just expanded node may not have realized its children yet: run one layout pass.
        if (owner.ContainerFromIndex(first ? 0 : owner.ItemCount - 1) is null)
        {
            owner.UpdateLayout();
        }

        var step = first ? 1 : -1;
        for (var i = first ? 0 : owner.ItemCount - 1; i >= 0 && i < owner.ItemCount; i += step)
        {
            if (owner.ContainerFromIndex(i) is TreeViewItem child && IsSelfNavigable(child))
            {
                return child;
            }
        }

        return null;
    }
}
