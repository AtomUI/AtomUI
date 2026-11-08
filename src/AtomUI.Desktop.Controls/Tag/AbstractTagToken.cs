using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens shared by <see cref="Tag"/> and <see cref="CheckableTag"/> (antd Tag <c>tagFontSize</c>,
/// <c>tagLineHeight</c>, <c>tagIconSize</c> and the tag paddings). Each control has its own terminal Token, so
/// <c>Controls[Tag]</c> and <c>Controls[CheckableTag]</c> compute and override these values independently.
/// </summary>
[ControlDesignToken]
internal abstract class AbstractTagToken : AbstractControlDesignToken
{
    /// <summary>
    /// 标签字号
    /// Font size of the tag text
    /// </summary>
    public double TagFontSize { get; set; }

    /// <summary>
    /// 标签行高
    /// Line height of the tag text
    /// </summary>
    public double TagLineHeight { get; set; }

    /// <summary>
    /// 标签图标尺寸
    /// Size of the tag icon
    /// </summary>
    public double TagIconSize { get; set; }

    /// <summary>
    /// 标签内间距
    /// Padding of the tag
    /// </summary>
    public Thickness TagPadding { get; set; }

    /// <summary>
    /// 标签文字与图标之间的间距
    /// Inline gap between the tag text and its icon
    /// </summary>
    public Thickness TagTextPaddingInline { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TagFontSize          = EffectiveGlobalToken.FontSizeSM;
        TagLineHeight        = EffectiveGlobalToken.FontHeightSM;
        TagIconSize          = EffectiveGlobalToken.FontSizeIcon;
        TagPadding           = new Thickness(EffectiveGlobalToken.SizeXS - 1, 0);
        TagTextPaddingInline = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0);
    }
}
