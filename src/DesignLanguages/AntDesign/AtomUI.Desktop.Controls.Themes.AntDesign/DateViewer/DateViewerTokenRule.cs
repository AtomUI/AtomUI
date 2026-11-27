using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="DateViewerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class DateViewerTokenRule : IControlTokenRule<AntDesignToken, DateViewerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<DateViewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DateViewerTokenCalculator.Calculate(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = DateViewerTokenCalculator.CalculateCellRowHeight(token, global, token.CellHeight);
        token.FooterBorderThickness = new Thickness(0, global.LineWidth, 0, 0);
    }
}
