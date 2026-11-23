using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MessageBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGMessageBoxDerivation : IControlDerivation<PrimeNGToken, MessageBoxToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<MessageBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGMessageBoxTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractDialogDerivation.DeriveShared(in context, global, token);
        token.HeaderPadding = PrimeNGAbstractDialogDerivation.CalculateHeaderPadding(token, global);
        token.MinHeight = PrimeNGAbstractDialogDerivation.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 410;
        token.StyleIconSize = global.SizeLG * 1.2;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMessageBoxTokens.EvaluateOwn(in context, global, token);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.MinHeight = PrimeNGAbstractDialogDerivation.CalculateMinimumHeight(token, global, token.HeaderPadding);
    }
}
