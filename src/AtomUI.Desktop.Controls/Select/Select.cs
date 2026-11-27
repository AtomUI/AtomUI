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

/// <summary>
/// The kind of selection that a <see cref="Select" /> keeps, chosen with <see cref="Select.Mode" />.
/// </summary>
public enum SelectMode
{
    /// <summary>
    /// One option is selected at a time. The selection is <see cref="Select.SelectedOption" />.
    /// </summary>
    Single,
    /// <summary>
    /// Several options can be selected at a time. The selection is <see cref="Select.SelectedOptions" />.
    /// </summary>
    Multiple,
    /// <summary>
    /// Several options can be selected, and the filter text can be turned into a new tag. The selection is
    /// <see cref="Select.SelectedOptions" />.
    /// </summary>
    Tags
}

/// <summary>
/// A drop-down list that selects one option, several options or tags from a set of
/// <see cref="ISelectOption" /> items, as chosen by <see cref="Mode" />. The options are declared in
/// <see cref="Options" /> or supplied by <see cref="OptionsSource" />, and can be loaded asynchronously
/// through <see cref="OptionsLoader" />.
/// </summary>
[PseudoClasses(SelectPseudoClass.DropdownOpen)]
public partial class Select : AbstractSelect
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="OptionsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<ISelectOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<Select, IEnumerable<ISelectOption>?>(nameof(OptionsSource));

    /// <summary>
    /// Identifies the <see cref="OptionTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<Select, IDataTemplate?>(nameof(OptionTemplate));

    /// <summary>
    /// Identifies the <see cref="IsDefaultActiveFirstOption" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDefaultActiveFirstOptionProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsDefaultActiveFirstOption));

    /// <summary>
    /// Identifies the <see cref="IsGroupEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGroupEnabledProperty =
        ListView.IsGroupEnabledProperty.AddOwner<Select>();

    /// <summary>
    /// Identifies the <see cref="GroupPropertySelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> GroupPropertySelectorProperty =
        ListView.GroupPropertySelectorProperty.AddOwner<Select>();

    /// <summary>
    /// Identifies the <see cref="IsHideSelectedOptions" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHideSelectedOptionsProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsHideSelectedOptions));

    /// <summary>
    /// Identifies the <see cref="Mode" /> property.
    /// </summary>
    public static readonly StyledProperty<SelectMode> ModeProperty =
        AvaloniaProperty.Register<Select, SelectMode>(nameof(Mode));

    /// <summary>
    /// Identifies the <see cref="SelectedOptions" /> property.
    /// </summary>
    public static readonly DirectProperty<Select, IList<ISelectOption>?> SelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<Select, IList<ISelectOption>?>(
            nameof(SelectedOptions),
            o => o.SelectedOptions,
            (o, v) => o.SelectedOptions = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="SelectedOption" /> property.
    /// </summary>
    public static readonly DirectProperty<Select, ISelectOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<Select, ISelectOption?>(
            nameof(SelectedOption),
            o => o.SelectedOption,
            (o, v) => o.SelectedOption = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="OptionFontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> OptionFontSizeProperty =
        AvaloniaProperty.Register<Select, double>(nameof(OptionFontSize));

    /// <summary>
    /// Identifies the <see cref="AutoScrollToSelectedOptions" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoScrollToSelectedOptionsProperty =
        AvaloniaProperty.Register<Select, bool>(
            nameof(AutoScrollToSelectedOptions),
            defaultValue: false);

    /// <summary>
    /// Identifies the <see cref="OptionsAsyncLoadContext" /> property.
    /// </summary>
    public static readonly DirectProperty<Select, object?> OptionsAsyncLoadContextProperty =
        AvaloniaProperty.RegisterDirect<Select, object?>(
            nameof(OptionsAsyncLoadContext),
            o => o.OptionsAsyncLoadContext,
            (o, v) => o.OptionsAsyncLoadContext = v);

    /// <summary>
    /// Identifies the <see cref="OptionsLoader" /> property.
    /// </summary>
    public static readonly StyledProperty<ISelectOptionsAsyncLoader?> OptionsLoaderProperty =
        AvaloniaProperty.Register<Select, ISelectOptionsAsyncLoader?>(nameof(OptionsLoader));

    /// <summary>
    /// Identifies the <see cref="AsyncLoadTimeout" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AsyncLoadTimeoutProperty =
        AvaloniaProperty.Register<Select, TimeSpan>(
            nameof(AsyncLoadTimeout),
            TimeSpan.FromSeconds(10));

    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<Select, IValueFilter?>(nameof(Filter));

    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<Select, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));

    /// <summary>
    /// The options offered in the dropdown, or <see langword="null" /> to use only the options declared in
    /// <see cref="Options" />. Assigning a source clears those options and makes <see cref="Options" /> a
    /// read-only view of it. The selection is then kept only for the options that the new source also contains,
    /// matched by <c>ItemKey</c> or content; the rest is cleared and <see cref="DefaultValues" /> fills the
    /// empty selection.
    /// </summary>
    public IEnumerable<ISelectOption>? OptionsSource
    {
        get => GetValue(OptionsSourceProperty);
        set => SetValue(OptionsSourceProperty, value);
    }

    /// <summary>
    /// The template that renders each option in the dropdown. It is passed to the list view of the dropdown as
    /// its item template. When it is not set, the theme of the select supplies a default.
    /// </summary>
    [InheritDataTypeFromItems(nameof(OptionsSource))]
    public IDataTemplate? OptionTemplate
    {
        get => GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }

    /// <summary>
    /// Has no effect: the select registers the property, but nothing reads it. The default is
    /// <see langword="false" />.
    /// </summary>
    public bool IsDefaultActiveFirstOption
    {
        get => GetValue(IsDefaultActiveFirstOptionProperty);
        set => SetValue(IsDefaultActiveFirstOptionProperty, value);
    }

    /// <summary>
    /// Whether the dropdown groups its options under a header row. The group key comes from
    /// <see cref="GroupPropertySelector" />. The value is passed to the list view of the dropdown and defaults
    /// to <see langword="false" />.
    /// </summary>
    public bool IsGroupEnabled
    {
        get => GetValue(IsGroupEnabledProperty);
        set => SetValue(IsGroupEnabledProperty, value);
    }

    /// <summary>
    /// The function that returns the group key of an option while <see cref="IsGroupEnabled" /> is
    /// <see langword="true" />; options with the same key form one group. The value is passed to the list view of
    /// the dropdown. When it is <see langword="null" />, that list groups by the <c>Group</c> of an
    /// <c>IGroupHeader</c> item.
    /// </summary>
    public DefaultFilterValueSelector? GroupPropertySelector
    {
        get => GetValue(GroupPropertySelectorProperty);
        set => SetValue(GroupPropertySelectorProperty, value);
    }

    /// <summary>
    /// Whether the dropdown hides the options that are already selected. The value is passed to the list view of
    /// the dropdown and to its option items. A selected option then disappears from the list, and the empty
    /// indicator also shows when the list has options but all of them are hidden. The default is
    /// <see langword="false" />.
    /// </summary>
    public bool IsHideSelectedOptions
    {
        get => GetValue(IsHideSelectedOptionsProperty);
        set => SetValue(IsHideSelectedOptionsProperty, value);
    }

    /// <summary>
    /// The kind of selection: one option, several options or tags. It decides whether the selection is kept in
    /// <see cref="SelectedOption" /> or in <see cref="SelectedOptions" />, and is the mode that
    /// <see cref="SelectionChanged" /> reports. The default is <see cref="SelectMode.Single" />. Leaving
    /// <see cref="SelectMode.Tags" /> mode discards the tags that were created from the filter text.
    /// </summary>
    public SelectMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>
    /// The options offered in the dropdown. It is the XAML content property, so options can be declared inline;
    /// only items that are <see cref="ISelectOption" /> are used. After <see cref="OptionsSource" /> is
    /// assigned, the declared items are cleared and this collection is a read-only view of the source until the
    /// source is set to <see langword="null" /> again. Tags created from the filter text in
    /// <see cref="SelectMode.Tags" /> mode are kept apart from it.
    /// </summary>
    [Content]
    public ItemCollection Options { get; } = new();

    private IList<ISelectOption>? _selectedOptions;

    /// <summary>
    /// The selected options in <see cref="SelectMode.Multiple" /> and <see cref="SelectMode.Tags" /> mode, or
    /// <see langword="null" /> when nothing is selected. Replacing the list, or adding an option to it or
    /// removing one from it, raises <see cref="SelectionChanged" />. Bindings are two-way by default and report
    /// their validation errors on the select.
    /// </summary>
    public IList<ISelectOption>? SelectedOptions
    {
        get => _selectedOptions;
        set => SetAndRaise(SelectedOptionsProperty, ref _selectedOptions, value);
    }

    private ISelectOption? _selectedOption;

    /// <summary>
    /// The selected option in <see cref="SelectMode.Single" /> mode, or <see langword="null" /> when nothing is
    /// selected. Replacing it raises <see cref="SelectionChanged" />. Bindings are two-way by default and report
    /// their validation errors on the select.
    /// </summary>
    public ISelectOption? SelectedOption
    {
        get => _selectedOption;
        set => SetAndRaise(SelectedOptionProperty, ref _selectedOption, value);
    }

    /// <summary>
    /// Font size of the options. The theme sets it from the <c>FontSize</c> token, but the select does not read
    /// it: the option items of the dropdown and the tags take their font size from the <c>OptionFontSize</c> token
    /// directly, the tags through the attached <c>SelectTag.OptionFontSize</c>. The registered default is
    /// <c>0</c>.
    /// </summary>
    public double OptionFontSize
    {
        get => GetValue(OptionFontSizeProperty);
        set => SetValue(OptionFontSizeProperty, value);
    }

    /// <summary>
    /// Whether the dropdown list scrolls the selected option into view when its template is applied. The value
    /// is passed to <see cref="ListView.AutoScrollToSelectedItem" /> of that list. The default is
    /// <see langword="false" />, which switches the scroll off.
    /// </summary>
    public bool AutoScrollToSelectedOptions
    {
        get => GetValue(AutoScrollToSelectedOptionsProperty);
        set => SetValue(AutoScrollToSelectedOptionsProperty, value);
    }

    private object? _optionsAsyncLoadContext;

    /// <summary>
    /// The value that tells <see cref="OptionsLoader" /> what to load. It is passed to
    /// <see cref="ISelectOptionsAsyncLoader.LoadAsync" /> when a load starts and is carried by the
    /// <see cref="OptionsLoading" /> and <see cref="OptionsLoaded" /> events. The default is
    /// <see langword="null" />.
    /// </summary>
    public object? OptionsAsyncLoadContext
    {
        get => _optionsAsyncLoadContext;
        set => SetAndRaise(OptionsAsyncLoadContextProperty, ref _optionsAsyncLoadContext, value);
    }

    /// <summary>
    /// The filter that decides which options the dropdown shows for the filter text. It receives the value that
    /// <see cref="FilterValueSelector" /> returns for an option. The value is passed to the list view of the
    /// dropdown. The default is <see langword="null" />, which leaves the filtering to that list.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    /// <summary>
    /// The function that returns the value of an option that <see cref="Filter" /> receives. The value is passed
    /// to the list view of the dropdown. When it is <see langword="null" /> when the select is initialized,
    /// <see cref="HeaderFilterPropertySelector" /> is set as the current value.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    /// <summary>
    /// The loader that supplies the options asynchronously, or <see langword="null" /> to load nothing. A click
    /// that requests the dropdown while no load has succeeded yet makes the select call
    /// <see cref="ISelectOptionsAsyncLoader.LoadAsync" /> with <see cref="OptionsAsyncLoadContext" />, switch
    /// <see cref="AbstractSelect.IsLoading" /> on and, on success, take the returned data as the
    /// <see cref="OptionsSource" />. The dropdown then opens only when the data has at least one option.
    /// </summary>
    public ISelectOptionsAsyncLoader? OptionsLoader
    {
        get => GetValue(OptionsLoaderProperty);
        set => SetValue(OptionsLoaderProperty, value);
    }

    /// <summary>
    /// How long one options load may take. It is read when the load starts. When it elapses, the load is abandoned
    /// and <see cref="OptionsLoaded" /> reports a timed out result. The default is 10 seconds.
    /// </summary>
    public TimeSpan AsyncLoadTimeout
    {
        get => GetValue(AsyncLoadTimeoutProperty);
        set => SetValue(AsyncLoadTimeoutProperty, value);
    }
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when an options load starts without an <see cref="OptionsLoader" />; a load that a loader performs
    /// does not raise it. Cancelling the event skips the <see cref="OptionsLoaded" /> event that ends the load.
    /// </summary>
    public event EventHandler<SelectOptionsLoadingEventArgs>? OptionsLoading;
    /// <summary>
    /// Raised when an options load has finished. On success the loaded data has already become the
    /// <see cref="OptionsSource" />, and the dropdown opens after the event when the data has at least one
    /// option. When the loader threw or the <see cref="AsyncLoadTimeout" /> elapsed, the event carries a failed
    /// result, the dropdown stays closed and the next click loads again. The result is <see langword="null" />
    /// when no <see cref="OptionsLoader" /> was set.
    /// </summary>
    public event EventHandler<SelectOptionsLoadedEventArgs>? OptionsLoaded;
    /// <summary>
    /// Raised after a selection change is committed: when <see cref="SelectedOption" /> or
    /// <see cref="SelectedOptions" /> was replaced, when the selection of the dropdown list changed, or when an
    /// option was added to or removed from the <see cref="SelectedOptions" /> list.
    /// </summary>
    public event EventHandler<SelectSelectionChangedEventArgs>? SelectionChanged;

    #endregion

    /// <summary>
    /// A ready-made <see cref="FilterValueSelector" /> that returns the header of an <see cref="ISelectOption" />
    /// and <see langword="null" /> for any other item. The select sets it as the current
    /// <see cref="FilterValueSelector" /> when none is set at initialization.
    /// </summary>
    public static readonly DefaultFilterValueSelector HeaderFilterPropertySelector = data =>
    {
        if (data is ISelectOption option)
        {
            return option.Header;
        }
        return null;
    };

    /// <summary>
    /// A ready-made <see cref="FilterValueSelector" /> that returns the content of an <see cref="ISelectOption" />
    /// and <see langword="null" /> for any other item. The select does not use it by default.
    /// </summary>
    public static readonly DefaultFilterValueSelector ValueFilterPropertySelector = data =>
    {
        if (data is ISelectOption option)
        {
            return option.Content;
        }
        return null;
    };

    /// <summary>
    /// The comparison that decides whether an entry of <see cref="DefaultValues" /> and an option are the same.
    /// It receives the entry and the option. When it is <see langword="null" />, the text of the entry is compared
    /// with the text of the option's content.
    /// </summary>
    public Func<object, ISelectOption, bool>? DefaultValueCompareFn { get; set; }

    /// <summary>
    /// The values that are selected while the selection is empty, or <see langword="null" /> to select nothing.
    /// They are applied when the select is loaded and again when <see cref="OptionsSource" /> changes, and only
    /// while nothing is selected. In <see cref="SelectMode.Single" /> mode only the first entry is used and
    /// selects the first option that matches; in the other modes every entry selects every option that matches.
    /// </summary>
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

    /// <summary>
    /// Clears the selection by setting <see cref="SelectedOptions" /> and <see cref="SelectedOption" /> to
    /// <see langword="null" />. The select calls it when its handle requests a clear.
    /// </summary>
    public void ClearValue()
    {
        SelectedOptions = null;
        SelectedOption  = null;
    }

    /// <summary>
    /// After the base method ran, sets <see cref="HeaderFilterPropertySelector" /> as the current
    /// <see cref="FilterValueSelector" /> when none is set.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (FilterValueSelector == null)
        {
            SetCurrentValue(FilterValueSelectorProperty, HeaderFilterPropertySelector);
        }
    }

    /// <summary>
    /// After the base method ran, applies <see cref="DefaultValues" />.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureDefaultValues();
    }

    /// <summary>
    /// After the base method ran, starts listening for changes of the <see cref="SelectedOptions" /> list and
    /// refreshes the state that the select derives from the selection.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureSelectedOptionsCollectionChangedSource(SelectedOptions);
        ConfigureSelectionValueState();
    }

    /// <summary>
    /// Cancels a pending options load, removes the candidate list from the popup and stops listening for changes
    /// of the <see cref="SelectedOptions" /> list, then calls the base method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelPendingOptionsLoad();
        ClearPopupContent();
        ReleaseSelectedOptionsCollectionChangedSource();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Discards a previously created candidate list, calls the base method, and then resolves
    /// <c>PART_SingleFilterInput</c>, subscribes to the search input of the selected options box and lets
    /// pointer input pass through the popup overlay to the add-on decorated box of the template. It refreshes
    /// the placeholder, the single result and the filter text box, and creates the dropdown content when the
    /// dropdown is already open.
    /// </summary>
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

    /// <summary>
    /// Creates the candidate list of the dropdown once, passes the option, filter, group and selection settings
    /// of the select to it, and puts it into the border that the template set as the child of the popup. It
    /// returns without doing anything when the template provides no popup. The base method is not called.
    /// </summary>
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

        // The popup frame is the Border declared statically in the Select ControlTheme (Popup.Child).
        // It is read from Popup.Child every time, so that no cached field can point to the previous
        // instance after the template is applied again.
        if (Popup.Child is Border popupFrame && !ReferenceEquals(popupFrame.Child, _candidateList))
        {
            popupFrame.Child = _candidateList;
        }
    }

    /// <summary>
    /// After the base method ran, brings the template parts and pseudo classes in line with the changed property.
    /// The single result and the filter text box follow the dropdown state, the mode, the filter text and the
    /// placeholder; the popup width follows <see cref="AbstractSelect.IsPopupMatchSelectWidth" />; the pseudo
    /// classes follow the style variant and the status. Leaving <see cref="SelectMode.Tags" /> mode discards the
    /// tags that were created from the filter text.
    /// </summary>
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

    /// <summary>
    /// Handles the keyboard of the dropdown after the base method ran, and returns when the base method already
    /// handled the event. While the dropdown is open, the candidate list gets the key first. Then F4 without Alt
    /// and Alt+Up or Alt+Down toggle <see cref="AbstractSelect.IsDropDownOpen" />; Down, Up, Enter and Space open
    /// a closed dropdown; Escape closes an open one. Each of these keys is marked handled.
    /// </summary>
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

    /// <summary>
    /// After the base method ran, marks a press inside the popup as handled. While the dropdown is open, a press
    /// on the select closes it and is marked handled, except a press on the filter text box or on the close
    /// button of a tag, which leaves the dropdown open. A press elsewhere while the dropdown is closed only sets
    /// the <c>pressed</c> pseudo-class.
    /// </summary>
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

    /// <summary>
    /// Marks a release inside the popup as handled. A release that ends a press on the select, which
    /// <see cref="OnPointerPressed" /> recorded with the <c>pressed</c> pseudo-class, opens a closed dropdown
    /// (through the open request, which loads the options first when needed) or closes an open one, unless the
    /// release is on the close button of a tag; it is marked handled. The <c>pressed</c> pseudo-class is cleared
    /// and the base method is called last.
    /// </summary>
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

    /// <summary>
    /// Called after the popup closed. In <see cref="SelectMode.Single" /> mode it clears the filter text and
    /// updates the filter text box. It clears the active option of the dropdown list and calls the base method
    /// last.
    /// </summary>
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

    /// <summary>
    /// Called after the popup opened. It marks the dropdown list as active and copies the selection into it: the
    /// selected option, or a copy of <see cref="SelectedOptions" />. In <see cref="SelectMode.Single" /> mode it
    /// focuses the filter text box when filtering is in effect. The base method is called last.
    /// </summary>
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

    /// <summary>
    /// Switches <see cref="AbstractSelect.IsLoading" /> on and raises <see cref="OptionsLoading" />. It runs
    /// when an options load starts without an <see cref="OptionsLoader" />; a load that a loader performs does
    /// not pass through it. Call the base method to raise the event.
    /// </summary>
    protected virtual void NotifyOptionsLoading(SelectOptionsLoadingEventArgs e)
    {
        IsLoading = true;
        OptionsLoading?.Invoke(this, e);
    }

    /// <summary>
    /// Switches <see cref="AbstractSelect.IsLoading" /> off and raises <see cref="OptionsLoaded" />; the caller
    /// then opens or closes the dropdown. It runs when a load succeeded and when a load without a loader
    /// completed. A failed or timed out load raises <see cref="OptionsLoaded" /> directly instead. Call the base
    /// method to raise the event.
    /// </summary>
    protected virtual void NotifyOptionsLoaded(SelectOptionsLoadedEventArgs e)
    {
        IsLoading = false;
        OptionsLoaded?.Invoke(this, e);
    }

    #region 实现 FormItem 接口

    /// <summary>
    /// Applies the value that the owning form item sets. In <see cref="SelectMode.Single" /> mode it must be an
    /// <see cref="ISelectOption" /> and becomes <see cref="SelectedOption" />; in the other modes it must be an
    /// <c>IList&lt;ISelectOption&gt;</c> and becomes <see cref="SelectedOptions" />. A value of any other type
    /// clears the selection.
    /// </summary>
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

    /// <summary>
    /// Returns the value that the owning form item reads: <see cref="SelectedOption" /> in
    /// <see cref="SelectMode.Single" /> mode, otherwise <see cref="SelectedOptions" />.
    /// </summary>
    protected override object? NotifyGetFormValue()
    {
        if (Mode == SelectMode.Single)
        {
            return SelectedOption;
        }
        return SelectedOptions;
    }

    /// <summary>
    /// Clears the selection for the owning form item: sets <see cref="SelectedOption" /> in
    /// <see cref="SelectMode.Single" /> mode, otherwise <see cref="SelectedOptions" />, to
    /// <see langword="null" />.
    /// </summary>
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
        // For now the options are loaded only once; a failed load does not change that state, so the
        // next open request loads again.
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

        // The popup frame is the Border declared statically in the Select ControlTheme (Popup.Child).
        // Only the candidate list inside it is removed; the Border itself stays as the root of the popup.
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
