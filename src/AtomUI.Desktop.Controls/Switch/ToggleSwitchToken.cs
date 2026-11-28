using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the <see cref="ToggleSwitch" />: the sizes and colors of the track, the handle and the content
/// in the track, the inset of the content, the loading indicator and the opacity of a disabled switch, for the
/// standard and the small size.
/// </summary>
[ControlDesignToken]
public sealed partial class ToggleSwitchToken : AbstractControlDesignToken
{

    /// <summary>
    /// Height of the track at the standard switch size.
    /// </summary>
    public partial double TrackHeight { get; set; }

    /// <summary>
    /// Height of the track at the small switch size.
    /// </summary>
    public partial double TrackHeightSM { get; set; }

    /// <summary>
    /// Minimum width of the track at the standard switch size.
    /// </summary>
    public partial double TrackMinWidth { get; set; }

    /// <summary>
    /// Minimum width of the track at the small switch size.
    /// </summary>
    public partial double TrackMinWidthSM { get; set; }

    /// <summary>
    /// Inset between the edge of the track and the handle's end positions.
    /// </summary>
    public partial double TrackPadding { get; set; }

    /// <summary>
    /// Fill of the handle in an unchecked switch at rest. Pairs with <see cref="TrackBg" />.
    /// </summary>
    public partial Color HandleBg { get; set; }

    /// <summary>
    /// Fill of the handle in a checked switch at rest. Pairs with the <see cref="SwitchColor" /> track.
    /// </summary>
    public partial Color HandleCheckedBg { get; set; }

    /// <summary>
    /// Fill of the handle in an unchecked switch under the pointer. Pairs with <see cref="TrackHoverBg" />.
    /// </summary>
    public partial Color HandleHoverBg { get; set; }

    /// <summary>
    /// Fill of the handle in a checked switch under the pointer. Pairs with <see cref="TrackCheckedHoverBg" />.
    /// </summary>
    public partial Color HandleCheckedHoverBg { get; set; }

    /// <summary>
    /// Fill of the unchecked track at rest. Pairs with <see cref="HandleBg" /> and
    /// <see cref="UncheckedContentColor" />.
    /// </summary>
    public partial Color TrackBg { get; set; }

    /// <summary>
    /// Fill of the unchecked track under the pointer. Pairs with <see cref="HandleHoverBg" /> and
    /// <see cref="UncheckedContentColor" />.
    /// </summary>
    public partial Color TrackHoverBg { get; set; }

    /// <summary>
    /// Fill of the checked track under the pointer. Pairs with <see cref="HandleCheckedHoverBg" /> and the shared
    /// <c>ColorTextLightSolid</c> content foreground.
    /// </summary>
    public partial Color TrackCheckedHoverBg { get; set; }

    /// <summary>
    /// The outline of an unchecked switch's track, drawn inside the track when <see cref="TrackBorderThickness" /> is
    /// not 0. Defaults to transparent.
    /// </summary>
    public partial Color TrackBorderColor { get; set; }

    /// <summary>
    /// The outline of a checked switch's track, drawn inside the track when <see cref="TrackBorderThickness" /> is not
    /// 0. Defaults to transparent.
    /// </summary>
    public partial Color TrackCheckedBorderColor { get; set; }

    /// <summary>
    /// Thickness of the outline drawn inside the track.
    /// </summary>
    public partial double TrackBorderThickness { get; set; }

    /// <summary>
    /// Corner radius of the track, or <see langword="null" /> to draw a capsule pill.
    /// </summary>
    public partial CornerRadius? TrackCornerRadius { get; set; }

    /// <summary>
    /// Corner radius of the handle, or <see langword="null" /> to draw a circle or capsule pill.
    /// </summary>
    public partial CornerRadius? HandleCornerRadius { get; set; }

    /// <summary>
    /// Shadow drawn under the moving switch handle.
    /// </summary>
    public partial BoxShadow HandleShadow { get; set; }

    /// <summary>
    /// Width and height of the handle at the standard switch size.
    /// </summary>
    public partial Size HandleSize { get; set; }

    /// <summary>
    /// Width and height of the handle at the small switch size.
    /// </summary>
    public partial Size HandleSizeSM { get; set; }

    /// <summary>
    /// Whether pressing the switch stretches the handle inward and shifts the visible content. Defaults to true.
    /// </summary>
    public partial bool EnableHandlePressStretch { get; set; }

    /// <summary>
    /// Left inset of the visible on content at the standard switch size before any press offset.
    /// </summary>
    public partial double InnerMinMargin { get; set; }

    /// <summary>
    /// Left inset of the visible off content at the standard switch size before any press offset.
    /// </summary>
    public partial double InnerMaxMargin { get; set; }

    /// <summary>
    /// Left inset of the visible on content at the small switch size before any press offset.
    /// </summary>
    public partial double InnerMinMarginSM { get; set; }

    /// <summary>
    /// Left inset of the visible off content at the small switch size before any press offset.
    /// </summary>
    public partial double InnerMaxMarginSM { get; set; }

    /// <summary>
    /// Size of the icon shown in the track at the standard switch size.
    /// </summary>
    public partial double ContentIconSize { get; set; }

    /// <summary>
    /// Size of the icon shown in the track at the small switch size.
    /// </summary>
    public partial double ContentIconSizeSM { get; set; }

    /// <summary>
    /// Fill of the checked track at rest and foreground of the checked handle's loading indicator. The track pairs with
    /// <see cref="HandleCheckedBg" /> and the shared <c>ColorTextLightSolid</c> content foreground.
    /// </summary>
    public partial Color SwitchColor { get; set; }

    /// <summary>
    /// Foreground of the text and icon inside the unchecked track. Pairs with <see cref="TrackBg" /> or
    /// <see cref="TrackHoverBg" />.
    /// </summary>
    public partial Color UncheckedContentColor { get; set; }
    
    /// <summary>
    /// Opacity of the entire switch while disabled or loading.
    /// </summary>
    public partial double SwitchDisabledOpacity { get; set; }
    
    /// <summary>
    /// Font size of the text in the visible portion of the track at the standard switch size.
    /// </summary>
    public partial double ExtraInfoFontSize { get; set; }
    
    /// <summary>
    /// Font size of the text in the visible portion of the track at the small switch size.
    /// </summary>
    public partial double ExtraInfoFontSizeSM { get; set; }
    
    /// <summary>
    /// Duration of one cycle of the handle's loading animation.
    /// </summary>
    public partial TimeSpan LoadingAnimationDuration { get; set; }
    
    /// <summary>
    /// Foreground of the loading indicator inside the unchecked handle. Pairs with <see cref="HandleBg" />.
    /// </summary>
    public partial Color OffStateLoadIndicatorColor { get; set; }
    
}
