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
    
    public static readonly StyledProperty<IEnumerable<ICascaderOption>?> OptionsSourceProperty =
        CascaderView.OptionsSourceProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        CascaderView.OptionTemplateProperty.AddOwner<Cascader>();
    
    public static readonly DirectProperty<Cascader, ICascaderOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<Cascader, ICascaderOption?>(
            nameof(SelectedOption),
            o => o.SelectedOption,
            (o, v) => o.SelectedOption = v);
    
    public static readonly DirectProperty<CascaderView, IList<ICascaderOption>?> CheckedOptionsProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, IList<ICascaderOption>?>(
            nameof(CheckedOptions),
            o => o.CheckedOptions,
            (o, v) => o.CheckedOptions = v);
    
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
    
    public static readonly StyledProperty<TreeNodePath?> DefaultSelectOptionPathProperty =
        AvaloniaProperty.Register<Cascader, TreeNodePath?>(nameof(DefaultSelectOptionPath));
    
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
    
    private ICascaderOption? _selectedOption;
    
    public ICascaderOption? SelectedOption
    {
        get => _selectedOption;
        set => SetAndRaise(SelectedOptionProperty, ref _selectedOption, value);
    }
        
    private IList<ICascaderOption>? _checkedOptions;
    
    public IList<ICascaderOption>? CheckedOptions
    {
        get => _checkedOptions;
        set => SetAndRaise(CheckedOptionsProperty, ref _checkedOptions, value);
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
    
    public TreeNodePath? DefaultSelectOptionPath
    {
        get => GetValue(DefaultSelectOptionPathProperty);
        set => SetValue(DefaultSelectOptionPathProperty, value);
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
    
    internal static readonly DirectProperty<Cascader, IList<ICascaderOption>?> EffectiveCheckedOptionsProperty =
        AvaloniaProperty.RegisterDirect<Cascader, IList<ICascaderOption>?>(
            nameof(EffectiveCheckedOptions),
            o => o.EffectiveCheckedOptions,
            (o, v) => o.EffectiveCheckedOptions = v);
    
    internal static readonly DirectProperty<Cascader, string?> SelectedOptionPathProperty =
        AvaloniaProperty.RegisterDirect<Cascader, string?>(
            nameof(SelectedOptionPath),
            o => o.SelectedOptionPath,
            (o, v) => o.SelectedOptionPath = v);
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }
    
    private IList<ICascaderOption>? _effectiveCheckedOptions;

    internal IList<ICascaderOption>? EffectiveCheckedOptions
    {
        get => _effectiveCheckedOptions;
        set => SetAndRaise(EffectiveCheckedOptionsProperty, ref _effectiveCheckedOptions, value);
    }
    
    private string? _selectOptionPath;

    internal string? SelectedOptionPath
    {
        get => _selectOptionPath;
        set => SetAndRaise(SelectedOptionPathProperty, ref _selectOptionPath, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CascaderToken.ID;

    #endregion
    
    private readonly ItemCollection _options = new();
    private SelectFilterTextBox? _singleFilterInput;
    private CascaderView? _cascaderView;
    private bool _needSkipSyncCheckedOptions;

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
        _options.SetItemsSource(args.GetNewValue<IEnumerable<ICascaderOption>?>());
    }

    private void HandleCascaderOptionsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (_cascaderView != null)
        {
            _cascaderView.OptionsSource = Options.Cast<ICascaderOption>().ToList();
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
        SelectedOption     = null;
        SelectedOptionPath = null;
        CheckedOptions     = null;
    }
    
    private void ConfigurePlaceholderVisible()
    {
        if (IsMultiple)
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, (CheckedOptions == null || CheckedOptions?.Count == 0) && 
                                                              string.IsNullOrWhiteSpace(ActivateFilterValue) &&
                                                              string.IsNullOrWhiteSpace(SelectedOptionPath));
        }
        else
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, SelectedOption == null && 
                                                              string.IsNullOrWhiteSpace(ActivateFilterValue) &&
                                                              string.IsNullOrWhiteSpace(SelectedOptionPath));
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
        else if (change.Property == CheckedOptionsProperty ||
                 change.Property == SelectedOptionProperty ||
                 change.Property == SelectedOptionPathProperty ||
                 change.Property == IsMultipleProperty)
        {
            ConfigurePlaceholderVisible();
            ConfigureSelectionIsEmpty();
            if (IsMultiple)
            {
                SetCurrentValue(SelectedCountProperty, CheckedOptions?.Count ?? 0);
            }
            else
            {
                SetCurrentValue(SelectedCountProperty, SelectedOption != null ? 1 : 0);
            }
        }
        
        if (change.Property == CheckedOptionsProperty ||
            change.Property == IsMultipleProperty)
        {
            SyncCheckedOptionsToCascaderView();
        }

        if (change.Property == IsMultipleProperty ||
            change.Property == MaxCountProperty ||
            change.Property == EffectiveCheckedOptionsProperty ||
            change.Property == IsMultipleProperty)
        {
            ConfigureMaxSelectReached();
        }
        
        if (change.Property == CheckedOptionsProperty ||
            change.Property == ShowCheckedStrategyProperty)
        {
            BuildEffectiveSelectedOptions();
        }

        if (change.Property == SelectedOptionProperty)
        {
            ConfigureSelectedOptionPath();
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
            _cascaderView.CheckedOptionsChanged -= HandleCascaderViewItemsCheckedChanged;
            _cascaderView.ItemDoubleClicked     -= HandleCascaderViewItemDoubleClicked;
            _cascaderView.ItemClicked           -= HandleCascaderViewItemClicked;
            _cascaderView.OptionSelected        -= HandleCascaderViewItemSelected;
            _cascaderView.OptionsSource         =  null;
        }
        
        _singleFilterInput = e.NameScope.Find<SelectFilterTextBox>(CascaderThemeConstants.SingleFilterInputPart);
        Popup              = e.NameScope.Find<Popup>(CascaderThemeConstants.PopupPart);
        _cascaderView      = e.NameScope.Find<CascaderView>(CascaderThemeConstants.CascaderViewPart);
        
        if (_cascaderView != null)
        {
            _cascaderView.CheckedOptionsChanged += HandleCascaderViewItemsCheckedChanged;
            _cascaderView.ItemDoubleClicked     += HandleCascaderViewItemDoubleClicked;
            _cascaderView.ItemClicked           += HandleCascaderViewItemClicked;
            _cascaderView.OptionSelected        += HandleCascaderViewItemSelected;
            _cascaderView.OptionsSource         =  Options.Cast<ICascaderOption>().ToList();
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

    private void HandleCascaderViewItemsCheckedChanged(object? sender, CascaderViewCheckedOptionsChangedEventArgs e)
    {
        if (_cascaderView == null)
        {
            return;
        }

        var                       needSync        = false;
        HashSet<ICascaderOption>? cascaderViewSet = null;
        if (_checkedOptions == null || _checkedOptions?.Count != _cascaderView.CheckedOptions?.Count)
        {
            needSync = true;
        }
        else
        {
            var currentSet = _checkedOptions?.Cast<ICascaderOption>().ToHashSet() ?? new HashSet<ICascaderOption>();
            cascaderViewSet = _cascaderView?.CheckedOptions?.Cast<ICascaderOption>().ToHashSet() ?? new HashSet<ICascaderOption>();
            if (!currentSet.SetEquals(cascaderViewSet))
            {
                needSync = true;
            }
        }
        
        if (needSync)
        {
            try
            {
                _needSkipSyncCheckedOptions =   true;
                cascaderViewSet             ??= _cascaderView?.CheckedOptions?.Cast<ICascaderOption>().ToHashSet();
                if (cascaderViewSet != null)
                {
                    CheckedOptions = cascaderViewSet.ToList();
                }
           
            }
            finally
            {
                _needSkipSyncCheckedOptions = false;
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
            SetCurrentValue(IsSelectionEmptyProperty, CheckedOptions == null || CheckedOptions?.Count == 0);
        }
        else
        {
            SetCurrentValue(IsSelectionEmptyProperty, SelectedOption == null);
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

    private void HandleCascaderViewItemClicked(object? sender, CascaderOptionSelectedEventArgs eventArgs)
    {
        if (!IsMultiple)
        {
            var option = eventArgs.Option;
            if ((IsAllowSelectParent && option.IsEffectiveLeaf()) || !IsAllowSelectParent)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
            SetCurrentValue(SelectedOptionProperty, option);
        }
    }

    private void HandleCascaderViewItemSelected(object? sender, CascaderOptionSelectedEventArgs eventArgs)
    {
        if (!IsMultiple)
        {
            var option = eventArgs.Option;
            SetCurrentValue(SelectedOptionProperty, option);
        }
    }
    
    private void HandleCascaderViewItemClicked(object? sender, CascaderItemClickedEventArgs eventArgs)
    {
        var option = eventArgs.Item.AttachedOption;
        if (option != null)
        {
            if (!IsMultiple)
            {
                if (option.IsEffectiveLeaf())
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
            }
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
            IsMaxSelectReached = MaxCount <= CheckedOptions?.Count;
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
                _needSkipSyncCheckedOptions  = true;
                _cascaderView.SelectedOption = null;
                _cascaderView.CheckedOptions = null;
            }
            finally
            {
                _needSkipSyncCheckedOptions = false;
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
            if (CheckedOptions != null)
            {
                var checkedOptions = new List<ICascaderOption>();
                foreach (var option in CheckedOptions)
                {
                    checkedOptions.Add(option);
                }
                RemoveOptionRecursive(checkedOptions, viewOption);
                CheckedOptions = checkedOptions;
            }
        }
        e.Handled = true;
    }
    
    private void RemoveOptionRecursive(List<ICascaderOption> options, ICascaderOption option)
    {
        foreach (var child in option.Children)
        {
            RemoveOptionRecursive(options, child);
        }
        options.Remove(option);
    }
    
    private void SyncCheckedOptionsToCascaderView()
    {
        if (!_needSkipSyncCheckedOptions)
        {
            if (_cascaderView != null)
            {
                if (CheckedOptions != null && IsMultiple)
                {
                    if (_cascaderView.CheckedOptions == null)
                    {
                        var checkedOptions = new List<ICascaderOption>();
                        checkedOptions.AddRange(CheckedOptions);
                        _cascaderView.CheckedOptions = checkedOptions;
                    }
                    else
                    {
                        var cascaderViewSet = _cascaderView.CheckedOptions.ToHashSet();
                        var currentSet      = CheckedOptions.ToHashSet();
                        var deletedOptions  = cascaderViewSet.Except(currentSet);
                        var addedOptions    = currentSet.Except(cascaderViewSet);
                        
                        var currentCheckedOptions = new List<ICascaderOption>();
                        currentCheckedOptions.AddRange(_cascaderView.CheckedOptions);
                        
                        foreach (var option in deletedOptions)
                        {
                            currentCheckedOptions.Remove(option);
                        }
        
                        foreach (var option in addedOptions)
                        {
                            currentCheckedOptions.Add(option);
                        }
                        _cascaderView.CheckedOptions = currentCheckedOptions;
                    }
                }
                else
                {
                    _cascaderView.SelectedOption = null;
                    _cascaderView.CheckedOptions = null;
                }
            }
        }
    }
    
    private void BuildEffectiveSelectedOptions()
    {
        if (CheckedOptions != null)
        {
            if (ShowCheckedStrategy == TreeSelectCheckedStrategy.All)
            {
                EffectiveCheckedOptions = CheckedOptions;
            }
            else
            {
                var effectiveSelectedOptions = new List<ICascaderOption>();
                if (ShowCheckedStrategy.HasFlag(TreeSelectCheckedStrategy.ShowParent))
                {
                    var selectedSet = CheckedOptions.Cast<object>().ToHashSet();
                    var fullySelectedParents = CheckedOptions
                                               .Where(node => node.Children.All(child => selectedSet.Contains(child)))
                                               .ToHashSet();
                    foreach (var option in CheckedOptions)
                    {
                        bool isDescendantOfFullySelectedParent = fullySelectedParents
                            .Any(parent => IsDescendantOf(option, parent));
            
                        if (!isDescendantOfFullySelectedParent)
                        {
                            effectiveSelectedOptions.Add(option);
                        }
                    }
                }
                if (ShowCheckedStrategy.HasFlag(TreeSelectCheckedStrategy.ShowChild))
                {
                    foreach (var option in CheckedOptions)
                    {
                        if (option.Children.Count == 0)
                        {
                            effectiveSelectedOptions.Add(option);
                        }
                    }
                }
                EffectiveCheckedOptions = effectiveSelectedOptions;
            }
        }
        else
        {
            EffectiveCheckedOptions = null;
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

    private void ConfigureSelectedOptionPath()
    {
        if (!IsMultiple)
        {
            if (SelectedOption == null)
            {
                SelectedOptionPath = null;
            }
            else
            {
                var current = SelectedOption;
                var parts   = new List<string>();
                while (current != null)
                {
                    parts.Add(current.Header?.ToString() ?? string.Empty);
                    current = current.ParentNode as ICascaderOption;
                }

                parts.Reverse();
                SelectedOptionPath = string.Join('/', parts);
            }

            if (_cascaderView != null)
            {
                _cascaderView.SelectedOption = SelectedOption;
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DefaultSelectOptionPath != null && SelectedOptionPath == null)
        {
            if (_cascaderView != null)
            {
                if (_cascaderView.TryParseSelectPath(DefaultSelectOptionPath, out var options))
                {
                    var parts = new List<string>();
                    foreach (var option in options)
                    {
                        parts.Add(option.Header?.ToString() ?? string.Empty);
                    }
                    SetCurrentValue(SelectedOptionPathProperty, string.Join("/", parts));
                }
            }
        }
    }
}