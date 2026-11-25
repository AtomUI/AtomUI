using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="QRCodeToken"/>.</summary>
[ControlTokenRule]
internal sealed class QRCodeTokenRule : IControlTokenRule<PrimeNGToken, QRCodeToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<QRCodeToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGQRCodeTokens.EvaluateGlobal(in context, global);
        var colorBgContainer = global.ColorBgContainer;
        token.QRCodeTextColor           = global.ColorText;
        token.QRCodeMaskBackgroundColor = Color.FromArgb(244, colorBgContainer.R, colorBgContainer.G, colorBgContainer.B);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGQRCodeTokens.EvaluateOwn(in context, global, token);
    }
}
