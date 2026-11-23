using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="WindowTitleBarToggleButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGWindowTitleBarToggleButtonDerivation : IControlDerivation<PrimeNGToken, WindowTitleBarToggleButtonToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<WindowTitleBarToggleButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractTitleBarActionDerivation.DeriveShared(in context, global, token);
        token.CaptionButtonIconSize = global.IconSize;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);
        token.CaptionButtonCornerRadius = PrimeNGAbstractTitleBarActionDerivation.CalculateCaptionCornerRadius(token.CaptionButtonIconSize, token.CaptionButtonPadding);
    }
}
