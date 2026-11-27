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

/// <summary>
/// Base class of a control that presents mutually exclusive options as one joined block. An item is an
/// <see cref="AbstractOptionButton" /> or data such as <see cref="OptionButtonData" />; a derived class
/// creates the option buttons in <c>CreateContainerForItemOverride</c>, and they must derive from
/// <see cref="AbstractOptionButton" />. The checked option is the one at
/// <see cref="SelectingItemsControl.SelectedIndex" />.
/// </summary>
public abstract class AbstractOptionButtonGroup : SelectingItemsControl,
                                                  ICustomizableSizeTypeAware,
                                                  IInteractionEffectAwareControl,
                                                  IFormItemAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractOptionButtonGroup>();

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AbstractOptionButtonGroup>();

    /// <summary>
    /// Identifies the <see cref="ButtonStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<OptionButtonStyle> ButtonStyleProperty =
        AvaloniaProperty.Register<AbstractOptionButtonGroup, OptionButtonStyle>(nameof(ButtonStyle));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractOptionButtonGroup>();

    /// <summary>
    /// Identifies the <see cref="IsInteractionEffectEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInteractionEffectEnabledProperty =
        InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty.AddOwner<AbstractOptionButtonGroup>();

    /// <summary>
    /// Size preset of the group and of every option, <see cref="CustomizableSizeType.Middle" /> by default.
    /// It drives the height, the corner radius, the font size, the padding and the icon size.
    /// <see cref="CustomizableSizeType.Custom" /> has no preset of its own here and renders like
    /// <see cref="CustomizableSizeType.Middle" />.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Direction in which the options are lined up,
    /// <see cref="Avalonia.Layout.Orientation.Horizontal" /> by default. It also decides which corners of
    /// an option are rounded and the direction of the separators.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Visual variant of the options, <see cref="OptionButtonStyle.Outline" /> by default.
    /// Every option follows the value set on the group.
    /// </summary>
    public OptionButtonStyle ButtonStyle
    {
        get => GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    /// <summary>
    /// Whether the options animate their foreground, background and border colors when their state changes.
    /// The theme sets it from the shared <c>EnableMotion</c> token, and every option follows the value set
    /// on the group.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Whether an option plays interaction feedback when a press on it ends; this also needs
    /// <see cref="IsMotionEnabled" /> to be <see langword="true" />. The theme sets it from the shared
    /// <c>EnableInteractionEffect</c> token, and every option follows the value set on the group.
    /// </summary>
    public bool IsInteractionEffectEnabled
    {
        get => GetValue(IsInteractionEffectEnabledProperty);
        set => SetValue(IsInteractionEffectEnabledProperty, value);
    }
    
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Identifies the <see cref="OptionCheckedChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<OptionCheckedChangedEventArgs> OptionCheckedChangedEvent =
        RoutedEvent.Register<AbstractOptionButtonGroup, OptionCheckedChangedEventArgs>(
            nameof(OptionCheckedChanged),
            RoutingStrategies.Bubble);
    
    /// <summary>
    /// Raised after an option became the checked one and the selection of the group was moved to it:
    /// when the <c>IsChecked</c> of an option changes to <see langword="true" />, and when an option that is
    /// already checked, such as one declared with <c>IsChecked="True"</c>, is prepared. The event bubbles
    /// up from the group.
    /// </summary>
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

    /// <summary>
    /// Border thickness that the theme applies to the frame of the group. The class only stores the value and never
    /// changes it itself, and no other code assigns it, so the frame never has a border; the group's own rendering
    /// reads <see cref="TemplatedControl.BorderThickness" /> instead.
    /// </summary>
    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }

    /// <summary>
    /// Border brush of the background that the group draws behind the selected option. The theme sets it from the
    /// <c>ColorPrimary</c> token of the group; <see langword="null" /> draws the background without a border.
    /// </summary>
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

    /// <summary>
    /// Gives every realized option button the new orientation when <see cref="Orientation" /> changes.
    /// Overrides must call the base method.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            UpdateRealizedOptionButtonsState();
        }
    }

    /// <summary>
    /// Subscribes to index changes of the options and to selection changes, then brings the realized
    /// options in line with the orientation and the selection. Overrides must call the base method.
    /// </summary>
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

    /// <summary>
    /// Unsubscribes the handlers that <see cref="OnAttachedToVisualTree" /> added. Overrides must call the
    /// base method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (this is IChildIndexProvider childIndexProvider)
        {
            childIndexProvider.ChildIndexChanged -= HandleChildIndexChanged;
        }
        SelectionChanged -= HandleSelectionChanged;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Selects the option button that received focus through directional navigation and, when that
    /// selection is accepted, marks the focus event handled. Overrides must call the base method.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.NavigationMethod == NavigationMethod.Directional && e.Source is AbstractOptionButton optionButton)
        {
            e.Handled = UpdateSelectionFromEvent(optionButton, e);
        }
    }

    /// <summary>
    /// Prepares an option button for an item and binds it to the size, style and motion settings of the
    /// group. An item that implements <c>IOptionButtonData</c>, such as <see cref="OptionButtonData" />,
    /// also provides the content, the icon and the enabled state of the generated option button; an
    /// option button used as the item keeps its own content, icon and enabled state. Ends by calling
    /// <see cref="PrepareOptionButton" />. Overrides must call the base method.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is not an <see cref="AbstractOptionButton" />.
    /// </exception>
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
    
    /// <summary>
    /// Called for each option button after its content and its bindings to the group are in place, both
    /// for generated option buttons and for option buttons used directly as items. The base method does
    /// nothing.
    /// </summary>
    protected virtual void PrepareOptionButton(AbstractOptionButton optionButton, object? item, int index)
    {
    }

    /// <summary>
    /// Refreshes the position of every realized option button and keeps the prepared one in line with the
    /// selection: an option button at <c>SelectedIndex</c> that is not checked becomes checked, and an
    /// option button that is already checked becomes the selected option and raises
    /// <see cref="OptionCheckedChanged" />. Overrides must call the base method.
    /// </summary>
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

    /// <summary>
    /// Releases what the group attached to an option button before its container is reused or removed:
    /// the bindings to the group, the checked handler, the group orientation and the position in the group.
    /// Overrides must call the base method.
    /// </summary>
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
                IsInteractionEffectEnabledProperty,
                optionButton,
                AbstractOptionButton.IsInteractionEffectEnabledProperty,
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

    /// <summary>
    /// Reports whether the group has to create an option button for an item.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when <paramref name="item" /> is not an <see cref="AbstractOptionButton" />;
    /// an option button used directly as an item is its own container.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractOptionButton>(item, out recycleKey);
    }

    /// <summary>
    /// Gives an option button the position that belongs to its new index, so that its corner radius
    /// follows a move within the group.
    /// </summary>
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

    /// <summary>
    /// Paints the background and border of the group, the separators between the options and, in the
    /// <see cref="OptionButtonStyle.Outline" /> style, the border and fill of the checked option. In the
    /// <see cref="OptionButtonStyle.Solid" /> style the checked option paints itself and the separators
    /// next to it are not drawn.
    /// </summary>
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
                    renderBorderThickness,
                    optionButton.OutlineCheckedFill);
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
        Thickness renderBorderThickness,
        IBrush? fill)
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
            fill,
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
    
    /// <summary>
    /// Notifies the form item that the value of the group changed. The base method raises the
    /// <see cref="IFormItemAware.ValueChanged" /> event.
    /// </summary>
    /// <param name="value">
    /// The new selected item.
    /// </param>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies a value that the form item assigns to the group. The base method makes it the
    /// <see cref="SelectingItemsControl.SelectedItem" />.
    /// </summary>
    /// <param name="value">
    /// The item to select.
    /// </param>
    protected virtual void NotifySetFormValue(object? value)
    {
        SetCurrentValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Returns the value of the group for the form item. The base method returns
    /// <see cref="SelectingItemsControl.SelectedItem" />.
    /// </summary>
    /// <returns>
    /// The selected item, or <see langword="null" /> when nothing is selected.
    /// </returns>
    protected virtual object? NotifyGetFormValue()
    {
        return SelectedItem;
    }

    /// <summary>
    /// Called when the form item clears its value. The base method assigns <see langword="false" /> to
    /// <see cref="SelectingItemsControl.SelectedItem" />, a value that equals no item.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(SelectedItemProperty, false);
    }

    /// <summary>
    /// Passes the validation status that the form item reports. The base method does nothing.
    /// </summary>
    /// <param name="status">
    /// The validation status of the form item.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
