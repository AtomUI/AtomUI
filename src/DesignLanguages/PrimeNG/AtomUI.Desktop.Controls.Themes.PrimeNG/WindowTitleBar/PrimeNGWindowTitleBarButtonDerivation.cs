using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="WindowTitleBarButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGWindowTitleBarButtonDerivation : IControlDerivation<PrimeNGToken, WindowTitleBarButtonToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<WindowTitleBarButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractTitleBarActionDerivation.DeriveShared(in context, global, token);
        token.CaptionButtonIconSize = global.IconSize;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);
        token.CaptionButtonCornerRadius = PrimeNGAbstractTitleBarActionDerivation.CalculateCaptionCornerRadius(token.CaptionButtonIconSize, token.CaptionButtonPadding);
    }
}
