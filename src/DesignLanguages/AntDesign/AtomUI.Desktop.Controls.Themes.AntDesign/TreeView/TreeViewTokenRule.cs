using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TreeViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class TreeViewTokenRule : IControlTokenRule<AntDesignToken, TreeViewToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TreeViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TreeViewTokenCalculator.Calculate(in context, global, token);
        token.DirectoryNodeSelectedBg = global.ColorPrimary;
        token.DirectoryNodeSelectedColor = global.ColorTextLightSolid;
    }
}
