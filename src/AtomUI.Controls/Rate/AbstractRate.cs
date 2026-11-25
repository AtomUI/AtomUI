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

[TemplatePart("PART_RateItems", typeof(ItemsControl))]
public abstract class AbstractRate : TemplatedControl, 
                                     IMotionAwareControl, 
                                     ISizeTypeAware,
                                     IFormItemAware
{
    #region 公共属性定义
    
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsAllowClear), true);
    
    public static readonly StyledProperty<bool> IsAllowHalfProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsAllowHalf));
    
    public static readonly StyledProperty<object?> CharacterProperty =
        AvaloniaProperty.Register<AbstractRate, object?>(nameof(Character));
    
    public static readonly StyledProperty<IBrush?> StarColorProperty =
        AvaloniaProperty.Register<AbstractRate, IBrush?>(nameof(StarColor));
    
    public static readonly StyledProperty<IBrush?> StarBgColorProperty =
        AvaloniaProperty.Register<AbstractRate, IBrush?>(nameof(StarBgColor));
    
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<AbstractRate, int>(nameof(Count), 5);
    
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<AbstractRate, double>(
            nameof(Value),
            double.NaN,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<double> DefaultValueProperty =
        AvaloniaProperty.Register<AbstractRate, double>(nameof(DefaultValue), 0);
    
    public static readonly StyledProperty<bool> IsKeyboardEnabledProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsKeyboardEnabled), true);
    
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<bool> IsHoverPreviewEnabledProperty =
        AvaloniaProperty.Register<AbstractRate, bool>(nameof(IsHoverPreviewEnabled), true);

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractRate>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractRate>();
    
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    public bool IsAllowHalf
    {
        get => GetValue(IsAllowHalfProperty);
        set => SetValue(IsAllowHalfProperty, value);
    }
    
    public object? Character
    {
        get => GetValue(CharacterProperty);
        set => SetValue(CharacterProperty, value);
    }
    
    public IBrush? StarColor
    {
        get => GetValue(StarColorProperty);
        set => SetValue(StarColorProperty, value);
    }
    
    public IBrush? StarBgColor
    {
        get => GetValue(StarBgColorProperty);
        set => SetValue(StarBgColorProperty, value);
    }

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
    
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public double DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public bool IsKeyboardEnabled
    {
        get => GetValue(IsKeyboardEnabledProperty);
        set => SetValue(IsKeyboardEnabledProperty, value);
    }
    
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool IsHoverPreviewEnabled
    {
        get => GetValue(IsHoverPreviewEnabledProperty);
        set => SetValue(IsHoverPreviewEnabledProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    public event EventHandler<RateValueChangedEventArgs>? ValueChanged;
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

            // 共用输入根校验已排除异窗事件；命中仍从整个 root 检查遮罩与裁剪。
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
                // 每次释放都结束当前手势，区域外/跨窗口释放同样不能留下按下状态。
                // 在 Value 回调前取出并清空，避免回调重入沿用已结束的手势。
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

    protected double? GetPointerValue(Point position) => _itemsControl is not null &&
        this.TranslatePoint(position, _itemsControl) is { } itemPosition
        ? CalculateEffectiveValue(itemPosition)
        : null;

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

    protected virtual bool HandlesPointerRating => true;

    private protected ItemsControl? RateItems => _itemsControl;

    private protected virtual void OnRateItemsChanged()
    {
    }

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

    protected virtual void NotifySetFormValue(double? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    protected virtual double? NotifyGetFormValue()
    {
        return Value;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(ValueProperty, 0.0);
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
