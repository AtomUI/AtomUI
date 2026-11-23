using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ResultToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGResultDerivation : IControlDerivation<PrimeNGToken, ResultToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ResultToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.HeaderFontSize    = global.FontSizeHeading3;
        token.SubHeaderFontSize = global.FontSize;
        token.StatusIconSize    = global.FontSizeHeading3 * 3;
        token.ExtraMargin       = new Thickness(0, global.UniformlyMargin, 0, 0);

        token.ImageWidth  = 250;
        token.ImageHeight = 295;

        token.ResultInfoIconColor    = global.ColorInfo;
        token.ResultSuccessIconColor = global.ColorSuccess;
        token.ResultWarningIconColor = global.ColorWarning;
        token.ResultErrorIconColor   = global.ColorError;

        token.ContentPadding    = new Thickness(global.UniformlyPadding * 2.5, global.UniformlyPaddingLG);
        token.ContentMargin     = new Thickness(0, global.UniformlyPaddingLG, 0, 0);
        token.StatusImageMargin = new Thickness(0, 0, 0, global.UniformlyMargin);
        token.HeaderMargin      = new Thickness(0, global.UniformlyMarginXS);
        token.FramePadding      = new Thickness(global.UniformlyPaddingLG * 2, global.UniformlyMarginXL);
    }
}
