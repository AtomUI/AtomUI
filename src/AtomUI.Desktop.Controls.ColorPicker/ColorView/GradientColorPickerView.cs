using Avalonia;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class GradientColorPickerView : AbstractColorPickerView
{
    #region 公共属性定义
    public static readonly StyledProperty<LinearGradientBrush?> DefaultValueProperty =
        AvaloniaProperty.Register<GradientColorPickerView, LinearGradientBrush?>(nameof(DefaultValue));

    public static readonly StyledProperty<LinearGradientBrush?> ValueProperty =
        AvaloniaProperty.Register<GradientColorPickerView, LinearGradientBrush?>(nameof(Value));

    public LinearGradientBrush? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public LinearGradientBrush? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    #endregion

    #region 公共事件定义
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
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = new GradientStops { new(Colors.White, 0) }
        };
        SetCurrentValue(ValueProperty, editorValue);
        SetCurrentValue(ActivatedStopIndexProperty, editorValue.GradientStops.Count > 0 ? 0 : null);
        var activeColor = editorValue.GradientStops.Count > 0 ? editorValue.GradientStops[0].Color : Colors.White;
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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Value == null)
        {
            SetCurrentValue(ValueProperty, DefaultValue);
        }
    }

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

    protected virtual void NotifyGradientValueChanged(GradientColorChangedEventArgs e)
    {
        if (IsNeedConfirm)
        {
            DraftValue = e.NewColor;
            SetCurrentValue(IsEmptyDraftProperty, e.NewColor == null);
        }
        GradientValueChanged?.Invoke(this, e);
    }

    protected override void NotifyColorClearRequest()
    {
        SetCurrentValue(ValueProperty, new LinearGradientBrush()
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
                new GradientStop(Color.FromArgb(0,0, 0, 0), 0.0)
            }
        });
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

    protected override void NotifyPaletteColorSelected(Color color)
    {
        SetCurrentValue(HsvValueProperty, color.ToHsv());
        if (IsNeedConfirm)
        {
            NotifyInputColorEdited();
        }
    }
}
