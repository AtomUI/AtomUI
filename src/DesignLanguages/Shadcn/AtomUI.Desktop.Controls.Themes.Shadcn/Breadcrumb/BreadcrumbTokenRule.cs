using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="BreadcrumbToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class BreadcrumbTokenRule : IControlTokenRule<ShadcnToken, BreadcrumbToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<BreadcrumbToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.ItemColor                   = global.ColorTextDescription;
        token.LastItemColor               = global.ColorText;
        token.LinkColor                   = global.ColorTextDescription;
        token.LinkHoverColor              = global.ColorText;
        token.LinkHoverBgColor            = global.ColorBgTextHover;
        token.SeparatorColor              = global.ColorTextDescription;
        token.SeparatorMargin             = new Thickness(global.UniformlyMarginXXS, 0);
        token.BreadcrumbItemContentPadding = new Thickness(global.UniformlyPaddingXXS, global.UniformlyPaddingXXS,
            global.UniformlyPaddingXXS, global.UniformlyPaddingXXS);
    }
}
