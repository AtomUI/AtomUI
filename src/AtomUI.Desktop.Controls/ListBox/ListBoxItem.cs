using Avalonia.Controls.Primitives;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

public class ListBoxItem : AvaloniaListBoxItem, IListItemVirtualizingContextAware, IStateMotionOwner
{
    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickedEvent =
        RoutedEvent.Register<ListBoxItem, RoutedEventArgs>(
            nameof(Clicked),
            RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Clicked
    {
        add => AddHandler(ClickedEvent, value);
        remove => RemoveHandler(ClickedEvent, value);
    }

    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ListBoxItem>();
    
    internal static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        ListBox.ItemHoverBgProperty.AddOwner<ListBoxItem>();
    
    internal static readonly StyledProperty<IBrush?> ItemSelectedBgProperty =
        ListBox.ItemSelectedBgProperty.AddOwner<ListBoxItem>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ListBoxItem>();
    
    internal static readonly DirectProperty<ListBoxItem, bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsShowSelectedIndicator),
            o => o.IsShowSelectedIndicator,
            (o, v) => o.IsShowSelectedIndicator = v);
    
    internal static readonly DirectProperty<ListBoxItem, bool> IsSelectedIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsSelectedIndicatorVisible),
            o => o.IsSelectedIndicatorVisible,
            (o, v) => o.IsSelectedIndicatorVisible = v);
    
    internal static readonly DirectProperty<ListBoxItem, IconTemplate?> SelectedIndicatorProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, IconTemplate?>(nameof(SelectedIndicator),
            o => o.SelectedIndicator,
            (o, v) => o.SelectedIndicator = v);
    
    internal static readonly DirectProperty<ListBoxItem, bool> IsFilteringProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsFiltering),
            o => o.IsFiltering,
            (o, v) => o.IsFiltering = v);
    
    internal static readonly DirectProperty<ListBoxItem, object?> FilterValueProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, object?>(nameof(FilterValue),
            o => o.FilterValue,
            (o, v) => o.FilterValue = v);
    
    internal static readonly DirectProperty<ListBoxItem, TextBlockHighlightStrategy> FilterHighlightStrategyProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, TextBlockHighlightStrategy>(
            nameof(FilterHighlightStrategy),
            o => o.FilterHighlightStrategy,
            (o, v) => o.FilterHighlightStrategy = v);
    
    internal static readonly DirectProperty<ListBoxItem, string?> FilterHighlightWordsProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, string?>(
            nameof(FilterHighlightWords), t => t.FilterHighlightWords, 
            (t, v) => t.FilterHighlightWords = v);
    
    internal static readonly DirectProperty<ListBoxItem, string?> ContentTextProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, string?>(
            nameof(ContentText), t => t.ContentText, 
            (t, v) => t.ContentText = v);
    
    internal static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        ListBox.FilterHighlightForegroundProperty.AddOwner<ListBoxItem>();
    
    internal static readonly DirectProperty<ListBoxItem, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);

    internal static readonly DirectProperty<ListBoxItem, bool> IsSplitLineVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsSplitLineVisible),
            o => o.IsSplitLineVisible,
            (o, v) => o.IsSplitLineVisible = v);

    internal static readonly DirectProperty<ListBoxItem, bool> IsSplitLineEffectiveVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsSplitLineEffectiveVisible),
            o => o.IsSplitLineEffectiveVisible,
            (o, v) => o.IsSplitLineEffectiveVisible = v);

    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal IBrush? ItemHoverBg
    {
        get => GetValue(ItemHoverBgProperty);
        set => SetValue(ItemHoverBgProperty, value);
    }
    
    internal IBrush? ItemSelectedBg
    {
        get => GetValue(ItemSelectedBgProperty);
        set => SetValue(ItemSelectedBgProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isShowSelectedIndicator;

    internal bool IsShowSelectedIndicator
    {
        get => _isShowSelectedIndicator;
        set => SetAndRaise(IsShowSelectedIndicatorProperty, ref _isShowSelectedIndicator, value);
    }
    
    private bool _isSelectedIndicatorVisible;

    internal bool IsSelectedIndicatorVisible
    {
        get => _isSelectedIndicatorVisible;
        set => SetAndRaise(IsSelectedIndicatorVisibleProperty, ref _isSelectedIndicatorVisible, value);
    }
    
    private IconTemplate? _selectedIndicator;

    internal IconTemplate? SelectedIndicator
    {
        get => _selectedIndicator;
        set => SetAndRaise(SelectedIndicatorProperty, ref _selectedIndicator, value);
    }
    
    private bool _isFiltering;

    internal bool IsFiltering
    {
        get => _isFiltering;
        set => SetAndRaise(IsFilteringProperty, ref _isFiltering, value);
    }
    
    private object? _filterValue;

    internal object? FilterValue
    {
        get => _filterValue;
        set => SetAndRaise(FilterValueProperty, ref _filterValue, value);
    }
    
    private TextBlockHighlightStrategy _filterHighlightStrategy = TextBlockHighlightStrategy.All;
    
    public TextBlockHighlightStrategy FilterHighlightStrategy
    {
        get => _filterHighlightStrategy;
        set => SetAndRaise(FilterHighlightStrategyProperty, ref _filterHighlightStrategy, value);
    }
    
    private string? _filterHighlightWords;
    internal string? FilterHighlightWords
    {
        get => _filterHighlightWords;
        set => SetAndRaise(FilterHighlightWordsProperty, ref _filterHighlightWords, value);
    }
    
    private string? _contentText;
    internal string? ContentText
    {
        get => _contentText;
        set => SetAndRaise(ContentTextProperty, ref _contentText, value);
    }
    
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }

    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        private set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }

    private bool _isSplitLineVisible;

    internal bool IsSplitLineVisible
    {
        get => _isSplitLineVisible;
        set => SetAndRaise(IsSplitLineVisibleProperty, ref _isSplitLineVisible, value);
    }

    private bool _isSplitLineEffectiveVisible;

    internal bool IsSplitLineEffectiveVisible
    {
        get => _isSplitLineEffectiveVisible;
        private set => SetAndRaise(IsSplitLineEffectiveVisibleProperty, ref _isSplitLineEffectiveVisible, value);
    }
    #endregion
    
    private static readonly Point s_invalidPoint = new(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;

    private readonly StateMotionSession _stateMotion;

    public ListBoxItem()
    {
        _stateMotion = new StateMotionSession(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _stateMotion.CompleteInitialization();
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _stateMotion?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _stateMotion.CompleteInitialization();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsSelectedProperty ||
            change.Property == IsShowSelectedIndicatorProperty)
        {
            ConfigureSelectedIndicator();
        }
        else if (change.Property == ContentProperty)
        {
            if (Content is IListItemData listBoxItemData)
            {
                ContentText = listBoxItemData.Content as string;
            }
            else if (Content is string strContent)
            {
                ContentText = strContent;
            }
        }
        else if (change.Property == FilterValueProperty)
        {
            FilterHighlightWords = FilterValue?.ToString();
        }
        else if (change.Property == BorderThicknessProperty ||
                 change.Property == IsSplitLineVisibleProperty)
        {
            ConfigureEffectiveBorderThickness();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureSelectedIndicator();
        ConfigureEffectiveBorderThickness();

    }





    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _pointerDownPoint = s_invalidPoint;

        if (e.Handled)
        {
            return;
        }

        if (!e.Handled && ItemsControl.ItemsControlFromItemContainer(this) is ListBox owner)
        {
            var p = e.GetCurrentPoint(this);

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or 
                PointerUpdateKind.RightButtonPressed)
            {
                if (p.Pointer.Type == PointerType.Mouse || (p.Pointer.Type == PointerType.Pen && p.Properties.IsRightButtonPressed))
                {
                    // If the pressed point comes from a mouse or right-click pen, perform the selection immediately.
                    // In case of pen, only right-click is accepted, as left click (a tip touch) is used for scrolling. 
                    e.Handled = owner.UpdateSelectionFromPointerEvent(this, e);
                }
                else
                {
                    // Otherwise perform the selection when the pointer is released as to not
                    // interfere with gestures.
                    _pointerDownPoint = p.Position;

                    // Ideally we'd set handled here, but that would prevent the scroll gesture
                    // recognizer from working.
                    // e.Handled = true;
                }
            }

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                RaiseEvent(new RoutedEventArgs(ClickedEvent, this));
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.Source == this && !e.Handled && e.InitialPressMouseButton == MouseButton.Right)
        {
            var args = new ContextRequestedEventArgs(e);
            RaiseEvent(args);
            e.Handled = args.Handled;
        }
        if (!e.Handled && !double.IsNaN(_pointerDownPoint.X) &&
            e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point    = e.GetCurrentPoint(this);
            var settings = (e.Source as Visual)?.GetPlatformSettings();
            var tapSize  = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position) &&
                ItemsControl.ItemsControlFromItemContainer(this) is ListBox owner)
            {
                if (owner.UpdateSelectionFromPointerEvent(this, e))
                {
                    // As we only update selection from touch/pen on pointer release, we need to raise
                    // the pointer event on the owner to trigger a commit.
                    if (e.Pointer.Type != PointerType.Mouse)
                    {
                        var sourceBackup = e.Source;
                        owner.RaiseEvent(e);
                        e.Source = sourceBackup;
                    }
                    e.Handled = true;
                }
            }
        }

        _pointerDownPoint = s_invalidPoint;
    }
    #region 实现 IListItemVirtualizingContextAware 接口

    int IListItemVirtualizingContextAware.VirtualIndex { get; set; } = -1;
    bool IListItemVirtualizingContextAware.VirtualContextOperating { get; set; }

    #endregion

    #region 实现 IStateMotionOwner 接口

    StateMotionSession IStateMotionOwner.StateMotionSession => _stateMotion;

    #endregion

    private void ConfigureSelectedIndicator()
    {
        SetCurrentValue(IsSelectedIndicatorVisibleProperty, IsShowSelectedIndicator && IsSelected);
    }

    private void ConfigureEffectiveBorderThickness()
    {
        var showSplitLine = IsSplitLineVisible && BorderThickness.Bottom > 0;
        IsSplitLineEffectiveVisible = showSplitLine;
        EffectiveBorderThickness = showSplitLine ? BorderThickness : new Thickness(0);
    }


}