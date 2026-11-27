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
/// A tab control with line tabs. The selected tab is marked by an ink bar, each item is hosted in a line-shaped
/// <see cref="TabItem" />, and the content area shows the content of the selected tab.
/// </summary>
public partial class TabControl : BaseTabControl, ITransitionInitializationOwner
{
    #region 内部属性定义

    internal static readonly StyledProperty<double> SelectedIndicatorThicknessProperty =
        AvaloniaProperty.Register<TabControl, double>(nameof(SelectedIndicatorThickness));
    
    internal static readonly StyledProperty<ITransform?> SelectedIndicatorRenderTransformProperty = 
        AvaloniaProperty.Register<TabControl, ITransform?>(nameof (SelectedIndicatorRenderTransform));
    
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

    public TabControl()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
        SelectionChanged += HandleSelectionChanged;
    }

    /// <summary>
    /// Arranges the control and then places the ink bar on the selected tab. Call the base member first: the ink
    /// bar is placed from the bounds of the selected tab, which the base arrange pass produces.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (SelectedItem is not null && ContainerFromItem(SelectedItem) is TabItem)
        {
            SetupSelectedIndicator();
        }

        return size;
    }

    /// <summary>
    /// Creates the container for an item that is not a <see cref="TabItem" /> already: a new line-shaped tab that
    /// carries the semantic item class.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var tabItem = new TabItem
        {
            Shape = TabSharp.Line
        };
        tabItem.Classes.Add(TabControlSemanticParts.ItemClass);
        return tabItem;
    }

    /// <summary>
    /// Calls the base member and then gives a <see cref="TabItem" /> container the line shape and the semantic item
    /// class. This also covers the tabs that the app supplied as items.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabItem tabItem)
        {
            tabItem.Shape = TabSharp.Line;
            tabItem.Classes.Add(TabControlSemanticParts.ItemClass);
        }
    }

    /// <summary>
    /// Resets the transition initialization of the ink bar, then calls the base member. The ink bar is placed again
    /// for the new template, and that first placement is not animated.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Applies the template and sets up the ink bar. The transition initialization is reset and the base member is
    /// called; then the ink bar, the items presenter and the tab scroll viewer are looked up, the scroll offset
    /// subscription moves to the new scroll viewer, and the ink bar is configured and placed.
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
    /// Resets the transition initialization, calls the base member, and then follows the bounds of the selected tab
    /// and places the ink bar.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnAttachedToVisualTree(e);
        SubscribeSelectedItemBounds();
        SetupSelectedIndicator();
    }

    /// <summary>
    /// Resets the transition initialization and stops following the bounds of the selected tab before the base
    /// member is called.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        _selectedItemBoundsSubscription?.Dispose();
        _selectedItemBoundsSubscription = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base member and then keeps the ink bar in step with the change. A change of the length animation
    /// or the centering mode hands the template parts to the base class and requests a new arrange pass. A change
    /// of the indicator mode, the fixed indicator length or the ink bar thickness places the ink bar again.
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
    /// Calls the base member and then places the ink bar.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetupSelectedIndicator();
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

        if (SelectedItem is not null && ContainerFromItem(SelectedItem) is TabItem tabItem)
        {
            _selectedItemBoundsSubscription = tabItem.GetObservable(Visual.BoundsProperty).Subscribe(_ =>
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
            if (_selectedIndicator is not null && SelectedItem is not null && ContainerFromItem(SelectedItem) is TabItem tabItem)
            {
                if (_itemsPresenter is not { IsArrangeValid: true } || !tabItem.IsArrangeValid ||
                    !tabItem.IsAttachedToVisualTree() ||
                    !double.IsFinite(tabItem.Bounds.Width) || !double.IsFinite(tabItem.Bounds.Height) ||
                    !MathUtils.GreaterThan(tabItem.Bounds.Width, 0) || !MathUtils.GreaterThan(tabItem.Bounds.Height, 0))
                {
                    return;
                }
                var selectedBounds = tabItem.Bounds;
                var builder        = new TransformOperations.Builder(1);
                var offset         = _itemsPresenter?.Bounds.Position ?? default;
                var previewOffset  = GetTabReorderPreviewPrimaryOffset(tabItem);
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
                var bounds = TabSelectionIndicator.ResolveBounds(tabItem, itemBounds,
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
