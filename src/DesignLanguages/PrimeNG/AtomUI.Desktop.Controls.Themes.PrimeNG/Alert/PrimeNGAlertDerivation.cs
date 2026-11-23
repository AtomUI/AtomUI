using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AlertToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGAlertDerivation : IControlDerivation<PrimeNGToken, AlertToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<AlertToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides and the severity colours, read by the formulas below.
        var statuses = PrimeNGAlertTokens.EvaluateGlobal(in context, global);
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
        token.SuccessIconColor    = statuses.Success;
        token.InfoIconColor       = statuses.Primary;
        token.WarningIconColor    = statuses.Warning;
        token.ErrorIconColor      = statuses.Error;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAlertTokens.EvaluateOwn(in context, global, token, statuses);
    }
}
