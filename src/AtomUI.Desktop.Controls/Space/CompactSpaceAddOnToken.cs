using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class CompactSpaceAddOnToken : AbstractControlDesignToken
{
    /// <summary>
    /// Add On 背景色
    /// </summary>
    public Color AddonBg { get; set; }

    /// <summary>
    /// AddOn 内边距
    /// </summary>
    public Thickness AddOnPadding { get; set; }

    /// <summary>
    /// AddOn 小号内边距
    /// </summary>
    public Thickness AddOnPaddingSM { get; set; }

    /// <summary>
    /// AddOn 大号内边距
    /// </summary>
    public Thickness AddOnPaddingLG { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        AddonBg       = EffectiveGlobalToken.ColorFillAlter;

        AddOnPadding   = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, 0);
        AddOnPaddingSM = new Thickness(EffectiveGlobalToken.ControlPaddingHorizontalSM, 0);
        AddOnPaddingLG = new Thickness(EffectiveGlobalToken.ControlPaddingHorizontal, 0);
    }
}
