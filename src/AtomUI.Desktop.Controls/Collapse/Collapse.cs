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
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum CollapseTriggerType
{
    Header,
    Icon
}

public enum CollapseExpandIconPosition
{
    Start,
    End
}

[TemplatePart(CollapseThemeConstants.ItemsPresenterPart, typeof(ItemsPresenter))]
public class Collapse : SelectingItemsControl,
                        IMotionAwareControl,
                        IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Collapse>();

    public static readonly StyledProperty<bool> IsGhostStyleProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(IsGhostStyle));

    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(IsBorderless));

    public static readonly StyledProperty<bool> IsAccordionProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(IsAccordion));

    public static readonly StyledProperty<CollapseTriggerType> TriggerTypeProperty =
        AvaloniaProperty.Register<Collapse, CollapseTriggerType>(nameof(TriggerType));

    public static readonly StyledProperty<CollapseExpandIconPosition> ExpandIconPositionProperty =
        AvaloniaProperty.Register<Collapse, CollapseExpandIconPosition>(nameof(ExpandIconPosition));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Collapse>();
    
    public static readonly StyledProperty<Thickness?> ItemHeaderPaddingProperty =
        AvaloniaProperty.Register<Collapse, Thickness?>(nameof(ItemHeaderPadding));
    
    public static readonly StyledProperty<Thickness?> ItemContentPaddingProperty =
        AvaloniaProperty.Register<Collapse, Thickness?>(nameof(ItemContentPadding));

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsGhostStyle
    {
        get => GetValue(IsGhostStyleProperty);
        set => SetValue(IsGhostStyleProperty, value);
    }

    public bool IsBorderless
    {
        get => GetValue(IsBorderlessProperty);
        set => SetValue(IsBorderlessProperty, value);
    }

    public bool IsAccordion
    {
        get => GetValue(IsAccordionProperty);
        set => SetValue(IsAccordionProperty, value);
    }

    public CollapseTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    public CollapseExpandIconPosition ExpandIconPosition
    {
        get => GetValue(ExpandIconPositionProperty);
        set => SetValue(ExpandIconPositionProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public Thickness? ItemHeaderPadding
    {
        get => GetValue(ItemHeaderPaddingProperty);
        set => SetValue(ItemHeaderPaddingProperty, value);
    }

    public Thickness? ItemContentPadding
    {
        get => GetValue(ItemContentPaddingProperty);
        set => SetValue(ItemContentPaddingProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Collapse, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<Collapse, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);

    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel
        {
            Orientation = Orientation.Vertical
        });
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CollapseToken.ID;

    #endregion
    
    private readonly Dictionary<CollapseItem, CompositeDisposable> _itemsBindingDisposables = new();

    static Collapse()
    {
        SelectionModeProperty.OverrideDefaultValue<Collapse>(SelectionMode.Multiple | SelectionMode.Toggle);
        ItemsPanelProperty.OverrideDefaultValue<Collapse>(DefaultPanel);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<Collapse>(false);
        AffectsRender<Collapse>(SelectionModeProperty);
    }

    public Collapse()
    {
        SelectionChanged                  += HandleSelectionChanged;
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
        this.RegisterResources();
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is CollapseItem collapseItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(collapseItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(collapseItem);
                        }
                    }
                }
            }
        }
    }

    private void HandleSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        SetupItemsBorderThickness();
    }

    private void SetupItemsBorderThickness()
    {
        if (this.IsAttachedToVisualTree())
        {
            for (var i = 0; i < ItemCount; ++i)
            {
                if (Items[i] is CollapseItem collapseItem)
                {
                    SetupCollapseBorderThickness(collapseItem, i);
                }
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CollapseItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CollapseItem>(item, out recycleKey);
    }

    protected sealed override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is CollapseItem collapseItem)
        {
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual)
            {
                if (!collapseItem.IsSet(CollapseItem.ContentProperty))
                {
                    collapseItem.SetCurrentValue(CollapseItem.ContentProperty, item);
                }

                if (item is ICollapseItemData itemData)
                {
                    if (!collapseItem.IsSet(CollapseItem.HeaderProperty))
                    {
                        collapseItem.SetCurrentValue(CollapseItem.HeaderProperty, itemData.Header);
                    }
                    if (!collapseItem.IsSet(CollapseItem.IsSelectedProperty))
                    {
                        collapseItem.SetCurrentValue(CollapseItem.IsSelectedProperty, itemData.IsSelected);
                    }
                    if (!collapseItem.IsSet(CollapseItem.IsShowExpandIconProperty))
                    {
                        collapseItem.SetCurrentValue(CollapseItem.IsShowExpandIconProperty, itemData.IsShowExpandIcon);
                    }
                }
            }

            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, collapseItem, CollapseItem.ContentTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, collapseItem, CollapseItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, EffectiveBorderThicknessProperty, collapseItem, BorderThicknessProperty));
            disposables.Add(BindUtils.RelayBind(this, IsGhostStyleProperty, collapseItem, CollapseItem.IsGhostStyleProperty));
            disposables.Add(BindUtils.RelayBind(this, IsBorderlessProperty, collapseItem, CollapseItem.IsBorderlessProperty));
            disposables.Add(BindUtils.RelayBind(this, TriggerTypeProperty, collapseItem, CollapseItem.TriggerTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, ExpandIconPositionProperty, collapseItem,
                CollapseItem.ExpandIconPositionProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, collapseItem, CollapseItem.IsMotionEnabledProperty));

            PrepareCollapseItem(collapseItem, item, index, disposables);
        
            if (_itemsBindingDisposables.TryGetValue(collapseItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(collapseItem);
            }
            _itemsBindingDisposables.Add(collapseItem, disposables);
            
            SetupCollapseBorderThickness(collapseItem, index);
            ConfigureItemPaddings(collapseItem);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type CollapseItem.");
        }
    }

    protected virtual void PrepareCollapseItem(CollapseItem collapseItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        if (container is CollapseItem collapseItem)
        {
            SetupCollapseBorderThickness(collapseItem, newIndex);
        }
    }

    private void SetupCollapseBorderThickness(CollapseItem collapseItem, int index)
    {
        var headerBorderBottom  = BorderThickness.Bottom;
        var contentBorderBottom = BorderThickness.Bottom;
        if (!IsGhostStyle)
        {
            if (!IsBorderless)
            {
                if (index == ItemCount - 1 && !collapseItem.IsSelected)
                {
                    headerBorderBottom = 0d;
                }
            }
            else
            {
                if (collapseItem.IsSelected || (index == ItemCount - 1 && !collapseItem.IsSelected))
                {
                    headerBorderBottom = 0d;
                }
            }

            if (index == ItemCount - 1 &&
                (collapseItem.IsSelected || (!collapseItem.IsSelected && collapseItem.InAnimating)))
            {
                contentBorderBottom = 0d;
            }
        }
        else
        {
            headerBorderBottom  = 0d;
            contentBorderBottom = 0d;
        }

        collapseItem.HeaderBorderThickness  = new Thickness(0, 0, 0, headerBorderBottom);
        collapseItem.ContentBorderThickness = new Thickness(0, 0, 0, contentBorderBottom);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.NavigationMethod == NavigationMethod.Directional)
        {
            var containerFromEventSource = GetContainerFromEventSource(e.Source);
            if (containerFromEventSource is CollapseItem collapseItem)
            {
                if (!collapseItem.InAnimating)
                {
                    e.Handled = UpdateSelectionFromEventSource(e.Source);
                }
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.Pointer.Type == PointerType.Mouse)
        {
            var containerFromEventSource = GetContainerFromEventSource(e.Source);
            if (containerFromEventSource is CollapseItem collapseItem)
            {
                if (!collapseItem.InAnimating && collapseItem.IsPointInHeaderBounds(e.GetPosition(collapseItem)))
                {
                    e.Handled = UpdateSelectionFromEventSource(e.Source);
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left && e.Pointer.Type != PointerType.Mouse)
        {
            var container = GetContainerFromEventSource(e.Source);
            if (container != null
                && container.GetVisualsAt(e.GetPosition(container))
                            .Any(c => container == c || container.IsVisualAncestorOf(c)))
            {
                var containerFromEventSource = GetContainerFromEventSource(e.Source);
                if (containerFromEventSource is CollapseItem collapseItem)
                {
                    if (!collapseItem.InAnimating && collapseItem.IsPointInHeaderBounds(e.GetPosition(collapseItem)))
                    {
                        e.Handled = UpdateSelectionFromEventSource(e.Source);
                    }
                }
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsBorderlessProperty)
        {
            SetupEffectiveBorderThickness();
        }
        else if (change.Property == IsAccordionProperty)
        {
            SetupSelectionMode();
        }
        else if (change.Property == IsBorderlessProperty ||
                 change.Property == IsGhostStyleProperty)
        {
            SetupItemsBorderThickness();
        }
        if (change.Property == ItemHeaderPaddingProperty ||
            change.Property == ItemContentPaddingProperty)
        {
            if (Items.Count > 0)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    var item = Items[i];
                    if (item is CollapseItem collapseItem)
                    {
                        ConfigureItemPaddings(collapseItem);
                    }
                }
            }
        }
    }

    private void SetupEffectiveBorderThickness()
    {
        if (IsBorderless || IsGhostStyle)
        {
            EffectiveBorderThickness = default;
        }
        else
        {
            EffectiveBorderThickness = BorderThickness;
        }
    }

    private void SetupSelectionMode()
    {
        if (IsAccordion)
        {
            SelectionMode = SelectionMode.Single | SelectionMode.Toggle;
        }
        else
        {
            SelectionMode = SelectionMode.Multiple | SelectionMode.Toggle;
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupEffectiveBorderThickness();
    }
    
    private void ConfigureItemPaddings(CollapseItem collapseItem)
    {
        if (collapseItem.HeaderPadding == null)
        {
            collapseItem.SetCurrentValue(CollapseItem.HeaderPaddingProperty, ItemHeaderPadding);
        }

        if (collapseItem.ContentPadding == null)
        {
            collapseItem.SetCurrentValue(CollapseItem.ContentPaddingProperty, ItemContentPadding);
        }
    }
}