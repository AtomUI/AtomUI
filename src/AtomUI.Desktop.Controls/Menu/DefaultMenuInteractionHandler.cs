using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaDefaultMenuInteractionHandler = Avalonia.Controls.Platform.DefaultMenuInteractionHandler;

/// <summary>
/// Menu interaction handler shared by <see cref="Menu" />, <see cref="ContextMenu" /> and
/// <see cref="MenuFlyoutPresenter" />. It adds hover delays to the Avalonia default handler: a submenu opens
/// after a delay once the pointer enters its item, and an open submenu closes after a delay once the pointer
/// leaves it or enters a sibling item. A key the base handler handles, an access key or a pointer press cancels
/// the pending open and close.
/// </summary>
public class DefaultMenuInteractionHandler : AvaloniaDefaultMenuInteractionHandler, IMenuInteractionHandler
{
    /// <summary>
    /// Creates a handler that runs its hover delays with <c>DispatcherTimer.RunOnce</c>.
    /// </summary>
    /// <param name="isContextMenu">
    /// Whether the owner is a context menu rather than a menu bar. <see cref="Menu" /> passes
    /// <see langword="false" />, <see cref="ContextMenu" /> and <see cref="MenuFlyoutPresenter" /> pass
    /// <see langword="true" />.
    /// </param>
    public DefaultMenuInteractionHandler(bool isContextMenu)
        : base(isContextMenu)
    {
        _delayRun = (callback, delay) => DispatcherTimer.RunOnce(callback, delay);
    }

    /// <summary>
    /// Creates a handler that runs its hover delays through a scheduler supplied by the caller.
    /// </summary>
    /// <param name="isContextMenu">
    /// Whether the owner is a context menu rather than a menu bar.
    /// </param>
    /// <param name="inputManager">
    /// The input manager handed to the Avalonia base handler. May be <see langword="null" />.
    /// </param>
    /// <param name="delayRun">
    /// Runs a callback after a delay and returns a handle that cancels it. The handler wraps the callback so that it
    /// runs at most once and does nothing once the operation has been canceled.
    /// </param>
    public DefaultMenuInteractionHandler(
        bool isContextMenu,
        IInputManager? inputManager,
        Action<Action, TimeSpan> delayRun)
        : base(isContextMenu, inputManager, delayRun)
    {
        _delayRun = AdaptDelayRun(delayRun);
    }

    private readonly Func<Action, TimeSpan, IDisposable> _delayRun;
    private MenuBase? _owner;
    private MenuItem? _pendingOpenTarget;
    private IDisposable? _pendingOpenDelay;
    private long _openIntentGeneration;
    private MenuItem? _pendingCloseTarget;
    private IDisposable? _pendingCloseDelay;
    private long _closeIntentGeneration;

    /// <summary>
    /// Lets the base handler process the key first. When the base handler marks a key that was unhandled before as
    /// handled, the pending hover open and close are canceled, so that an earlier hover is not applied afterwards.
    /// </summary>
    protected override void KeyDown(object? sender, KeyEventArgs e)
    {
        var wasHandled = e.Handled;
        base.KeyDown(sender, e);

        if (!wasHandled && e.Handled)
        {
            CancelPendingHoverOperations();
        }
    }

    /// <summary>
    /// Cancels the pending hover open and close before the base handler processes the access key.
    /// </summary>
    protected override void AccessKeyPressed(object? sender, RoutedEventArgs e)
    {
        CancelPendingHoverOperations();
        base.AccessKeyPressed(sender, e);
    }

    /// <summary>
    /// Handles the pointer entering a menu item below the menu bar. The item and its ancestors are selected. A
    /// submenu item that is not open yet is scheduled to open once its pending close is canceled; any other item
    /// cancels the pending open and close and schedules an open sibling submenu to close. The base handler is not
    /// called for these items. A top-level item and a source outside a menu item cancel the pending hover
    /// operations and are passed to the base handler.
    /// </summary>
    protected override void PointerEntered(object? sender, RoutedEventArgs e)
    {
        var item = GetMenuItemCore(e.Source as Control);

        if (item?.Parent == null)
        {
            base.PointerEntered(sender, e);
            return;
        }

        if (item.IsTopLevel)
        {
            CancelPendingHoverOperations();
            base.PointerEntered(sender, e);
            return;
        }

        if (item.HasSubMenu)
        {
            CancelPendingClose(item);
            SelectItemAndAncestors(item);
            if (!item.IsSubMenuOpen)
            {
                ScheduleOpen(item);
            }
        }
        else
        {
            CancelPendingOpen();
            CancelPendingClose();
            SelectItemAndAncestors(item);

            var openedSibling = FindOpenedSibling(item);
            if (openedSibling != null)
            {
                ScheduleClose(openedSibling);
            }
        }
    }

    /// <summary>
    /// Does not call the base handler. While the left button is pressed and the pointer has left the bounds of the
    /// menu item under it, the pointer capture is released. The capture is kept while the source of the event is
    /// inside a scroll bar thumb. Events that have no menu item as source, or whose item has no transformed bounds,
    /// are ignored.
    /// </summary>
    protected override void PointerMoved(object? sender, PointerEventArgs e)
    {
        var item = GetMenuItemCore(e.Source as Control);

        if (item == null)
        {
            return;
        }

        var transformedBounds = item.GetTransformedBounds();
        if (transformedBounds == null)
        {
            return;
        }

        var point = e.GetCurrentPoint(null);

        if (point.Properties.IsLeftButtonPressed
            && transformedBounds.Value.Contains(point.Position) == false)
        {
            var sourceControl  = e.Source as Control;
            var scrollBarThumb = sourceControl.FindAncestorOfType<ScrollBarThumb>();
            if (scrollBarThumb != null)
            {
                return;
            }

            e.Pointer.Capture(null);
        }
    }

    /// <summary>
    /// Handles the pointer leaving a menu item below the menu bar. A submenu that is still closed cancels its pending
    /// open and clears the selection of its parent. An open submenu is scheduled to close, unless the pointer is over
    /// that submenu, in which case its pending close is canceled instead. The base handler is not called for these
    /// items. A top-level item and a source outside a menu item cancel the pending hover operations and are passed to
    /// the base handler.
    /// </summary>
    protected override void PointerExited(object? sender, RoutedEventArgs e)
    {
        var item = GetMenuItemCore(e.Source as Control);

        if (item?.Parent == null)
        {
            base.PointerExited(sender, e);
            return;
        }

        if (item.IsTopLevel)
        {
            CancelPendingHoverOperations();
            base.PointerExited(sender, e);
            return;
        }

        if (!item.IsSubMenuOpen)
        {
            CancelPendingOpen(item);
            ClearParentSelection(item);
            return;
        }

        if (item.IsPointerOverSubMenu)
        {
            CancelPendingClose(item);
        }
        else
        {
            ScheduleClose(item);
        }
    }

    /// <summary>
    /// Cancels the pending hover open and close before the base handler processes the press.
    /// </summary>
    protected override void PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CancelPendingHoverOperations();
        base.PointerPressed(sender, e);
    }

    void IMenuInteractionHandler.Attach(MenuBase menu)
    {
        base.Attach(menu);
        AttachOwner(menu);
    }

    void IMenuInteractionHandler.Detach(MenuBase menu)
    {
        try
        {
            base.Detach(menu);
        }
        finally
        {
            DetachOwner(menu);
        }
    }

    internal void CancelPendingHoverOperations()
    {
        CancelPendingOpen();
        CancelPendingClose();
    }

    internal static MenuItem? GetMenuItemCore(StyledElement? item)
    {
        while (true)
        {
            if (item == null)
            {
                return null;
            }

            if (item is MenuItem menuItem)
            {
                return menuItem;
            }

            item = item.Parent;
        }
    }

    private static Func<Action, TimeSpan, IDisposable> AdaptDelayRun(
        Action<Action, TimeSpan> delayRun)
    {
        return (callback, delay) =>
        {
            Action? callbackGate = callback;

            void InvokeCallback()
            {
                Interlocked.Exchange(ref callbackGate, null)?.Invoke();
            }

            try
            {
                delayRun(InvokeCallback, delay);
            }
            catch
            {
                Interlocked.Exchange(ref callbackGate, null);
                throw;
            }

            return Disposable.Create(() => Interlocked.Exchange(ref callbackGate, null));
        };
    }

    private void AttachOwner(MenuBase menu)
    {
        _owner = menu;
        menu.Closed += OwnerClosed;
    }

    private void DetachOwner(MenuBase menu)
    {
        if (!ReferenceEquals(_owner, menu))
        {
            return;
        }

        menu.Closed -= OwnerClosed;
        _owner = null;
        CancelPendingHoverOperations();
    }

    private void OwnerClosed(object? sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _owner))
        {
            CancelPendingHoverOperations();
        }
    }

    private void ScheduleOpen(MenuItem target)
    {
        if (ReferenceEquals(_pendingOpenTarget, target))
        {
            return;
        }

        CancelPendingOpen();
        CancelPendingClose(target);
        var generation       = _openIntentGeneration;
        _pendingOpenTarget   = target;
        _pendingOpenDelay    = null;

        IDisposable delay;
        try
        {
            delay = _delayRun(() => CommitOpen(target, generation), MenuShowDelay);
        }
        catch
        {
            CancelPendingOpen(target);
            throw;
        }

        if (IsCurrentOpenIntent(target, generation))
        {
            _pendingOpenDelay = delay;
        }
        else
        {
            delay.Dispose();
        }
    }

    private void CommitOpen(MenuItem target, long generation)
    {
        if (!TakeOpenIntent(target, generation))
        {
            return;
        }

        if (BelongsToOwner(target) &&
            target.HasSubMenu &&
            !target.IsSubMenuOpen &&
            IsSelectedContainer(target))
        {
            target.Open();
        }
    }

    private void ScheduleClose(MenuItem target)
    {
        if (ReferenceEquals(_pendingCloseTarget, target))
        {
            return;
        }

        CancelPendingOpen(target);
        CancelPendingClose();
        var generation       = _closeIntentGeneration;
        _pendingCloseTarget  = target;
        _pendingCloseDelay   = null;

        IDisposable delay;
        try
        {
            delay = _delayRun(() => CommitClose(target, generation), MenuShowDelay);
        }
        catch
        {
            CancelPendingClose(target);
            throw;
        }

        if (IsCurrentCloseIntent(target, generation))
        {
            _pendingCloseDelay = delay;
        }
        else
        {
            delay.Dispose();
        }
    }

    private void CommitClose(MenuItem target, long generation)
    {
        if (!TakeCloseIntent(target, generation))
        {
            return;
        }

        if (BelongsToOwner(target) &&
            target.IsSubMenuOpen &&
            !target.IsPointerOver &&
            !target.IsPointerOverSubMenu)
        {
            target.Close();
        }
    }

    private bool TakeOpenIntent(MenuItem target, long generation)
    {
        if (!IsCurrentOpenIntent(target, generation))
        {
            return false;
        }

        var delay = _pendingOpenDelay;
        _pendingOpenTarget = null;
        _pendingOpenDelay  = null;
        ++_openIntentGeneration;
        delay?.Dispose();
        return true;
    }

    private bool TakeCloseIntent(MenuItem target, long generation)
    {
        if (!IsCurrentCloseIntent(target, generation))
        {
            return false;
        }

        var delay = _pendingCloseDelay;
        _pendingCloseTarget = null;
        _pendingCloseDelay  = null;
        ++_closeIntentGeneration;
        delay?.Dispose();
        return true;
    }

    private bool IsCurrentOpenIntent(MenuItem target, long generation)
    {
        return generation == _openIntentGeneration &&
               ReferenceEquals(_pendingOpenTarget, target);
    }

    private bool IsCurrentCloseIntent(MenuItem target, long generation)
    {
        return generation == _closeIntentGeneration &&
               ReferenceEquals(_pendingCloseTarget, target);
    }

    private void CancelPendingOpen(MenuItem target)
    {
        if (ReferenceEquals(_pendingOpenTarget, target))
        {
            CancelPendingOpen();
        }
    }

    private void CancelPendingOpen()
    {
        var delay = _pendingOpenDelay;
        _pendingOpenTarget = null;
        _pendingOpenDelay  = null;
        ++_openIntentGeneration;
        delay?.Dispose();
    }

    private void CancelPendingClose(MenuItem target)
    {
        if (_pendingCloseTarget is { } pendingTarget &&
            IsSelfOrDescendantOf(target, pendingTarget))
        {
            CancelPendingClose();
        }
    }

    private void CancelPendingClose()
    {
        var delay = _pendingCloseDelay;
        _pendingCloseTarget = null;
        _pendingCloseDelay  = null;
        ++_closeIntentGeneration;
        delay?.Dispose();
    }

    private static bool IsSelfOrDescendantOf(MenuItem target, MenuItem ancestor)
    {
        StyledElement? current = target;
        while (current != null)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private bool BelongsToOwner(MenuItem target)
    {
        if (_owner == null)
        {
            return false;
        }

        StyledElement? current = target;
        while (current != null)
        {
            if (ReferenceEquals(current, _owner))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool IsSelectedContainer(MenuItem target)
    {
        if (target.Parent is not SelectingItemsControl parent)
        {
            return false;
        }

        var targetIndex = parent.IndexFromContainer(target);
        return targetIndex >= 0 &&
               parent.SelectedIndex == targetIndex &&
               ReferenceEquals(parent.ContainerFromIndex(targetIndex), target);
    }

    private static void SelectItemAndAncestors(MenuItem target)
    {
        var current = target;
        while (current.Parent is SelectingItemsControl parent)
        {
            var targetIndex = parent.IndexFromContainer(current);
            if (targetIndex < 0 || !ReferenceEquals(parent.ContainerFromIndex(targetIndex), current))
            {
                return;
            }

            parent.SelectedIndex = targetIndex;
            if (parent is not MenuItem parentItem)
            {
                return;
            }

            current = parentItem;
        }
    }

    private static void ClearParentSelection(MenuItem target)
    {
        if (target.Parent is not SelectingItemsControl parent)
        {
            return;
        }

        var targetIndex = parent.IndexFromContainer(target);
        if (targetIndex >= 0 &&
            parent.SelectedIndex == targetIndex &&
            ReferenceEquals(parent.ContainerFromIndex(targetIndex), target))
        {
            parent.SelectedIndex = -1;
        }
    }

    private static MenuItem? FindOpenedSibling(MenuItem target)
    {
        if (target.Parent is not SelectingItemsControl parent)
        {
            return null;
        }

        for (var index = 0; index < parent.ItemCount; index++)
        {
            if (parent.ContainerFromIndex(index) is MenuItem sibling &&
                !ReferenceEquals(sibling, target) &&
                sibling.IsSubMenuOpen)
            {
                return sibling;
            }
        }

        return null;
    }
}
