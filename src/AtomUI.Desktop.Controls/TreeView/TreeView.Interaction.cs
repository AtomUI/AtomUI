using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    /// <summary>
    /// Identifies the <see cref="Behavior" /> property.
    /// </summary>
    public static readonly StyledProperty<TreeViewBehavior?> BehaviorProperty =
        AvaloniaProperty.Register<TreeView, TreeViewBehavior?>(nameof(Behavior));

    /// <summary>
    /// The optional policy for eligible header gestures.
    /// </summary>
    public TreeViewBehavior? Behavior
    {
        get => GetValue(BehaviorProperty);
        set => SetValue(BehaviorProperty, value);
    }

    private TopLevel? _interactionRoot;
    private RadioButtonGroupManager? _groupManager;
    private IDisposable? _nonClientPointerSubscription;

    /// <summary>
    /// Indicates whether pointer presses outside tree items dismiss an enclosing floating host.
    /// </summary>
    private protected virtual bool CloseOnNonClientPointer => false;

    private void AttachInteraction()
    {
        InteractionHandler?.Attach(this);
        _interactionRoot = TopLevel.GetTopLevel(this);
        if (_interactionRoot is not { } root)
        {
            return;
        }
        _groupManager = RadioButtonGroupManager.GetOrCreateForRoot(root);
        for (var index = 0; index < ItemCount; index++)
        {
            if (ContainerFromIndex(index) is TreeViewItem item)
            {
                AddTreeViewItemToRadioGroup(_groupManager, item);
            }
        }
        if (this is FloatableTreeView && InteractionHandler is null)
        {
            root.AddHandler(InputElement.PointerPressedEvent, HandleRootPointerPressed, RoutingStrategies.Tunnel);
            if (CloseOnNonClientPointer)
            {
                _nonClientPointerSubscription = AvaloniaLocator.Current.GetService<IInputManager>()?.Process.Subscribe(HandleRawInput);
            }
        }
    }

    private void DetachInteraction()
    {
        InteractionHandler?.Detach(this);
        if (_groupManager is { } manager)
        {
            for (var index = 0; index < ItemCount; index++)
            {
                if (ContainerFromIndex(index) is TreeViewItem item)
                {
                    RemoveTreeViewItemFromRadioGroup(manager, item);
                }
            }
        }
        if (this is FloatableTreeView && InteractionHandler is null)
        {
            _interactionRoot?.RemoveHandler(InputElement.PointerPressedEvent, HandleRootPointerPressed);
        }
        _groupManager = null;
        _interactionRoot = null;
        _nonClientPointerSubscription?.Dispose();
        _nonClientPointerSubscription = null;
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

    private void HandleTreePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
            GetTreeViewItemCore(args.Source as Control)?.ItemCount > 0)
        {
            args.Handled = true;
        }
    }

    private void HandleTreePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (GetTreeViewItemCore(args.Source as Control) is not { } item || !item.PointInHeaderBounds(args))
        {
            return;
        }
        if (args.InitialPressMouseButton == MouseButton.Left)
        {
            args.Handled = true;
            var context = new TreeViewHeaderContext(args.KeyModifiers, item.IsLeaf, item.IsExpanded,
                item.IsSelected, IsSelectable && item.IsSelectable);
            if (ShouldActivateHeader(context))
            {
                item.NotifyHeaderClick();
                item.RaiseClick();
            }
        }
        else if (args.InitialPressMouseButton == MouseButton.Right)
        {
            if (IsSelectOnRightClick)
            {
                EnsureItemSelectedForContextMenu(item);
            }
            CloseOpenContextMenus(item);
            item.RaiseContextMenuRequest();
            args.Handled = true;
        }
    }

    private bool ShouldActivateHeader(in TreeViewHeaderContext context)
    {
        try
        {
            var behavior = Behavior;
            if (behavior is null && this.TryFindResource(typeof(TreeViewBehavior), ActualThemeVariant, out var resource))
            {
                behavior = resource as TreeViewBehavior;
            }
            return (behavior ?? ControlBehavior<TreeView>.GetScoped(this) as TreeViewBehavior ?? TreeViewBehavior.Default)
                .ShouldActivateHeader(context);
        }
        catch (Exception error)
        {
            Logger.TryGet(LogEventLevel.Warning, LogArea.Control)?.Log(this,
                "TreeView header policy failed: {Error}", error);
            return false;
        }
    }

    // Light-dismiss pass-through from an already open ContextMenu may swallow the
    // selection-updating PointerPressed, leaving the previously right-clicked item
    // visually selected. Force selection to the currently right-clicked item here.
    private void EnsureItemSelectedForContextMenu(TreeViewItem item)
    {
        if (!IsSelectable || !item.IsSelectable)
        {
            return;
        }

        if (item.IsSelected)
        {
            return;
        }

        var treeItem = TreeItemFromContainer(item) ?? item;
        SelectedItem = treeItem;
    }

    // ContextMenu.Open() is a no-op while IsOpen==true, so a menu opened on A stays
    // pinned over A when the user right-clicks B. Close any still-open menu on the
    // TreeView or on any ancestor TreeViewItem before re-raising the request.
    private void CloseOpenContextMenus(TreeViewItem item)
    {
        if (ContextMenu is { IsOpen: true } treeMenu)
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

    private void HandleRootPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (this is FloatableTreeView { IsOpen: true } floating &&
            args.Source is ILogical control && !floating.IsLogicalAncestorOf(control))
        {
            floating.Close();
        }
    }

    private void HandleRawInput(RawInputEventArgs args)
    {
        if (args is RawPointerEventArgs { Type: RawPointerEventType.NonClientLeftButtonDown } && this is FloatableTreeView floating)
        {
            floating.Close();
        }
    }

    internal void OnCheckedChanged(TreeViewItem viewItem)
    {
        // A checked state being applied projects onto containers; those changes are not toggles.
        if (IsApplyingCheckedState)
        {
            return;
        }

        if (ToggleType == ItemToggleType.Radio && viewItem is IRadioButton radioButton)
        {
            _groupManager?.OnCheckedChanged(radioButton);
            return;
        }

        if (ToggleType != ItemToggleType.CheckBox)
        {
            return;
        }

        if (IsCheckStrictly)
        {
            UpdateStrictCheckedState(this, viewItem);
        }
        else
        {
            UpdateCascadingCheckedState(this, viewItem);
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

    private static void RemoveTreeViewItemFromRadioGroup(RadioButtonGroupManager manager, TreeViewItem element)
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
                RemoveTreeViewItemFromRadioGroup(manager, treeViewItem);
            }
        }
    }
}
