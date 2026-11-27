using Avalonia;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Editor surface of a linear gradient, exposed through <see cref="Value" />. <c>GradientColorPicker</c> creates
/// it as its presenter.
/// </summary>
public class GradientColorPickerView : AbstractColorPickerView
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="DefaultValue" /> property.
    /// </summary>
    public static readonly StyledProperty<LinearGradientBrush?> DefaultValueProperty =
        AvaloniaProperty.Register<GradientColorPickerView, LinearGradientBrush?>(nameof(DefaultValue));

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<LinearGradientBrush?> ValueProperty =
        AvaloniaProperty.Register<GradientColorPickerView, LinearGradientBrush?>(nameof(Value));

    /// <summary>
    /// The gradient that <see cref="Value" /> takes when <see cref="Value" /> is <see langword="null" />: when the
    /// view initializes, and afterwards, while the view is attached to the visual tree, whenever this property
    /// changes. The brush is assigned as is, without a copy. The default is <see langword="null" />, which does not
    /// seed <see cref="Value" />.
    /// </summary>
    public LinearGradientBrush? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    /// <summary>
    /// The gradient being edited, or <see langword="null" /> when none is set. A change raises
    /// <see cref="GradientValueChanged" />. While a confirmation is required and an edit is in progress, the
    /// <c>Opacity</c> and <c>SpreadMethod</c> of the previous brush are copied to the new one.
    /// </summary>
    public LinearGradientBrush? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Raised after <see cref="Value" /> changes, with the previous and the new gradient. When a confirmation is
    /// required, the new gradient has become the draft before the event is raised, and
    /// <see cref="AbstractColorPickerView.IsEmptyDraft" /> is set to whether it is <see langword="null" />.
    /// </summary>
    public event EventHandler<GradientColorChangedEventArgs>? GradientValueChanged;
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<GradientColorPickerView, int?> ActivatedStopIndexProperty =
        AvaloniaProperty.RegisterDirect<GradientColorPickerView, int?>(
            nameof(ActivatedStopIndex),
            o => o.ActivatedStopIndex,
            (o, v) => o.ActivatedStopIndex = v);

    private int? _activatedStopIndex;

    internal int? ActivatedStopIndex
    {
        get => _activatedStopIndex;
        set => SetAndRaise(ActivatedStopIndexProperty, ref _activatedStopIndex, value);
    }

    #endregion

    internal LinearGradientBrush? DraftValue { get; private set; }

    internal void ResetDraft(LinearGradientBrush? value)
    {
        EndEdit();
        // An empty committed value still needs an editable stop. The editor fallback
        // is a projection only: the nullable draft remains empty until user input.
        var editorValue = CopyGradient(value) ?? new LinearGradientBrush
        {
#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = new GradientStops { new(Colors.White, 0) }
        };
#pragma warning restore ATOMUIGEN044
        SetCurrentValue(ValueProperty, editorValue);
        SetCurrentValue(ActivatedStopIndexProperty, editorValue.GradientStops.Count > 0 ? 0 : null);
#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
        var activeColor = editorValue.GradientStops.Count > 0 ? editorValue.GradientStops[0].Color : Colors.White;
#pragma warning restore ATOMUIGEN044
        SetCurrentValue(HsvValueProperty, activeColor.ToHsv());
        if (Value != null)
        {
            Value.Opacity = editorValue.Opacity;
            Value.SpreadMethod = editorValue.SpreadMethod;
        }
        DraftValue = value == null ? null : Value;
        SetCurrentValue(IsEmptyDraftProperty, IsNeedConfirm && value == null);
        BeginEdit();
    }

    internal static LinearGradientBrush? CopyGradient(LinearGradientBrush? value)
    {
        if (value == null)
        {
            return null;
        }
        var copy = new LinearGradientBrush
        {
            StartPoint = value.StartPoint,
            EndPoint = value.EndPoint,
            Opacity = value.Opacity,
            SpreadMethod = value.SpreadMethod
        };
        foreach (var stop in value.GradientStops)
        {
            copy.GradientStops.Add(new GradientStop(stop.Color, stop.Offset));
        }
        return copy;
    }

    internal static bool GradientEquals(LinearGradientBrush? left, LinearGradientBrush? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        if (left == null || right == null || left.StartPoint != right.StartPoint ||
            left.EndPoint != right.EndPoint || left.Opacity != right.Opacity ||
            left.SpreadMethod != right.SpreadMethod || left.GradientStops.Count != right.GradientStops.Count)
        {
            return false;
        }
        for (var i = 0; i < left.GradientStops.Count; ++i)
        {
            if (left.GradientStops[i].Color != right.GradientStops[i].Color ||
                left.GradientStops[i].Offset != right.GradientStops[i].Offset)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Calls the base member and then, when <see cref="Value" /> is <see langword="null" />, sets <see cref="Value" />
    /// to <see cref="DefaultValue" />.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Value == null)
        {
            SetCurrentValue(ValueProperty, DefaultValue);
        }
    }

    /// <summary>
    /// Keeps the editor state in step with property changes. The base member is called only while
    /// <c>IgnorePropertyChanged</c> is set. Otherwise a change of
    /// <see cref="AbstractColorPickerView.IsNeedConfirm" /> recomputes
    /// <see cref="AbstractColorPickerView.IsEmptyDraft" />, a <see cref="Value" /> change copies <c>Opacity</c> and
    /// <c>SpreadMethod</c> from the previous brush to the new one while an edit that needs a confirmation is in
    /// progress and reports the change, an <c>HsvValue</c> change refreshes the HSV brushes, and, while the view is
    /// attached to the visual tree, a <see cref="DefaultValue" /> change sets <see cref="Value" /> when it is
    /// <see langword="null" />.
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
            // The track rebuilds stops; brush-level settings belong to the draft,
            // not to the track's default-valued replacement brush.
            if (IsNeedConfirm && IsEditing && change.GetOldValue<LinearGradientBrush?>() is { } previous && Value is { } current)
            {
                current.Opacity = previous.Opacity;
                current.SpreadMethod = previous.SpreadMethod;
            }
            NotifyGradientValueChanged(new GradientColorChangedEventArgs(
                change.GetOldValue<LinearGradientBrush>(),
                change.GetNewValue<LinearGradientBrush>()));
        }
        else if (change.Property == HsvValueProperty)
        {
            ConfigureHsvColorBrushes();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DefaultValueProperty)
            {
                if (Value == null)
                {
                    SetCurrentValue(ValueProperty, DefaultValue);
                }
            }
        }
    }

    /// <summary>
    /// Reports a gradient change by raising <see cref="GradientValueChanged" />. When a confirmation is required,
    /// the new gradient first becomes the draft and <see cref="AbstractColorPickerView.IsEmptyDraft" /> is set to
    /// whether it is <see langword="null" />. Called when <see cref="Value" /> changes.
    /// </summary>
    protected virtual void NotifyGradientValueChanged(GradientColorChangedEventArgs e)
    {
        if (IsNeedConfirm)
        {
            DraftValue = e.NewColor;
            SetCurrentValue(IsEmptyDraftProperty, e.NewColor == null);
        }
        GradientValueChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Called when the clear block named <c>PART_ClearColor</c> raises <c>ClearRequest</c>. It replaces
    /// <see cref="Value" /> with a left-to-right gradient whose only stop is transparent black at offset 0, sets
    /// <see cref="AbstractColorPickerView.HsvValue" /> to a fully transparent color and raises
    /// <see cref="AbstractColorPickerView.ColorValueCleared" />. It then discards the draft and marks the draft empty
    /// when a confirmation is required. The base member is not called.
    /// </summary>
    protected override void NotifyColorClearRequest()
    {
        SetCurrentValue(ValueProperty, new LinearGradientBrush()
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
                new GradientStop(Color.FromArgb(0,0, 0, 0), 0.0)
            }
        });
#pragma warning restore ATOMUIGEN044
        HsvValue = new HsvColor(0.0, 0, 0, 0);
        InvokeColorValueClearedEvent();
        DraftValue = null;
        SetCurrentValue(IsEmptyDraftProperty, IsNeedConfirm);
    }

    internal override void NotifyInputColorEdited()
    {
        DraftValue = Value;
        SetCurrentValue(IsEmptyDraftProperty, false);
    }

    /// <summary>
    /// Called after the user picks a color in the palette. It sets
    /// <see cref="AbstractColorPickerView.HsvValue" /> to the picked color and, when a confirmation is required,
    /// makes the current <see cref="Value" /> the draft and marks the draft as not empty.
    /// </summary>
    protected override void NotifyPaletteColorSelected(Color color)
    {
        SetCurrentValue(HsvValueProperty, color.ToHsv());
        if (IsNeedConfirm)
        {
            NotifyInputColorEdited();
        }
    }
}
