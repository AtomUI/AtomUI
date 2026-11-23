using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TimelineToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTimelineDerivation : IControlDerivation<AntDesignToken, TimelineToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TimelineToken> view)
    {
        var token  = view.Token;
        var global = view.Global;

        token.IndicatorTailColor = global.ColorSplit;
        token.IndicatorTailWidth = global.LineWidthBold;

        token.IndicatorDotBorderWidth = global.LineWidth * 3;
        token.ItemPaddingBottom       = new Thickness(0, 0, 0, global.UniformlyPadding * 1.25);

        token.ItemPaddingBottomLG     = token.ItemPaddingBottom * 2;

        token.IndicatorStartModeMargin  = new Thickness(0, 0, global.UniformlyMargin, 0);
        token.IndicatorEndModeMargin    = new Thickness(global.UniformlyMargin, 0, 0, 0);
        token.IndicatorMiddleModeMargin = new Thickness(global.UniformlyMargin, 0);

        token.IndicatorSize            = global.SizeMS;
        token.IndicatorDotSize         = 8;
    }
}
