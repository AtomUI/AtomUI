using System.Globalization;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public class SliderTrack : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<double> MinimumProperty =
        RangeBase.MinimumProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<double> MaximumProperty =
        RangeBase.MaximumProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<double> ValueProperty =
        RangeBase.ValueProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<SliderRangeValue> RangeValueProperty =
        AvaloniaProperty.Register<SliderTrack, SliderRangeValue>(nameof(RangeValue),
            coerce: CoerceRangeValue);

    public static readonly StyledProperty<bool> IsRangeModeProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IsRangeMode));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<SliderThumb?> StartSliderThumbProperty =
        AvaloniaProperty.Register<SliderTrack, SliderThumb?>(nameof(StartSliderThumb));

    public static readonly StyledProperty<SliderThumb?> EndSliderThumbProperty =
        AvaloniaProperty.Register<SliderTrack, SliderThumb?>(nameof(EndSliderThumb));

    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IsDirectionReversed));

    public static readonly StyledProperty<bool> IgnoreThumbDragProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IgnoreThumbDrag));

    public static readonly StyledProperty<bool> DeferThumbDragProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(DeferThumbDrag));

    public static readonly StyledProperty<bool> IncludedProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(Included), true);

    public static readonly StyledProperty<IBrush?> TrackBarBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(TrackBarBrush));

    public static readonly StyledProperty<IBrush?> TrackGrooveBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(TrackGrooveBrush));

    public static readonly StyledProperty<AvaloniaList<SliderMark>?> MarksProperty =
        AvaloniaProperty.Register<SliderTrack, AvaloniaList<SliderMark>?>(nameof(Marks));

    public static readonly StyledProperty<double> MarkLabelFontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<FontFamily> MarkLabelFontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<IBrush?> MarkLabelBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(MarkLabelBrush));

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public SliderRangeValue RangeValue
    {
        get => GetValue(RangeValueProperty);
        set => SetValue(RangeValueProperty, value);
    }
    
    public bool IsRangeMode
    {
        get => GetValue(IsRangeModeProperty);
        set => SetValue(IsRangeModeProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public SliderThumb? StartSliderThumb
    {
        get => GetValue(StartSliderThumbProperty);
        set => SetValue(StartSliderThumbProperty, value);
    }

    public SliderThumb? EndSliderThumb
    {
        get => GetValue(EndSliderThumbProperty);
        set => SetValue(EndSliderThumbProperty, value);
    }

    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    public bool IgnoreThumbDrag
    {
        get => GetValue(IgnoreThumbDragProperty);
        set => SetValue(IgnoreThumbDragProperty, value);
    }

    public bool DeferThumbDrag
    {
        get => GetValue(DeferThumbDragProperty);
        set => SetValue(DeferThumbDragProperty, value);
    }

    public bool Included
    {
        get => GetValue(IncludedProperty);
        set => SetValue(IncludedProperty, value);
    }

    public IBrush? TrackBarBrush
    {
        get => GetValue(TrackBarBrushProperty);
        set => SetValue(TrackBarBrushProperty, value);
    }

    public IBrush? TrackGrooveBrush
    {
        get => GetValue(TrackGrooveBrushProperty);
        set => SetValue(TrackGrooveBrushProperty, value);
    }

    public AvaloniaList<SliderMark>? Marks
    {
        get => GetValue(MarksProperty);
        set => SetValue(MarksProperty, value);
    }

    public double MarkLabelFontSize
    {
        get => GetValue(MarkLabelFontSizeProperty);
        set => SetValue(MarkLabelFontSizeProperty, value);
    }

    public FontFamily MarkLabelFontFamily
    {
        get => GetValue(MarkLabelFontFamilyProperty);
        set => SetValue(MarkLabelFontSizeProperty, value);
    }

    public IBrush? MarkLabelBrush
    {
        get => GetValue(MarkLabelBrushProperty);
        set => SetValue(MarkLabelBrushProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> SliderTrackSizeProperty =
        AvaloniaProperty.Register<SliderTrack, double>(nameof(SliderTrackSize));

    internal static readonly StyledProperty<double> SliderRailSizeProperty =
        AvaloniaProperty.Register<SliderTrack, double>(nameof(SliderRailSize));

    internal static readonly StyledProperty<double> SliderMarkSizeProperty =
        AvaloniaProperty.Register<SliderTrack, double>(nameof(SliderMarkSize));

    internal static readonly StyledProperty<IBrush?> MarkBorderBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(MarkBorderBrush));

    internal static readonly StyledProperty<IBrush?> MarkBorderActiveBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(MarkBorderActiveBrush));

    internal static readonly StyledProperty<IBrush?> MarkBackgroundBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(MarkBackgroundBrush));

    internal static readonly StyledProperty<Thickness> MarkBorderThicknessProperty =
        AvaloniaProperty.Register<SliderTrack, Thickness>(nameof(MarkBorderThickness));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SliderTrack>();

    internal double SliderTrackSize
    {
        get => GetValue(SliderTrackSizeProperty);
        set => SetValue(SliderTrackSizeProperty, value);
    }

    internal double SliderRailSize
    {
        get => GetValue(SliderRailSizeProperty);
        set => SetValue(SliderRailSizeProperty, value);
    }

    internal double SliderMarkSize
    {
        get => GetValue(SliderMarkSizeProperty);
        set => SetValue(SliderMarkSizeProperty, value);
    }

    internal IBrush? MarkBorderBrush
    {
        get => GetValue(MarkBorderBrushProperty);
        set => SetValue(MarkBorderBrushProperty, value);
    }

    internal IBrush? MarkBorderActiveBrush
    {
        get => GetValue(MarkBorderActiveBrushProperty);
        set => SetValue(MarkBorderActiveBrushProperty, value);
    }

    internal IBrush? MarkBackgroundBrush
    {
        get => GetValue(MarkBackgroundBrushProperty);
        set => SetValue(MarkBackgroundBrushProperty, value);
    }

    internal Thickness MarkBorderThickness
    {
        get => GetValue(MarkBorderThicknessProperty);
        set => SetValue(MarkBorderThicknessProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    private VectorEventArgs? _deferredThumbDrag;
    private Vector _lastDrag;
    private RenderContextData? _renderContextData;
    
    private double ThumbValue => Value + (_deferredThumbDrag == null
        ? 0
        : ValueFromDistance(_deferredThumbDrag.Vector.X, _deferredThumbDrag.Vector.Y));

    private double ThumbRangeStartValue => RangeValue.StartValue + (_deferredThumbDrag == null
        ? 0
        : ValueFromDistance(_deferredThumbDrag.Vector.X, _deferredThumbDrag.Vector.Y));

    private double ThumbRangeEndValue => RangeValue.EndValue + (_deferredThumbDrag == null
        ? 0
        : ValueFromDistance(_deferredThumbDrag.Vector.X, _deferredThumbDrag.Vector.Y));

    private double ThumbCenterOffset { get; set; }
    private Point StartThumbCenterOffset { get; set; }
    private Point EndThumbCenterOffset { get; set; }
    private double Density { get; set; }
    private IDisposable? _focusProcessDisposable;
    private Size _markLabelSize;

    static SliderTrack()
    {
        StartSliderThumbProperty.Changed.AddClassHandler<SliderTrack>((x, e) => x.ThumbChanged(e));
        EndSliderThumbProperty.Changed.AddClassHandler<SliderTrack>((x, e) => x.ThumbChanged(e));
        AffectsArrange<SliderTrack>(IsDirectionReversedProperty,
            MinimumProperty,
            MaximumProperty,
            ValueProperty,
            RangeValueProperty,
            OrientationProperty,
            IsRangeModeProperty);
        AffectsRender<SliderTrack>(TrackBarBrushProperty,
            TrackGrooveBrushProperty,
            IncludedProperty,
            MarkBorderBrushProperty,
            MarkLabelBrushProperty);
    }

    public SliderTrack()
    {
        UpdatePseudoClasses(Orientation);
    }

    private static SliderRangeValue CoerceRangeValue(AvaloniaObject sender, SliderRangeValue value)
    {
        if (ValidateRangeValue(ref value))
        {
            var startValue = MathUtilities.Clamp(value.StartValue, sender.GetValue(MinimumProperty),
                sender.GetValue(MaximumProperty));
            var endValue = MathUtilities.Clamp(value.EndValue, sender.GetValue(MinimumProperty),
                sender.GetValue(MaximumProperty));
            return new SliderRangeValue
            {
                StartValue = startValue,
                EndValue   = endValue
            };
        }

        return sender.GetValue(RangeValueProperty);
    }

    private static bool ValidateRangeValue(ref SliderRangeValue value)
    {
        return !double.IsInfinity(value.StartValue) && !double.IsNaN(value.StartValue) &&
               !double.IsInfinity(value.EndValue) && !double.IsNaN(value.EndValue);
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(TrackGrooveBrushProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(TrackBarBrushProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(MarkBorderBrushProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    private void HandleRangeModeChanged()
    {
        if (IsRangeMode)
        {
            if (EndSliderThumb is not null)
            {
                EndSliderThumb.IsVisible = true;
            }
        }
        else
        {
            if (EndSliderThumb is not null)
            {
                EndSliderThumb.IsVisible = false;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var inputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
        _focusProcessDisposable = inputManager.Process.Subscribe(args =>
        {
            if (args is RawPointerEventArgs pointerEventArgs)
            {
                var eventType = pointerEventArgs.Type;
                switch (eventType)
                {
                    case RawPointerEventType.LeftButtonDown:
                    case RawPointerEventType.RightButtonDown:
                    case RawPointerEventType.MiddleButtonDown:
                    case RawPointerEventType.XButton1Down:
                    case RawPointerEventType.XButton2Down:
                        HandleGlobalMousePressed(pointerEventArgs.Position);
                        break;
                }
            }
        });
        HandleRangeModeChanged();
        CalculateMaxMarkSize();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _focusProcessDisposable?.Dispose();
    }

    private void HandleGlobalMousePressed(Point point)
    {
        var globalOffset = GetGlobalOffset();
        var trailGlobalBounds = new Rect(globalOffset + _renderContextData!.RailRect.Position,
            _renderContextData.RailRect.Size);
        if (trailGlobalBounds.Contains(point))
        {
            // 点击在轨道上，要不设置值，要不本身就在 Thumb 上，所以不需要处理
            return;
        }

        if (StartSliderThumb is not null && StartSliderThumb.IsVisible)
        {
            HandleThumbFocus(StartSliderThumb, point);
        }

        if (EndSliderThumb is not null && EndSliderThumb.IsVisible)
        {
            HandleThumbFocus(EndSliderThumb, point);
        }
    }

    private Point GetGlobalOffset()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return default;
        }

        return this.TranslatePoint(Bounds.Position, topLevel) ?? default;
    }

    private void HandleThumbFocus(SliderThumb sliderThumb, Point point)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var offset = GetGlobalOffset();

        var thumbGOffset = offset + sliderThumb.Bounds.Position;
        var thumbGBounds = new Rect(thumbGOffset, sliderThumb.Bounds.Size);
        if (!thumbGBounds.Contains(point) && sliderThumb.IsFocused)
        {
            topLevel.FocusManager?.ClearFocus();
        }
    }
    
    public virtual double ValueFromDistance(double horizontal, double vertical)
    {
        double scale = IsDirectionReversed ? -1 : 1;

        if (Orientation == Orientation.Horizontal)
        {
            return scale * horizontal * Density;
        }

        // Increases in y cause decreases in Sliders value
        return -1 * scale * vertical * Density;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var targetWidth  = 0d;
        var targetHeight = 0d;
        if (StartSliderThumb is not null)
        {
            StartSliderThumb.Measure(availableSize);
        }

        if (EndSliderThumb is not null)
        {
            EndSliderThumb.Measure(availableSize);
        }

        // TODO 暂时先不加 Margin 试试
        if (Orientation == Orientation.Horizontal)
        {
            targetWidth  = Math.Max(0, SliderTrackSize);
            targetHeight = SliderTrackSize + _markLabelSize.Height;
        }
        else
        {
            targetHeight = Math.Max(0, SliderTrackSize);
            targetWidth  = SliderTrackSize + _markLabelSize.Width;
        }

        targetWidth  += Padding.Left + Padding.Right;
        targetHeight += Padding.Top + Padding.Bottom;

        return new Size(targetWidth, targetHeight);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var isVertical = Orientation == Orientation.Vertical;

        ComputeDensity(arrangeSize, isVertical);
        
        var thumbCenterPoints = CalculateThumbCenterOffset(arrangeSize);
        StartThumbCenterOffset = thumbCenterPoints.Item1;
        EndThumbCenterOffset   = thumbCenterPoints.Item2;
        var hasMarks = Marks?.Count > 0;

        if (IsRangeMode)
        {
            if (StartSliderThumb != null)
            {
                var offset = StartThumbCenterOffset;

                if (Orientation == Orientation.Horizontal)
                {
                    offset -= new Point(StartSliderThumb.DesiredSize.Width / 2,
                        hasMarks
                            ? Padding.Top
                            : StartSliderThumb.DesiredSize.Height / 2);
                }
                else
                {
                    offset -= new Point(hasMarks
                            ? Padding.Left
                            : StartSliderThumb.DesiredSize.Width / 2,
                        StartSliderThumb.DesiredSize.Height / 2);
                }

                var bounds = new Rect(offset, StartSliderThumb.DesiredSize);
                var adjust = CalculateThumbAdjustment(StartSliderThumb, bounds);
                StartSliderThumb.Arrange(bounds);
                StartSliderThumb.AdjustDrag(adjust);
            }

            if (EndSliderThumb != null)
            {
                var offset = EndThumbCenterOffset;

                if (Orientation == Orientation.Horizontal)
                {
                    offset -= new Point(EndSliderThumb.DesiredSize.Width / 2,
                        hasMarks
                            ? Padding.Top
                            : EndSliderThumb.DesiredSize.Height / 2);
                }
                else
                {
                    offset -= new Point(hasMarks
                            ? Padding.Left
                            : EndSliderThumb.DesiredSize.Width / 2,
                        EndSliderThumb.DesiredSize.Height / 2);
                }

                var bounds = new Rect(offset, EndSliderThumb.DesiredSize);
                var adjust = CalculateThumbAdjustment(EndSliderThumb, bounds);
                EndSliderThumb.Arrange(bounds);
                EndSliderThumb.AdjustDrag(adjust);
            }
        }
        else
        {
            if (StartSliderThumb != null)
            {
                var offset = EndThumbCenterOffset;
                if (Orientation == Orientation.Horizontal)
                {
                    offset -= new Point(StartSliderThumb.DesiredSize.Width / 2,
                        hasMarks
                            ? Padding.Top
                            : StartSliderThumb.DesiredSize.Height / 2);
                }
                else
                {
                    offset -= new Point(hasMarks
                            ? Padding.Left
                            : StartSliderThumb.DesiredSize.Width / 2,
                        StartSliderThumb.DesiredSize.Height / 2);
                }

                var bounds = new Rect(offset, StartSliderThumb.DesiredSize);
                var adjust = CalculateThumbAdjustment(StartSliderThumb, bounds);
                StartSliderThumb.Arrange(bounds);
                StartSliderThumb.AdjustDrag(adjust);
            }
        }

        _lastDrag = default;
        return arrangeSize;
    }

    private (Point, Point) CalculateThumbCenterOffset(Size arrangeSize)
    {
        CalculateThumbValuePivotOffset(arrangeSize,
            Orientation == Orientation.Vertical,
            out var startThumbPivotOffset,
            out var endThumbPivotOffset);
        if (Orientation == Orientation.Horizontal)
        {
            var offsetY = Marks?.Count > 0
                ? Padding.Top
                : arrangeSize.Height / 2;
            return (new Point(startThumbPivotOffset, offsetY), new Point(endThumbPivotOffset, offsetY));
        }

        var offsetX = Marks?.Count > 0
            ? Padding.Left
            : arrangeSize.Width / 2;
        return (new Point(offsetX, startThumbPivotOffset), new Point(offsetX, endThumbPivotOffset));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<Orientation>());
        }
        else if (change.Property == DeferThumbDragProperty)
        {
            if (!change.GetNewValue<bool>())
            {
                ApplyDeferredThumbDrag();
            }
        }
        else if (change.Property == IsRangeModeProperty)
        {
            HandleRangeModeChanged();
        }

        if (this.IsAttachedToLogicalTree())
        {
            if (change.Property == IsEnabledProperty)
            {
                CalculateMaxMarkSize(true);
            }
            else if (change.Property == MarksProperty)
            {
                CalculateMaxMarkSize();
            }
           
        }

        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    private Vector CalculateThumbAdjustment(SliderThumb thumb, Rect newThumbBounds)
    {
        var thumbDelta = newThumbBounds.Position - thumb.Bounds.Position;
        return _lastDrag - thumbDelta;
    }

    // TODO 需要重构，这里对圆心的半径做了处理，理论上不应该在这里做
    private void CalculateThumbValuePivotOffset(Size arrangeSize,
                                                bool isVertical,
                                                out double startThumbOffset,
                                                out double endThumbOffset)
    {
        var min       = Minimum;
        var range     = Math.Max(0.0, Maximum - min);
        var thumbSize = StartSliderThumb!.Width;
        var totalSize = arrangeSize.Width;
        var factor    = 1;
        if (isVertical)
        {
            totalSize = arrangeSize.Height;
            factor    = -1;
        }

        totalSize -= thumbSize;
        if (IsRangeMode)
        {
            var startRatio = Math.Min(range, ThumbRangeStartValue - min) / range;
            var endRatio   = Math.Min(range, ThumbRangeEndValue - min) / range;

            startThumbOffset = totalSize * startRatio;
            endThumbOffset   = totalSize * endRatio;
        }
        else
        {
            startThumbOffset = 0d;
            var ratio = Math.Min(range, ThumbValue - min) / range;
            endThumbOffset = totalSize * ratio;
        }

        startThumbOffset += factor * thumbSize / 2;
        endThumbOffset   += factor * thumbSize / 2;
        if (isVertical)
        {
            startThumbOffset = totalSize - startThumbOffset;
            endThumbOffset   = totalSize - endThumbOffset;
        }
    }

    // 计算 value 对应的进度条上的坐标
    private double CalculateMarkPivotOffset(Size arrangeSize, bool isVertical, double value)
    {
        var min       = Minimum;
        var range     = Math.Max(0.0, Maximum - min);
        var totalSize = arrangeSize.Width;
        if (isVertical)
        {
            totalSize = arrangeSize.Height;
        }

        var ratio  = Math.Min(range, value - min) / range;
        var offset = totalSize * ratio;

        if (isVertical)
        {
            offset = totalSize - offset;
        }

        return offset;
    }

    private void ComputeDensity(Size arrangeSize, bool isVertical)
    {
        var min   = Minimum;
        var range = Math.Max(0.0, Maximum - min);

        double trackLength;
        // Compute thumb size
        if (isVertical)
        {
            trackLength = arrangeSize.Height;
        }
        else
        {
            trackLength = arrangeSize.Width;
        }

        var remainingTrackLength = trackLength;
        Density = range / remainingTrackLength;
    }

    private void ThumbChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var oldThumb = (SliderThumb?)change.OldValue;
        var newThumb = (SliderThumb?)change.NewValue;

        if (oldThumb != null)
        {
            oldThumb.DragDelta     -= ThumbDragged;
            oldThumb.DragCompleted -= ThumbDragCompleted;
            LogicalChildren.Remove(oldThumb);
            VisualChildren.Remove(oldThumb);
        }

        if (newThumb != null)
        {
            newThumb.DragDelta     += ThumbDragged;
            newThumb.DragCompleted += ThumbDragCompleted;
            LogicalChildren.Add(newThumb);
            VisualChildren.Add(newThumb);
        }
    }

    private void ThumbDragged(object? sender, VectorEventArgs e)
    {
        if (IgnoreThumbDrag)
        {
            return;
        }

        if (DeferThumbDrag)
        {
            _deferredThumbDrag = e;
            InvalidateArrange();
        }
        else
        {
            ApplyThumbDrag(e);
        }
    }

    private void ApplyThumbDrag(VectorEventArgs e)
    {
        var delta    = ValueFromDistance(e.Vector.X, e.Vector.Y);
        var factor   = e.Vector / delta;
        var oldValue = Value;

        SetCurrentValue(ValueProperty, MathUtilities.Clamp(
            Value + delta,
            Minimum,
            Maximum));

        // Record the part of the drag that actually had effect as the last drag delta.
        // Due to clamping, we need to compare the two values instead of using the drag delta.
        _lastDrag = (Value - oldValue) * factor;
    }

    private void ThumbDragCompleted(object? sender, EventArgs e)
    {
        ApplyDeferredThumbDrag();
    }

    private void ApplyDeferredThumbDrag()
    {
        if (_deferredThumbDrag != null)
        {
            ApplyThumbDrag(_deferredThumbDrag);
            _deferredThumbDrag = null;
        }
    }

    private void UpdatePseudoClasses(Orientation o)
    {
        PseudoClasses.Set(StdPseudoClass.Vertical, o == Orientation.Vertical);
        PseudoClasses.Set(StdPseudoClass.Horizontal, o == Orientation.Horizontal);
    }

    private void CalculateMaxMarkSize(bool force = false)
    {
        if (_markLabelSize == default || force)
        {
            var targetWidth  = 0d;
            var targetHeight = 0d;
            if (Marks is not null)
            {
                foreach (var mark in Marks)
                {
                    var markTextSize = TextUtils.CalculateTextSize(mark.Label,
                        MarkLabelFontSize,
                        MarkLabelFontFamily,
                        mark.LabelFontStyle,
                        mark.LabelFontWeight);
                    mark.LabelSize = markTextSize;
                    targetWidth    = Math.Max(targetWidth, markTextSize.Width);
                    targetHeight   = Math.Max(targetHeight, markTextSize.Height);

                    var typeface = new Typeface(MarkLabelFontFamily, mark.LabelFontStyle, mark.LabelFontWeight);
                    var formattedText = new FormattedText(mark.Label, CultureInfo.CurrentUICulture,
                        GetFlowDirection(this),
                        typeface, 1, mark.LabelBrush is not null && IsEnabled ? mark.LabelBrush : MarkLabelBrush);
                    formattedText.SetFontSize(MarkLabelFontSize);
                    formattedText.TextAlignment = TextAlignment.Left;
                    mark.FormattedText          = formattedText;
                }
            }

            _markLabelSize = new Size(targetWidth, targetHeight);
        }
    }

    internal SliderMark? GetMarkForPosition(Point point)
    {
        if (Marks is not null)
        {
            if (_renderContextData!.MarkTextRects is not null)
            {
                var entries = _renderContextData.MarkTextRects!;
                for (var i = 0; i < entries.Count; i++)
                {
                    var textEntry = entries[i];
                    if (textEntry.Item1.Contains(point))
                    {
                        return Marks[i];
                    }
                }
            }
        }

        return null;
    }

    private void PrepareRenderInfo()
    {
        _renderContextData = new RenderContextData();
        CalculateThumbValuePivotOffset(Bounds.Size, Orientation == Orientation.Vertical,
            out var startThumbPivotOffset,
            out var endThumbPivotOffset);
        var thumbSize = StartSliderThumb!.DesiredSize.Width;
        if (Orientation == Orientation.Horizontal)
        {
            {
                // 计算轨道位置
                var offsetX = thumbSize / 2;
                var offsetY = Marks?.Count > 0
                    ? Math.Max(Padding.Top, (thumbSize - SliderRailSize) / 2)
                    : (Bounds.Height - SliderRailSize) / 2;
                _renderContextData.RailRect = new Rect(new Point(offsetX, offsetY),
                    new Size(Bounds.Width - thumbSize, SliderRailSize));
            }
            {
                // 计算 range bar rect
                var offsetY = Marks?.Count > 0
                    ? Math.Max(Padding.Top, (thumbSize - SliderRailSize) / 2)
                    : (Bounds.Height - SliderRailSize) / 2;
                _renderContextData.TrackRangeRect = new Rect(
                    new Point(Math.Min(startThumbPivotOffset, endThumbPivotOffset), offsetY),
                    new Size(Math.Abs(endThumbPivotOffset - startThumbPivotOffset), SliderRailSize));
            }

            // 计算 mark 的位置
            {
                if (Marks?.Count > 0)
                {
                    _renderContextData.MarkRects     = new List<(Rect, int, bool)>();
                    _renderContextData.MarkTextRects = new List<(Rect, int, bool, FormattedText)>();
                    var railRect     = _renderContextData.RailRect;
                    var railCenterY  = railRect.Center.Y;
                    var markIncluded = false;
                    for (var i = 0; i < Marks.Count; i++)
                    {
                        var mark = Marks[i];
                        if (Included)
                        {
                            if (IsRangeMode)
                            {
                                markIncluded = MathUtils.GreaterThanOrClose(mark.Value, RangeValue.StartValue) &&
                                               MathUtils.LessThanOrClose(mark.Value, RangeValue.EndValue);
                            }
                            else
                            {
                                markIncluded = MathUtils.LessThanOrClose(mark.Value, Value);
                            }
                        }

                        var offsetX = railRect.X + CalculateMarkPivotOffset(railRect.Size,
                            Orientation == Orientation.Vertical, mark.Value);
                        var offsetY = railCenterY;
                        // 将圆心放到合适的坐标
                        offsetX -= SliderMarkSize / 2;
                        offsetY -= SliderMarkSize / 2;

                        var markRect = new Rect(new Point(offsetX, offsetY), new Size(SliderMarkSize, SliderMarkSize));
                        _renderContextData.MarkRects.Add((markRect, i, markIncluded));

                        var textOffsetX = offsetX - mark.LabelSize.Width / 3;
                        var textOffsetY = railCenterY + thumbSize / 2;

                        if (i == Marks.Count - 1)
                        {
                            textOffsetX -= Padding.Right; // 不知道为啥会出去一点点
                        }

                        var textRect = new Rect(new Point(textOffsetX, textOffsetY), mark.LabelSize);
                        _renderContextData.MarkTextRects.Add((textRect, i, markIncluded, mark.FormattedText!));
                    }
                }
            }
        }
        else
        {
            {
                // 计算轨道位置
                var offsetX = Marks?.Count > 0
                    ? Math.Max(Padding.Left, (thumbSize - SliderRailSize) / 2)
                    : (Bounds.Width - SliderRailSize) / 2;
                var offsetY = thumbSize / 2;
                _renderContextData.RailRect = new Rect(new Point(offsetX, offsetY),
                    new Size(SliderRailSize, Bounds.Height - thumbSize));
            }
            {
                // 计算 range bar rect
                var offsetX = Marks?.Count > 0
                    ? Math.Max(Padding.Left, (thumbSize - SliderRailSize) / 2)
                    : (Bounds.Width - SliderRailSize) / 2;
                _renderContextData.TrackRangeRect = new Rect(
                    new Point(offsetX, Math.Min(startThumbPivotOffset, endThumbPivotOffset)),
                    new Size(SliderRailSize, Math.Abs(endThumbPivotOffset - startThumbPivotOffset)));
            }

            // 计算 mark 的位置
            {
                if (Marks?.Count > 0)
                {
                    _renderContextData.MarkRects     = new List<(Rect, int, bool)>();
                    _renderContextData.MarkTextRects = new List<(Rect, int, bool, FormattedText)>();
                    var railRect     = _renderContextData.RailRect;
                    var railCenterX  = railRect.Center.X;
                    var markIncluded = false;
                    for (var i = 0; i < Marks.Count; i++)
                    {
                        var mark = Marks[i];
                        if (Included)
                        {
                            if (IsRangeMode)
                            {
                                markIncluded = MathUtils.GreaterThanOrClose(mark.Value, RangeValue.StartValue) &&
                                               MathUtils.LessThanOrClose(mark.Value, RangeValue.EndValue);
                            }
                            else
                            {
                                markIncluded = MathUtils.LessThanOrClose(mark.Value, Value);
                            }
                        }

                        var offsetX = railCenterX;
                        var offsetY = railRect.Y + CalculateMarkPivotOffset(railRect.Size,
                            Orientation == Orientation.Vertical, mark.Value);

                        // 将圆心放到合适的坐标
                        offsetX -= SliderMarkSize / 2;
                        offsetY -= SliderMarkSize / 2;

                        var markRect = new Rect(new Point(offsetX, offsetY), new Size(SliderMarkSize, SliderMarkSize));
                        _renderContextData.MarkRects.Add((markRect, i, markIncluded));

                        var textOffsetX = railCenterX + thumbSize / 2;
                        var textOffsetY = offsetY;
                        if (i == 0)
                        {
                            textOffsetY -= Padding.Bottom;
                        }
                        else
                        {
                            textOffsetY -= mark.LabelSize.Height / 2;
                        }

                        var textRect = new Rect(new Point(textOffsetX, textOffsetY), mark.LabelSize);
                        _renderContextData.MarkTextRects.Add((textRect, i, markIncluded, mark.FormattedText!));
                    }
                }
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        PrepareRenderInfo();
        DrawGroove(context);
        DrawTrackBar(context);
        DrawMark(context);
    }

    private void DrawGroove(DrawingContext context)
    {
        context.DrawPilledRect(TrackGrooveBrush, null, _renderContextData!.RailRect, Orientation);
    }

    private void DrawMark(DrawingContext context)
    {
        if (_renderContextData?.MarkRects is not null)
        {
            foreach (var markRectEntry in _renderContextData.MarkRects)
            {
                var centerPos = markRectEntry.Item1.Center;
                var radius    = SliderMarkSize / 2;
                var circlePen = new Pen(markRectEntry.Item3 ? MarkBorderActiveBrush : MarkBorderBrush,
                    MarkBorderThickness.Left);
                context.DrawEllipse(MarkBackgroundBrush, circlePen, centerPos, radius, radius);
            }
        }

        // 绘制文字
        if (_renderContextData?.MarkTextRects is not null)
        {
            foreach (var markTextRectEntry in _renderContextData.MarkTextRects)
            {
                var pos = markTextRectEntry.Item1.Position;
                context.DrawText(markTextRectEntry.Item4, pos);
            }
        }
    }

    private void DrawTrackBar(DrawingContext context)
    {
        if (Included)
        {
            context.DrawPilledRect(TrackBarBrush, null, _renderContextData!.TrackRangeRect, Orientation);
        }
    }

    // 跟渲染相关的数据
    private class RenderContextData
    {
        public Rect RailRect { get; set; }
        public Rect TrackRangeRect { get; set; }
        public List<(Rect, int, bool)>? MarkRects { get; set; }
        public List<(Rect, int, bool, FormattedText)>? MarkTextRects { get; set; }
    }
}