using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CheckBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignCheckBoxDerivation : IControlDerivation<AntDesignToken, CheckBoxToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CheckBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.CheckIndicatorSize        = global.ControlInteractiveSize;

        token.CheckedMarkSize           = token.CheckIndicatorSize * 0.6;
        token.IndicatorTristateMarkSize = global.FontSizeLG / 2;
        token.IndicatorHoverBorderColor = global.ColorPrimaryHover;
        token.TextMargin                = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS, 0);
    }
}
