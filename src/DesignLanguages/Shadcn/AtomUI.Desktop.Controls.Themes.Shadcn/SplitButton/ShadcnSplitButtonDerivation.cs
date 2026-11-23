using Avalonia;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SplitButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnSplitButtonDerivation : IControlDerivation<ShadcnToken, SplitButtonToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<SplitButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // The style's button radius ladder.
        global.BorderRadiusMD   = new CornerRadius(context.Language.ButtonRadius);
        global.BorderRadiusLG   = new CornerRadius(context.Language.ButtonRadiusLG);
        global.BorderRadiusSM   = new CornerRadius(context.Language.ButtonRadiusSM);
        token.InsetSeparator         = false;
        token.GutterToFlyout         = global.UniformlyMarginXXS;
        token.SolidSeparatorColor    = global.ColorPrimaryHover;
        token.DangerSeparatorColor   = global.ColorErrorHover;
        // Reserve a layout slot for the inset separator.
        token.InsetSeparator = true;
        // The upstream split example joins secondary buttons. On a primary fill, tint the seam with the paired foreground so
        // it stays understated against either polarity of the fill.
        token.SolidSeparatorColor  = ShadcnRecipes.WithAlpha(global.ColorTextLightSolid, 0x2E);
        token.DangerSeparatorColor = ShadcnRecipes.WithAlpha(global.ColorError, 0x38);
    }
}
