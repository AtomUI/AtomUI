using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FlyoutControl = AtomUI.Desktop.Controls.Flyout;

namespace AtomUI.Desktop.Controls;

internal class DataGridFilterIndicator : IconButton
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsFilterActivatedProperty =
        AvaloniaProperty.Register<DataGridFilterIndicator, bool>(nameof(IsFilterActivated));

    public static readonly DirectProperty<DataGridFilterIndicator, DataGridFilterPresenterMode> FilterPresenterModeProperty =
        AvaloniaProperty.RegisterDirect<DataGridFilterIndicator, DataGridFilterPresenterMode>(
            nameof(FilterPresenterMode),
            o => o.FilterPresenterMode,
            (o, v) => o.FilterPresenterMode = v);
    
    public static readonly StyledProperty<bool> IsMultipleSelectionEnabledProperty =
        AvaloniaProperty.Register<DataGridFilterIndicator, bool>(nameof(IsMultipleSelectionEnabled));
    
    public event EventHandler<DataGridColumnFilterEventArgs>? FilterRequest;

    public bool IsFilterActivated
    {
        get => GetValue(IsFilterActivatedProperty);
        set => SetValue(IsFilterActivatedProperty, value);
    }

    private DataGridFilterPresenterMode _filterPresenterMode;

    public DataGridFilterPresenterMode FilterPresenterMode
    {
        get => _filterPresenterMode;
        set => SetAndRaise(FilterPresenterModeProperty, ref _filterPresenterMode, value);
    }
    
    public bool IsMultipleSelectionEnabled
    {
        get => GetValue(IsMultipleSelectionEnabledProperty);
        set => SetValue(IsMultipleSelectionEnabledProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<DataGridFilterIndicator, string?> SelectedAllTextProperty =
        AvaloniaProperty.RegisterDirect<DataGridFilterIndicator, string?>(
            nameof(SelectedAllText),
            o => o.SelectedAllText,
            (o, v) => o.SelectedAllText = v);

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        FlyoutControl.IsPopupPinnedOpenProperty.AddOwner<DataGridFilterIndicator>();
    
    internal string? SelectedAllText
    {
        get => _selectedAllText;
        set => SetAndRaise(SelectedAllTextProperty, ref _selectedAllText, value);
    }
    private string? _selectedAllText;

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion
    
    private DataGridColumn? _owningColumn;
    private static int s_indicatorSeed = 0;
    private string? _treeRadioCheckGroupName;
    private DataGrid? _subscribedGrid;
    private INotifyCollectionChanged? _subscribedFilters;
    private bool _isFlyoutContentMaterialized;
    private IDisposable? _popupPinnedOpenRelay;
    private int _pinnedOpenGeneration;

    internal DataGridColumn? OwningColumn
    {
        get => _owningColumn;
        set
        {
            if (ReferenceEquals(_owningColumn, value))
            {
                return;
            }

            ClosePopupForLifecycle();
            UnregisterOwningColumnSubscriptions();
            _owningColumn = value;
            if (_owningColumn != null)
            {
                RegisterOwningColumnSubscriptions(_owningColumn);
                FilterPresenterMode      = _owningColumn.FilterPresenterMode;
                IsMultipleSelectionEnabled = _owningColumn.FilterSelectionMode == DataGridFilterSelectionMode.Multiple;
            }
            RefreshFilterFlyoutState();
        }
    }

    private readonly FlyoutStateHelper _flyoutStateHelper;

    public DataGridFilterIndicator()
    {
        _flyoutStateHelper = new FlyoutStateHelper
        {
            AnchorTarget = this,
            TriggerType  = FlyoutTriggerType.Click
        };
        _flyoutStateHelper.FlyoutAboutToShow += HandleFlyoutAboutToShow;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (Icon is null)
        {
            SetValue(IconProperty, new FilterFilled(), BindingPriority.Template);
        }
        base.OnApplyTemplate(e);
        RefreshFilterFlyoutState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs changed)
    {
        base.OnPropertyChanged(changed);
        if (changed.Property == FilterPresenterModeProperty)
        {
            RecreateFlyoutShell();
        }
        else if (changed.Property == IsMultipleSelectionEnabledProperty)
        {
            var rematerialize = _isFlyoutContentMaterialized;
            ConfigureTreeFlyoutToggleType();
            ClearFlyoutContent();
            if (rematerialize)
            {
                MaterializeFlyoutContent();
            }
        }
        else if (changed.Property == IsPopupPinnedOpenProperty)
        {
            if (changed.GetNewValue<bool>())
            {
                RefreshFilterFlyoutState();
            }
            else
            {
                ++_pinnedOpenGeneration;
            }
        }
    }

    internal void RefreshFilterFlyoutState()
    {
        if (ShouldProvideFlyout())
        {
            EnsureFlyoutShell();
            if (IsPopupPinnedOpen)
            {
                QueuePinnedOpen();
            }
        }
        else
        {
            ClearFlyout();
        }
    }

    private bool ShouldProvideFlyout()
    {
        return OwningColumn?.OwningGrid is not null &&
               OwningColumn.HasFilterItems;
    }

    private void RecreateFlyoutShell()
    {
        var rematerialize = _isFlyoutContentMaterialized;
        ClearFlyout();
        RefreshFilterFlyoutState();
        if (rematerialize)
        {
            MaterializeFlyoutContent();
        }
    }

    private void EnsureFlyoutShell()
    {
        Debug.Assert(OwningColumn is not null);
        Debug.Assert(OwningColumn.OwningGrid is not null);

        if (OwningColumn?.OwningGrid is not { } owningGrid)
        {
            return;
        }

        if (FilterPresenterMode == DataGridFilterPresenterMode.Menu && Flyout is not DataGridMenuFilterFlyout)
        {
            ClearFlyout();
            var menuFlyout = new DataGridMenuFilterFlyout
            {
                IsArrowVisible             = false,
                Placement                 = PlacementMode.BottomEdgeAlignedRight,
                ShouldUseOverlayPopup     = true
            };
            menuFlyout[!MotionAwareControlProperty.IsMotionEnabledProperty] =
                owningGrid[!MotionAwareControlProperty.IsMotionEnabledProperty];

            Flyout                          =  menuFlyout;
            _flyoutStateHelper.Flyout       =  menuFlyout;
            _popupPinnedOpenRelay = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                menuFlyout,
                FlyoutControl.IsPopupPinnedOpenProperty);
            menuFlyout.FilterValuesSelected += HandleFilterValuesSelected;
        }
        else if (FilterPresenterMode == DataGridFilterPresenterMode.Tree && Flyout is not DataGridTreeFilterFlyout)
        {
            ClearFlyout();
            var treeFlyout = new DataGridTreeFilterFlyout
            {
                IsArrowVisible             = false,
                Placement                 = PlacementMode.BottomEdgeAlignedRight,
                ShouldUseOverlayPopup     = true
            };
            treeFlyout[!MotionAwareControlProperty.IsMotionEnabledProperty] =
                owningGrid[!MotionAwareControlProperty.IsMotionEnabledProperty];
            ConfigureTreeFlyoutToggleType(treeFlyout);

            Flyout                    = treeFlyout;
            _flyoutStateHelper.Flyout = treeFlyout;
            _popupPinnedOpenRelay = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                treeFlyout,
                FlyoutControl.IsPopupPinnedOpenProperty);
            treeFlyout.FilterValuesSelected += HandleFilterValuesSelected;
        }

        ConfigureFlyoutPassiveCloseFilter();
    }

    private bool ShouldFilterOnPassiveClose()
    {
        return OwningColumn?.FilterApplyMode == DataGridFilterApplyMode.Close;
    }

    private void ConfigureFlyoutPassiveCloseFilter()
    {
        if (Flyout is DataGridMenuFilterFlyout menuFlyout)
        {
            menuFlyout.ShouldFilterOnPassiveClose = ShouldFilterOnPassiveClose;
        }
        else if (Flyout is DataGridTreeFilterFlyout treeFlyout)
        {
            treeFlyout.ShouldFilterOnPassiveClose = ShouldFilterOnPassiveClose;
        }
    }

    private void HandleFlyoutAboutToShow(object? sender, EventArgs args)
    {
        MaterializeFlyoutContent();
    }

    private void MaterializeFlyoutContent()
    {
        if (_isFlyoutContentMaterialized || OwningColumn is null)
        {
            return;
        }

        EnsureFlyoutShell();
        if (Flyout is DataGridMenuFilterFlyout menuFlyout)
        {
            PopulateMenuItems(menuFlyout.Items, OwningColumn.GetEffectiveFilterItems());
            _isFlyoutContentMaterialized = true;
        }
        else if (Flyout is DataGridTreeFilterFlyout treeFlyout)
        {
            if (IsMultipleSelectionEnabled)
            {
                var selectAllTreeItem = new DataGridFilterTreeViewItem();
                selectAllTreeItem[!DataGridFilterTreeViewItem.HeaderProperty] = this[!SelectedAllTextProperty];
                PopulateTreeItems(selectAllTreeItem.Items, OwningColumn.GetEffectiveFilterItems());
                treeFlyout.Items.Add(selectAllTreeItem);
            }
            else
            {
                PopulateTreeItems(treeFlyout.Items, OwningColumn.GetEffectiveFilterItems());
            }
            _isFlyoutContentMaterialized = true;
        }
    }

    private string TreeRadioCheckGroupName =>
        _treeRadioCheckGroupName ??= $"tree-{nameof(DataGridFilterIndicator)}-{s_indicatorSeed++}";

    private void PopulateMenuItems(ItemCollection targetItems, IEnumerable<DataGridFilterItem> filterItems)
    {
        foreach (var item in filterItems)
        {
            var menuItem = new DataGridFilterMenuItem()
            {
                Header           = item.Text,
                FilterValue      = item.Value,
                IsChecked        = OwningColumn?.IsFilterValueSelected(item.Value) == true,
                StaysOpenOnClick = true,
                ToggleType       = IsMultipleSelectionEnabled
                    ? MenuItemToggleType.CheckBox
                    : MenuItemToggleType.Radio
            };
            targetItems.Add(menuItem);
            if (item.HasChildren)
            {
                PopulateMenuItems(menuItem.Items, item.Children);
            }
        }
    }

    private void PopulateTreeItems(ItemCollection targetItems, IEnumerable<DataGridFilterItem> filterItems)
    {
        foreach (var item in filterItems)
        {
            var treeItem = new DataGridFilterTreeViewItem
            {
                Header      = item.Text,
                FilterValue = item.Value,
                GroupName   = TreeRadioCheckGroupName,
                IsChecked   = OwningColumn?.IsFilterValueSelected(item.Value) == true
            };
  
            targetItems.Add(treeItem);
            if (item.HasChildren)
            {
                PopulateTreeItems(treeItem.Items, item.Children);
            }
        }
    }

    private void HandleFilterValuesSelected(object? sender, DataGridFilterValuesSelectedEventArgs args)
    {
        Debug.Assert(OwningColumn != null);
        if (ShouldApplyFilterValues(args))
        {
            FilterRequest?.Invoke(this, new DataGridColumnFilterEventArgs(OwningColumn, args.Values));
        }
    }

    private bool ShouldApplyFilterValues(DataGridFilterValuesSelectedEventArgs args)
    {
        return args.IsConfirmed ||
               (args.IsPassiveClose && OwningColumn?.FilterApplyMode == DataGridFilterApplyMode.Close) ||
               (args.IsSelectionChanged && OwningColumn?.FilterApplyMode == DataGridFilterApplyMode.SelectionChanged);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ++_pinnedOpenGeneration;
        base.OnDetachedFromVisualTree(e);
        _flyoutStateHelper.NotifyDetachedFromVisualTree();
        UnregisterOwningColumnSubscriptions();
        ClearFlyout();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (OwningColumn != null)
        {
            RegisterOwningColumnSubscriptions(OwningColumn);
        }
        _flyoutStateHelper.NotifyAttachedToVisualTree();
        RefreshFilterFlyoutState();
    }

    protected override void OnClick()
    {
        if (IsEffectivelyEnabled)
        {
            var e = new RoutedEventArgs(ClickEvent);
            RaiseEvent(e);
            var (command, parameter) = (Command, CommandParameter);
            if (!e.Handled && command is not null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
                e.Handled = true;
            }
        }
    }

    private void RegisterOwningColumnSubscriptions(DataGridColumn column)
    {
        if (!ReferenceEquals(_subscribedFilters, column.Filters))
        {
            if (_subscribedFilters != null)
            {
                _subscribedFilters.CollectionChanged -= HandleFiltersChanged;
            }

            _subscribedFilters = column.Filters as INotifyCollectionChanged;
            if (_subscribedFilters != null)
            {
                _subscribedFilters.CollectionChanged += HandleFiltersChanged;
            }
        }

        if (column.OwningGrid is { } grid && !ReferenceEquals(_subscribedGrid, grid))
        {
            UnregisterGrid();
            _subscribedGrid = grid;
            _subscribedGrid.PropertyChanged += HandleOwningGridPropertyChanged;
            UpdateFilterActivatedState();
        }
    }

    private void UnregisterOwningColumnSubscriptions()
    {
        if (_subscribedFilters != null)
        {
            _subscribedFilters.CollectionChanged -= HandleFiltersChanged;
            _subscribedFilters = null;
        }
        UnregisterGrid();
    }

    private void UnregisterGrid()
    {
        if (_subscribedGrid != null)
        {
            _subscribedGrid.PropertyChanged -= HandleOwningGridPropertyChanged;
            _subscribedGrid = null;
        }
        UpdateFilterActivatedState();
    }

    private void HandleOwningGridPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == DataGrid.QueryProperty ||
                 change.Property == DataGrid.ItemsSourceProperty)
        {
            RefreshSelectedFilterValues();
        }
    }

    private void HandleFiltersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var rematerialize = _isFlyoutContentMaterialized;
        ClearFlyoutContent();
        RefreshFilterFlyoutState();
        if (rematerialize)
        {
            MaterializeFlyoutContent();
        }
    }

    internal void RefreshSelectedFilterValues()
    {
        if (!_isFlyoutContentMaterialized)
        {
            UpdateFilterActivatedState();
            return;
        }

        ClearFlyoutContent();
        MaterializeFlyoutContent();
        UpdateFilterActivatedState();
    }

    private void UpdateFilterActivatedState()
    {
        IsFilterActivated = OwningColumn?.HasActiveQueryFilter == true;
    }

    private void ConfigureTreeFlyoutToggleType()
    {
        if (Flyout is DataGridTreeFilterFlyout treeFlyout)
        {
            ConfigureTreeFlyoutToggleType(treeFlyout);
        }
    }

    private void ConfigureTreeFlyoutToggleType(DataGridTreeFilterFlyout treeFlyout)
    {
        treeFlyout.ToggleType = IsMultipleSelectionEnabled
            ? ItemToggleType.CheckBox
            : ItemToggleType.Radio;
    }

    private void ClearFlyoutContent()
    {
        if (Flyout is DataGridMenuFilterFlyout menuFlyout)
        {
            menuFlyout.Items.Clear();
        }
        else if (Flyout is DataGridTreeFilterFlyout treeFlyout)
        {
            treeFlyout.Items.Clear();
        }
        _isFlyoutContentMaterialized = false;
    }

    private void ClearFlyout()
    {
        ++_pinnedOpenGeneration;
        if (Flyout is FlyoutControl atomFlyout)
        {
            atomFlyout.CloseForLifecycle();
        }

        _popupPinnedOpenRelay?.Dispose();
        _popupPinnedOpenRelay = null;
        if (Flyout is FlyoutControl releasedFlyout)
        {
            releasedFlyout.SetCurrentValue(FlyoutControl.IsPopupPinnedOpenProperty, false);
        }

        if (Flyout is DataGridMenuFilterFlyout menuFlyout)
        {
            menuFlyout.FilterValuesSelected -= HandleFilterValuesSelected;
            menuFlyout.ShouldFilterOnPassiveClose = null;
            menuFlyout.Items.Clear();
        }
        else if (Flyout is DataGridTreeFilterFlyout treeFlyout)
        {
            treeFlyout.FilterValuesSelected -= HandleFilterValuesSelected;
            treeFlyout.ShouldFilterOnPassiveClose = null;
            treeFlyout.Items.Clear();
        }

        Flyout                    = null;
        _flyoutStateHelper.Flyout = null;
        _isFlyoutContentMaterialized = false;
    }

    internal void ClosePopupForLifecycle()
    {
        ++_pinnedOpenGeneration;
        if (Flyout is FlyoutControl flyout)
        {
            flyout.CloseForLifecycle();
        }
    }

    private void QueuePinnedOpen()
    {
        var generation = ++_pinnedOpenGeneration;
        var flyout     = Flyout as FlyoutControl;
        Dispatcher.UIThread.Post(() =>
        {
            if (generation == _pinnedOpenGeneration &&
                IsPopupPinnedOpen &&
                this.IsAttachedToVisualTree() &&
                IsEffectivelyEnabled &&
                IsEffectivelyVisible &&
                OwningColumn?.OwningGrid is not null &&
                TopLevel.GetTopLevel(this) is not null &&
                ReferenceEquals(flyout, Flyout) &&
                flyout is { IsOpen: false })
            {
                _flyoutStateHelper.ShowFlyout(immediately: true);
            }
        }, DispatcherPriority.Loaded);
    }
}
