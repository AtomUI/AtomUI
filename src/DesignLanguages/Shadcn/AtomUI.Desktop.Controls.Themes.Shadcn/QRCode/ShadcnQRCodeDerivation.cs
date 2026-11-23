using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="QRCodeToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnQRCodeDerivation : IControlDerivation<ShadcnToken, QRCodeToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<QRCodeToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var colorBgContainer = global.ColorBgContainer;
        token.QRCodeTextColor           = global.ColorText;
        token.QRCodeMaskBackgroundColor = Color.FromArgb(244, colorBgContainer.R, colorBgContainer.G, colorBgContainer.B);
    }
}
