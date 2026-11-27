using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="OtpLineEditToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class OtpLineEditTokenRule : IControlTokenRule<AntDesignToken, OtpLineEditToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<OtpLineEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        token.CellWidth               = global.ControlHeight;
        token.CellWidthLG             = global.ControlHeightLG;
        token.CellWidthSM             = global.ControlHeightSM;
        token.CellGap                 = global.UniformlyPaddingXXS;
        // Ant Design draws separate cells.
        token.JoinCells = false;
    }
}
