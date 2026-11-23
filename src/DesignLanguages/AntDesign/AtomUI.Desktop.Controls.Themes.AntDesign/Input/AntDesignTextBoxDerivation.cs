using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TextBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTextBoxDerivation : IControlDerivation<AntDesignToken, TextBoxToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TextBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;

        var fontSize     = global.FontSize;
        var fontSizeSM   = global.FontSizeSM;
        var fontSizeLG   = global.FontSizeLG;
        var lineHeight   = global.RelativeLineHeight;
        var lineHeightSM = global.RelativeLineHeightSM;
        var lineHeightLG = global.RelativeLineHeightLG;
        var lineWidth    = global.LineWidth;

        token.ContentPadding = new Thickness(global.UniformlyPaddingSM - lineWidth,
            Math.Round((global.ControlHeight - fontSize * lineHeight) / 2 * 10) / 10 - lineWidth);
        token.ContentPaddingSM = new Thickness(global.ControlPaddingHorizontalSM - lineWidth,
            Math.Round((global.ControlHeightSM - fontSizeSM * lineHeightSM) / 2 * 10) / 10 -
            lineWidth);
        token.ContentPaddingLG = new Thickness(global.ControlPaddingHorizontal - lineWidth,
            Math.Ceiling((global.ControlHeightLG - fontSizeLG * lineHeightLG) / 2 * 10) / 10 -
            lineWidth);
    }
}
