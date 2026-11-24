using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TreeViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class TreeViewTokenRule : IControlTokenRule<ShadcnToken, TreeViewToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TreeViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TreeViewTokenCalculator.Calculate(in context, global, token);
        token.DirectoryNodeSelectedBg = global.ColorPrimary;
        token.DirectoryNodeSelectedColor = global.ColorTextLightSolid;
        if (context.Variant.IsDark)
        {
            token.NodeSelectedBg = ShadcnRecipes.CompositeOver(global.ColorText, 0.14, token.NodeSelectedBg);
        }
    }
}
