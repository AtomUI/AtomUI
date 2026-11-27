using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shows the items of a <see cref="TreeViewFlyout" /> in its popup: a <see cref="FloatableTreeView" /> that
/// renders its items inside the <see cref="ArrowDecoratedBox" /> of its template and answers the shadow mask
/// queries <see cref="GetMaskCornerRadius" />, <see cref="GetMaskBounds" /> and <see cref="GetMaskBackground" />
/// from that box.
/// </summary>
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public class TreeViewFlyoutPresenter : FloatableTreeView, IShadowMaskInfoProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<TreeViewFlyoutPresenter>();

    /// <summary>
    /// Identifies the <see cref="ArrowPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<TreeViewFlyoutPresenter>();

    /// <summary>
    /// Gets or sets whether the box of the template shows its indicator arrow. While it is <see langword="false" />,
    /// the arrow is not laid out and the outline of a bordered box leaves it out. The theme forwards the value to
    /// the box. A hosting <see cref="TreeViewFlyout" /> relays its own arrow visibility here. Without a value, the
    /// default of <see cref="ArrowDecoratedBox" />, the owner of the property, applies.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the edge of the box that carries the indicator arrow and, for the edge-aligned values, the end
    /// of the edge it sits toward. The value moves only the arrow; where the box lies next to its target is up to
    /// the host. The theme forwards the value to the box. A hosting <see cref="TreeViewFlyout" /> relays its own
    /// arrow position here. Without a value, the default of <see cref="ArrowDecoratedBox" />, the owner of the
    /// property, applies.
    /// </summary>
    public ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="TreeViewItemClicked" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<FlyoutTreeViewItemClickedEventArgs> TreeViewItemClickedEvent =
        RoutedEvent.Register<TreeViewFlyoutPresenter, FlyoutTreeViewItemClickedEventArgs>(
            nameof(TreeViewItemClicked),
            RoutingStrategies.Bubble);
    
    /// <summary>
    /// Raised by the <c>Click</c> handler this presenter attaches to its <see cref="MenuItem" /> items, when the
    /// clicked item is a <see cref="TreeViewItem" />. The event bubbles up from this presenter and carries the
    /// clicked node in <see cref="FlyoutTreeViewItemClickedEventArgs.ViewItem" />.
    /// </summary>
    public event EventHandler<FlyoutTreeViewItemClickedEventArgs>? TreeViewItemClicked
    {
        add => AddHandler(TreeViewItemClickedEvent, value);
        remove => RemoveHandler(TreeViewItemClickedEvent, value);
    }

    #endregion
        
    #region 内部属性定义
    
    private ArrowDecoratedBox? _arrowDecoratedBox;

    #endregion
    
    /// <inheritdoc />
    private protected override bool CloseOnNonClientPointer => true;

    /// <summary>
    /// Creates a presenter in floating tree view mode.
    /// </summary>
    public TreeViewFlyoutPresenter()
    {
    }

    /// <summary>
    /// Creates a presenter that delegates the handling of input on its nodes to
    /// <paramref name="menuInteractionHandler" />.
    /// </summary>
    public TreeViewFlyoutPresenter(ITreeViewInteractionHandler menuInteractionHandler)
        : base(menuInteractionHandler)
    {
    }
    
    /// <summary>
    /// Called once a container has been prepared for its item. Calls the base member first, then, when the
    /// container is a <see cref="MenuItem" />, attaches the <see cref="TreeViewItemClicked" /> handler to that item
    /// and to its nested <see cref="MenuItem" /> items. Overrides must call the base member to keep both.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            BindTreeViewItemClickedRecursive(menuItem);
        }
    }
    
    /// <summary>
    /// Called when the container of an item is cleared. Calls the base member first, then, when the container is a
    /// <see cref="MenuItem" />, detaches the <see cref="TreeViewItemClicked" /> handler from that item and from its
    /// nested <see cref="MenuItem" /> items. Overrides must call the base member to keep both.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);
        if (container is MenuItem menuItem)
        {
            ClearMenuItemClickedRecursive(menuItem);
        }
    }
    
    private void BindTreeViewItemClickedRecursive(MenuItem menuItem)
    {
        foreach (var childItem in menuItem.Items)
        {
            if (childItem is MenuItem childMenuItem)
            {
                BindTreeViewItemClickedRecursive(childMenuItem);
            }
        }

        // The item itself comes last, after its nested items.
        menuItem.Click += HandleTreeViewItemClicked;
    }

    private void ClearMenuItemClickedRecursive(MenuItem menuItem)
    {
        foreach (var childItem in menuItem.Items)
        {
            if (childItem is MenuItem childMenuItem)
            {
                ClearMenuItemClickedRecursive(childMenuItem);
            }
        }

        // The item itself comes last, after its nested items.
        menuItem.Click -= HandleTreeViewItemClicked;
    }

    private void HandleTreeViewItemClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is TreeViewItem treeViewItem)
        {
            var ev = new FlyoutTreeViewItemClickedEventArgs(TreeViewItemClickedEvent, treeViewItem);
            RaiseEvent(ev);
        }
    }
    
    /// <summary>
    /// Returns the corner radius the shadow mask follows: the <c>CornerRadius</c> of the box of the template, or a
    /// radius of <c>0</c> while the template is not applied or has no <c>ArrowDecoratorPart</c>.
    /// </summary>
    public CornerRadius GetMaskCornerRadius()
    {
        if (_arrowDecoratedBox is not null)
        {
            return _arrowDecoratedBox.CornerRadius;
        }

        return new CornerRadius(0);
    }
    
    /// <summary>
    /// Returns the bounds of the shadow mask in the coordinates of this presenter: the content area of the box of
    /// the template, which leaves out the arrow, or the <c>Bounds</c> of this presenter while the template is not
    /// applied or has no <c>ArrowDecoratorPart</c>.
    /// </summary>
    public Rect GetMaskBounds()
    {
        if (_arrowDecoratedBox is not null)
        {
            var contentRect = _arrowDecoratedBox.GetMaskBounds();
            var adjustedPos = _arrowDecoratedBox.TranslatePoint(contentRect.Position, this) ?? default;
            return new Rect(adjustedPos, contentRect.Size);
        }

        return Bounds;
    }

    /// <summary>
    /// Returns the brush the shadow mask uses: the <c>Background</c> of the box of the template, or
    /// <see langword="null" /> while the template is not applied or has no <c>ArrowDecoratorPart</c>.
    /// </summary>
    public IBrush? GetMaskBackground()
    {
        return _arrowDecoratedBox?.Background;
    }
    
    /// <summary>
    /// Calls the base member, then stores the <c>ArrowDecoratorPart</c> of the new template that the mask queries
    /// read, or <see langword="null" /> when the template has no such part. Overrides must call the base member to
    /// keep <see cref="GetMaskCornerRadius" />, <see cref="GetMaskBounds" /> and <see cref="GetMaskBackground" />
    /// answering from the box.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _arrowDecoratedBox = e.NameScope.Find<ArrowDecoratedBox>(ArrowDecoratedBox.ArrowDecoratorPart);
    } 
    
    /// <summary>
    /// Binds the motion switch of a <see cref="MenuItem" /> container (<c>IsMotionEnabled</c>) to
    /// <see cref="TreeView.IsMotionEnabledProperty" /> of this presenter, so the motion of each item follows the
    /// setting of the presenter, and then calls the base member.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is MenuItem menuItem)
        {
            menuItem[!MenuItem.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        }

        base.PrepareContainerForItemOverride(container, item, index);
    }
}

/// <summary>
/// Carries the clicked <see cref="TreeViewItem" /> of a
/// <see cref="TreeViewFlyoutPresenter.TreeViewItemClicked" /> event.
/// </summary>
public class FlyoutTreeViewItemClickedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The tree node whose click raised the event.
    /// </summary>
    public TreeViewItem ViewItem { get; }

    /// <summary>
    /// Creates the arguments for a click on <paramref name="treeViewItem" />.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event to raise, <see cref="TreeViewFlyoutPresenter.TreeViewItemClickedEvent" />.
    /// </param>
    /// <param name="treeViewItem">
    /// The clicked tree node, exposed as <see cref="ViewItem" />.
    /// </param>
    public FlyoutTreeViewItemClickedEventArgs(RoutedEvent routedEvent, TreeViewItem treeViewItem)
        : base(routedEvent)
    {
        ViewItem = treeViewItem;
    }
}
