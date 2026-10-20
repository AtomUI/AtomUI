using AtomUI.Animations;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

public abstract class AbstractSegmented : SelectingItemsControl,
                                          IMotionAwareControl,
                                          ICustomizableSizeTypeAware,
                                          IFormItemAware,
                                          IStateMotionOwner
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSegmented>();

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AbstractSegmented>();

    public static readonly StyledProperty<SegmentedShape> ShapeProperty =
        AvaloniaProperty.Register<AbstractSegmented, SegmentedShape>(nameof(Shape));

    public static readonly StyledProperty<bool> IsExpandingProperty =
        AvaloniaProperty.Register<AbstractSegmented, bool>(nameof(IsExpanding));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSegmented>();

    public static readonly StyledProperty<CornerRadius> SelectedThumbCornerRadiusProperty =
        AvaloniaProperty.Register<AbstractSegmented, CornerRadius>(nameof(SelectedThumbCornerRadius));

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public SegmentedShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public bool IsExpanding
    {
        get => GetValue(IsExpandingProperty);
        set => SetValue(IsExpandingProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<IBrush?> SelectedThumbBgProperty =
        AvaloniaProperty.Register<AbstractSegmented, IBrush?>(
            nameof(SelectedThumbBg));

    internal static readonly StyledProperty<BoxShadows> SelectedThumbBoxShadowsProperty =
        AvaloniaProperty.Register<AbstractSegmented, BoxShadows>(
            nameof(SelectedThumbBoxShadows));

    internal static readonly StyledProperty<Size> SelectedThumbSizeProperty =
        AvaloniaProperty.Register<AbstractSegmented, Size>(nameof(SelectedThumbSize));

    internal static readonly StyledProperty<Point> SelectedThumbPosProperty =
        AvaloniaProperty.Register<AbstractSegmented, Point>(nameof(SelectedThumbPos));

    internal CornerRadius SelectedThumbCornerRadius
    {
        get => GetValue(SelectedThumbCornerRadiusProperty);
        set => SetValue(SelectedThumbCornerRadiusProperty, value);
    }

    internal IBrush? SelectedThumbBg
    {
        get => GetValue(SelectedThumbBgProperty);
        set => SetValue(SelectedThumbBgProperty, value);
    }

    internal BoxShadows SelectedThumbBoxShadows
    {
        get => GetValue(SelectedThumbBoxShadowsProperty);
        set => SetValue(SelectedThumbBoxShadowsProperty, value);
    }

    // 内部动画属性
    internal Size SelectedThumbSize
    {
        get => GetValue(SelectedThumbSizeProperty);
        set => SetValue(SelectedThumbSizeProperty, value);
    }

    internal Point SelectedThumbPos
    {
        get => GetValue(SelectedThumbPosProperty);
        set => SetValue(SelectedThumbPosProperty, value);
    }
    
    #endregion

    #region 内部协作 API

    internal bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        return UpdateSelectionFromEvent(source, e);
    }

    #endregion

    private EventHandler? _formValueChanged;
    private bool _isThumbVisible;
    private bool _isThumbMotionActive;
    private DispatcherTimer? _thumbMotionTimer;

    // 滑块过渡在属性提交后的下一渲染帧才真正启动，计时补偿一帧，保证抑制态晚于视觉落地结束。
    private static readonly TimeSpan ThumbMotionCompletionMargin = TimeSpan.FromMilliseconds(32);

    private readonly StateMotionSession _stateMotion;

    static AbstractSegmented()
    {
        OrientationProperty.OverrideDefaultValue<AbstractSegmented>(Orientation.Horizontal);
        AffectsMeasure<AbstractSegmented>(OrientationProperty, IsExpandingProperty, SizeTypeProperty);
        AffectsRender<AbstractSegmented>(
            SelectedThumbCornerRadiusProperty, 
            SelectedThumbBgProperty,
            SelectedThumbBoxShadowsProperty,
            SelectedThumbSizeProperty, 
            SelectedThumbPosProperty);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<AbstractSegmented>(false);
        SelectedItemProperty.Changed.AddClassHandler<AbstractSegmented>(
            (segmented, args) => segmented.NotifyFormValueChanged(args.NewValue));
    }

    public AbstractSegmented()
    {
        _stateMotion = new StateMotionSession(this);
        SelectionMode = SelectionMode.Single;
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetupSelectedThumbRect();
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
        var hasDefaultSelected = false;
        foreach (var item in Items)
        {
            if (item is not null)
            {
                var container = ContainerFromItem(item);
                if (container is not null)
                {
                    if (GetIsSelected(container))
                    {
                        hasDefaultSelected = true;
                    }
                }
            }
        }
        
        if (!hasDefaultSelected && SelectedIndex == -1 && SelectedItem is null && Items.Count > 0)
        {
            SetCurrentValue(SelectedIndexProperty, 0);
        }
        
        SetupSelectedThumbRect();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        SetupSelectedThumbRect();
        return result;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        base.OnAttachedToVisualTree(e);
        SelectionChanged += HandleSelectionChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        SelectionChanged -= HandleSelectionChanged;
        StopThumbMotion();
        base.OnDetachedFromVisualTree(e);
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractSegmentedItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is AbstractSegmentedItem segmentedItem)
        {
            if (item != null && item is not Visual)
            {
                segmentedItem.SetCurrentValue(AbstractSegmentedItem.ContentProperty, item);
                if (ItemTemplate != null)
                {
                    segmentedItem[!AbstractSegmentedItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
                }
            }

            segmentedItem[!SizeTypeProperty] = this[!SizeTypeProperty];
            segmentedItem[!AbstractSegmentedItem.ShapeProperty] = this[!ShapeProperty];
            segmentedItem[!IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            segmentedItem.SetCurrentValue(AbstractSegmentedItem.IsThumbMotionActiveProperty, _isThumbMotionActive);

            if (segmentedItem.IsSelected)
            {
                SetCurrentValue(SelectedItemProperty, ItemFromContainer(segmentedItem));
            }

            PrepareSegmentedItem(segmentedItem, item, index);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container),
                "The container type is incorrect, it must be type AbstractSegmentedItem.");
        }
    }
    
    protected virtual void PrepareSegmentedItem(AbstractSegmentedItem segmentedItem, object? item, int index)
    {
    }

    protected override bool ShouldTriggerSelection(Visual selectable, PointerEventArgs eventArgs)
    {
        // Segmented should trigger selection on pointer release for better UX
        if (eventArgs.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased &&
            eventArgs.Pointer.Type == PointerType.Mouse)
        {
            return true;
        }

        return base.ShouldTriggerSelection(selectable, eventArgs);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var direction = e.Key switch
        {
            Key.Left or Key.Up    => NavigationDirection.Previous,
            Key.Right or Key.Down => NavigationDirection.Next,
            _                     => (NavigationDirection?)null
        };

        if (!e.Handled && direction is { } value)
        {
            e.Handled = MoveSelection(value, wrap: true);
        }

        base.OnKeyDown(e);
    }
    
    public sealed override void Render(DrawingContext context)
    {
        context.DrawRectangle(Background, null, new RoundedRect(new Rect(Bounds.Size), CornerRadius));
        if (_isThumbVisible)
        {
            context.DrawRectangle(SelectedThumbBg, null,
                new RoundedRect(new Rect(SelectedThumbPos, SelectedThumbSize), SelectedThumbCornerRadius),
                SelectedThumbBoxShadows);
        }
    }

    #region 实现 FormItem 接口
    
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(object? value)
    {
        SelectedItem = value;
    }

    protected virtual object? NotifyGetFormValue()
    {
        return SelectedItem;
    }

    protected virtual void NotifyClearFormValue()
    {
        SelectedItem = null;
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion

    #region 实现 IStateMotionOwner 接口

    StateMotionSession IStateMotionOwner.StateMotionSession => _stateMotion;

    #endregion

    private void HandleSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (this.IsAttachedToVisualTree())
        {
            SetupSelectedThumbRect(animate: true);
        }
    }

    private void SetupSelectedThumbRect(bool animate = false)
    {
        if (SelectedItem is null)
        {
            // 与 antd 一致：不存在可飞行的来源几何时滑块立即消失，不做出现动画。
            StopThumbMotion();
            SetThumbVisible(false);
            _stateMotion.CompleteInitialization();
            return;
        }

        var segmentedItem = ContainerFromItem(SelectedItem);
        if (segmentedItem is not { IsArrangeValid: true } || !segmentedItem.IsAttachedToVisualTree())
        {
            // 容器尚未实现，等待下一次排列同步。
            return;
        }

        var translated = segmentedItem.TranslatePoint(default, this);
        if (translated is not { } offset || !double.IsFinite(offset.X) || !double.IsFinite(offset.Y) ||
            !double.IsFinite(segmentedItem.Bounds.Width) || !double.IsFinite(segmentedItem.Bounds.Height) ||
            !MathUtils.GreaterThan(segmentedItem.Bounds.Width, 0) || !MathUtils.GreaterThan(segmentedItem.Bounds.Height, 0))
        {
            return;
        }

        var targetPos  = new Point(offset.X, offset.Y);
        var targetSize = segmentedItem.Bounds.Size;

        // 当前插值值不是目标值；重复排列同一目标不能撤销正在运行的过渡。
        var basePos = GetBaseValue(SelectedThumbPosProperty);
        var baseSize = GetBaseValue(SelectedThumbSizeProperty);
        if (_isThumbVisible && basePos.HasValue && baseSize.HasValue &&
            targetPos == basePos.Value && targetSize == baseSize.Value)
        {
            _stateMotion.CompleteInitialization();
            return;
        }

        // 只有选中项主动切换且滑块已可见时才允许飞行；首次出现与布局跟随一律瞬移。
        var canAnimate = animate &&
                         _stateMotion.IsReady &&
                         _isThumbVisible &&
                         (targetPos != SelectedThumbPos || targetSize != SelectedThumbSize);

        using var snap = !canAnimate && _stateMotion.IsReady
            ? new TransitionSuppressionScope(this)
            : null;

        SelectedThumbPos  = targetPos;
        SelectedThumbSize = targetSize;

        SetThumbVisible(true);
        _stateMotion.CompleteInitialization();

        if (canAnimate)
        {
            BeginThumbMotion();
        }
    }

    private void SetThumbVisible(bool visible)
    {
        if (_isThumbVisible == visible)
        {
            return;
        }

        _isThumbVisible = visible;
        InvalidateVisual();
    }

    private void BeginThumbMotion()
    {
        var duration = GetThumbMotionDuration();
        if (duration <= TimeSpan.Zero)
        {
            return;
        }

        SetThumbMotionActive(true);

        if (_thumbMotionTimer is null)
        {
            _thumbMotionTimer = new DispatcherTimer();
            _thumbMotionTimer.Tick += HandleThumbMotionTimerTick;
        }

        _thumbMotionTimer.Stop();
        _thumbMotionTimer.Interval = duration + ThumbMotionCompletionMargin;
        _thumbMotionTimer.Start();
    }

    private void HandleThumbMotionTimerTick(object? sender, EventArgs e)
    {
        _thumbMotionTimer?.Stop();
        SetThumbMotionActive(false);
    }

    private void StopThumbMotion()
    {
        _thumbMotionTimer?.Stop();
        SetThumbMotionActive(false);
    }

    private void SetThumbMotionActive(bool active)
    {
        if (_isThumbMotionActive == active)
        {
            return;
        }

        _isThumbMotionActive = active;
        foreach (var item in Items)
        {
            if (item is not null && ContainerFromItem(item) is AbstractSegmentedItem container)
            {
                container.SetCurrentValue(AbstractSegmentedItem.IsThumbMotionActiveProperty, active);
            }
        }
    }

    private TimeSpan GetThumbMotionDuration()
    {
        if (Transitions is not null)
        {
            foreach (var transition in Transitions)
            {
                if (transition is TransitionBase transitionBase &&
                    Equals(transitionBase.Property, SelectedThumbPosProperty))
                {
                    return transitionBase.Duration;
                }
            }
        }

        return TimeSpan.Zero;
    }

}
