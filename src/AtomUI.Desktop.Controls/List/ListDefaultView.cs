using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class ListDefaultView : SelectingItemsControl
{
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());

    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ListDefaultView>();
    
    public static readonly DirectProperty<ListDefaultView, IScrollable?> ScrollProperty =
        AvaloniaProperty.RegisterDirect<ListDefaultView, IScrollable?>(nameof(ListDefaultView), o => o.Scroll);
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1010",
        Justification = "This property is owned by SelectingItemsControl, but protected there. ListDefaultView changes its visibility.")]
    public new static readonly DirectProperty<SelectingItemsControl, IList?> SelectedItemsProperty =
        SelectingItemsControl.SelectedItemsProperty;
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1010",
        Justification = "This property is owned by SelectingItemsControl, but protected there. ListDefaultView changes its visibility.")]
    public new static readonly DirectProperty<SelectingItemsControl, ISelectionModel> SelectionProperty =
        SelectingItemsControl.SelectionProperty;
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1010",
        Justification = "This property is owned by SelectingItemsControl, but protected there. ListDefaultView changes its visibility.")]
    public new static readonly StyledProperty<SelectionMode> SelectionModeProperty =
        SelectingItemsControl.SelectionModeProperty;
    
    public static readonly StyledProperty<bool> IsItemSelectableProperty =
       List.IsItemSelectableProperty.AddOwner<ListDefaultView>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<ListDefaultView>();
    
    public static readonly StyledProperty<bool> DisabledItemHoverEffectProperty =
        List.DisabledItemHoverEffectProperty.AddOwner<ListDefaultView>();
    
    public static readonly StyledProperty<bool> IsShowSelectedIndicatorProperty =
        List.IsShowSelectedIndicatorProperty.AddOwner<ListDefaultView>();
    
    public static readonly StyledProperty<IDataTemplate?> GroupItemTemplateProperty =
        List.GroupItemTemplateProperty.AddOwner<ListDefaultView>();
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private IScrollable? _scroll;
    public IScrollable? Scroll
    {
        get => _scroll;
        private set => SetAndRaise(ScrollProperty, ref _scroll, value);
    }
    
    public new IList? SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }
    
    public new ISelectionModel Selection
    {
        get => base.Selection;
        set => base.Selection = value;
    }
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1012",
        Justification = "This property is owned by SelectingItemsControl, but protected there. List changes its visibility.")]
    public new SelectionMode SelectionMode
    {
        get => base.SelectionMode;
        set => base.SelectionMode = value;
    }
    
    public bool IsItemSelectable
    {
        get => GetValue(IsItemSelectableProperty);
        set => SetValue(IsItemSelectableProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool DisabledItemHoverEffect
    {
        get => GetValue(DisabledItemHoverEffectProperty);
        set => SetValue(DisabledItemHoverEffectProperty, value);
    }
    
    public bool IsShowSelectedIndicator
    {
        get => GetValue(IsShowSelectedIndicatorProperty);
        set => SetValue(IsShowSelectedIndicatorProperty, value);
    }
    
    public IDataTemplate? GroupItemTemplate
    {
        get => GetValue(GroupItemTemplateProperty);
        set => SetValue(GroupItemTemplateProperty, value);
    }
    #endregion
    
    internal List? OwnerList { get; set; }
    
    private protected readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();

    static ListDefaultView()
    {
        ItemsPanelProperty.OverrideDefaultValue<List>(DefaultPanel);
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue(
            typeof(List),
            KeyboardNavigationMode.Once);
    }
   
    public ListDefaultView()
    {
        LogicalChildren.CollectionChanged += HandleChildrenChanged;
    }
    
    private void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    DisposableListItem(item);
                }
            }
        }
    }

    private protected void DisposableListItem(object item)
    {
        if (_itemsBindingDisposables.TryGetValue(item, out var disposable))
        {
            disposable.Dispose();
            _itemsBindingDisposables.Remove(item);
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        Debug.Assert(OwnerList != null);
        return OwnerList.CreateContainerForItemOverride(item, index, recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        Debug.Assert(OwnerList != null);
        
        var disposables = new CompositeDisposable(4);
        OwnerList.PrepareContainerForItemOverride(disposables, container, item, index);
        DisposableListItem(container);
        _itemsBindingDisposables.Add(container, disposables);
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsItemSelectable)
        {
            var hotkeys = Application.Current!.PlatformSettings?.HotkeyConfiguration;
            var ctrl    = hotkeys is not null && e.KeyModifiers.HasAllFlags(hotkeys.CommandModifiers);

            if (!ctrl &&
                e.Key.ToNavigationDirection() is { } direction && 
                direction.IsDirectional())
            {
                e.Handled |= MoveSelection(
                    direction,
                    WrapSelection,
                    e.KeyModifiers.HasAllFlags(KeyModifiers.Shift));
            }
            else if (SelectionMode.HasAllFlags(SelectionMode.Multiple) &&
                     hotkeys is not null && hotkeys.SelectAll.Any(x => x.Matches(e)))
            {
                Selection.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                UpdateSelectionFromEventSource(
                    e.Source,
                    true,
                    e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                    ctrl);
            }

        }
        
        base.OnKeyDown(e);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Scroll = e.NameScope.Find<IScrollable>(ListThemeConstants.ScrollViewerPart);
    }
    
    protected internal virtual bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        if (OwnerList != null && OwnerList.UpdateSelectionFromPointerEvent(source, e))
        {
            return true;
        }
        if (IsItemSelectable)
        {
            var hotkeys = Application.Current!.PlatformSettings?.HotkeyConfiguration;
            var toggle  = hotkeys is not null && e.KeyModifiers.HasAllFlags(hotkeys.CommandModifiers);
            return UpdateSelectionFromEventSource(
                source,
                true,
                e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
                toggle,
                e.GetCurrentPoint(source).Properties.IsRightButtonPressed);
        }
        return false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsItemSelectableProperty)
        {
            if (!IsItemSelectable)
            {
                SetCurrentValue(SelectedItemsProperty, null);
                SetCurrentValue(SelectedItemProperty, null);
                SetCurrentValue(SelectedIndexProperty, -1);
            }
        }
        else if (change.Property == SelectionModeProperty)
        {
            if (SelectionMode == SelectionMode.Multiple)
            {
                if (SelectedItems != null)
                {
                    var selected = new List<object>();
                    foreach (var item in SelectedItems)
                    {
                        selected.Add(item);
                    }
                    SetCurrentValue(SelectedItemsProperty, selected);
                }
            }
        }
    }
    
}