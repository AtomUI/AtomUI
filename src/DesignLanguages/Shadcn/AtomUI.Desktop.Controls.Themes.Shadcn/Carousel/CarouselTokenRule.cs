using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="CarouselToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CarouselTokenRule : IControlTokenRule<ShadcnToken, CarouselToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<CarouselToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.IndicatorActiveWidth = 24;
        token.ArrowSize            = 16;
        token.ArrowOffset          = global.SpacingXS;
        // AtomUI extension: Ant Design fixes the resting arrow at 0.4; the value here stays at the 0.2 default.
        token.ArrowOpacity         = 0.2;
        token.IndicatorWidth       = 16;
        token.IndicatorHeight      = 3;
        token.IndicatorGap         = global.UniformlyMarginXS;
        token.PaginationOffset     = 12;
    }
}
