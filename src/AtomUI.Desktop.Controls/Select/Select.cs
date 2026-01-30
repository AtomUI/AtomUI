using System.Diagnostics;
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
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum SelectMode
{
    Single,
    Multiple,
    Tags
}

[PseudoClasses(SelectPseudoClass.DropdownOpen)]
public partial class Select : AbstractSelect, IControlSharedTokenResourcesHost
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
    
    public static readonly DirectProperty<Select, object?> OptionsAsyncLoadContextProperty =
        AvaloniaProperty.RegisterDirect<Select, object?>(
            nameof(OptionsAsyncLoadContext),
            o => o.OptionsAsyncLoadContext,
            (o, v) => o.OptionsAsyncLoadContext = v);
    
    public static readonly StyledProperty<ISelectOptionsAsyncLoader?> OptionsLoaderProperty =
        AvaloniaProperty.Register<Select, ISelectOptionsAsyncLoader?>(nameof(OptionsLoader));
    
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
    
    private object? _optionsAsyncLoadContext;

    public object? OptionsAsyncLoadContext
    {
        get => _optionsAsyncLoadContext;
        set => SetAndRaise(OptionsAsyncLoadContextProperty, ref _optionsAsyncLoadContext, value);
    }
    
    public ISelectOptionsAsyncLoader? OptionsLoader
    {
        get => GetValue(OptionsLoaderProperty);
        set => SetValue(OptionsLoaderProperty, value);
    }
    #endregion

    #region 公共属性定义

    public event EventHandler<SelectOptionsLoadingEventArgs>? OptionsLoading;
    public event EventHandler<SelectOptionsLoadedEventArgs>? OptionsLoaded;

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
    
    private SelectCandidateList? _candidateList;
    private SelectFilterTextBox? _singleFilterInput;
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
        SetCurrentValue(SelectedOptionsProperty, newSelection);
        SyncSelection();

        if (Mode == SelectMode.Tags && removedItem.IsDynamicAdded)
        {
            Options.Remove(removedItem);
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
            // if (e.Key == Key.Down)
            // {
            //     _candidateList?.MoveActiveBy(1);
            //     e.Handled = true;
            // }
            // else if (e.Key == Key.Up)
            // {
            //     _candidateList?.MoveActiveBy(-1);
            //     e.Handled = true;
            // }
            // else if (e.Key == Key.Enter || e.Key == Key.Space)
            // {
            //     _candidateList?.CommitActiveSelection();
            //     if (Mode == SelectMode.Single)
            //     {
            //         SetCurrentValue(IsDropDownOpenProperty, false);
            //     }
            //     e.Handled = true;
            // }
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

        // if (e.Key == Key.Down)
        // {
        //     _candidateList?.MoveActiveBy(1);
        //     e.Handled = true;
        // }
        // else if (e.Key == Key.Up)
        // {
        //     _candidateList?.MoveActiveBy(-1);
        //     e.Handled = true;
        // }
        // else if (e.Key == Key.Enter)
        // {
        //     _candidateList?.CommitActiveSelection();
        //     if (Mode == SelectMode.Single)
        //     {
        //         SetCurrentValue(IsDropDownOpenProperty, false);
        //     }
        //     e.Handled = true;
        // }
        // else if (e.Key == Key.Escape)
        // {
        //     SetCurrentValue(IsDropDownOpenProperty, false);
        //     e.Handled = true;
        // }
        // else if (e.Key == Key.Tab)
        // {
        //     SetCurrentValue(IsDropDownOpenProperty, false);
        // }
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
                var parent = sourceControl.FindAncestorOfType<IconButton>();
                var tag    = parent?.FindAncestorOfType<SelectTag>();
                if (tag != null)
                {
                    IgnorePopupClose = true;
                }
            }
            else if (!IsHideSelectedOptions)
            {
                ClosingDropDown(IsDropDownOpen);
                // SetCurrentValue(IsDropDownOpenProperty, false);
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
                if (Mode == SelectMode.Single)
                {
                    var optionItem = source.FindAncestorOfType<ListItem>();
                    if (optionItem != null && optionItem.IsEnabled)
                    {
                        ClosingDropDown(IsDropDownOpen);
                        // SetCurrentValue(IsDropDownOpenProperty, false);
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
                    if (!IsDropDownOpen)
                    {
                        HandleOpenDropRequest();
                    }
                    else
                    {
                        ClosingDropDown(true);
                    }
                    // SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
                }
    
                e.Handled = true;
            }
        }

        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }

    private void HandleOpenDropRequest()
    {
        // 暂时设计只加载一次，如果加载出错不改变状态
        if (OptionsLoader != null && !_asyncOptionsLoaded)
        {
            LoadOptionsAsync();
        }
        else
        {
            OpeningDropDown(false);
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
       
        _candidateList        = e.NameScope.Get<SelectCandidateList>(SelectThemeConstants.OptionsBoxPart);
        _singleFilterInput = e.NameScope.Get<SelectFilterTextBox>(SelectThemeConstants.SingleFilterInputPart);
        if (_candidateList != null)
        {
            ConfigureOptionsBoxSelectionMode();
        }
        
        ConfigurePlaceholderVisible();
        ConfigureSelectionIsEmpty();
        UpdatePseudoClasses();
        ConfigureSingleFilterTextBox();
        ConfigureEffectiveSearchEnabled();
    }

    internal void NotifyLogicalSelectOption(ISelectOption selectOption)
    {
        Debug.Assert(_candidateList != null);
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
        SetCurrentValue(SelectedOptionsProperty, selectedOptions);
        SyncSelection();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty)
        {
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
    
    protected override void PopupClosed(object? sender, EventArgs e)
    {
        if (Mode == SelectMode.Single)
        {
            if (_singleFilterInput != null)
            {
                _singleFilterInput.Clear();
                _singleFilterInput.Width = double.NaN;
            }
        }
    }

    protected override void PopupOpened(object? sender, EventArgs e)
    {
        if (Mode == SelectMode.Single)
        {
            _singleFilterInput?.Focus();
        }

        SyncSelection();
    }

    private void SyncSelection()
    {
        if (_candidateList != null)
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
            _candidateList.SetCurrentValue(SelectCandidateList.SelectedItemsProperty, selectedItems);
            // _candidateList.EnsureActiveOption();
        }
    }

    private void ConfigureOptionsBoxSelectionMode()
    {
        if (_candidateList == null)
        {
            return;
        }

        _candidateList.SetCurrentValue(List.SelectionModeProperty,
            Mode == SelectMode.Single ? SelectionMode.Single : SelectionMode.Multiple);
    }
    
    public void Clear()
    {
        SelectedOptions = null;
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
        if (_candidateList != null && _candidateList.FilterDescriptions != null)
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
                    _addNewOption = null;
                }
            }
            ConfigurePlaceholderVisible();
            if (string.IsNullOrEmpty(ActivateFilterValue))
            {
                _candidateList.FilterDescriptions.Clear();
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
                        _candidateList.FilterDescriptions.Remove(oldFilter);
                        _candidateList.FilterDescriptions.Add(_filterDescription);
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
                    _candidateList.FilterDescriptions.Add(_filterDescription);
                }
            }

            // Only allow "create from search text" in Tags mode.
            // For Single/Multiple, when data is empty (or filter results are empty),
            // the dropdown should show the empty indicator instead of adding a temporary option.
            if (Mode == SelectMode.Tags &&
                _candidateList.CollectionView?.Count == 0 &&
                !string.IsNullOrWhiteSpace(ActivateFilterValue))
            {
                _addNewOption = new SelectOption()
                {
                    Header = ActivateFilterValue,
                    Value  = ActivateFilterValue,
                    IsDynamicAdded = true
                };
                Options.Add(_addNewOption);
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
                SelectedOptions = selectedOptions;
                SyncSelection();
            }

            if (Mode == SelectMode.Tags)
            {
                if (tag.Item is ISelectOption selectOption && selectOption.IsDynamicAdded)
                {
                    Options.Remove(selectOption);
                }
          
            }
        }
        e.Handled = true;
    }

    private void HandleOptionFilterPropChanged()
    {
        if (_filterDescription != null && _candidateList!= null && _candidateList.FilterDescriptions != null)
        {
            var oldFilter = _filterDescription;
            _filterDescription = new ListFilterDescription()
            {
                PropertyPath     = OptionFilterProp,
                Filter           =  oldFilter.Filter,
                FilterConditions = oldFilter.FilterConditions
            };
            _candidateList.FilterDescriptions.Remove(oldFilter);
            _candidateList.FilterDescriptions.Add(_filterDescription);
        }
    }
    
    private void HandleOptionsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newItemsSource = (IEnumerable<ISelectOption>?)change.NewValue;
        if (newItemsSource != null)
        {
            Options.Clear();
            Options.AddRange(newItemsSource);
            ConfigureDefaultValues();
            SyncSelection();
        }
    }

    private void ConfigureDefaultValues()
    {
        if (SelectedOptions == null || SelectedOptions.Count == 0)
        {
            if (Mode == SelectMode.Single)
            {
                if (DefaultValues?.Count > 0)
                {
                    var defaultValue = DefaultValues.First();
                    foreach (var option in Options)
                    {
                        if (OptionEqualByValue(defaultValue, option))
                        {
                            SetCurrentValue(SelectedOptionsProperty, new List<ISelectOption>()
                            {
                                option
                            });
                            break;
                        }
                    }
                }
            }
            else if (Mode == SelectMode.Multiple)
            {
                if (DefaultValues?.Count > 0)
                {
                    var selectedOptions = new List<ISelectOption>();
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
                    SetCurrentValue(SelectedOptionsProperty, selectedOptions);
                }
            }
        }
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

    private void ConfigureSelectedFilterDescription()
    {
        if (_candidateList?.FilterDescriptions != null)
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
                    _candidateList.FilterDescriptions.Remove(oldFilter);
                }
                _candidateList.FilterDescriptions.Add(_filterSelectedDescription);
            }
            else
            {
                if (_filterSelectedDescription != null)
                {
                    _candidateList.FilterDescriptions.Remove(_filterSelectedDescription);
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
            if (ReferenceEquals(_addNewOption, option))
            {
                _addNewOption = null;
            }
        }
    }
    
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigurePopupMinWith(e.NewSize.Width);
    }
}
