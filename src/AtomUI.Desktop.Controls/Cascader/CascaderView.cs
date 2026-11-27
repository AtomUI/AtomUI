using System.Collections;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.DataLoad;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using ItemCollection = AtomUI.Collections.ItemCollection;

/// <summary>
/// The user action that opens an option with children in a <see cref="CascaderView" />.
/// </summary>
public enum CascaderViewExpandTrigger
{
    /// <summary>
    /// The option opens when the user clicks it. This is the default.
    /// </summary>
    Click,
    /// <summary>
    /// The option opens as soon as the pointer rests on it; no click is needed.
    /// </summary>
    Hover
}

/// <summary>
/// Lists options level by level: opening an option with children adds its children as a further list beside the
/// earlier levels. One option is selected through <see cref="SelectedOption" />, or several are checked through
/// <see cref="SelectedOptions" /> while <see cref="IsCheckable" /> is set. While a filter is active, the lists are
/// replaced by the matching option paths.
/// </summary>
/// <remarks>
/// <see cref="Cascader" /> hosts a view in its dropdown and forwards its properties to it. A view that is placed
/// on its own takes its options from <see cref="Options" /> or <see cref="OptionsSource" />.
/// </remarks>
public partial class CascaderView : TemplatedControl, 
                                    IMotionAwareControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="OptionsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<ICascaderOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<CascaderView, IEnumerable<ICascaderOption>?>(nameof(OptionsSource));
    
    /// <summary>
    /// Identifies the <see cref="OptionTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<CascaderView, IDataTemplate?>(nameof(OptionTemplate));
    
    /// <summary>
    /// Identifies the <see cref="ExpandIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> ExpandIconProperty =
        AvaloniaProperty.Register<CascaderView, IconTemplate?>(nameof(ExpandIcon));
    
    /// <summary>
    /// Identifies the <see cref="LoadingIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> LoadingIconProperty =
        AvaloniaProperty.Register<CascaderView, IconTemplate?>(nameof(LoadingIcon));
    
    /// <summary>
    /// Identifies the <see cref="ExpandTrigger" /> property.
    /// </summary>
    public static readonly StyledProperty<CascaderViewExpandTrigger> ExpandTriggerProperty =
        AvaloniaProperty.Register<CascaderView, CascaderViewExpandTrigger>(nameof(ExpandTrigger), CascaderViewExpandTrigger.Click);
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CascaderView>();
    
    /// <summary>
    /// Identifies the <see cref="IsCheckable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCheckableProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsCheckable));
    
    /// <summary>
    /// Identifies the <see cref="DefaultExpandedPath" /> property.
    /// </summary>
    public static readonly StyledProperty<TreeNodePath?> DefaultExpandedPathProperty =
        AvaloniaProperty.Register<CascaderView, TreeNodePath?>(nameof(DefaultExpandedPath));
    
    /// <summary>
    /// Identifies the <see cref="DataLoader" /> property.
    /// </summary>
    public static readonly StyledProperty<ICascaderItemDataLoader?> DataLoaderProperty =
        AvaloniaProperty.Register<CascaderView, ICascaderItemDataLoader?>(nameof(DataLoader));
    
    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<CascaderView, IValueFilter?>(nameof(Filter));

    /// <summary>
    /// Identifies the <see cref="FilterValue" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<CascaderView, object?>(nameof(FilterValue));

    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<CascaderView, DefaultFilterValueSelector?>(nameof(FilterValueSelector));
    
    /// <summary>
    /// Identifies the <see cref="FilterHighlightStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<TextBlockHighlightStrategy> FilterHighlightStrategyProperty =
        AvaloniaProperty.Register<CascaderView, TextBlockHighlightStrategy>(nameof(FilterHighlightStrategy), TextBlockHighlightStrategy.All);
    
    /// <summary>
    /// Identifies the <see cref="FilterResultCount" /> property.
    /// </summary>
    public static readonly DirectProperty<CascaderView, int> FilterResultCountProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, int>(nameof(FilterResultCount),
            o => o.FilterResultCount);
    
    /// <summary>
    /// Identifies the <see cref="FilterHighlightForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<CascaderView, IBrush?>(nameof(FilterHighlightForeground));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<CascaderView, object?>(nameof(EmptyIndicator));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<CascaderView, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsShowEmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsShowEmptyIndicator), true);
    
    /// <summary>
    /// Identifies the <see cref="IsAllowSelectParent" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowSelectParentProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsAllowSelectParent));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<CascaderView, Thickness>(nameof(EmptyIndicatorPadding));
    
    /// <summary>
    /// Identifies the <see cref="SelectedOption" /> property.
    /// </summary>
    public static readonly DirectProperty<CascaderView, ICascaderOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, ICascaderOption?>(
            nameof(SelectedOption),
            o => o.SelectedOption,
            (o, v) => o.SelectedOption = v);
    
    /// <summary>
    /// Identifies the <see cref="SelectedOptions" /> property.
    /// </summary>
    public static readonly DirectProperty<CascaderView, IList<ICascaderOption>?> SelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, IList<ICascaderOption>?>(
            nameof(SelectedOptions),
            o => o.SelectedOptions,
            (o, v) => o.SelectedOptions = v);
    
    /// <summary>
    /// The root options; each option provides the next level through its children. Assigning it replaces the items of
    /// <see cref="Options" />, closes the open levels and, once the view is loaded, applies a set
    /// <see cref="DefaultExpandedPath" /> again. <see langword="null" /> or an empty source leaves the view without
    /// options.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Options" /> already holds items that were added directly.
    /// </exception>
    public IEnumerable<ICascaderOption>? OptionsSource
    {
        get => GetValue(OptionsSourceProperty);
        set => SetValue(OptionsSourceProperty, value);
    }
    
    /// <summary>
    /// The template that presents one option in every level. Its <c>DataType</c> is taken from the items of
    /// <see cref="OptionsSource" />. When left unset, the theme default is used, which shows the header of the option
    /// as text.
    /// </summary>
    [InheritDataTypeFromItems("OptionsSource")]
    public IDataTemplate? OptionTemplate
    {
        get => GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }

    /// <summary>
    /// The expand icon shown by options that have children, in every level; the value is forwarded to every option
    /// item. When left unset, the theme default is used, a right arrow.
    /// </summary>
    public IconTemplate? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }
    
    /// <summary>
    /// The icon an option item shows while <see cref="DataLoader" /> loads its children. When left unset, the theme
    /// default is used, a spinning loading icon.
    /// </summary>
    public IconTemplate? LoadingIcon
    {
        get => GetValue(LoadingIconProperty);
        set => SetValue(LoadingIconProperty, value);
    }
    
    /// <summary>
    /// How an option with children is opened: by clicking it (<see cref="CascaderViewExpandTrigger.Click" />, the
    /// default) or by resting the pointer on it (<see cref="CascaderViewExpandTrigger.Hover" />).
    /// </summary>
    public CascaderViewExpandTrigger ExpandTrigger
    {
        get => GetValue(ExpandTriggerProperty);
        set => SetValue(ExpandTriggerProperty, value);
    }
    
    /// <summary>
    /// Controls whether state changes of the option items are animated, such as the color change of a selection
    /// highlight; the value is forwarded to every item. When left unset, it takes the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Switches to a multiple selection: the option items get check boxes, and the checked options are reported
    /// through <see cref="SelectedOptions" /> instead of the single <see cref="SelectedOption" />. The default is
    /// <see langword="false" />, which selects one option at a time.
    /// </summary>
    public bool IsCheckable
    {
        get => GetValue(IsCheckableProperty);
        set => SetValue(IsCheckableProperty, value);
    }
    
    /// <summary>
    /// The option that is expanded, together with its ancestors, the first time the view loads; the option is not
    /// selected. Each segment of the path matches an option by its <c>ItemKey</c> or by the string form of its value.
    /// <see langword="null" /> (the default) expands nothing. The path is applied again after
    /// <see cref="OptionsSource" /> changes while the view is loaded; a later change of the path alone has no effect.
    /// </summary>
    public TreeNodePath? DefaultExpandedPath
    {
        get => GetValue(DefaultExpandedPathProperty);
        set => SetValue(DefaultExpandedPathProperty, value);
    }
    
    /// <summary>
    /// Loads the children of an option on demand when the option is being expanded, has no children and is not a
    /// leaf. <see langword="null" /> (the default) turns loading off, so such an option stays without children. A load
    /// is bounded by <see cref="AsyncLoadTimeout" />, and <see cref="ItemAsyncLoaded" /> reports its outcome.
    /// </summary>
    public ICascaderItemDataLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }
    
    /// <summary>
    /// The filter that decides whether an option path matches <see cref="FilterValue" /> while a filter is active.
    /// Changing it runs the filter again. When it is <see langword="null" /> when the view initializes, a
    /// <see cref="ValueFilterMode.Contains" /> filter is set as the current value.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    /// <summary>
    /// The value the options are matched against. Any value other than <see langword="null" /> or a blank string
    /// activates the filter, which replaces the level lists with the matching option paths; clearing it shows the
    /// level lists again. The value also highlights the matched text in the result list.
    /// </summary>
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    /// <summary>
    /// Maps each candidate option path to the value that <see cref="Filter" /> matches against. When it is
    /// <see langword="null" /> when the view initializes, the default selector is set as the current value; the
    /// default returns the path text, i.e. the headers from the root option down to the option joined with "/".
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    /// <summary>
    /// A combination of the <c>TextBlockHighlightStrategy</c> flags that selects how matched text is emphasized in a
    /// filter result. The default is <c>All</c>. The theme of the view does not apply this value to its result list,
    /// so changing it only runs the filter again.
    /// </summary>
    public TextBlockHighlightStrategy FilterHighlightStrategy
    {
        get => GetValue(FilterHighlightStrategyProperty);
        set => SetValue(FilterHighlightStrategyProperty, value);
    }
    
    private int _filterResultCount;
    
    /// <summary>
    /// The number of option paths in the current filter result, or 0 when no filter is active. The view writes it
    /// after every filter run; a value set from outside only feeds the empty indicator state.
    /// </summary>
    public int FilterResultCount
    {
        get => _filterResultCount;
        set => SetAndRaise(FilterResultCountProperty, ref _filterResultCount, value);
    }
    
    /// <summary>
    /// The foreground brush of the matched text in the filter result list. When left unset, it takes the
    /// <c>FilterHighlightColor</c> token.
    /// </summary>
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    /// <summary>
    /// The content shown in place of the level lists when the view has no options. It is presented with
    /// <see cref="EmptyIndicatorTemplate" />, or by the default presenter when that is <see langword="null" />. While
    /// this and the template are both <see langword="null" />, the built-in empty indicator is shown instead.
    /// </summary>
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    /// <summary>
    /// The template that presents <see cref="EmptyIndicator" />. While it and <see cref="EmptyIndicator" /> are both
    /// <see langword="null" />, the built-in empty indicator is shown instead.
    /// </summary>
    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    /// <summary>
    /// Controls whether an empty indicator is shown at all when the view has no options, the custom one or the
    /// built-in one. The default is <see langword="true" />; <see langword="false" /> leaves the view blank.
    /// </summary>
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    /// <summary>
    /// Controls whether an option with children can be selected. When <see langword="false" /> (the default), only a
    /// leaf option can be picked: <see cref="OptionSelected" /> is raised and <see cref="SelectedOption" /> changes
    /// only for a leaf, and clicking an option with children only opens it. When <see langword="true" />, an option
    /// with children can be picked as well.
    /// </summary>
    public bool IsAllowSelectParent
    {
        get => GetValue(IsAllowSelectParentProperty);
        set => SetValue(IsAllowSelectParentProperty, value);
    }
    
    /// <summary>
    /// The padding around the custom empty indicator content, and the margin around the built-in indicator. When left
    /// unset, it takes the <c>Padding</c> token of the view.
    /// </summary>
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }
    
    private ICascaderOption? _selectedOption;
    
    /// <summary>
    /// The option selected in the view, or <see langword="null" /> when none is. After an option is assigned, the view
    /// expands the option's path and selects the option in its level without raising
    /// <see cref="OptionSelected" />; this happens on a later dispatcher pass, and again when the view loads.
    /// Assigning <see langword="null" /> collapses every level.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The option has children while <see cref="IsAllowSelectParent" /> is <see langword="false" />. Thrown at once
    /// when the view is loaded, otherwise when it loads.
    /// </exception>
    public ICascaderOption? SelectedOption
    {
        get => _selectedOption;
        set => SetAndRaise(SelectedOptionProperty, ref _selectedOption, value);
    }
    
    private IList<ICascaderOption>? _checkedOptions;
    
    /// <summary>
    /// The options that are checked while <see cref="IsCheckable" /> is <see langword="true" />. Assigning a list
    /// makes the view check and uncheck its items to match it; assigning <see langword="null" /> clears the selection.
    /// The view replaces the list with a new instance after each check or uncheck made through the UI, so read the
    /// property again instead of keeping a reference to the list.
    /// </summary>
    public IList<ICascaderOption>? SelectedOptions
    {
        get => _checkedOptions;
        set => SetAndRaise(SelectedOptionsProperty, ref _checkedOptions, value);
    }
    
    /// <summary>
    /// Gets an <see cref="ItemsSourceView" /> over the <see cref="Options" /> collection, created on demand.
    /// </summary>
    public ItemsSourceView OptionsView => ItemsSourceView.GetOrCreate(_options);
    
    /// <summary>
    /// The root options of the view, which the XAML content of the view is added to. Assigning
    /// <see cref="OptionsSource" /> replaces these items; <see cref="OptionsView" /> wraps the same collection.
    /// </summary>
    [Content]
    public ItemCollection Options => _options;
    
    #endregion
    
    #region 公共事件定义
    
    /// <summary>
    /// Raised after <see cref="SelectedOptions" /> was replaced with a different list, both when the property was
    /// assigned and when the view replaced the list after a check or uncheck made through the UI. The event data
    /// carries the previous and the new list. For an assignment from outside, the items are updated to match the
    /// new list after the event. Modifying the current list in place raises nothing.
    /// </summary>
    public event EventHandler<CascaderOptionsSelectedChangedEventArgs>? SelectedOptionsChanged;
    /// <summary>
    /// Raised when a load started through <see cref="DataLoader" /> for an expanded option has ended. The event data
    /// carries the item of the option and a <c>CascaderItemLoadResult</c> that states the outcome: the loaded
    /// children, a failure, or a timeout after <see cref="AsyncLoadTimeout" />.
    /// </summary>
    public event EventHandler<CascaderViewItemLoadedEventArgs>? ItemAsyncLoaded;
    /// <summary>
    /// Raised after the view expanded an option together with its ancestors and the option's item exists. This
    /// happens when <see cref="SelectedOption" /> or <see cref="DefaultExpandedPath" /> is presented, when an option
    /// becomes the keyboard candidate, and when the pointer opens an option with
    /// <see cref="CascaderViewExpandTrigger.Hover" />. The event data carries the item of the expanded option.
    /// </summary>
    public event EventHandler<CascaderItemExpandedEventArgs>? ItemExpanded;
    /// <summary>
    /// Raised after the view collapsed an option, which also closes the levels below it. The event data carries the
    /// item of the option. <see cref="CollapseAll" /> closes every level without raising it.
    /// </summary>
    public event EventHandler<CascaderItemCollapsedEventArgs>? ItemCollapsed;
    /// <summary>
    /// Raised when the user clicks an option item, before the view reacts to the click; a checkable leaf item is
    /// toggled afterwards. The event data carries the clicked item.
    /// </summary>
    public event EventHandler<CascaderItemClickedEventArgs>? ItemClicked;
    /// <summary>
    /// Raised when the user double-taps inside an option item. The event data carries that item.
    /// </summary>
    public event EventHandler<CascaderItemDoubleClickedEventArgs>? ItemDoubleClicked;
    /// <summary>
    /// Raised when an option is picked through the view, after the pick was stored in
    /// <see cref="SelectedOption" />: when clicking an option item selects it, or when Enter commits the keyboard
    /// candidate while <see cref="IsCheckable" /> is <see langword="false" />. Assigning
    /// <see cref="SelectedOption" /> from code does not raise it.
    /// </summary>
    public event EventHandler<CascaderOptionSelectedEventArgs>? OptionSelected;
    #endregion

    #region 内部属性定义
    internal static readonly DirectProperty<CascaderView, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);

    internal static readonly DirectProperty<CascaderView, bool> IsDefaultEmptyIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, bool>(
            nameof(IsDefaultEmptyIndicatorVisible),
            o => o.IsDefaultEmptyIndicatorVisible,
            (o, v) => o.IsDefaultEmptyIndicatorVisible = v);
    
    internal static readonly DirectProperty<CascaderView, ItemToggleType> EffectiveToggleTypeProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, ItemToggleType>(
            nameof(EffectiveToggleType),
            o => o.EffectiveToggleType,
            (o, v) => o.EffectiveToggleType = v);
    
    internal static readonly DirectProperty<CascaderView, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    private bool _isEffectiveEmptyVisible = false;
    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }

    private bool _isDefaultEmptyIndicatorVisible;
    internal bool IsDefaultEmptyIndicatorVisible
    {
        get => _isDefaultEmptyIndicatorVisible;
        set => SetAndRaise(IsDefaultEmptyIndicatorVisibleProperty, ref _isDefaultEmptyIndicatorVisible, value);
    }
    
    private ItemToggleType _effectiveToggleType = ItemToggleType.None;
    internal ItemToggleType EffectiveToggleType
    {
        get => _effectiveToggleType;
        set => SetAndRaise(EffectiveToggleTypeProperty, ref _effectiveToggleType, value);
    }

    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }
    
    #endregion
    
    #region 内部协作 API

    internal event EventHandler? CloseRequested;


    internal void HandleKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _isEnterKeyPressed = false;
        }
    }

    internal static bool IsComposingKeyInput(object? source)
    {
        return source is Avalonia.Controls.TextBox input &&
               input.GetVisualDescendants().OfType<TextPresenter>()
                    .Any(presenter => !string.IsNullOrEmpty(presenter.PreeditText));
    }

    internal void UpdateKeyboardCandidateContainer(CascaderViewItem item)
    {
        item.SetCurrentValue(CascaderViewItem.IsCandidateSelectedProperty,
            !IsFiltering && ReferenceEquals(item.AttachedOption, _keyboardCandidateOption));
    }

    internal void ResetInteractionState()
    {
        CancelPendingSelectedOptionPresentation();
        SetKeyboardCandidate(null);
        _filterList?.ClearCandidate();
        CollapseAll();
    }

    internal void RestoreSelectedOptionPath()
    {
        if (SelectedOption is { } option)
        {
            SelectTargetOption(option);
            SetKeyboardCandidateOption(IsOptionEnabled(option) ? option : null);
        }
    }

    internal bool TryParseSelectPath(TreeNodePath path, out IList<ICascaderOption> pathNodes)
    {
        var                    segments     = path.Segments;
        var                    count        = path.Segments.Count;
        var                    isPathValid  = true;
        IEnumerable<ICascaderOption> currentItems = BuildOptionList(_options);

        var                          options    = new List<ICascaderOption>(count);
        for (var i = 0; i < count; i++)
        {
            var segment = segments[i];
            var found   = false;
            foreach (var currentItem in currentItems)
            {
                if (segment == currentItem.ItemKey || segment == currentItem.Value?.ToString())
                {
                    options.Add(currentItem);
                    currentItems = currentItem.Children;
                    found        = true;
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

    #endregion

    #region 内部协作 API

    internal static readonly DefaultFilterValueSelector DefaultCascaderFilterValueSelector = value =>
    {
        if (value is ICascaderItemInfo info)
        {
            return info.Path;
        }
        return null;
    };

    #endregion

    private readonly ItemCollection _options = new();
    private bool _ignoreSyncSelectedOptions;
    private StackPanel? _itemsPanel;
    private int _ignoreExpandAndCollapseLevel;
    private bool _defaultExpandPathApplied;
    private bool _isSynchronizingSelectedOptionToView;
    private CascaderViewLevelList? _rootLevelList;
    private ICascaderOption? _keyboardCandidateOption;
    private bool _isEnterKeyPressed;
    private DispatcherOperation? _pendingSelectedOptionPresentation;
    
    static CascaderView()
    {
        FocusableProperty.OverrideDefaultValue<CascaderView>(true);
        SetupExpandAndCollapse();
        SetupChecked();
        CascaderViewItem.DoubleTappedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemDoubleClicked(args));
        CascaderViewItem.ClickedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemClicked(args));
        CascaderViewItem.SelectedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderOptionSelected(args));
        OptionsSourceProperty.Changed.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderSourceChanged(args));
    }
    
    public CascaderView()
    {
        _options.CollectionChanged += HandleCollectionChanged;
    }
    
    /// <summary>
    /// Calls the base member first. While <see cref="Filter" /> and <see cref="FilterValueSelector" /> are still
    /// <see langword="null" />, sets a default for each. Then refreshes the empty indicator and the toggle type of
    /// the option items.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }
        if (FilterValueSelector == null)
        {
            SetCurrentValue(FilterValueSelectorProperty, DefaultCascaderFilterValueSelector);
        }
        ConfigureEmptyIndicator();
        ConfigureEffectiveToggleType();
    }
    
    /// <summary>
    /// Resolves the template parts after the base member ran: <c>PART_ItemsPanel</c>, which hosts the level lists;
    /// <c>PART_RootLevelList</c>, which becomes the first level owned by this view; and <c>PART_FilterList</c>, whose
    /// candidate is cleared and whose selection handler is moved over from the previous list. A part missing from the
    /// template stays <see langword="null" />.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsPanel    = e.NameScope.Find<StackPanel>("PART_ItemsPanel");
        _rootLevelList = e.NameScope.Find<CascaderViewLevelList>("PART_RootLevelList");
        if (_rootLevelList != null)
        {
            _rootLevelList.Level     = 1;
            _rootLevelList.OwnerView = this;
        }
        
        if (_filterList != null)
        {
            _filterList.SelectionChanged -= HandleFilterListSelectionChanged;
        }
        _filterList = e.NameScope.Find<CascaderViewFilterList>("PART_FilterList");
        
        if (_filterList != null)
        {
            _filterList.ClearCandidate();
            _filterList.SelectionChanged += HandleFilterListSelectionChanged;
        }
    }

    /// <summary>
    /// Calls the base member first. On every load it runs the filter; on the first load it also applies
    /// <see cref="DefaultExpandedPath" />. Then it presents <see cref="SelectedOption" />: the option's path is
    /// expanded, the option is selected in its level, and it becomes the keyboard candidate when it is enabled.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        FilterItems();
        if (DefaultExpandedPath != null && !_defaultExpandPathApplied)
        {
            ApplyDefaultExpandPath();
            _defaultExpandPathApplied = true;
        }
        
        RestoreSelectedOptionPath();
    }

        
    
    

    private static List<ICascaderOption> BuildOptionList(IEnumerable source)
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
    
    private void ApplyDefaultExpandPath()
    {
        Debug.Assert(DefaultExpandedPath != null);
        if (TryParseSelectPath(DefaultExpandedPath, out IList<ICascaderOption> pathNodes))
        {
            if (pathNodes.Count > 0)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    await ExpandItemAsync(pathNodes[^1]);
                });
            }
        }
    }

    private void HandleCascaderSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        CancelPendingSelectedOptionPresentation();
        SetKeyboardCandidateOption(null);
        _filterList?.ClearCandidate();
        _allPathInfos = null;
        _asyncLoadCoordinator.CancelAll();
        ResetLevelListsForOptionsSourceChanging();
        _options.SetItemsSource(args.GetNewValue<IEnumerable<ICascaderOption>?>());
        RestoreRootLevelListItemsSource();
        if (DefaultExpandedPath != null && IsLoaded)
        {
            _defaultExpandPathApplied = false;
            ApplyDefaultExpandPath();
            _defaultExpandPathApplied = true;
        }
    }

    private void ResetLevelListsForOptionsSourceChanging()
    {
        if (_itemsPanel == null)
        {
            return;
        }

        for (var i = _itemsPanel.Children.Count - 1; i >= 0; i--)
        {
            if (_itemsPanel.Children[i] is CascaderViewLevelList levelList)
            {
                levelList.ResetVirtualizingContext();
                levelList.SelectedIndex = -1;
                levelList.SelectedItem = null;
                levelList.ItemsSource  = null;
                if (i > 0)
                {
                    levelList.Items.Clear();
                    _itemsPanel.Children.RemoveAt(i);
                }
                levelList.ResetVirtualizingContext();
            }
        }
    }

    private void RestoreRootLevelListItemsSource()
    {
        if (_rootLevelList == null)
        {
            return;
        }

        _rootLevelList.ItemsSource = Options;
    }
    
    /// <summary>
    /// Calls the base member first, then keeps the view in step with the changed property: runs the filter again for
    /// <see cref="Filter" />, <see cref="FilterValue" />, <see cref="FilterHighlightStrategy" />,
    /// <see cref="OptionsSource" />, <see cref="IsCheckable" /> and <see cref="IsAllowSelectParent" />; refreshes the
    /// empty indicator for <see cref="IsShowEmptyIndicator" />, <see cref="OptionsSource" />,
    /// <see cref="FilterResultCount" />, <see cref="EmptyIndicator" /> and <see cref="EmptyIndicatorTemplate" />;
    /// recalculates the item toggle type for <see cref="IsCheckable" /> and <see cref="IsAllowSelectParent" />;
    /// updates the loading state of the items from <see cref="DataLoader" />; and presents a changed
    /// <see cref="SelectedOption" />, or collapses every level when it is cleared.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
                
        if (change.Property == DataLoaderProperty)
        {
            HasItemAsyncDataLoader = DataLoader != null;
        }
        else if (change.Property == FilterProperty ||
                 change.Property == FilterHighlightStrategyProperty ||
                 change.Property == OptionsSourceProperty ||
                 change.Property == FilterValueProperty)
        {
            FilterItems();
        }
    
        if (change.Property == IsShowEmptyIndicatorProperty ||
            change.Property == OptionsSourceProperty ||
            change.Property == FilterResultCountProperty ||
            change.Property == EmptyIndicatorProperty ||
            change.Property == EmptyIndicatorTemplateProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == IsCheckableProperty || change.Property == IsAllowSelectParentProperty)
        {
            ConfigureEffectiveToggleType();
            _allPathInfos = null;
            FilterItems();
        }
        else if (change.Property == SelectedOptionProperty)
        {
            if (SelectedOption != null)
            {
                if (IsLoaded)
                {
                    SelectTargetOption(SelectedOption);
                }
            }
            else
            {
                CancelPendingSelectedOptionPresentation();
                CollapseAll();
            }
        }
    }
    
    private void SelectTargetOption(ICascaderOption option)
    {
        var isLeaf = option.IsLeaf || !option.HasChildren();
    
        if (!isLeaf && !IsAllowSelectParent)
        {
            throw new ArgumentException($"Option {option.Header} is not a Leaf node.");
        }
    
        CancelPendingSelectedOptionPresentation();
        _pendingSelectedOptionPresentation = Dispatcher.InvokeAsync<Task>(async () =>
        {
            await ExpandItemAsync(option);
            var targetLevelList = GetLevelListForOption(option);
            if (targetLevelList != null)
            {
                try
                {
                    _isSynchronizingSelectedOptionToView = true;
                    targetLevelList.SelectedItem          = option;
                }
                finally
                {
                    _isSynchronizingSelectedOptionToView = false;
                }
            }
        });
    }

    private void CancelPendingSelectedOptionPresentation()
    {
        _pendingSelectedOptionPresentation?.Abort();
        _pendingSelectedOptionPresentation = null;
    }

    private void SelectOptionFromInteraction(ICascaderOption option)
    {
        SetCurrentValue(SelectedOptionProperty, option);
        OptionSelected?.Invoke(this, new CascaderOptionSelectedEventArgs(option));
    }
    
    private void HandleCascaderItemClicked(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            NotifyCascaderItemClicked(item);
            ItemClicked?.Invoke(this, new CascaderItemClickedEventArgs(item));
            if (IsCheckable)
            {
                if (item.IsLeaf && item.IsCheckBoxEnabled)
                {
                    if (item.IsChecked == true)
                    {
                        item.IsChecked = false;
                    }
                    else
                    {
                        item.IsChecked = true;
                    }
                }
            }
        }
    }

    private void HandleCascaderOptionSelected(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            if (_isSynchronizingSelectedOptionToView)
            {
                return;
            }
            if (item.IsSelected)
            {
                var option = item.AttachedOption!;
                SelectOptionFromInteraction(option);
            }
        }
    }

    /// <summary>
    /// Called when an option item was clicked, before <see cref="ItemClicked" /> is raised and before a checkable leaf
    /// item is toggled. The default implementation does nothing.
    /// </summary>
    /// <param name="item">
    /// The clicked item.
    /// </param>
    protected virtual void NotifyCascaderItemClicked(CascaderViewItem item)
    {
    }
    
    /// <summary>
    /// Recomputes the two states that drive the empty indicator: <c>IsEffectiveEmptyVisible</c>, which is true when
    /// <see cref="IsShowEmptyIndicator" /> is set and the view is empty, and <c>IsDefaultEmptyIndicatorVisible</c>,
    /// which is true then while <see cref="EmptyIndicator" /> and <see cref="EmptyIndicatorTemplate" /> are both
    /// <see langword="null" />. The view counts as empty while the filter has no result, and otherwise when it has no
    /// root options. An override must call the base member to keep both states current.
    /// </summary>
    protected virtual void ConfigureEmptyIndicator()
    {
        var isEmpty = false;
        if (IsFiltering)
        {
            isEmpty = FilterResultCount == 0;
        }
        else
        {
            if (OptionsSource != null)
            {
                var enumerator = OptionsSource.GetEnumerator();
                isEmpty = !enumerator.MoveNext();
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            else
            {
                isEmpty = _options.Count == 0;
            }
        }
        IsEffectiveEmptyVisible        = IsShowEmptyIndicator && isEmpty;
        IsDefaultEmptyIndicatorVisible = IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null;
    }
    
    private void HandleCascaderItemDoubleClicked(RoutedEventArgs args)
    {
        if (args.Source is Control source)
        {
            var cascaderItem = source.FindAncestorOfType<CascaderViewItem>();
            if (cascaderItem != null)
            {
                ItemDoubleClicked?.Invoke(this, new CascaderItemDoubleClickedEventArgs(cascaderItem));
            }
        }
    }

    private void ConfigureEffectiveToggleType()
    {
        if (IsCheckable)
        {
            EffectiveToggleType = ItemToggleType.CheckBox;
        }
        else
        {
            EffectiveToggleType = ItemToggleType.None;
        }
    }
    
    private CascaderViewItem? GetContainerFromEventSource(object? eventSource)
    {
        for (var current = eventSource as Visual; current != null; current = current.GetVisualParent())
        {
            if (current is CascaderViewItem cascaderViewItem)
            {
                return cascaderViewItem;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Calls the base member first, then handles the pointer over an option item: a mouse pointer makes the item under
    /// it the keyboard candidate when that item is visible, enabled and not loading; with
    /// <see cref="CascaderViewExpandTrigger.Hover" /> an item that is not expanded yet is expanded, and resting on an
    /// expanded item clears the expanded state of its descendants.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Pointer.Type == PointerType.Mouse &&
            GetContainerFromEventSource(e.Source) is { } activeCandidate &&
            activeCandidate.IsVisible && activeCandidate.IsEnabled && !activeCandidate.IsLoading)
        {
            SetKeyboardCandidate(activeCandidate);
        }

        if (e.Source is Visual source)
        {
            if (ExpandTrigger == CascaderViewExpandTrigger.Hover)
            {
                var cascaderViewItem = GetContainerFromEventSource(e.Source);
                if (cascaderViewItem != null)
                {
                    if (!cascaderViewItem.IsExpanded)
                    {
                        Dispatcher.InvokeAsync(async () =>
                        {
                            await ExpandItemAsync(cascaderViewItem);
                        });
                    }
                    else
                    {
                        cascaderViewItem.NotifyClearDescendantExpanded();
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Calls the base member first, then passes an event that is not handled yet to <see cref="HandleKeyDown" />.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled)
        {
            HandleKeyDown(e);
        }
    }

    /// <summary>
    /// Calls the base member first. When keyboard focus leaves the view as a whole, it forgets that Enter is down, so
    /// the next press is handled again.
    /// </summary>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (!IsKeyboardFocusWithin)
        {
            _isEnterKeyPressed = false;
        }
    }

    /// <summary>
    /// Marks the Enter key as released before the base member runs, so that the next press commits again (see
    /// <see cref="HandleKeyDown" />).
    /// </summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        HandleKeyUp(e);
        base.OnKeyUp(e);
    }

    /// <summary>
    /// Handles the keyboard navigation of the view and marks each consumed event as handled: Enter commits the
    /// candidate (selects it, toggles its check state while <see cref="IsCheckable" /> is set, and requests that the
    /// view be closed after a selection), Up and Down move the candidate, Right moves to the first child and Left to
    /// the parent (the two are swapped for right-to-left flow), Back moves to the parent, and Escape requests that
    /// the view be closed. While a filter is active, Enter, Up and Down act on the filter result instead.
    /// </summary>
    /// <param name="e">
    /// The key event. Its <c>Handled</c> flag is set for every key the view consumes.
    /// </param>
    /// <remarks>
    /// Does nothing when the event is already handled, the view is disabled, a modifier key is down or the key belongs
    /// to a text composition. Enter commits once per key press; auto-repeats are only marked as handled. The close is
    /// requested through the internal <c>CloseRequested</c> event; a hosting <see cref="Cascader" /> closes its
    /// dropdown in response. It is also requested when the keyboard candidate moves to the parent of a root option.
    /// The <see cref="Cascader" /> host forwards its own key events to this method.
    /// </remarks>
    public void HandleKeyDown(KeyEventArgs e)
    {
        if (e.Handled || !IsEnabled || e.KeyModifiers != KeyModifiers.None || IsComposingKeyInput(e.Source))
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Enter:
                if (!_isEnterKeyPressed)
                {
                    _isEnterKeyPressed = true;
                    if (IsFiltering)
                    {
                        TryCommitFilterCandidate();
                    }
                    else
                    {
                        TryCommitKeyboardCandidate();
                    }
                }
                e.Handled = true;
                break;
            case Key.Up:
                e.Handled = IsFiltering ? TryMoveFilterCandidate(-1) : TryMoveKeyboardCandidate(-1);
                break;
            case Key.Down:
                e.Handled = IsFiltering ? TryMoveFilterCandidate(1) : TryMoveKeyboardCandidate(1);
                break;
            case Key.Left:
            case Key.Right:
                if (!IsFiltering)
                {
                    var isForward = (e.Key == Key.Right) == (FlowDirection != FlowDirection.RightToLeft);
                    e.Handled = isForward ? TryMoveKeyboardCandidateToChild() : TryMoveKeyboardCandidateToParent();
                }
                break;
            case Key.Back:
                e.Handled = !IsFiltering && TryMoveKeyboardCandidateToParent();
                break;
            case Key.Escape:
                CloseRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
        }
    }

    private bool TryMoveKeyboardCandidate(int delta)
    {
        var candidate = GetKeyboardCandidate();
        var siblings = candidate?.ParentNode is ICascaderOption parent
            ? parent.Children.ToList()
            : BuildOptionList(_options);
        if (siblings.Count == 0)
        {
            return false;
        }

        var index = candidate != null ? siblings.IndexOf(candidate) : -1;
        if (index < 0 && delta < 0)
        {
            index = siblings.Count;
        }
        for (var i = 0; i < siblings.Count; i++)
        {
            index = (index + delta + siblings.Count) % siblings.Count;
            if (IsOptionEnabled(siblings[index]))
            {
                ActivateKeyboardCandidate(siblings[index]);
                return true;
            }
        }
        return false;
    }

    private bool TryMoveKeyboardCandidateToChild()
    {
        var candidate = GetKeyboardCandidate();
        if (candidate == null || GetCascaderViewItem(candidate)?.IsLoading == true)
        {
            return false;
        }
        var child = candidate.Children.FirstOrDefault(IsOptionEnabled);
        if (child == null)
        {
            return false;
        }
        ActivateKeyboardCandidate(child);
        return true;
    }

    private bool TryMoveKeyboardCandidateToParent()
    {
        var candidate = GetKeyboardCandidate();
        if (candidate?.ParentNode is ICascaderOption parent)
        {
            ActivateKeyboardCandidate(parent);
        }
        else
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        return true;
    }

    private bool TryCommitKeyboardCandidate()
    {
        var candidate = GetKeyboardCandidate();
        if (candidate == null || !TryCommitOption(candidate))
        {
            return false;
        }
        if (!IsCheckable)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        return true;
    }

    private bool TryCommitOption(ICascaderOption option)
    {
        if (!IsOptionEnabled(option) || GetCascaderViewItem(option)?.IsLoading == true)
        {
            return false;
        }
        if (IsCheckable)
        {
            if (!option.IsCheckBoxEnabled || (IsMaxSelectReached && option.IsChecked != true))
            {
                return false;
            }
            if (option.IsChecked == true)
            {
                UnCheckedSubTree(option);
            }
            else
            {
                CheckedSubTree(option);
            }
            return true;
        }
        if (!option.IsEffectiveLeaf() && !IsAllowSelectParent)
        {
            return false;
        }
        SelectOptionFromInteraction(option);
        return true;
    }

    private ICascaderOption? GetKeyboardCandidate()
    {
        if (_keyboardCandidateOption is { } candidate && IsOptionInSource(candidate) && IsOptionEnabled(candidate))
        {
            return candidate;
        }
        SetKeyboardCandidateOption(null);
        return null;
    }

    private bool IsOptionInSource(ICascaderOption option)
    {
        while (option.ParentNode is ICascaderOption parent)
        {
            if (!parent.Children.Contains(option))
            {
                return false;
            }
            option = parent;
        }
        return _options.Contains(option);
    }

    private static bool IsOptionEnabled(ICascaderOption option)
    {
        for (ICascaderOption? current = option; current != null; current = current.ParentNode as ICascaderOption)
        {
            if (!current.IsEnabled)
            {
                return false;
            }
        }
        return true;
    }

    private void ActivateKeyboardCandidate(ICascaderOption option)
    {
        SetKeyboardCandidateOption(option);
        Dispatcher.InvokeAsync(async () =>
        {
            if (!IsLoaded || IsFiltering || !ReferenceEquals(_keyboardCandidateOption, option) || !IsOptionInSource(option))
            {
                return;
            }
            await ExpandItemAsync(option);
            if (!ReferenceEquals(_keyboardCandidateOption, option) || IsFiltering || !IsLoaded || !IsOptionInSource(option))
            {
                return;
            }
            var levelList = GetLevelListForOption(option);
            if (levelList != null)
            {
                if (levelList.Presenter?.Panel is AtomUI.Controls.Primitives.VirtualizingStackPanel panel)
                {
                    panel.ScrollItemIntoView(levelList.Items.IndexOf(option));
                    ExecuteLayoutPass();
                }
                if (TryGetRealizedCascaderViewItem(levelList, option) is { } container)
                {
                    UpdateKeyboardCandidateContainer(container);
                }
            }
        });
    }

    private void SetKeyboardCandidate(CascaderViewItem? item)
    {
        SetKeyboardCandidateOption(item?.AttachedOption);
    }

    private void SetKeyboardCandidateOption(ICascaderOption? option)
    {
        if (ReferenceEquals(_keyboardCandidateOption, option))
        {
            return;
        }
        if (_keyboardCandidateOption != null && GetCascaderViewItem(_keyboardCandidateOption) is { } previous)
        {
            previous.SetCurrentValue(CascaderViewItem.IsCandidateSelectedProperty, false);
        }
        _keyboardCandidateOption = option;
        if (option != null && GetCascaderViewItem(option) is { } current)
        {
            UpdateKeyboardCandidateContainer(current);
        }
    }

    /// <summary>
    /// Calls the base member first, then drops the pending presentation of <see cref="SelectedOption" />, the keyboard
    /// candidate and the Enter state, and cancels every asynchronous load that is still pending.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CancelPendingSelectedOptionPresentation();
        SetKeyboardCandidateOption(null);
        _isEnterKeyPressed = false;
        
        // Cancel the asynchronous loads that are still pending.
        _asyncLoadCoordinator.CancelAll();
    }
}
