using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="TimelineToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TimelineTokenRule : IControlTokenRule<ShadcnToken, TimelineToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TimelineToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        token.IndicatorTailColor = global.ColorSplit;
        token.IndicatorTailWidth = global.LineWidthBold;

        token.IndicatorDotBorderWidth = global.LineWidth * 3;
        token.ItemPaddingBottom       = new Thickness(0, 0, 0, global.UniformlyPadding * 1.25);

        token.ItemPaddingBottomLG     = token.ItemPaddingBottom * 2;

        token.IndicatorStartModeMargin  = new Thickness(0, 0, global.UniformlyMargin, 0);
        token.IndicatorEndModeMargin    = new Thickness(global.UniformlyMargin, 0, 0, 0);
        token.IndicatorMiddleModeMargin = new Thickness(global.UniformlyMargin, 0);

        // Ant Design formula kept as is: shadcn has no Timeline, so the language has no values of its own.
        token.IndicatorSize            = global.SizeMS;
        token.IndicatorDotSize         = 8;
    }
}
