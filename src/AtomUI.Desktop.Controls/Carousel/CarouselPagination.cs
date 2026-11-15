using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class CarouselPagination : SelectingItemsControl
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CarouselPagination>();
    
    public static readonly StyledProperty<bool> IsShowTransitionProgressProperty = 
        Carousel.IsShowTransitionProgressProperty.AddOwner<CarouselPagination>();
    
    public static readonly StyledProperty<TimeSpan> AutoPlaySpeedProperty = 
        Carousel.AutoPlaySpeedProperty.AddOwner<CarouselPagination>();
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsShowTransitionProgress
    {
        get => GetValue(IsShowTransitionProgressProperty);
        set => SetValue(IsShowTransitionProgressProperty, value);
    }
    
    public TimeSpan AutoPlaySpeed
    {
        get => GetValue(AutoPlaySpeedProperty);
        set => SetValue(AutoPlaySpeedProperty, value);
    }

    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<CarouselPagination, double>(nameof(ItemSpacing));
    
    internal double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    #endregion

    internal Carousel? Owner { get; set; }

    static CarouselPagination()
    {
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<CarouselPagination>(false);
    }
    
    public CarouselPagination()
    {
        SelectionMode = SelectionMode.Single;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CarouselPageIndicator();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CarouselPageIndicator>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is CarouselPageIndicator pageIndicator)
        {
            if (item != null && item is not Visual)
            {
                if (!pageIndicator.IsSet(CarouselPageIndicator.ContentProperty))
                {
                    pageIndicator.SetCurrentValue(CarouselPageIndicator.ContentProperty, item);
                }
            }
            
            if (ItemTemplate != null)
            {
                pageIndicator[!CarouselPageIndicator.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }
            
            pageIndicator[!IsMotionEnabledProperty]          = this[!IsMotionEnabledProperty];
            pageIndicator[!IsShowTransitionProgressProperty] = this[!IsShowTransitionProgressProperty];
            pageIndicator[!AutoPlaySpeedProperty]            = this[!AutoPlaySpeedProperty];
        
            PreparePageIndicator(pageIndicator, item, index);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type CarouselPageIndicator.");
        }
    }
    
    protected virtual void PreparePageIndicator(CarouselPageIndicator pageIndicator, object? item, int index)
    {
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.Pointer.Type == PointerType.Mouse)
        {
            var container = GetContainerFromEventSource(e.Source);
            if (container != null)
            {
                e.Handled = SelectFromPointer(container, e);
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left && e.Pointer.Type != PointerType.Mouse)
        {
            var container = GetContainerFromEventSource(e.Source);
            if (container != null
                && container.ContainsSelfOrDescendantAt(e.GetPosition(container)))
            {
                e.Handled = SelectFromPointer(container, e);
            }
        }
    }

    private bool SelectFromPointer(Control container, PointerEventArgs input)
    {
        if (Owner is null)
        {
            return UpdateSelectionFromEvent(container, input);
        }

        var index = IndexFromContainer(container);
        if (index < 0)
        {
            return false;
        }
        Owner.SelectPageFromPagination(index);
        container.PerformFeedback(FeedbackAction.Click);
        return true;
    }
}
