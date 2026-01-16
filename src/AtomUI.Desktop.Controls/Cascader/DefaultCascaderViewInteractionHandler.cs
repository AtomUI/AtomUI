using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Rendering;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class DefaultCascaderViewInteractionHandler : ICascaderViewInteractionHandler
{
    protected IInputManager? InputManager { get; }
    internal CascaderView? CascaderView { get; private set; }
    private IRenderRoot? _root;
    private RadioButtonGroupManager? _groupManager;

    public DefaultCascaderViewInteractionHandler()
    {
        InputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
    }

    public void Attach(CascaderView cascaderView)
    {
        CascaderView                 =  cascaderView;
        CascaderView.PointerPressed  += PointerPressed;
        CascaderView.PointerReleased += PointerReleased;
        _root                        =  CascaderView.GetVisualRoot();
        if (_root is not null)
        {
            _groupManager = RadioButtonGroupManager.GetOrCreateForRoot(_root);
            for (var i = 0; i < CascaderView.ItemCount; i++)
            {
                if (CascaderView.ContainerFromIndex(i) is CascaderViewItem item)
                {
                    AddTreeViewItemToRadioGroup(_groupManager, item);
                }
            }
        }
    }

    public void Detach(CascaderView cascaderView)
    {
        if (CascaderView != cascaderView)
        {
            throw new NotSupportedException("DefaultCascaderViewInteractionHandler is not attached to the TreeView.");
        }
        
        CascaderView.PointerPressed  -= PointerPressed;
        CascaderView.PointerReleased -= PointerReleased;
        
        if (_root is not null && _groupManager is { } oldManager)
        {
            _groupManager = null;
            for (var i = 0; i < CascaderView.ItemCount; i++)
            {
                if (CascaderView.ContainerFromIndex(i) is CascaderViewItem item)
                {
                    RemoveTreeViewItemToRadioGroup(oldManager, item);
                }
            }
        }

        CascaderView = null;
        _root        = null;
    }

    internal static CascaderViewItem? GetTreeViewItemCore(StyledElement? item)
    {
        while (true)
        {
            if (item == null)
            {
                return null;
            }

            if (item is CascaderViewItem cascaderViewItem)
            {
                return cascaderViewItem;
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
                Click(item);
                e.Handled = true;
            }
        }
    }

    internal void Click(CascaderViewItem item)
    {
        item.RaiseClick();
    }
    
    internal void OnCheckedChanged(CascaderViewItem item)
    {
        Debug.Assert(CascaderView != null);
        if (CascaderView.ToggleType == ItemToggleType.Radio && item is IRadioButton radioButton)
        {
            _groupManager?.OnCheckedChanged(radioButton);
        }
        else if (CascaderView.ToggleType == ItemToggleType.CheckBox)
        {
            if (!CascaderView.IsCheckStrictly)
            {
                if (item.IsChecked.HasValue)
                {
                    if (item.IsChecked.Value)
                    {
                        CascaderView.CheckedSubTree(item);
                    }
                    else
                    {
                        CascaderView.UnCheckedSubTree(item);
                    }
                }
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

    private static void AddTreeViewItemToRadioGroup(RadioButtonGroupManager manager, CascaderViewItem element)
    {
        // Instead add menu item to the group on attached/detached + ensure checked stated on attached.
        if (element is IRadioButton button)
        {
            manager.Add(button);
        }

        for (var i = 0; i < element.ItemCount; i++)
        {
            var item = element.ContainerFromIndex(i);
            if (item is CascaderViewItem cascaderViewItem)
            {
                AddTreeViewItemToRadioGroup(manager, cascaderViewItem);
            }
        }
    }

    private static void RemoveTreeViewItemToRadioGroup(RadioButtonGroupManager manager, CascaderViewItem element)
    {
        if (element is IRadioButton button)
        {
            manager.Remove(button, button.GroupName);
        }

        for (var i = 0; i < element.ItemCount; i++)
        {
            var item = element.ContainerFromIndex(i);
            if (item is CascaderViewItem cascaderViewItem)
            {
                RemoveTreeViewItemToRadioGroup(manager, cascaderViewItem);
            }
        }
    }
}