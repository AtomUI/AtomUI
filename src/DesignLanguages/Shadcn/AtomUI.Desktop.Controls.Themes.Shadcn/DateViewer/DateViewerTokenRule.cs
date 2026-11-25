using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DateViewerToken"/>.</summary>
[ControlTokenRule]
internal sealed class DateViewerTokenRule : IControlTokenRule<ShadcnToken, DateViewerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<DateViewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DateViewerTokenCalculator.Calculate(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = DateViewerTokenCalculator.CalculateCellRowHeight(token, global, token.CellHeight);
        token.FooterBorderThickness = new Thickness(0, global.LineWidth, 0, 0);
    }
}
