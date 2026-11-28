using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="SegmentedToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SegmentedTokenRule : IControlTokenRule<ShadcnToken, SegmentedToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SegmentedToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.TrackPadding      = new Thickness(global.LineWidthBold);

        token.TrackBg                     = global.ColorBgLayout;
        token.TrackBorderColor            = Colors.Transparent;
        token.TrackBorderThickness        = new Thickness(0);
        token.ItemSelectedBorderColor     = Colors.Transparent;
        token.ItemSelectedBorderThickness = new Thickness(0);
        token.ItemColor         = global.ColorTextLabel;
        token.ItemHoverColor    = global.ColorText;
        token.ItemHoverBg       = global.ColorFillSecondary;
        token.ItemSelectedBg    = global.ColorBgElevated;
        token.ItemActiveBg      = global.ColorFill;
        token.ItemSelectedColor = global.ColorText;
        var lineWidth = global.LineWidth;
        // shadcn has no segmented control: the item values keep the Ant formula. Only the track background takes a
        // language value (--muted), at the end of the method.
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
        // The default track reads ColorBgLayout, which is the page itself under shadcn: use --muted, the gray the antd
        // track renders in.
        token.TrackBg = context.Language.Muted;
    }
}
