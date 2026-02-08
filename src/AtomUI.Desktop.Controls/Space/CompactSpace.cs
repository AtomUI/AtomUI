using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[Flags]
internal enum SpaceItemPosition
{
    First  = 0x01,
    Middle = 0x02,
    Last   = 0x04,
}

public class CompactSpace : TemplatedControl,
                            IControlSharedTokenResourcesHost,
                            ISizeTypeAware,
                            IChildIndexProvider,
                            INavigableContainer
{
    internal const int ACTIVE_ZINDEX = 1000;
    internal const int NORMAL_ZINDEX = 0;
    
    #region 公共属性定义

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CompactSpace, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<CompactSpace>();
    
    public static readonly AttachedProperty<CompactSpaceSize> ItemSizeProperty =
        AvaloniaProperty.RegisterAttached<CompactSpace, Control, CompactSpaceSize>(
            "ItemSize",
            defaultValue: CompactSpaceSize.Auto);
    
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    [Content]
    public AvaloniaList<Control> Children { get; } = new();
    
    #endregion
    
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

    #region 内部属性定义
    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SpaceToken.ID;

    #endregion

    private Grid? _contentLayout;
    private readonly Dictionary<object, NotifyCollectionChangedEventHandler> _childClassesChangedHandlers = new();
    private readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();

    static CompactSpace()
    {
        AffectsMeasure<CompactSpace>(ItemSizeProperty, SizeTypeProperty, OrientationProperty);
        OrientationProperty.Changed.AddClassHandler<CompactSpace>((space, _) => space.ConfigureSizeDefinitions());
    }
    
    public CompactSpace()
    {
        this.RegisterResources();
        Children.CollectionChanged += HandleChildrenChanged;
    }
    
    public static void SetItemSize(Control element, CompactSpaceSize size)
    {
        element.SetValue(ItemSizeProperty, size);
    }

    public static CompactSpaceSize GetItemSize(Control element)
    {
        return element.GetValue(ItemSizeProperty);
    }
    
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
                var newItems          = e.NewItems!.OfType<Control>().ToList();
                var compactSpaceItems = new List<CompactSpaceItem>();
                foreach (var newItem in newItems)
                {
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
                var oldItems          = e.OldItems!.OfType<Control>().ToList();
                var oldCompactSpaceItems = new List<CompactSpaceItem>();
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
            var target = compactSpaceItem.Child;
            
            target.GotFocus       += HandleGotFocus;
            target.PointerEntered += HandlePointerEntered;
            target.PointerExited  += HandlePointerExited;

            target.PropertyChanged += CompactSpaceItemChildPropertyChanged;
            SetItemSize(compactSpaceItem, GetItemSize(target));
        
            NotifyCollectionChangedEventHandler childClassesChangedHandler = delegate (object? sender, NotifyCollectionChangedEventArgs e)
            {
                HandleChildFocusWithinChanged(compactSpaceItem, target.Classes.Contains(StdPseudoClass.FocusWithIn));
            };
            _childClassesChangedHandlers.Add(target, childClassesChangedHandler);
            target.Classes.CollectionChanged += childClassesChangedHandler;
            if (target is ICompactSpaceAware compactSpaceAware && compactSpaceAware.IsAlwaysActiveZIndex())
            {
                compactSpaceItem.ZIndex = ACTIVE_ZINDEX;
            }

            if (target is ISizeTypeAware)
            {
                var disposables = new CompositeDisposable(2);
                disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, target, SizeTypeProperty));
                _itemsBindingDisposables.Add(target, disposables);
            }
        }
    }
    
    private void NotifyRemoveCompactSpaceItem(CompactSpaceItem compactSpaceItem)
    {
        if (compactSpaceItem.Child != null)
        {
            var target = compactSpaceItem.Child;
            target.GotFocus        -= HandleGotFocus;
            target.PointerEntered  -= HandlePointerEntered;
            target.PointerExited   -= HandlePointerExited;
            target.PropertyChanged -= CompactSpaceItemChildPropertyChanged;
        
            if (_childClassesChangedHandlers.TryGetValue(target, out var childClassesChangedHandler))
            {
                target.Classes.CollectionChanged -= childClassesChangedHandler;
                _childClassesChangedHandlers.Remove(target);
            }

            if (_itemsBindingDisposables.TryGetValue(target, out var disposables))
            {
                disposables.Dispose();
                _itemsBindingDisposables.Remove(target);
            }
        }
        
    }

    private void CompactSpaceItemChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is Control currentControl && sender is ICompactSpaceAware)
        {
            var compactSpaceItem = currentControl.FindAncestorOfType<CompactSpaceItem>();
            if (compactSpaceItem != null && e.Property == ItemSizeProperty && e.NewValue is CompactSpaceSize compactSpaceSize)
            {
                SetItemSize(compactSpaceItem, compactSpaceSize);
            }
        }
    }
    
    #region 孩子的 ZIndex 处理方法

    private void HandleGotFocus(object? sender, GotFocusEventArgs e)
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
                    else  if (!IsEffectiveFocused(compactSpaceItem) && !compactSpaceAware.IsAlwaysActiveZIndex())
                    {
                        compactSpaceItem.ZIndex = NORMAL_ZINDEX;
                    }
                }
            }
        }
    }

    private void HandlePointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is CompactSpaceItem compactSpaceItem && compactSpaceItem.Child is ICompactSpaceAware compactSpaceAware)
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
    #endregion
    
    private protected virtual void InvalidateMeasureOnChildrenChanged()
    {
        InvalidateMeasure();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _contentLayout = e.NameScope.Get<Grid>(CompactSpaceThemeConstants.ContentLayoutPart);
        var compactSpaceItems = new List<CompactSpaceItem>();
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

    protected override Size MeasureCore(Size availableSize)
    {
        if (_contentLayout != null)
        {
            // 检查 filler 位置和数量
            var fillerCount = 0;
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
        
        // 计算位置
        var realChildren = children.Where(child => child is CompactSpaceItem compactSpaceItem && compactSpaceItem.Child is not CompactSpaceFiller).ToList();
        if (realChildren.Count == 1)
        {
            if (realChildren[0] is ICompactSpaceAware compactSpaceAware)
            {
                compactSpaceAware.NotifyPositionChange(SpaceItemPosition.First | SpaceItemPosition.Last);
            }
        }
        for (var i = 0; i < realChildren.Count; i++)
        {
            var child = realChildren[i];
            if (child is CompactSpaceItem compactSpaceItem)
            {
                compactSpaceItem.PositionIndex = i;
            }
            if (child is ICompactSpaceAware compactSpaceAware)
            {
                compactSpaceAware.NotifyOrientationChange(Orientation);
                if (i == 0)
                {
                    compactSpaceAware.NotifyPositionChange(SpaceItemPosition.First);
                }
                else if (i == realChildren.Count - 1)
                {
                    compactSpaceAware.NotifyPositionChange(SpaceItemPosition.Last);
                }
                else
                {
                    compactSpaceAware.NotifyPositionChange(SpaceItemPosition.Middle);
                }
            }
        }
    }
    
    private void HandleChildrenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Children.Count) || e.PropertyName is null)
        {
            _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.TotalCountChanged);
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
}