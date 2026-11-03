using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia.VisualTree;
using AtomUI.Theme.Resources;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class CarouselPagination : SelectingItemsControl, IControlThemeGlobalOwnerSource
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CarouselPagination>();

    public static readonly StyledProperty<bool> IsShowTransitionProgressProperty = 
        Carousel.IsShowTransitionProgressProperty.AddOwner<CarouselPagination>();

    public static readonly StyledProperty<TimeSpan> AutoPlaySpeedProperty = 
        Carousel.AutoPlaySpeedProperty.AddOwner<CarouselPagination>();

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<CarouselPagination, double>(nameof(ItemSpacing));

    #endregion

    private readonly Dictionary<CarouselPageIndicator, CompositeDisposable> _containerBindings = new();

    #region 公共属性定义

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

    internal double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    #endregion

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
            
            ConfigureContainerBindings(pageIndicator);

            PreparePageIndicator(pageIndicator, item, index);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type CarouselPageIndicator.");
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is CarouselPageIndicator indicator && _containerBindings.Remove(indicator, out var bindings))
        {
            bindings.Dispose();
        }
        base.ClearContainerForItemOverride(container);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        for (var index = 0; index < ItemCount; index++)
        {
            if (ContainerFromIndex(index) is CarouselPageIndicator indicator && !_containerBindings.ContainsKey(indicator))
            {
                ConfigureContainerBindings(indicator);
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        foreach (var bindings in _containerBindings.Values)
        {
            bindings.Dispose();
        }
        _containerBindings.Clear();
        base.OnDetachedFromVisualTree(e);
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
                e.Handled = UpdateSelectionFromEvent(container, e);
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
                e.Handled = UpdateSelectionFromEvent(container, e);
            }
        }
    }

    #region 实现 IControlThemeGlobalOwnerSource 接口

    Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => CarouselThemeOwner.Resolve(this);

    #endregion

    private void ConfigureContainerBindings(CarouselPageIndicator indicator)
    {
        if (_containerBindings.Remove(indicator, out var previous))
        {
            previous.Dispose();
        }
        // Runtime containers receive Local projections during Prepare, before
        // logical membership. ControlTheme cannot preserve that timing/priority;
        // each membership owns and retires these typed relay handles.
        var bindings = new CompositeDisposable
        {
            BindUtils.RelayBind(this, IsMotionEnabledProperty, indicator, CarouselPageIndicator.IsMotionEnabledProperty),
            BindUtils.RelayBind(this, IsShowTransitionProgressProperty, indicator, CarouselPageIndicator.IsShowTransitionProgressProperty),
            BindUtils.RelayBind(this, AutoPlaySpeedProperty, indicator, CarouselPageIndicator.AutoPlaySpeedProperty)
        };
        if (ItemTemplate != null)
        {
            bindings.Add(BindUtils.RelayBind(this, ItemTemplateProperty, indicator, CarouselPageIndicator.ContentTemplateProperty));
        }
        _containerBindings.Add(indicator, bindings);
    }
}
