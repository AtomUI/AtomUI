using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="OtpLineEditToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignOtpLineEditDerivation : IControlDerivation<AntDesignToken, OtpLineEditToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<OtpLineEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;

        token.CellWidth               = global.ControlHeight;
        token.CellWidthLG             = global.ControlHeightLG;
        token.CellWidthSM             = global.ControlHeightSM;
        token.CellGap                 = global.UniformlyPaddingXXS;
        // Ant Design draws separate cells.
        token.JoinCells = false;
    }
}
