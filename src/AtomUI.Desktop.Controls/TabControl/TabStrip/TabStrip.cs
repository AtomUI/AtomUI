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

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (SelectedItem is not null && ContainerFromItem(SelectedItem) is TabStripItem)
        {
            SetupSelectedIndicator();
        }

        return size;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var tabStripItem = new TabStripItem
        {
            Shape = TabSharp.Line
        };
        tabStripItem.Classes.Add(TabStripSemanticParts.ItemClass);
        return tabStripItem;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabStripItem tabStripItem)
        {
            tabStripItem.Shape = TabSharp.Line;
            tabStripItem.Classes.Add(TabStripSemanticParts.ItemClass);
        }
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

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

        SubscribeSelectedItemBounds();
        SetupSelectedIndicator();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnAttachedToVisualTree(e);
        SubscribeSelectedItemBounds();
        SetupSelectedIndicator();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetupSelectedIndicator();
    }

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
        if (IsTabReorderIndicatorRefreshDeferred)
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
                var selectedSize   = selectedBounds.Size;
                var previewOffset  = GetTabReorderPreviewPrimaryOffset(tabStripItem);
                if (!double.IsFinite(offset.X) || !double.IsFinite(offset.Y) || !double.IsFinite(previewOffset))
                {
                    return;
                }
                using var snap = !_selectedIndicator.IsVisible && _transitionInitialization.IsReady
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
