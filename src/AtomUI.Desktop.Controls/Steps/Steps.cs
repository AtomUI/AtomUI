using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum StepsItemIndicatorType
{
    Default,
    Dot
}

public enum StepsItemStatus
{
    Wait,
    Process,
    Finish,
    Error
}

public enum StepsStyle
{
    Default,
    Navigation,
    Inline
}

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public class Steps : SelectingItemsControl,
                     ISizeTypeAware,
                     IMotionAwareControl,
                     IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    
    public static readonly StyledProperty<int> CurrentStepProperty =
        AvaloniaProperty.Register<Steps, int>(nameof(CurrentStep), 0);
    
    public static readonly StyledProperty<int> InitialStepProperty =
        AvaloniaProperty.Register<Steps, int>(nameof(InitialStep), -1);
    
    public static readonly StyledProperty<double> ProgressValueProperty =
        AvaloniaProperty.Register<Steps, double>(nameof(ProgressValue), 0, coerce:CoerceProgressValue);
    
    public static readonly StyledProperty<StepsItemStatus> CurrentStepStatusProperty =
        AvaloniaProperty.Register<Steps, StepsItemStatus>(nameof(CurrentStepStatus), StepsItemStatus.Process);
    
    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<Steps>();
    
    public static readonly StyledProperty<Orientation> LabelPlacementProperty =
        AvaloniaProperty.Register<Steps, Orientation>(nameof(LabelPlacement), Orientation.Horizontal);

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Steps>();
    
    public static readonly StyledProperty<StepsItemIndicatorType> ItemIndicatorTypeProperty =
        AvaloniaProperty.Register<Steps, StepsItemIndicatorType>(nameof(ItemIndicatorType), StepsItemIndicatorType.Default);
    
    public static readonly StyledProperty<StepsStyle> StyleProperty =
        AvaloniaProperty.Register<Steps, StepsStyle>(nameof(Style), StepsStyle.Default);
        
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Steps>();
    
    public static readonly StyledProperty<bool> IsItemClickableProperty =
        AvaloniaProperty.Register<Steps, bool>(nameof(IsItemClickable), false);
    
    public static readonly StyledProperty<bool> IsShowItemProgressProperty =
        AvaloniaProperty.Register<Steps, bool>(nameof(IsShowItemProgress), false);
    
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<Steps>();
    
    public static readonly DirectProperty<Steps, object?> CurrentContentProperty =
        AvaloniaProperty.RegisterDirect<Steps, object?>(nameof(CurrentContent), o => o.CurrentContent);

    public static readonly DirectProperty<Steps, IDataTemplate?> CurrentContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<Steps, IDataTemplate?>(nameof(CurrentContentTemplate), o => o.CurrentContentTemplate);
    
    public int CurrentStep
    {
        get => GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }
    
    public int InitialStep
    {
        get => GetValue(InitialStepProperty);
        set => SetValue(InitialStepProperty, value);
    }
    
    public double ProgressValue
    {
        get => GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }
    
    public StepsItemStatus CurrentStepStatus
    {
        get => GetValue(CurrentStepStatusProperty);
        set => SetValue(CurrentStepStatusProperty, value);
    }
    
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    
    public Orientation LabelPlacement
    {
        get => GetValue(LabelPlacementProperty);
        set => SetValue(LabelPlacementProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
        
    public StepsItemIndicatorType ItemIndicatorType
    {
        get => GetValue(ItemIndicatorTypeProperty);
        set => SetValue(ItemIndicatorTypeProperty, value);
    }
    
    public StepsStyle Style
    {
        get => GetValue(StyleProperty);
        set => SetValue(StyleProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsItemClickable
    {
        get => GetValue(IsItemClickableProperty);
        set => SetValue(IsItemClickableProperty, value);
    }
    
    public bool IsShowItemProgress
    {
        get => GetValue(IsShowItemProgressProperty);
        set => SetValue(IsShowItemProgressProperty, value);
    }
    
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    private object? _currentContent;

    public object? CurrentContent
    {
        get => _currentContent;
        internal set => SetAndRaise(CurrentContentProperty, ref _currentContent, value);
    }
    
    private IDataTemplate? _currentContentTemplate;
    public IDataTemplate? CurrentContentTemplate
    {
        get => _currentContentTemplate;
        internal set => SetAndRaise(CurrentContentTemplateProperty, ref _currentContentTemplate, value);
    }
    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<double> HorizontalItemSpacingProperty =
        AvaloniaProperty.Register<Steps, double>(nameof(HorizontalItemSpacing));
    
    internal static readonly StyledProperty<double> VerticalItemSpacingProperty =
        AvaloniaProperty.Register<Steps, double>(nameof(VerticalItemSpacing));
    
    internal double HorizontalItemSpacing
    {
        get => GetValue(HorizontalItemSpacingProperty);
        set => SetValue(HorizontalItemSpacingProperty, value);
    }
    
    internal double VerticalItemSpacing
    {
        get => GetValue(VerticalItemSpacingProperty);
        set => SetValue(VerticalItemSpacingProperty, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => StepsToken.ID;

    #endregion

    private readonly Dictionary<StepsItem, CompositeDisposable> _itemsBindingDisposables = new();
    private Grid? _grid;
    private CompositeDisposable? _currentItemSubscriptions;
    
    static Steps()
    {
        AffectsMeasure<Steps>(SizeTypeProperty);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<Steps>(false);
        OrientationProperty.OverrideDefaultValue<Steps>(Orientation.Horizontal);
        SelectedItemProperty.Changed.AddClassHandler<Steps>((x, e) => x.UpdateCurrentContent());
    }
    
    public Steps()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
        SelectionMode                     =  SelectionMode.Single;
    }
    
    private static double CoerceProgressValue(AvaloniaObject sender, double value)
    {
        if (value < 0.0)
        {
            return 0.0;
        }
        if (value > 100.0)
        {
            return 100.0;
        }
        return value;
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is StepsItem stepsItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(stepsItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(stepsItem);
                        }
                    }
                }
            }
        }
        ConfigureItemsPanel();
        ConfigureCurrentStepsItem();
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new StepsItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<StepsItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is StepsItem stepsItem)
        {
            var disposables = new CompositeDisposable(2);
            
            if (item != null && item is not Visual)
            {
                if (!stepsItem.IsSet(StepsItem.ContentProperty))
                {
                    stepsItem.SetCurrentValue(StepsItem.ContentProperty, item);
                }
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, stepsItem, StepsItem.ContentTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, stepsItem, StepsItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, StyleProperty, stepsItem, StepsItem.StyleProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemIndicatorTypeProperty, stepsItem, StepsItem.IndicatorTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsItemClickableProperty, stepsItem, StepsItem.IsClickableProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, stepsItem, StepsItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, OrientationProperty, stepsItem, StepsItem.OrientationProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowItemProgressProperty, stepsItem, StepsItem.IsShowProgressProperty));
            disposables.Add(BindUtils.RelayBind(this, ProgressValueProperty, stepsItem, StepsItem.ProgressValueProperty));
            disposables.Add(BindUtils.RelayBind(this, LabelPlacementProperty, stepsItem, StepsItem.LabelPlacementProperty));
            
            PrepareStepsItem(stepsItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(stepsItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(stepsItem);
            }
            _itemsBindingDisposables.Add(stepsItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type StepsItem.");
        }
    }
    
    protected virtual void PrepareStepsItem(StepsItem stepsItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
        {
            UpdatePseudoClasses();
            ConfigureItemsPanel();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == SelectedIndexProperty)
            {
                ConfigureCurrentStepsItem();
            }
        }
        if (change.Property == CurrentStepProperty)
        {
            SyncCurrentStepToSelectedItem();    
        }
        else if (change.Property == ContentTemplateProperty)
        {
            var newTemplate = change.GetNewValue<IDataTemplate?>();
            if (CurrentContentTemplate != newTemplate &&
                ContainerFromIndex(SelectedIndex) is { } container && 
                container.GetValue(ContentControl.ContentTemplateProperty) == null)
            {
                CurrentContentTemplate = newTemplate;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var itemsPresenter = e.NameScope.Find<ItemsPresenter>(StepsThemeConstants.ItemsPresenterPart);
        itemsPresenter?.ApplyTemplate();
        if (itemsPresenter?.Panel != null)
        {
            _grid = itemsPresenter.Panel as Grid;
        }
        ConfigureItemsPanel();
        UpdatePseudoClasses();
        if (InitialStep != -1)
        {
            SetCurrentValue(CurrentStepProperty, InitialStep);
        }

        SyncCurrentStepToSelectedItem();
        ConfigureCurrentStepsItem();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Vertical, Orientation == Orientation.Vertical);
        PseudoClasses.Set(StdPseudoClass.Horizontal, Orientation == Orientation.Horizontal);
    }

    private void ConfigureItemsPanel()
    {
        if (_grid != null)
        {
            var count = _grid.Children.Count;
            _grid.RowDefinitions.Clear();
            _grid.RowDefinitions.Clear();
            if (Orientation == Orientation.Horizontal)
            {
                var columnDefinitions = new ColumnDefinitions();
                for (var i = 0; i < count; i++)
                {
                    GridLength gridLength       = default;
                    
                    if (i != count - 1)
                    {
                        gridLength = GridLength.Star;
                    }
                    else
                    {
                        if (Style == StepsStyle.Default || Style == StepsStyle.Inline)
                        {
                            gridLength = GridLength.Auto;
                        }
                        else
                        {
                            gridLength = GridLength.Star;
                        }
                    }
                    
                    columnDefinitions.Add(new ColumnDefinition(gridLength));
                }
                _grid.ColumnDefinitions = columnDefinitions;
            }
            else
            {
                var rowDefinitions = new RowDefinitions();
                for (var i = 0; i < count; i++)
                {
                    var rowDefinition =  new RowDefinition(GridLength.Auto);
                    if (Style == StepsStyle.Navigation)
                    {
                        rowDefinition.SharedSizeGroup = "NavStepsGridSizeGroup";
                    }
                    rowDefinitions.Add(rowDefinition);
                }
                _grid.RowDefinitions = rowDefinitions;
                _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            }
        }
    }

    private void ConfigureCurrentStepsItem()
    {
        for (var i = 0; i < ItemCount; ++i)
        {
            if (ContainerFromIndex(i) is StepsItem stepsItem)
            {
                stepsItem.SetCurrentValue(StepsItem.PositionProperty, i + 1);
                stepsItem.SetCurrentValue(StepsItem.IsFirstProperty, i == 0);
                stepsItem.SetCurrentValue(StepsItem.IsLastProperty, i == ItemCount - 1);
                if (SelectedIndex != -1)
                {
                    if (i < SelectedIndex)
                    {
                        stepsItem.SetCurrentValue(StepsItem.IsFinishedProperty, true);
                        stepsItem.SetValue(StepsItem.StatusProperty, StepsItemStatus.Finish, BindingPriority.Template);
                    }
                    else if (i == SelectedIndex)
                    {
                        stepsItem.SetValue(StepsItem.StatusProperty, CurrentStepStatus, BindingPriority.Template);
                    }
                    else
                    {
                        stepsItem.SetCurrentValue(StepsItem.IsFinishedProperty, false);
                        stepsItem.SetValue(StepsItem.StatusProperty, StepsItemStatus.Wait, BindingPriority.Template);
                    }
                }
                else
                {
                    if (CurrentStep >= ItemCount)
                    {
                        stepsItem.SetCurrentValue(StepsItem.IsFinishedProperty, true);
                        stepsItem.SetValue(StepsItem.StatusProperty, StepsItemStatus.Finish, BindingPriority.Template);
                    }
                }

                if (Orientation == Orientation.Horizontal)
                {
                    Grid.SetRow(stepsItem, 0);
                    Grid.SetColumn(stepsItem, i);
                }
                else
                {
                    Grid.SetRow(stepsItem, i);
                    Grid.SetColumn(stepsItem, 0);
                }
            }
        }
    }

    private void SyncCurrentStepToSelectedItem()
    {
        if (CurrentStep >= 0 && CurrentStep < ItemCount)
        {
            SetCurrentValue(SelectedIndexProperty, CurrentStep);
        }
        else
        {
            SetCurrentValue(SelectedIndexProperty, -1);
        }
    }
    
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);

        var selectedIndex = SelectedIndex;

        if (selectedIndex == oldIndex || selectedIndex == newIndex)
        {
            UpdateCurrentContent();
        }
    }
    
    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);
        UpdateCurrentContent();
    }
    
    private void UpdateCurrentContent(Control? container = null)
    {
        _currentItemSubscriptions?.Dispose();
        _currentItemSubscriptions = null;

        if (SelectedIndex == -1)
        {
            CurrentContent = CurrentContentTemplate = null;
        }
        else
        {
            container ??= ContainerFromIndex(SelectedIndex);
            if (container != null)
            {
                if (CurrentContentTemplate != EffectiveCurrentContentTemplate(container.GetValue(ContentTemplateProperty)))
                {
                    // If the value of CurrentContentTemplate is about to change, clear it first. This ensures
                    // that the template is not reused as soon as CurrentContent changes in the statement below
                    // this block, and also that controls generated from it are unloaded before CurrentContent
                    // (which is typically their DataContext) changes.
                    CurrentContentTemplate = null;
                }

                _currentItemSubscriptions = new CompositeDisposable(
                    container.GetObservable(ContentControl.ContentProperty).Subscribe(v => CurrentContent = v),
                    container.GetObservable(ContentControl.ContentTemplateProperty).Subscribe(v => CurrentContentTemplate = EffectiveCurrentContentTemplate(v)));

                // Note how we fall back to our own ContentTemplate if the container doesn't specify one
                IDataTemplate? EffectiveCurrentContentTemplate(IDataTemplate? containerTemplate) => containerTemplate ?? ContentTemplate;
            }
        }
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsItemClickable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.Pointer.Type == PointerType.Mouse)
        {
            e.Handled = UpdateSelectionFromEventSource(e.Source);
        }
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (IsItemClickable && e.InitialPressMouseButton == MouseButton.Left && e.Pointer.Type != PointerType.Mouse)
        {
            var container = GetContainerFromEventSource(e.Source);
            if (container != null && container.GetVisualsAt(e.GetPosition(container))
                                              .Any(c => container == c || container.IsVisualAncestorOf(c)))
            {
                e.Handled = UpdateSelectionFromEventSource(e.Source);
            }
        }
    }
}