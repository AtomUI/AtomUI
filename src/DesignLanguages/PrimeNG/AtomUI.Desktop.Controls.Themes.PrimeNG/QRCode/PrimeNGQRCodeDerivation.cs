using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="QRCodeToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGQRCodeDerivation : IControlDerivation<PrimeNGToken, QRCodeToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<QRCodeToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGQRCodeTokens.EvaluateGlobal(in context, global);
        var colorBgContainer = global.ColorBgContainer;
        token.QRCodeTextColor           = global.ColorText;
        token.QRCodeMaskBackgroundColor = Color.FromArgb(244, colorBgContainer.R, colorBgContainer.G, colorBgContainer.B);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGQRCodeTokens.EvaluateOwn(in context, global, token);
    }
}
