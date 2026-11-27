using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="RangeDateViewerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class RangeDateViewerTokenRule : IControlTokenRule<AntDesignToken, RangeDateViewerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<RangeDateViewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DateViewerTokenCalculator.Calculate(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = DateViewerTokenCalculator.CalculateCellRowHeight(token, global, token.CellHeight);
        token.PanelSpacing = global.UniformlyMargin;
    }
}
