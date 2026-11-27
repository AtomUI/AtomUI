using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SegmentedToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SegmentedTokenRule : IControlTokenRule<PrimeNGToken, SegmentedToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SegmentedToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.TrackPadding      = new Thickness(global.LineWidthBold);

        token.TrackBg           = global.ColorBgLayout;
        token.ItemColor         = global.ColorTextLabel;
        token.ItemHoverColor    = global.ColorText;
        token.ItemHoverBg       = global.ColorFillSecondary;
        token.ItemSelectedBg    = global.ColorBgElevated;
        token.ItemActiveBg      = global.ColorFill;
        token.ItemSelectedColor = global.ColorText;
        var lineWidth = global.LineWidth;
        token.SegmentedItemPadding = new Thickness(
            Math.Max(global.ControlPaddingHorizontal - lineWidth, 0),
            0,
            Math.Max(global.ControlPaddingHorizontal - lineWidth, 0),
            0);
        token.SegmentedItemPaddingSM = new Thickness(
            Math.Max(global.ControlPaddingHorizontalSM - lineWidth, 0),
            0,
            Math.Max(global.ControlPaddingHorizontalSM - lineWidth, 0),
            0);
        token.SegmentedItemContentMargin = new Thickness(global.UniformlyPaddingXXS, 0, 0, 0);

        token.ItemMinHeightLG = global.ControlHeightLG - token.TrackPadding.Top - token.TrackPadding.Bottom;
        token.ItemMinHeight   = global.ControlHeight - token.TrackPadding.Top - token.TrackPadding.Bottom;
        token.ItemMinHeightSM = global.ControlHeightSM - token.TrackPadding.Top - token.TrackPadding.Bottom;
    }
}
