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
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

/// <summary>
/// Container of one entry row in a <see cref="ListBox" />, generated from the item source or supplied by the
/// application.
/// </summary>
// Token-owner transparent item container: the item theme reads the hosting list's Tokens (ListBox, or the host of a derived
// list such as the Cascader filter list), so Controls[ListBox] also reaches the items and their transitions.
[TokenOwnerTransparent]
public class ListBoxItem : AvaloniaListBoxItem, IListItemVirtualizingContextAware, ITransitionInitializationOwner
{
    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="Clicked" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickedEvent =
        RoutedEvent.Register<ListBoxItem, RoutedEventArgs>(
            nameof(Clicked),
            RoutingStrategies.Bubble);
    
    /// <summary>
    /// Raised when the item is pressed with the left button, on press rather than on release. The event bubbles.
    /// It is raised only when the press reached the item unhandled and the item is hosted by a <see cref="ListBox" />.
    /// The selection of a mouse press is already updated at that moment; the selection of a touch or pen press follows
    /// on release.
    /// </summary>
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

    internal static readonly StyledProperty<IBrush?> ItemSelectedIndicatorColorProperty =
        ListBox.ItemSelectedIndicatorColorProperty.AddOwner<ListBoxItem>();

    internal IBrush? ItemSelectedIndicatorColor
    {
        get => GetValue(ItemSelectedIndicatorColorProperty);
        set => SetValue(ItemSelectedIndicatorColorProperty, value);
    }
    
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

    // The default split line under the item is SplitLineWidth thick (the theme reads the host's LineWidth); a BorderThickness
    // set by the application or a semantic style replaces it as a whole.
    internal static readonly StyledProperty<double> SplitLineWidthProperty =
        AvaloniaProperty.Register<ListBoxItem, double>(nameof(SplitLineWidth));

    internal double SplitLineWidth
    {
        get => GetValue(SplitLineWidthProperty);
        set => SetValue(SplitLineWidthProperty, value);
    }

    internal static readonly DirectProperty<ListBoxItem, bool> IsSplitLineEffectiveVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsSplitLineEffectiveVisible),
            o => o.IsSplitLineEffectiveVisible,
            (o, v) => o.IsSplitLineEffectiveVisible = v);

    /// <summary>
    /// Size step of the item. The item shares the <c>SizeType</c> styled property of the other size-type aware
    /// controls and does not change the value itself. The theme takes the minimum height and the padding of the item
    /// from the tokens of the step; <see cref="CustomizableSizeType.Custom" /> uses the values of the middle step.
    /// </summary>
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
    
    /// <summary>
    /// How the filter words are rendered in the text of the item while the list filters, as a combination of the
    /// <see cref="TextBlockHighlightStrategy" /> values. <see cref="TextBlockHighlightStrategy.HighlightedMatch" />
    /// colors the matches with <see cref="FilterHighlightForeground" /> and makes them bold when
    /// <see cref="TextBlockHighlightStrategy.BoldedMatch" /> is also set;
    /// <see cref="TextBlockHighlightStrategy.HighlightedWhole" /> colors the whole text instead when
    /// <see cref="TextBlockHighlightStrategy.HighlightedMatch" /> is not set, and
    /// <see cref="TextBlockHighlightStrategy.HideUnMatched" /> does not change the item text. The default is
    /// <see cref="TextBlockHighlightStrategy.All" />. <see cref="ListBox" /> binds this property on its items to its
    /// own <see cref="ListBox.FilterHighlightStrategy" />, so set the value on the list.
    /// </summary>
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
    
    /// <summary>
    /// Foreground of the filter matches that <see cref="FilterHighlightStrategy" /> highlights in the text of the item
    /// while the list filters. The default is <see langword="null" />, which leaves them in the foreground of the item,
    /// so they then stand out only through the bold weight. <see cref="ListBox" /> binds this property on its items to
    /// its own <see cref="ListBox.FilterHighlightForeground" />, whose theme value is
    /// <see cref="ListBoxToken.FilterHighlightColor" />; set the value on the list or through that Token.
    /// </summary>
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

    private readonly TransitionInitializationSession _transitionInitialization;

    public ListBoxItem()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Completes the transition initialization session once the base method has applied the template. An override that
    /// does not call the base method must still complete the initialization.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Resets the transition initialization session, then lets the base method run. An override that does not call
    /// the base method must still reset the session, so that it is completed again for the new template by
    /// <see cref="OnApplyTemplate" /> or <see cref="OnLoaded" />.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Resets the transition initialization session, then calls the base method. An override that does not call
    /// the base method must still reset the session, so that it is completed again by <see cref="OnLoaded" />,
    /// when the item is attached once more.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Completes the transition initialization session when the item is loaded. An override that does not call the base
    /// method must still complete the initialization.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Keeps the derived item states in step with their sources: the visibility of the selection indicator follows
    /// <c>IsSelected</c> and <see cref="IsShowSelectedIndicator" />, <see cref="ContentText" /> follows
    /// <c>Content</c> when it is a string or an <see cref="IListItemData" />, <see cref="FilterHighlightWords" />
    /// follows <see cref="FilterValue" />, and <see cref="EffectiveBorderThickness" /> with
    /// <see cref="IsSplitLineEffectiveVisible" /> follow <c>BorderThickness</c>, <see cref="SplitLineWidth" /> and
    /// <see cref="IsSplitLineVisible" />. An override that does not call the base method must still update them.
    /// </summary>
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
                 change.Property == SplitLineWidthProperty ||
                 change.Property == IsSplitLineVisibleProperty)
        {
            ConfigureEffectiveBorderThickness();
        }
    }

    /// <summary>
    /// Applies the initial visibility of the selection indicator and the initial effective border thickness from the
    /// current property values. An override that does not call the base method must still apply them.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureSelectedIndicator();
        ConfigureEffectiveBorderThickness();

    }





    /// <summary>
    /// Lets the owning <see cref="ListBox" /> update its selection, then raises <see cref="Clicked" /> for a left button
    /// press. A mouse press, or the right button of a pen, selects at once; a touch, or the left button of a pen, only
    /// records the press point and selects on release, so that it does not interfere with scrolling gestures. Nothing
    /// happens when the press is already handled or the item is not hosted by a <see cref="ListBox" />. The base method
    /// is not called.
    /// </summary>
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

                    // Ideally the press would be marked handled here, but that would prevent the scroll gesture
                    // recognizer from working, so it is left unhandled.
                }
            }

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                RaiseEvent(new RoutedEventArgs(ClickedEvent, this));
            }
        }
    }

    /// <summary>
    /// Requests the context menu when the right button is released on the item itself, and completes the selection of a
    /// touch or left pen press recorded by <see cref="OnPointerPressed" /> when the pointer is released unhandled within
    /// the tap tolerance inside the item. The list then updates its selection and the release is marked handled; for
    /// touch and pen the release is also raised on the list, which lets the list commit the selection. The base method
    /// is not called.
    /// </summary>
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

    #region 实现 ITransitionInitializationOwner 接口

    TransitionInitializationSession ITransitionInitializationOwner.TransitionInitialization => _transitionInitialization;

    #endregion

    private void ConfigureSelectedIndicator()
    {
        SetCurrentValue(IsSelectedIndicatorVisibleProperty, IsShowSelectedIndicator && IsSelected);
    }

    private void ConfigureEffectiveBorderThickness()
    {
        var thickness     = IsSet(BorderThicknessProperty) ? BorderThickness : new Thickness(0, 0, 0, SplitLineWidth);
        var showSplitLine = IsSplitLineVisible && thickness.Bottom > 0;
        IsSplitLineEffectiveVisible = showSplitLine;
        EffectiveBorderThickness = showSplitLine ? thickness : new Thickness(0);
    }


}