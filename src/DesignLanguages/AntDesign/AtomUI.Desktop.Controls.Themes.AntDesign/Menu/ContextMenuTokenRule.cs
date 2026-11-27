using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="ContextMenuToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ContextMenuTokenRule : IControlTokenRule<AntDesignToken, ContextMenuToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ContextMenuToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        MenuPopupTokenCalculator.Calculate(in context, global, token);
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        token.ContextMenuOffsetX = global.SpacingXXS;
        token.ContextMenuOffsetY = global.SpacingXXS;
    }
}
