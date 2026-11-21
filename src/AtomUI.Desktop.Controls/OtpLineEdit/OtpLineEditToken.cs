using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class OtpLineEditToken : AbstractControlDesignToken
{

    public OtpLineEditToken()

    {
    }

    public partial double CellWidth { get; set; }

    public partial double CellWidthLG { get; set; }

    public partial double CellWidthSM { get; set; }

    public partial double CellGap { get; set; }

    /// <summary>Joins adjacent cells into one frame when no separator is provided.</summary>
    public partial bool JoinCells { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        CellWidth               = EffectiveGlobalToken.ControlHeight;
        CellWidthLG             = EffectiveGlobalToken.ControlHeightLG;
        CellWidthSM             = EffectiveGlobalToken.ControlHeightSM;
        CellGap                 = EffectiveGlobalToken.UniformlyPaddingXXS;
    }

}
