using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class ColorPickerView : AbstractColorPickerView
{
    #region 公共属性定义
    public static readonly StyledProperty<Color> ValueProperty =
        AvaloniaProperty.Register<ColorPickerView, Color>(nameof(Value),
            Colors.White,
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceColor);

    public Color Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    #endregion

    #region 公共事件定义
    public event EventHandler<ColorChangedEventArgs>? ValueChanged;
    #endregion

    #region 内部属性定义

    internal Color? DraftValue { get; private set; }

    #endregion

    #region 内部协作 API

    internal void ResetDraft(Color? value)
    {
        SetCurrentValue(ValueProperty, value ?? Colors.White);
        SetCurrentValue(HsvValueProperty, (value ?? Colors.White).ToHsv());
        DraftValue = value;
        SetCurrentValue(IsEmptyDraftProperty, IsNeedConfirm && value == null);
        BeginEdit();
    }

    #endregion

    protected override void NotifyColorClearRequest()
    {
        base.NotifyColorClearRequest();
        DraftValue = null;
        SetCurrentValue(IsEmptyDraftProperty, IsNeedConfirm);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (IgnorePropertyChanged)
        {
            base.OnPropertyChanged(change);
            return;
        }
        base.OnPropertyChanged(change);
        if (change.Property == IsNeedConfirmProperty)
        {
            SetCurrentValue(IsEmptyDraftProperty, IsNeedConfirm && DraftValue == null);
        }
        if (change.Property == ValueProperty)
        {
            IgnorePropertyChanged = true;

            SetCurrentValue(HsvValueProperty, Value.ToHsv());

            NotifyColorChanged(new ColorChangedEventArgs(
                change.GetOldValue<Color>(),
                change.GetNewValue<Color>()));

            IgnorePropertyChanged = false;
        }

        if (change.Property == HsvValueProperty)
        {
            if (this.IsAttachedToVisualTree())
            {
                NotifyColorChanged(new ColorChangedEventArgs(
                    change.GetOldValue<HsvColor>().ToRgb(),
                    change.GetNewValue<HsvColor>().ToRgb()));
            }
        }
    }

    protected virtual void NotifyColorChanged(ColorChangedEventArgs e)
    {
        if (IsNeedConfirm)
        {
            DraftValue = e.NewColor;
            SetCurrentValue(IsEmptyDraftProperty, e.NewColor == null);
        }
        ValueChanged?.Invoke(this, e);
    }

    #region 内部协作 API

    internal override void NotifyInputColorEdited()
    {
        DraftValue = HsvValue.ToRgb();
        SetCurrentValue(IsEmptyDraftProperty, false);
    }

    #endregion

    protected override void NotifyPaletteColorSelected(Color color)
    {
        SetCurrentValue(ValueProperty, color);
        if (IsNeedConfirm)
        {
            // Explicit selection forms a draft even when equal to the editor fallback.
            DraftValue = Value;
            SetCurrentValue(IsEmptyDraftProperty, false);
        }
    }
}
