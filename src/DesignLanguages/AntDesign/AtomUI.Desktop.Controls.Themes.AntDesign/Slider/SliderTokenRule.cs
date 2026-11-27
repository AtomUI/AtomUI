using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="SliderToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SliderTokenRule : IControlTokenRule<AntDesignToken, SliderToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SliderToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // The thumb line width is always wider than the global line width: by 1, and by 1.5 on hover.
        var increaseThumbWidth   = 1d;
        var controlSize          = global.ControlHeightLG / 4;
        var controlSizeHover     = global.ControlHeightSM / 2;
        var handleLineWidth      = global.LineWidth + increaseThumbWidth;
        var handleLineWidthHover = global.LineWidth + increaseThumbWidth * 1.5;

        token.SliderTrackSize                 = controlSizeHover;

        token.RailSize                        = 4;
        token.MarkSize                        = 8;
        token.ThumbCircleSize                 = controlSize;
        token.ThumbCircleSizeHover            = controlSizeHover;

        token.ThumbCircleBorderThickness      = new Thickness(handleLineWidth);
        token.ThumbCircleBorderThicknessHover = new Thickness(handleLineWidthHover);

        token.RailBg       = global.ColorFillTertiary;
        token.RailHoverBg  = global.ColorFillSecondary;
        token.TrackBg      = global.ColorPrimaryBorder;
        token.TrackHoverBg = global.ColorPrimaryBorderHover;

        token.MarkBorderColor       = global.ColorBorderSecondary;
        token.MarkBorderColorHover  = global.ColorFillContentHover;
        token.MarkBorderColorActive = global.ColorPrimaryBorder;

        token.ThumbCircleBorderColor       = global.ColorPrimaryBorder;
        token.ThumbCircleBorderHoverColor  = global.ColorPrimaryBorderHover;
        token.ThumbCircleBorderActiveColor = global.ColorPrimary;

        token.ThumbCircleBorderColorDisabled =
            ColorUtils.OnBackground(global.ColorTextDisabled, global.ColorBgContainer);
        token.TrackBgDisabled = global.ColorBgContainerDisabled;

        // The track reserves the hovered handle circle across the rail; the padding brings the cross size to
        // ControlHeight. SliderTrackSize (antd controlSize) is reserved and does not take part in the layout.
        var trackCrossSize = token.ThumbCircleSizeHover;
        token.SliderPaddingHorizontal =
            new Thickness(trackCrossSize / 2, (global.ControlHeight - trackCrossSize) / 2);
        token.SliderPaddingVertical = new Thickness((global.ControlHeight - trackCrossSize) / 2,
            trackCrossSize / 2);
        token.MarginPartWithMark = new Thickness(0, 0, 0, global.ControlHeightLG - controlSize);

        token.ThumbBg = global.ColorBgContainer;
        token.ThumbOutlineColor = ColorUtils.FromRgbF(0.2, token.ThumbCircleBorderActiveColor.GetRedF(),
            token.ThumbCircleBorderActiveColor.GetGreenF(),
            token.ThumbCircleBorderActiveColor.GetBlueF());
        token.ThumbOutlineThickness = new Thickness(global.WaveAnimationRange);

        token.ThumbHoverOutlineThickness = token.ThumbOutlineThickness;

        token.ThumbSize = token.ThumbCircleSizeHover + token.ThumbCircleBorderThicknessHover.Left * 2 + token.ThumbOutlineThickness.Left * 2;
    }
}
