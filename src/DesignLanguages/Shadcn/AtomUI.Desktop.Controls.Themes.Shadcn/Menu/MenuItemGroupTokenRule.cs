using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="MenuItemGroupToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MenuItemGroupTokenRule : IControlTokenRule<ShadcnToken, MenuItemGroupToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<MenuItemGroupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: no registry value is merged into the menu item group tokens.
        token.TitleFontWeight = FontWeight.Normal;
    }
}
