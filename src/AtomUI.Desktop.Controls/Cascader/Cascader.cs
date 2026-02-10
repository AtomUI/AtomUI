using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Input;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using ItemCollection = AtomUI.Collections.ItemCollection;
using ItemsSourceView = AtomUI.Collections.ItemsSourceView;

public class Cascader : AbstractSelect, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<TreeSelectCheckedStrategy> ShowCheckedStrategyProperty =
        AvaloniaProperty.Register<Cascader, TreeSelectCheckedStrategy>(
            nameof(ShowCheckedStrategy), TreeSelectCheckedStrategy.All);
    
    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<Cascader, bool>(
            nameof(IsMultiple));
    
    public static readonly StyledProperty<IEnumerable?> OptionsSourceProperty =
        CascaderView.OptionsSourceProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        CascaderView.OptionTemplateProperty.AddOwner<Cascader>();
    
    public static readonly DirectProperty<Cascader, ICascaderOption?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<Cascader, ICascaderOption?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v);
    
    public static readonly DirectProperty<CascaderView, IList<ICascaderOption>?> CheckedItemsProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, IList<ICascaderOption>?>(
            nameof(CheckedItems),
            o => o.CheckedItems,
            (o, v) => o.CheckedItems = v);
    
    public static readonly StyledProperty<IconTemplate?> ExpandIconProperty =
        CascaderView.ExpandIconProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IconTemplate?> LoadingIconProperty =
        CascaderView.LoadingIconProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<CascaderViewExpandTrigger> ExpandTriggerProperty =
        CascaderView.ExpandTriggerProperty.AddOwner<Cascader>();

    public static readonly StyledProperty<ICascaderItemDataLoader?> DataLoaderProperty =
        CascaderView.DataLoaderProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<ICascaderItemFilter?> FilterProperty =
        CascaderView.FilterProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<TextBlockHighlightStrategy> FilterHighlightStrategyProperty =
        CascaderView.FilterHighlightStrategyProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        CascaderView.FilterHighlightForegroundProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<TreeNodePath?> DefaultSelectItemPathProperty =
        AvaloniaProperty.Register<Cascader, TreeNodePath?>(nameof(DefaultSelectItemPath));
    
    public static readonly StyledProperty<bool> IsAllowSelectParentProperty =
        CascaderView.IsAllowSelectParentProperty.AddOwner<Cascader>();
    
    public TreeSelectCheckedStrategy ShowCheckedStrategy
    {
        get => GetValue(ShowCheckedStrategyProperty);
        set => SetValue(ShowCheckedStrategyProperty, value);
    }
    
    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }
    
    public IEnumerable? OptionsSource
    {
        get => GetValue(OptionsSourceProperty);
        set => SetValue(OptionsSourceProperty, value);
    }
    
    [InheritDataTypeFromItems("OptionsSource")]
    public IDataTemplate? OptionTemplate
    {
        get => GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }
    
    private ICascaderOption? _selectedItem;
    
    public ICascaderOption? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }
        
    private IList<ICascaderOption>? _checkedItems;
    
    public IList<ICascaderOption>? CheckedItems
    {
        get => _checkedItems;
        set => SetAndRaise(CheckedItemsProperty, ref _checkedItems, value);
    }
    
    public IconTemplate? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }
    
    public IconTemplate? LoadingIcon
    {
        get => GetValue(LoadingIconProperty);
        set => SetValue(LoadingIconProperty, value);
    }
    
    public CascaderViewExpandTrigger ExpandTrigger
    {
        get => GetValue(ExpandTriggerProperty);
        set => SetValue(ExpandTriggerProperty, value);
    }

    public ICascaderItemDataLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }
    
    public ICascaderItemFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    public TextBlockHighlightStrategy FilterHighlightStrategy
    {
        get => GetValue(FilterHighlightStrategyProperty);
        set => SetValue(FilterHighlightStrategyProperty, value);
    }
    
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    public TreeNodePath? DefaultSelectItemPath
    {
        get => GetValue(DefaultSelectItemPathProperty);
        set => SetValue(DefaultSelectItemPathProperty, value);
    }
    
    public bool IsAllowSelectParent
    {
        get => GetValue(IsAllowSelectParentProperty);
        set => SetValue(IsAllowSelectParentProperty, value);
    }
    
    public ItemsSourceView OptionsView => _options;
    
    [Content]
    public ItemCollection Options => _options;
    
    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<Cascader, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<Cascader, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    internal static readonly DirectProperty<Cascader, IList<ICascaderOption>?> EffectiveCheckedItemsProperty =
        AvaloniaProperty.RegisterDirect<Cascader, IList<ICascaderOption>?>(
            nameof(EffectiveCheckedItems),
            o => o.EffectiveCheckedItems,
            (o, v) => o.EffectiveCheckedItems = v);
    
    internal static readonly DirectProperty<Cascader, string?> SelectedItemPathProperty =
        AvaloniaProperty.RegisterDirect<Cascader, string?>(
            nameof(SelectedItemPath),
            o => o.SelectedItemPath,
            (o, v) => o.SelectedItemPath = v);
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }
    
    private IList<ICascaderOption>? _effectiveCheckedItems;

    internal IList<ICascaderOption>? EffectiveCheckedItems
    {
        get => _effectiveCheckedItems;
        set => SetAndRaise(EffectiveCheckedItemsProperty, ref _effectiveCheckedItems, value);
    }
    
    private string? _selectItemPath;

    internal string? SelectedItemPath
    {
        get => _selectItemPath;
        set => SetAndRaise(SelectedItemPathProperty, ref _selectItemPath, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CascaderToken.ID;

    #endregion
    
    private readonly ItemCollection _options = new();
    private SelectFilterTextBox? _singleFilterInput;
    private CascaderView? _cascaderView;
    private bool _needSkipSyncCheckedItems;

    static Cascader()
    {
        FocusableProperty.OverrideDefaultValue<Cascader>(true);
        SelectHandle.ClearRequestedEvent.AddClassHandler<Cascader>((target, args) => target.HandleClearRequest());
        SelectFilterTextBox.TextChangedEvent.AddClassHandler<Cascader>((x, e) => x.HandleSearchInputTextChanged(e));
        SelectTag.ClosedEvent.AddClassHandler<Cascader>((x, e) => x.HandleTagCloseRequest(e));
        IsMultipleProperty.Changed.AddClassHandler<Cascader>((cascader, e) => cascader.HandleIsCheckableChanged());
        OptionsSourceProperty.Changed.AddClassHandler<Cascader>((view, args) => view.HandleCascaderSourceChanged(args));
    }
    
    public Cascader()
    {
        this.RegisterResources();
        Options.CollectionChanged += HandleCascaderOptionsChanged;
    }
    
    private void HandleCascaderSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        _options.SetItemsSource(args.GetNewValue<IEnumerable?>());
    }

    private void HandleCascaderOptionsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (_cascaderView != null)
        {
            _cascaderView.OptionsSource = Options.ToList();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, new DefaultCascaderItemFilter());
        }
    }
    
    private void HandleClearRequest()
    {
        Clear();
    }
    
    public void Clear()
    {
        SelectedItem     = null;
        SelectedItemPath = null;
        CheckedItems     = null;
    }
    
    private void ConfigurePlaceholderVisible()
    {
        if (IsMultiple)
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, (CheckedItems == null || CheckedItems?.Count == 0) && 
                                                              string.IsNullOrWhiteSpace(ActivateFilterValue) &&
                                                              string.IsNullOrWhiteSpace(SelectedItemPath));
        }
        else
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, SelectedItem == null && 
                                                              string.IsNullOrWhiteSpace(ActivateFilterValue) &&
                                                              string.IsNullOrWhiteSpace(SelectedItemPath));
        }
       
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        _ = e ?? throw new ArgumentNullException(nameof(e));
        base.OnKeyDown(e);
        if (e.Handled || !IsEnabled)
        {
            return;
        }
        
        if (IsDropDownOpen)
        {
            if (_cascaderView != null)
            {
                _cascaderView.HandleKeyDown(e);
                if (e.Handled)
                {
                    return;
                }
            }

            if (e.Key == Key.Escape)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
                e.Handled = true;
            }
        }
        else
        {
            // The drop down is not open, the Down key will toggle it open.
            // Ignore key buttons, if they are used for XY focus.
            if (e.Key == Key.Down
                && !this.IsAllowedXYNavigationMode(e.KeyDeviceType))
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
                e.Handled = true;
            }
        }

        // Standard drop down navigation
        switch (e.Key)
        {
            case Key.F4:
                SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
                e.Handled = true;
                break;

            default:
                break;
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty)
        {
            PseudoClasses.Set(SelectPseudoClass.DropdownOpen, change.GetNewValue<bool>());
            ConfigureSingleFilterTextBox();
        }
        if (change.Property == StyleVariantProperty ||
            change.Property == StatusProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == IsPopupMatchSelectWidthProperty)
        {
            ConfigurePopupMinWith(DesiredSize.Width);
        }
        else if (change.Property == CheckedItemsProperty ||
                 change.Property == SelectedItemProperty ||
                 change.Property == SelectedItemPathProperty ||
                 change.Property == IsMultipleProperty)
        {
            ConfigurePlaceholderVisible();
            ConfigureSelectionIsEmpty();
            if (IsMultiple)
            {
                SetCurrentValue(SelectedCountProperty, CheckedItems?.Count ?? 0);
            }
            else
            {
                SetCurrentValue(SelectedCountProperty, SelectedItem != null ? 1 : 0);
            }
        }
        
        if (change.Property == CheckedItemsProperty ||
            change.Property == IsMultipleProperty)
        {
            SyncCheckedItemsToCascaderView();
        }

        if (change.Property == IsMultipleProperty ||
            change.Property == MaxCountProperty ||
            change.Property == EffectiveCheckedItemsProperty ||
            change.Property == IsMultipleProperty)
        {
            ConfigureMaxSelectReached();
        }
        
        if (change.Property == CheckedItemsProperty ||
            change.Property == ShowCheckedStrategyProperty)
        {
            BuildEffectiveSelectedItems();
        }

        if (change.Property == SelectedItemProperty)
        {
            ConfigureSelectedItemPath();
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (Popup != null)
        {
            Popup.Opened -= HandlePopupOpened;
            Popup.Closed -= HandlePopupClosed;
        }
        
        if (_cascaderView != null)
        {
            _cascaderView.CheckedItemsChanged -= HandleCascaderViewItemsCheckedChanged;
            _cascaderView.ItemDoubleClicked   -= HandleCascaderViewItemDoubleClicked;
            _cascaderView.ItemSelected        -= HandleCascaderViewItemSelected;
            _cascaderView.OptionsSource       =  null;
        }
        
        _singleFilterInput = e.NameScope.Find<SelectFilterTextBox>(CascaderThemeConstants.SingleFilterInputPart);
        Popup              = e.NameScope.Find<Popup>(CascaderThemeConstants.PopupPart);
        _cascaderView      = e.NameScope.Find<CascaderView>(CascaderThemeConstants.CascaderViewPart);
        
        if (_cascaderView != null)
        {
            _cascaderView.CheckedItemsChanged += HandleCascaderViewItemsCheckedChanged;
            _cascaderView.ItemDoubleClicked   += HandleCascaderViewItemDoubleClicked;
            _cascaderView.ItemSelected        += HandleCascaderViewItemSelected;
            _cascaderView.OptionsSource       =  Options.Cast<ICascaderOption>().ToList();
        }
        
        if (Popup != null)
        {
            Popup.ClickHidePredicate =  PopupClosePredicate;
            Popup.Opened             += HandlePopupOpened;
            Popup.Closed             += HandlePopupClosed;
        }
        
        ConfigurePlaceholderVisible();
        ConfigureSelectionIsEmpty();
        UpdatePseudoClasses();
        ConfigureSingleFilterTextBox();
    }
    
    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        SubscriptionsOnOpen.Clear();
        NotifyPopupClosed();
        if (!IsMultiple)
        {
            if (_singleFilterInput != null)
            {
                _singleFilterInput.Clear();
                _singleFilterInput.Width = double.NaN;
                ActivateFilterValue      = null;
            }
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        SubscriptionsOnOpen.Clear();
        this.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(SubscriptionsOnOpen);
        foreach (var parent in this.GetVisualAncestors().OfType<Control>())
        {
            parent.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(SubscriptionsOnOpen);
        }
        NotifyPopupOpened();
        if (!IsMultiple)
        {
            _singleFilterInput?.Focus();
        }
    }

    private void HandleCascaderViewItemsCheckedChanged(object? sender, CascaderViewCheckedItemsChangedEventArgs e)
    {
        if (_cascaderView == null)
        {
            return;
        }
        var                           needSync        = false;
        HashSet<ICascaderOption>? cascaderViewSet = null;
        if (_checkedItems == null || _checkedItems?.Count != _cascaderView.CheckedItems?.Count)
        {
            needSync = true;
        }
        else
        {
            var currentSet      = _checkedItems?.Cast<ICascaderOption>().ToHashSet() ?? new HashSet<ICascaderOption>();
            cascaderViewSet = _cascaderView?.CheckedItems?.Cast<ICascaderOption>().ToHashSet() ?? new HashSet<ICascaderOption>();
            if (!currentSet.SetEquals(cascaderViewSet))
            {
                needSync = true;
            }
        }
        
        if (needSync)
        {
            try
            {
                _needSkipSyncCheckedItems =   true;
                cascaderViewSet           ??= _cascaderView?.CheckedItems?.Cast<ICascaderOption>().ToHashSet();
                if (cascaderViewSet != null)
                {
                    CheckedItems             = cascaderViewSet.ToList();
                }
           
            }
            finally
            {
                _needSkipSyncCheckedItems = false;
            }
        }
    }

    private void HandleIsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }
    
    private void ConfigureSelectionIsEmpty()
    {
        if (IsMultiple)
        {
            SetCurrentValue(IsSelectionEmptyProperty, CheckedItems == null || CheckedItems?.Count == 0);
        }
        else
        {
            SetCurrentValue(IsSelectionEmptyProperty, SelectedItem == null);
        }
    }
    
    private void ConfigureSingleFilterTextBox()
    {
        if (_singleFilterInput != null)
        {
            if (IsDropDownOpen)
            {
                _singleFilterInput.Width = _singleFilterInput.Bounds.Width;
            }
        }
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if(!e.Handled && e.Source is Visual source)
        {
            if (Popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
                return;
            }
        }
        if (IsDropDownOpen)
        {
            // When a drop-down is open with OverlayDismissEventPassThrough enabled and the control
            // is pressed, close the drop-down
            if (e.Source is Control sourceControl)
            {
                var filterTextBox = sourceControl.FindAncestorOfType<SelectFilterTextBox>();
                if (filterTextBox != null)
                {
                    IgnorePopupClose = true;
                    return;
                }
       
                var parent = sourceControl.FindAncestorOfType<IconButton>();
                var tag    = parent?.FindAncestorOfType<SelectTag>();
                if (tag != null)
                {
                    IgnorePopupClose = true;
                }
            }
            else
            {
                SetCurrentValue(IsDropDownOpenProperty, false); 
                e.Handled = true;
            }
        }
        else
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
        }
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.Source is Visual source)
        {
            if (Popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
            }
            else if (PseudoClasses.Contains(StdPseudoClass.Pressed))
            {
                var clickInTagCloseButton = false;
                if (e.Source is Control sourceControl)
                {
                    var parent = sourceControl.FindAncestorOfType<IconButton>();
                    var tag    = parent?.FindAncestorOfType<SelectTag>();
                    if (tag != null)
                    {
                        clickInTagCloseButton = true;
                    }
                }
    
                if (!clickInTagCloseButton)
                {
                    SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
                }
                e.Handled = true;
            }
        }
    
        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }
    
    private void HandleCascaderViewItemDoubleClicked(object? sender, CascaderItemDoubleClickedEventArgs eventArgs)
    {
        if (!IsMultiple && IsAllowSelectParent)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void HandleCascaderViewItemSelected(object? sender, CascaderItemSelectedEventArgs eventArgs)
    {
        if (!IsMultiple)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            SetCurrentValue(SelectedItemProperty, eventArgs.Item);
        }
    }
    
    private void HandleSearchInputTextChanged(TextChangedEventArgs e)
    {
        if (Filter != null)
        {
            if (e.Source is TextBox textBox)
            {
                ActivateFilterValue = textBox.Text?.Trim();
            }
            ConfigurePlaceholderVisible();
        }

        e.Handled = true;
    }

    private void ConfigureMaxSelectReached()
    {
        if (IsMultiple)
        {
            IsMaxSelectReached = MaxCount <= CheckedItems?.Count;
        }
        else
        {
            IsMaxSelectReached = false;
        }
    }

    private void HandleIsCheckableChanged()
    {
        if (_cascaderView != null)
        {
            try
            {
                _needSkipSyncCheckedItems  = true;
                _cascaderView.SelectedItem = null;
                _cascaderView.CheckedItems = null;
            }
            finally
            {
                _needSkipSyncCheckedItems = false;
            }
        }
    }
    
    private void HandleTagCloseRequest(RoutedEventArgs e)
    {
        if (!IsMultiple)
        {
            return;
        }
        if (e.Source is SelectTag tag && tag.Item is ICascaderOption viewOption)
        {
            if (CheckedItems != null)
            {
                var checkedItems = new List<ICascaderOption>();
                foreach (var item in CheckedItems)
                {
                    checkedItems.Add(item);
                }
                RemoveItemRecursive(checkedItems, viewOption);
                CheckedItems = checkedItems;
            }
        }
        e.Handled = true;
    }
    
    private void RemoveItemRecursive(List<ICascaderOption> items, ICascaderOption item)
    {
        foreach (var child in item.Children)
        {
            RemoveItemRecursive(items, child);
        }
        items.Remove(item);
    }
    
    private void SyncCheckedItemsToCascaderView()
    {
        if (!_needSkipSyncCheckedItems)
        {
            if (_cascaderView != null)
            {
                if (CheckedItems != null && IsMultiple)
                {
                    if (_cascaderView.CheckedItems == null)
                    {
                        var checkedItems = new List<ICascaderOption>();
                        checkedItems.AddRange(CheckedItems);
                        _cascaderView.CheckedItems = checkedItems;
                    }
                    else
                    {
                        var cascaderViewSet  = _cascaderView.CheckedItems.ToHashSet();
                        var currentSet   = CheckedItems.ToHashSet();
                        var deletedItems = cascaderViewSet.Except(currentSet);
                        var addedItems   = currentSet.Except(cascaderViewSet);
                        
                        var currentCheckedItems = new List<ICascaderOption>();
                        currentCheckedItems.AddRange(_cascaderView.CheckedItems);
                        
                        foreach (var item in deletedItems)
                        {
                            currentCheckedItems.Remove(item);
                        }
        
                        foreach (var item in addedItems)
                        {
                            currentCheckedItems.Add(item);
                        }
                        _cascaderView.CheckedItems = currentCheckedItems;
                    }
                }
                else
                {
                    _cascaderView.SelectedItem = null;
                    _cascaderView.CheckedItems = null;
                }
            }
        }
    }
    
    private void BuildEffectiveSelectedItems()
    {
        if (CheckedItems != null)
        {
            if (ShowCheckedStrategy == TreeSelectCheckedStrategy.All)
            {
                EffectiveCheckedItems = CheckedItems;
            }
            else
            {
                var effectiveSelectedItems = new List<ICascaderOption>();
                if (ShowCheckedStrategy.HasFlag(TreeSelectCheckedStrategy.ShowParent))
                {
                    var selectedSet = CheckedItems.Cast<object>().ToHashSet();
                    var fullySelectedParents = CheckedItems.Cast<ICascaderOption>()
                                                            .Where(node => node.Children.All(child => selectedSet.Contains(child)))
                                                            .ToHashSet();
                    foreach (var option in CheckedItems)
                    {
                        bool isDescendantOfFullySelectedParent = fullySelectedParents
                            .Any(parent => IsDescendantOf(option, parent));
            
                        if (!isDescendantOfFullySelectedParent)
                        {
                            effectiveSelectedItems.Add(option);
                        }
                    }
                }
                if (ShowCheckedStrategy.HasFlag(TreeSelectCheckedStrategy.ShowChild))
                {
                    foreach (var option in CheckedItems)
                    {
                        if (option.Children.Count == 0)
                        {
                            effectiveSelectedItems.Add(option);
                        }
                    }
                }
                EffectiveCheckedItems = effectiveSelectedItems;
            }
        }
        else
        {
            EffectiveCheckedItems = null;
        }
    }
        
    private bool IsDescendantOf(ICascaderOption node, ICascaderOption parent)
    {
        if (node == parent)
        {
            return false;
        }
        ICascaderOption? current = node;
        while (current != null)
        {
            if (current == parent)
            {
                return true;
            }
            current = current.ParentNode as ICascaderOption;
        }
        return false;
    }

    private void ConfigureSelectedItemPath()
    {
        if (!IsMultiple)
        {
            if (SelectedItem == null)
            {
                SelectedItemPath = null;
            }
            else
            {
                var current = SelectedItem;
                var parts   = new List<string>();
                while (current != null)
                {
                    parts.Add(current.Header?.ToString() ?? string.Empty);
                    current = current.ParentNode as ICascaderOption;
                }

                parts.Reverse();
                SelectedItemPath = string.Join('/', parts);
            }

            if (_cascaderView != null)
            {
                _cascaderView.SelectedItem = SelectedItem;
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DefaultSelectItemPath != null && SelectedItemPath == null)
        {
            if (_cascaderView != null)
            {
                if (_cascaderView.TryParseSelectPath(DefaultSelectItemPath, out var nodes))
                {
                    var parts = new List<string>();
                    foreach (var node in nodes)
                    {
                        parts.Add(node.Header?.ToString() ?? string.Empty);
                    }
                    SetCurrentValue(SelectedItemPathProperty, string.Join("/", parts));
                }
            }
        }
    }
}