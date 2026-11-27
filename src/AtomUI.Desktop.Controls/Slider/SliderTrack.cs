using AtomUI.Theme;
using System.Globalization;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Media;
using AtomUI.Reflection;
using AtomUI.Theme.Resources;
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
using Avalonia.Media.Immutable;
using Avalonia.Rendering;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The track of a <see cref="Slider" />, created by the template of the slider as <c>PART_Track</c>.
/// It owns the groove, the filled segments, the marks and one thumb per handle, lays them out along
/// the rail, and maps positions and drag distances on the rail to values.
/// </summary>
/// <remarks>
/// The theme of the slider forwards the properties of the slider to the track, so applications set
/// them on the slider. Properties that the track owns from another control, such as
/// <see cref="Minimum" /> and <see cref="Orientation" />, keep the default of that control.
/// </remarks>
[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
// A Token-owner transparent part: the track is a template part of the Slider only, so its theme reads the Slider
// TokenResource and its Token-declared transitions resolve to the Slider through the inherited owner.
[TokenOwnerTransparent]
public class SliderTrack : TemplatedControl, ICustomHitTest
{
    /// <summary>
    /// Reports whether a pointer position lies within the bounds of the track.
    /// SliderTrack no longer self-draws its rail/track visuals, so it has no draw
    /// list for the compositor to hit test against. Report the whole bounds as the
    /// hit target so pointer input still routes through the track exactly as it did
    /// when the rail was self-drawn.
    /// </summary>
    public bool HitTest(Point point) => new Rect(Bounds.Size).Contains(point);

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Minimum" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty =
        RangeBase.MinimumProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="Maximum" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty =
        RangeBase.MaximumProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty =
        RangeBase.ValueProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="RangeValues" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> RangeValuesProperty =
        AbstractSlider.RangeValuesProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="DisabledHandles" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<bool>?> DisabledHandlesProperty =
        AvaloniaProperty.Register<SliderTrack, IReadOnlyList<bool>?>(nameof(DisabledHandles));

    /// <summary>
    /// Identifies the <see cref="IsDraggableTrack" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDraggableTrackProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IsDraggableTrack));

    /// <summary>
    /// Identifies the <see cref="IsRangeMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRangeModeProperty =
        AbstractSlider.IsRangeModeProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="IsDirectionReversed" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IsDirectionReversed));

    /// <summary>
    /// Identifies the <see cref="IgnoreThumbDrag" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IgnoreThumbDragProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(IgnoreThumbDrag));

    /// <summary>
    /// Identifies the <see cref="DeferThumbDrag" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> DeferThumbDragProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(DeferThumbDrag));

    /// <summary>
    /// Identifies the <see cref="IsIncluded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIncludedProperty =
        AbstractSlider.IsIncludedProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="TrackBarBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TrackBarBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(TrackBarBrush));

    /// <summary>
    /// Identifies the <see cref="TracksBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TracksBrushProperty =
        AbstractSlider.TracksBrushProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="TrackGrooveBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TrackGrooveBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(TrackGrooveBrush));

    /// <summary>
    /// Identifies the <see cref="Marks" /> property.
    /// </summary>
    public static readonly StyledProperty<List<SliderMark>?> MarksProperty =
        AbstractSlider.MarksProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="MarkLabelFontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MarkLabelFontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="MarkLabelFontFamily" /> property.
    /// </summary>
    public static readonly StyledProperty<FontFamily> MarkLabelFontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<SliderTrack>();

    /// <summary>
    /// Identifies the <see cref="MarkLabelBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> MarkLabelBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(MarkLabelBrush));

    /// <summary>
    /// The smallest value the track places on the rail. Pointer positions and drag distances are scaled
    /// to the range from here to <see cref="Maximum" />.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="RangeBase.Minimum" /> to this property. A
    /// cleared value falls back to the default of <see cref="RangeBase.Minimum" />.
    /// </remarks>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// The largest value the track places on the rail. Pointer positions and drag distances are scaled to
    /// the range from <see cref="Minimum" /> to here.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="RangeBase.Maximum" /> to this property. A
    /// cleared value falls back to the default of <see cref="RangeBase.Maximum" />.
    /// </remarks>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// The value of the only thumb and the end of the filled part while <see cref="IsRangeMode" /> is
    /// cleared. While <see cref="IsRangeMode" /> is set, <see cref="RangeValues" /> places the thumbs and
    /// the filled part instead, and this property is only the fallback for a handle that has no entry.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="RangeBase.Value" /> to this property in both
    /// directions, so the track writes it back while it applies a thumb drag itself. A cleared value falls
    /// back to the default of <see cref="RangeBase.Value" />.
    /// </remarks>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// The values of the handles, in ascending order, while <see cref="IsRangeMode" /> is set. Every
    /// entry places one thumb, and a filled segment runs between each pair of adjacent entries. While
    /// <see cref="IsRangeMode" /> is cleared, <see cref="Value" /> is used instead.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="AbstractSlider.RangeValues" /> to this
    /// property in both directions, so the track writes it back while it applies a thumb drag itself.
    /// While <see cref="IsRangeMode" /> is set, <see langword="null" /> is read as two handles at
    /// <see cref="Minimum" />.
    /// </remarks>
    public IReadOnlyList<double>? RangeValues
    {
        get => GetValue(RangeValuesProperty);
        set => SetValue(RangeValuesProperty, value);
    }

    /// <summary>
    /// For each handle, whether it is disabled: entry <c>i</c> belongs to handle <c>i</c>. A disabled
    /// handle gets a disabled thumb that the track does not drag, and the drag of the whole range does not
    /// start while any handle is disabled. The default, <see langword="null" />, disables no handle.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="Slider.DisabledHandles" /> to this property.
    /// </remarks>
    public IReadOnlyList<bool>? DisabledHandles
    {
        get => GetValue(DisabledHandlesProperty);
        set => SetValue(DisabledHandlesProperty, value);
    }

    /// <summary>
    /// While set, a pointer drag that starts on the filled range between the outer handles, not on a
    /// thumb, moves every handle together. It takes effect only while <see cref="IsRangeMode" /> is set
    /// with at least two handles, none of which is disabled.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="Slider.IsDraggableTrack" /> to this property.
    /// </remarks>
    public bool IsDraggableTrack
    {
        get => GetValue(IsDraggableTrackProperty);
        set => SetValue(IsDraggableTrackProperty, value);
    }

    /// <summary>
    /// While set, the track shows one thumb per entry of <see cref="RangeValues" /> and a filled segment
    /// between adjacent thumbs. While cleared, it shows one thumb at <see cref="Value" /> and one segment
    /// from <see cref="Minimum" /> to it.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="AbstractSlider.IsRangeMode" /> to this
    /// property, which keeps the default of <see cref="AbstractSlider.IsRangeMode" />.
    /// </remarks>
    public bool IsRangeMode
    {
        get => GetValue(IsRangeModeProperty);
        set => SetValue(IsRangeModeProperty, value);
    }

    /// <summary>
    /// The direction of the rail. While horizontal, values grow from left to right; while vertical, from
    /// bottom to top, unless <see cref="IsDirectionReversed" /> is set. It also selects the
    /// <c>:horizontal</c> or <c>:vertical</c> pseudo-class of the track.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="Slider.Orientation" /> to this property, which
    /// keeps the default of <see cref="Avalonia.Controls.Primitives.ScrollBar.Orientation" />.
    /// </remarks>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// While set, the rail runs the other way round: <see cref="Maximum" /> lies at the left end of a
    /// horizontal rail and at the top end of a vertical one. Pointer positions, drag distances, thumbs,
    /// filled segments and marks all follow.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="Slider.IsDirectionReversed" /> to this
    /// property.
    /// </remarks>
    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    /// <summary>
    /// While set, the track ignores the drag events of its thumbs and leaves <see cref="Value" /> and
    /// <see cref="RangeValues" /> untouched. The <see cref="Slider" /> sets it on the track of its
    /// template and handles the drag of a thumb itself.
    /// </summary>
    public bool IgnoreThumbDrag
    {
        get => GetValue(IgnoreThumbDragProperty);
        set => SetValue(IgnoreThumbDragProperty, value);
    }

    /// <summary>
    /// While set, the track does not apply the drag of a thumb while it moves. It keeps the latest drag
    /// event and applies it once when the drag completes or when this property is cleared. It has no
    /// effect while <see cref="IgnoreThumbDrag" /> is set.
    /// </summary>
    public bool DeferThumbDrag
    {
        get => GetValue(DeferThumbDragProperty);
        set => SetValue(DeferThumbDragProperty, value);
    }

    /// <summary>
    /// Whether the filled part of the track is shown: the container behind the filled segments, the
    /// segments themselves, and the included state of the marks that lie inside the selected range. The
    /// groove and the marks stay visible while it is cleared.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="AbstractSlider.IsIncluded" /> to this
    /// property, which keeps the default of <see cref="AbstractSlider.IsIncluded" />, which is
    /// <see langword="true" />.
    /// </remarks>
    public bool IsIncluded
    {
        get => GetValue(IsIncludedProperty);
        set => SetValue(IsIncludedProperty, value);
    }

    /// <summary>
    /// The brush of each filled segment of the track. The theme supplies the <c>TrackBg</c> token, and the
    /// <c>TrackHoverBg</c> token while the slider is hovered. <see langword="null" /> leaves the segments
    /// unpainted.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="Slider.TrackBarBrush" /> to this property.
    /// </remarks>
    public IBrush? TrackBarBrush
    {
        get => GetValue(TrackBarBrushProperty);
        set => SetValue(TrackBarBrushProperty, value);
    }

    /// <summary>
    /// The brush of the container behind the filled segments, which is shown only while
    /// <see cref="IsIncluded" /> is set. The container spans from the first handle to the last one while
    /// <see cref="IsRangeMode" /> is set, and from <see cref="Minimum" /> to <see cref="Value" />
    /// otherwise. <see cref="TrackBarBrush" /> paints the segments themselves.
    /// <see langword="null" /> leaves the container unpainted.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="AbstractSlider.TracksBrush" /> to this
    /// property, which keeps the default of <see cref="AbstractSlider.TracksBrush" />.
    /// </remarks>
    public IBrush? TracksBrush
    {
        get => GetValue(TracksBrushProperty);
        set => SetValue(TracksBrushProperty, value);
    }

    /// <summary>
    /// The brush of the groove, the full-length rail behind the filled segments, which stays visible
    /// while <see cref="IsIncluded" /> is cleared. The theme supplies the <c>RailBg</c> token, and the
    /// <c>RailHoverBg</c> token while the slider is hovered. <see langword="null" /> leaves the groove
    /// unpainted. No property of the <see cref="Slider" /> is forwarded to it, so a style that selects
    /// the track sets it.
    /// </summary>
    public IBrush? TrackGrooveBrush
    {
        get => GetValue(TrackGrooveBrushProperty);
        set => SetValue(TrackGrooveBrushProperty, value);
    }

    /// <summary>
    /// The marks that are drawn on the rail: one dot at the value of each entry, with the label of the
    /// entry below the rail on a horizontal track and to the right of the rail on a vertical one. The
    /// largest label enlarges the measured size of the track across the rail. <see langword="null" /> or
    /// an empty list draws none of them.
    /// </summary>
    /// <remarks>
    /// The <see cref="Slider" /> forwards its own <see cref="AbstractSlider.Marks" /> to this property,
    /// which keeps the default of <see cref="AbstractSlider.Marks" />.
    /// </remarks>
    public List<SliderMark>? Marks
    {
        get => GetValue(MarksProperty);
        set => SetValue(MarksProperty, value);
    }

    /// <summary>
    /// The font size of the mark labels. The theme of the slider forwards the <c>FontSize</c> of the
    /// slider to it, and a change re-measures the labels and the track.
    /// </summary>
    /// <remarks>
    /// This is the <c>TextElement.FontSize</c> property owned by the track, so a value that is not set is
    /// inherited from the parent.
    /// </remarks>
    public double MarkLabelFontSize
    {
        get => GetValue(MarkLabelFontSizeProperty);
        set => SetValue(MarkLabelFontSizeProperty, value);
    }

    /// <summary>
    /// The font family of the mark labels. The theme of the slider forwards the <c>FontFamily</c> of the
    /// slider to it, and a change re-measures the labels and the track.
    /// </summary>
    /// <remarks>
    /// This is the <c>TextElement.FontFamily</c> property owned by the track, so a value that is not set
    /// is inherited from the parent.
    /// </remarks>
    public FontFamily MarkLabelFontFamily
    {
        get => GetValue(MarkLabelFontFamilyProperty);
        set => SetValue(MarkLabelFontFamilyProperty, value);
    }

    /// <summary>
    /// The brush that paints the mark labels. The theme of the track supplies the <c>ColorText</c> token,
    /// and the <c>ColorTextDisabled</c> token while the track is disabled. <see langword="null" /> leaves
    /// the labels unpainted, except for a mark that sets its own <c>LabelBrush</c> while the track is
    /// enabled.
    /// </summary>
    public IBrush? MarkLabelBrush
    {
        get => GetValue(MarkLabelBrushProperty);
        set => SetValue(MarkLabelBrushProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> TickFrequencyProperty = AbstractSlider.TickFrequencyProperty.AddOwner<SliderTrack>();

    internal static readonly StyledProperty<bool> IsDotsVisibleProperty = AbstractSlider.IsDotsVisibleProperty.AddOwner<SliderTrack>();

    internal static readonly StyledProperty<bool> UseTrackAndMarkTokenResourcesProperty =
        AvaloniaProperty.Register<SliderTrack, bool>(nameof(UseTrackAndMarkTokenResources));

    internal static readonly StyledProperty<IBrush?> MarkBorderBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(MarkBorderBrush));

    internal static readonly StyledProperty<IBrush?> MarkBorderActiveBrushProperty =
        AvaloniaProperty.Register<SliderTrack, IBrush?>(nameof(MarkBorderActiveBrush));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SliderTrack>();

    internal double TickFrequency => GetValue(TickFrequencyProperty);

    internal bool IsDotsVisible => GetValue(IsDotsVisibleProperty);

    internal bool UseTrackAndMarkTokenResources
    {
        get => GetValue(UseTrackAndMarkTokenResourcesProperty);
        set => SetValue(UseTrackAndMarkTokenResourcesProperty, value);
    }

    private double SliderTrackSize => _trackSize;
    private double SliderRailSize => _railSize;
    private double SliderMarkSize => _markPaint.Size;

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

    /// <summary>
    /// Converts a drag distance into the change of value it stands for on the rail.
    /// </summary>
    /// <param name="horizontal">
    /// The distance dragged along the x axis. It is used while <see cref="Orientation" /> is horizontal.
    /// </param>
    /// <param name="vertical">
    /// The distance dragged along the y axis. It is used while <see cref="Orientation" /> is vertical.
    /// </param>
    /// <returns>
    /// The change of value, positive for a drag towards <see cref="Maximum" />. It is neither clamped to
    /// the range from <see cref="Minimum" /> to <see cref="Maximum" /> nor snapped to a step, and it is 0
    /// when the rail has no length in the direction of the track.
    /// </returns>
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
    private OwnerTokenSet? _trackTokens;
    private SliderMarkPaint _markPaint;
    private double _trackSize;
    private double _railSize;

    private static readonly object[] s_trackKeys =
    [
        SliderTokenKind.ThumbCircleSizeHover,
        SliderTokenKind.RailSize,
        SliderTokenKind.MarkSize,
        ControlTokenResourceKey.Global(SliderTokens.Identity, SharedTokenKind.ColorBgElevated),
        SliderTokenKind.ThumbCircleBorderThickness
    ];

    static SliderTrack()
    {
        // The layout includes the caller's padding and mark labels.
        AffectsMeasure<SliderTrack>(MarksProperty,
            MarkLabelFontSizeProperty,
            MarkLabelFontFamilyProperty,
            RangeValuesProperty,
            IsRangeModeProperty,
            PaddingProperty);
        AffectsArrange<SliderTrack>(IsDirectionReversedProperty,
            TickFrequencyProperty,
            IsDotsVisibleProperty,
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

    /// <summary>
    /// Subscribes to the token resources that feed the track, rail and mark metrics, starts observing
    /// pointer presses so that a focused thumb loses focus when the pointer is pressed outside the rail,
    /// and creates the thumbs and measures the mark labels once the track is attached. An override must
    /// call the base member, or the track gets no thumbs and no subscriptions.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateTrackAndMarkTokenSubscription();
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

    /// <summary>
    /// Releases the token subscription and the pointer-press observation and removes every thumb once the
    /// track is detached. An override must call the base member, or the subscriptions stay alive and the
    /// thumbs stay attached.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _trackTokens?.Dispose();
        _trackTokens = null;
        base.OnDetachedFromVisualTree(e);
        _focusProcessDisposable?.Dispose();
        _focusProcessDisposable = null;
        ClearThumbs();
    }

    private void UpdateTrackAndMarkTokenSubscription()
    {
        if (UseTrackAndMarkTokenResources && _trackTokens is null)
        {
            _trackTokens = new OwnerTokenSet(this, s_trackKeys.Length, RefreshTrackAndMarkStyle);
            for (var index = 0; index < s_trackKeys.Length; ++index)
            {
                _trackTokens.SetResourceKey(index, s_trackKeys[index]);
            }
        }
        else if (!UseTrackAndMarkTokenResources && _trackTokens is not null)
        {
            _trackTokens.Dispose();
            _trackTokens = null;
        }
        RefreshTrackAndMarkStyle();
    }

    private void RefreshTrackAndMarkStyle()
    {
        var trackSize = ReadNumber(_trackTokens?.GetValue(0), _trackSize);
        var railSize = ReadNumber(_trackTokens?.GetValue(1), _railSize);
        var markSize = ReadNumber(_trackTokens?.GetValue(2), _markPaint.Size);
        var background = _trackTokens?.GetValue(3) switch
        {
            IBrush brush => brush,
            Color color => new ImmutableSolidColorBrush(color),
            string text when Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
            var value when ReferenceEquals(value, BindingOperations.DoNothing) => _markPaint.Background,
            _ => null
        };
        var borderThickness = ReadMarkBorderThickness(_trackTokens?.GetValue(4));
        var layoutChanged = !_trackSize.Equals(trackSize) || !_railSize.Equals(railSize) || !_markPaint.Size.Equals(markSize);

        _trackSize = trackSize;
        _railSize = railSize;
        _markPaint = new SliderMarkPaint(background, borderThickness, markSize);
        _marksElement?.UpdateStaticPaint(_markPaint);
        if (layoutChanged)
        {
            InvalidateMeasure();
        }
    }

    private static double ReadNumber(object? value, double previous)
    {
        if (value is double number)
        {
            return number;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private Thickness ReadMarkBorderThickness(object? value)
    {
        if (value is Thickness thickness)
        {
            return thickness;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return _markPaint.BorderThickness;
        }
        if (value is string text)
        {
            try
            {
                return Thickness.Parse(text);
            }
            catch (FormatException)
            {
                return default;
            }
        }
        return default;
    }

    /// <summary>
    /// Measures the thumbs with the available size and returns the size of the track: the token track
    /// size along the rail, the largest mark label across the rail, and the padding on both axes. The
    /// base member is not called, so an override that replaces this method must measure the thumbs itself.
    /// </summary>
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

    /// <summary>
    /// Places every thumb with its center on the rail at its value, arranges the groove, the filled
    /// segments and the marks, and returns the whole arrange size. An override that needs the thumbs and
    /// the parts placed must call this base member.
    /// </summary>
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

    /// <summary>
    /// Keeps the track in step with its properties: the orientation pseudo-classes, the number and the
    /// enabled state of the thumbs, the token subscription, the visibility of the filled parts and the
    /// metrics of the mark labels. It applies a deferred thumb drag when <see cref="DeferThumbDrag" />
    /// is cleared. The base member is called first, and an override must call it as well.
    /// </summary>
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
        else if (change.Property == UseTrackAndMarkTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateTrackAndMarkTokenSubscription();
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
        _marksElement.UpdateStaticPaint(_markPaint);
        // This renderer is created at runtime and has the Slider as its templated parent. The relay passes
        // the track's state brushes, including values driven by its transitions.
        BindUtils.RelayBind(this, MarkBorderBrushProperty, _marksElement,
            SliderMarksElement.MarkBorderBrushProperty, BindingMode.OneWay, BindingPriority.Template);
        BindUtils.RelayBind(this, MarkBorderActiveBrushProperty, _marksElement,
            SliderMarksElement.MarkBorderActiveBrushProperty, BindingMode.OneWay, BindingPriority.Template);
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
        // The pill radius follows the arranged rail; Template priority keeps semantic part styles in charge of it.
        var cornerRadius = new CornerRadius(pillRadius);
        _railElement.SetValue(Border.CornerRadiusProperty, cornerRadius, BindingPriority.Template);
        _railElement.Arrange(railRect);
        _tracksElement.SetValue(Border.CornerRadiusProperty, cornerRadius, BindingPriority.Template);
        _tracksElement.Arrange(_trackRangeRect);
        for (var i = 0; i < _trackSegmentElements.Count; i++)
        {
            var segment = _trackSegmentElements[i].Element;
            segment.SetValue(Border.CornerRadiusProperty, cornerRadius, BindingPriority.Template);
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

        if (!IsDotsVisible && Marks is not { Count: > 0 })
        {
            _marksElement.MarkRects     = null;
            _marksElement.MarkTextRects = null;
            _markTextRects              = null;
            _marksElement.InvalidateVisual();
            return;
        }

        var markRects = new List<(Rect, int, bool)>();
        var frequency = TickFrequency;
        if (IsDotsVisible && frequency > 0 && double.IsFinite(frequency))
        {
            var count = (Maximum - Minimum) / frequency;
            var length = Orientation == Orientation.Horizontal ? railRect.Width : railRect.Height;
            if (double.IsFinite(count) && count >= 0)
            {
                // Coalesce ticks that land on the same visible pixel.
                var stride = Math.Max(1, Math.Ceiling(count / Math.Max(1, length)));
                for (var index = 0d; index < count; index += stride)
                {
                    AddMark(markRects, railRect, values, Minimum + index * frequency, -1);
                }
                AddMark(markRects, railRect, values, Maximum, -1);
            }
        }

        if (Marks is null || Marks.Count == 0)
        {
            _marksElement.MarkRects     = markRects.Count > 0 ? markRects : null;
            _marksElement.MarkTextRects = null;
            _markTextRects              = null;
            _marksElement.InvalidateVisual();
            return;
        }

        var markTextRects = new List<(Rect, int, bool, FormattedText)>(Marks.Count);
        var thumbSize     = GetThumbSize();

        for (var i = 0; i < Marks.Count; i++)
        {
            var mark = Marks[i];
            var markIncluded = IsMarkIncluded(mark.Value, values);
            var center = MarkCenterPoint(railRect, mark.Value);
            var markRect = CreateMarkRect(center);
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

    private void AddMark(List<(Rect, int, bool)> marks, Rect railRect, IReadOnlyList<double> values, double value, int index)
    {
        var center = MarkCenterPoint(railRect, value);
        marks.Add((CreateMarkRect(center), index, IsMarkIncluded(value, values)));
    }

    private Rect CreateMarkRect(Point center) =>
        new(center.X - SliderMarkSize / 2, center.Y - SliderMarkSize / 2, SliderMarkSize, SliderMarkSize);

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
