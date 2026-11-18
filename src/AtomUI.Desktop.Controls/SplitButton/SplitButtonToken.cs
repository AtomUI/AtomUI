using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SplitButtonToken : AbstractControlDesignToken
{
    public partial double GutterToFlyout { get; set; }

    /// <summary>
    /// 实心（Primary）形态接缝分隔色
    /// </summary>
    public partial Color SolidSeparatorColor { get; set; }

    /// <summary>
    /// 实心危险（Danger）形态接缝分隔色
    /// </summary>
    public partial Color DangerSeparatorColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GutterToFlyout         = EffectiveGlobalToken.UniformlyMarginXXS;
        SolidSeparatorColor    = EffectiveGlobalToken.ColorPrimaryHover;
        DangerSeparatorColor   = EffectiveGlobalToken.ColorErrorHover;
    }
}
