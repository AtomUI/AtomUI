using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="QRCodeToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class QRCodeTokenRule : IControlTokenRule<AntDesignToken, QRCodeToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<QRCodeToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var colorBgContainer = global.ColorBgContainer;
        token.QRCodeTextColor           = global.ColorText;
        token.QRCodeMaskBackgroundColor = Color.FromArgb(244, colorBgContainer.R, colorBgContainer.G, colorBgContainer.B);
    }
}
