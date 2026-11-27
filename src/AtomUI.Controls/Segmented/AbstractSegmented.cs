using AtomUI.Animations;
using AtomUI.Media;
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

/// <summary>
/// Base class of the segmented control. It presents mutually exclusive options as a row or column of items,
/// keeps a single item selected and draws a thumb behind the selected one. An item is an
/// <see cref="AbstractSegmentedItem" /> or plain data; a derived class creates the containers for data items
/// in <c>CreateContainerForItemOverride</c>, and every container must derive from
/// <see cref="AbstractSegmentedItem" />.
/// </summary>
public abstract class AbstractSegmented : SelectingItemsControl,
                                          IMotionAwareControl,
                                          ICustomizableSizeTypeAware,
                                          IFormItemAware,
                                          ITransitionInitializationOwner
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="AbstractSegmented.SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSegmented>();

    /// <summary>
    /// Identifies the <see cref="AbstractSegmented.Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AbstractSegmented>();

    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<SegmentedShape> ShapeProperty =
        AvaloniaProperty.Register<AbstractSegmented, SegmentedShape>(nameof(Shape));

    /// <summary>
    /// Identifies the <see cref="IsExpanding" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandingProperty =
        AvaloniaProperty.Register<AbstractSegmented, bool>(nameof(IsExpanding));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSegmented>();

    /// <summary>
    /// Identifies the <see cref="SelectedThumbCornerRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> SelectedThumbCornerRadiusProperty =
        AvaloniaProperty.Register<AbstractSegmented, CornerRadius>(nameof(SelectedThumbCornerRadius));

    /// <summary>
    /// Size preset of the control and of every item, <see cref="CustomizableSizeType.Middle" /> by default. It
    /// sets the corner radius of the track and of the thumb and, on each item, the minimum height, the font size,
    /// the padding and the icon size. <see cref="CustomizableSizeType.Custom" /> has no preset of its own and
    /// uses the values of <see cref="CustomizableSizeType.Middle" />, except that the thumb follows the corner
    /// radius that is set on the selected item.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Direction in which the items are lined up, <see cref="Avalonia.Layout.Orientation.Horizontal" /> by default.
    /// In a vertical layout every item spans the width of the control.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Outline of the track, the thumb and the items. <see cref="SegmentedShape.Round" /> replaces the corner
    /// radii of the <see cref="AbstractSegmented.SizeType" /> preset with a radius larger than half of any height,
    /// which turns the track, the thumb and the items into capsules.
    /// </summary>
    public SegmentedShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// Whether the control takes all of the available width instead of the width of its content; the default is
    /// <see langword="false" />. The theme stretches the control horizontally, and in a horizontal layout the
    /// visible items get equal widths. In a vertical layout the value only changes the width of the control.
    /// While the available width is infinite the control keeps the width of its content, and a horizontal layout
    /// divides that width equally.
    /// </summary>
    public bool IsExpanding
    {
        get => GetValue(IsExpandingProperty);
        set => SetValue(IsExpandingProperty, value);
    }
    
    /// <summary>
    /// Whether the thumb glides to the newly selected item and the item colors fade when their state changes.
    /// The theme sets it from the shared <c>EnableMotion</c> token, and every item follows the value set on the
    /// control. With <see langword="false" /> both change at once.
    /// </summary>
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

    /// <summary>
    /// The corner radius of the thumb behind the selected item, in DIPs. The theme sets it for each preset of
    /// <see cref="AbstractSegmented.SizeType" /> and to a capsule radius for <see cref="SegmentedShape.Round" />.
    /// With <see cref="CustomizableSizeType.Custom" /> the corner radius that is set on the selected item takes
    /// precedence. When the thumb is drawn, a radius larger than half of its shorter side is clamped to that half.
    /// </summary>
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

    /// <summary>
    /// The brush and the shadow with which the track draws the thumb behind the selected item; <c>Segmented</c> in
    /// AtomUI.Desktop.Controls overrides it.
    /// </summary>
    /// <remarks>
    /// It is read on each render pass while a selected item is shown. The base class returns <c>SelectedThumbBg</c>
    /// and <c>SelectedThumbBoxShadows</c>.
    /// </remarks>
    private protected virtual (IBrush? Brush, BoxShadows Shadows) ThumbPaint =>
        (SelectedThumbBg, SelectedThumbBoxShadows);

    /// <summary>
    /// The size of the thumb behind the selected item, in DIPs: the size of the selected item, confined to the area
    /// inside <see cref="TemplatedControl.Padding" />. The control writes it together with
    /// <see cref="SelectedThumbPos" /> when the selection or the layout of the selected item changes. The theme
    /// animates it with a transition.
    /// </summary>
    internal Size SelectedThumbSize
    {
        get => GetValue(SelectedThumbSizeProperty);
        set => SetValue(SelectedThumbSizeProperty, value);
    }

    /// <summary>
    /// The position of the top-left corner of the thumb behind the selected item, relative to the control, in DIPs.
    /// The control writes it together with <see cref="SelectedThumbSize" /> when the selection or the layout of the
    /// selected item changes. The theme animates it with a transition, so the thumb glides to the newly selected
    /// item instead of jumping.
    /// </summary>
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

    // A thumb transition starts on the render frame after the properties are committed. The timer adds one frame,
    // so the suppression always ends after the visual has landed.
    private static readonly TimeSpan ThumbMotionCompletionMargin = TimeSpan.FromMilliseconds(32);

    private readonly TransitionInitializationSession _transitionInitialization;

    static AbstractSegmented()
    {
        OrientationProperty.OverrideDefaultValue<AbstractSegmented>(Orientation.Horizontal);
        AffectsMeasure<AbstractSegmented>(OrientationProperty, IsExpandingProperty, SizeTypeProperty);
        // The track (Background, CornerRadius) is drawn in Render, so a Token change of TrackBg or the radius
        // must redraw it.
        AffectsRender<AbstractSegmented>(
            BackgroundProperty,
            CornerRadiusProperty,
            PaddingProperty,
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
        _transitionInitialization = new TransitionInitializationSession(this);
        SelectionMode = SelectionMode.Single;
    }
    
    /// <summary>
    /// Moves the thumb behind the selected item once the control is loaded. Overrides must call the base member,
    /// otherwise the thumb is not positioned after loading.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetupSelectedThumbRect();
    }

    /// <summary>
    /// Resets the transition initialization when the template changes, so that the state written for the new
    /// template parts is not animated. Overrides must call the base member.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Selects the first item when the template is applied with items but without any selection. The current
    /// value is set, so a binding on <see cref="SelectingItemsControl.SelectedIndex" /> stays in place. Then the
    /// thumb is positioned. Overrides must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _transitionInitialization.Reset();
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

    /// <summary>
    /// Arranges the items through the base member and then moves the thumb to the new bounds of the selected
    /// item, so the thumb follows every size change of the control. Overrides must call the base member.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        SetupSelectedThumbRect();
        return result;
    }

    /// <summary>
    /// Resets the transition initialization and subscribes to <see cref="SelectingItemsControl.SelectionChanged" />
    /// when the control joins the visual tree, so the thumb follows every selection change. Overrides must call
    /// the base member, otherwise the thumb does not move on a selection change.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnAttachedToVisualTree(e);
        SelectionChanged += HandleSelectionChanged;
    }

    /// <summary>
    /// Unsubscribes from <see cref="SelectingItemsControl.SelectionChanged" />, stops a running thumb motion and
    /// resets the transition initialization when the control leaves the visual tree. Overrides must call the base
    /// member to release the subscription and stop the motion timer.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        SelectionChanged -= HandleSelectionChanged;
        StopThumbMotion();
        base.OnDetachedFromVisualTree(e);
    }

    // Called by the items panel after it relaid out its children: a theme switch rebuilds the containers and a
    // visibility change re-divides the expanding panel, and neither re-runs this control's own ArrangeOverride —
    // without this hook a thumb that captured a mid-relayout geometry would stay frozen on it.
    internal void NotifyItemsRelaidOut()
    {
        if (!_isThumbVisible)
        {
            return;
        }

        // The notification arrives from the items panel inside an in-flight layout pass: the panel has arranged
        // its children, but the surrounding measure/arrange of this control may still be running with mixed old
        // and new geometry, and ConstrainThumb would clip the thumb against a stale track. Recalibrate one
        // dispatcher turn later, when the pass has fully settled; the same-target guard keeps settled frames free.
        Avalonia.Threading.Dispatcher.UIThread.Post(
            () => SetupSelectedThumbRect(),
            Avalonia.Threading.DispatcherPriority.Loaded);
    }
    
    /// <summary>
    /// Returns whether <paramref name="item" /> needs a container. An item that is an
    /// <see cref="AbstractSegmentedItem" /> is its own container; any other item needs one, which the derived
    /// control creates, and <paramref name="recycleKey" /> then holds the key that identifies such a container.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractSegmentedItem>(item, out recycleKey);
    }

    /// <summary>
    /// Prepares an <see cref="AbstractSegmentedItem" /> container for its item after the base member ran. An
    /// item that is neither null nor a <see cref="Visual" /> becomes the content of the container, and the content
    /// template is bound to <see cref="ItemsControl.ItemTemplate" /> while this control has one.
    /// <see cref="AbstractSegmented.SizeType" />, <see cref="Shape" /> and <see cref="IsMotionEnabled" /> are
    /// bound from this control, a container that is already selected becomes the selected item, and
    /// <see cref="PrepareSegmentedItem" /> runs last. Overrides must call the base member.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is not an <see cref="AbstractSegmentedItem" />.
    /// </exception>
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
    
    /// <summary>
    /// Called from <see cref="PrepareContainerForItemOverride" /> once the content,
    /// <see cref="AbstractSegmented.SizeType" />, <see cref="Shape" /> and <see cref="IsMotionEnabled" /> of
    /// <paramref name="segmentedItem" /> are set, so that a derived class can prepare the item further. The base
    /// member does nothing.
    /// </summary>
    protected virtual void PrepareSegmentedItem(AbstractSegmentedItem segmentedItem, object? item, int index)
    {
    }

    /// <summary>
    /// Decides whether a pointer event selects an item. The release of the left mouse button does, so a mouse
    /// click selects on release instead of on press; every other input follows the base rule, and overrides must
    /// call the base member for it.
    /// </summary>
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

    /// <summary>
    /// Moves the selection to the previous item for Left and Up and to the next item for Right and Down,
    /// wrapping around at both ends, whatever the <see cref="AbstractSegmented.Orientation" /> is. The key is
    /// marked as handled when the selection moved; handled keys and all other keys are left to the base member.
    /// </summary>
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
    
    /// <summary>
    /// Draws the track with <see cref="TemplatedControl.Background" /> and the corner radius of the control, then the thumb behind
    /// the selected item while one is selected. The thumb is confined to the area inside <see cref="TemplatedControl.Padding" />,
    /// and a radius larger than half of the shorter side is clamped to that half for both shapes. The method is
    /// sealed; derived classes provide the brush and the shadow of the thumb through <see cref="ThumbPaint" />.
    /// </summary>
    public sealed override void Render(DrawingContext context)
    {
        var trackBounds = new Rect(Bounds.Size);
        // CSS border-radius semantics: a theme radius larger than half the shorter side would draw
        // asymmetrically in Avalonia, so both the track and the thumb clamp first (CornerRadiusUtils).
        var trackRadius = CornerRadiusUtils.ClampToSize(CornerRadius, trackBounds.Width, trackBounds.Height);
        context.DrawRectangle(Background, null, new RoundedRect(trackBounds, trackRadius));
        if (_isThumbVisible)
        {
            var thumb = ConstrainThumb(new Rect(SelectedThumbPos, SelectedThumbSize), Bounds.Size);
            if (thumb.Width > 0 && thumb.Height > 0)
            {
                var radius = SizeType == CustomizableSizeType.Custom &&
                             SelectedIndex >= 0 &&
                             ContainerFromIndex(SelectedIndex) is AbstractSegmentedItem item &&
                             item.IsSet(CornerRadiusProperty)
                    ? item.CornerRadius
                    : SelectedThumbCornerRadius;
                var thumbRadius = CornerRadiusUtils.ClampToSize(radius, thumb.Width, thumb.Height);
                var paint = ThumbPaint;
                context.DrawRectangle(paint.Brush, null,
                    new RoundedRect(thumb, thumbRadius), paint.Shadows);
            }
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
    
    /// <summary>
    /// Raises the <see cref="IFormItemAware.ValueChanged" /> event once
    /// <see cref="SelectingItemsControl.SelectedItem" /> has changed, so that the hosting form item notices the
    /// change. The event carries no data; <paramref name="value" /> is passed for derived overrides. Overrides
    /// must call the base member.
    /// </summary>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Assigns <paramref name="value" /> to <see cref="SelectingItemsControl.SelectedItem" /> when a form sets
    /// the value of its item. Overrides must call the base member, otherwise the form cannot select an item.
    /// </summary>
    protected virtual void NotifySetFormValue(object? value)
    {
        SelectedItem = value;
    }

    /// <summary>
    /// Returns <see cref="SelectingItemsControl.SelectedItem" />, or <see langword="null" /> while nothing is
    /// selected, when a form reads the value of its item. Overrides must call the base member to keep the form
    /// value equal to the selected item.
    /// </summary>
    protected virtual object? NotifyGetFormValue()
    {
        return SelectedItem;
    }

    /// <summary>
    /// Clears the selection by setting <see cref="SelectingItemsControl.SelectedItem" /> to
    /// <see langword="null" /> when a form resets its item; the thumb disappears with the selection. Overrides
    /// must call the base member.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SelectedItem = null;
    }

    /// <summary>
    /// Receives the validation status of the hosting form item through
    /// <see cref="IFormItemAware.NotifyValidateStatus" />. The base member does nothing, because the control
    /// shows no validation state of its own; a derived class can override it to show the status.
    /// </summary>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion

    #region 实现 ITransitionInitializationOwner 接口

    TransitionInitializationSession ITransitionInitializationOwner.TransitionInitialization => _transitionInitialization;

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
            // As in antd: when there is no source geometry to fly from, the thumb disappears at once and is not
            // animated in.
            StopThumbMotion();
            SetThumbVisible(false);
            _transitionInitialization.CompleteInitialization();
            return;
        }

        var segmentedItem = ContainerFromItem(SelectedItem);
        if (segmentedItem is not { IsArrangeValid: true } || !segmentedItem.IsAttachedToVisualTree())
        {
            // The container is not realized yet; the next arrange pass syncs the thumb.
            return;
        }

        var translated = segmentedItem.TranslatePoint(default, this);
        if (translated is not { } offset || !double.IsFinite(offset.X) || !double.IsFinite(offset.Y) ||
            !double.IsFinite(segmentedItem.Bounds.Width) || !double.IsFinite(segmentedItem.Bounds.Height) ||
            !MathUtils.GreaterThan(segmentedItem.Bounds.Width, 0) || !MathUtils.GreaterThan(segmentedItem.Bounds.Height, 0))
        {
            return;
        }

        var target = ConstrainThumb(new Rect(offset, segmentedItem.Bounds.Size), Bounds.Size);
        var targetPos = target.Position;
        var targetSize = target.Size;

        // The current value is an interpolation, not the target: arranging again for the same target must not cancel
        // the transition that is running.
        var basePos = GetBaseValue(SelectedThumbPosProperty);
        var baseSize = GetBaseValue(SelectedThumbSizeProperty);
        if (_isThumbVisible && basePos.HasValue && baseSize.HasValue &&
            targetPos == basePos.Value && targetSize == baseSize.Value)
        {
            _transitionInitialization.CompleteInitialization();
            return;
        }

        // The thumb flies only when the selection changes while the thumb is already visible; its first appearance
        // and every layout follow-up jump to the target.
        var canAnimate = animate &&
                         _transitionInitialization.IsReady &&
                         _isThumbVisible &&
                         (targetPos != SelectedThumbPos || targetSize != SelectedThumbSize);

        using var snap = !canAnimate && _transitionInitialization.IsReady
            ? new TransitionSuppressionScope(this)
            : null;

        SelectedThumbPos  = targetPos;
        SelectedThumbSize = targetSize;

        SetThumbVisible(true);
        _transitionInitialization.CompleteInitialization();

        if (canAnimate)
        {
            BeginThumbMotion();
        }
    }

    private Rect ConstrainThumb(Rect thumb, Size trackSize)
    {
        var padding = Padding;
        var inset = new Thickness(Math.Max(0, padding.Left), Math.Max(0, padding.Top),
            Math.Max(0, padding.Right), Math.Max(0, padding.Bottom));
        return thumb.Intersect(new Rect(trackSize).Deflate(inset));
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
