using System.Collections.Specialized;
using System.ComponentModel;
using AtomUI.Controls;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Position of an item in a <see cref="CompactSpace" />. The control passes it to
/// <see cref="ICompactSpaceAware.NotifyPositionChange" /> so that the item can drop the corners that touch a
/// neighbour. An item of a group with several items receives <see cref="First" />, <see cref="Middle" /> or
/// <see cref="Last" />, and a <see cref="CompactSpaceFiller" /> is not counted. The only item of a group
/// receives <see cref="First" /> and <see cref="Last" /> together and keeps all of its corners.
/// AtomUI.Desktop.Controls.ColorPicker keeps the value in a styled property of the color picker.
/// </summary>
[Flags]
internal enum SpaceItemPosition
{
    First  = 0x01,
    Middle = 0x02,
    Last   = 0x04,
}

/// <summary>
/// Lays its <see cref="Children" /> out as one joined row or column without gaps between the items. Each item
/// is told whether it is the first, a middle or the last item and what the orientation is, so that it can
/// drop the corners that touch a neighbour. The length an item takes comes from the
/// <c>CompactSpace.ItemSize</c> attached property.
/// </summary>
public class CompactSpace : TemplatedControl,
                            ICustomizableSizeTypeAware,
                            IChildIndexProvider,
                            INavigableContainer
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CompactSpace, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<CompactSpace>();
    
    /// <summary>
    /// The attached property for the width of an item in a horizontal <see cref="CompactSpace" /> and for its
    /// height in a vertical one. Set it on an item of <see cref="Children" />. A pixel size fixes the length, a
    /// star size such as <c>2*</c> gives a weighted share of the remaining length, and
    /// <see cref="CompactSpaceSize.Auto" /> sizes the item to its content, which is also the default. The value
    /// is read from the items of <see cref="Children" /> only and is not inherited.
    /// </summary>
    public static readonly AttachedProperty<CompactSpaceSize> ItemSizeProperty =
        AvaloniaProperty.RegisterAttached<CompactSpace, Control, CompactSpaceSize>(
            "ItemSize",
            defaultValue: CompactSpaceSize.Auto);
    
    /// <summary>
    /// Direction in which the items are lined up: <c>Horizontal</c> puts them in one row, <c>Vertical</c> in one
    /// column. The default is <c>Horizontal</c>.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Size preset of the group. The control only forwards it: an item that is size-type aware receives this value
    /// while it belongs to the control and loses it when it is removed, so that all items follow one preset.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// The items this control lays out, in layout order. It is the XAML content property. Each item has to
    /// implement <c>ICompactSpaceAware</c>; an item that does not is rejected with <see cref="ArgumentException" />.
    /// Use at most one <see cref="CompactSpaceFiller" /> and put it last.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when an item that does not implement <c>ICompactSpaceAware</c> is added to a control whose template
    /// is applied, or when the template is applied while such an item is in the list.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the list is cleared or an item is replaced, because Replace and Reset changes are not supported.
    /// </exception>
    [Content]
    public AvaloniaList<Control> Children { get; } = new();
    
    #endregion

    internal const int ACTIVE_ZINDEX = 1000;
    internal const int NORMAL_ZINDEX = 0;

    internal static bool HasPositionFlag(SpaceItemPosition position, SpaceItemPosition flag)
    {
        return (position & flag) == flag;
    }

    internal static bool IsPartialPosition(SpaceItemPosition position)
    {
        return !HasPositionFlag(position, SpaceItemPosition.First) ||
               !HasPositionFlag(position, SpaceItemPosition.Last);
    }

    internal static CornerRadius CalculateEffectiveCornerRadius(CornerRadius cornerRadius,
                                                                bool isUsedInCompactSpace,
                                                                SpaceItemPosition? compactSpaceItemPosition,
                                                                Orientation compactSpaceOrientation)
    {
        var topLeft     = cornerRadius.TopLeft;
        var topRight    = cornerRadius.TopRight;
        var bottomRight = cornerRadius.BottomRight;
        var bottomLeft  = cornerRadius.BottomLeft;
        if (isUsedInCompactSpace &&
            compactSpaceItemPosition.HasValue)
        {
            var position  = compactSpaceItemPosition.Value;
            var isFirst   = HasPositionFlag(position, SpaceItemPosition.First);
            var isMiddle  = HasPositionFlag(position, SpaceItemPosition.Middle);
            var isLast    = HasPositionFlag(position, SpaceItemPosition.Last);
            var isPartial = !isFirst || !isLast;
            if (isPartial && isFirst)
            {
                if (compactSpaceOrientation == Orientation.Horizontal)
                {
                    topRight    = 0;
                    bottomRight = 0;
                }
                else
                {
                    bottomLeft  = 0;
                    bottomRight = 0;
                }
            }
            else if (isPartial && isMiddle)
            {
                topRight    = 0;
                topLeft     = 0;
                bottomLeft  = 0;
                bottomRight = 0;
            }
            else if (isPartial && isLast)
            {
                if (compactSpaceOrientation == Orientation.Horizontal)
                {
                    topLeft    = 0;
                    bottomLeft = 0;
                }
                else
                {
                    topLeft  = 0;
                    topRight = 0;
                }
            }
        }
        return new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
    }

    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;
    private Grid? _contentLayout;
    private readonly Dictionary<object, NotifyCollectionChangedEventHandler> _childClassesChangedHandlers = new();

    static CompactSpace()
    {
        OrientationProperty.Changed.AddClassHandler<CompactSpace>((space, _) => space.ConfigureSizeDefinitions());
        TemplatedParentProperty.Changed.AddClassHandler<CompactSpace>((space, args) => space.HandleTemplatedParentChanged());
    }
    
    public CompactSpace()
    {
        Children.CollectionChanged += HandleChildrenChanged;
    }
    
    /// <summary>
    /// Sets the <see cref="ItemSizeProperty" /> attached value of an element.
    /// </summary>
    /// <param name="element">
    /// The element to set the size on.
    /// </param>
    /// <param name="size">
    /// The size: sized to the content, a fixed number of pixels, or a weighted share of the remaining length.
    /// </param>
    public static void SetItemSize(Control element, CompactSpaceSize size)
    {
        element.SetValue(ItemSizeProperty, size);
    }

    /// <summary>
    /// Reads the <see cref="ItemSizeProperty" /> attached value of an element.
    /// </summary>
    /// <param name="element">
    /// The element to read from.
    /// </param>
    /// <returns>
    /// The size set on the element, or <see cref="CompactSpaceSize.Auto" /> when none is set.
    /// </returns>
    public static CompactSpaceSize GetItemSize(Control element)
    {
        return element.GetValue(ItemSizeProperty);
    }
    
    /// <summary>
    /// Mirrors a change of <see cref="Children" /> into the layout grid: Add wraps each new item in a wrapper
    /// item at the same index, Move moves the wrappers, and Remove discards the wrappers of the removed items. It
    /// then rebuilds the size definitions, reports <c>ChildIndexesReset</c> to
    /// <see cref="IChildIndexProvider.ChildIndexChanged" /> subscribers and invalidates the measure. It does
    /// nothing while the template has not been applied; <see cref="OnApplyTemplate" /> then builds all wrappers.
    /// </summary>
    /// <param name="sender">
    /// The <see cref="Children" /> list that changed.
    /// </param>
    /// <param name="e">
    /// The collection change. Replace and Reset changes are not supported.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="e" /> describes a Replace or a Reset change.
    /// </exception>
    /// <remarks>
    /// An override must call the base implementation unless it maintains the wrappers itself.
    /// </remarks>
    protected virtual void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_contentLayout == null)
        {
            return;
        }
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                var compactSpaceItems = new List<CompactSpaceItem>(e.NewItems!.Count);
                for (var i = 0; i < e.NewItems.Count; i++)
                {
                    var newItem = (Control)e.NewItems[i]!;
                    EnsureCompactSpaceItem(newItem);
                    var compactSpaceItem = new CompactSpaceItem()
                    {
                        Child = newItem
                    };
                    NotifyAddCompactSpaceItem(compactSpaceItem);
                    compactSpaceItems.Add(compactSpaceItem);
                }
                _contentLayout.Children.InsertRange(e.NewStartingIndex, compactSpaceItems);
                break;
            }

            case NotifyCollectionChangedAction.Move:
                _contentLayout.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
            {
                var oldItems             = new HashSet<Control>(e.OldItems!.Count);
                for (var i = 0; i < e.OldItems.Count; i++)
                {
                    var oldItem = (Control)e.OldItems[i]!;
                    oldItems.Add(oldItem);
                }
                var oldCompactSpaceItems = new List<CompactSpaceItem>(e.OldItems.Count);
                foreach (var oldItem in _contentLayout.Children)
                {
                    if (oldItem is CompactSpaceItem compactSpaceItem)
                    {
                        if (oldItems.Contains(compactSpaceItem.Child!))
                        {
                            oldCompactSpaceItems.Add(compactSpaceItem);
                        }
                    }
                }
                foreach (var compactSpaceItem in oldCompactSpaceItems)
                {
                    NotifyRemoveCompactSpaceItem(compactSpaceItem);
                }
                _contentLayout.Children.RemoveAll(oldCompactSpaceItems);
                break;
            }

            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException();
        }
        
        ConfigureSizeDefinitions();
        _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
        InvalidateMeasureOnChildrenChanged();
    }

    /// <summary>
    /// Invalidates the measure after the <see cref="Children" /> collection changed or the control template was
    /// applied, so that the layout is rebuilt for the current items. Override it to run further updates at these
    /// points; call the base implementation from an override to keep the measure invalidated.
    /// </summary>
    private protected virtual void InvalidateMeasureOnChildrenChanged()
    {
        InvalidateMeasure();
    }

    /// <summary>
    /// Rebuilds the layout after the base implementation has run. It discards the wrappers of a previous template,
    /// takes the <c>PART_ContentLayout</c> grid from the new template and wraps every item of
    /// <see cref="Children" /> in a wrapper item inside that grid. It then rebuilds the size definitions, reports
    /// <c>ChildIndexesReset</c> to <see cref="IChildIndexProvider.ChildIndexChanged" /> subscribers and invalidates
    /// the measure.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ClearContentLayout();
        _contentLayout = e.NameScope.Get<Grid>("PART_ContentLayout");
        var compactSpaceItems = new List<CompactSpaceItem>(Children.Count);
        foreach (var child in Children)
        {
            EnsureCompactSpaceItem(child);
            var compactSpaceItem = new CompactSpaceItem()
            {
                Child = child,
            };
            NotifyAddCompactSpaceItem(compactSpaceItem);
            compactSpaceItems.Add(compactSpaceItem);
        }
        _contentLayout.Children.AddRange(compactSpaceItems);

        ConfigureSizeDefinitions();
        _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
        InvalidateMeasureOnChildrenChanged();
    }

    /// <summary>
    /// Measures the control. Before the base measure it looks for <see cref="CompactSpaceFiller" /> instances among
    /// the direct children of the layout grid. It throws when one is found at an index other than 0 or when more
    /// than one is found.
    /// </summary>
    /// <param name="availableSize">
    /// The available size. It is passed to the base implementation unchanged.
    /// </param>
    /// <returns>
    /// The size returned by the base implementation.
    /// </returns>
    /// <exception cref="InvalidSpaceFillerUsageException">
    /// Thrown when a filler is found at an index other than 0, or when more than one filler is found.
    /// </exception>
    /// <remarks>
    /// The direct children of the layout grid are the wrapper items that hold the items of
    /// <see cref="Children" />, not the items themselves. The check therefore does not see a filler that was added
    /// to <see cref="Children" />.
    /// </remarks>
    protected override Size MeasureCore(Size availableSize)
    {
        if (_contentLayout != null)
        {
            // Check the position and the count of the fillers.
            var fillerCount   = 0;
            var childrenCount = _contentLayout.Children.Count;
            for (var i = 0; i < childrenCount; i++)
            {
                var child = _contentLayout.Children[i];
                if (child is CompactSpaceFiller)
                {
                    ++fillerCount;
                    if (i != fillerCount - 1)
                    {
                        throw new InvalidSpaceFillerUsageException("The CompactSpaceFiller is misplaced, it can only be the last child.");
                    }
                }
            }

            if (fillerCount > 1)
            {
                throw new InvalidSpaceFillerUsageException("There can only be one CompactSpaceFiller.");
            }
        }
        return base.MeasureCore(availableSize);
    }

    /// <summary>
    /// Detaches the handlers that watch the class lists of the items, after the base implementation has run, so
    /// that a detached control stops reacting to class changes of its items.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ClearChildClassesChangedHandlers();
    }

    /// <summary>
    /// Registers a handler on the class list of each item, after the base implementation has run. The handler
    /// keeps an item whose content has focus above its neighbours.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ReRegisterChildClassesChangedHandlers();
    }

    #region 实现 IChildIndexProvider 接口

    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add
        {
            if (_childIndexChanged is null)
            {
                Children.PropertyChanged += HandleChildrenPropertyChanged;
            }
            _childIndexChanged += value;
        }

        remove
        {
            _childIndexChanged -= value;
            if (_childIndexChanged is null)
            {
                Children.PropertyChanged -= HandleChildrenPropertyChanged;
            }
        }
    }

    int IChildIndexProvider.GetChildIndex(ILogical child)
    {
        return child is Control control ? Children.IndexOf(control) : -1;
    }

    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        count = Children.Count;
        return true;
    }

    #endregion

    #region 实现 INavigableContainer 接口

    IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var  orientation = Orientation;
        var  children    = Children;
        bool horiz       = orientation == Orientation.Horizontal;
        int  index       = from is not null ? Children.IndexOf((Control)from) : -1;

        switch (direction)
        {
            case NavigationDirection.First:
                index = 0;
                break;
            case NavigationDirection.Last:
                index = children.Count - 1;
                break;
            case NavigationDirection.Next:
                ++index;
                break;
            case NavigationDirection.Previous:
                --index;
                break;
            case NavigationDirection.Left:
                index = horiz ? index - 1 : -1;
                break;
            case NavigationDirection.Right:
                index = horiz ? index + 1 : -1;
                break;
            case NavigationDirection.Up:
                index = horiz ? -1 : index - 1;
                break;
            case NavigationDirection.Down:
                index = horiz ? -1 : index + 1;
                break;
        }

        if (index >= 0 && index < children.Count)
        {
            return children[index];
        }
        return null;
    }

    #endregion

    private static void EnsureCompactSpaceItem(object item)
    {
        if (item is not ICompactSpaceAware)
        {
            throw new ArgumentException($"{item.GetType().FullName} is not ICompactSpaceAware.");
        }
    }

    private void NotifyAddCompactSpaceItem(CompactSpaceItem compactSpaceItem)
    {
        if (compactSpaceItem.Child != null)
        {
            var target                  = compactSpaceItem.Child;
            var targetCompactSpaceAware = target as ICompactSpaceAware;
            if (targetCompactSpaceAware != null && !targetCompactSpaceAware.IgnoreZIndexChange())
            {
                target.GotFocus       += HandleGotFocus;
                target.PointerEntered += HandlePointerEntered;
                target.PointerExited  += HandlePointerExited;
            }

            target.PropertyChanged += CompactSpaceItemChildPropertyChanged;
            SetItemSize(compactSpaceItem, GetItemSize(target));

            RegisterChildClassesChangedHandler(compactSpaceItem, target);
            if (targetCompactSpaceAware != null && targetCompactSpaceAware.IsAlwaysActiveZIndex())
            {
                compactSpaceItem.ZIndex = ACTIVE_ZINDEX;
            }

            if (target is ICustomizableSizeTypeAware)
            {
                target[!SizeTypeProperty] = this[!SizeTypeProperty];
            }
            target.SetTemplatedParent(TemplatedParent);
        }
    }
    
    private void NotifyRemoveCompactSpaceItem(CompactSpaceItem compactSpaceItem)
    {
        if (compactSpaceItem.Child != null)
        {
            var target                  = compactSpaceItem.Child;
            var targetCompactSpaceAware = target as ICompactSpaceAware;
            if (targetCompactSpaceAware != null && !targetCompactSpaceAware.IgnoreZIndexChange())
            {
                target.GotFocus       -= HandleGotFocus;
                target.PointerEntered -= HandlePointerEntered;
                target.PointerExited  -= HandlePointerExited;
            }
            target.PropertyChanged -= CompactSpaceItemChildPropertyChanged;
        
            if (_childClassesChangedHandlers.TryGetValue(target, out var childClassesChangedHandler))
            {
                target.Classes.CollectionChanged -= childClassesChangedHandler;
                _childClassesChangedHandlers.Remove(target);
            }

            targetCompactSpaceAware?.NotifyPositionChange(null);
            if (target is ICustomizableSizeTypeAware)
            {
                target.ClearValue(SizeTypeProperty);
            }

            target.SetTemplatedParent(null);
            compactSpaceItem.Child = null;
        }
    }

    private void CompactSpaceItemChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is Control currentControl && sender is ICompactSpaceAware)
        {
            var compactSpaceItem = FindCompactSpaceItem(currentControl);
            if (compactSpaceItem != null && e.Property == ItemSizeProperty && e.NewValue is CompactSpaceSize compactSpaceSize)
            {
                SetItemSize(compactSpaceItem, compactSpaceSize);
            }
        }
    }
    
    #region 孩子的 ZIndex 处理方法

    private void HandleGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (sender is Control currentControl && _contentLayout != null)
        {
            foreach (var child in _contentLayout.Children)
            {
                if (child is CompactSpaceItem compactSpaceItem && compactSpaceItem.Child is ICompactSpaceAware compactSpaceAware)
                {
                    if (compactSpaceItem.Child == currentControl)
                    {
                        compactSpaceItem.ZIndex = ACTIVE_ZINDEX;
                    }
                    else if (!compactSpaceAware.IsAlwaysActiveZIndex())
                    {
                        compactSpaceItem.ZIndex = NORMAL_ZINDEX;
                    }
                }
            }
        }
    }

    private void HandlePointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Control currentControl && _contentLayout != null)
        {
            foreach (var child in _contentLayout.Children)
            {
                if (child is CompactSpaceItem compactSpaceItem && compactSpaceItem.Child is ICompactSpaceAware compactSpaceAware)
                {
                    if (compactSpaceItem.Child == currentControl)
                    {
                        compactSpaceItem.ZIndex = ACTIVE_ZINDEX;
                    }
                    else if (!IsEffectiveFocused(compactSpaceItem) && !compactSpaceAware.IsAlwaysActiveZIndex())
                    {
                        compactSpaceItem.ZIndex = NORMAL_ZINDEX;
                    }
                }
            }
        }
    }

    private void HandlePointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is not Control target)
        {
            return;
        }

        var compactSpaceItem = FindCompactSpaceItem(target);
        if (compactSpaceItem?.Child is ICompactSpaceAware compactSpaceAware)
        {
            if (!IsEffectiveFocused(compactSpaceItem) && !compactSpaceAware.IsAlwaysActiveZIndex())
            {
                compactSpaceItem.ZIndex = NORMAL_ZINDEX;
            }
        }
    }

    private void HandleChildFocusWithinChanged(Control control, bool focused)
    {
        if (focused)
        {
            control.ZIndex = ACTIVE_ZINDEX;
        }
    }

    private bool IsEffectiveFocused(CompactSpaceItem control)
    {
        if (control.Child == null)
        {
            return false;
        }
        var target = control.Child;
        return target.Classes.Contains(StdPseudoClass.FocusWithIn) || target.IsFocused;
    }

    private CompactSpaceItem? FindCompactSpaceItem(Control target)
    {
        if (_contentLayout == null)
        {
            return null;
        }

        foreach (var child in _contentLayout.Children)
        {
            if (child is CompactSpaceItem compactSpaceItem && compactSpaceItem.Child == target)
            {
                return compactSpaceItem;
            }
        }

        return null;
    }
    #endregion

    private void ClearChildClassesChangedHandlers()
    {
        foreach (var (target, handler) in _childClassesChangedHandlers)
        {
            if (target is Control control)
            {
                control.Classes.CollectionChanged -= handler;
            }
        }
        _childClassesChangedHandlers.Clear();
    }

    private void ClearContentLayout()
    {
        if (_contentLayout == null)
        {
            ClearChildClassesChangedHandlers();
            return;
        }

        var compactSpaceItems = new List<CompactSpaceItem>();
        foreach (var child in _contentLayout.Children)
        {
            if (child is CompactSpaceItem compactSpaceItem)
            {
                compactSpaceItems.Add(compactSpaceItem);
            }
        }

        foreach (var compactSpaceItem in compactSpaceItems)
        {
            NotifyRemoveCompactSpaceItem(compactSpaceItem);
        }
        _contentLayout.Children.Clear();
        ClearChildClassesChangedHandlers();
    }

    private void ReRegisterChildClassesChangedHandlers()
    {
        if (_contentLayout == null)
        {
            return;
        }

        foreach (var child in _contentLayout.Children)
        {
            if (child is CompactSpaceItem compactSpaceItem && compactSpaceItem.Child is { } target)
            {
                RegisterChildClassesChangedHandler(compactSpaceItem, target);
            }
        }
    }

    private void RegisterChildClassesChangedHandler(CompactSpaceItem compactSpaceItem, Control target)
    {
        if (_childClassesChangedHandlers.ContainsKey(target))
        {
            return;
        }

        NotifyCollectionChangedEventHandler handler = (_, _) =>
        {
            HandleChildFocusWithinChanged(compactSpaceItem, target.Classes.Contains(StdPseudoClass.FocusWithIn));
        };
        _childClassesChangedHandlers.Add(target, handler);
        target.Classes.CollectionChanged += handler;
    }

    private void ConfigureSizeDefinitions()
    {
        if (_contentLayout == null || _contentLayout.Children.Count == 0)
        {
            return;
        }
        var children = _contentLayout.Children;
        if (Orientation == Orientation.Horizontal)
        {
            var columnDefinitions = new ColumnDefinitions();
            for (var i = 0; i < children.Count; i++)
            {
                var child    = children[i];
                Grid.SetColumn(child, i);
                var itemSize = GetItemSize(child);
                if (itemSize.IsAbsolute)
                {
                    columnDefinitions.Add(new ColumnDefinition(itemSize.Value, GridUnitType.Pixel));
                }
                else if (itemSize.IsAuto)
                {
                    columnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                }
                else
                {
                    columnDefinitions.Add(new ColumnDefinition(itemSize.Value, GridUnitType.Star));
                }
            }
            _contentLayout.ColumnDefinitions = columnDefinitions;
        }
        else
        {
            var rowDefinitions = new RowDefinitions();
            for (var i = 0; i < children.Count; i++)
            {
                var child    = children[i];
                Grid.SetRow(child, i);
                var itemSize = GetItemSize(child);
                if (itemSize.IsAbsolute)
                {
                    rowDefinitions.Add(new RowDefinition(itemSize.Value, GridUnitType.Pixel));
                }
                else if (itemSize.IsAuto)
                {
                    rowDefinitions.Add(new RowDefinition(GridLength.Auto));
                }
                else
                {
                    rowDefinitions.Add(new RowDefinition(itemSize.Value, GridUnitType.Star));
                }
            }
            _contentLayout.RowDefinitions = rowDefinitions;
        }
        
        // Compute the position of each item, then notify it of the position and the orientation.
        var realChildrenCount = CountRealChildren(children);
        if (realChildrenCount == 1)
        {
            if (FindFirstRealChild(children) is ICompactSpaceAware compactSpaceAware)
            {
                compactSpaceAware.NotifyPositionChange(SpaceItemPosition.First | SpaceItemPosition.Last);
            }
        }
        else
        {
            var realChildIndex = 0;
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (!IsRealChild(child))
                {
                    continue;
                }

                if (child is CompactSpaceItem compactSpaceItem)
                {
                    compactSpaceItem.PositionIndex = realChildIndex;
                }
                if (child is ICompactSpaceAware compactSpaceAware)
                {
                    compactSpaceAware.NotifyOrientationChange(Orientation);
                    if (realChildIndex == 0)
                    {
                        compactSpaceAware.NotifyPositionChange(SpaceItemPosition.First);
                    }
                    else if (realChildIndex == realChildrenCount - 1)
                    {
                        compactSpaceAware.NotifyPositionChange(SpaceItemPosition.Last);
                    }
                    else
                    {
                        compactSpaceAware.NotifyPositionChange(SpaceItemPosition.Middle);
                    }
                }

                realChildIndex++;
            }
        }
    }

    private static int CountRealChildren(Avalonia.Controls.Controls children)
    {
        var count = 0;
        for (var i = 0; i < children.Count; i++)
        {
            if (IsRealChild(children[i]))
            {
                count++;
            }
        }

        return count;
    }

    private static Control? FindFirstRealChild(Avalonia.Controls.Controls children)
    {
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (IsRealChild(child))
            {
                return child;
            }
        }

        return null;
    }

    private static bool IsRealChild(Control child)
    {
        return child is CompactSpaceItem { Child: not CompactSpaceFiller };
    }
    
    private void HandleChildrenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Children.Count) || e.PropertyName is null)
        {
            _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.TotalCountChanged);
        }
    }
    
    private void HandleTemplatedParentChanged()
    {
        foreach (var child in Children)
        {
            child.SetTemplatedParent(TemplatedParent);
        }
    }
}
