using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="QRCodeToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignQRCodeDerivation : IControlDerivation<AntDesignToken, QRCodeToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<QRCodeToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var colorBgContainer = global.ColorBgContainer;
        token.QRCodeTextColor           = global.ColorText;
        token.QRCodeMaskBackgroundColor = Color.FromArgb(244, colorBgContainer.R, colorBgContainer.G, colorBgContainer.B);
    }
}
