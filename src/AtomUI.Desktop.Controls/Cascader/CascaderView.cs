using System.Collections;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.Primitives;
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
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using ItemCollection = AtomUI.Collections.ItemCollection;
using OptionsSourceView = AtomUI.Collections.ItemsSourceView;

public enum CascaderViewExpandTrigger
{
    Click,
    Hover
}

public partial class CascaderView : TemplatedControl, 
                                    IMotionAwareControl,
                                    IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<IEnumerable<ICascaderOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<CascaderView, IEnumerable<ICascaderOption>?>(nameof(OptionsSource));
    
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<CascaderView, IDataTemplate?>(nameof(OptionTemplate));
    
    public static readonly StyledProperty<IconTemplate?> ExpandIconProperty =
        AvaloniaProperty.Register<CascaderView, IconTemplate?>(nameof(ExpandIcon));
    
    public static readonly StyledProperty<IconTemplate?> LoadingIconProperty =
        AvaloniaProperty.Register<CascaderView, IconTemplate?>(nameof(LoadingIcon));
    
    public static readonly StyledProperty<CascaderViewExpandTrigger> ExpandTriggerProperty =
        AvaloniaProperty.Register<CascaderView, CascaderViewExpandTrigger>(nameof(ExpandTrigger), CascaderViewExpandTrigger.Click);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CascaderView>();
    
    public static readonly StyledProperty<bool> IsCheckableProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsCheckable));
    
    public static readonly StyledProperty<TreeNodePath?> DefaultExpandedPathProperty =
        AvaloniaProperty.Register<CascaderView, TreeNodePath?>(nameof(DefaultExpandedPath));
    
    public static readonly StyledProperty<ICascaderItemDataLoader?> DataLoaderProperty =
        AvaloniaProperty.Register<CascaderView, ICascaderItemDataLoader?>(nameof(DataLoader));
    
    public static readonly StyledProperty<ICascaderItemFilter?> FilterProperty =
        AvaloniaProperty.Register<CascaderView, ICascaderItemFilter?>(nameof(Filter));
    
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<CascaderView, object?>(nameof(FilterValue));
    
    public static readonly StyledProperty<TextBlockHighlightStrategy> FilterHighlightStrategyProperty =
        AvaloniaProperty.Register<CascaderView, TextBlockHighlightStrategy>(nameof(FilterHighlightStrategy), TextBlockHighlightStrategy.All);
    
    public static readonly DirectProperty<CascaderView, int> FilterResultCountProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, int>(nameof(FilterResultCount),
            o => o.FilterResultCount);
    
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<CascaderView, IBrush?>(nameof(FilterHighlightForeground));
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<CascaderView, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<CascaderView, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsShowEmptyIndicator), true);
    
    public static readonly StyledProperty<bool> IsAllowSelectParentProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsAllowSelectParent));
    
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<CascaderView, Thickness>(nameof(EmptyIndicatorPadding));
    
    public static readonly DirectProperty<CascaderView, ICascaderOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, ICascaderOption?>(
            nameof(SelectedOption),
            o => o.SelectedOption,
            (o, v) => o.SelectedOption = v);
    
    public static readonly DirectProperty<CascaderView, IList<ICascaderOption>?> CheckedOptionsProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, IList<ICascaderOption>?>(
            nameof(CheckedOptions),
            o => o.CheckedOptions,
            (o, v) => o.CheckedOptions = v);
    
    public IEnumerable<ICascaderOption>? OptionsSource
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
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsCheckable
    {
        get => GetValue(IsCheckableProperty);
        set => SetValue(IsCheckableProperty, value);
    }
    
    public TreeNodePath? DefaultExpandedPath
    {
        get => GetValue(DefaultExpandedPathProperty);
        set => SetValue(DefaultExpandedPathProperty, value);
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

    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }
    
    public TextBlockHighlightStrategy FilterHighlightStrategy
    {
        get => GetValue(FilterHighlightStrategyProperty);
        set => SetValue(FilterHighlightStrategyProperty, value);
    }
    
    private int _filterResultCount;
    
    public int FilterResultCount
    {
        get => _filterResultCount;
        set => SetAndRaise(FilterResultCountProperty, ref _filterResultCount, value);
    }
    
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    /// <summary>
    /// 一版情况只有在点击叶子节点的时候才会触发 Select 事件
    /// </summary>
    public bool IsAllowSelectParent
    {
        get => GetValue(IsAllowSelectParentProperty);
        set => SetValue(IsAllowSelectParentProperty, value);
    }
    
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
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
    
    public OptionsSourceView OptionsView => _options;
    
    [Content]
    public ItemCollection Options => _options;
    
    #endregion
    
    #region 公共事件定义
    
    public event EventHandler<CascaderViewCheckedOptionsChangedEventArgs>? CheckedOptionsChanged;
    public event EventHandler<CascaderViewItemLoadedEventArgs>? ItemAsyncLoaded;
    public event EventHandler<CascaderItemExpandedEventArgs>? ItemExpanded;
    public event EventHandler<CascaderItemCollapsedEventArgs>? ItemCollapsed;
    public event EventHandler<CascaderItemClickedEventArgs>? ItemClicked;
    public event EventHandler<CascaderItemDoubleClickedEventArgs>? ItemDoubleClicked;
    public event EventHandler<CascaderOptionSelectedEventArgs>? OptionSelected;
    #endregion

    #region 内部属性定义
    internal static readonly DirectProperty<CascaderView, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);
    
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TreeViewToken.ID;
    
    #endregion
    
    private readonly ItemCollection _options = new();
    private static readonly IList Empty = Array.Empty<object>();
    private bool _ignoreSyncCheckedOptions;
    private StackPanel? _itemsPanel;
    private int _ignoreExpandAndCollapseLevel;
    private bool _defaultExpandPathApplied;
    private bool _ignoreSelectedPropertyChanged;
    private CascaderViewLevelList? _rootLevelList;
    private readonly Dictionary<CascaderViewLevelList, CompositeDisposable> _levelListDisposables = new();
    
    static CascaderView()
    {
        SetupExpandAndCollapse();
        SetupChecked();
        CascaderViewItem.DoubleTappedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemDoubleClicked(args));
        CascaderViewItem.ClickedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemClicked(args));
        CascaderViewItem.SelectedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderOptionSelected(args));
        OptionsSourceProperty.Changed.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderSourceChanged(args));
    }
    
    public CascaderView()
    {
        this.RegisterResources();
        _options.CollectionChanged += HandleCollectionChanged;
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, new DefaultCascaderItemFilter());
        }
        ConfigureEmptyIndicator();
        ConfigureEffectiveToggleType();
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsPanel    = e.NameScope.Find<StackPanel>(CascaderViewThemeConstants.ItemsPanelPart);
        _rootLevelList = e.NameScope.Find<CascaderViewLevelList>(CascaderViewThemeConstants.RootLevelListPart);
        if (_rootLevelList != null)
        {
            _rootLevelList.Level     = 1;
            _rootLevelList.OwnerView = this;
        }
        
        if (_filterList != null)
        {
            _filterList.SelectionChanged -= HandleFilterListSelectionChanged;
        }
        _filterList = e.NameScope.Find<CascaderViewFilterList>(CascaderViewThemeConstants.FilterListPart);
        
        if (_filterList != null)
        {
            _filterList.SelectionChanged += HandleFilterListSelectionChanged;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        FilterItems();
        if (DefaultExpandedPath != null && !_defaultExpandPathApplied)
        {
            ApplyDefaultExpandPath();
            _defaultExpandPathApplied = true;
        }
        
        if (SelectedOption != null)
        {
            SelectTargetOption(SelectedOption);
        }
    }

    internal bool TryParseSelectPath(TreeNodePath path, out IList<ICascaderOption> pathNodes)
    {
        var                    segments     = path.Segments;
        var                    count        = path.Segments.Count;
        var                    isPathValid  = true;
        IList<ICascaderOption> currentItems = _options.Cast<ICascaderOption>().ToList();
        
        var                          options    = new List<ICascaderOption>();
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
    
    private void ApplyDefaultExpandPath()
    {
        Debug.Assert(DefaultExpandedPath != null);
        if (TryParseSelectPath(DefaultExpandedPath, out IList<ICascaderOption> pathNodes))
        {
            if (pathNodes.Count > 0)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await ExpandItemAsync(pathNodes[^1]);
                });
            }
        }
    }

    private void HandleCascaderSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        _options.SetItemsSource(args.GetNewValue<IEnumerable<ICascaderOption>?>());
    }
    
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
            change.Property == FilterResultCountProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == IsCheckableProperty)
        {
            ConfigureEffectiveToggleType();
        }
        else if (change.Property == SelectedOptionProperty)
        {
            if (_ignoreSelectedPropertyChanged)
            {
                _ignoreSelectedPropertyChanged = false;
                return;
            }
            if (SelectedOption != null)
            {
                if (IsLoaded)
                {
                    SelectTargetOption(SelectedOption);
                }
            }
            else
            {
                CollapseAll();
            }
        }
    }
    
    private void SelectTargetOption(ICascaderOption option)
    {
        var isLeaf = option.IsLeaf || option.Children.Count == 0;
    
        if (!isLeaf && !IsAllowSelectParent)
        {
            throw new ArgumentException($"Option {option.Header} is not a Leaf node.");
        }
    
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var expandedItem = await ExpandItemAsync(option);
            if (expandedItem != null)
            {
                var targetLevelList = ItemsControl.ItemsControlFromItemContainer(expandedItem) as CascaderViewLevelList;
                Debug.Assert(targetLevelList != null);
                targetLevelList.SelectedItem = expandedItem;
            }
        });
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
            if (item.IsSelected)
            {
                var option = item.AttachedOption!;
                _ignoreSelectedPropertyChanged = true;
                SetCurrentValue(SelectedOptionProperty, option);
                OptionSelected?.Invoke(this, new CascaderOptionSelectedEventArgs(option));
            }
        }
    }

    protected virtual void NotifyCascaderItemClicked(CascaderViewItem item)
    {
    }
    
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
        IsEffectiveEmptyVisible = IsShowEmptyIndicator && isEmpty;
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
    
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Source is Visual source)
        {
            if (ExpandTrigger == CascaderViewExpandTrigger.Hover)
            {
                var cascaderViewItem = GetContainerFromEventSource(e.Source);
                if (cascaderViewItem != null)
                {
                    if (!cascaderViewItem.IsExpanded)
                    {
                        Dispatcher.UIThread.InvokeAsync(async () =>
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
    
    public void HandleKeyDown(KeyEventArgs e)
    {
        // TODO
        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = false;
                break;

            case Key.Up:
                e.Handled = false;
                break;

            case Key.Down:
                e.Handled = false;
                break;

            default:
                break;
        }
    }
}