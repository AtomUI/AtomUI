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

/// <summary>
/// Cascade selection control: a drop-down select that presents a tree of <see cref="ICascaderOption" /> level by
/// level and selects one option or, with <see cref="IsMultiple" />, a set of options.
/// </summary>
public partial class Cascader : AbstractSelect
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="ShowCheckedStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<TreeSelectCheckedStrategy> ShowCheckedStrategyProperty =
        AvaloniaProperty.Register<Cascader, TreeSelectCheckedStrategy>(
            nameof(ShowCheckedStrategy), TreeSelectCheckedStrategy.All);
    
    /// <summary>
    /// Identifies the <see cref="IsMultiple" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<Cascader, bool>(
            nameof(IsMultiple));
    
    /// <summary>
    /// Identifies the <see cref="OptionsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<ICascaderOption>?> OptionsSourceProperty =
        CascaderView.OptionsSourceProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Identifies the <see cref="OptionTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        CascaderView.OptionTemplateProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Identifies the <see cref="SelectedOption" /> property.
    /// </summary>
    public static readonly DirectProperty<Cascader, ICascaderOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<Cascader, ICascaderOption?>(
            nameof(SelectedOption),
            o => o.SelectedOption,
            (o, v) => o.SelectedOption = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="SelectedOptions" /> property.
    /// </summary>
    public static readonly DirectProperty<Cascader, IList<ICascaderOption>?> SelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<Cascader, IList<ICascaderOption>?>(
            nameof(SelectedOptions),
            o => o.SelectedOptions,
            (o, v) => o.SelectedOptions = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="ExpandIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> ExpandIconProperty =
        CascaderView.ExpandIconProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Identifies the <see cref="LoadingIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> LoadingIconProperty =
        CascaderView.LoadingIconProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Identifies the <see cref="ExpandTrigger" /> property.
    /// </summary>
    public static readonly StyledProperty<CascaderViewExpandTrigger> ExpandTriggerProperty =
        CascaderView.ExpandTriggerProperty.AddOwner<Cascader>();

    /// <summary>
    /// Identifies the <see cref="DataLoader" /> property.
    /// </summary>
    public static readonly StyledProperty<ICascaderItemDataLoader?> DataLoaderProperty =
        CascaderView.DataLoaderProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        CascaderView.FilterProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Identifies the <see cref="FilterHighlightStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<TextBlockHighlightStrategy> FilterHighlightStrategyProperty =
        CascaderView.FilterHighlightStrategyProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Identifies the <see cref="FilterHighlightForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        CascaderView.FilterHighlightForegroundProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Identifies the <see cref="DefaultSelectOptionPath" /> property.
    /// </summary>
    public static readonly StyledProperty<TreeNodePath?> DefaultSelectOptionPathProperty =
        AvaloniaProperty.Register<Cascader, TreeNodePath?>(nameof(DefaultSelectOptionPath));
    
    /// <summary>
    /// Identifies the <see cref="IsAllowSelectParent" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowSelectParentProperty =
        CascaderView.IsAllowSelectParentProperty.AddOwner<Cascader>();
    
    /// <summary>
    /// Chooses which selected options are shown as tags in the selection box. It applies only when
    /// <see cref="IsMultiple" /> is <see langword="true" />. With
    /// <see cref="TreeSelectCheckedStrategy.ShowParent" />, a selected option is shown in place of the options
    /// selected below it once all of its children are selected. With
    /// <see cref="TreeSelectCheckedStrategy.ShowChild" />, only selected options that have no children are shown.
    /// <see cref="TreeSelectCheckedStrategy.All" /> (the default) shows every selected option.
    /// </summary>
    public TreeSelectCheckedStrategy ShowCheckedStrategy
    {
        get => GetValue(ShowCheckedStrategyProperty);
        set => SetValue(ShowCheckedStrategyProperty, value);
    }
    
    /// <summary>
    /// Enables multiple selection: options are checked in the drop-down, the selection is held by
    /// <see cref="SelectedOptions" /> and shown as tags. When <see langword="false" /> (the default), a single
    /// option is selected and <see cref="SelectedOption" /> is shown as a path.
    /// </summary>
    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the source of the options offered in the drop-down: an enumerable of
    /// <see cref="ICascaderOption" /> items, where the children of an option form the next level. A new source
    /// replaces the items of <see cref="Options" />. The selection is then resolved again by option path
    /// (<c>ItemKey</c> or <c>Value</c>), so it is kept when its path exists in the new source.
    /// </summary>
    public IEnumerable? OptionsSource
    {
        get => GetValue(OptionsSourceProperty);
        set => SetValue(OptionsSourceProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that renders an option in the drop-down. Its data item is an
    /// <see cref="ICascaderOption" />. The registered default is <see langword="null" />; the theme then supplies a
    /// template that shows the <c>Header</c> of the option.
    /// </summary>
    [InheritDataTypeFromItems("OptionsSource")]
    public IDataTemplate? OptionTemplate
    {
        get => GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }
    
    private ICascaderOption? _selectedOption;
    
    /// <summary>
    /// Gets or sets the option selected in single-selection mode. Assign an option of the current
    /// <see cref="Options" />: the path shown in the selection box is built from its <c>ParentNode</c> chain.
    /// Setting it reports the new value to the owning FormItem and forwards the option to the drop-down view.
    /// <see langword="null" /> clears the selection, so the placeholder can show.
    /// </summary>
    public ICascaderOption? SelectedOption
    {
        get => _selectedOption;
        set => SetAndRaise(SelectedOptionProperty, ref _selectedOption, value);
    }
        
    private IList<ICascaderOption>? _checkedOptions;
    
    /// <summary>
    /// Gets or sets the options selected in multiple-selection mode; its items are shown as tags, chosen by
    /// <see cref="ShowCheckedStrategy" />. The default is <see langword="null" />, which means that nothing is
    /// selected. Assign a list that implements <c>INotifyCollectionChanged</c> to have later changes of the list
    /// tracked. When options are checked in the drop-down, the control replaces this value with a new list.
    /// </summary>
    public IList<ICascaderOption>? SelectedOptions
    {
        get => _checkedOptions;
        set => SetAndRaise(SelectedOptionsProperty, ref _checkedOptions, value);
    }
    
    /// <summary>
    /// Gets or sets the template of the icon that marks an option which has a next level. Set it on the
    /// Cascader; the theme forwards it to the option items. The registered default is
    /// <see langword="null" />; the theme then supplies a right arrow.
    /// </summary>
    public IconTemplate? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template of the icon shown on an option item while its children are being loaded by the
    /// <see cref="DataLoader" />. Set it on the Cascader; the theme forwards it to the option items. The
    /// registered default is <see langword="null" />; the theme then supplies a spinning loading icon.
    /// </summary>
    public IconTemplate? LoadingIcon
    {
        get => GetValue(LoadingIconProperty);
        set => SetValue(LoadingIconProperty, value);
    }
    
    /// <summary>
    /// Gets or sets what expands an option that has children into the next level: clicking it
    /// (<see cref="CascaderViewExpandTrigger.Click" />, the default) or resting the pointer on it
    /// (<see cref="CascaderViewExpandTrigger.Hover" />). Set it on the Cascader; the theme forwards it to the
    /// drop-down view.
    /// </summary>
    public CascaderViewExpandTrigger ExpandTrigger
    {
        get => GetValue(ExpandTriggerProperty);
        set => SetValue(ExpandTriggerProperty, value);
    }

    /// <summary>
    /// Gets or sets the loader that supplies the children of an option on demand when the option is expanded.
    /// Set it on the Cascader; the theme forwards it to the drop-down view. The default is
    /// <see langword="null" />, which turns loading on demand off; only the children already present in the
    /// option tree are shown.
    /// </summary>
    public ICascaderItemDataLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the filter that decides which options stay visible while text is typed in the search box.
    /// Set it on the Cascader; the theme forwards it to the drop-down view. If the value is still
    /// <see langword="null" /> when the control is initialized, a
    /// <see cref="ValueFilterMode.Contains" /> filter is set as the current value, which leaves a binding in
    /// place. A <see langword="null" /> set afterwards disables filtering.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how the text that matched the search is emphasized in the option paths of the filtered list.
    /// It is a combination of the <see cref="TextBlockHighlightStrategy" /> flags; the default is <c>All</c>.
    /// Set it on the Cascader; the theme forwards it to the drop-down view.
    /// </summary>
    public TextBlockHighlightStrategy FilterHighlightStrategy
    {
        get => GetValue(FilterHighlightStrategyProperty);
        set => SetValue(FilterHighlightStrategyProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the brush of the highlighted match in the option paths of the filtered list. Set it on the
    /// Cascader; the theme forwards it to the drop-down view. The registered default is
    /// <see langword="null" />; the theme then supplies the <c>FilterHighlightColor</c> token of the Cascader.
    /// </summary>
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the option path that the drop-down expands along by default and that is shown as selected
    /// while nothing is selected in single-selection mode. Each segment names the option of one level by its
    /// <c>ItemKey</c> or <c>Value</c>. Showing the path sets only the text of the selection box:
    /// <see cref="SelectedOption" /> stays <see langword="null" /> until an option is picked or assigned.
    /// </summary>
    public TreeNodePath? DefaultSelectOptionPath
    {
        get => GetValue(DefaultSelectOptionPathProperty);
        set => SetValue(DefaultSelectOptionPathProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether an option that has children can be selected as a single value. When
    /// <see langword="true" />, clicking such an option selects it and double-clicking it closes the drop-down.
    /// When <see langword="false" /> (the default), only options without children can be selected. Set it on the
    /// Cascader; the theme forwards it to the drop-down view.
    /// </summary>
    public bool IsAllowSelectParent
    {
        get => GetValue(IsAllowSelectParentProperty);
        set => SetValue(IsAllowSelectParentProperty, value);
    }
    
    /// <summary>
    /// Gets an <see cref="ItemsSourceView" /> over the <see cref="Options" /> collection, created on demand.
    /// </summary>
    public ItemsSourceView OptionsView => ItemsSourceView.GetOrCreate(_options);
    
    /// <summary>
    /// Gets the options offered in the drop-down. Declare them as XAML content of the Cascader or add them here;
    /// each item must implement <see cref="ICascaderOption" />, and the children of an option form the next level.
    /// Setting <see cref="OptionsSource" /> replaces the items of this collection.
    /// </summary>
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

    /// <summary>
    /// Sets a default <see cref="ValueFilterMode.Contains" /> filter when <see cref="Filter" /> is still
    /// <see langword="null" />, without replacing a binding. The base member is called first.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }
    }

    /// <summary>
    /// Starts tracking changes of the <see cref="SelectedOptions" /> collection and refreshes the state derived
    /// from the selection. The base member is called first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureSelectedOptionsCollectionChangedSource(SelectedOptions);
        ConfigureSelectedOptionsValueState();
    }

    /// <summary>
    /// Stops tracking the <see cref="SelectedOptions" /> collection and clears the flag that suppresses
    /// auto-repeated Enter presses. The base member is called last.
    /// </summary>
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
    
    /// <summary>
    /// Clears the selection in either mode: sets <see cref="SelectedOption" />, <see cref="SelectedOptions" /> and
    /// the path shown in the selection box to <see langword="null" />. It does not restore a
    /// <see cref="DefaultSelectOptionPath" />.
    /// </summary>
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
    
    /// <summary>
    /// Handles the keys that act on the drop-down: Down, Enter and Space open it, Escape closes it and F4 toggles
    /// it. While it is open, keys are passed to the drop-down view first. The base member is called first.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        HandleKeyDownCore(e);
    }

    /// <summary>
    /// Clears the flag that suppresses auto-repeated Enter presses, here and in the drop-down view, when keyboard
    /// focus leaves the control as a whole. The base member is called first.
    /// </summary>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (!IsKeyboardFocusWithin)
        {
            _isEnterKeyPressed = false;
            _cascaderView?.HandleKeyUp(new KeyEventArgs { Key = Key.Enter });
        }
    }

    /// <summary>
    /// Clears the flag that suppresses auto-repeated Enter presses when Enter is released, and passes every key
    /// release to the drop-down view, which clears its own flag. The base member is called last.
    /// </summary>
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
    
    /// <summary>
    /// Keeps the control in step with property changes: refreshes the search box and the popup width, the state
    /// derived from the selection and from <see cref="IsMultiple" /> (placeholder, shown path, tags, selected
    /// count), and shows a changed <see cref="DefaultSelectOptionPath" /> while nothing is selected. The base
    /// member is called first.
    /// </summary>
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
    
    /// <summary>
    /// Binds the template parts <c>PART_SingleFilterInput</c> and <c>PART_CascaderView</c>: releases the previous
    /// drop-down view, hooks the events of the new one, gives it <see cref="Options" /> and the selection, and
    /// refreshes the state derived from the selection. The base member is called first.
    /// </summary>
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
    
    /// <summary>
    /// Runs after the drop-down has closed. In single-selection mode it clears the search box and the filter
    /// value, lets the search box size itself again and resets the interaction state of the drop-down view. The
    /// base member is called last.
    /// </summary>
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

    /// <summary>
    /// Runs after the drop-down has opened. In single-selection mode it restores the selected option in the
    /// drop-down view and moves keyboard focus to the search box. The base member is called first.
    /// </summary>
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
    
    /// <summary>
    /// Marks a press inside the open drop-down popup as handled. When the drop-down is closed, it sets the
    /// <c>:pressed</c> pseudo-class; <see cref="OnPointerReleased" /> toggles the drop-down only when that
    /// pseudo-class is set. The base member is called first.
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
    
    /// <summary>
    /// Toggles the drop-down when the release ends a press that started on the closed control and lands outside
    /// the drop-down popup, unless it lands on the close button of a selected tag. In every case it clears the
    /// <c>:pressed</c> pseudo-class. A release inside the open drop-down popup is marked handled. The base member
    /// is called last.
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

    /// <summary>
    /// Shows the <see cref="DefaultSelectOptionPath" /> while nothing is selected. The base member is called first.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ApplyDefaultSelectOptionPath();
    }
    
    #region 实现 FormItem 接口
    
    /// <summary>
    /// Assigns a value received from a FormItem: an <see cref="ICascaderOption" /> becomes
    /// <see cref="SelectedOption" /> in single-selection mode, and an <c>IList&lt;ICascaderOption&gt;</c> becomes
    /// <see cref="SelectedOptions" /> in multiple-selection mode. Any other value clears the selection of the
    /// current mode.
    /// </summary>
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
    
    /// <summary>
    /// Returns the value that a FormItem reads: <see cref="SelectedOption" /> in single-selection mode and
    /// <see cref="SelectedOptions" /> in multiple-selection mode.
    /// </summary>
    protected override object? NotifyGetFormValue()
    {
        if (!IsMultiple)
        {
            return SelectedOption;
        }
        return SelectedOptions;
    }
    
    /// <summary>
    /// Clears the value that a FormItem resets: sets <see cref="SelectedOption" /> in single-selection mode and
    /// <see cref="SelectedOptions" /> in multiple-selection mode to <see langword="null" />.
    /// </summary>
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
