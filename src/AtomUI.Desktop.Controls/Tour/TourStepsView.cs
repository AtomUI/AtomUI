using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia.Data;
using AtomUI.Theme.Resources;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal enum TourStepPosition
{
    First,
    Middle,
    Last,
    OnePage
}

internal enum TourStepNavType
{
    Previous,
    Next,
    Finished,
}

internal class TourStepsView : SelectingItemsControl, IControlThemeGlobalOwnerSource
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<TourStyleType> StyleTypeProperty =
        Tour.StyleTypeProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        Tour.IsArrowVisibleProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        Tour.IsPointAtCenterProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<TourPlacementMode> PlacementProperty =
        Tour.PlacementProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<bool> IsShowMaskProperty =
        Tour.IsShowMaskProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<IBrush?> MaskColorProperty =
        Tour.MaskColorProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<bool> IsScrollIntoViewProperty =
        Tour.IsScrollIntoViewProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<IIconTemplate?> CloseIconProperty =
        Tour.CloseIconProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<TourIndicator?> IndicatorProperty =
        Tour.IndicatorProperty.AddOwner<TourStepsView>();

    public static readonly StyledProperty<IList<Control>?> CustomActionsProperty =
        AvaloniaProperty.Register<TourStepsView, IList<Control>?>(nameof(CustomActions));

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public TourStyleType StyleType
    {
        get => GetValue(StyleTypeProperty);
        set => SetValue(StyleTypeProperty, value);
    }

    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    public TourPlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public bool IsShowMask
    {
        get => GetValue(IsShowMaskProperty);
        set => SetValue(IsShowMaskProperty, value);
    }

    public IBrush? MaskColor
    {
        get => GetValue(MaskColorProperty);
        set => SetValue(MaskColorProperty, value);
    }

    public bool IsScrollIntoView
    {
        get => GetValue(IsScrollIntoViewProperty);
        set => SetValue(IsScrollIntoViewProperty, value);
    }

    public IIconTemplate? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    public TourIndicator? Indicator
    {
        get => GetValue(IndicatorProperty);
        set => SetValue(IndicatorProperty, value);
    }

    public IList<Control>? CustomActions
    {
        get => GetValue(CustomActionsProperty);
        set => SetValue(CustomActionsProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler? CloseRequest;

    public event EventHandler<TourStepNavRequestEventArgs>? NavRequest;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<TourStepPosition> IndexPositionProperty =
        AvaloniaProperty.Register<TourStepsView, TourStepPosition>(nameof(IndexPosition));

    internal static readonly DirectProperty<TourStepsView, TourStyleType> CurrentStyleTypeProperty =
        AvaloniaProperty.RegisterDirect<TourStepsView, TourStyleType>(nameof(CurrentStyleType),
            o => o.CurrentStyleType,
            (o, v) => o.CurrentStyleType = v);

    internal TourStepPosition IndexPosition
    {
        get => GetValue(IndexPositionProperty);
        set => SetValue(IndexPositionProperty, value);
    }

    private TourStyleType _currentStyleType;

    internal TourStyleType CurrentStyleType
    {
        get => _currentStyleType;
        private set => SetAndRaise(CurrentStyleTypeProperty, ref _currentStyleType, value);
    }

    #endregion

    #region 内部协作 API

internal void ReleaseTemplateContent()
    {
        // Retire the old template's TwoWay edge before clearing its items, so
        // selection reset cannot write into the live Tour or its caller binding.
        BindingOperations.GetBindingExpressionBase(this, SelectedIndexProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(this, ItemsSourceProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(this, IndicatorProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(this, CustomActionsProperty)?.Dispose();
        SetCurrentValue(ItemsSourceProperty, null);
        SetCurrentValue(IndicatorProperty, null);
        SetCurrentValue(CustomActionsProperty, null);
        _indicatorPresenter?.UpdateChild();
    }

    #endregion

    #region 运行时字段

    private Panel? _customActionsPanel;

    private readonly Dictionary<TourStep, CompositeDisposable> _stepBindings = new();
    private ContentPresenter? _indicatorPresenter;

    #endregion

    #region 构造与初始化

    static TourStepsView()
    {
        SelectionModeProperty.OverrideDefaultValue<TourStepsView>(SelectionMode.Single);
        Button.ClickEvent.AddClassHandler<TourStepsView>((view, args) => view.HandleButtonClick(args));
    }

    #endregion

    #region 公共与受保护 API

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TourStep>(item, out recycleKey);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TourStep();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TourStep tourStep)
        {
            PrepareStepBindings(tourStep, item);
            PrepareStepItem(tourStep, item, index);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TourStep.");
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is TourStep step)
        {
            RetireStepBindings(step);
        }
        base.ClearContainerForItemOverride(container);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        foreach (var step in _stepBindings.Keys.ToArray())
        {
            RetireStepBindings(step);
        }
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        foreach (var container in GetRealizedContainers())
        {
            var index = IndexFromContainer(container);
            if (container is TourStep step && index >= 0 && index < Items.Count)
            {
                PrepareStepBindings(step, Items[index]);
            }
        }
    }

    protected virtual void PrepareStepItem(TourStep tourStep, object? item, int index)
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_customActionsPanel != null && CustomActions != null)
        {
            foreach (var action in CustomActions)
            {
                _customActionsPanel.Children.Remove(action);
            }
        }
        base.OnApplyTemplate(e);
        _indicatorPresenter = e.NameScope.Find<ContentPresenter>("IndicatorPresenter");
        _customActionsPanel = e.NameScope.Find<StackPanel>("ActionsLayout");
        if (_customActionsPanel != null)
        {
            if (CustomActions != null)
            {
                foreach (Control customAction in CustomActions)
                {
                    if (customAction is ITourAction tourAction)
                    {
                        SyncActionProperties(tourAction);
                    }
                    _customActionsPanel.Children.Insert(0, customAction);
                }
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedIndexProperty ||
            change.Property == ItemCountProperty)
        {
            if (ItemCount == 1)
            {
                IndexPosition = TourStepPosition.OnePage;
            }
            else if (ItemCount > 1)
            {
                if (SelectedIndex == 0)
                {
                    IndexPosition = TourStepPosition.First;
                }
                else if (SelectedIndex == ItemCount - 1)
                {
                    IndexPosition = TourStepPosition.Last;
                }
                else
                {
                    IndexPosition = TourStepPosition.Middle;
                }
            }
        }
        else if (change.Property == CustomActionsProperty)
        {
            if (change.OldValue is IList<Control> oldCustomActions)
            {
                foreach (Control customAction in oldCustomActions)
                {
                    _customActionsPanel?.Children.Remove(customAction);
                }
            }

            if (change.NewValue is IList<Control> newCustomActions)
            {
                foreach (Control customAction in newCustomActions)
                {
                    if (customAction is ITourAction tourAction)
                    {
                        SyncActionProperties(tourAction);
                    }
                    _customActionsPanel?.Children.Insert(0, customAction);
                }
            }
        }

        if (change.Property == SelectedIndexProperty ||
            change.Property == ItemCountProperty ||
            change.Property == CurrentStyleTypeProperty)
        {
            if (CustomActions != null)
            {
                foreach (Control customAction in CustomActions)
                {
                    if (customAction is ITourAction tourAction)
                    {
                        SyncActionProperties(tourAction);
                    }
                }
            }
        }
    }

    #endregion

    #region 实现 IControlThemeGlobalOwnerSource 接口

    Avalonia.Controls.Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => TourThemeOwner.Resolve(this);

    #endregion

    #region 私有实现

    private void HandleButtonClick(RoutedEventArgs args)
    {
        if (args.Source is DialogCaptionButton)
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        else if (args.Source is Button button && button.Tag is TourStepNavType navType)
        {
            if (navType == TourStepNavType.Finished)
            {
                CloseRequest?.Invoke(this, EventArgs.Empty);
            }
            else if (navType == TourStepNavType.Previous)
            {
                NavRequest?.Invoke(this, new TourStepNavRequestEventArgs(SelectedIndex - 1));
            }
            else if (navType == TourStepNavType.Next)
            {
                NavRequest?.Invoke(this, new TourStepNavRequestEventArgs(SelectedIndex + 1));
            }
        }
    }

    private void PrepareStepBindings(TourStep step, object? item)
    {
        RetireStepBindings(step);
        // Runtime items are prepared before template membership with the original
        // Local/Template priorities. A template setter cannot preserve this timing;
        // release each typed projection when its container membership ends.
        var bindings = new CompositeDisposable();
        _stepBindings.Add(step, bindings);
        step.DetachedFromVisualTree += HandleStepDetached;
        if (item != null && item is not Visual)
        {
            step.SetCurrentValue(TourStep.ContentProperty, item);
            bindings.Add(BindUtils.RelayBind(this, ItemTemplateProperty, step, TourStep.ContentTemplateProperty));
        }
        bindings.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, step, TourStep.IsMotionEnabledProperty));
        bindings.Add(BindUtils.RelayBind(this, CloseIconProperty, step, TourStep.CloseIconTemplateProperty));
        if (!ReferenceEquals(step, item))
        {
            bindings.Add(BindUtils.RelayBind(this, StyleTypeProperty, step, TourStep.StyleTypeProperty, priority: BindingPriority.Template));
            bindings.Add(BindUtils.RelayBind(this, IsArrowVisibleProperty, step, TourStep.IsArrowVisibleProperty, priority: BindingPriority.Template));
            bindings.Add(BindUtils.RelayBind(this, IsPointAtCenterProperty, step, TourStep.IsPointAtCenterProperty, priority: BindingPriority.Template));
            bindings.Add(BindUtils.RelayBind(this, PlacementProperty, step, TourStep.PlacementProperty, priority: BindingPriority.Template));
            bindings.Add(BindUtils.RelayBind(this, IsShowMaskProperty, step, TourStep.IsShowMaskProperty, priority: BindingPriority.Template));
            bindings.Add(BindUtils.RelayBind(this, IsScrollIntoViewProperty, step, TourStep.IsScrollIntoViewProperty, priority: BindingPriority.Template));
            bindings.Add(BindUtils.RelayBind(this, MaskColorProperty, step, TourStep.MaskColorProperty, priority: BindingPriority.Template));
        }
    }

    private void RetireStepBindings(TourStep step)
    {
        if (_stepBindings.Remove(step, out var bindings))
        {
            step.DetachedFromVisualTree -= HandleStepDetached;
            bindings.Dispose();
        }
    }

    private void HandleStepDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is TourStep step)
        {
            RetireStepBindings(step);
        }
    }



    private void SyncActionProperties(ITourAction customAction)
    {
        customAction.StyleType   = CurrentStyleType;
        customAction.ActiveIndex = SelectedIndex;
        customAction.StepCount   = ItemCount;
    }

    #endregion

}
