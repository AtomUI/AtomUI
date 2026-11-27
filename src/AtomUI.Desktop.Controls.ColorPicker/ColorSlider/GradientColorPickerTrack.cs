using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class GradientColorPickerTrack : AbstractColorPickerSliderTrack
{
    #region 公共属性定义

    public static readonly StyledProperty<LinearGradientBrush?> GradientValueProperty =
        GradientColorSlider.GradientValueProperty.AddOwner<GradientColorPickerTrack>();

    public static readonly StyledProperty<GradientColorSliderThumb?> ActivatedThumbProperty =
        AvaloniaProperty.Register<GradientColorPickerTrack, GradientColorSliderThumb?>(nameof(ActivatedThumb));

    public LinearGradientBrush? GradientValue
    {
        get => GetValue(GradientValueProperty);
        set => SetValue(GradientValueProperty, value);
    }

    public GradientColorSliderThumb? ActivatedThumb
    {
        get => GetValue(ActivatedThumbProperty);
        set => SetValue(ActivatedThumbProperty, value);
    }

    private bool _ignoringPropertyChanged;

    #endregion

    #region 内部协作 API

    internal List<GradientColorSliderThumb> Thumbs { get; } = new();

    #endregion

    static GradientColorPickerTrack()
    {
        AffectsArrange<GradientColorPickerTrack>(GradientValueProperty, ActivatedThumbProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_ignoringPropertyChanged)
        {
            base.OnPropertyChanged(change);
            return;
        }

        if (change.Property == GradientValueProperty)
        {
            ConfigureThumbs();
        }

        else if (change.Property == ValueProperty)
        {
            SyncGradientValueFromThumbs();
        }
        else if (change.Property == ActivatedThumbProperty)
        {
            foreach (var thumb in Thumbs)
            {
                ConfigureActivatedThumb(thumb, ActivatedThumb == thumb);
            }
        }
    }

    private void ConfigureThumbs()
    {
        if (GradientValue is null || GradientValue.GradientStops.Count == 0)
        {
            Thumbs.Clear();
            LogicalChildren.Clear();
            VisualChildren.Clear();
        }
        else
        {
            var stops = GradientValue.GradientStops;
            if (Thumbs.Count > 0)
            {
                if (stops.Count > Thumbs.Count)
                {
                    var delta = stops.Count - Thumbs.Count;
                    // Add the missing thumbs.
                    for (var i = 0; i < delta; i++)
                    {
                        var thumb = new GradientColorSliderThumb();
                        Thumbs.Add(thumb);
                        LogicalChildren.Add(thumb);
                        VisualChildren.Add(thumb);
                    }
                }
                else if (stops.Count < Thumbs.Count)
                {
                    // Remove the surplus thumbs.
                    var delta = Thumbs.Count - stops.Count;
                    var tobeRemoved = new List<GradientColorSliderThumb>(delta);
                    for (var i = 0; i < delta; i++)
                    {
                        var thumb = Thumbs[i];
                        tobeRemoved.Add(thumb);
                    }

                    foreach (var thumb in tobeRemoved)
                    {
                        Thumbs.Remove(thumb);
                        LogicalChildren.Remove(thumb);
                        VisualChildren.Remove(thumb);
                    }

                    if (ActivatedThumb != null && !Thumbs.Contains(ActivatedThumb))
                    {
                        Debug.Assert(Thumbs.Count > 0);
                        SetCurrentValue(ActivatedThumbProperty, Thumbs[0]);
                    }
                }
            }
            else
            {
                for (var i = 0; i < stops.Count; i++)
                {
                    var thumb = new GradientColorSliderThumb();
                    Thumbs.Add(thumb);
                    LogicalChildren.Add(thumb);
                    VisualChildren.Add(thumb);
                }
            }
        }

        ConfigureThumbsColor();

        InvalidateArrange();
    }

    private void ConfigureThumbsColor()
    {
        if (GradientValue == null)
        {
            return;
        }
        Debug.Assert(Thumbs.Count == GradientValue.GradientStops.Count);
        var gradientStops = GradientValue.GradientStops;
        for (var i = 0; i < gradientStops.Count; i++)
        {
            var thumb = Thumbs[i];
            var gradientStop = gradientStops[i];
            thumb.Color = gradientStop.Color;
            thumb.Value = gradientStop.Offset;
        }
    }

    internal GradientColorSliderThumb? GetThumbAtPressed(PointerPressedEventArgs args)
    {
        foreach (var thumb in Thumbs)
        {
            if (thumb.Bounds.Contains(args.GetPosition(this)))
            {
                return thumb;
            }
        }
        return null;
    }

    internal GradientColorSliderThumb NotifyTrackPressed(PointerPressedEventArgs e)
    {
        {
            var thumb = GetThumbAtPressed(e);
            if (thumb == null)
            {
                var position = e.GetPosition(this);
                var stopOffset = position.X / Bounds.Width;
                var color = GetColorAtPosition(stopOffset);
                thumb = new GradientColorSliderThumb()
                {
                    Value = stopOffset,
                    Color = color
                };
                Thumbs.Add(thumb);
                LogicalChildren.Add(thumb);
                VisualChildren.Add(thumb);
            }

            SetCurrentValue(ActivatedThumbProperty, thumb);
            using var scope = BeginIgnoringPropertyChanged();
            SetCurrentValue(ValueProperty, thumb.Value);
        }
        foreach (var thumb in Thumbs)
        {
            ConfigureActivatedThumb(thumb, ActivatedThumb == thumb);
        }
        InvalidateMeasure();
        Debug.Assert(ActivatedThumb != null);
        return ActivatedThumb;
    }

    internal void SetActiveThumb(int index)
    {
        if (index >= 0 && index < Thumbs.Count)
        {
            SetCurrentValue(ActivatedThumbProperty, Thumbs[index]);
            SetCurrentValue(ValueProperty, Thumbs[index].Value);
        }
    }

    private void ConfigureActivatedThumb(GradientColorSliderThumb thumb, bool isActivated)
    {
        if (isActivated)
        {
            thumb.ZIndex = GradientColorSliderThumb.ActivatedZIndex;
            thumb.IsActivated = true;
        }
        else
        {
            thumb.ZIndex = GradientColorSliderThumb.NormalZIndex;
            thumb.IsActivated = false;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        var desiredSize = new Size(0.0, 0.0);
        foreach (var thumb in Thumbs)
        {
            thumb.Measure(availableSize);
            desiredSize = thumb.DesiredSize;
        }
        return desiredSize;
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        double min = Minimum;
        double range = Math.Max(0.0, Maximum - min);
        double offset = Math.Min(range, ThumbValue - min);
        Density = range / arrangeSize.Width;
        if (ActivatedThumb != null)
        {
            ActivatedThumb.Value = offset;
        }

        foreach (var thumb in Thumbs)
        {
            var offsetX = thumb.Value * arrangeSize.Width - thumb.DesiredSize.Width / 2;
            var bounds = new Rect(new Point(offsetX, 0), new Size(thumb.DesiredSize.Width, arrangeSize.Height));
            thumb.Arrange(bounds);
        }
        return arrangeSize;
    }

    private Color GetColorAtPosition(double position)
    {
        position = Math.Max(0.0, Math.Min(1.0, position));
        if (GradientValue == null || GradientValue.GradientStops.Count == 0)
        {
#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
            return Colors.Black;
#pragma warning restore ATOMUIGEN044
        }

        var colorStops = CopyGradientStopsByOffset(GradientValue.GradientStops);

        if (colorStops.Count == 1)
        {
            return colorStops[0].Color;
        }

        int lowerIndex = 0;
        int upperIndex = colorStops.Count - 1;

        if (position <= colorStops[0].Offset)
        {
            return colorStops[0].Color;
        }

        if (position >= colorStops[upperIndex].Offset)
        {
            return colorStops[upperIndex].Color;
        }

        for (int i = 0; i < colorStops.Count - 1; i++)
        {
            if (position >= colorStops[i].Offset && position <= colorStops[i + 1].Offset)
            {
                lowerIndex = i;
                upperIndex = i + 1;
                break;
            }
        }

        double lowerPos = colorStops[lowerIndex].Offset;
        double upperPos = colorStops[upperIndex].Offset;
        double relativePosition = (position - lowerPos) / (upperPos - lowerPos);

        // Interpolates between the two colors.
        return InterpolateColor(
            colorStops[lowerIndex].Color,
            colorStops[upperIndex].Color,
            relativePosition);
    }

    private Color InterpolateColor(Color startColor, Color endColor, double ratio)
    {
        ratio = Math.Max(0.0, Math.Min(1.0, ratio));

        byte a = (byte)(startColor.A + (endColor.A - startColor.A) * ratio);
        byte r = (byte)(startColor.R + (endColor.R - startColor.R) * ratio);
        byte g = (byte)(startColor.G + (endColor.G - startColor.G) * ratio);
        byte b = (byte)(startColor.B + (endColor.B - startColor.B) * ratio);

        return Color.FromArgb(a, r, g, b);
    }

    internal void NotifyActivateStopColorChanged(HsvColor color)
    {
        if (ActivatedThumb != null)
        {
            ActivatedThumb.Color = color.ToRgb();
            SyncGradientValueFromThumbs();
        }
    }

    private void SyncGradientValueFromThumbs()
    {
        var thumbs = CopyThumbsByValue(Thumbs);
        var newLinearGradients = new LinearGradientBrush
        {
            StartPoint = GradientValue?.StartPoint ?? new RelativePoint(new Point(0, 0.5), RelativeUnit.Relative),
            EndPoint = GradientValue?.EndPoint ?? new RelativePoint(new Point(1, 0.5), RelativeUnit.Relative),
        };
        foreach (var thumb in thumbs)
        {
            newLinearGradients.GradientStops.Add(new GradientStop(thumb.Color, thumb.Value));
        }

        using var scope = BeginIgnoringPropertyChanged();
        SetCurrentValue(GradientValueProperty, newLinearGradients);
    }

    private static List<GradientStop> CopyGradientStopsByOffset(IReadOnlyList<GradientStop> stops)
    {
        var colorStops = new List<GradientStop>(stops.Count);
        for (var i = 0; i < stops.Count; ++i)
        {
            colorStops.Add(stops[i]);
        }

        StableSortGradientStopsByOffset(colorStops);
        return colorStops;
    }

    internal static List<GradientColorSliderThumb> CopyThumbsByValue(IReadOnlyList<GradientColorSliderThumb> thumbs)
    {
        var sortedThumbs = new List<GradientColorSliderThumb>(thumbs.Count);
        for (var i = 0; i < thumbs.Count; ++i)
        {
            sortedThumbs.Add(thumbs[i]);
        }

        StableSortThumbsByValue(sortedThumbs);
        return sortedThumbs;
    }

    private static void StableSortGradientStopsByOffset(List<GradientStop> stops)
    {
        for (var i = 1; i < stops.Count; ++i)
        {
            var item = stops[i];
            var j    = i - 1;
            while (j >= 0 && stops[j].Offset > item.Offset)
            {
                stops[j + 1] = stops[j];
                --j;
            }

            stops[j + 1] = item;
        }
    }

    private static void StableSortThumbsByValue(List<GradientColorSliderThumb> thumbs)
    {
        for (var i = 1; i < thumbs.Count; ++i)
        {
            var item = thumbs[i];
            var j    = i - 1;
            while (j >= 0 && thumbs[j].Value > item.Value)
            {
                thumbs[j + 1] = thumbs[j];
                --j;
            }

            thumbs[j + 1] = item;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (GradientValue != null)
        {
            ConfigureThumbs();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        foreach (var thumb in Thumbs)
        {
            LogicalChildren.Remove(thumb);
            VisualChildren.Remove(thumb);
        }
        Thumbs.Clear();
        SetCurrentValue(ActivatedThumbProperty, null);
    }

    private IgnorePropertyChanged BeginIgnoringPropertyChanged() => new IgnorePropertyChanged(this);

    private readonly struct IgnorePropertyChanged : IDisposable
    {
        private readonly GradientColorPickerTrack _owner;

        public IgnorePropertyChanged(GradientColorPickerTrack owner)
        {
            _owner = owner;
            _owner._ignoringPropertyChanged = true;
        }

        public void Dispose() => _owner._ignoringPropertyChanged = false;
    }

}
