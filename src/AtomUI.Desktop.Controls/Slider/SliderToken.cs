using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// vertical: part (in a horizontal slider, the vertical direction is named part)
// horizontal: full (in a horizontal slider, the horizontal direction is named full)
/// <summary>
/// Design tokens of the <see cref="Slider" />: sizes and state colors of the rail, the filled track, the tick
/// and mark dots and the thumb circle, plus the padding of the track layout in both orientations.
/// </summary>
[ControlDesignToken]
public sealed partial class SliderToken : AbstractControlDesignToken
{

    /// <summary>
    /// Height of the slider control, the antd <c>controlSize</c> token.
    /// Reserved: the cross size of the slider is pinned to the control height by the derived padding and the rail
    /// inset follows the thumb size, so this Token has no design-meaningful effect yet.
    /// </summary>
    [ReservedToken("antd Slider controlSize. The Slider cross size is pinned to ControlHeight by the derived padding and " +
                   "the rail inset follows the thumb size, so this Token has no design-meaningful effect yet.")]
    public partial double SliderTrackSize { get; set; }

    /// <summary>
    /// Thickness of the unfilled rail across the slider's travel axis.
    /// </summary>
    public partial double RailSize { get; set; }

    /// <summary>
    /// Diameter of a tick or mark dot on the slider.
    /// </summary>
    public partial double MarkSize { get; set; }

    /// <summary>
    /// Width and height reserved for the thumb, including room for its border and outline.
    /// </summary>
    public partial double ThumbSize { get; set; }

    /// <summary>
    /// Diameter of the thumb circle in its resting state.
    /// </summary>
    public partial double ThumbCircleSize { get; set; }

    /// <summary>
    /// Diameter of the thumb circle while hovered or focused.
    /// </summary>
    public partial double ThumbCircleSizeHover { get; set; }

    /// <summary>
    /// Thickness of the thumb circle border in its resting state.
    /// </summary>
    public partial Thickness ThumbCircleBorderThickness { get; set; }

    /// <summary>
    /// Thickness of the thumb circle border while hovered or focused.
    /// </summary>
    public partial Thickness ThumbCircleBorderThicknessHover { get; set; }

    /// <summary>
    /// Background color of the unfilled rail at rest.
    /// </summary>
    public partial Color RailBg { get; set; }

    /// <summary>
    /// Background color of the unfilled rail while the pointer is over the slider track.
    /// </summary>
    public partial Color RailHoverBg { get; set; }

    /// <summary>
    /// Background color of the filled track.
    /// </summary>
    public partial Color TrackBg { get; set; }

    /// <summary>
    /// Background color of the filled track while the pointer is over the slider.
    /// </summary>
    public partial Color TrackHoverBg { get; set; }

    /// <summary>
    /// Border color of tick and mark dots outside the filled track.
    /// </summary>
    public partial Color MarkBorderColor { get; set; }

    /// <summary>
    /// Border color of unfilled tick and mark dots while the pointer is over the slider track.
    /// </summary>
    public partial Color MarkBorderColorHover { get; set; }

    /// <summary>
    /// Border color of tick and mark dots on the filled portion of the track.
    /// </summary>
    public partial Color MarkBorderColorActive { get; set; }

    /// <summary>
    /// Resting border color of the thumb circle around <see cref="ThumbBg" />.
    /// </summary>
    public partial Color ThumbCircleBorderColor { get; set; }

    /// <summary>
    /// Border color of an unfocused thumb while the pointer is over the slider outside that thumb.
    /// </summary>
    public partial Color ThumbCircleBorderHoverColor { get; set; }

    /// <summary>
    /// Border color of a thumb that is focused or under the pointer.
    /// </summary>
    public partial Color ThumbCircleBorderActiveColor { get; set; }

    /// <summary>
    /// Border color of the disabled thumb, also used for disabled tick and mark dots.
    /// </summary>
    public partial Color ThumbCircleBorderColorDisabled { get; set; }

    /// <summary>
    /// Fill color of the thumb circle.
    /// </summary>
    public partial Color ThumbBg { get; set; }

    /// <summary>
    /// Color of the outline ring drawn around the thumb circle.
    /// </summary>
    public partial Color ThumbOutlineColor { get; set; }

    /// <summary>
    /// Thickness of the thumb outline while the thumb has focus.
    /// </summary>
    public partial Thickness ThumbOutlineThickness { get; set; }

    /// <summary>
    /// Thickness of the outline ring around the thumb circle while the pointer is over it. The ring of a focused
    /// thumb is sized by <see cref="ThumbOutlineThickness" />.
    /// </summary>
    public partial Thickness ThumbHoverOutlineThickness { get; set; }

    /// <summary>
    /// Background color of the filled track when the slider is disabled.
    /// </summary>
    public partial Color TrackBgDisabled { get; set; }

    /// <summary>
    /// Padding of the track layout in a horizontal slider.
    /// </summary>
    public partial Thickness SliderPaddingHorizontal { get; set; }
    /// <summary>
    /// Padding of the track layout in a vertical slider.
    /// </summary>
    public partial Thickness SliderPaddingVertical { get; set; }
    /// <summary>
    /// Bottom margin of a slider with marks, from the antd <c>marginPartWithMark</c> token
    /// (<c>controlHeightLG - controlSize</c>). Reserved: <see cref="SliderTrack" /> measures the mark labels and
    /// adds their size to the track height, so no fixed mark margin is applied yet.
    /// </summary>
    [ReservedToken("antd Slider marginPartWithMark (controlHeightLG - controlSize). SliderTrack measures the mark labels " +
                   "and adds their size to the track height, so no fixed mark margin is applied yet.")]
    public partial Thickness MarginPartWithMark { get; set; }
    
}
