using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="TabControlToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TabControlTokenRule : IControlTokenRule<ShadcnToken, TabControlToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TabControlToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        LineTabTokenCalculator.Calculate(in context, global, token);
        token.MenuEdgeThickness = global.ControlHeight;
        token.TabAndContentGutter = global.UniformlyMarginSM;
        token.HorizontalMargin = new Thickness(0, 0, global.UniformlyMargin, 0);
    }
}
