using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ButtonSpinnerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnButtonSpinnerDerivation : IControlDerivation<ShadcnToken, ButtonSpinnerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ButtonSpinnerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractButtonSpinnerDerivation.DeriveShared(in context, global, token);
        token.ControlWidth = 90;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = ShadcnAbstractButtonSpinnerDerivation.CalculateFilledHandleBackground(token, global, token.HandleBg);
    }
}
