using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="OtpLineEditToken"/>.</summary>
[ControlTokenRule]
internal sealed class OtpLineEditTokenRule : IControlTokenRule<PrimeNGToken, OtpLineEditToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<OtpLineEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGOtpLineEditTokens.EvaluateGlobal(in context, global);

        token.CellWidth               = global.ControlHeight;
        token.CellWidthLG             = global.ControlHeightLG;
        token.CellWidthSM             = global.ControlHeightSM;
        token.CellGap                 = global.UniformlyPaddingXXS;
        // Ant Design draws separate cells.
        token.JoinCells = false;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGOtpLineEditTokens.EvaluateOwn(in context, global, token);
    }
}
