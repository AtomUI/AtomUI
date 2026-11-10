using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractPaginationToken : AbstractControlDesignToken
{
    public double ItemSize { get; set; }
    public double ItemSizeSM { get; set; }
    public Color ItemActiveColorDisabled { get; set; }
    public double PaginationLayoutSpacing { get; set; }
    public double PaginationLayoutMiniSpacing { get; set; }
    public double PaginationQuickJumperInputWidth { get; set; }
    public double PaginationMiniQuickJumperInputWidth { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemSize                  = EffectiveGlobalToken.ControlHeight;
        ItemSizeSM                = EffectiveGlobalToken.ControlHeightSM;
        ItemActiveColorDisabled   = EffectiveGlobalToken.ColorTextDisabled;
        PaginationLayoutSpacing             = EffectiveGlobalToken.UniformlyMarginXS;
        PaginationLayoutMiniSpacing         = EffectiveGlobalToken.UniformlyMarginXXS / 2;
        PaginationQuickJumperInputWidth     = EffectiveGlobalToken.ControlHeightLG * 1.25;
        PaginationMiniQuickJumperInputWidth = EffectiveGlobalToken.ControlHeightLG * 1.1;
    }
}
