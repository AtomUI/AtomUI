using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="RangeDateViewerToken"/>.</summary>
[ControlTokenRule]
internal sealed class RangeDateViewerTokenRule : IControlTokenRule<ShadcnToken, RangeDateViewerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<RangeDateViewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DateViewerTokenCalculator.Calculate(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = DateViewerTokenCalculator.CalculateCellRowHeight(token, global, token.CellHeight);
        token.PanelSpacing = global.UniformlyMargin;
    }
}
