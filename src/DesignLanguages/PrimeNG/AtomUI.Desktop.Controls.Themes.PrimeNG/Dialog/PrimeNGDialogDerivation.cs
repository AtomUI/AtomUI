using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DialogToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGDialogDerivation : IControlDerivation<PrimeNGToken, DialogToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<DialogToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGDialogTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractDialogDerivation.DeriveShared(in context, global, token);
        token.HeaderPadding = PrimeNGAbstractDialogDerivation.CalculateHeaderPadding(token, global);
        token.MinHeight = PrimeNGAbstractDialogDerivation.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 200;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGDialogTokens.EvaluateOwn(in context, global, token);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.MinHeight = PrimeNGAbstractDialogDerivation.CalculateMinimumHeight(token, global, token.HeaderPadding);
    }
}
