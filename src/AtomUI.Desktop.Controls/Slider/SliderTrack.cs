using System.Globalization;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Media;
using AtomUI.Reflection;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public class SliderTrack : TemplatedControl, ICustomHitTest
{
    /// <summary>
    /// SliderTrack no longer self-draws its rail/track visuals, so it has no draw
    /// list for the compositor to hit test against. Report the whole bounds as the
    /// hit target so pointer input still routes through the track exactly as it did
    /// when the rail was self-drawn.
    /// </summary>
    public bool HitTest(Point point) => new Rect(Bounds.Size).Contains(point);

    #region 公共属性定义

    public static readonly StyledProperty<double> MinimumProperty =
        RangeBase.MinimumProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<double> MaximumProperty =
        RangeBase.MaximumProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<double> ValueProperty =
        RangeBase.ValueProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<IReadOnlyList<double>?> RangeValuesProperty =
        AvaloniaProperty.Register<SliderTrack, IReadOnlyList<double>?>(
            nameof(RangeValues),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true,
            coerce: CoerceRangeValues);

    public static readonly StyledProperty<IReadOnlyList<bool>?> DisabledHandlesProperty =
        AvaloniaProperty.Register<SliderTrack, IReadOnlyList<bool>?>(nameof(DisabledHandles));

    public static readonly StyledProperty<bool> IsDraggableTrackProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IsDraggableTrack));

    public static readonly StyledProperty<bool> IsRangeModeProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IsRangeMode));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<SliderTrack>();

    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IsDirectionReversed));

    public static readonly StyledProperty<bool> IgnoreThumbDragProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IgnoreThumbDrag));

    public static readonly StyledProperty<bool> DeferThumbDragProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(DeferThumbDrag));

    public static readonly StyledProperty<bool> IsIncludedProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IsIncluded), true);

    public static readonly StyledProperty<IBrush?> TrackBarBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(TrackBarBrush));

    public static readonly StyledProperty<IBrush?> TracksBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(TracksBrush));

    public static readonly StyledProperty<IBrush?> TrackGrooveBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(TrackGrooveBrush));

    public static readonly StyledProperty<List<SliderMark>?> MarksProperty =
        AvaloniaProperty.Register<SliderTrack, List<SliderMark>?>(nameof(Marks));

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

    public IReadOnlyList<double>? RangeValues
    {
        get => GetValue(RangeValuesProperty);
        set => SetValue(RangeValuesProperty, value);
    }

    public IReadOnlyList<bool>? DisabledHandles
    {
        get => GetValue(DisabledHandlesProperty);
        set => SetValue(DisabledHandlesProperty, value);
    }

    public bool IsDraggableTrack
    {
        get => GetValue(IsDraggableTrackProperty);
        set => SetValue(IsDraggableTrackProperty, value);
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

    public bool IsIncluded
    {
        get => GetValue(IsIncludedProperty);
        set => SetValue(IsIncludedProperty, value);
    }

    public IBrush? TrackBarBrush
    {
        get => GetValue(TrackBarBrushProperty);
        set => SetValue(TrackBarBrushProperty, value);
    }

    public IBrush? TracksBrush
    {
        get => GetValue(TracksBrushProperty);
        set => SetValue(TracksBrushProperty, value);
    }

    public IBrush? TrackGrooveBrush
    {
        get => GetValue(TrackGrooveBrushProperty);
        set => SetValue(TrackGrooveBrushProperty, value);
    }

    public List<SliderMark>? Marks
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
        set => SetValue(MarkLabelFontFamilyProperty, value);
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

    #region 内部协作 API

    internal IReadOnlyList<SliderThumb> Thumbs => _thumbs;

    internal event EventHandler? ThumbsChanged;

    internal IReadOnlyList<double> EffectiveRangeValues => IsRangeMode
        ? RangeValues ?? [Minimum, Minimum]
        : [Value];

    internal SliderThumb? FocusedThumb => _thumbs.FirstOrDefault(thumb => thumb.IsFocused);

    internal double FocusedThumbValue => FocusedThumb is { } thumb ? GetThumbValue(thumb.HandleIndex) : Value;

    internal double GetThumbValue(int index)
    {
        var values = EffectiveRangeValues;
        return index >= 0 && index < values.Count ? values[index] : Value;
    }

    internal double ValueFromPoint(Point point)
    {
        var railRect = GetRailRect(Bounds.Size);
        double ratio;
        if (Orientation == Orientation.Horizontal)
        {
            ratio = railRect.Width <= 0 ? 0 : (point.X - railRect.X) / railRect.Width;
        }
        else
        {
            ratio = railRect.Height <= 0 ? 0 : 1 - (point.Y - railRect.Y) / railRect.Height;
        }

        if (IsDirectionReversed)
        {
            ratio = 1 - ratio;
        }

        return SliderRangeMath.RatioToValue(ratio, Minimum, Maximum);
    }

    public virtual double ValueFromDistance(double horizontal, double vertical)
    {
        var railRect = GetRailRect(Bounds.Size);
        var range = Maximum - Minimum;
        if (Orientation == Orientation.Horizontal)
        {
            var scale = IsDirectionReversed ? -1 : 1;
            return railRect.Width <= 0 ? 0 : scale * horizontal / railRect.Width * range;
        }

        var verticalScale = IsDirectionReversed ? 1 : -1;
        return railRect.Height <= 0 ? 0 : verticalScale * vertical / railRect.Height * range;
    }

    internal SliderMark? GetMarkForPosition(Point point)
    {
        if (Marks is not null && _markTextRects is not null)
        {
            var entries = _markTextRects;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Item1.Contains(point))
                {
                    return Marks[i];
                }
            }
        }

        return null;
    }

    internal bool CanDragRangeTrackAt(Point point)
    {
        var values = EffectiveRangeValues;
        return IsRangeMode &&
               IsDraggableTrack &&
               values.Count >= 2 &&
               !SliderRangeMath.HasDisabledHandle(values.Count, DisabledHandles) &&
               _thumbs.All(thumb => !thumb.Bounds.Contains(point)) &&
               _trackRangeRect.Contains(point);
    }

    #endregion

    private readonly List<SliderThumb> _thumbs = [];
    private readonly List<(Border Element, IDisposable Binding)> _trackSegmentElements = [];
    private VectorEventArgs? _deferredThumbDrag;
    private Vector _lastDrag;
    private Border? _railElement;
    private Border? _tracksElement;
    private SliderMarksElement? _marksElement;
    private Rect _railRect;
    private Rect _trackRangeRect;
    private List<(Rect, int, bool, FormattedText)>? _markTextRects;
    private IDisposable? _focusProcessDisposable;
    private Size _markLabelSize;

    static SliderTrack()
    {
        AffectsMeasure<SliderTrack>(MarksProperty,
            MarkLabelFontSizeProperty,
            MarkLabelFontFamilyProperty,
            RangeValuesProperty,
            IsRangeModeProperty);
        AffectsArrange<SliderTrack>(IsDirectionReversedProperty,
            MinimumProperty,
            MaximumProperty,
            ValueProperty,
            RangeValuesProperty,
            DisabledHandlesProperty,
            OrientationProperty,
            IsRangeModeProperty);
    }

    public SliderTrack()
    {
        UpdatePseudoClasses(Orientation);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var inputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
        _focusProcessDisposable = inputManager.Process.Subscribe(args =>
        {
            if (args is RawPointerEventArgs pointerEventArgs &&
                this.TryGetInputPosition(pointerEventArgs, out var position))
            {
                var eventType = pointerEventArgs.Type;
                switch (eventType)
                {
                    case RawPointerEventType.LeftButtonDown:
                    case RawPointerEventType.RightButtonDown:
                    case RawPointerEventType.MiddleButtonDown:
                    case RawPointerEventType.XButton1Down:
                    case RawPointerEventType.XButton2Down:
                        HandleGlobalMousePressed(position);
                        break;
                }
            }
        });
        EnsureThumbs();
        CalculateMaxMarkSize();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _focusProcessDisposable?.Dispose();
        _focusProcessDisposable = null;
        ClearThumbs();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureThumbs();
        foreach (var thumb in _thumbs)
        {
            thumb.Measure(availableSize);
        }

        var targetWidth  = 0d;
        var targetHeight = 0d;
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
        EnsureThumbs();
        var values = EffectiveRangeValues;

        for (var i = 0; i < _thumbs.Count; i++)
        {
            var thumb = _thumbs[i];
            var center = ValueToCenterPoint(arrangeSize, values[i]);
            var offset = center - new Point(thumb.DesiredSize.Width / 2, thumb.DesiredSize.Height / 2);

            var bounds = new Rect(offset, thumb.DesiredSize);
            var adjust = CalculateThumbAdjustment(thumb, bounds);
            thumb.Arrange(bounds);
            thumb.AdjustDrag(adjust);
        }

        ArrangeTrackElements(arrangeSize, values);

        _lastDrag = default;
        return arrangeSize;
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
        else if (change.Property == IsMotionEnabledProperty)
        {
            var isMotionEnabled = change.GetNewValue<bool>();
            foreach (var thumb in _thumbs)
            {
                thumb.SetCurrentValue(SliderThumb.IsMotionEnabledProperty, isMotionEnabled);
            }
        }

        if (change.Property == RangeValuesProperty ||
            change.Property == IsRangeModeProperty ||
            change.Property == DisabledHandlesProperty)
        {
            EnsureThumbs();
        }
        else if (change.Property == IsIncludedProperty)
        {
            UpdatePartVisibility();
            PushMarkRenderInfo(GetRailRect(Bounds.Size), EffectiveRangeValues, Bounds.Size);
        }

        if (IsMarkTextProperty(change.Property))
        {
            RefreshMarkTextMetrics();
        }
    }

    private static IReadOnlyList<double>? CoerceRangeValues(AvaloniaObject sender, IReadOnlyList<double>? values)
    {
        var normalized = SliderRangeMath.NormalizeRangeValues(
            values,
            sender.GetValue(MinimumProperty),
            sender.GetValue(MaximumProperty));
        return normalized.Count == 0 ? sender.GetValue(RangeValuesProperty) : normalized;
    }

    private void EnsureThumbs()
    {
        EnsureTrackElements();
        SyncTrackSegmentElements();

        var handleCount = IsRangeMode ? EffectiveRangeValues.Count : 1;
        while (_thumbs.Count < handleCount)
        {
            AddThumb(_thumbs.Count);
        }

        while (_thumbs.Count > handleCount)
        {
            RemoveThumb(_thumbs[^1]);
        }

        for (var i = 0; i < _thumbs.Count; i++)
        {
            var thumb = _thumbs[i];
            thumb.HandleIndex = i;
            thumb.IsEnabled = IsEnabled && !SliderRangeMath.IsHandleDisabled(DisabledHandles, i);
        }
    }

    private void AddThumb(int index)
    {
        var thumb = new SliderThumb
        {
            HandleIndex = index,
            IsMotionEnabled = IsMotionEnabled
        };
        thumb.SetTemplatedParent(TemplatedParent ?? this);
        thumb.Classes.Add(SliderSemanticParts.HandleClass);
        ToolTip.SetShowDelay(thumb, 20);
        thumb.DragDelta += ThumbDragged;
        thumb.DragCompleted += ThumbDragCompleted;
        _thumbs.Add(thumb);
        LogicalChildren.Add(thumb);
        VisualChildren.Add(thumb);
        ThumbsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RemoveThumb(SliderThumb thumb)
    {
        thumb.DragDelta -= ThumbDragged;
        thumb.DragCompleted -= ThumbDragCompleted;
        LogicalChildren.Remove(thumb);
        VisualChildren.Remove(thumb);
        thumb.SetTemplatedParent(null);
        _thumbs.Remove(thumb);
        ThumbsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ClearThumbs()
    {
        foreach (var thumb in _thumbs.ToArray())
        {
            RemoveThumb(thumb);
        }
    }

    private void EnsureTrackElements()
    {
        if (_railElement is not null)
        {
            return;
        }

        _railElement = CreatePartBorder(SliderSemanticParts.RailClass, isVisible: true);
        BindUtils.RelayBind(this, TrackGrooveBrushProperty, _railElement, Border.BackgroundProperty,
            BindingMode.OneWay, BindingPriority.Template);
        LogicalChildren.Add(_railElement);
        VisualChildren.Add(_railElement);

        _tracksElement = CreatePartBorder(SliderSemanticParts.TracksClass, IsIncluded);
        BindUtils.RelayBind(this, TracksBrushProperty, _tracksElement, Border.BackgroundProperty,
            BindingMode.OneWay, BindingPriority.Template);
        LogicalChildren.Add(_tracksElement);
        VisualChildren.Add(_tracksElement);

        _marksElement = new SliderMarksElement
        {
            IsHitTestVisible = false
        };
        _marksElement.SetTemplatedParent(TemplatedParent ?? this);
        BindUtils.RelayBind(this, MarkBorderBrushProperty, _marksElement,
            SliderMarksElement.MarkBorderBrushProperty, BindingMode.OneWay, BindingPriority.Template);
        BindUtils.RelayBind(this, MarkBorderActiveBrushProperty, _marksElement,
            SliderMarksElement.MarkBorderActiveBrushProperty, BindingMode.OneWay, BindingPriority.Template);
        BindUtils.RelayBind(this, MarkBackgroundBrushProperty, _marksElement,
            SliderMarksElement.MarkBackgroundBrushProperty, BindingMode.OneWay, BindingPriority.Template);
        BindUtils.RelayBind(this, MarkBorderThicknessProperty, _marksElement,
            SliderMarksElement.MarkBorderThicknessProperty, BindingMode.OneWay, BindingPriority.Template);
        BindUtils.RelayBind(this, SliderMarkSizeProperty, _marksElement,
            SliderMarksElement.SliderMarkSizeProperty, BindingMode.OneWay, BindingPriority.Template);
        LogicalChildren.Add(_marksElement);
        VisualChildren.Add(_marksElement);
    }

    private Border CreatePartBorder(string semanticClass, bool isVisible)
    {
        var border = new Border
        {
            IsHitTestVisible = false,
            IsVisible        = isVisible
        };
        border.SetTemplatedParent(TemplatedParent ?? this);
        border.Classes.Add(semanticClass);
        return border;
    }

    private void SyncTrackSegmentElements()
    {
        var values = EffectiveRangeValues;
        var segmentCount = !IsRangeMode || values.Count < 2 ? 1 : values.Count - 1;
        while (_trackSegmentElements.Count < segmentCount)
        {
            AddTrackSegmentElement();
        }

        while (_trackSegmentElements.Count > segmentCount)
        {
            RemoveTrackSegmentElement();
        }
    }

    private void AddTrackSegmentElement()
    {
        var segment = CreatePartBorder(SliderSemanticParts.TrackClass, IsIncluded);
        var binding = BindUtils.RelayBind(this, TrackBarBrushProperty, segment, Border.BackgroundProperty,
            BindingMode.OneWay, BindingPriority.Template);
        LogicalChildren.Add(segment);
        var insertIndex = _marksElement is null ? VisualChildren.Count : VisualChildren.IndexOf(_marksElement);
        VisualChildren.Insert(insertIndex, segment);
        _trackSegmentElements.Add((segment, binding));
    }

    private void RemoveTrackSegmentElement()
    {
        var (element, binding) = _trackSegmentElements[^1];
        _trackSegmentElements.RemoveAt(_trackSegmentElements.Count - 1);
        LogicalChildren.Remove(element);
        VisualChildren.Remove(element);
        element.SetTemplatedParent(null);
        binding.Dispose();
    }

    private void UpdatePartVisibility()
    {
        var isIncluded = IsIncluded;
        if (_tracksElement is not null)
        {
            _tracksElement.IsVisible = isIncluded;
        }

        foreach (var (element, _) in _trackSegmentElements)
        {
            element.IsVisible = isIncluded;
        }
    }

    private void HandleGlobalMousePressed(Point point)
    {
        if (_railRect.Contains(point))
        {
            return;
        }

        foreach (var thumb in _thumbs)
        {
            HandleThumbFocus(thumb, point);
        }
    }

    private void HandleThumbFocus(SliderThumb sliderThumb, Point point)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        if (!sliderThumb.Bounds.Contains(point) && sliderThumb.IsFocused)
        {
            topLevel.FocusManager?.Focus(null);
        }
    }

    private Vector CalculateThumbAdjustment(SliderThumb thumb, Rect newThumbBounds)
    {
        var thumbDelta = newThumbBounds.Position - thumb.Bounds.Position;
        return _lastDrag - thumbDelta;
    }

    private Point ValueToCenterPoint(Size arrangeSize, double value)
    {
        var railRect = GetRailRect(arrangeSize);
        var ratio = SliderRangeMath.ValueToRatio(value, Minimum, Maximum);
        if (IsDirectionReversed)
        {
            ratio = 1 - ratio;
        }

        if (Orientation == Orientation.Horizontal)
        {
            return new Point(railRect.X + railRect.Width * ratio, railRect.Center.Y);
        }

        return new Point(railRect.Center.X, railRect.Y + railRect.Height * (1 - ratio));
    }

    private Rect GetRailRect(Size size)
    {
        var thumbSize = GetThumbSize();
        if (Orientation == Orientation.Horizontal)
        {
            var offsetX = thumbSize / 2;
            var offsetY = Marks?.Count > 0
                ? Math.Max(Padding.Top, (thumbSize - SliderRailSize) / 2)
                : (size.Height - SliderRailSize) / 2;
            return new Rect(new Point(offsetX, offsetY),
                new Size(Math.Max(0, size.Width - thumbSize), SliderRailSize));
        }

        var offsetXVertical = Marks?.Count > 0
            ? Math.Max(Padding.Left, (thumbSize - SliderRailSize) / 2)
            : (size.Width - SliderRailSize) / 2;
        var offsetYVertical = thumbSize / 2;
        return new Rect(new Point(offsetXVertical, offsetYVertical),
            new Size(SliderRailSize, Math.Max(0, size.Height - thumbSize)));
    }

    private double GetThumbSize()
    {
        if (_thumbs.Count > 0)
        {
            var desired = Orientation == Orientation.Horizontal
                ? _thumbs[0].DesiredSize.Width
                : _thumbs[0].DesiredSize.Height;
            if (desired > 0)
            {
                return desired;
            }

            var explicitSize = Orientation == Orientation.Horizontal ? _thumbs[0].Width : _thumbs[0].Height;
            if (explicitSize > 0 && !double.IsNaN(explicitSize))
            {
                return explicitSize;
            }
        }

        return SliderTrackSize;
    }

    private void ThumbDragged(object? sender, VectorEventArgs e)
    {
        if (IgnoreThumbDrag || sender is not SliderThumb thumb || !thumb.IsEnabled)
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
            ApplyThumbDrag(thumb, e);
        }
    }

    private void ApplyThumbDrag(SliderThumb thumb, VectorEventArgs e)
    {
        var delta = ValueFromDistance(e.Vector.X, e.Vector.Y);
        if (delta == 0)
        {
            return;
        }

        var factor = e.Vector / delta;
        if (!IsRangeMode)
        {
            var oldValue = Value;
            SetCurrentValue(ValueProperty, Math.Clamp(Value + delta, Minimum, Maximum));
            _lastDrag = (Value - oldValue) * factor;
            return;
        }

        var values = EffectiveRangeValues;
        var oldHandleValue = values[thumb.HandleIndex];
        var updated = SliderRangeMath.MoveHandle(
            values,
            DisabledHandles,
            thumb.HandleIndex,
            oldHandleValue + delta,
            Minimum,
            Maximum);
        SetCurrentValue(RangeValuesProperty, updated);
        _lastDrag = (updated[thumb.HandleIndex] - oldHandleValue) * factor;
    }

    private void ThumbDragCompleted(object? sender, EventArgs e)
    {
        ApplyDeferredThumbDrag();
    }

    private void ApplyDeferredThumbDrag()
    {
        if (_deferredThumbDrag != null && FocusedThumb is { } thumb)
        {
            ApplyThumbDrag(thumb, _deferredThumbDrag);
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
        if (_markLabelSize != default && !force)
        {
            return;
        }

        var targetWidth = 0d;
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
                targetWidth = Math.Max(targetWidth, markTextSize.Width);
                targetHeight = Math.Max(targetHeight, markTextSize.Height);

                var typeface = new Typeface(MarkLabelFontFamily, mark.LabelFontStyle, mark.LabelFontWeight);
                var formattedText = new FormattedText(mark.Label, CultureInfo.CurrentUICulture,
                    GetFlowDirection(this),
                    typeface,
                    1,
                    mark.LabelBrush is not null && IsEnabled ? mark.LabelBrush : MarkLabelBrush);
                formattedText.SetFontSize(MarkLabelFontSize);
                formattedText.TextAlignment = TextAlignment.Left;
                mark.FormattedText = formattedText;
            }
        }

        _markLabelSize = new Size(targetWidth, targetHeight);
    }

    private static bool IsMarkTextProperty(AvaloniaProperty property)
    {
        return property == IsEnabledProperty ||
               property == MarksProperty ||
               property == MarkLabelFontSizeProperty ||
               property == MarkLabelFontFamilyProperty ||
               property == MarkLabelBrushProperty;
    }

    private void RefreshMarkTextMetrics()
    {
        CalculateMaxMarkSize(true);
        InvalidateMeasure();
        PushMarkRenderInfo(GetRailRect(Bounds.Size), EffectiveRangeValues, Bounds.Size);
    }

    private void ArrangeTrackElements(Size arrangeSize, IReadOnlyList<double> values)
    {
        if (_railElement is null || _tracksElement is null || _marksElement is null)
        {
            return;
        }

        var railRect = GetRailRect(arrangeSize);
        _railRect = railRect;
        var segmentRects = CreateSegmentRects(railRect, values);
        _trackRangeRect = CreateTrackRangeRect(railRect, values);

        var pillRadius = (Orientation == Orientation.Horizontal ? railRect.Height : railRect.Width) / 2;
        var cornerRadius = new CornerRadius(pillRadius);
        _railElement.CornerRadius = cornerRadius;
        _railElement.Arrange(railRect);
        _tracksElement.CornerRadius = cornerRadius;
        _tracksElement.Arrange(_trackRangeRect);
        for (var i = 0; i < _trackSegmentElements.Count; i++)
        {
            var segment = _trackSegmentElements[i].Element;
            segment.CornerRadius = cornerRadius;
            segment.Arrange(segmentRects[i]);
        }

        _marksElement.Arrange(new Rect(arrangeSize));
        PushMarkRenderInfo(railRect, values, arrangeSize);
    }

    private void PushMarkRenderInfo(Rect railRect, IReadOnlyList<double> values, Size boundsSize)
    {
        if (_marksElement is null)
        {
            return;
        }

        if (Marks is null || Marks.Count == 0)
        {
            _marksElement.MarkRects     = null;
            _marksElement.MarkTextRects = null;
            _markTextRects              = null;
            _marksElement.InvalidateVisual();
            return;
        }

        var markRects     = new List<(Rect, int, bool)>(Marks.Count);
        var markTextRects = new List<(Rect, int, bool, FormattedText)>(Marks.Count);
        var thumbSize     = GetThumbSize();

        for (var i = 0; i < Marks.Count; i++)
        {
            var mark = Marks[i];
            var markIncluded = IsMarkIncluded(mark.Value, values);
            var center = MarkCenterPoint(railRect, mark.Value);
            var markRect = new Rect(
                new Point(center.X - SliderMarkSize / 2, center.Y - SliderMarkSize / 2),
                new Size(SliderMarkSize, SliderMarkSize));
            markRects.Add((markRect, i, markIncluded));

            Point textPosition;
            if (Orientation == Orientation.Horizontal)
            {
                var textOffsetX = center.X - mark.LabelSize.Width / 2;
                var textOffsetY = railRect.Center.Y + thumbSize / 4;
                if (textOffsetX + mark.LabelSize.Width > boundsSize.Width)
                {
                    textOffsetX = boundsSize.Width - mark.LabelSize.Width;
                }

                textPosition = new Point(textOffsetX, textOffsetY);
            }
            else
            {
                var textOffsetX = railRect.Center.X + thumbSize / 2;
                var textOffsetY = i == 0
                    ? markRect.Y - Padding.Bottom
                    : markRect.Y - mark.LabelSize.Height / 2;
                textPosition = new Point(textOffsetX, textOffsetY);
            }

            markTextRects.Add((new Rect(textPosition, mark.LabelSize), i, markIncluded, mark.FormattedText!));
        }

        _marksElement.MarkRects     = markRects;
        _marksElement.MarkTextRects = markTextRects;
        _markTextRects              = markTextRects;
        _marksElement.InvalidateVisual();
    }

    private List<Rect> CreateSegmentRects(Rect railRect, IReadOnlyList<double> values)
    {
        var rects = new List<Rect>();
        if (!IsRangeMode || values.Count < 2)
        {
            var startRatio = SliderRangeMath.ValueToRatio(Minimum, Minimum, Maximum);
            var endRatio = SliderRangeMath.ValueToRatio(Value, Minimum, Maximum);
            if (IsDirectionReversed)
            {
                startRatio = 1 - startRatio;
                endRatio = 1 - endRatio;
            }

            rects.Add(SliderRangeMath.CreateSegmentRect(railRect, Orientation, startRatio, endRatio));
            return rects;
        }

        for (var i = 0; i < values.Count - 1; i++)
        {
            var startRatio = SliderRangeMath.ValueToRatio(values[i], Minimum, Maximum);
            var endRatio = SliderRangeMath.ValueToRatio(values[i + 1], Minimum, Maximum);
            if (IsDirectionReversed)
            {
                startRatio = 1 - startRatio;
                endRatio = 1 - endRatio;
            }

            rects.Add(SliderRangeMath.CreateSegmentRect(railRect, Orientation, startRatio, endRatio));
        }

        return rects;
    }

    private Rect CreateTrackRangeRect(Rect railRect, IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return default;
        }

        var startValue = IsRangeMode ? values[0] : Minimum;
        var endValue   = values[^1];
        var startRatio = SliderRangeMath.ValueToRatio(startValue, Minimum, Maximum);
        var endRatio   = SliderRangeMath.ValueToRatio(endValue, Minimum, Maximum);
        if (IsDirectionReversed)
        {
            startRatio = 1 - startRatio;
            endRatio = 1 - endRatio;
        }

        return SliderRangeMath.CreateSegmentRect(railRect, Orientation, startRatio, endRatio);
    }

    private Point MarkCenterPoint(Rect railRect, double value)
    {
        var ratio = SliderRangeMath.ValueToRatio(value, Minimum, Maximum);
        if (IsDirectionReversed)
        {
            ratio = 1 - ratio;
        }

        if (Orientation == Orientation.Horizontal)
        {
            return new Point(railRect.X + railRect.Width * ratio, railRect.Center.Y);
        }

        return new Point(railRect.Center.X, railRect.Y + railRect.Height * (1 - ratio));
    }

    private bool IsMarkIncluded(double value, IReadOnlyList<double> values)
    {
        if (!IsIncluded)
        {
            return false;
        }

        if (!IsRangeMode)
        {
            return MathUtils.LessThanOrClose(value, Value);
        }

        return values.Count > 0 &&
               MathUtils.GreaterThanOrClose(value, values[0]) &&
               MathUtils.LessThanOrClose(value, values[^1]);
    }
}
