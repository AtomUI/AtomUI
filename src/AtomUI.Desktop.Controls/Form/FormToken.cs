using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class FormToken : AbstractControlDesignToken
{
    
    public FormToken()

    {
    }
    
    /// <summary>
    /// 必填项标记颜色
    /// Required mark color
    /// </summary>
    public partial Color LabelRequiredMarkColor { get; set; }
    
    /// <summary>
    /// 标签颜色
    /// Label color
    /// </summary>
    public partial Color LabelColor { get; set; }
    
    /// <summary>
    /// 标签字体大小
    /// Label font size
    /// </summary>
    public partial double LabelFontSize { get; set; }
    
    /// <summary>
    /// 标签冒号间距
    /// Label colon margin-inline
    /// </summary>
    public partial Thickness LabelColonMargin { get; set; }
    
    /// <summary>
    /// 表单项间距
    /// Form item spacing
    /// </summary>
    public partial double FormItemSpacing { get; set; }
    
    /// <summary>
    /// 行内布局表单项间距
    /// Inline layout form item spacing
    /// </summary>
    public partial double InlineItemSpacing { get; set; }
    
    /// <summary>
    /// 垂直布局标签内边距
    /// Vertical layout label padding
    /// </summary>
    public partial Thickness VerticalLabelPadding { get; set; }
    
    /// <summary>
    /// 垂直布局标签外边距
    /// Vertical layout label margin
    /// </summary>
    public partial Thickness VerticalLabelMargin { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        LabelRequiredMarkColor = EffectiveGlobalToken.ColorError;
        LabelColor             = EffectiveGlobalToken.ColorTextHeading;
        LabelFontSize          = EffectiveGlobalToken.FontSize;
        LabelColonMargin       = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS / 2, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
        FormItemSpacing        = EffectiveGlobalToken.SpacingLG;
        VerticalLabelPadding   = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyPaddingXS);
        InlineItemSpacing      = EffectiveGlobalToken.Spacing;
        VerticalLabelMargin    = default;
    }
    
}
