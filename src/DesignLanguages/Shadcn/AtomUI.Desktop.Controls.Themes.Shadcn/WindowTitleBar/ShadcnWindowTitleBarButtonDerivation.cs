using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="WindowTitleBarButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnWindowTitleBarButtonDerivation : IControlDerivation<ShadcnToken, WindowTitleBarButtonToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<WindowTitleBarButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractTitleBarActionDerivation.DeriveShared(in context, global, token);
        token.CaptionButtonIconSize = global.IconSize;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);
        token.CaptionButtonCornerRadius = ShadcnAbstractTitleBarActionDerivation.CalculateCaptionCornerRadius(token.CaptionButtonIconSize, token.CaptionButtonPadding);
    }
}
