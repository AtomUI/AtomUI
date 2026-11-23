using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="PopupConfirmToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignPopupConfirmDerivation : IControlDerivation<AntDesignToken, PopupConfirmToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<PopupConfirmToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.ButtonSpacing          = global.UniformlyMarginXS;
        token.IconMargin             = new Thickness(0, global.UniformlyMarginXS / 2, global.UniformlyMarginXS, 0);
        token.ContentContainerMargin = new Thickness(0, 0, 0, global.UniformlyMarginXS);
        token.TitleMargin            = new Thickness(0, 0, 0, global.UniformlyMarginXS);
        token.ButtonContainerMargin  = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
