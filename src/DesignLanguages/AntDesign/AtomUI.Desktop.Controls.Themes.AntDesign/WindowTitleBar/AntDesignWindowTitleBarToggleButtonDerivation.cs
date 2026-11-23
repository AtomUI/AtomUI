using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="WindowTitleBarToggleButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignWindowTitleBarToggleButtonDerivation : IControlDerivation<AntDesignToken, WindowTitleBarToggleButtonToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<WindowTitleBarToggleButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractTitleBarActionDerivation.DeriveShared(in context, global, token);
        token.CaptionButtonIconSize = global.IconSize;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);
        token.CaptionButtonCornerRadius = AntDesignAbstractTitleBarActionDerivation.CalculateCaptionCornerRadius(token.CaptionButtonIconSize, token.CaptionButtonPadding);
    }
}
