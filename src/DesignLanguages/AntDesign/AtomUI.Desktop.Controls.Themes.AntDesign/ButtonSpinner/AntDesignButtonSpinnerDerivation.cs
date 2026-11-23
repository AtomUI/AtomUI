using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ButtonSpinnerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignButtonSpinnerDerivation : IControlDerivation<AntDesignToken, ButtonSpinnerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ButtonSpinnerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractButtonSpinnerDerivation.DeriveShared(in context, global, token);
        token.ControlWidth = 90;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = AntDesignAbstractButtonSpinnerDerivation.CalculateFilledHandleBackground(token, global, token.HandleBg);
    }
}
