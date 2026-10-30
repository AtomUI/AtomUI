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
    
    public static readonly StyledProperty<IList<string>?> ToolTipsProperty =
        AvaloniaProperty.Register<AbstractRate, IList<string>?>(nameof(ToolTips));

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
    
    public IList<string>? ToolTips
    {
        get => GetValue(ToolTipsProperty);
        set => SetValue(ToolTipsProperty, value);
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
        else if (change.Property == ToolTipsProperty)
        {
            ConfigureToolTips();
        }
        else if (change.Property == IsEffectivelyEnabledProperty && !IsEffectivelyEnabled)
        {
            ResetPointerState();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
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
        if (!IsEffectivelyEnabled)
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
                _pressedEffectiveValue = isPointerInRate ? EffectiveValue : null;
            }
            else if (pointerEventArgs.Type == RawPointerEventType.LeftButtonUp)
            {
                // 每次释放都结束当前手势，区域外/跨窗口释放同样不能留下按下状态。
                // 在 Value 回调前取出并清空，避免回调重入沿用已结束的手势。
                var pressedValue = _pressedEffectiveValue;
                _pressedEffectiveValue = null;
                if (isPointerInRate)
                {
                    if (pressedValue != null && MathUtils.AreClose(Math.Round(pressedValue.Value, MidpointRounding.AwayFromZero), Math.Round(EffectiveValue, MidpointRounding.AwayFromZero)))
                    {
                        if (IsAllowClear)
                        {
                            var localPoint = _itemsControl is not null ? this.TranslatePoint(position, _itemsControl) : null;
                            var value = localPoint is { } itemPosition ? CalculateEffectiveValue(itemPosition) : null;
                            if (value != null)
                            {
                                if ((IsAllowHalf && MathUtils.AreClose(Value, value.Value)) ||
                                    (!IsAllowHalf && MathUtils.AreClose(Math.Round(Value, MidpointRounding.AwayFromZero), 
                                        Math.Round(value.Value, MidpointRounding.AwayFromZero))))
                                {
                                    SetCurrentValue(ValueProperty, 0d);
                                }
                                else
                                {
                                    SetCurrentValue(ValueProperty, value);
                                }
                            }
                        }
                        else
                        {
                            SetCurrentValue(ValueProperty, EffectiveValue);
                        }
                        
                    }
                }
            }
        }
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
        ConfigureToolTips();
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
        if (_itemsControl != null)
        {
            for (var i = 0; i < Count; i++)
            {
                if (_itemsControl.ContainerFromIndex(i) is RateItem rateItem)
                {
                    if (rateItem.Bounds.Contains(e.GetPosition(_itemsControl)))
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
        double? value = null;
        if (_itemsControl != null)
        {
            var offsetX = point.X;
            if (Count > 0)
            {
                var firstItem = _itemsControl.ContainerFromIndex(0) as RateItem;
                var lastItem  = _itemsControl.ContainerFromItem(Count - 1) as RateItem;
                if (firstItem != null)
                {
                    if (offsetX < firstItem.Bounds.Left)
                    {
                        value = 0;
                    }
                }

                if (lastItem != null)
                {
                    if (offsetX > lastItem.Bounds.Right)
                    {
                        value = Count;
                    }
                }
            }
            for (var i = 0; i < Count; i++)
            {
                if (_itemsControl.ContainerFromIndex(i) is RateItem rateItem)
                {
                    var bounds = rateItem.Bounds;
                    var left   = bounds.Left;
                    var right  = bounds.Right;
                    var middle = bounds.Center.X;
                    if (IsAllowHalf)
                    {
                        if (MathUtils.GreaterThanOrClose(offsetX, left) && MathUtils.LessThanOrClose(offsetX, middle))
                        {
                            value = i + 0.5;
                        }
                        else if (MathUtils.GreaterThan(offsetX, middle) && MathUtils.LessThan(offsetX, right))
                        {
                  
                            value = i + 1;
                        }
                    }
                    else
                    {
                        if (MathUtils.GreaterThanOrClose(offsetX, left) && MathUtils.LessThan(offsetX, right))
                        {
                            value = i + 1;
                        }
                    }
                }
            }
        }
        return value;
    }

    private void ConfigureToolTips()
    {
        if (_itemsControl != null && ToolTips?.Count > 0)
        {
            for (var i = 0; i < Count; i++)
            {
                if (_itemsControl.Items[i] is RateItem rateItem && i < ToolTips.Count)
                {
                    ToolTip.SetTip(rateItem, ToolTips[i]);
                }
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsKeyboardEnabled && !_isPointerInRate)
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
