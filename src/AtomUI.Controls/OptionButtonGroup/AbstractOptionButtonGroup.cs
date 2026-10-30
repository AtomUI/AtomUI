using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

public abstract class AbstractOptionButtonGroup : SelectingItemsControl,
                                                  ICustomizableSizeTypeAware,
                                                  IWaveSpiritAwareControl,
                                                  IFormItemAware
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractOptionButtonGroup>();

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AbstractOptionButtonGroup>();

    public static readonly StyledProperty<OptionButtonStyle> ButtonStyleProperty =
        AvaloniaProperty.Register<AbstractOptionButtonGroup, OptionButtonStyle>(nameof(ButtonStyle));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractOptionButtonGroup>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<AbstractOptionButtonGroup>();

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

    public OptionButtonStyle ButtonStyle
    {
        get => GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }
    
    #endregion

    #region 公共事件定义
    public static readonly RoutedEvent<OptionCheckedChangedEventArgs> OptionCheckedChangedEvent =
        RoutedEvent.Register<AbstractOptionButtonGroup, OptionCheckedChangedEventArgs>(
            nameof(OptionCheckedChanged),
            RoutingStrategies.Bubble);
    
    public event EventHandler<OptionCheckedChangedEventArgs>? OptionCheckedChanged
    {
        add => AddHandler(OptionCheckedChangedEvent, value);
        remove => RemoveHandler(OptionCheckedChangedEvent, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractOptionButtonGroup, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<AbstractOptionButtonGroup, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);

    internal static readonly StyledProperty<IBrush?> SelectedOptionBorderColorProperty =
        AvaloniaProperty.Register<AbstractOptionButtonGroup, IBrush?>(nameof(SelectedOptionBorderColor));

    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }

    internal IBrush? SelectedOptionBorderColor
    {
        get => GetValue(SelectedOptionBorderColorProperty);
        set => SetValue(SelectedOptionBorderColorProperty, value);
    }

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel
        {
            Orientation = Orientation.Horizontal
        });

    #endregion

    private readonly BorderRenderHelper _borderRenderHelper = new();
    private readonly Dictionary<AbstractOptionButton, CompositeDisposable> _optionButtonOwnerStateBindings = new();
    private IPen? _separatorPen;

    static AbstractOptionButtonGroup()
    {
        SelectionModeProperty.OverrideDefaultValue<AbstractOptionButtonGroup>(SelectionMode.Single | SelectionMode.AlwaysSelected);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<AbstractOptionButtonGroup>(false);
        ItemsPanelProperty.OverrideDefaultValue<AbstractOptionButtonGroup>(DefaultPanel);
        OrientationProperty.OverrideDefaultValue<AbstractOptionButtonGroup>(Orientation.Horizontal);
        AffectsRender<AbstractOptionButtonGroup>(SelectionModeProperty);

        AffectsMeasure<AbstractOptionButtonGroup>(SizeTypeProperty, OrientationProperty);
        AffectsRender<AbstractOptionButtonGroup>(
            OrientationProperty,
            SelectedOptionBorderColorProperty,
            ButtonStyleProperty,
            SelectedIndexProperty,
            SelectedItemProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            UseLayoutRoundingProperty);
        SelectedItemProperty.Changed.AddClassHandler<AbstractOptionButtonGroup>((group, args) => group.NotifyFormValueChanged(args.NewValue));
    }

    protected AbstractOptionButtonGroup()
    {
        ItemsView.CollectionChanged += HandleItemsCollectionChanged;
    }

    private void HandleChildIndexChanged(object? sender, EventArgs args)
    {
        UpdateRealizedOptionButtonsState();
    }

    private void HandleSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        SyncSelectedOptionButton();
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var container in GetRealizedContainers())
            {
                ReleaseDirectOptionButton(container as AbstractOptionButton);
            }
        }
        else if (args.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace &&
                 args.OldItems is not null)
        {
            foreach (var oldItem in args.OldItems)
            {
                ReleaseDirectOptionButton(oldItem as AbstractOptionButton);
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            UpdateRealizedOptionButtonsState();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (this is IChildIndexProvider childIndexProvider)
        {
            childIndexProvider.ChildIndexChanged += HandleChildIndexChanged;
        }

        SelectionChanged += HandleSelectionChanged;
        UpdateRealizedOptionButtonsState();
        SyncSelectedOptionButton();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (this is IChildIndexProvider childIndexProvider)
        {
            childIndexProvider.ChildIndexChanged -= HandleChildIndexChanged;
        }
        SelectionChanged -= HandleSelectionChanged;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.NavigationMethod == NavigationMethod.Directional && e.Source is AbstractOptionButton optionButton)
        {
            e.Handled = UpdateSelectionFromEvent(optionButton, e);
        }
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is not AbstractOptionButton optionButton)
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type AbstractOptionButton.");
        }

        ReleaseOptionButtonOwnerState(optionButton);
        base.PrepareContainerForItemOverride(container, item, index);
        if (item != null && item is not Visual)
        {
            {
                if (!optionButton.IsSet(AbstractOptionButton.ContentProperty))
                {
                    if (ItemTemplate != null)
                    {
                        optionButton.SetCurrentValue(AbstractOptionButton.ContentProperty, item);
                    }
                    else
                    {
                        if (item is IOptionButtonData optionButtonData)
                        {
                            optionButton.SetCurrentValue(AbstractOptionButton.ContentProperty, optionButtonData.Header);
                        }
                    }
                }
            }

            {
                if (item is IOptionButtonData optionButtonData)
                {
                    if (!optionButton.IsSet(IsEnabledProperty))
                    {
                        optionButton.SetCurrentValue(IsEnabledProperty, optionButtonData.IsEnabled);
                    }

                    if (!optionButton.IsSet(AbstractOptionButton.IconProperty))
                    {
                        optionButton.SetCurrentValue(AbstractOptionButton.IconProperty, optionButtonData.Icon);
                    }
                }
            }
        }

        BindOptionButtonOwnerState(optionButton, !ReferenceEquals(container, item));
        optionButton.GroupOrientation = Orientation;

        PrepareOptionButton(optionButton, item, index);
    }
    
    protected virtual void PrepareOptionButton(AbstractOptionButton optionButton, object? item, int index)
    {
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is AbstractOptionButton optionButton)
        {
            UpdateRealizedOptionButtonsState();
            if (index == SelectedIndex && optionButton.IsChecked != true)
            {
                optionButton.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
                return;
            }

            if (optionButton.IsChecked.HasValue && optionButton.IsChecked.Value)
            {
                var containerIndex = IndexFromContainer(optionButton);
                if (containerIndex != -1)
                {
                    Selection.Select(containerIndex);
                }
                RaiseEvent(new OptionCheckedChangedEventArgs(OptionCheckedChangedEvent, optionButton,
                    index));
            }
        }
    }

    private void HandleOptionButtonChecked(object? sender, RoutedEventArgs args)
    {
        if (sender is AbstractOptionButton optionButton && optionButton.IsChecked.HasValue && optionButton.IsChecked.Value)
        {
            var index = IndexFromContainer(optionButton);
            if (index != -1)
            {
                Selection.Select(index);
            }
            RaiseEvent(new OptionCheckedChangedEventArgs(OptionCheckedChangedEvent, optionButton,
                SelectedIndex));
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is AbstractOptionButton optionButton)
        {
            ReleaseOptionButtonOwnerState(optionButton);
        }
        base.ClearContainerForItemOverride(container);
    }

    private void ReleaseDirectOptionButton(AbstractOptionButton? optionButton)
    {
        if (optionButton is not null)
        {
            ReleaseOptionButtonOwnerState(optionButton);
        }
    }

    private void ReleaseOptionButtonOwnerState(AbstractOptionButton optionButton)
    {
        if (_optionButtonOwnerStateBindings.Remove(optionButton, out var bindings))
        {
            bindings.Dispose();
        }

        optionButton.GroupOrientation   = Orientation.Horizontal;
        optionButton.GroupPositionTrait = OptionButtonPositionTrait.OnlyOne;
    }

    private void BindOptionButtonOwnerState(AbstractOptionButton optionButton, bool isGeneratedContainer)
    {
        var bindings = new CompositeDisposable(isGeneratedContainer ? 6 : 5);
        _optionButtonOwnerStateBindings.Add(optionButton, bindings);
        try
        {
            if (isGeneratedContainer && ItemTemplate is not null)
            {
                bindings.Add(BindUtils.RelayBind(
                    this,
                    ItemTemplateProperty,
                    optionButton,
                    AbstractOptionButton.ContentTemplateProperty,
                    BindingMode.OneWay));
            }

            bindings.Add(BindUtils.RelayBind(
                this,
                IsMotionEnabledProperty,
                optionButton,
                AbstractOptionButton.IsMotionEnabledProperty,
                BindingMode.OneWay));
            bindings.Add(BindUtils.RelayBind(
                this,
                SizeTypeProperty,
                optionButton,
                AbstractOptionButton.SizeTypeProperty,
                BindingMode.OneWay));
            bindings.Add(BindUtils.RelayBind(
                this,
                IsWaveSpiritEnabledProperty,
                optionButton,
                AbstractOptionButton.IsWaveSpiritEnabledProperty,
                BindingMode.OneWay));
            bindings.Add(BindUtils.RelayBind(
                this,
                ButtonStyleProperty,
                optionButton,
                AbstractOptionButton.ButtonStyleProperty,
                BindingMode.OneWay));

            optionButton.IsCheckedChanged += HandleOptionButtonChecked;
            bindings.Add(Disposable.Create(() =>
                optionButton.IsCheckedChanged -= HandleOptionButtonChecked));
        }
        catch
        {
            ReleaseOptionButtonOwnerState(optionButton);
            throw;
        }
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractOptionButton>(item, out recycleKey);
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        if (container is AbstractOptionButton optionButton)
        {
            optionButton.GroupOrientation   = Orientation;
            optionButton.GroupPositionTrait = GetPositionTrait(newIndex, ItemCount);
        }
    }

    private void UpdateRealizedOptionButtonsState()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is AbstractOptionButton button)
            {
                button.GroupOrientation   = Orientation;
                button.GroupPositionTrait = GetPositionTrait(i, ItemCount);
            }
        }
    }

    private static OptionButtonPositionTrait GetPositionTrait(int index, int count)
    {
        if (count <= 1)
        {
            return OptionButtonPositionTrait.OnlyOne;
        }

        if (index == 0)
        {
            return OptionButtonPositionTrait.First;
        }

        return index == count - 1
            ? OptionButtonPositionTrait.Last
            : OptionButtonPositionTrait.Middle;
    }

    private void SyncSelectedOptionButton()
    {
        if (SelectedIndex >= 0 &&
            ContainerFromIndex(SelectedIndex) is AbstractOptionButton optionButton &&
            optionButton.IsChecked != true)
        {
            optionButton.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
        }
    }

    public override void Render(DrawingContext context)
    {
        var renderBorderThickness = BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness);
        var renderSize = Orientation == Orientation.Horizontal
            ? DesiredSize
            : Bounds.Size;
        _borderRenderHelper.Render(context,
            renderSize,
            renderBorderThickness,
            CornerRadius,
            BackgroundSizing.CenterBorder,
            Background,
            BorderBrush);

        if (Bounds.Width <= 0 || Bounds.Height <= 0 || ItemCount <= 0)
        {
            return;
        }

        var selectedIndex = SelectedIndex;
        for (var i = 0; i < ItemCount; ++i)
        {
            if (ContainerFromIndex(i) is not AbstractOptionButton optionButton ||
                !TryGetGroupLocalBounds(optionButton, out var optionBounds))
            {
                continue;
            }

            if (i != ItemCount - 1)
            {
                var isAdjacentToSelection = i == selectedIndex || i + 1 == selectedIndex;
                if (ButtonStyle != OptionButtonStyle.Solid || !isAdjacentToSelection)
                {
                    DrawSeparator(context, optionBounds, renderBorderThickness);
                }
            }

            if (ButtonStyle == OptionButtonStyle.Outline &&
                i == selectedIndex &&
                IsEnabled &&
                optionButton.IsEnabled)
            {
                DrawSelectedOptionBorder(
                    context,
                    optionBounds,
                    i,
                    renderBorderThickness);
            }
        }
    }

    private bool TryGetGroupLocalBounds(Control container, out Rect bounds)
    {
        if (container.TranslatePoint(default, this) is { } origin)
        {
            bounds = new Rect(origin, container.Bounds.Size);
            return true;
        }

        bounds = default;
        return false;
    }

    private void DrawSeparator(
        DrawingContext context,
        Rect optionBounds,
        Thickness renderBorderThickness)
    {
        Point startPoint;
        Point endPoint;
        double thickness;
        if (Orientation == Orientation.Horizontal)
        {
            thickness = renderBorderThickness.Left;
            var offset = optionBounds.Right - thickness / 2;
            startPoint = new Point(offset, 0);
            endPoint   = new Point(offset, Bounds.Height);
        }
        else
        {
            thickness = renderBorderThickness.Top;
            var offset = optionBounds.Bottom - thickness / 2;
            startPoint = new Point(0, offset);
            endPoint   = new Point(Bounds.Width, offset);
        }

        if (MathUtils.IsZero(thickness))
        {
            return;
        }

        PenUtils.TryModifyOrCreate(ref _separatorPen, BorderBrush, thickness);
        if (_separatorPen is null)
        {
            return;
        }

        using var optionState = context.PushRenderOptions(new RenderOptions
        {
            EdgeMode = EdgeMode.Aliased
        });
        context.DrawLine(_separatorPen, startPoint, endPoint);
    }

    private void DrawSelectedOptionBorder(
        DrawingContext context,
        Rect optionBounds,
        int selectedIndex,
        Thickness renderBorderThickness)
    {
        var selectedBounds = optionBounds;
        if (selectedIndex > 0)
        {
            selectedBounds = Orientation == Orientation.Horizontal
                ? new Rect(
                    selectedBounds.X - renderBorderThickness.Left,
                    selectedBounds.Y,
                    selectedBounds.Width + renderBorderThickness.Left,
                    selectedBounds.Height)
                : new Rect(
                    selectedBounds.X,
                    selectedBounds.Y - renderBorderThickness.Top,
                    selectedBounds.Width,
                    selectedBounds.Height + renderBorderThickness.Top);
        }

        using var state = context.PushTransform(
            Matrix.CreateTranslation(selectedBounds.X, selectedBounds.Y));
        _borderRenderHelper.Render(context,
            selectedBounds.Size,
            renderBorderThickness,
            BuildSelectedCornerRadius(selectedIndex),
            BackgroundSizing.InnerBorderEdge,
            null,
            SelectedOptionBorderColor);
    }

    private CornerRadius BuildSelectedCornerRadius(int selectedIndex)
    {
        var position = GetPositionTrait(selectedIndex, ItemCount);
        if (position == OptionButtonPositionTrait.OnlyOne)
        {
            return CornerRadius;
        }

        if (position == OptionButtonPositionTrait.Middle)
        {
            return default;
        }

        if (Orientation == Orientation.Horizontal)
        {
            return position == OptionButtonPositionTrait.First
                ? new CornerRadius(CornerRadius.TopLeft, 0, 0, CornerRadius.BottomLeft)
                : new CornerRadius(0, CornerRadius.TopRight, CornerRadius.BottomRight, 0);
        }

        return position == OptionButtonPositionTrait.First
            ? new CornerRadius(CornerRadius.TopLeft, CornerRadius.TopRight, 0, 0)
            : new CornerRadius(0, 0, CornerRadius.BottomRight, CornerRadius.BottomLeft);
    }
    
    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
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
        SetCurrentValue(SelectedItemProperty, value);
    }

    protected virtual object? NotifyGetFormValue()
    {
        return SelectedItem;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(SelectedItemProperty, false);
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
