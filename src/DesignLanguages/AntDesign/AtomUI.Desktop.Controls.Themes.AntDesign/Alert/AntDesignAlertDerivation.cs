using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AlertToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignAlertDerivation : IControlDerivation<AntDesignToken, AlertToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<AlertToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        const double paddingHorizontal = 12; // Fixed value here.
        token.WithDescriptionIconSize = global.FontSizeHeading3;
        token.DefaultPadding          = new Thickness(paddingHorizontal, global.PaddingContentVerticalSM);
        token.WithDescriptionPadding  = new Thickness(global.PaddingContentHorizontalLG, global.UniformlyPaddingMD);

        token.IconDefaultMargin            = new Thickness(0, 0, global.UniformlyMarginXS, 0);
        token.IconWithDescriptionMargin    = new Thickness(0, 0, global.UniformlyMarginSM, 0);
        token.ExtraElementMargin           = new Thickness(global.UniformlyMarginXS, 0, 0, 0);
        token.DescriptionLabelMargin       = new Thickness(0, global.UniformlyMarginXS, 0, 0);

        token.CloseIconSize = global.FontSizeIcon;
        token.DefaultIconSize = global.FontSizeLG;
        token.SuccessMessageColor = global.ColorText;
        token.InfoMessageColor    = global.ColorText;
        token.WarningMessageColor = global.ColorText;
        token.ErrorMessageColor   = global.ColorText;
        token.TextFontWeight      = FontWeight.Normal;
        token.SuccessIconColor    = global.ColorSuccess;
        token.InfoIconColor       = global.ColorPrimary;
        token.WarningIconColor    = global.ColorWarning;
        token.ErrorIconColor      = global.ColorError;
    }
}
