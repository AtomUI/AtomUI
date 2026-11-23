using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DescriptionsToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGDescriptionsDerivation : IControlDerivation<PrimeNGToken, DescriptionsToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<DescriptionsToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGDescriptionsTokens.EvaluateGlobal(in context, global);
        token.LabelBg      = global.ColorFillAlter;
        token.LabelColor   = global.ColorTextTertiary;
        token.TitleColor   = global.ColorText;
        token.HeaderMargin = new Thickness(0, 0, 0, global.FontSizeSM * global.RelativeLineHeightSM);
        token.ItemPaddingLG  = new Thickness(global.UniformlyPaddingLG, global.UniformlyPadding);
        token.ItemPadding  = new Thickness(global.UniformlyPaddingLG, global.UniformlyPaddingSM);
        token.ItemPaddingSM  = new Thickness(global.UniformlyPadding, global.UniformlyPaddingXS);
        token.ColonMargin  = new Thickness(global.UniformlyMarginXXS / 2, 0, global.UniformlyMarginXS, 0);
        token.ContentColor = global.ColorText;
        token.ExtraColor   = global.ColorText;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGDescriptionsTokens.EvaluateOwn(in context, global, token);
    }
}
