using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Editor surface of a single color, exposed through <see cref="Value" />. <c>ColorPicker</c> creates it as its
/// presenter, and it can also be placed directly, without that control.
/// </summary>
public class ColorPickerView : AbstractColorPickerView
{
    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
    #region 公共属性定义
    public static readonly StyledProperty<Color> ValueProperty =
        AvaloniaProperty.Register<ColorPickerView, Color>(nameof(Value),
            Colors.White,
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceColor);
/// <summary>
/// The color being edited. The default is white, and a binding on this property is two-way by default.
/// When <c>IsAlphaEnabled</c> is <see langword="false" />, the alpha channel of a value is coerced to 255.
/// A change updates <see cref="AbstractColorPickerView.HsvValue" /> from the new color and raises
/// <see cref="ValueChanged" />.
/// </summary>
#pragma warning restore ATOMUIGEN044

    public Color Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Raised after <see cref="Value" /> changes, and after <see cref="AbstractColorPickerView.HsvValue" /> changes
    /// while the view is attached to the visual tree. The arguments carry the previous and the new color. An
    /// <c>HsvValue</c> change does not update <see cref="Value" />: its colors are the old and the new HSV value
    /// converted to RGB. When a confirmation is required, the new color has become the draft before the event is
    /// raised.
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ValueChanged;
    #endregion

    internal Color? DraftValue { get; private set; }

    internal void ResetDraft(Color? value)
    {
#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
        SetCurrentValue(ValueProperty, value ?? Colors.White);
        SetCurrentValue(HsvValueProperty, (value ?? Colors.White).ToHsv());
#pragma warning restore ATOMUIGEN044
        DraftValue = value;
        SetCurrentValue(IsEmptyDraftProperty, IsNeedConfirm && value == null);
        BeginEdit();
    }

    /// <summary>
    /// Called when the clear block named <c>PART_ClearColor</c> raises <c>ClearRequest</c>. The base call makes
    /// <see cref="AbstractColorPickerView.HsvValue" /> transparent and raises
    /// <see cref="AbstractColorPickerView.ColorValueCleared" />. This override then discards the draft and marks
    /// it empty when a confirmation is required.
    /// </summary>
    protected override void NotifyColorClearRequest()
    {
        base.NotifyColorClearRequest();
        DraftValue = null;
        SetCurrentValue(IsEmptyDraftProperty, IsNeedConfirm);
    }

    /// <summary>
    /// Keeps the editor state in step with property changes. The base member is called only while
    /// <c>IgnorePropertyChanged</c> is set. Otherwise a change of
    /// <see cref="AbstractColorPickerView.IsNeedConfirm" /> recomputes
    /// <see cref="AbstractColorPickerView.IsEmptyDraft" />, a <see cref="Value" /> change updates
    /// <see cref="AbstractColorPickerView.HsvValue" /> from the new color without reporting that follow-up change,
    /// and an <c>HsvValue</c> change refreshes the HSV brushes and reports the color while the view is attached to
    /// the visual tree.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (IgnorePropertyChanged)
        {
            base.OnPropertyChanged(change);
            return;
        }
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
            ConfigureHsvColorBrushes();
            if (this.IsAttachedToVisualTree())
            {
                NotifyColorChanged(new ColorChangedEventArgs(
                    change.GetOldValue<HsvColor>().ToRgb(),
                    change.GetNewValue<HsvColor>().ToRgb()));
            }
        }
    }

    /// <summary>
    /// Reports a color change by raising <see cref="ValueChanged" />. When a confirmation is required, the new color
    /// first becomes the draft and <see cref="AbstractColorPickerView.IsEmptyDraft" /> is set to whether it is
    /// <see langword="null" />. Called for a <see cref="Value" /> change and for an <c>HsvValue</c> change while the
    /// view is attached to the visual tree.
    /// </summary>
    protected virtual void NotifyColorChanged(ColorChangedEventArgs e)
    {
        if (IsNeedConfirm)
        {
            DraftValue = e.NewColor;
            SetCurrentValue(IsEmptyDraftProperty, e.NewColor == null);
        }
        ValueChanged?.Invoke(this, e);
    }

    internal override void NotifyInputColorEdited()
    {
        DraftValue = HsvValue.ToRgb();
        SetCurrentValue(IsEmptyDraftProperty, false);
    }

    /// <summary>
    /// Called after the user picks a color in the palette. It sets <see cref="Value" /> to the picked color and, when
    /// a confirmation is required, also makes that color the draft and marks the draft as not empty.
    /// </summary>
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
