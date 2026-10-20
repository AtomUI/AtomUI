using System.Collections;
using System.Collections.Specialized;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Reflection;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using ItemCollection = AtomUI.Collections.ItemCollection;

public enum SelectMode
{
    Single,
    Multiple,
    Tags
}

[PseudoClasses(SelectPseudoClass.DropdownOpen)]
public partial class Select : AbstractSelect
{
    #region 公共属性定义
    public static readonly StyledProperty<IEnumerable<ISelectOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<Select, IEnumerable<ISelectOption>?>(nameof(OptionsSource));

    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<Select, IDataTemplate?>(nameof(OptionTemplate));

    public static readonly StyledProperty<bool> IsDefaultActiveFirstOptionProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsDefaultActiveFirstOption));

    public static readonly StyledProperty<bool> IsGroupEnabledProperty =
        ListView.IsGroupEnabledProperty.AddOwner<Select>();

    public static readonly StyledProperty<DefaultFilterValueSelector?> GroupPropertySelectorProperty =
        ListView.GroupPropertySelectorProperty.AddOwner<Select>();

    public static readonly StyledProperty<bool> IsHideSelectedOptionsProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsHideSelectedOptions));

    public static readonly StyledProperty<SelectMode> ModeProperty =
        AvaloniaProperty.Register<Select, SelectMode>(nameof(Mode));

    public static readonly DirectProperty<Select, IList<ISelectOption>?> SelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<Select, IList<ISelectOption>?>(
            nameof(SelectedOptions),
            o => o.SelectedOptions,
            (o, v) => o.SelectedOptions = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly DirectProperty<Select, ISelectOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<Select, ISelectOption?>(
            nameof(SelectedOption),
            o => o.SelectedOption,
            (o, v) => o.SelectedOption = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<double> OptionFontSizeProperty =
        AvaloniaProperty.Register<Select, double>(nameof(OptionFontSize));

    public static readonly StyledProperty<bool> AutoScrollToSelectedOptionsProperty =
        AvaloniaProperty.Register<Select, bool>(
            nameof(AutoScrollToSelectedOptions),
            defaultValue: false);

    public static readonly DirectProperty<Select, object?> OptionsAsyncLoadContextProperty =
        AvaloniaProperty.RegisterDirect<Select, object?>(
            nameof(OptionsAsyncLoadContext),
            o => o.OptionsAsyncLoadContext,
            (o, v) => o.OptionsAsyncLoadContext = v);

    public static readonly StyledProperty<ISelectOptionsAsyncLoader?> OptionsLoaderProperty =
        AvaloniaProperty.Register<Select, ISelectOptionsAsyncLoader?>(nameof(OptionsLoader));

    public static readonly StyledProperty<TimeSpan> AsyncLoadTimeoutProperty =
        AvaloniaProperty.Register<Select, TimeSpan>(
            nameof(AsyncLoadTimeout),
            TimeSpan.FromSeconds(10));

    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<Select, IValueFilter?>(nameof(Filter));

    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<Select, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));

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

    public DefaultFilterValueSelector? GroupPropertySelector
    {
        get => GetValue(GroupPropertySelectorProperty);
        set => SetValue(GroupPropertySelectorProperty, value);
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

    [Content]
    public ItemCollection Options { get; } = new();

    private IList<ISelectOption>? _selectedOptions;

    public IList<ISelectOption>? SelectedOptions
    {
        get => _selectedOptions;
        set => SetAndRaise(SelectedOptionsProperty, ref _selectedOptions, value);
    }

    private ISelectOption? _selectedOption;

    public ISelectOption? SelectedOption
    {
        get => _selectedOption;
        set => SetAndRaise(SelectedOptionProperty, ref _selectedOption, value);
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

    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    public ISelectOptionsAsyncLoader? OptionsLoader
    {
        get => GetValue(OptionsLoaderProperty);
        set => SetValue(OptionsLoaderProperty, value);
    }

    public TimeSpan AsyncLoadTimeout
    {
        get => GetValue(AsyncLoadTimeoutProperty);
        set => SetValue(AsyncLoadTimeoutProperty, value);
    }
    #endregion

    #region 公共事件定义

    public event EventHandler<SelectOptionsLoadingEventArgs>? OptionsLoading;
    public event EventHandler<SelectOptionsLoadedEventArgs>? OptionsLoaded;
    public event EventHandler<SelectSelectionChangedEventArgs>? SelectionChanged;

    #endregion

    public static readonly DefaultFilterValueSelector HeaderFilterPropertySelector = data =>
    {
        if (data is ISelectOption option)
        {
            return option.Header;
        }
        return null;
    };

    public static readonly DefaultFilterValueSelector ValueFilterPropertySelector = data =>
    {
        if (data is ISelectOption option)
        {
            return option.Content;
        }
        return null;
    };

    public Func<object, ISelectOption, bool>? DefaultValueCompareFn { get; set; }

    public IList<object>? DefaultValues { get; set; }

    #region 内部属性定义

    internal static readonly DirectProperty<Select, bool> IsEffectiveFilterEnabledProperty =
        AvaloniaProperty.RegisterDirect<Select, bool>(nameof(IsEffectiveFilterEnabled),
            o => o.IsEffectiveFilterEnabled,
            (o, v) => o.IsEffectiveFilterEnabled = v);

    private bool _isEffectiveFilterEnabled;

    internal bool IsEffectiveFilterEnabled
    {
        get => _isEffectiveFilterEnabled;
        set => SetAndRaise(IsEffectiveFilterEnabledProperty, ref _isEffectiveFilterEnabled, value);
    }

    #endregion

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());

    private readonly AvaloniaList<ISelectOption> _runtimeDynamicOptions = new()
    {
        ResetBehavior = ResetBehavior.Remove
    };
    private readonly AvaloniaList<ISelectOption> _effectiveOptions = new()
    {
        ResetBehavior = ResetBehavior.Remove
    };
    private SelectCandidateList? _candidateList;
    private SelectFilterTextBox? _singleFilterInput;
    private SelectResultOptionsBox? _selectedOptionsBox;
    private IDisposable? _selectedOptionsBoxSearchInputSubscription;
    private INotifyCollectionChanged? _selectedOptionsCollectionChangedSource;
    private List<ISelectOption>? _selectedOptionsSnapshot;
    private bool _ignoreSyncSelection;
    private bool _candidateListActivated;
    private bool _syncingSingleFilterInputText;
    private ISelectOption? _addNewOption;

    static Select()
    {
        FocusableProperty.OverrideDefaultValue<Select>(true);
        SelectHandle.ClearRequestedEvent.AddClassHandler<Select>((select, args) => select.ClearValue());
        OptionsSourceProperty.Changed.AddClassHandler<Select>((select, e) => select.HandleOptionsSourcePropertyChanged(e));
        SelectFilterTextBox.TextChangedEvent.AddClassHandler<Select>((select, e) => select.HandleSearchInputTextChanged(e));
        SelectTag.ClosedEvent.AddClassHandler<Select>((select, args) => select.HandleTagCloseRequest(args));
        SelectedOptionsProperty.Changed.AddClassHandler<Select>((select, args) => select.HandleSelectedOptionsChanged(args));
        SelectedOptionProperty.Changed.AddClassHandler<Select>((select, args) => select.HandleSelectedOptionChanged(args));
        SelectResultOptionsBox.KeyDownEvent.AddClassHandler<Select>(
            (x, e) => x.HandleFilterInputKeyDown(e),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    public Select()
    {
        Options.CollectionChanged += HandleOptionsCollectionChanged;
        RebuildEffectiveOptions();
    }

    public void ClearValue()
    {
        SelectedOptions = null;
        SelectedOption  = null;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (FilterValueSelector == null)
        {
            SetCurrentValue(FilterValueSelectorProperty, HeaderFilterPropertySelector);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureDefaultValues();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureSelectedOptionsCollectionChangedSource(SelectedOptions);
        ConfigureSelectionValueState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelPendingOptionsLoad();
        ClearPopupContent();
        ReleaseSelectedOptionsCollectionChangedSource();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ClearPopupContent();
        base.OnApplyTemplate(e);

        _singleFilterInput = e.NameScope.Get<SelectFilterTextBox>("PART_SingleFilterInput");
        SetupSelectedOptionsBoxSearchInputSubscription(e);
        if (Popup != null)
        {
            Popup.OverlayInputPassThroughElement =
                e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        }

        ConfigurePlaceholderVisible();
        ConfigureSelectionIsEmpty();
        ConfigureSingleResultVisible();
        UpdatePseudoClasses();
        ConfigureSingleFilterTextBox();
        ConfigureEffectiveSearchEnabled();
        if (IsDropDownOpen)
        {
            EnsurePopupContent();
        }
    }

    private void SetupSelectedOptionsBoxSearchInputSubscription(TemplateAppliedEventArgs e)
    {
        _selectedOptionsBoxSearchInputSubscription?.Dispose();
        _selectedOptionsBoxSearchInputSubscription = null;

        _selectedOptionsBox = e.NameScope.Find<SelectResultOptionsBox>("SelectedOptionsBox");
        if (_selectedOptionsBox is not null)
        {
            _selectedOptionsBoxSearchInputSubscription =
                _selectedOptionsBox.GetObservable(SelectResultOptionsBox.IsSearchInputEmptyProperty)
                                   .Subscribe(_ => ConfigurePlaceholderVisible());
        }
    }

    protected override void EnsurePopupContent()
    {
        if (Popup == null)
        {
            return;
        }

        if (_candidateList == null)
        {
            _candidateList = new SelectCandidateList
            {
                Name            = "PART_CandidateList",
                BorderThickness = new Thickness(0),
                ItemsSource     = _effectiveOptions
            };
            _candidateList.Classes.Add(SelectSemanticParts.PopupListClass);
            _candidateList.SetTemplatedParent(this);
            _candidateList[!ListView.IsShowEmptyIndicatorProperty]     = this[!IsShowEmptyIndicatorProperty];
            _candidateList[!ListView.EmptyIndicatorProperty]          = this[!EmptyIndicatorProperty];
            _candidateList[!ListView.EmptyIndicatorTemplateProperty]  = this[!EmptyIndicatorTemplateProperty];
            _candidateList[!ListView.FilterProperty]                    = this[!FilterProperty];
            _candidateList[!ListView.FilterValueProperty]               = this[!FilterValueProperty];
            _candidateList[!ListView.FilterValueSelectorProperty]       = this[!FilterValueSelectorProperty];
            _candidateList[!ListView.IsGroupEnabledProperty]            = this[!IsGroupEnabledProperty];
            _candidateList[!ListView.GroupPropertySelectorProperty]     = this[!GroupPropertySelectorProperty];
            _candidateList[!ListView.IsMotionEnabledProperty]           = this[!IsMotionEnabledProperty];
            _candidateList[!SelectCandidateList.IsHideSelectedOptionsProperty] = this[!IsHideSelectedOptionsProperty];
            _candidateList[!SelectCandidateList.MaxCountProperty]       = this[!MaxCountProperty];
            _candidateList[!ListView.AutoScrollToSelectedItemProperty]  = this[!AutoScrollToSelectedOptionsProperty];
            _candidateList[!ItemsControl.ItemTemplateProperty]          = this[!OptionTemplateProperty];
            ((ICandidateList)_candidateList).SelectionChanged += HandleCandidateListSelectionChanged;
            _candidateList.Commit           += HandleCandidateListComplete;
            _candidateList.Cancel           += HandleCandidateListCanceled;
            ConfigureOptionsBoxSelectionMode();
            SyncSelectionToCandidateList();
        }

        // PopupFrame 现在是 SelectTheme 中静态模板 Border（即 Popup.Child），
        // 每次以 Popup.Child 为准取回，避免缓存字段在模板重应用后指向旧实例。
        if (Popup.Child is Border popupFrame && !ReferenceEquals(popupFrame.Child, _candidateList))
        {
            popupFrame.Child = _candidateList;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty)
        {
            ConfigureSingleResultVisible();
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
        if (change.Property == SelectedOptionsProperty ||
            change.Property == SelectedOptionProperty)
        {
            ConfigureSelectionValueState();
        }
        else if (change.Property == ModeProperty)
        {
            if (Mode != SelectMode.Tags)
            {
                ClearRuntimeDynamicOptions();
            }
            ConfigureOptionsBoxSelectionMode();
            ConfigureSingleResultVisible();
            ConfigureSingleFilterTextBox();
        }
        else if (change.Property == FilterValueProperty)
        {
            ConfigurePlaceholderVisible();
            ConfigureSingleResultVisible();
            if (!IsDropDownOpen)
            {
                ConfigureSingleFilterTextBox();
            }
        }
        else if (change.Property == PlaceholderTextProperty)
        {
            ConfigureSingleFilterTextBox();
        }

        if (change.Property == IsFilterEnabledProperty ||
            change.Property == ModeProperty)
        {
            ConfigureEffectiveSearchEnabled();
            ConfigureSingleFilterTextBox();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
        {
            return;
        }

        if (IsDropDownOpen)
        {
            if (_candidateList != null)
            {
                _candidateList.HandleKeyDown(e);
                if (e.Handled)
                {
                    return;
                }
            }
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
                var filterTextBox = sourceControl.FindAncestorOfType<SelectFilterTextBox>(includeSelf: true);
                if (filterTextBox != null)
                {
                    return;
                }

                if (SelectTag.IsCloseButtonSource(sourceControl))
                {
                    e.Handled = true;
                    return;
                }
            }

            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
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
                    clickInTagCloseButton = SelectTag.IsCloseButtonSource(sourceControl);
                }

                if (!clickInTagCloseButton)
                {
                    if (!IsDropDownOpen)
                    {
                        HandleOpenDropRequest();
                    }
                    else
                    {
                        SetCurrentValue(IsDropDownOpenProperty, false);
                    }
                }

                e.Handled = true;
            }
        }

        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }

    protected override void PopupClosed(object? sender, EventArgs e)
    {
        if (Mode == SelectMode.Single)
        {
            FilterValue = null;
            ConfigureSingleFilterTextBox();
        }

        _candidateList?.ClearActiveCandidate();
        _candidateListActivated = false;
        base.PopupClosed(sender, e);
    }

    protected override void PopupOpened(object? sender, EventArgs e)
    {
        _candidateListActivated = true;
        if (_candidateList != null)
        {
            if (Mode == SelectMode.Single)
            {
                ((ICandidateList)_candidateList).SelectedItem = SelectedOption;
            }
            else
            {
                if (SelectedOptions != null && SelectedOptions.Count > 0)
                {
                    ((ICandidateList)_candidateList).SelectedItems = CopySelectedOptions(SelectedOptions);
                }
            }
        }

        if (Mode == SelectMode.Single && IsEffectiveFilterEnabled)
        {
            _singleFilterInput?.Focus();
        }
        base.PopupOpened(sender, e);
    }

    protected virtual void NotifyOptionsLoading(SelectOptionsLoadingEventArgs e)
    {
        IsLoading = true;
        OptionsLoading?.Invoke(this, e);
    }

    protected virtual void NotifyOptionsLoaded(SelectOptionsLoadedEventArgs e)
    {
        IsLoading = false;
        OptionsLoaded?.Invoke(this, e);
    }

    #region 实现 FormItem 接口

    protected override void NotifySetFormValue(object? value)
    {
        if (Mode == SelectMode.Single)
        {
            SelectedOption = value as ISelectOption;
        }
        else
        {
            SelectedOptions = value as IList<ISelectOption>;
        }
    }

    protected override object? NotifyGetFormValue()
    {
        if (Mode == SelectMode.Single)
        {
            return SelectedOption;
        }
        return SelectedOptions;
    }

    protected override void NotifyClearFormValue()
    {
        if (Mode == SelectMode.Single)
        {
            SelectedOption = null;
        }
        else
        {
            SelectedOptions = null;
        }
    }
    #endregion

    private void HandleSelectedOptionsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        ConfigureSelectedOptionsCollectionChangedSource(args.GetNewValue<IList<ISelectOption>?>());
        NotifyFormValueChanged(args.NewValue);
        SelectionChanged?.Invoke(this, new SelectSelectionChangedEventArgs(Mode, args.OldValue, args.NewValue));
        if (_ignoreSyncSelection)
        {
            _ignoreSyncSelection = false;
            return;
        }
        if (_candidateList != null)
        {
            ((ICandidateList)_candidateList).SelectedItems = CopySelectedOptions(SelectedOptions);
        }
    }

    private void ConfigureSelectedOptionsCollectionChangedSource(IList<ISelectOption>? selectedOptions)
    {
        if (!this.IsAttachedToVisualTree())
        {
            ReleaseSelectedOptionsCollectionChangedSource();
            _selectedOptionsSnapshot = BuildSelectedOptionsList(selectedOptions);
            return;
        }

        if (ReferenceEquals(_selectedOptionsCollectionChangedSource, selectedOptions))
        {
            _selectedOptionsSnapshot = BuildSelectedOptionsList(selectedOptions);
            return;
        }

        ReleaseSelectedOptionsCollectionChangedSource();

        _selectedOptionsCollectionChangedSource = selectedOptions as INotifyCollectionChanged;
        if (_selectedOptionsCollectionChangedSource != null)
        {
            _selectedOptionsCollectionChangedSource.CollectionChanged += HandleSelectedOptionsCollectionChanged;
        }

        _selectedOptionsSnapshot = BuildSelectedOptionsList(selectedOptions);
    }

    private void ReleaseSelectedOptionsCollectionChangedSource()
    {
        if (_selectedOptionsCollectionChangedSource != null)
        {
            _selectedOptionsCollectionChangedSource.CollectionChanged -= HandleSelectedOptionsCollectionChanged;
            _selectedOptionsCollectionChangedSource = null;
        }
    }

    private void HandleSelectedOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (!ReferenceEquals(sender, _selectedOptionsCollectionChangedSource))
        {
            return;
        }

        var oldSnapshot = _selectedOptionsSnapshot;
        var newSnapshot = BuildSelectedOptionsList(SelectedOptions);
        _selectedOptionsSnapshot = newSnapshot;

        NotifyFormValueChanged(SelectedOptions);
        SelectionChanged?.Invoke(this, new SelectSelectionChangedEventArgs(Mode, oldSnapshot, newSnapshot));

        ConfigureSelectionValueState();
        if (_candidateList != null)
        {
            ((ICandidateList)_candidateList).SelectedItems = CopySelectedOptions(SelectedOptions);
        }
    }

    private void HandleSelectedOptionChanged(AvaloniaPropertyChangedEventArgs args)
    {
        NotifyFormValueChanged(args.NewValue);
        SelectionChanged?.Invoke(this, new SelectSelectionChangedEventArgs(Mode, args.OldValue, args.NewValue));
        if (_ignoreSyncSelection)
        {
            _ignoreSyncSelection = false;
            return;
        }

        if (_candidateList != null)
        {
            ((ICandidateList)_candidateList).SelectedItem = SelectedOption;
        }
    }

    private bool OptionEqualByValue(object value, ISelectOption selectOption)
    {
        if (DefaultValueCompareFn != null)
        {
            return DefaultValueCompareFn(value, selectOption);
        }
        var strValue = value.ToString();
        var optValue = selectOption.Content?.ToString();
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

        if (e.Source is AbstractTextInput textInput && string.IsNullOrWhiteSpace(textInput.Text) == false)
        {
            return false;
        }

        var lastIndex   = SelectedOptions.Count - 1;
        var removedItem = SelectedOptions[lastIndex];
        var newSelection = new List<ISelectOption>(lastIndex);
        for (var i = 0; i < lastIndex; i++)
        {
            newSelection.Add(SelectedOptions[i]);
        }

        SelectedOptions = newSelection;

        if (Mode == SelectMode.Tags && removedItem.IsDynamicAdded)
        {
            RemoveRuntimeDynamicOption(removedItem);
        }

        e.Handled = true;
        return true;
    }

    private void HandleFilterInputKeyDown(KeyEventArgs e)
    {
        if (TryHandleDeleteKey(e))
        {
            return;
        }
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
            SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    private void ClearPopupContent()
    {
        if (_candidateList != null)
        {
            _candidateList.ClearActiveCandidate();
            ((ICandidateList)_candidateList).SelectionChanged -= HandleCandidateListSelectionChanged;
            _candidateList.Commit           -= HandleCandidateListComplete;
            _candidateList.Cancel           -= HandleCandidateListCanceled;
            _candidateList.ItemsSource      =  null;
            _candidateList.SetTemplatedParent(null);
        }

        // PopupFrame 是 SelectTheme 中静态模板 Border（即 Popup.Child），
        // 仅清空其内部的候选列表，保留 Border 自身作为弹层根。
        if (Popup != null && Popup.Child is Border popupFrame && ReferenceEquals(popupFrame.Child, _candidateList))
        {
            popupFrame.Child = null;
        }

        _candidateList          = null;
        _candidateListActivated = false;
    }

    private void HandleCandidateListComplete(object? sender, RoutedEventArgs e)
    {
        if (_candidateList != null)
        {
            SetSelectionFromCandidateListWithoutSyncBack();
        }
        if (IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void SetSelectionFromCandidateListWithoutSyncBack()
    {
        RunWithoutSyncingCandidateSelection(() =>
        {
            if (Mode == SelectMode.Single)
            {
                SelectedOption = (ISelectOption?)_candidateList?.SelectedItem;
            }
            else
            {
                SelectedOptions = BuildSelectedOptionsList(_candidateList?.SelectedItems);
            }
        });
    }

    private void RunWithoutSyncingCandidateSelection(Action updateSelection)
    {
        _ignoreSyncSelection = true;
        try
        {
            updateSelection();
        }
        finally
        {
            if (_ignoreSyncSelection)
            {
                _ignoreSyncSelection = false;
            }
        }
    }

    private void HandleCandidateListCanceled(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void HandleCandidateListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_candidateListActivated)
        {
            return;
        }
        if (Mode != SelectMode.Single)
        {
            if ((SelectedOptions == null || SelectedOptions.Count == 0) && e.AddedItems.Count == 0)
            {
                return;
            }

            if (SelectedOptions == null || SelectedOptions.Count == 0)
            {
                var addedSelectedSet = new HashSet<ISelectOption>(e.AddedItems.Count);
                foreach (var item in e.AddedItems)
                {
                    if (item is ISelectOption selectOption)
                    {
                        addedSelectedSet.Add(selectOption);
                    }
                }

                if (addedSelectedSet.Count > 0)
                {
                    SelectedOptions = CopySelectedOptions(addedSelectedSet);
                }
                return;
            }

            var currentSelectedSet = BuildSelectedOptionSet(SelectedOptions);
            var newSelectedSet     = new HashSet<ISelectOption>(currentSelectedSet.Count + e.AddedItems.Count);
            foreach (var item in currentSelectedSet)
            {
                newSelectedSet.Add(item);
            }

            foreach (var item in e.AddedItems)
            {
                if (item is ISelectOption selectOption)
                {
                    newSelectedSet.Add(selectOption);
                }
            }
            foreach (var item in e.RemovedItems)
            {
                if (item is ISelectOption selectOption)
                {
                    newSelectedSet.Remove(selectOption);
                }
            }

            if (!newSelectedSet.SetEquals(currentSelectedSet))
            {
                SelectedOptions = CopySelectedOptions(newSelectedSet);
            }
        }
    }

    private void SyncSelectionToCandidateList()
    {
        if (_candidateList != null)
        {
            if (Mode == SelectMode.Single)
            {
                ((ICandidateList)_candidateList).SelectedItem = SelectedOption;
            }
            else
            {
                ((ICandidateList)_candidateList).SelectedItems = CopySelectedOptions(SelectedOptions);
            }
        }
    }

    private void ConfigureOptionsBoxSelectionMode()
    {
        if (_candidateList == null)
        {
            return;
        }

        _candidateList.SetCurrentValue(ListView.SelectionModeProperty,
            Mode == SelectMode.Single ? SelectionMode.Single : SelectionMode.Multiple);
    }

    private void ConfigurePlaceholderVisible()
    {
        if (Mode == SelectMode.Single)
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, false);
        }
        else
        {
            var isSearchInputEmpty = _selectedOptionsBox?.IsSearchInputEmpty ?? true;
            var hasFilterValue     = !string.IsNullOrEmpty(FilterValue?.ToString());
            SetCurrentValue(IsPlaceholderTextVisibleProperty,
                (SelectedOptions == null || SelectedOptions.Count == 0) &&
                isSearchInputEmpty &&
                !hasFilterValue);
        }
    }

    private void ConfigureSingleResultVisible()
    {
        SetCurrentValue(IsSingleResultVisibleProperty, false);
    }

    private void ConfigureSelectionValueState()
    {
        ConfigureSelectionIsEmpty();
        ConfigurePlaceholderVisible();
        ConfigureSingleResultVisible();
        SetCurrentValue(SelectedCountProperty, SelectedOptions?.Count ?? 0);
        CleanRuntimeDynamicOptions();
        ConfigureSingleFilterTextBox();
        _selectedOptionsBox?.RefreshSelectedOptions();
    }

    private void ConfigureSelectionIsEmpty()
    {
        if (Mode == SelectMode.Single)
        {
            SetCurrentValue(IsSelectionEmptyProperty, SelectedOption == null);
        }
        else
        {
            SetCurrentValue(IsSelectionEmptyProperty, SelectedOptions == null || SelectedOptions?.Count == 0);
        }
    }

    private void ConfigureSingleFilterTextBox()
    {
        if (_singleFilterInput == null)
        {
            return;
        }

        _singleFilterInput.Width = double.NaN;

        if (Mode != SelectMode.Single)
        {
            _singleFilterInput.Focusable            = false;
            _singleFilterInput.IsCaretLockedToStart = true;
            _singleFilterInput.IsReadOnly           = true;
            _singleFilterInput.PlaceholderText      = null;
            SetSingleFilterInputText(null);
            ResetSingleFilterInputCaret();
            return;
        }

        var selectedText = SelectedOption?.Header?.ToString();
        var isEditableSearch = IsDropDownOpen && IsEffectiveFilterEnabled;
        _singleFilterInput.Focusable            = IsEffectiveFilterEnabled;
        _singleFilterInput.IsCaretLockedToStart = !isEditableSearch;
        if (isEditableSearch)
        {
            _singleFilterInput.IsReadOnly           = false;
            _singleFilterInput.PlaceholderText      = selectedText ?? PlaceholderText;
            SetSingleFilterInputText(string.Empty);
            ResetSingleFilterInputCaret();
        }
        else
        {
            _singleFilterInput.IsReadOnly           = true;
            _singleFilterInput.PlaceholderText      = PlaceholderText;
            SetSingleFilterInputText(selectedText ?? string.Empty);
            ResetSingleFilterInputCaret();
        }
    }

    private void SetSingleFilterInputText(string? text)
    {
        if (_singleFilterInput == null || _singleFilterInput.Text == text)
        {
            return;
        }

        _syncingSingleFilterInputText = true;
        try
        {
            _singleFilterInput.Text = text;
        }
        finally
        {
            _syncingSingleFilterInputText = false;
        }
    }

    private void ResetSingleFilterInputCaret()
    {
        if (_singleFilterInput == null)
        {
            return;
        }

        _singleFilterInput.ResetCaretToStart();
    }

    private void HandleSearchInputTextChanged(TextChangedEventArgs e)
    {
        if (_candidateList != null)
        {
            if (e.Source is AbstractTextInput textInput)
            {
                if (ReferenceEquals(textInput, _singleFilterInput) &&
                    Mode == SelectMode.Single &&
                    (_syncingSingleFilterInputText || !IsDropDownOpen || !IsEffectiveFilterEnabled))
                {
                    e.Handled = true;
                    return;
                }

                var searchText = textInput.Text?.Trim();
                FilterValue = string.IsNullOrEmpty(searchText) ? null : searchText;
            }

            var filterValue = FilterValue?.ToString();
            if (_addNewOption != null)
            {
                var isSelected     = SelectedOptions?.Contains(_addNewOption) == true;
                var isCurrentInput = filterValue == _addNewOption.Header?.ToString();
                if (!isSelected && !isCurrentInput)
                {
                    RemoveRuntimeDynamicOption(_addNewOption);
                }
                else if (!isSelected && isCurrentInput)
                {
                    ActivateCandidateOption(_addNewOption);
                }
            }
            ConfigurePlaceholderVisible();
            ConfigureSingleResultVisible();

            // Only allow "create from search text" in Tags mode.
            // For Single/Multiple, when data is empty (or filter results are empty),
            // the dropdown should show the empty indicator instead of adding a temporary option.
            if (Mode == SelectMode.Tags &&
                _candidateList.TotalItemCount == 0 &&
                !string.IsNullOrWhiteSpace(filterValue))
            {
                _addNewOption = new SelectOption()
                {
                    Header         = filterValue,
                    Content        = filterValue,
                    IsDynamicAdded = true
                };
                AddRuntimeDynamicOption(_addNewOption);
                ActivateCandidateOption(_addNewOption);
            }
            Dispatcher.Post(SyncSelectionToCandidateList);
        }
        e.Handled = true;
    }

    private void ActivateCandidateOption(ISelectOption option)
    {
        if (_candidateList == null)
        {
            return;
        }

        if (!_candidateList.TrySetCandidateItemSelected(option))
        {
            Dispatcher.Post(() => _candidateList?.TrySetCandidateItemSelected(option));
        }
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
                var selectedOptions = new List<ISelectOption>(SelectedOptions.Count > 0 ? SelectedOptions.Count - 1 : 0);
                var removed         = false;
                foreach (var selectedItem in SelectedOptions)
                {
                    if (!removed && EqualityComparer<ISelectOption>.Default.Equals(selectedItem, tagOption))
                    {
                        removed = true;
                        continue;
                    }
                    selectedOptions.Add(selectedItem);
                }
                SelectedOptions = selectedOptions;
            }

            if (Mode == SelectMode.Tags)
            {
                if (tag.Item is ISelectOption selectOption && selectOption.IsDynamicAdded)
                {
                    RemoveRuntimeDynamicOption(selectOption);
                }
            }
        }
        e.Handled = true;
    }

    private void HandleOptionsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var selectedOptionSnapshot  = Mode == SelectMode.Single ? BuildOptionSnapshot(SelectedOption) : null;
        var selectedOptionSnapshots = Mode != SelectMode.Single ? BuildSelectedOptionSnapshots(SelectedOptions) : null;

        if (!Options.IsReadOnly)
        {
            Options.Clear();
        }
        Options.SetItemsSource(change.GetNewValue<IEnumerable<ISelectOption>?>());

        if (Mode == SelectMode.Single)
        {
            if (selectedOptionSnapshot != null &&
                TryFindOptionByIdentity(selectedOptionSnapshot.Value.Identity, out var remappedOption))
            {
                SelectedOption = remappedOption;
            }
            else
            {
                SelectedOption = null;
                ConfigureDefaultValues();
            }
        }
        else if (selectedOptionSnapshots != null)
        {
            var remappedOptions = new List<ISelectOption>(selectedOptionSnapshots.Count);
            foreach (var snapshot in selectedOptionSnapshots)
            {
                if (TryFindOptionByIdentity(snapshot.Identity, out var remappedOption))
                {
                    remappedOptions.Add(remappedOption);
                    RemoveRuntimeDynamicOptionByIdentity(snapshot.Identity);
                }
                else if (Mode == SelectMode.Tags && snapshot.Option.IsDynamicAdded)
                {
                    remappedOptions.Add(snapshot.Option);
                }
            }

            SelectedOptions = remappedOptions.Count > 0 ? remappedOptions : null;
            ConfigureDefaultValues();
        }
        else
        {
            ConfigureDefaultValues();
        }

        RemoveRuntimeDynamicOptionsShadowedByUserOptions();
        RebuildEffectiveOptions();
    }

    private void HandleOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RemapSelectedRuntimeOptionsToUserOptions();
        RemoveRuntimeDynamicOptionsShadowedByUserOptions();
        RebuildEffectiveOptions();
        SyncSelectionToCandidateList();
    }

    private static (string Identity, ISelectOption Option)? BuildOptionSnapshot(ISelectOption? option)
    {
        var identity = BuildOptionIdentity(option);
        return identity == null || option == null ? null : (identity, option);
    }

    private static List<(string Identity, ISelectOption Option)>? BuildSelectedOptionSnapshots(ICollection<ISelectOption>? options)
    {
        if (options == null)
        {
            return null;
        }

        var snapshots = new List<(string Identity, ISelectOption Option)>(options.Count);
        foreach (var option in options)
        {
            var identity = BuildOptionIdentity(option);
            if (identity != null)
            {
                snapshots.Add((identity, option));
            }
        }
        return snapshots;
    }

    private static string? BuildOptionIdentity(ISelectOption? option)
    {
        if (option == null)
        {
            return null;
        }

        var itemKey = option.ItemKey?.ToString();
        if (!string.IsNullOrEmpty(itemKey))
        {
            return itemKey;
        }
        return option.Content?.ToString();
    }

    private bool TryFindOptionByIdentity(string identity, out ISelectOption option)
    {
        foreach (var item in Options)
        {
            if (item is ISelectOption currentOption &&
                identity == BuildOptionIdentity(currentOption))
            {
                option = currentOption;
                return true;
            }
        }

        option = null!;
        return false;
    }

    private void ConfigureDefaultValues()
    {
        if (Mode == SelectMode.Single)
        {
            if (SelectedOption == null)
            {
                if (DefaultValues?.Count > 0)
                {
                    var defaultValue = DefaultValues[0];
                    foreach (var item in Options)
                    {
                        if (item is ISelectOption option)
                        {
                            if (OptionEqualByValue(defaultValue, option))
                            {
                                SelectedOption = option;
                                break;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (SelectedOptions == null || SelectedOptions.Count == 0)
            {
                if (DefaultValues?.Count > 0)
                {
                    var selectedOptions = new List<ISelectOption>(DefaultValues.Count);
                    foreach (var defaultValue in DefaultValues)
                    {
                        foreach (var item in Options)
                        {
                            if (item is ISelectOption option)
                            {
                                if (OptionEqualByValue(defaultValue, option))
                                {
                                    selectedOptions.Add(option);
                                }
                            }
                        }
                    }

                    SelectedOptions = selectedOptions;
                }
            }
        }
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

    private void CleanRuntimeDynamicOptions()
    {
        if (Mode != SelectMode.Tags)
        {
            ClearRuntimeDynamicOptions();
            return;
        }

        HashSet<ISelectOption>? selected = null;
        if (SelectedOptions is { Count: > 0 })
        {
            selected = new HashSet<ISelectOption>(SelectedOptions.Count);
            foreach (var opt in SelectedOptions)
            {
                selected.Add(opt);
            }
        }

        var removed = false;
        for (var i = _runtimeDynamicOptions.Count - 1; i >= 0; i--)
        {
            var option = _runtimeDynamicOptions[i];
            if (selected?.Contains(option) != true)
            {
                _runtimeDynamicOptions.RemoveAt(i);
                removed = true;
                if (ReferenceEquals(_addNewOption, option))
                {
                    _addNewOption = null;
                }
            }
        }

        if (removed)
        {
            RebuildEffectiveOptions();
        }
    }

    private void AddRuntimeDynamicOption(ISelectOption option)
    {
        if (_runtimeDynamicOptions.Contains(option))
        {
            return;
        }

        _runtimeDynamicOptions.Add(option);
        RebuildEffectiveOptions();
    }

    private void RemoveRuntimeDynamicOption(ISelectOption option)
    {
        var index = _runtimeDynamicOptions.IndexOf(option);
        if (index >= 0)
        {
            _runtimeDynamicOptions.RemoveAt(index);
            RebuildEffectiveOptions();
        }

        if (ReferenceEquals(_addNewOption, option))
        {
            _addNewOption = null;
        }
    }

    private void ClearRuntimeDynamicOptions()
    {
        if (_runtimeDynamicOptions.Count == 0)
        {
            return;
        }

        _runtimeDynamicOptions.Clear();
        _addNewOption = null;
        RebuildEffectiveOptions();
    }

    private void RemoveRuntimeDynamicOptionByIdentity(string identity)
    {
        for (var i = _runtimeDynamicOptions.Count - 1; i >= 0; i--)
        {
            var option = _runtimeDynamicOptions[i];
            if (identity == BuildOptionIdentity(option))
            {
                _runtimeDynamicOptions.RemoveAt(i);
                if (ReferenceEquals(_addNewOption, option))
                {
                    _addNewOption = null;
                }
            }
        }
    }

    private void RemoveRuntimeDynamicOptionsShadowedByUserOptions()
    {
        for (var i = _runtimeDynamicOptions.Count - 1; i >= 0; i--)
        {
            var option   = _runtimeDynamicOptions[i];
            var identity = BuildOptionIdentity(option);
            if (identity != null && TryFindOptionByIdentity(identity, out _))
            {
                _runtimeDynamicOptions.RemoveAt(i);
                if (ReferenceEquals(_addNewOption, option))
                {
                    _addNewOption = null;
                }
            }
        }
    }

    private void RemapSelectedRuntimeOptionsToUserOptions()
    {
        if (Mode != SelectMode.Tags || SelectedOptions is not { Count: > 0 })
        {
            return;
        }

        var changed         = false;
        var remappedOptions = new List<ISelectOption>(SelectedOptions.Count);
        foreach (var option in SelectedOptions)
        {
            var identity = BuildOptionIdentity(option);
            if (option.IsDynamicAdded &&
                identity != null &&
                TryFindOptionByIdentity(identity, out var userOption))
            {
                remappedOptions.Add(userOption);
                changed = true;
                RemoveRuntimeDynamicOptionByIdentity(identity);
            }
            else
            {
                remappedOptions.Add(option);
            }
        }

        if (changed)
        {
            SelectedOptions = remappedOptions;
        }
    }

    private void RebuildEffectiveOptions()
    {
        var candidateList = _candidateList;
        if (candidateList != null)
        {
            candidateList.ClearActiveCandidate();
            ((ICandidateList)candidateList).SelectionChanged -= HandleCandidateListSelectionChanged;
        }

        try
        {
            _effectiveOptions.Clear();
            foreach (var item in Options)
            {
                if (item is ISelectOption option)
                {
                    _effectiveOptions.Add(option);
                }
            }

            foreach (var option in _runtimeDynamicOptions)
            {
                var identity = BuildOptionIdentity(option);
                if (identity == null || !TryFindOptionByIdentity(identity, out _))
                {
                    _effectiveOptions.Add(option);
                }
            }
        }
        finally
        {
            if (candidateList != null)
            {
                ((ICandidateList)candidateList).SelectionChanged += HandleCandidateListSelectionChanged;
            }
        }
    }

    private static List<ISelectOption>? BuildSelectedOptionsList(IEnumerable? source)
    {
        if (source == null)
        {
            return null;
        }

        var selectedOptions = source switch
        {
            ICollection collection => new List<ISelectOption>(collection.Count),
            IReadOnlyCollection<ISelectOption> collection => new List<ISelectOption>(collection.Count),
            _ => new List<ISelectOption>()
        };
        foreach (var item in source)
        {
            selectedOptions.Add((ISelectOption)item!);
        }
        return selectedOptions;
    }

    private static List<ISelectOption>? CopySelectedOptions(ICollection<ISelectOption>? source)
    {
        if (source == null)
        {
            return null;
        }

        var selectedOptions = new List<ISelectOption>(source.Count);
        foreach (var option in source)
        {
            selectedOptions.Add(option);
        }
        return selectedOptions;
    }

    private static HashSet<ISelectOption> BuildSelectedOptionSet(ICollection<ISelectOption>? source)
    {
        var selectedOptions = source == null
            ? new HashSet<ISelectOption>()
            : new HashSet<ISelectOption>(source.Count);
        if (source != null)
        {
            foreach (var option in source)
            {
                selectedOptions.Add(option);
            }
        }
        return selectedOptions;
    }
}
