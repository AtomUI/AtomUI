using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class TabControl : BaseTabControl
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

    private Border? _selectedIndicator;
    private ItemsPresenter? _itemsPresenter;
    private TabControlScrollViewer? _scrollViewer;
    private bool _isScrollSubscriptionHooked;

    public TabControl()
    {
        SelectionChanged += HandleSelectionChanged;
    }

    private void HandleSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (this.IsAttachedToVisualTree())
        {
            SetupSelectedIndicator();
        }
    }
    
    private void SetupSelectedIndicator()
    {
        if (Items.Count == 0)
        {
            _selectedIndicator?.SetCurrentValue(IsVisibleProperty, false);
        }
        else
        {
            if (_selectedIndicator is not null && SelectedItem is not null && ContainerFromItem(SelectedItem) is TabItem tabItem)
            {
                _selectedIndicator.SetCurrentValue(IsVisibleProperty, true);
                var selectedBounds = tabItem.Bounds;
                var builder        = new TransformOperations.Builder(1);
                var offset         = _itemsPresenter?.Bounds.Position ?? default;

                if (TabStripPlacement == Dock.Top)
                {
                    _selectedIndicator.SetCurrentValue(WidthProperty, tabItem.DesiredSize.Width);
                    _selectedIndicator.SetCurrentValue(HeightProperty, SelectedIndicatorThickness);
                    builder.AppendTranslate(offset.X + selectedBounds.Left, 0);
                }
                else if (TabStripPlacement == Dock.Right)
                {
                    _selectedIndicator.SetCurrentValue(HeightProperty, tabItem.DesiredSize.Height);
                    _selectedIndicator.SetCurrentValue(WidthProperty, SelectedIndicatorThickness);
                    builder.AppendTranslate(0, offset.Y + selectedBounds.Y);
                }
                else if (TabStripPlacement == Dock.Bottom)
                {
                    _selectedIndicator.SetCurrentValue(WidthProperty, tabItem.DesiredSize.Width);
                    _selectedIndicator.SetCurrentValue(HeightProperty, SelectedIndicatorThickness);
                    builder.AppendTranslate(offset.X + selectedBounds.Left, 0);
                }
                else
                {
                    _selectedIndicator.SetCurrentValue(HeightProperty, tabItem.DesiredSize.Height);
                    _selectedIndicator.SetCurrentValue(WidthProperty, SelectedIndicatorThickness);
                    builder.AppendTranslate(0, offset.Y + selectedBounds.Y);
                }

                SelectedIndicatorRenderTransform = builder.Build();
            }
        }
        
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
        return tabItem;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabItem tabItem)
        {
            tabItem.Shape = TabSharp.Line;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _selectedIndicator = e.NameScope.Find<Border>(TabControlThemeConstants.SelectedItemIndicatorPart);
        _itemsPresenter    = e.NameScope.Find<ItemsPresenter>(TabControlThemeConstants.ItemsPresenterPart);
        
        if (_scrollViewer != null && _isScrollSubscriptionHooked)
        {
            _scrollViewer.PropertyChanged -= HandleScrollViewerPropertyChanged;
            _isScrollSubscriptionHooked   = false;
        }

        _scrollViewer = e.NameScope.Find<TabControlScrollViewer>(TabControlThemeConstants.TabsContainerPart);
        if (_scrollViewer != null)
        {
            _scrollViewer.TabControl      = this;
            _scrollViewer.PropertyChanged += HandleScrollViewerPropertyChanged;
            _isScrollSubscriptionHooked    = true;
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<TransformOperationsTransition>(SelectedIndicatorRenderTransformProperty,
                        SharedTokenKey.MotionDurationSlow, new ExponentialEaseOut())
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (_scrollViewer != null && _isScrollSubscriptionHooked)
        {
            _scrollViewer.PropertyChanged -= HandleScrollViewerPropertyChanged;
            _isScrollSubscriptionHooked    = false;
        }
        Transitions = null;
    }

    private void HandleScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScrollViewer.OffsetProperty)
        {
            SetupSelectedIndicator();
        }
    }
}
