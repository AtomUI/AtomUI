using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A tab strip whose tabs are line items, with an indicator bar that marks the selected tab.
/// The bar is positioned in code from the bounds of the selected tab's container, so it follows
/// arrangement, scrolling and the offset of a tab reorder preview.
/// </summary>
public partial class TabStrip : BaseTabStrip, ITransitionInitializationOwner
{
    #region 内部属性定义

    internal static readonly StyledProperty<double> SelectedIndicatorThicknessProperty =
        AvaloniaProperty.Register<TabStrip, double>(nameof(SelectedIndicatorThickness));
    
    internal static readonly StyledProperty<ITransform?> SelectedIndicatorRenderTransformProperty = 
        AvaloniaProperty.Register<TabStrip, ITransform?>(nameof (SelectedIndicatorRenderTransform));
    
    internal double SelectedIndicatorThickness
    {
        get => GetValue(SelectedIndicatorThicknessProperty);
        set => SetValue(SelectedIndicatorThicknessProperty, value);
    }
    
    internal ITransform? SelectedIndicatorRenderTransform
    {
        get => GetValue(SelectedIndicatorRenderTransformProperty);
        set => SetValue(SelectedIndicatorRenderTransformProperty, value);
    }
    
    #endregion

    #region 内部协作 API

    internal void NotifyTabReorderPreviewChanged()
    {
        SetupSelectedIndicator();
    }

    #endregion

    private Border? _selectedIndicator;
    private ItemsPresenter? _itemsPresenter;
    private TabScrollViewer? _scrollViewer;
    private IDisposable? _selectedItemBoundsSubscription;

    private readonly TransitionInitializationSession _transitionInitialization;

    public TabStrip()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
        SelectionChanged += HandleSelectionChanged;
    }

    /// <summary>
    /// Updates the indicator from the arranged bounds of the selected tab's container.
    /// Call the base method first, because it arranges the tab containers that the indicator is placed on.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (SelectedItem is not null && ContainerFromItem(SelectedItem) is TabStripItem)
        {
            SetupSelectedIndicator();
        }

        return size;
    }

    /// <summary>
    /// Creates a line-shaped <see cref="TabStripItem" /> and adds <c>TabStripSemanticParts.ItemClass</c> to its
    /// classes. The item, its index and the recycle key do not affect the container.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var tabStripItem = new TabStripItem
        {
            Shape = TabSharp.Line
        };
        tabStripItem.Classes.Add(TabStripSemanticParts.ItemClass);
        return tabStripItem;
    }

    /// <summary>
    /// Calls the base implementation, then keeps a <see cref="TabStripItem" /> container line-shaped and adds
    /// <c>TabStripSemanticParts.ItemClass</c> to its classes.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabStripItem tabStripItem)
        {
            tabStripItem.Shape = TabSharp.Line;
            tabStripItem.Classes.Add(TabStripSemanticParts.ItemClass);
        }
    }

    /// <summary>
    /// Resets the transition initialization session before the base class handles the template change, so
    /// that the indicator state of the new template is written without a transition animation.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Locates the <c>PART_SelectedItemIndicator</c> border, the <c>PART_ItemsPresenter</c> and the
    /// <c>PART_TabsContainer</c> scroller, moves the scroll offset subscription to that scroller, and positions
    /// the indicator. The transition initialization session is reset first, so that the first placement of the
    /// indicator is not animated.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnApplyTemplate(e);
        _selectedIndicator = e.NameScope.Find<Border>("PART_SelectedItemIndicator");
        _itemsPresenter    = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");

        if (_scrollViewer != null)
        {
            _scrollViewer.PropertyChanged -= HandleScrollViewerPropertyChanged;
        }

        _scrollViewer = e.NameScope.Find<TabScrollViewer>("PART_TabsContainer");
        if (_scrollViewer != null)
        {
            _scrollViewer.PropertyChanged += HandleScrollViewerPropertyChanged;
        }

        ConfigureCustomIndicator();
        SubscribeSelectedItemBounds();
        SetupSelectedIndicator();
    }

    /// <summary>
    /// Resets the transition initialization session, subscribes to the bounds of the selected tab's container
    /// and positions the indicator. The bounds subscription was disposed on detach.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnAttachedToVisualTree(e);
        SubscribeSelectedItemBounds();
        SetupSelectedIndicator();
    }

    /// <summary>
    /// Keeps the indicator in sync: a change of <see cref="AbstractTabStrip.IsIndicatorLengthAnimated" /> or
    /// <see cref="AbstractTabStrip.IsSelectedItemCentered" /> registers the custom indicator parts with the base
    /// class again, or clears them, and invalidates the arrangement; a change of
    /// <see cref="AbstractTabStrip.IndicatorMode" />, <see cref="AbstractTabStrip.IndicatorFixedLength" />,
    /// <see cref="AbstractTabStrip.IsIndicatorLengthAnimated" /> or <see cref="SelectedIndicatorThickness" />
    /// repositions the indicator. Call the base method as well.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsIndicatorLengthAnimatedProperty || change.Property == IsSelectedItemCenteredProperty ||
            change.Property == IndicatorMotionModeProperty)
        {
            ConfigureCustomIndicator();
            InvalidateArrange();
        }
        // The ink bar thickness is a Token (InkBarThickness): a theme or Control-level change re-lays the indicator out.
        if (change.Property == IndicatorModeProperty || change.Property == IndicatorFixedLengthProperty ||
            change.Property == IsIndicatorLengthAnimatedProperty || change.Property == SelectedIndicatorThicknessProperty)
        {
            SetupSelectedIndicator();
        }
    }

    private bool UsesComposedIndicator => IsIndicatorLengthAnimated || IndicatorMotionMode == IndicatorMotionMode.Stretch;

    private void ConfigureCustomIndicator()
    {
        if (UsesComposedIndicator || IsSelectedItemCentered)
        {
            SetCustomIndicatorParts(UsesComposedIndicator ? _selectedIndicator : null,
                IsSelectedItemCentered ? _scrollViewer : null, _itemsPresenter);
        }
        else
        {
            ResetCustomIndicatorParts();
        }
    }

    /// <summary>
    /// Updates the indicator when the control has been loaded. This call also completes the transition
    /// initialization session, which only completes on a loaded control.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetupSelectedIndicator();
    }

    /// <summary>
    /// Resets the transition initialization session and disposes the subscription to the bounds of the
    /// selected tab's container. The subscription is created again on the next attach.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        _selectedItemBoundsSubscription?.Dispose();
        _selectedItemBoundsSubscription = null;
        base.OnDetachedFromVisualTree(e);
    }

    #region 实现 ITransitionInitializationOwner 接口

    TransitionInitializationSession ITransitionInitializationOwner.TransitionInitialization => _transitionInitialization;

    #endregion

    private void HandleSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (this.IsAttachedToVisualTree())
        {
            SubscribeSelectedItemBounds();
            SetupSelectedIndicator();
        }
    }

    private void SubscribeSelectedItemBounds()
    {
        _selectedItemBoundsSubscription?.Dispose();
        _selectedItemBoundsSubscription = null;

        if (SelectedItem is not null && ContainerFromItem(SelectedItem) is TabStripItem tabStripItem)
        {
            _selectedItemBoundsSubscription = tabStripItem.GetObservable(Visual.BoundsProperty).Subscribe(_ =>
            {
                SetupSelectedIndicator();
            });
        }
    }
    
    private void SetupSelectedIndicator()
    {
        if (UsesComposedIndicator || IsTabReorderIndicatorRefreshDeferred)
        {
            return;
        }

        if (Items.Count == 0)
        {
            _selectedIndicator?.SetCurrentValue(IsVisibleProperty, false);
            _transitionInitialization.CompleteInitialization();
        }
        else
        {
            if (_selectedIndicator is not null && SelectedItem is not null && ContainerFromItem(SelectedItem) is TabStripItem tabStripItem)
            {
                if (_itemsPresenter is not { IsArrangeValid: true } || !tabStripItem.IsArrangeValid ||
                    !tabStripItem.IsAttachedToVisualTree() ||
                    !double.IsFinite(tabStripItem.Bounds.Width) || !double.IsFinite(tabStripItem.Bounds.Height) ||
                    !MathUtils.GreaterThan(tabStripItem.Bounds.Width, 0) || !MathUtils.GreaterThan(tabStripItem.Bounds.Height, 0))
                {
                    return;
                }
                var selectedBounds = tabStripItem.Bounds;
                var builder        = new TransformOperations.Builder(1);
                var offset         = _itemsPresenter?.Bounds.Position ?? default;
                var previewOffset  = GetTabReorderPreviewPrimaryOffset(tabStripItem);
                if (!double.IsFinite(offset.X) || !double.IsFinite(offset.Y) || !double.IsFinite(previewOffset))
                {
                    return;
                }
                using var snap = !_selectedIndicator.IsVisible && _transitionInitialization.IsReady
                    ? new TransitionSuppressionScope(this)
                    : null;
                _selectedIndicator.SetCurrentValue(IsVisibleProperty, true);


                var vertical = TabStripPlacement is Dock.Left or Dock.Right;
                var itemBounds = new Rect(selectedBounds.Position + offset +
                    (vertical ? new Point(0, previewOffset) : new Point(previewOffset, 0)), selectedBounds.Size);
                var bounds = TabSelectionIndicator.ResolveBounds(tabStripItem, itemBounds,
                    vertical, IndicatorMode, IndicatorFixedLength);
                _selectedIndicator.SetCurrentValue(WidthProperty, vertical ? SelectedIndicatorThickness : bounds.Length);
                _selectedIndicator.SetCurrentValue(HeightProperty, vertical ? bounds.Length : SelectedIndicatorThickness);
                builder.AppendTranslate(vertical ? 0 : bounds.Start, vertical ? bounds.Start : 0);

                var transform = builder.Build();
                var baseTransform = GetBaseValue(SelectedIndicatorRenderTransformProperty);
                if (!baseTransform.HasValue || baseTransform.Value?.Value != transform.Value)
                {
                    SelectedIndicatorRenderTransform = transform;
                }
                _transitionInitialization.CompleteInitialization();
            }
        }
    }

    private void HandleScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScrollViewer.OffsetProperty)
        {
            SetupSelectedIndicator();
        }
    }




}
