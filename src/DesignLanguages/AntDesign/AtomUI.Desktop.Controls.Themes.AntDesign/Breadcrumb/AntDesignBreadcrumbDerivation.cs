using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="BreadcrumbToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignBreadcrumbDerivation : IControlDerivation<AntDesignToken, BreadcrumbToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<BreadcrumbToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
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
