using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A <see cref="TreeView" /> that keeps track of the flyout assigned to its
/// <see cref="TreeViewFlyout" /> property: <see cref="IsOpen" /> mirrors the open state of the flyout,
/// <see cref="Opened" /> and <see cref="Closed" /> are raised when the flyout opens and closes,
/// <see cref="Close" /> hides the flyout, and the flyout is shown at the tree while
/// <c>IsPopupPinnedOpen</c> is <see langword="true" />.
/// </summary>
public class FloatableTreeView : TreeView
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly DirectProperty<FloatableTreeView, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<FloatableTreeView, bool>(
            nameof(IsOpen),
            o => o.IsOpen);

    private bool _isOpen;

    /// <summary>
    /// Whether the flyout assigned to <see cref="TreeViewFlyout" /> is open. The value follows that flyout while
    /// the tree is attached to the visual tree, and it is <see langword="false" /> while the tree is detached or
    /// no flyout is assigned. Only derived classes can set it.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        protected set => SetAndRaise(IsOpenProperty, ref _isOpen, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="Opened" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<FloatableTreeView, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Closed" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<FloatableTreeView, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the flyout assigned to <see cref="TreeViewFlyout" /> opens, after <see cref="IsOpen" /> has
    /// been set to <see langword="true" />. The event bubbles up the visual tree.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    /// <summary>
    /// Raised when the flyout assigned to <see cref="TreeViewFlyout" /> closes, after <see cref="IsOpen" /> has
    /// been set to <see langword="false" />. The event bubbles up the visual tree.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    #endregion

    private TreeViewFlyout? _flyout;
    private TreeViewFlyout? _registeredFlyout;
    private int _pinnedOpenGeneration;

    /// <summary>
    /// The flyout that this tree keeps track of while it is attached to the visual tree: its open state is
    /// mirrored in <see cref="IsOpen" />, its opening and closing raise <see cref="Opened" /> and
    /// <see cref="Closed" />, and <see cref="Close" /> hides it. Assigning another flyout or
    /// <see langword="null" /> releases the flyout tracked before: <see cref="IsOpen" /> becomes
    /// <see langword="false" /> and that flyout is told to close.
    /// </summary>
    public TreeViewFlyout? TreeViewFlyout
    {
        get => _flyout;
        set
        {
            if (ReferenceEquals(_flyout, value))
            {
                return;
            }

            _flyout = value;
            NotifyFlyoutAssigned(value);
        }
    }

    public FloatableTreeView()
    {
    }

    /// <summary>
    /// Initializes the tree with the handler for user interaction with the nodes.
    /// </summary>
    /// <param name="interactionHandler">
    /// The handler that reacts to user interaction with the nodes, used instead of the default handler.
    /// </param>
    protected FloatableTreeView(ITreeViewInteractionHandler interactionHandler)
        : base(interactionHandler)
    {
    }

    /// <summary>
    /// Hides the flyout assigned to <see cref="TreeViewFlyout" />. Does nothing when no flyout is assigned.
    /// </summary>
    public void Close()
    {
        if (TreeViewFlyout is not null)
        {
            TreeViewFlyout.Hide();
        }
    }

    /// <summary>
    /// Moves the tracking to the flyout newly assigned to <see cref="TreeViewFlyout" />: the flyout tracked before
    /// is released, and the new one is tracked from now on when the tree is attached to the visual tree, or from
    /// the moment the tree attaches. Only a <see cref="AtomUI.Desktop.Controls.TreeViewFlyout" /> is tracked.
    /// Overrides must call the base implementation to keep <see cref="IsOpen" />, <see cref="Opened" /> and
    /// <see cref="Closed" /> in step with the flyout.
    /// </summary>
    /// <param name="flyout">
    /// The newly assigned flyout, or <see langword="null" />.
    /// </param>
    protected virtual void NotifyFlyoutAssigned(Flyout? flyout)
    {
        UnregisterFlyout(_registeredFlyout);
        if (flyout is TreeViewFlyout treeViewFlyout && this.IsAttachedToVisualTree())
        {
            RegisterFlyout(treeViewFlyout);
        }
    }

    /// <summary>
    /// Registers the flyout assigned to <see cref="TreeViewFlyout" /> for tracking, after the base implementation
    /// has run.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RegisterFlyout(TreeViewFlyout);
    }

    /// <summary>
    /// Releases the tracked flyout, which sets <see cref="IsOpen" /> to <see langword="false" />, and then calls
    /// the base implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnregisterFlyout(_registeredFlyout);
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base implementation, then forwards a change of <c>IsPopupPinnedOpen</c> to the tracked flyout. A
    /// value of <see langword="true" /> shows the flyout at this tree when the tree is attached, visible and
    /// enabled and the flyout is not open yet; any other value cancels a show that is still pending.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (_registeredFlyout is { } flyout)
            {
                flyout.SetCurrentValue(Flyout.IsPopupPinnedOpenProperty, change.GetNewValue<bool>());
            }

            if (change.GetNewValue<bool>() && this.IsAttachedToVisualTree())
            {
                QueuePinnedOpen();
            }
            else
            {
                ++_pinnedOpenGeneration;
            }
        }
    }

    private void RegisterFlyout(TreeViewFlyout? flyout)
    {
        if (flyout is null || ReferenceEquals(flyout, _registeredFlyout))
        {
            return;
        }

        UnregisterFlyout(_registeredFlyout);
        _registeredFlyout = flyout;
        flyout.Opened += HandleFlyoutOpened;
        flyout.Closed += HandleFlyoutClosed;
        flyout.SetCurrentValue(Flyout.IsPopupPinnedOpenProperty, IsPopupPinnedOpen);
        IsOpen = flyout.IsOpen;
        if (IsPopupPinnedOpen)
        {
            QueuePinnedOpen();
        }
    }

    private void UnregisterFlyout(TreeViewFlyout? flyout)
    {
        if (flyout is null || !ReferenceEquals(flyout, _registeredFlyout))
        {
            return;
        }

        ++_pinnedOpenGeneration;
        flyout.CloseForLifecycle();
        flyout.Opened -= HandleFlyoutOpened;
        flyout.Closed -= HandleFlyoutClosed;
        flyout.SetCurrentValue(Flyout.IsPopupPinnedOpenProperty, false);
        _registeredFlyout = null;
        IsOpen = false;
    }

    private void QueuePinnedOpen()
    {
        var generation = ++_pinnedOpenGeneration;
        var flyout     = _registeredFlyout;
        Dispatcher.UIThread.Post(() =>
        {
            if (generation == _pinnedOpenGeneration &&
                IsPopupPinnedOpen &&
                this.IsAttachedToVisualTree() &&
                IsEffectivelyEnabled &&
                IsVisible &&
                ReferenceEquals(flyout, _registeredFlyout) &&
                flyout is { IsOpen: false })
            {
                flyout.ShowAt(this);
            }
        }, DispatcherPriority.Loaded);
    }

    private void HandleFlyoutOpened(object? sender, EventArgs e)
    {
        IsOpen = true;
        RaiseEvent(new RoutedEventArgs
        {
            RoutedEvent = OpenedEvent,
            Source      = this,
        });
    }

    private void HandleFlyoutClosed(object? sender, EventArgs e)
    {
        IsOpen = false;
        RaiseEvent(new RoutedEventArgs
        {
            RoutedEvent = ClosedEvent,
            Source      = this,
        });
    }
}
