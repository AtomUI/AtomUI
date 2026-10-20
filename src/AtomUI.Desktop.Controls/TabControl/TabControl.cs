using AtomUI.Animations;
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

public partial class TabControl : BaseTabControl, IStateMotionOwner
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

    private readonly StateMotionSession _stateMotion;

    public TabControl()
    {
        _stateMotion = new StateMotionSession(this);
        SelectionChanged += HandleSelectionChanged;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (SelectedItem is not null && ContainerFromItem(SelectedItem) is TabItem)
        {
            SetupSelectedIndicator();
        }

        return size;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var tabItem = new TabItem
        {
            Shape = TabSharp.Line
        };
        tabItem.Classes.Add(TabControlSemanticParts.ItemClass);
        return tabItem;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabItem tabItem)
        {
            tabItem.Shape = TabSharp.Line;
            tabItem.Classes.Add(TabControlSemanticParts.ItemClass);
        }
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _stateMotion?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _stateMotion.Reset();
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

        SubscribeSelectedItemBounds();
        SetupSelectedIndicator();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        base.OnAttachedToVisualTree(e);
        SubscribeSelectedItemBounds();
        SetupSelectedIndicator();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        _selectedItemBoundsSubscription?.Dispose();
        _selectedItemBoundsSubscription = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetupSelectedIndicator();
    }
    #region 实现 IStateMotionOwner 接口

    StateMotionSession IStateMotionOwner.StateMotionSession => _stateMotion;

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
        if (IsTabReorderIndicatorRefreshDeferred)
        {
            return;
        }

        if (Items.Count == 0)
        {
            _selectedIndicator?.SetCurrentValue(IsVisibleProperty, false);
            _stateMotion.CompleteInitialization();
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
                var selectedSize   = selectedBounds.Size;
                var previewOffset  = GetTabReorderPreviewPrimaryOffset(tabItem);
                if (!double.IsFinite(offset.X) || !double.IsFinite(offset.Y) || !double.IsFinite(previewOffset))
                {
                    return;
                }
                using var snap = !_selectedIndicator.IsVisible && _stateMotion.IsReady
                    ? new TransitionSuppressionScope(this)
                    : null;
                _selectedIndicator.SetCurrentValue(IsVisibleProperty, true);


                if (TabStripPlacement == Dock.Top)
                {
                    _selectedIndicator.SetCurrentValue(WidthProperty, selectedSize.Width);
                    _selectedIndicator.SetCurrentValue(HeightProperty, SelectedIndicatorThickness);
                    builder.AppendTranslate(offset.X + selectedBounds.Left + previewOffset, 0);
                }
                else if (TabStripPlacement == Dock.Right)
                {
                    _selectedIndicator.SetCurrentValue(HeightProperty, selectedSize.Height);
                    _selectedIndicator.SetCurrentValue(WidthProperty, SelectedIndicatorThickness);
                    builder.AppendTranslate(0, offset.Y + selectedBounds.Y + previewOffset);
                }
                else if (TabStripPlacement == Dock.Bottom)
                {
                    _selectedIndicator.SetCurrentValue(WidthProperty, selectedSize.Width);
                    _selectedIndicator.SetCurrentValue(HeightProperty, SelectedIndicatorThickness);
                    builder.AppendTranslate(offset.X + selectedBounds.Left + previewOffset, 0);
                }
                else
                {
                    _selectedIndicator.SetCurrentValue(HeightProperty, selectedSize.Height);
                    _selectedIndicator.SetCurrentValue(WidthProperty, SelectedIndicatorThickness);
                    builder.AppendTranslate(0, offset.Y + selectedBounds.Y + previewOffset);
                }

                var transform = builder.Build();
                var baseTransform = GetBaseValue(SelectedIndicatorRenderTransformProperty);
                if (!baseTransform.HasValue || baseTransform.Value?.Value != transform.Value)
                {
                    SelectedIndicatorRenderTransform = transform;
                }
                _stateMotion.CompleteInitialization();
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
