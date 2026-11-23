using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="NumericUpDownToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnNumericUpDownDerivation : IControlDerivation<ShadcnToken, NumericUpDownToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<NumericUpDownToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractButtonSpinnerDerivation.DeriveShared(in context, global, token);
        token.InputFontSizeSM = global.FontSize;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = ShadcnAbstractButtonSpinnerDerivation.CalculateFilledHandleBackground(token, global, token.HandleBg);
    }
}
