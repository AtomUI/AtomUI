using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

internal class DefaultTreeViewInteractionHandler : ITreeViewInteractionHandler
{
    protected IInputManager? InputManager { get; }
    internal TreeView? TreeView { get; private set; }
    private readonly bool _isFloatingTreeView;
    private IDisposable? _inputManagerSubscription;
    private TopLevel? _root;
    private RadioButtonGroupManager? _groupManager;

    public DefaultTreeViewInteractionHandler(bool isFloatingTreeView)
    {
        _isFloatingTreeView = isFloatingTreeView;
        InputManager        = AvaloniaLocator.Current.GetService<IInputManager>()!;
    }

    public void Attach(TreeView treeView)
    {
        TreeView                 =  treeView;
        TreeView.PointerPressed  += PointerPressed;
        TreeView.PointerReleased += PointerReleased;
        _root                    =  TopLevel.GetTopLevel(TreeView);
        if (_root is not null)
        {
            _groupManager = RadioButtonGroupManager.GetOrCreateForRoot(_root);
            for (var i = 0; i < TreeView.ItemCount; i++)
            {
                if (TreeView.ContainerFromIndex(i) is TreeViewItem item)
                {
                    AddTreeViewItemToRadioGroup(_groupManager, item);
                }
            }
        }
        if (_root is InputElement inputRoot)
        {
            inputRoot.AddHandler(InputElement.PointerPressedEvent, RootPointerPressed, RoutingStrategies.Tunnel);
        }
        _inputManagerSubscription = InputManager?.Process.Subscribe(RawInput);
    }

    public void Detach(TreeView treeView)
    {
        if (TreeView != treeView)
        {
            throw new NotSupportedException("DefaultTreeViewInteractionHandler is not attached to the TreeView.");
        }
        
        TreeView.PointerPressed  -= PointerPressed;
        TreeView.PointerReleased -= PointerReleased;
        
        if (_root is not null && _groupManager is { } oldManager)
        {
            _groupManager = null;
            for (var i = 0; i < TreeView.ItemCount; i++)
            {
                if (TreeView.ContainerFromIndex(i) is TreeViewItem item)
                {
                    RemoveTreeViewItemToRadioGroup(oldManager, item);
                }
            }
        }
        if (_root is InputElement inputRoot)
        {
            inputRoot.RemoveHandler(InputElement.PointerPressedEvent, RootPointerPressed);
        }

        _inputManagerSubscription?.Dispose();
        _inputManagerSubscription = null;
        TreeView                  = null;
        _root                     = null;
    }

    internal static TreeViewItem? GetTreeViewItemCore(StyledElement? item)
    {
        while (true)
        {
            if (item == null)
            {
                return null;
            }

            if (item is TreeViewItem treeViewItem)
            {
                return treeViewItem;
            }

            item = item.Parent;
        }
    }

    protected internal virtual void PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var item = GetTreeViewItemCore(e.Source as Control);

        if (sender is Visual visual &&
            e.GetCurrentPoint(visual).Properties.IsLeftButtonPressed && item?.ItemCount > 0)
        {
            e.Handled = true;
        }
    }

    protected internal virtual void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var item = GetTreeViewItemCore(e.Source as Control);

        if (e.InitialPressMouseButton == MouseButton.Left && item != null)
        {
            if (item.PointInHeaderBounds(e))
            {
                item.NotifyHeaderClick();
                Click(item);
                e.Handled = true;
            }
        }

        if (e.InitialPressMouseButton == MouseButton.Right && item != null)
        {
            if (item.PointInHeaderBounds(e))
            {
                if (TreeView?.IsSelectOnRightClick == true)
                {
                    EnsureItemSelectedForContextMenu(item);
                }
                CloseOpenContextMenus(item);
                item.RaiseContextMenuRequest();
                e.Handled = true;
            }
        }
    }

    // Light-dismiss pass-through from an already open ContextMenu may swallow the
    // selection-updating PointerPressed, leaving the previously right-clicked item
    // visually selected. Force selection to the currently right-clicked item here.
    private void EnsureItemSelectedForContextMenu(TreeViewItem item)
    {
        if (TreeView is null || !TreeView.IsSelectable || !item.IsSelectable)
        {
            return;
        }

        if (item.IsSelected)
        {
            return;
        }

        var treeItem = TreeView.TreeItemFromContainer(item) ?? item;
        TreeView.SelectedItem = treeItem;
    }

    // ContextMenu.Open() is a no-op while IsOpen==true, so a menu opened on A stays
    // pinned over A when the user right-clicks B. Close any still-open menu on the
    // TreeView or on any ancestor TreeViewItem before re-raising the request.
    private void CloseOpenContextMenus(TreeViewItem item)
    {
        if (TreeView?.ContextMenu is { IsOpen: true } treeMenu)
        {
            treeMenu.Close();
        }

        Control? cursor = item;
        while (cursor is not null)
        {
            if (cursor.ContextMenu is { IsOpen: true } menu)
            {
                menu.Close();
            }
            cursor = cursor.Parent as Control;
        }
    }

    internal void Click(TreeViewItem viewItem)
    {
        viewItem.RaiseClick();
    }

    protected internal virtual void RawInput(RawInputEventArgs e)
    {
        var mouse = e as RawPointerEventArgs;

        if (mouse?.Type == RawPointerEventType.NonClientLeftButtonDown && _isFloatingTreeView)
        {
            if (TreeView is FloatableTreeView floatableTreeView)
            {
                floatableTreeView.Close();
            }
        }
    }

    protected internal virtual void RootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (TreeView is FloatableTreeView floatableTreeView)
        {
            if (floatableTreeView.IsOpen)
            {
                if (e.Source is ILogical control && !floatableTreeView.IsLogicalAncestorOf(control))
                {
                    floatableTreeView.Close();
                }
            }
        }
    }

    internal void OnCheckedChanged(TreeViewItem viewItem)
    {
        // A checked state being applied projects onto containers; those changes are not toggles.
        if (TreeView is not { } treeView || treeView.IsApplyingCheckedState)
        {
            return;
        }

        if (treeView.ToggleType == ItemToggleType.Radio && viewItem is IRadioButton radioButton)
        {
            _groupManager?.OnCheckedChanged(radioButton);
            return;
        }

        if (treeView.ToggleType != ItemToggleType.CheckBox)
        {
            return;
        }

        if (treeView.IsCheckStrictly)
        {
            UpdateStrictCheckedState(treeView, viewItem);
        }
        else
        {
            UpdateCascadingCheckedState(treeView, viewItem);
        }
    }

    private static void UpdateCascadingCheckedState(TreeView treeView, TreeViewItem viewItem)
    {
        if (!viewItem.IsChecked.HasValue)
        {
            return;
        }

        if (viewItem.IsChecked.Value)
        {
            treeView.CheckedSubTree(viewItem);
        }
        else
        {
            treeView.UnCheckedSubTree(viewItem);
        }
    }

    private static void UpdateStrictCheckedState(TreeView treeView, TreeViewItem viewItem)
    {
        if (!viewItem.IsEffectiveCheckable())
        {
            return;
        }

        if (treeView.TreeItemFromContainer(viewItem) is not { } treeNode)
        {
            return;
        }

        using (treeView.BeginCheckedItemsSync())
        {
            if (viewItem.IsChecked == true)
            {
                treeView.AddCheckedItemCore(treeNode);
            }
            else
            {
                treeView.RemoveCheckedItemCore(treeNode);
            }
        }
    }

    internal void OnGroupOrTypeChanged(IRadioButton button, string? oldGroupName)
    {
        if (!string.IsNullOrEmpty(oldGroupName))
        {
            _groupManager?.Remove(button, oldGroupName);
        }

        if (!string.IsNullOrEmpty(button.GroupName))
        {
            _groupManager?.Add(button);
        }
    }

    private static void AddTreeViewItemToRadioGroup(RadioButtonGroupManager manager, TreeViewItem element)
    {
        // Instead add menu item to the group on attached/detached + ensure checked stated on attached.
        if (element is IRadioButton button)
        {
            manager.Add(button);
        }

        for (var i = 0; i < element.ItemCount; i++)
        {
            var item = element.ContainerFromIndex(i);
            if (item is TreeViewItem treeViewItem)
            {
                AddTreeViewItemToRadioGroup(manager, treeViewItem);
            }
        }
    }

    private static void RemoveTreeViewItemToRadioGroup(RadioButtonGroupManager manager, TreeViewItem element)
    {
        if (element is IRadioButton button)
        {
            manager.Remove(button, button.GroupName);
        }

        for (var i = 0; i < element.ItemCount; i++)
        {
            var item = element.ContainerFromIndex(i);
            if (item is TreeViewItem treeViewItem)
            {
                RemoveTreeViewItemToRadioGroup(manager, treeViewItem);
            }
        }
    }
}
