using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ExpanderToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnExpanderDerivation : IControlDerivation<ShadcnToken, ExpanderToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ExpanderToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.HeaderPadding            = new Thickness(global.UniformlyPadding, global.UniformlyPaddingSM);
        token.HeaderBg                 = global.ColorFillAlter;
        token.HeaderColor              = global.ColorTextHeading;
        token.HeaderFontWeight         = FontWeight.Normal;
        token.ContentPadding           = new Thickness(16, global.UniformlyPadding);
        token.ContentPaddingSM         = global.PaddingSM;
        token.ContentPaddingLG         = global.PaddingLG;
        token.ContentBg                = global.ColorBgContainer;
        token.HeaderPaddingSM          = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.HeaderPaddingLG          = new Thickness(global.UniformlyPaddingLG, global.UniformlyPadding);
        token.ExpanderBorderRadius     = global.BorderRadiusLG;
        token.LeftExpandButtonHMargin  = new Thickness(0, 0, global.UniformlyMarginSM, 0);
        token.RightExpandButtonHMargin = new Thickness(global.UniformlyMarginSM, 0, 0, 0);
    }
}
