using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="FormToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGFormDerivation : IControlDerivation<PrimeNGToken, FormToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<FormToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.LabelRequiredMarkColor = global.ColorError;
        token.LabelColor             = global.ColorTextHeading;
        token.LabelFontSize          = global.FontSize;
        token.LabelColonMargin       = new Thickness(global.UniformlyMarginXXS / 2, 0, global.UniformlyMarginXS, 0);
        token.FormItemSpacing        = global.SpacingLG;
        token.VerticalLabelPadding   = new Thickness(0, 0, 0, global.UniformlyPaddingXS);
        token.InlineItemSpacing      = global.Spacing;
        token.VerticalLabelMargin    = default;
    }
}
