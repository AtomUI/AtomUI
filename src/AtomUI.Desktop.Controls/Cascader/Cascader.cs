using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using ItemCollection = AtomUI.Collections.ItemCollection;

public partial class Cascader : AbstractSelect
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
            (o, v) => o.SelectedOption = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly DirectProperty<Cascader, IList<ICascaderOption>?> SelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<Cascader, IList<ICascaderOption>?>(
            nameof(SelectedOptions),
            o => o.SelectedOptions,
            (o, v) => o.SelectedOptions = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<IconTemplate?> ExpandIconProperty =
        CascaderView.ExpandIconProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IconTemplate?> LoadingIconProperty =
        CascaderView.LoadingIconProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<CascaderViewExpandTrigger> ExpandTriggerProperty =
        CascaderView.ExpandTriggerProperty.AddOwner<Cascader>();

    public static readonly StyledProperty<ICascaderItemDataLoader?> DataLoaderProperty =
        CascaderView.DataLoaderProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
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
    
    public IList<ICascaderOption>? SelectedOptions
    {
        get => _checkedOptions;
        set => SetAndRaise(SelectedOptionsProperty, ref _checkedOptions, value);
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
    
    public IValueFilter? Filter
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
    
    public ItemsSourceView OptionsView => ItemsSourceView.GetOrCreate(_options);
    
    [Content]
    public ItemCollection Options => _options;
    
    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<Cascader, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<Cascader, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    internal static readonly DirectProperty<Cascader, IList<ICascaderOption>?> EffectiveSelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<Cascader, IList<ICascaderOption>?>(
            nameof(EffectiveSelectedOptions),
            o => o.EffectiveSelectedOptions,
            (o, v) => o.EffectiveSelectedOptions = v);
    
    internal static readonly DirectProperty<Cascader, string?> SelectedOptionPathProperty =
        AvaloniaProperty.RegisterDirect<Cascader, string?>(
            nameof(SelectedOptionPath),
            o => o.SelectedOptionPath,
            (o, v) => o.SelectedOptionPath = v);
    
    internal static readonly DirectProperty<Cascader, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<Cascader, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }
    
    private IList<ICascaderOption>? _effectiveSelectedOptions;

    internal IList<ICascaderOption>? EffectiveSelectedOptions
    {
        get => _effectiveSelectedOptions;
        set => SetAndRaise(EffectiveSelectedOptionsProperty, ref _effectiveSelectedOptions, value);
    }
    
    private string? _selectOptionPath;

    internal string? SelectedOptionPath
    {
        get => _selectOptionPath;
        set => SetAndRaise(SelectedOptionPathProperty, ref _selectOptionPath, value);
    }

    private bool _isEffectiveEmptyVisible = false;
    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }
    #endregion
    
    private readonly ItemCollection _options = new();
    private SelectFilterTextBox? _singleFilterInput;
    private CascaderView? _cascaderView;
    private INotifyCollectionChanged? _selectedOptionsCollectionChangedSource;
    private bool _needSkipSyncSelectedOptions;
    private bool _isDefaultSelectOptionPathApplied;
    private bool _isEnterKeyPressed;

    static Cascader()
    {
        FocusableProperty.OverrideDefaultValue<Cascader>(true);
        KeyDownEvent.AddClassHandler<Cascader>((control, e) => control.HandlePreviewKeyDown(e), RoutingStrategies.Tunnel);
        SelectHandle.ClearRequestedEvent.AddClassHandler<Cascader>((target, args) => target.HandleClearRequest());
        SelectFilterTextBox.TextChangedEvent.AddClassHandler<Cascader>((x, e) => x.HandleSearchInputTextChanged(e));
        SelectTag.ClosedEvent.AddClassHandler<Cascader>((x, e) => x.HandleTagCloseRequest(e));
        IsMultipleProperty.Changed.AddClassHandler<Cascader>((cascader, e) => cascader.HandleIsCheckableChanged());
        OptionsSourceProperty.Changed.AddClassHandler<Cascader>((cascader, args) => cascader.HandleCascaderSourceChanged(args));
        SelectedOptionProperty.Changed.AddClassHandler<Cascader>((cascader, args) =>
            cascader.HandleSelectedOptionChanged(args));
        SelectedOptionsProperty.Changed.AddClassHandler<Cascader>((cascader, args) =>
            cascader.HandleSelectedOptionsChanged(args));
    }
    
    public Cascader()
    {
        Options.CollectionChanged += HandleCascaderOptionsChanged;
    }
    
    private void HandleCascaderSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var selectedOptionPath  = !IsMultiple ? BuildOptionIdentityPath(SelectedOption) : null;
        var selectedOptionPaths = IsMultiple ? BuildSelectedOptionIdentityPaths(SelectedOptions) : null;

        _options.SetItemsSource(args.GetNewValue<IEnumerable<ICascaderOption>?>());

        if (!IsMultiple)
        {
            if (selectedOptionPath != null &&
                TryParseSelectPath(selectedOptionPath, out var selectedOptions) &&
                selectedOptions.Count > 0)
            {
                SelectedOption = selectedOptions[^1];
                _isDefaultSelectOptionPathApplied = false;
            }
            else
            {
                ApplyDefaultSelectOptionPath(_isDefaultSelectOptionPathApplied);
            }
        }
        else if (selectedOptionPaths != null)
        {
            var remappedOptions = new List<ICascaderOption>(selectedOptionPaths.Count);
            foreach (var path in selectedOptionPaths)
            {
                if (TryParseSelectPath(path, out var selectedOptions) &&
                    selectedOptions.Count > 0)
                {
                    remappedOptions.Add(selectedOptions[^1]);
                }
            }

            if (remappedOptions.Count > 0)
            {
                SelectedOptions = remappedOptions;
            }
        }
    }

    private void HandleCascaderOptionsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (_cascaderView != null)
        {
            _cascaderView.OptionsSource = BuildOptionsList(Options);
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureSelectedOptionsCollectionChangedSource(SelectedOptions);
        ConfigureSelectedOptionsValueState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseSelectedOptionsCollectionChangedSource();
        _isEnterKeyPressed = false;
        base.OnDetachedFromVisualTree(e);
    }
    
    private void HandleClearRequest()
    {
        Clear();
    }
    
    public void Clear()
    {
        _isDefaultSelectOptionPathApplied = false;
        SelectedOption     = null;
        SelectedOptionPath = null;
        SelectedOptions    = null;
    }
    
    private void ConfigurePlaceholderVisible()
    {
        if (IsMultiple)
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, (SelectedOptions == null || SelectedOptions?.Count == 0) && 
                                                              string.IsNullOrWhiteSpace(FilterValue?.ToString()) &&
                                                              string.IsNullOrWhiteSpace(SelectedOptionPath));
        }
        else
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, SelectedOption == null && 
                                                              string.IsNullOrWhiteSpace(FilterValue?.ToString()) &&
                                                              string.IsNullOrWhiteSpace(SelectedOptionPath));
        }
       
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        HandleKeyDownCore(e);
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (!IsKeyboardFocusWithin)
        {
            _isEnterKeyPressed = false;
            _cascaderView?.HandleKeyUp(new KeyEventArgs { Key = Key.Enter });
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _isEnterKeyPressed = false;
        }
        _cascaderView?.HandleKeyUp(e);
        base.OnKeyUp(e);
    }

    private void HandlePreviewKeyDown(KeyEventArgs e)
    {
        if (IsDropDownOpen && e.Source is Avalonia.Controls.TextBox &&
            (e.Key is Key.Up or Key.Down or Key.Enter or Key.Escape ||
             (e.Key == Key.Back && e.Source is Avalonia.Controls.TextBox { Text: null or "" } &&
              _cascaderView?.IsFiltering == false)))
        {
            HandleKeyDownCore(e);
        }
    }

    private void HandleKeyDownCore(KeyEventArgs e)
    {
        if (e.Handled || !IsEnabled || CascaderView.IsComposingKeyInput(e.Source) ||
            (e.KeyModifiers != KeyModifiers.None && e.Key != Key.F4))
        {
            return;
        }
        if (e.Key == Key.Enter)
        {
            if (_isEnterKeyPressed)
            {
                e.Handled = true;
                return;
            }
            _isEnterKeyPressed = true;
        }
        if (IsDropDownOpen)
        {
            _cascaderView?.HandleKeyDown(e);
            if (e.Handled)
            {
                return;
            }
            if (e.Key == Key.Escape)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
                e.Handled = true;
            }
        }
        else if ((e.Key == Key.Down && !this.IsAllowedXYNavigationMode(e.KeyDeviceType)) ||
                 e.Key is Key.Enter or Key.Space)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        if (e.Key == Key.F4)
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
    }

    private void HandleCascaderViewCloseRequested(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty)
        {
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
        else if (change.Property == SelectedOptionsProperty ||
                 change.Property == SelectedOptionProperty ||
                 change.Property == SelectedOptionPathProperty ||
                 change.Property == IsMultipleProperty)
        {
            ConfigurePlaceholderVisible();
            ConfigureSelectionIsEmpty();
            if (IsMultiple)
            {
                SetCurrentValue(SelectedCountProperty, SelectedOptions?.Count ?? 0);
            }
            else
            {
                SetCurrentValue(SelectedCountProperty, SelectedOption != null ? 1 : 0);
            }
        }
        
        if (change.Property == SelectedOptionsProperty ||
            change.Property == IsMultipleProperty)
        {
            SyncSelectedOptionsToCascaderView();
        }

        if (change.Property == IsMultipleProperty ||
            change.Property == MaxCountProperty ||
            change.Property == EffectiveSelectedOptionsProperty ||
            change.Property == IsMultipleProperty)
        {
            ConfigureMaxSelectReached();
        }
        
        if (change.Property == SelectedOptionsProperty ||
            change.Property == ShowCheckedStrategyProperty)
        {
            BuildEffectiveSelectedOptions();
        }

        if (change.Property == SelectedOptionProperty)
        {
            if (SelectedOption != null)
            {
                _isDefaultSelectOptionPathApplied = false;
            }
            ConfigureSelectedOptionPath();
        }
        else if (change.Property == DefaultSelectOptionPathProperty)
        {
            _isDefaultSelectOptionPathApplied = false;
            if (SelectedOption == null)
            {
                ApplyDefaultSelectOptionPath(true);
            }
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_cascaderView != null)
        {
            _cascaderView.SelectedOptionsChanged -= HandleCascaderViewItemsCheckedChanged;
            _cascaderView.ItemDoubleClicked      -= HandleCascaderViewItemDoubleClicked;
            _cascaderView.OptionSelected         -= HandleCascaderViewItemSelected;
            _cascaderView.CloseRequested         -= HandleCascaderViewCloseRequested;
            _cascaderView.OptionsSource          =  null;
        }

        _singleFilterInput = e.NameScope.Find<SelectFilterTextBox>("PART_SingleFilterInput");
        _cascaderView      = e.NameScope.Find<CascaderView>("PART_CascaderView");

        if (_cascaderView != null)
        {
            _cascaderView.SelectedOptionsChanged += HandleCascaderViewItemsCheckedChanged;
            _cascaderView.ItemDoubleClicked      += HandleCascaderViewItemDoubleClicked;
            _cascaderView.OptionSelected         += HandleCascaderViewItemSelected;
            _cascaderView.CloseRequested         += HandleCascaderViewCloseRequested;
            _cascaderView.OptionsSource          =  BuildOptionsList(Options);
            SyncSelectedOptionsToCascaderView();
        }

        ConfigurePlaceholderVisible();
        ConfigureSelectionIsEmpty();
        UpdatePseudoClasses();
        ConfigureSingleFilterTextBox();
    }
    
    protected override void PopupClosed(object? sender, EventArgs e)
    {
        if (!IsMultiple && _singleFilterInput != null)
        {
            _singleFilterInput.Clear();
            _singleFilterInput.Width = double.NaN;
            FilterValue              = null;
            _cascaderView?.ResetInteractionState();
        }
        base.PopupClosed(sender, e);
    }

    protected override void PopupOpened(object? sender, EventArgs e)
    {
        base.PopupOpened(sender, e);
        if (!IsMultiple)
        {
            _cascaderView?.RestoreSelectedOptionPath();
            _singleFilterInput?.Focus();
        }
    }

    private void HandleCascaderViewItemsCheckedChanged(object? sender, CascaderOptionsSelectedChangedEventArgs e)
    {
        if (_cascaderView == null)
        {
            return;
        }

        var                       needSync        = false;
        HashSet<ICascaderOption>? cascaderViewSet = null;
        var                       selectedOptions = _cascaderView.SelectedOptions;
        if (_checkedOptions == null || selectedOptions == null || _checkedOptions.Count != selectedOptions.Count)
        {
            needSync = true;
        }
        else
        {
            var currentSet = BuildOptionSet(_checkedOptions)!;
            cascaderViewSet = BuildOptionSet(selectedOptions)!;
            if (!currentSet.SetEquals(cascaderViewSet))
            {
                needSync = true;
            }
        }
        
        if (needSync)
        {
            try
            {
                _needSkipSyncSelectedOptions =   true;
                cascaderViewSet              ??= BuildOptionSet(_cascaderView?.SelectedOptions);
                if (cascaderViewSet != null)
                {
                    SelectedOptions = BuildOptionList(cascaderViewSet);
                }
           
            }
            finally
            {
                _needSkipSyncSelectedOptions = false;
            }
        }
    }

    private void HandleSelectedOptionChanged(AvaloniaPropertyChangedEventArgs args)
    {
        NotifyFormValueChanged(args.NewValue);
    }

    private void HandleSelectedOptionsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        ConfigureSelectedOptionsCollectionChangedSource(args.GetNewValue<IList<ICascaderOption>?>());
        NotifyFormValueChanged(args.NewValue);
    }

    private void ConfigureSelectedOptionsCollectionChangedSource(IList<ICascaderOption>? selectedOptions)
    {
        if (!this.IsAttachedToVisualTree())
        {
            ReleaseSelectedOptionsCollectionChangedSource();
            return;
        }

        if (ReferenceEquals(_selectedOptionsCollectionChangedSource, selectedOptions))
        {
            return;
        }

        ReleaseSelectedOptionsCollectionChangedSource();

        _selectedOptionsCollectionChangedSource = selectedOptions as INotifyCollectionChanged;
        if (_selectedOptionsCollectionChangedSource != null)
        {
            _selectedOptionsCollectionChangedSource.CollectionChanged += HandleSelectedOptionsCollectionChanged;
        }
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

        NotifyFormValueChanged(SelectedOptions);
        ConfigureSelectedOptionsValueState();
    }

    private void ConfigureSelectedOptionsValueState()
    {
        ConfigurePlaceholderVisible();
        ConfigureSelectionIsEmpty();
        if (IsMultiple)
        {
            SetCurrentValue(SelectedCountProperty, SelectedOptions?.Count ?? 0);
        }
        else
        {
            SetCurrentValue(SelectedCountProperty, SelectedOption != null ? 1 : 0);
        }
        SyncSelectedOptionsToCascaderView();
        BuildEffectiveSelectedOptions();
        ConfigureMaxSelectReached();
    }

    private void ConfigureSelectionIsEmpty()
    {
        if (IsMultiple)
        {
            SetCurrentValue(IsSelectionEmptyProperty, SelectedOptions == null || SelectedOptions?.Count == 0);
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
                    return;
                }
        
                var parent = sourceControl.FindAncestorOfType<IconButton>();
                var tag    = parent?.FindAncestorOfType<SelectTag>();
                if (tag != null)
                {
                    return;
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

    private void HandleCascaderViewItemSelected(object? sender, CascaderOptionSelectedEventArgs eventArgs)
    {
        if (!IsMultiple)
        {
            var option = eventArgs.Option;
            SetCurrentValue(SelectedOptionProperty, option);
            if (option.IsEffectiveLeaf())
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
        }
    }
    
    private void HandleSearchInputTextChanged(TextChangedEventArgs e)
    {
        if (Filter != null)
        {
            if (e.Source is AbstractTextInput textInput)
            {
                FilterValue = textInput.Text?.Trim();
            }
            ConfigurePlaceholderVisible();
        }

        e.Handled = true;
    }

    private void ConfigureMaxSelectReached()
    {
        if (IsMultiple)
        {
            IsMaxSelectReached = MaxCount <= SelectedOptions?.Count;
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
                _needSkipSyncSelectedOptions  = true;
                _cascaderView.SelectedOption  = null;
                _cascaderView.SelectedOptions = null;
            }
            finally
            {
                _needSkipSyncSelectedOptions = false;
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
            if (SelectedOptions != null)
            {
                var checkedOptions = new List<ICascaderOption>(SelectedOptions.Count);
                foreach (var option in SelectedOptions)
                {
                    checkedOptions.Add(option);
                }
                RemoveOptionRecursive(checkedOptions, viewOption);
                SelectedOptions = checkedOptions;
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
    
    private void SyncSelectedOptionsToCascaderView()
    {
        if (!_needSkipSyncSelectedOptions)
        {
            if (_cascaderView != null)
            {
                if (SelectedOptions != null && IsMultiple)
                {
                    if (_cascaderView.SelectedOptions == null)
                    {
                        var checkedOptions = new List<ICascaderOption>(SelectedOptions.Count);
                        checkedOptions.AddRange(SelectedOptions);
                        _cascaderView.SelectedOptions = checkedOptions;
                    }
                    else
                    {
                        var cascaderViewSet = BuildOptionSet(_cascaderView.SelectedOptions)!;
                        var currentSet      = BuildOptionSet(SelectedOptions)!;
                        
                        var currentSelectedOptions = new List<ICascaderOption>(_cascaderView.SelectedOptions.Count);
                        currentSelectedOptions.AddRange(_cascaderView.SelectedOptions);
                        
                        foreach (var option in cascaderViewSet)
                        {
                            if (!currentSet.Contains(option))
                            {
                                currentSelectedOptions.Remove(option);
                            }
                        }
        
                        foreach (var option in currentSet)
                        {
                            if (!cascaderViewSet.Contains(option))
                            {
                                currentSelectedOptions.Add(option);
                            }
                        }
                        _cascaderView.SelectedOptions = currentSelectedOptions;
                    }
                }
                else
                {
                    _cascaderView.SelectedOption  = null;
                    _cascaderView.SelectedOptions = null;
                }
            }
        }
    }
    
    private void BuildEffectiveSelectedOptions()
    {
        if (SelectedOptions != null)
        {
            if (SelectedOptions.Count == 0 || ShowCheckedStrategy == TreeSelectCheckedStrategy.All)
            {
                EffectiveSelectedOptions = BuildOptionList(SelectedOptions);
            }
            else
            {
                var effectiveSelectedOptions = new List<ICascaderOption>(SelectedOptions.Count);
                var showCheckedStrategy = ShowCheckedStrategy;
                if ((showCheckedStrategy & TreeSelectCheckedStrategy.ShowParent) == TreeSelectCheckedStrategy.ShowParent)
                {
                    var selectedSet          = BuildOptionSet(SelectedOptions)!;
                    var fullySelectedParents = new HashSet<ICascaderOption>(SelectedOptions.Count);
                    foreach (var node in SelectedOptions)
                    {
                        if (AreAllChildrenSelected(node.Children, selectedSet))
                        {
                            fullySelectedParents.Add(node);
                        }
                    }
                    foreach (var option in SelectedOptions)
                    {
                        if (!HasSelectedAncestor(option, fullySelectedParents))
                        {
                            effectiveSelectedOptions.Add(option);
                        }
                    }
                }
                if ((showCheckedStrategy & TreeSelectCheckedStrategy.ShowChild) == TreeSelectCheckedStrategy.ShowChild)
                {
                    foreach (var option in SelectedOptions)
                    {
                        if (!option.HasChildren())
                        {
                            effectiveSelectedOptions.Add(option);
                        }
                    }
                }
                EffectiveSelectedOptions = effectiveSelectedOptions;
            }
        }
        else
        {
            EffectiveSelectedOptions = null;
        }
    }

    private static List<ICascaderOption> BuildOptionsList(IEnumerable source)
    {
        var options = source switch
        {
            ICollection collection => new List<ICascaderOption>(collection.Count),
            IReadOnlyCollection<ICascaderOption> collection => new List<ICascaderOption>(collection.Count),
            _ => new List<ICascaderOption>()
        };
        foreach (var item in source)
        {
            options.Add((ICascaderOption)item!);
        }
        return options;
    }

    private static List<ICascaderOption> BuildOptionList(ICollection<ICascaderOption> source)
    {
        var options = new List<ICascaderOption>(source.Count);
        foreach (var item in source)
        {
            options.Add(item);
        }
        return options;
    }

    private static HashSet<ICascaderOption>? BuildOptionSet(ICollection<ICascaderOption>? source)
    {
        if (source == null)
        {
            return null;
        }

        var options = new HashSet<ICascaderOption>(source.Count);
        foreach (var item in source)
        {
            options.Add(item);
        }
        return options;
    }

    private static List<TreeNodePath>? BuildSelectedOptionIdentityPaths(ICollection<ICascaderOption>? options)
    {
        if (options == null)
        {
            return null;
        }

        var paths = new List<TreeNodePath>(options.Count);
        foreach (var option in options)
        {
            var path = BuildOptionIdentityPath(option);
            if (path != null)
            {
                paths.Add(path);
            }
        }
        return paths;
    }

    private static TreeNodePath? BuildOptionIdentityPath(ICascaderOption? option)
    {
        if (option == null)
        {
            return null;
        }

        var depth    = CountOptionPathDepth(option);
        var segments = new string[depth];
        var current  = option;
        for (var i = segments.Length - 1; current != null; i--)
        {
            var segment = current.ItemKey?.ToString() ?? current.Value?.ToString();
            if (string.IsNullOrEmpty(segment))
            {
                return null;
            }

            segments[i] = segment;
            current     = current.ParentNode as ICascaderOption;
        }

        return new TreeNodePath(segments);
    }

    private bool TryParseSelectPath(TreeNodePath path, out IList<ICascaderOption> pathNodes)
    {
        var segments     = path.Segments;
        var isPathValid  = true;
        var currentItems = BuildOptionsList(Options);
        var options      = new List<ICascaderOption>(segments.Count);
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var found   = false;
            foreach (var currentItem in currentItems)
            {
                if (segment == currentItem.ItemKey || segment == currentItem.Value?.ToString())
                {
                    options.Add(currentItem);
                    currentItems = BuildOptionsList(currentItem.Children);
                    found        = true;
                    break;
                }
            }

            if (!found)
            {
                isPathValid = false;
                break;
            }
        }

        pathNodes = options;
        return isPathValid;
    }

    private static bool AreAllChildrenSelected(
        IEnumerable<ICascaderOption> children,
        ISet<ICascaderOption> selected)
    {
        foreach (var child in children)
        {
            if (!selected.Contains(child))
            {
                return false;
            }
        }
        return true;
    }

    private static bool HasSelectedAncestor(ICascaderOption option, ISet<ICascaderOption> selectedAncestors)
    {
        var current = option.ParentNode as ICascaderOption;
        while (current != null)
        {
            if (selectedAncestors.Contains(current))
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
                var parts   = new string[CountOptionPathDepth(current)];
                for (var i = parts.Length - 1; current != null; i--)
                {
                    parts[i] = current.Header?.ToString() ?? string.Empty;
                    current = current.ParentNode as ICascaderOption;
                }

                SelectedOptionPath = string.Join('/', parts);
            }

            if (_cascaderView != null)
            {
                _cascaderView.SelectedOption = SelectedOption;
            }
        }
    }

    private static int CountOptionPathDepth(ICascaderOption option)
    {
        var count   = 0;
        var current = option;
        while (current != null)
        {
            count++;
            current = current.ParentNode as ICascaderOption;
        }

        return count;
    }

    private void ApplyDefaultSelectOptionPath(bool forceRefresh = false)
    {
        if (IsMultiple || DefaultSelectOptionPath == null)
        {
            return;
        }

        if (!forceRefresh && (SelectedOption != null || SelectedOptionPath != null))
        {
            return;
        }

        if (TryParseSelectPath(DefaultSelectOptionPath, out var options) &&
            options.Count > 0)
        {
            var parts = new string[options.Count];
            for (var i = 0; i < options.Count; i++)
            {
                parts[i] = options[i].Header?.ToString() ?? string.Empty;
            }
            SetCurrentValue(SelectedOptionPathProperty, string.Join("/", parts));
            _isDefaultSelectOptionPathApplied = true;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ApplyDefaultSelectOptionPath();
    }
    
    #region 实现 FormItem 接口
    
    protected override void NotifySetFormValue(object? value)
    {
        if (!IsMultiple)
        {
            SelectedOption = value as ICascaderOption;
        }
        else
        {
            SelectedOptions = value as IList<ICascaderOption>;
        }
    }
    
    protected override object? NotifyGetFormValue()
    {
        if (!IsMultiple)
        {
            return SelectedOption;
        }
        return SelectedOptions;
    }
    
    protected override void NotifyClearFormValue()
    {
        if (!IsMultiple)
        {
            SelectedOption = null;
        }
        else
        {
            SelectedOptions = null;
        }
    }
    #endregion
}
