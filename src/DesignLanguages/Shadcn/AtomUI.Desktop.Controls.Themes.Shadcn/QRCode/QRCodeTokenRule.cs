using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="QRCodeToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class QRCodeTokenRule : IControlTokenRule<ShadcnToken, QRCodeToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<QRCodeToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var colorBgContainer = global.ColorBgContainer;
        // Ant Design formula kept as is: shadcn has no QR code, so the language has no values of its own.
        token.QRCodeTextColor           = global.ColorText;
        token.QRCodeMaskBackgroundColor = Color.FromArgb(244, colorBgContainer.R, colorBgContainer.G, colorBgContainer.B);
    }
}
