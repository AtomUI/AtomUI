using System.Diagnostics;
using AtomUI.Icons.AntDesign;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of rating controls. It creates one star per <see cref="Count" />, shows the rating of
/// <see cref="Value" /> on them, and turns pointer and keyboard input into changes of <see cref="Value" />.
/// </summary>
/// <remarks>
/// The template must contain an <see cref="ItemsControl" /> named <c>PART_RateItems</c>. A derived class that
/// rates through its own input returns <see langword="false" /> from <see cref="HandlesPointerRating" />.
/// </remarks>
[TemplatePart("PART_RateItems", typeof(ItemsControl))]
public abstract class AbstractRate : TemplatedControl, 
                                     IMotionAwareControl, 
                                     ISizeTypeAware,
                                     IFormItemAware
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsAllowClear), true);
    
    /// <summary>
    /// Identifies the <see cref="IsAllowHalf" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowHalfProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsAllowHalf));
    
    /// <summary>
    /// Identifies the <see cref="Character" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CharacterProperty =
        AvaloniaProperty.Register<AbstractRate, object?>(nameof(Character));
    
    /// <summary>
    /// Identifies the <see cref="StarColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StarColorProperty =
        AvaloniaProperty.Register<AbstractRate, IBrush?>(nameof(StarColor));
    
    /// <summary>
    /// Identifies the <see cref="StarBgColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StarBgColorProperty =
        AvaloniaProperty.Register<AbstractRate, IBrush?>(nameof(StarBgColor));
    
    /// <summary>
    /// Identifies the <see cref="Count" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<AbstractRate, int>(nameof(Count), 5);
    
    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<AbstractRate, double>(
            nameof(Value),
            double.NaN,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="DefaultValue" /> property.
    /// </summary>
    public static readonly StyledProperty<double> DefaultValueProperty =
        AvaloniaProperty.Register<AbstractRate, double>(nameof(DefaultValue), 0);
    
    /// <summary>
    /// Identifies the <see cref="IsKeyboardEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsKeyboardEnabledProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsKeyboardEnabled), true);
    
    /// <summary>
    /// Identifies the <see cref="IsReadOnly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsReadOnly));

    /// <summary>
    /// Identifies the <see cref="IsHoverPreviewEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHoverPreviewEnabledProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsHoverPreviewEnabled), true);

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractRate>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractRate>();
    
    /// <summary>
    /// Lets the user clear the rating: when the pointer is released over the value that is already selected,
    /// <see cref="Value" /> becomes 0 instead. With <see cref="IsAllowHalf" /> disabled, both values are rounded to
    /// whole stars before they are compared.
    /// </summary>
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    /// <summary>
    /// Enables half-star ratings: the left half of a star selects its index plus 0.5, the Left and Right keys step
    /// <see cref="Value" /> by 0.5 instead of 1, and a half-selected star shows only its left half in
    /// <see cref="StarColor" />.
    /// </summary>
    public bool IsAllowHalf
    {
        get => GetValue(IsAllowHalfProperty);
        set => SetValue(IsAllowHalfProperty, value);
    }
    
    /// <summary>
    /// The glyph of every star: an <see cref="Icon" />, a <see cref="char" />, or a <see cref="string" /> whose first
    /// character is used. The glyph is painted with <see cref="StarColor" /> and <see cref="StarBgColor" />; the
    /// brushes of an icon are ignored. <see langword="null" /> is replaced with a <see cref="StarFilled" /> icon
    /// when the control is initialized; any other value paints nothing.
    /// </summary>
    public object? Character
    {
        get => GetValue(CharacterProperty);
        set => SetValue(CharacterProperty, value);
    }
    
    /// <summary>
    /// Brush of the glyph of the selected stars, of the left half of a half-selected star, and of the dashed outline
    /// of a star that is marked as the start of keyboard stepping. Set it on the rate; it applies to every star.
    /// Clearing the value restores the brush that the theme sets.
    /// </summary>
    public IBrush? StarColor
    {
        get => GetValue(StarColorProperty);
        set => SetValue(StarColorProperty, value);
    }
    
    /// <summary>
    /// Brush of the glyph of the unselected stars and of the right half of a half-selected star. Set it on the rate;
    /// it applies to every star. Clearing the value restores the brush that the theme sets.
    /// </summary>
    public IBrush? StarBgColor
    {
        get => GetValue(StarBgColorProperty);
        set => SetValue(StarBgColorProperty, value);
    }

    /// <summary>
    /// Number of stars. Pointer and keyboard input never produce a <see cref="Value" /> above it. Changing it while
    /// the control is attached to a visual tree rebuilds the stars; 0 or less shows no star.
    /// </summary>
    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
    
    /// <summary>
    /// Current rating in stars, with steps of 0.5 when <see cref="IsAllowHalf" /> is enabled. Binds two-way by default,
    /// with data validation enabled for its binding. <see cref="double.NaN" /> marks a rating that is not set and is
    /// replaced by <see cref="DefaultValue" /> when the control is initialized. A value that is set directly is not
    /// clamped to <see cref="Count" />.
    /// </summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    /// <summary>
    /// Rating that <see cref="Value" /> takes when the control is initialized while <see cref="Value" /> is
    /// <see cref="double.NaN" />. Changing it later does not change <see cref="Value" />.
    /// </summary>
    public double DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    /// <summary>
    /// Lets the Left and Right keys change the rating while the control has focus. The keys are ignored while
    /// <see cref="IsReadOnly" /> is <see langword="true" />, or while the pointer is inside the control and
    /// <see cref="IsHoverPreviewEnabled" /> is <see langword="true" />.
    /// </summary>
    public bool IsKeyboardEnabled
    {
        get => GetValue(IsKeyboardEnabledProperty);
        set => SetValue(IsKeyboardEnabledProperty, value);
    }
    
    /// <summary>
    /// Blocks rating changes from pointer and keyboard input; <see cref="Value" /> can still be set by code or a
    /// binding. While it is <see langword="true" />, the stars show no hover preview and presses are not tracked.
    /// Changing this property cancels the pointer gesture in progress and returns the stars to <see cref="Value" />.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Lets the stars preview the rating under the pointer without changing <see cref="Value" />; the preview ends
    /// when the pointer leaves the control. While the pointer is inside the control, the Left and Right keys are
    /// ignored. Changing this property ends the preview.
    /// </summary>
    public bool IsHoverPreviewEnabled
    {
        get => GetValue(IsHoverPreviewEnabledProperty);
        set => SetValue(IsHoverPreviewEnabledProperty, value);
    }
    
    /// <summary>
    /// Size of every star: <see cref="SizeType.Small" />, <see cref="SizeType.Middle" /> or
    /// <see cref="SizeType.Large" />. Set it on the rate; it applies to every star.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Animates the scale change of a star that is hovered or marked as the start of keyboard stepping; when
    /// disabled, the star scales at once. Set it on the rate; it applies to every star. Clearing the value returns to
    /// the value of the theme.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised after <see cref="Value" /> changed, with the previous and the new rating.
    /// </summary>
    public event EventHandler<RateValueChangedEventArgs>? ValueChanged;
    /// <summary>
    /// Raised after the rating shown by the stars changed: during a hover preview, and when <see cref="Value" />
    /// changes that rating as well, then before <see cref="ValueChanged" />. The arguments carry the zero-based index
    /// of the last star covered by the previous and the new rating, or -1 for a rating of 0.
    /// </summary>
    public event EventHandler<RateValueChangedEventArgs>? HoverValueChanged;

    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<AbstractRate, double> EffectiveValueProperty =
        AvaloniaProperty.RegisterDirect<AbstractRate, double>(
            nameof(EffectiveValue),
            o => o.EffectiveValue,
            (o, v) => o.EffectiveValue = v);
    
    private double _effectiveValue;

    internal double EffectiveValue
    {
        get => _effectiveValue;
        set => SetAndRaise(EffectiveValueProperty, ref _effectiveValue, value);
    }
    #endregion
    
    private ItemsControl? _itemsControl;
    private IDisposable? _pointerEventHandleDisposable;
    private double? _pressedEffectiveValue;
    private bool _isPointerInRate;
    private RateItem? _focusStartItem;
    
    static AbstractRate()
    {
        AffectsMeasure<AbstractRate>(CountProperty, CharacterProperty);
        AffectsRender<AbstractRate>(ValueProperty, StarColorProperty);
        ValueProperty.Changed.AddClassHandler<AbstractRate>((rate, args) => rate.HandleValueChanged());
    }

    /// <summary>
    /// Replaces a <see langword="null" /> <see cref="Character" /> with a <see cref="StarFilled" /> icon and an unset
    /// <see cref="Value" /> with <see cref="DefaultValue" />.
    /// </summary>
    /// <remarks>
    /// Call the base method first; it performs both replacements.
    /// </remarks>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Character == null)
        {
            SetCurrentValue(CharacterProperty, new StarFilled());
        }

        if (double.IsNaN(Value))
        {
            SetCurrentValue(ValueProperty, DefaultValue);
        }
    }

    /// <summary>
    /// Rebuilds the stars when <see cref="Count" /> changes while the control is attached, shows a changed
    /// <see cref="Value" /> on the stars, and resets the pointer state when <see cref="IsReadOnly" /> or
    /// <see cref="IsHoverPreviewEnabled" /> changes or the control becomes disabled.
    /// </summary>
    /// <remarks>
    /// Call the base method first. It raises <see cref="HoverValueChanged" /> and <see cref="ValueChanged" />.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == CountProperty)
            {
                HandleCountChanged();
            }
        }

        if (change.Property == ValueProperty)
        {
            SetCurrentValue(EffectiveValueProperty, Value);
            ValueChanged?.Invoke(this, new RateValueChangedEventArgs(change.GetOldValue<double>(), change.GetNewValue<double>()));
        }
        else if (change.Property == EffectiveValueProperty)
        {
            HandleEffectiveValueChanged();
            var oldEffectiveValue = (int)(Math.Round(change.GetOldValue<double>(), MidpointRounding.AwayFromZero) - 1);
            var newEffectiveValue = (int)(Math.Round(change.GetNewValue<double>(), MidpointRounding.AwayFromZero) - 1);
            HoverValueChanged?.Invoke(this, new RateValueChangedEventArgs(oldEffectiveValue, newEffectiveValue));
        }
        else if (change.Property == IsReadOnlyProperty || change.Property == IsHoverPreviewEnabledProperty ||
                 change.Property == IsEffectivelyEnabledProperty && !IsEffectivelyEnabled)
        {
            ResetPointerState();
        }
    }

    /// <summary>
    /// Subscribes to the raw pointer input of the input manager when <see cref="HandlesPointerRating" /> is enabled.
    /// The subscription drives the press, release and hover tracking of the stars.
    /// </summary>
    /// <remarks>
    /// Call the base method first.
    /// </remarks>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!HandlesPointerRating)
        {
            return;
        }
        var inputManager = AvaloniaLocator.Current.GetService(typeof(IInputManager)) as IInputManager;
        Debug.Assert(inputManager != null);
        _pointerEventHandleDisposable = inputManager.Process.Subscribe(HandleGlobalPointerEvent);
    }

    /// <summary>
    /// Disposes the raw pointer subscription that was made on attach and resets the pointer state.
    /// </summary>
    /// <remarks>
    /// Call the base method first.
    /// </remarks>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _pointerEventHandleDisposable?.Dispose();
        _pointerEventHandleDisposable = null;
        ResetPointerState();
    }
    
    private void HandleGlobalPointerEvent(RawInputEventArgs args)
    {
        if (!IsEffectivelyEnabled || IsReadOnly)
        {
            return;
        }
        if (args is RawPointerEventArgs pointerEventArgs &&
            this.TryGetInputPosition(pointerEventArgs, out var position))
        {
            if (pointerEventArgs.Type is RawPointerEventType.LeaveWindow or RawPointerEventType.TouchCancel)
            {
                ResetPointerState();
                return;
            }

            // The shared input-root check has already rejected events of other windows.
            // The hit test still starts from the whole root so that covering elements and clipping are respected.
            var root = this.GetPresentationSource()?.RootVisual;
            var hit = new Rect(Bounds.Size).Contains(position)
                ? (root as IInputElement)?.InputHitTest(pointerEventArgs.Position) as Visual
                : null;
            var isPointerInRate = hit is not null && (ReferenceEquals(hit, this) || this.IsVisualAncestorOf(hit));
            if (pointerEventArgs.Type == RawPointerEventType.Move)
            {
                if (!isPointerInRate)
                {
                    _isPointerInRate = false;
                    SetCurrentValue(EffectiveValueProperty, Value);
                }
                else
                {
                    _isPointerInRate = true;
                }
            }
            else if (pointerEventArgs.Type == RawPointerEventType.LeftButtonDown)
            {
                _pressedEffectiveValue = isPointerInRate ? GetPointerValue(position) : null;
            }
            else if (pointerEventArgs.Type == RawPointerEventType.LeftButtonUp)
            {
                // Every release ends the current gesture, so a release outside the control or in another window
                // leaves no pressed state behind.
                // The pressed value is read and cleared before the value is committed, so a handler that re-enters
                // cannot reuse the ended gesture.
                var pressedValue = _pressedEffectiveValue;
                _pressedEffectiveValue = null;
                if (isPointerInRate && pressedValue is { } pressed && GetPointerValue(position) is { } released &&
                    MathUtils.AreClose(Math.Round(pressed, MidpointRounding.AwayFromZero),
                        Math.Round(released, MidpointRounding.AwayFromZero)))
                {
                    CommitPointerValue(released);
                }
            }
        }
    }

    /// <summary>
    /// Returns the rating under a position relative to this control, or <see langword="null" /> when the stars are not
    /// available yet or the position lies in none of them.
    /// </summary>
    /// <param name="position">
    /// The position in the coordinates of this control.
    /// </param>
    /// <returns>
    /// 0 left of the first star, <see cref="Count" /> from the right edge of the last star onwards, and otherwise the
    /// index of the star plus 1, or plus 0.5 for its left half when <see cref="IsAllowHalf" /> is enabled.
    /// </returns>
    protected double? GetPointerValue(Point position) => _itemsControl is not null &&
        this.TranslatePoint(position, _itemsControl) is { } itemPosition
        ? CalculateEffectiveValue(itemPosition)
        : null;

    /// <summary>
    /// Sets <see cref="Value" /> to a rating under the pointer, or to 0 when <see cref="IsAllowClear" /> is enabled and
    /// the rating is already selected. Does nothing when <see cref="IsReadOnly" /> is enabled or the control is
    /// disabled.
    /// </summary>
    /// <param name="value">
    /// The rating to commit, for example the result of <see cref="GetPointerValue" />.
    /// </param>
    protected void CommitPointerValue(double value)
    {
        if (IsReadOnly || !IsEffectivelyEnabled)
        {
            return;
        }

        var wasSelected = IsAllowHalf ? MathUtils.AreClose(Value, value) :
            MathUtils.AreClose(Math.Round(Value, MidpointRounding.AwayFromZero), Math.Round(value, MidpointRounding.AwayFromZero));
        SetCurrentValue(ValueProperty, IsAllowClear && wasSelected ? 0d : value);
    }

    private void ResetPointerState()
    {
        _pressedEffectiveValue = null;
        _isPointerInRate = false;
        SetCurrentValue(EffectiveValueProperty, Value);
    }

    /// <summary>
    /// Resets the pointer state, finds the <c>PART_RateItems</c> items control, and brings its stars in step with
    /// <see cref="Count" /> and <see cref="Value" />.
    /// </summary>
    /// <remarks>
    /// Call the base method first. It assigns <see cref="RateItems" />, so an override can use that property only
    /// after the call.
    /// </remarks>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ResetPointerState();
        _itemsControl = e.NameScope.Find<ItemsControl>("PART_RateItems");
        HandleCountChanged();
        HandleEffectiveValueChanged();
    }

    private void HandleCountChanged()
    {
        if (_itemsControl != null)
        {
            _itemsControl.Items.Clear();
            for (var i = 0; i < Count; i++)
            {
                var rateItem = new RateItem();
                _itemsControl.Items.Add(rateItem);
            }
        }

        HandleEffectiveValueChanged();
        OnRateItemsChanged();
    }

    private void HandleEffectiveValueChanged()
    {
        if (_itemsControl != null)
        {
            for (var i = 0; i < Count; i++)
            {
                if (_itemsControl.Items[i] is RateItem rateItem)
                {
                    if (MathUtils.LessThan(i, EffectiveValue))
                    {
                        if (IsAllowHalf)
                        {
                            var delta = EffectiveValue - i;
                            if (MathUtils.LessThanOrClose(delta, 0.5d))
                            {
                                rateItem.SelectedState = RateItemSelectedState.HalfSelected;
                            }
                            else
                            {
                                rateItem.SelectedState = RateItemSelectedState.FullSelected;
                            }
                        }
                        else
                        {
                            rateItem.SelectedState = RateItemSelectedState.FullSelected;
                        }
                    }
                    else
                    {
                        rateItem.SelectedState = RateItemSelectedState.None;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Previews the rating under the pointer on the stars without changing <see cref="Value" />, when
    /// <see cref="HandlesPointerRating" /> and <see cref="IsHoverPreviewEnabled" /> are enabled and the control is
    /// neither read-only nor disabled.
    /// </summary>
    /// <remarks>
    /// Call the base method first.
    /// </remarks>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!HandlesPointerRating || IsReadOnly || !IsHoverPreviewEnabled || !IsEffectivelyEnabled)
        {
            return;
        }

        var point = e.GetPosition(_itemsControl);
        var value = CalculateEffectiveValue(point);
        if (value != null)
        {
            SetCurrentValue(EffectiveValueProperty, value);
        }
    }

    /// <summary>
    /// Remembers the star under the pointer as the start of keyboard stepping. <see cref="OnKeyDown" /> marks that star
    /// when Left or Right is pressed while keyboard rating is allowed. Does nothing when
    /// <see cref="HandlesPointerRating" /> is disabled, the control is read-only or the template has not been applied.
    /// </summary>
    /// <remarks>
    /// Call the base method first.
    /// </remarks>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (HandlesPointerRating && !IsReadOnly && _itemsControl != null)
        {
            for (var i = 0; i < Count; i++)
            {
                if (_itemsControl.ContainerFromIndex(i) is RateItem rateItem)
                {
                    if (GetItemBounds(_itemsControl, i) is { } bounds && bounds.Contains(e.GetPosition(_itemsControl)))
                    {
                        _focusStartItem = rateItem;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Removes the keyboard stepping mark from the star that was pressed when the control loses focus.
    /// </summary>
    /// <remarks>
    /// Call the base method first.
    /// </remarks>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (_focusStartItem != null)
        {
            _focusStartItem.IsFocusStartItem = false;
            _focusStartItem                  = null;
        }
    }

    private double? CalculateEffectiveValue(Point point)
    {
        if (_itemsControl is not { } items || Count <= 0)
        {
            return null;
        }

        var first = GetItemBounds(items, 0);
        var last = GetItemBounds(items, Count - 1);
        var isMirrored = first is { } firstRect && last is { } lastRect && firstRect.Left > lastRect.Left;
        var position = isMirrored ? -point.X : point.X;
        if (first is { } start && position < (isMirrored ? -start.Right : start.Left))
        {
            return 0;
        }
        if (last is { } end && position >= (isMirrored ? -end.Left : end.Right))
        {
            return Count;
        }

        for (var index = 0; index < Count; index++)
        {
            if (GetItemBounds(items, index) is { } bounds)
            {
                var left = isMirrored ? -bounds.Right : bounds.Left;
                var right = isMirrored ? -bounds.Left : bounds.Right;
                if (MathUtils.GreaterThanOrClose(position, left) && MathUtils.LessThan(position, right))
                {
                    var center = isMirrored ? -bounds.Center.X : bounds.Center.X;
                    return index + (IsAllowHalf && MathUtils.LessThanOrClose(position, center) ? .5 : 1);
                }
            }
        }

        return null;
    }

    private static Rect? GetItemBounds(ItemsControl items, int index)
    {
        return items.ContainerFromIndex(index) is RateItem item && item.TransformToVisual(items) is { } transform
            ? new Rect(item.Bounds.Size).TransformToAABB(transform)
            : null;
    }

    /// <summary>
    /// Gives this class the pointer rating: the subscription to raw pointer input, the hover preview and the start
    /// mark of keyboard stepping. Returns <see langword="true" /> by default. A derived class that rates through its
    /// own input overrides it to return <see langword="false" /> and takes over all three. The subscription is decided
    /// once, when the control is attached.
    /// </summary>
    protected virtual bool HandlesPointerRating => true;

    /// <summary>
    /// The items control of the template that holds the stars, or <see langword="null" /> before the template is
    /// applied.
    /// </summary>
    private protected ItemsControl? RateItems => _itemsControl;

    /// <summary>
    /// Called after the stars were created for the current <see cref="Count" />: when the template is applied and
    /// when <see cref="Count" /> changes while the control is attached.
    /// </summary>
    /// <remarks>
    /// The default implementation does nothing. <see cref="RateItems" /> is assigned when this method is called.
    /// </remarks>
    private protected virtual void OnRateItemsChanged()
    {
    }

    /// <summary>
    /// Changes <see cref="Value" /> by one step, 0.5 with <see cref="IsAllowHalf" /> and 1 without, when Left or Right
    /// is pressed and keyboard rating is allowed (see <see cref="IsKeyboardEnabled" />). The result is kept between 0
    /// and <see cref="Count" />.
    /// </summary>
    /// <remarks>
    /// Call the base method first.
    /// </remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsReadOnly && IsKeyboardEnabled && (!_isPointerInRate || !IsHoverPreviewEnabled))
        {
            var currentValue = Value;
            if (e.Key == Key.Left)
            {
                if (IsAllowHalf)
                {
                    currentValue -= 0.5;
                }
                else
                {
                    currentValue -= 1.0;
                }

                if (_focusStartItem != null)
                {
                    _focusStartItem.IsFocusStartItem = true;
                }
            }
            else if (e.Key == Key.Right)
            {
                if (IsAllowHalf)
                {
                    currentValue += 0.5;
                }
                else
                {
                    currentValue += 1.0;
                }
                if (_focusStartItem != null)
                {
                    _focusStartItem.IsFocusStartItem = true;
                }
            }
            currentValue = Math.Max(0, Math.Min(currentValue, Count));
            SetCurrentValue(ValueProperty, currentValue);
        }
    }
    
    #region 实现 FormItem 接口
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as double?);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    private void HandleValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Assigns a rating that the form system sets on the control. The default writes it into <see cref="Value" /> with
    /// <c>SetCurrentValue</c>, so a binding on the property is kept.
    /// </summary>
    /// <param name="value">
    /// The rating to assign.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is <see langword="null" />, because <see cref="Value" /> is not nullable.
    /// </exception>
    protected virtual void NotifySetFormValue(double? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    /// <summary>
    /// Returns the rating that the form system reads from the control. The default returns <see cref="Value" />.
    /// </summary>
    protected virtual double? NotifyGetFormValue()
    {
        return Value;
    }

    /// <summary>
    /// Resets the rating when the form system clears the value of the control. The default sets <see cref="Value" />
    /// to 0.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(ValueProperty, 0.0);
    }

    /// <summary>
    /// Passes the validation status that the form system assigns to the control. The default does nothing.
    /// </summary>
    /// <param name="status">
    /// The validation status of the form item that hosts the control.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
