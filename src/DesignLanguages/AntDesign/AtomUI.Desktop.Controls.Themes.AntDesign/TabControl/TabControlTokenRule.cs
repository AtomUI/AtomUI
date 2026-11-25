using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TabControlToken"/>.</summary>
[ControlTokenRule]
internal sealed class TabControlTokenRule : IControlTokenRule<AntDesignToken, TabControlToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TabControlToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        LineTabTokenCalculator.Calculate(in context, global, token);
        token.MenuEdgeThickness = global.ControlHeight;
        token.TabAndContentGutter = global.UniformlyMarginSM;
        token.HorizontalMargin = new Thickness(0, 0, global.UniformlyMargin, 0);
    }
}
