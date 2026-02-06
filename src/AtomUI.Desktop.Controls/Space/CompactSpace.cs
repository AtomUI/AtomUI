using System.Collections.Specialized;
using System.ComponentModel;
using AtomUI.Controls;
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

namespace AtomUI.Desktop.Controls;

internal enum SpaceItemPosition
{
    First,
    Last,
    Middle,
    FirstAndLast
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
    private Dictionary<object, NotifyCollectionChangedEventHandler> _childClassesChangedHandlers = new();

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
                var newItems = e.NewItems!.OfType<Control>().ToList();
                foreach (var newItem in newItems)
                {
                    EnsureCompactSpaceItem(newItem);
                    NotifyAddCompactSpaceItem(newItem);
                }
                _contentLayout.Children.InsertRange(e.NewStartingIndex, newItems);
                break;

            case NotifyCollectionChangedAction.Move:
                _contentLayout.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                var oldItems = e.OldItems!.OfType<Control>().ToList();
                foreach (var oldItem in oldItems)
                {
                    NotifyRemoveCompactSpaceItem(oldItem);
                }
                _contentLayout.Children.RemoveAll(oldItems);
                break;

            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.OldItems!.Count; ++i)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (Control)e.NewItems![i]!;
                    _contentLayout.Children[index] = child;
                }
                break;

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

    private void NotifyAddCompactSpaceItem(Control child)
    {
        child.GotFocus       += HandleGotFocus;
        child.PointerEntered += HandlePointerEntered;
        child.PointerExited  += HandlePointerExited;
        NotifyCollectionChangedEventHandler childClassesChangedHandler = delegate (object? sender, NotifyCollectionChangedEventArgs e)
        {
            HandleChildFocusWithinChanged(child, child.Classes.Contains(StdPseudoClass.FocusWithIn));
        };
        _childClassesChangedHandlers.Add(child, childClassesChangedHandler);
        child.Classes.CollectionChanged += childClassesChangedHandler;
    }
    
    private void NotifyRemoveCompactSpaceItem(Control child)
    {
        child.GotFocus       -= HandleGotFocus;
        child.PointerEntered -= HandlePointerEntered;
        child.PointerExited  -= HandlePointerExited;
        if (_childClassesChangedHandlers.TryGetValue(child, out var childClassesChangedHandler))
        {
            child.Classes.CollectionChanged -= childClassesChangedHandler;
            _childClassesChangedHandlers.Remove(child);
        }
    }

    #region 孩子的 ZIndex 处理方法

    private void HandleGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is Control currentControl)
        {
            foreach (var child in Children)
            {
                if (child != null)
                {
                    child.ZIndex = child == currentControl ? ACTIVE_ZINDEX : NORMAL_ZINDEX;
                }
            }
        }
    }

    private void HandlePointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Control currentControl)
        {
            foreach (var child in Children)
            {
                if (child != null)
                {
                    if (child == currentControl)
                    {
                        child.ZIndex = ACTIVE_ZINDEX;
                    }
                    else if (!IsEffectiveFocused(child))
                    {
                        child.ZIndex = NORMAL_ZINDEX;
                    }
                }
            }
        }
    }

    private void HandlePointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Control control)
        {
            if (!IsEffectiveFocused(control))
            {
                control.ZIndex = NORMAL_ZINDEX;
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

    private bool IsEffectiveFocused(Control control)
    {
        return control.Classes.Contains(StdPseudoClass.FocusWithIn) || control.IsFocused;
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

        foreach (var child in Children)
        {
            EnsureCompactSpaceItem(child);
            NotifyAddCompactSpaceItem(child);
        }
        _contentLayout.Children.AddRange(Children);

        ConfigureSizeDefinitions();
        _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
        InvalidateMeasureOnChildrenChanged();
    }

    private void ConfigureSizeDefinitions()
    {
        if (_contentLayout == null || Children.Count == 0)
        {
            return;
        }

        if (Orientation == Orientation.Horizontal)
        {
            var columnDefinitions = new ColumnDefinitions();
            for (var i = 0; i < Children.Count; i++)
            {
                var child    = Children[i];
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
            for (var i = 0; i < Children.Count; i++)
            {
                var child    = Children[i];
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
        var realChildren = Children.Where(child => child is not CompactSpaceFiller).ToList();
        if (realChildren.Count == 1)
        {
            if (realChildren[0] is ICompactSpaceAware compactSpaceAware)
            {
                compactSpaceAware.NotifyPositionChange(SpaceItemPosition.FirstAndLast);
            }
        }
        for (var i = 0; i < realChildren.Count; i++)
        {
            var child = realChildren[i];
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