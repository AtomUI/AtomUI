using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractPaginationToken : AbstractControlDesignToken
{
    public partial double ItemSize { get; set; }
    public partial double ItemSizeSM { get; set; }
    public partial Color ItemActiveColorDisabled { get; set; }
    public partial double PaginationLayoutSpacing { get; set; }
    public partial double PaginationLayoutMiniSpacing { get; set; }
    public partial double PaginationQuickJumperInputWidth { get; set; }
    public partial double PaginationMiniQuickJumperInputWidth { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
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
