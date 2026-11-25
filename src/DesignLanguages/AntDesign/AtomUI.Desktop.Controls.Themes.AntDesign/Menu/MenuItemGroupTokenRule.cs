using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MenuItemGroupToken"/>.</summary>
[ControlTokenRule]
internal sealed class MenuItemGroupTokenRule : IControlTokenRule<AntDesignToken, MenuItemGroupToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<MenuItemGroupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.TitleFontWeight = FontWeight.Normal;
    }
}
