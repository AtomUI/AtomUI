using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Desktop.Controls.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum SelectMode
{
    Single,
    Multiple,
    Tags
}

[PseudoClasses(SelectPseudoClass.DropdownOpen)]
public class Select : AbstractSelect, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<IEnumerable<ISelectOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<Select, IEnumerable<ISelectOption>?>(nameof(OptionsSource));
    
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<Select, IDataTemplate?>(nameof(OptionTemplate));
    
    public static readonly StyledProperty<bool> IsDefaultActiveFirstOptionProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsDefaultActiveFirstOption));
    
    public static readonly StyledProperty<bool> IsGroupEnabledProperty =
        List.IsGroupEnabledProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<string> GroupPropertyPathProperty =
        List.GroupPropertyPathProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<bool> IsHideSelectedOptionsProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsHideSelectedOptions));

    public static readonly StyledProperty<SelectMode> ModeProperty =
        AvaloniaProperty.Register<Select, SelectMode>(nameof(Mode));
    
    public static readonly DirectProperty<Select, IList<ISelectOption>?> SelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<Select, IList<ISelectOption>?>(
            nameof(SelectedOptions),
            o => o.SelectedOptions,
            (o, v) => o.SelectedOptions = v);
    
    public static readonly StyledProperty<double> OptionFontSizeProperty =
        AvaloniaProperty.Register<Select, double>(nameof(OptionFontSize));
    
    public static readonly StyledProperty<bool> AutoScrollToSelectedOptionsProperty =
        AvaloniaProperty.Register<Select, bool>(
            nameof(AutoScrollToSelectedOptions),
            defaultValue: false);
    
    public static readonly StyledProperty<string> OptionFilterPropProperty =
        AvaloniaProperty.Register<Select, string>(
            nameof(OptionFilterProp), "Value");
    
    public IEnumerable<ISelectOption>? OptionsSource
    {
        get => GetValue(OptionsSourceProperty);
        set => SetValue(OptionsSourceProperty, value);
    }
    
    [InheritDataTypeFromItems(nameof(OptionsSource))]
    public IDataTemplate? OptionTemplate
    {
        get => GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }
    
    public bool IsDefaultActiveFirstOption
    {
        get => GetValue(IsDefaultActiveFirstOptionProperty);
        set => SetValue(IsDefaultActiveFirstOptionProperty, value);
    }
    
    public bool IsGroupEnabled
    {
        get => GetValue(IsGroupEnabledProperty);
        set => SetValue(IsGroupEnabledProperty, value);
    }
    
    public string GroupPropertyPath
    {
        get => GetValue(GroupPropertyPathProperty);
        set => SetValue(GroupPropertyPathProperty, value);
    }
    
    public bool IsHideSelectedOptions
    {
        get => GetValue(IsHideSelectedOptionsProperty);
        set => SetValue(IsHideSelectedOptionsProperty, value);
    }
    
    public SelectMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }
    
    public string OptionFilterProp
    {
        get => GetValue(OptionFilterPropProperty);
        set => SetValue(OptionFilterPropProperty, value);
    }
    
    [Content]
    public AvaloniaList<ISelectOption> Options { get; set; } = new();
    
    private IList<ISelectOption>? _selectedOptions;

    public IList<ISelectOption>? SelectedOptions
    {
        get => _selectedOptions;
        set => SetAndRaise(SelectedOptionsProperty, ref _selectedOptions, value);
    }

    public double OptionFontSize
    {
        get => GetValue(OptionFontSizeProperty);
        set => SetValue(OptionFontSizeProperty, value);
    }
    
    public bool AutoScrollToSelectedOptions
    {
        get => GetValue(AutoScrollToSelectedOptionsProperty);
        set => SetValue(AutoScrollToSelectedOptionsProperty, value);
    }
    #endregion
    
    public Func<object, object, bool>? FilterFn { get; set; }
    
    public Func<object, ISelectOption, bool>? DefaultValueCompareFn { get; set; }
    public IList<object>? DefaultValues { get; set; }

    #region 内部属性定义
    
    internal static readonly DirectProperty<Select, ISelectOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<Select, ISelectOption?>(
            nameof(SelectedOption),
            o => o.SelectedOption,
            (o, v) => o.SelectedOption = v);
    
    internal static readonly DirectProperty<Select, bool> IsEffectiveFilterEnabledProperty =
        AvaloniaProperty.RegisterDirect<Select, bool>(nameof(IsEffectiveFilterEnabled),
            o => o.IsEffectiveFilterEnabled,
            (o, v) => o.IsEffectiveFilterEnabled = v);
    
    private ISelectOption? _selectedOption;

    internal ISelectOption? SelectedOption
    {
        get => _selectedOption;
        set => SetAndRaise(SelectedOptionProperty, ref _selectedOption, value);
    }
    
    private bool _isEffectiveFilterEnabled;

    internal bool IsEffectiveFilterEnabled
    {
        get => _isEffectiveFilterEnabled;
        set => SetAndRaise(IsEffectiveFilterEnabledProperty, ref _isEffectiveFilterEnabled, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SelectToken.ID;

    #endregion
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());
    
    private Popup? _popup;
    private SelectOptionList? _optionsBox;
    private SelectFilterTextBox? _singleFilterInput;
    private INotifyCollectionChanged? _selectedOptionsNotifier;
    private INotifyCollectionChanged? _optionsSourceNotifier;
    private bool _selectedOptionsRefreshScheduled;
    private bool _isReconcilingSelectedOptions;
    private const string NullValueKey = "\0";
    private readonly Dictionary<string, List<ISelectOption>> _optionsByValue = new();
    private readonly CompositeDisposable _subscriptionsOnOpen = new ();
    private ListFilterDescription? _filterDescription;
    private ListFilterDescription? _filterSelectedDescription;

    private ISelectOption? _addNewOption;

    static Select()
    {
        FocusableProperty.OverrideDefaultValue<Select>(true);
        SelectHandle.ClearRequestedEvent.AddClassHandler<Select>((target, args) =>
        {
            target.HandleClearRequest();
        });
        OptionsSourceProperty.Changed.AddClassHandler<Select>((x, e) => x.HandleOptionsSourcePropertyChanged(e));
        SelectFilterTextBox.TextChangedEvent.AddClassHandler<Select>((x, e) => x.HandleSearchInputTextChanged(e));
        SelectFilterTextBox.KeyDownEvent.AddClassHandler<Select>(
            (x, e) => x.HandleSearchInputKeyDown(e),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        SelectResultOptionsBox.KeyDownEvent.AddClassHandler<Select>(
            (x, e) => x.HandleSearchInputKeyDown(e),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        SelectTag.ClosedEvent.AddClassHandler<Select>((x, e) => x.HandleTagCloseRequest(e));
    }
    
    public Select()
    {
        this.RegisterResources();
    }

    private void HandleClearRequest()
    {
        Clear();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureDefaultValues();
    }

    private bool OptionEqualByValue(object value, ISelectOption selectOption)
    {
        if (DefaultValueCompareFn != null)
        {
            return DefaultValueCompareFn(value, selectOption);
        }
        var strValue = value.ToString();
        var optValue = selectOption.Value?.ToString();
        return strValue == optValue;
    }

    private bool TryHandleDeleteKey(KeyEventArgs e)
    {
        if (Mode == SelectMode.Single || SelectedOptions == null || SelectedOptions.Count == 0)
        {
            return false;
        }

        if (e.Key != Key.Back && e.Key != Key.Delete)
        {
            return false;
        }

        if (e.Source is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text) == false)
        {
            return false;
        }

        var newSelection = new List<ISelectOption>();
        foreach (var selectedItem in SelectedOptions)
        {
            newSelection.Add(selectedItem);
        }

        if (newSelection.Count == 0)
        {
            return false;
        }

        var lastIndex   = newSelection.Count - 1;
        var removedItem = newSelection[lastIndex];
        newSelection.RemoveAt(lastIndex);
        SetSelectedOptionsInternal(newSelection);
        SyncSelection();

        if (Mode == SelectMode.Tags && removedItem.IsDynamicAdded)
        {
            Options.Remove(removedItem);
            RemoveOptionFromIndex(removedItem);
            if (ReferenceEquals(_addNewOption, removedItem))
            {
                _addNewOption = null;
            }
        }

        e.Handled = true;
        return true;
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if (TryHandleDeleteKey(e))
        {
            return;
        }

        if ((e.Key == Key.F4 && e.KeyModifiers.HasAllFlags(KeyModifiers.Alt) == false) ||
            ((e.Key == Key.Down || e.Key == Key.Up) && e.KeyModifiers.HasAllFlags(KeyModifiers.Alt)))
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && (e.Key == Key.Down || e.Key == Key.Up))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (IsDropDownOpen)
        {
            if (e.Key == Key.Down)
            {
                _optionsBox?.MoveActiveBy(1);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                _optionsBox?.MoveActiveBy(-1);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                _optionsBox?.CommitActiveSelection();
                if (Mode == SelectMode.Single)
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
                e.Handled = true;
            }
        }
    }

    private void HandleSearchInputKeyDown(KeyEventArgs e)
    {
        if (TryHandleDeleteKey(e))
        {
            return;
        }

        if (e.Handled)
        {
            return;
        }

        if (!IsDropDownOpen)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
                e.Handled = true;
            }
            return;
        }

        if (e.Key == Key.Down)
        {
            _optionsBox?.MoveActiveBy(1);
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            _optionsBox?.MoveActiveBy(-1);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            _optionsBox?.CommitActiveSelection();
            if (Mode == SelectMode.Single)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (e.Key == Key.Tab)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if(!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true)
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
                var parent = sourceControl.FindAncestorOfType<IconButton>();
                var tag    = parent?.FindAncestorOfType<SelectTag>();
                if (tag != null)
                {
                    IgnorePopupClose = true;
                }
            }
            else if (!IsHideSelectedOptions)
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
            if (_popup?.IsInsidePopup(source) == true)
            {
                if (Mode == SelectMode.Single)
                {
                    var optionItem = source.FindAncestorOfType<SelectOptionItem>();
                    if (optionItem != null && optionItem.IsEnabled)
                    {
                        SetCurrentValue(IsDropDownOpenProperty, false);
                    }
                }
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
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_popup != null)
        {
            _popup.Opened -= PopupOpened;
            _popup.Closed -= PopupClosed;
        }
        _optionsBox        = e.NameScope.Get<SelectOptionList>(SelectThemeConstants.OptionsBoxPart);
        _singleFilterInput = e.NameScope.Get<SelectFilterTextBox>(SelectThemeConstants.SingleFilterInputPart);
        if (_optionsBox != null)
        {
            _optionsBox.Select = this;
            ConfigureOptionsBoxSelectionMode();
        }
        
        _popup                    =  e.NameScope.Get<Popup>(SelectThemeConstants.PopupPart);
        _popup.ClickHidePredicate =  PopupClosePredicate;
        _popup.Opened             += PopupOpened;
        _popup.Closed             += PopupClosed;
        ConfigurePlaceholderVisible();
        ConfigureSelectionIsEmpty();
        UpdatePseudoClasses();
        ConfigureSingleFilterTextBox();
        ConfigureEffectiveSearchEnabled();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeSelectedOptions(SelectedOptions);
        SubscribeOptionsSource(OptionsSource as IEnumerable<ISelectOption>);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_selectedOptionsNotifier != null)
        {
            _selectedOptionsNotifier.CollectionChanged -= HandleSelectedOptionsCollectionChanged;
            _selectedOptionsNotifier = null;
        }
        if (_optionsSourceNotifier != null)
        {
            _optionsSourceNotifier.CollectionChanged -= HandleOptionsSourceCollectionChanged;
            _optionsSourceNotifier = null;
        }
    }

    internal void NotifyLogicalSelectOption(ISelectOption selectOption)
    {
        Debug.Assert(_optionsBox != null);
        var selectedOptions = new List<ISelectOption>();
        if (Mode == SelectMode.Single)
        {
            if (_singleFilterInput != null)
            {
                _singleFilterInput.Width = double.NaN;
            }
        
            selectedOptions.Add(selectOption);
        }
        else
        {
            if (Mode == SelectMode.Tags)
            {
                _addNewOption = null;
            }
            if (SelectedOptions != null)
            {
                foreach (var item in SelectedOptions)
                {
                    selectedOptions.Add(item);
                }
            }
            
            if (!selectedOptions.Contains(selectOption))
            {
                selectedOptions.Add(selectOption);
            }
            else if (selectedOptions.Contains(selectOption))
            {
                selectedOptions.Remove(selectOption);
            }
        }
        SetSelectedOptionsInternal(selectedOptions);
        SyncSelection();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty)
        {
            UpdatePseudoClasses();
            ConfigureSingleFilterTextBox();
        }
        else if (change.Property == IsPopupMatchSelectWidthProperty)
        {
            ConfigurePopupMinWith(DesiredSize.Width);
        }
        else if (change.Property == StyleVariantProperty ||
                 change.Property == StatusProperty)
        {
            UpdatePseudoClasses();
        }
        if (change.Property == SelectedOptionsProperty)
        {
            SubscribeSelectedOptions(SelectedOptions);
            ConfigureSingleSelectedOption();
            ConfigureSelectionIsEmpty();
            ConfigurePlaceholderVisible();
            ConfigureSelectedFilterDescription();
            SetCurrentValue(SelectedCountProperty, SelectedOptions?.Count ?? 0);
            CleanDynamicAddedOptions();
        }
        else if (change.Property == OptionFilterPropProperty)
        {
            HandleOptionFilterPropChanged();
        }
        else if (change.Property == ModeProperty)
        {
            ConfigureSingleSelectedOption();
            ConfigureOptionsBoxSelectionMode();
        }
        else if (change.Property == IsHideSelectedOptionsProperty)
        {
            if (!IsHideSelectedOptions)
            {
                _filterSelectedDescription = null;
            }
        }
        
        if (change.Property == IsFilterEnabledProperty ||
            change.Property == ModeProperty)
        {
            ConfigureEffectiveSearchEnabled();
        }
    }
    
    private void PopupClosed(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen.Clear();
        NotifyPopupClosed();
        if (Mode == SelectMode.Single)
        {
            if (_singleFilterInput != null)
            {
                _singleFilterInput.Clear();
                _singleFilterInput.Width = double.NaN;
            }
        }
    }

    private void PopupOpened(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen.Clear();
        this.GetObservable(IsVisibleProperty).Subscribe(IsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        foreach (var parent in this.GetVisualAncestors().OfType<Control>())
        {
            parent.GetObservable(IsVisibleProperty).Subscribe(IsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        }

        NotifyPopupOpened();
        if (Mode == SelectMode.Single)
        {
            _singleFilterInput?.Focus();
        }

        SyncSelection();
    }

    private void SyncSelection()
    {
        if (_optionsBox != null)
        {
            var selectedItems = new List<object>();
            if (Mode == SelectMode.Single)
            {
                if (SelectedOptions != null)
                {
                    foreach (var option in SelectedOptions)
                    {
                        selectedItems.Add(option);
                        break;
                    }
                }
            }
            else
            {
                if (SelectedOptions != null)
                {
                    foreach (var option in SelectedOptions)
                    {
                        selectedItems.Add(option);
                    }
                }
            }
            _optionsBox.SetCurrentValue(SelectOptionList.SelectedItemsProperty, selectedItems);
            _optionsBox.EnsureActiveOption();
        }
    }

    private void ConfigureOptionsBoxSelectionMode()
    {
        if (_optionsBox == null)
        {
            return;
        }

        _optionsBox.SetCurrentValue(List.SelectionModeProperty,
            Mode == SelectMode.Single ? SelectionMode.Single : SelectionMode.Multiple);
    }
    
    private void IsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }
    
    public void Clear()
    {
        ClearSelectedOptions();
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(SelectPseudoClass.DropdownOpen, IsDropDownOpen);
        PseudoClasses.Set(StdPseudoClass.Error, Status == AddOnDecoratedStatus.Error);
        PseudoClasses.Set(StdPseudoClass.Warning, Status == AddOnDecoratedStatus.Warning);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Outline, StyleVariant == AddOnDecoratedVariant.Outline);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Filled, StyleVariant == AddOnDecoratedVariant.Filled);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Borderless, StyleVariant == AddOnDecoratedVariant.Borderless);
    }

    private void ConfigurePlaceholderVisible()
    {
        SetCurrentValue(IsPlaceholderTextVisibleProperty, (SelectedOptions == null || SelectedOptions?.Count == 0) && string.IsNullOrEmpty(ActivateFilterValue));
    }

    private void ConfigureSelectionIsEmpty()
    {
        SetCurrentValue(IsSelectionEmptyProperty, SelectedOptions == null || SelectedOptions?.Count == 0);
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

    private void HandleSearchInputTextChanged(TextChangedEventArgs e)
    {
        if (_optionsBox != null && _optionsBox.FilterDescriptions != null)
        {
            if (e.Source is TextBox textBox)
            {
                ActivateFilterValue = textBox.Text?.Trim();
            }

            if (_addNewOption != null)
            {
                var isSelected = SelectedOptions?.Contains(_addNewOption) == true;
                var isCurrentInput = ActivateFilterValue == _addNewOption.Header?.ToString();
                if (!isSelected && !isCurrentInput)
                {
                    Options.Remove(_addNewOption);
                    RemoveOptionFromIndex(_addNewOption);
                    _addNewOption = null;
                }
            }
            ConfigurePlaceholderVisible();
            if (string.IsNullOrEmpty(ActivateFilterValue))
            {
                _optionsBox.FilterDescriptions.Clear();
                _filterDescription = null;
            }
            else
            {
                if (_filterDescription != null)
                {
                    var oldFilter = _filterDescription;
                    Debug.Assert(oldFilter.FilterConditions.Count == 1);
                    var oldFilterValue = oldFilter.FilterConditions.First().ToString();
                    if (oldFilterValue != ActivateFilterValue)
                    {
                        _filterDescription = new ListFilterDescription()
                        {
                            PropertyPath     = _filterDescription.PropertyPath,
                            Filter           =  _filterDescription.Filter,
                            FilterConditions = [ActivateFilterValue]
                        };
                        _optionsBox.FilterDescriptions.Remove(oldFilter);
                        _optionsBox.FilterDescriptions.Add(_filterDescription);
                    }
                }
                else
                {
                    _filterDescription = new ListFilterDescription()
                    {
                        PropertyPath     = OptionFilterProp,
                        Filter           = FilterFn,
                        FilterConditions = [ActivateFilterValue],
                    };
                    _optionsBox.FilterDescriptions.Add(_filterDescription);
                }
            }

            // Only allow "create from search text" in Tags mode.
            // For Single/Multiple, when data is empty (or filter results are empty),
            // the dropdown should show the empty indicator instead of adding a temporary option.
            if (Mode == SelectMode.Tags &&
                _optionsBox.CollectionView?.Count == 0 &&
                !string.IsNullOrWhiteSpace(ActivateFilterValue))
            {
                _addNewOption = new SelectOption()
                {
                    Header = ActivateFilterValue,
                    Value  = ActivateFilterValue,
                    IsDynamicAdded = true
                };
                Options.Add(_addNewOption);
                AddOptionToIndex(_addNewOption);
            }
            SyncSelection();
        }
        e.Handled = true;
    }

    private void HandleTagCloseRequest(RoutedEventArgs e)
    {
        if (Mode == SelectMode.Single)
        {
            return;
        }
        if (e.Source is SelectTag tag && tag.Item is  ISelectOption tagOption)
        {
            if (SelectedOptions != null)
            {
                var selectedOptions = new List<ISelectOption>();
                foreach (var selectedItem in SelectedOptions)
                {
                    selectedOptions.Add(selectedItem);
                }
                selectedOptions.Remove(tagOption);
                SetSelectedOptionsInternal(selectedOptions);
                SyncSelection();
            }

            if (Mode == SelectMode.Tags)
            {
                if (tag.Item is ISelectOption selectOption && selectOption.IsDynamicAdded)
                {
                    Options.Remove(selectOption);
                    RemoveOptionFromIndex(selectOption);
                }
          
            }
        }
        e.Handled = true;
    }

    private void HandleOptionFilterPropChanged()
    {
        if (_filterDescription != null && _optionsBox!= null && _optionsBox.FilterDescriptions != null)
        {
            var oldFilter = _filterDescription;
            _filterDescription = new ListFilterDescription()
            {
                PropertyPath     = OptionFilterProp,
                Filter           =  oldFilter.Filter,
                FilterConditions = oldFilter.FilterConditions
            };
            _optionsBox.FilterDescriptions.Remove(oldFilter);
            _optionsBox.FilterDescriptions.Add(_filterDescription);
        }
    }
    
    private void HandleOptionsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newItemsSource = (IEnumerable<ISelectOption>?)change.NewValue;
        SubscribeOptionsSource(newItemsSource);
        ReloadOptionsFromSource(newItemsSource);
        var selectionChanged = ReconcileSelectedOptionsWithOptionsSource();
        selectionChanged |= ConfigureDefaultValues();
        if (selectionChanged)
        {
            RefreshSelectedOptions();
        }
        else
        {
            SyncSelection();
        }
    }

    private void SubscribeOptionsSource(IEnumerable<ISelectOption>? optionsSource)
    {
        if (_optionsSourceNotifier != null)
        {
            _optionsSourceNotifier.CollectionChanged -= HandleOptionsSourceCollectionChanged;
            _optionsSourceNotifier = null;
        }

        _optionsSourceNotifier = optionsSource as INotifyCollectionChanged;
        if (_optionsSourceNotifier != null)
        {
            _optionsSourceNotifier.CollectionChanged += HandleOptionsSourceCollectionChanged;
        }
    }

    private void HandleOptionsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ApplyOptionsSourceChange(e);
        var selectionChanged = false;
        if (e.Action == NotifyCollectionChangedAction.Replace)
        {
            selectionChanged |= ReplaceSelectedOptionsFromItems(e.OldItems, e.NewItems);
        }

        if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Replace ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            selectionChanged |= ReconcileSelectedOptionsWithOptionsSource();
        }
        selectionChanged |= ConfigureDefaultValues();

        if (selectionChanged)
        {
            RefreshSelectedOptions();
        }
        else
        {
            SyncSelection();
        }
    }

    private void ReloadOptionsFromSource(IEnumerable<ISelectOption>? optionsSource)
    {
        Options.Clear();
        if (optionsSource != null)
        {
            Options.AddRange(optionsSource);
        }
        RebuildOptionsIndex();
    }

    private void ApplyOptionsSourceChange(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                InsertOptions(e.NewItems, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveOptions(e.OldItems, e.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                ReplaceOptions(e.NewItems, e.OldItems, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Move:
                MoveOptions(e.NewItems, e.OldStartingIndex, e.NewStartingIndex);
                break;
            default:
                ReloadOptionsFromSource(OptionsSource as IEnumerable<ISelectOption>);
                break;
        }
    }

    private void InsertOptions(IList? newItems, int startIndex)
    {
        var items = ExtractOptions(newItems);
        if (items.Count == 0)
        {
            return;
        }

        var insertIndex = startIndex;
        if (insertIndex < 0 || insertIndex > Options.Count)
        {
            insertIndex = Options.Count;
        }

        for (var i = 0; i < items.Count; i++)
        {
            Options.Insert(insertIndex + i, items[i]);
            AddOptionToIndex(items[i]);
        }
    }

    private void RemoveOptions(IList? oldItems, int startIndex)
    {
        var items = ExtractOptions(oldItems);
        if (items.Count == 0)
        {
            return;
        }

        if (startIndex >= 0 && startIndex + items.Count <= Options.Count)
        {
            for (var i = 0; i < items.Count; i++)
            {
                Options.RemoveAt(startIndex);
                RemoveOptionFromIndex(items[i]);
            }
            return;
        }

        foreach (var item in items)
        {
            Options.Remove(item);
            RemoveOptionFromIndex(item);
        }
    }

    private void ReplaceOptions(IList? newItems, IList? oldItems, int startIndex)
    {
        var items = ExtractOptions(newItems);
        var old = ExtractOptions(oldItems);
        foreach (var removed in old)
        {
            RemoveOptionFromIndex(removed);
        }
        if (items.Count == 0)
        {
            RemoveOptions(oldItems, startIndex);
            return;
        }

        if (startIndex >= 0 && startIndex + items.Count <= Options.Count)
        {
            for (var i = 0; i < items.Count; i++)
            {
                Options[startIndex + i] = items[i];
                AddOptionToIndex(items[i]);
            }
            return;
        }

        RemoveOptions(oldItems, -1);
        Options.AddRange(items);
        foreach (var item in items)
        {
            AddOptionToIndex(item);
        }
    }

    private void MoveOptions(IList? movedItems, int oldIndex, int newIndex)
    {
        var items = ExtractOptions(movedItems);
        if (items.Count == 0)
        {
            return;
        }

        if (oldIndex < 0 || newIndex < 0)
        {
            ReloadOptionsFromSource(OptionsSource as IEnumerable<ISelectOption>);
            return;
        }

        for (var i = 0; i < items.Count; i++)
        {
            Options.RemoveAt(oldIndex);
        }

        if (oldIndex < newIndex)
        {
            newIndex -= items.Count;
        }

        for (var i = 0; i < items.Count; i++)
        {
            Options.Insert(newIndex + i, items[i]);
        }
    }

    private List<ISelectOption> ExtractOptions(IList? items)
    {
        var result = new List<ISelectOption>();
        if (items == null)
        {
            return result;
        }

        foreach (var item in items)
        {
            if (item is ISelectOption option)
            {
                result.Add(option);
            }
        }

        return result;
    }

    private bool ReplaceSelectedOptionsFromItems(IList? oldItems, IList? newItems)
    {
        if (SelectedOptions is not IList<ISelectOption> selection || selection.IsReadOnly)
        {
            return false;
        }

        if (SelectedOptions is not INotifyCollectionChanged)
        {
            return false;
        }

        if (oldItems == null || newItems == null || oldItems.Count == 0)
        {
            return false;
        }

        var changed = false;
        var count   = Math.Min(oldItems.Count, newItems.Count);
        for (var i = 0; i < count; i++)
        {
            if (oldItems[i] is not ISelectOption oldOption ||
                newItems[i] is not ISelectOption newOption)
            {
                continue;
            }

            for (var j = 0; j < selection.Count; j++)
            {
                if (ReferenceEquals(selection[j], oldOption))
                {
                    selection[j] = newOption;
                    changed = true;
                }
            }
        }

        return changed;
    }

    private bool ReconcileSelectedOptionsWithOptionsSource()
    {
        if (_isReconcilingSelectedOptions || SelectedOptions == null)
        {
            return false;
        }

        var updatedSelection = new List<ISelectOption>();
        foreach (var selected in SelectedOptions)
        {
            var match = FindMatchingOption(selected);
            if (match != null)
            {
                updatedSelection.Add(match);
            }
        }

        if (Mode == SelectMode.Single && updatedSelection.Count > 1)
        {
            updatedSelection = [updatedSelection[0]];
        }

        if (SelectionsEquivalent(SelectedOptions, updatedSelection))
        {
            return false;
        }

        _isReconcilingSelectedOptions = true;
        try
        {
            if (SelectedOptions is IList<ISelectOption> list && !list.IsReadOnly)
            {
                list.Clear();
                foreach (var option in updatedSelection)
                {
                    list.Add(option);
                }
            }
            else
            {
                SetCurrentValue(SelectedOptionsProperty, updatedSelection);
            }
        }
        finally
        {
            _isReconcilingSelectedOptions = false;
        }
        return true;
    }

    private ISelectOption? FindMatchingOption(ISelectOption selectedOption)
    {
        if (selectedOption.Value == null)
        {
            foreach (var option in Options)
            {
                if (ReferenceEquals(option, selectedOption))
                {
                    return option;
                }
            }
            return null;
        }

        var compareValue = selectedOption.Value;
        if (DefaultValueCompareFn == null)
        {
            var key = GetOptionKey(compareValue);
            if (_optionsByValue.TryGetValue(key, out var list) && list.Count > 0)
            {
                foreach (var option in list)
                {
                    if (ReferenceEquals(option, selectedOption))
                    {
                        return option;
                    }
                }
                return list[0];
            }
            return null;
        }

        foreach (var option in Options)
        {
            if (OptionEqualByValue(compareValue, option))
            {
                return option;
            }
        }
        return null;
    }

    private static bool SelectionsEquivalent(IList<ISelectOption> current, List<ISelectOption> updated)
    {
        if (current.Count != updated.Count)
        {
            return false;
        }

        for (var i = 0; i < current.Count; i++)
        {
            if (!ReferenceEquals(current[i], updated[i]))
            {
                return false;
            }
        }

        return true;
    }

    private void RebuildOptionsIndex()
    {
        _optionsByValue.Clear();
        if (DefaultValueCompareFn != null)
        {
            return;
        }

        foreach (var option in Options)
        {
            AddOptionToIndex(option);
        }
    }

    private void AddOptionToIndex(ISelectOption option)
    {
        if (DefaultValueCompareFn != null)
        {
            return;
        }

        var key = GetOptionKey(option.Value);
        if (!_optionsByValue.TryGetValue(key, out var list))
        {
            list = new List<ISelectOption>();
            _optionsByValue[key] = list;
        }
        list.Add(option);
    }

    private void RemoveOptionFromIndex(ISelectOption option)
    {
        if (DefaultValueCompareFn != null)
        {
            return;
        }

        var key = GetOptionKey(option.Value);
        if (!_optionsByValue.TryGetValue(key, out var list))
        {
            return;
        }
        list.Remove(option);
        if (list.Count == 0)
        {
            _optionsByValue.Remove(key);
        }
    }

    private static string GetOptionKey(object? value)
    {
        return value?.ToString() ?? NullValueKey;
    }

    private bool ConfigureDefaultValues()
    {
        if (SelectedOptions != null && SelectedOptions.Count > 0)
        {
            return false;
        }

        List<ISelectOption>? selectedOptions = null;

        if (Mode == SelectMode.Single)
        {
            if (DefaultValues?.Count > 0)
            {
                var defaultValue = DefaultValues.First();
                foreach (var option in Options)
                {
                    if (OptionEqualByValue(defaultValue, option))
                    {
                        selectedOptions = [option];
                        break;
                    }
                }
            }
        }
        else if (Mode == SelectMode.Multiple)
        {
            if (DefaultValues?.Count > 0)
            {
                selectedOptions = new List<ISelectOption>();
                foreach (var defaultValue in DefaultValues)
                {
                    foreach (var option in Options)
                    {
                        if (OptionEqualByValue(defaultValue, option))
                        {
                            selectedOptions.Add(option);
                        }
                    }
                }
            }
        }

        if (selectedOptions == null || selectedOptions.Count == 0)
        {
            return false;
        }

        if (SelectedOptions is IList<ISelectOption> currentSelection && !currentSelection.IsReadOnly)
        {
            currentSelection.Clear();
            foreach (var option in selectedOptions)
            {
                currentSelection.Add(option);
            }
        }
        else
        {
            SetCurrentValue(SelectedOptionsProperty, selectedOptions);
        }
        return true;
    }

    private void ConfigureSingleSelectedOption()
    {
        if (Mode == SelectMode.Single)
        {
            if (SelectedOptions?.Count > 0)
            {
                SetCurrentValue(SelectedOptionProperty, SelectedOptions[0]);
            }
            else
            {
                SetCurrentValue(SelectedOptionProperty, null);
            }
        }
        else
        {
            SetCurrentValue(SelectedOptionProperty, null);
        }
    }

    private void ClearSelectedOptions()
    {
        if (SelectedOptions is IList<ISelectOption> list &&
            !list.IsReadOnly &&
            SelectedOptions is INotifyCollectionChanged)
        {
            list.Clear();
        }
        else
        {
            SetCurrentValue(SelectedOptionsProperty, null);
        }
    }

    private void SetSelectedOptionsInternal(IList<ISelectOption>? selection)
    {
        if (SelectedOptions is IList<ISelectOption> list &&
            !list.IsReadOnly &&
            SelectedOptions is INotifyCollectionChanged)
        {
            list.Clear();
            if (selection != null)
            {
                foreach (var option in selection)
                {
                    list.Add(option);
                }
            }
            return;
        }

        SetCurrentValue(SelectedOptionsProperty, selection);
    }

    private void SubscribeSelectedOptions(IList<ISelectOption>? selectedOptions)
    {
        if (_selectedOptionsNotifier != null)
        {
            _selectedOptionsNotifier.CollectionChanged -= HandleSelectedOptionsCollectionChanged;
            _selectedOptionsNotifier = null;
        }

        _selectedOptionsNotifier = selectedOptions as INotifyCollectionChanged;
        if (_selectedOptionsNotifier != null)
        {
            _selectedOptionsNotifier.CollectionChanged += HandleSelectedOptionsCollectionChanged;
        }
    }

    private void HandleSelectedOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ScheduleSelectedOptionsRefresh();
    }

    private void ScheduleSelectedOptionsRefresh()
    {
        if (_selectedOptionsRefreshScheduled)
        {
            return;
        }

        _selectedOptionsRefreshScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            _selectedOptionsRefreshScheduled = false;
            RefreshSelectedOptions();
        }, DispatcherPriority.Background);
    }

    private void RefreshSelectedOptions()
    {
        ConfigureSingleSelectedOption();
        ConfigureSelectionIsEmpty();
        ConfigurePlaceholderVisible();
        ConfigureSelectedFilterDescription();
        SetCurrentValue(SelectedCountProperty, SelectedOptions?.Count ?? 0);
        CleanDynamicAddedOptions();
        SyncSelection();
    }

    private void ConfigureSelectedFilterDescription()
    {
        if (_optionsBox?.FilterDescriptions != null)
        {
            if (IsHideSelectedOptions)
            {
                var selectedOptions = new HashSet<ISelectOption>();
                if (SelectedOptions?.Count > 0)
                {
                    foreach (var selectedOption in SelectedOptions)
                    {
                        selectedOptions.Add(selectedOption);
                    }
                }
                var oldFilter = _filterSelectedDescription;
                _filterSelectedDescription = new ListFilterDescription()
                {
                    Filter           = SelectFilterFn,
                    FilterConditions = [selectedOptions],
                };
                if (oldFilter != null)
                {
                    _optionsBox.FilterDescriptions.Remove(oldFilter);
                }
                _optionsBox.FilterDescriptions.Add(_filterSelectedDescription);
            }
            else
            {
                if (_filterSelectedDescription != null)
                {
                    _optionsBox.FilterDescriptions.Remove(_filterSelectedDescription);
                }
                _filterSelectedDescription = null;
            }
        }
    }

    private static bool SelectFilterFn(object value, object filterValue)
    {
        if (filterValue is HashSet<ISelectOption> set)
        {
            return !set.Contains(value);
        }
        return true;
    }

    private void ConfigureEffectiveSearchEnabled()
    {
        if (Mode == SelectMode.Tags)
        {
            SetCurrentValue(IsEffectiveFilterEnabledProperty, true);
        }
        else
        {
            SetCurrentValue(IsEffectiveFilterEnabledProperty, IsFilterEnabled);
        }
    }

    private void CleanDynamicAddedOptions()
    {
        if (Mode != SelectMode.Tags)
        {
            return;
        }
        
        var selected = new HashSet<ISelectOption>();
        if (SelectedOptions != null)
        {
            foreach (var opt in SelectedOptions)
            {
                selected.Add(opt);
            }
        }

        var toRemove = new List<ISelectOption>();
        foreach (var option in Options)
        {
            if (option.IsDynamicAdded && !selected.Contains(option))
            {
                toRemove.Add(option);
            }
        }

        foreach (var option in toRemove)
        {
            Options.Remove(option);
            RemoveOptionFromIndex(option);
            if (ReferenceEquals(_addNewOption, option))
            {
                _addNewOption = null;
            }
        }
    }
}
