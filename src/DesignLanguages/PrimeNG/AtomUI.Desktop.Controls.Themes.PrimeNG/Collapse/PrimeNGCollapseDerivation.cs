using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CollapseToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGCollapseDerivation : IControlDerivation<PrimeNGToken, CollapseToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<CollapseToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGCollapseTokens.EvaluateGlobal(in context, global);
        token.HeaderPadding             = new Thickness(global.UniformlyPadding, global.UniformlyPaddingSM);
        token.HeaderBg                  = global.ColorFillAlter;
        token.HeaderColor               = global.ColorTextHeading;
        token.HeaderActiveColor         = global.ColorTextHeading;
        token.HeaderFontWeight          = FontWeight.Normal;
        token.ContentPadding            = new Thickness(16, global.UniformlyPadding);
        token.CollapseContentPaddingSM  = global.PaddingSM;
        token.CollapseContentPaddingLG  = global.PaddingLG;
        token.ContentBg                 = global.ColorBgContainer;
        token.CollapseHeaderPaddingSM   = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.CollapseHeaderPaddingLG   = new Thickness(global.UniformlyPaddingLG, global.UniformlyPadding);
        token.CollapsePanelBorderRadius = global.BorderRadiusLG;
        token.LeftExpandButtonMarginSM  = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        token.LeftExpandButtonMargin    = new Thickness(0, 0, global.UniformlyMarginXS, 0);
        token.LeftExpandButtonMarginLG  = new Thickness(0, 0, global.UniformlyMarginSM, 0);
        token.RightExpandButtonMarginSM = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.RightExpandButtonMargin   = new Thickness(global.UniformlyMarginXS, 0, 0, 0);
        token.RightExpandButtonMarginLG   = new Thickness(global.UniformlyMarginSM, 0, 0, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCollapseTokens.EvaluateOwn(in context, global, token);
    }
}
