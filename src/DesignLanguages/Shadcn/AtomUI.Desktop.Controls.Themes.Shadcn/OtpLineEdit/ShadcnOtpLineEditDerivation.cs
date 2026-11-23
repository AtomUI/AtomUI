using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="OtpLineEditToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnOtpLineEditDerivation : IControlDerivation<ShadcnToken, OtpLineEditToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<OtpLineEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;

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
