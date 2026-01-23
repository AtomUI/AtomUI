using System.Diagnostics;
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
    }

    public void Detach(CascaderView cascaderView)
    {
        if (CascaderView != cascaderView)
        {
            throw new NotSupportedException("DefaultCascaderViewInteractionHandler is not attached to the TreeView.");
        }
        
        CascaderView.PointerPressed  -= PointerPressed;
        CascaderView.PointerReleased -= PointerReleased;

        CascaderView = null;
        _root        = null;
    }

    internal static CascaderViewItem? GetCascaderViewItemCore(StyledElement? item)
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
        var item = GetCascaderViewItemCore(e.Source as Control);

        if (sender is Visual visual &&
            e.GetCurrentPoint(visual).Properties.IsLeftButtonPressed && item?.ItemCount > 0)
        {
            e.Handled = true;
        }
    }

    protected internal virtual void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var item = GetCascaderViewItemCore(e.Source as Control);

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
        if (CascaderView != null)
        {
            if (CascaderView.IsCheckable)
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
}