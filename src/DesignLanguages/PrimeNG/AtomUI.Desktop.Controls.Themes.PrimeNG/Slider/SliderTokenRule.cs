using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SliderToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SliderTokenRule : IControlTokenRule<PrimeNGToken, SliderToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SliderToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGSliderTokens.EvaluateGlobal(in context, global);
        // The thumb border is always wider than the line width: by 1 at rest and by 1.5 on hover.
        var increaseThumbWidth   = 1d;
        var controlSize          = global.ControlHeightLG / 4;
        var controlSizeHover     = global.ControlHeightSM / 2;
        var handleLineWidth      = global.LineWidth + increaseThumbWidth;
        var handleLineWidthHover = global.LineWidth + increaseThumbWidth * 1.5;

        // Ant Design formula kept as is: the PrimeNG slider stage at the end of the method replaces the rail, track
        // and handle values (all but SliderTrackSize and MarkSize of this group).
        token.SliderTrackSize                 = controlSizeHover;

        token.RailSize                        = 4;
        token.MarkSize                        = 8;
        token.ThumbCircleSize                 = controlSize;
        token.ThumbCircleSizeHover            = controlSizeHover;

        token.ThumbCircleBorderThickness      = new Thickness(handleLineWidth);
        token.ThumbCircleBorderThicknessHover = new Thickness(handleLineWidthHover);
        token.ThumbCornerRadius               = null;

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
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSliderTokens.EvaluateOwn(in context, global, token);
    }
}
