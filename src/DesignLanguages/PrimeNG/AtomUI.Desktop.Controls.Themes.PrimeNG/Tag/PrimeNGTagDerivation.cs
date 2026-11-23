using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TagToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTagDerivation : IControlDerivation<PrimeNGToken, TagToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TagToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides and the status colours, read by the formulas below.
        var statuses = PrimeNGTagTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractTagDerivation.DeriveShared(in context, global, token);
        token.TagCloseIconSize = global.IconSizeXS;
        token.DefaultBg        = ColorUtils.OnBackground(global.ColorFillTertiary, global.ColorBgContainer);
        token.DefaultColor     = global.ColorText;
        token.FilledStatusSolid = false;
        token.SuccessColor      = statuses.Success;
        token.InfoColor         = statuses.Info;
        token.WarningColor      = statuses.Warning;
        token.ErrorColor        = statuses.Error;
        token.SolidTextColor   = ColorUtils.IsBright(global.ColorBgSolid, Colors.White)
            ? Colors.Black
            : Colors.White;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTagTokens.EvaluateOwn(in context, global, token);
    }
}
