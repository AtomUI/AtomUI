using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="OtpLineEditToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class OtpLineEditTokenRule : IControlTokenRule<ShadcnToken, OtpLineEditToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<OtpLineEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        token.CellWidth               = global.ControlHeight;
        token.CellWidthLG             = global.ControlHeightLG;
        token.CellWidthSM             = global.ControlHeightSM;
        token.CellGap                 = global.UniformlyPaddingXXS;
        // Ant Design draws separate cells.
        token.JoinCells = false;
        // Luma joins the cells into one field.
        if (context.Variant.Style == ShadcnStyles.Luma)
        {
            token.JoinCells = true;
        }
    }
}
