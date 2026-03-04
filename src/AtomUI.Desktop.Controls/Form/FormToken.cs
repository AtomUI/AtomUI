using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class FormToken : AbstractControlDesignToken
{
    public const string ID = "Form";

    public FormToken()
        : base(ID)
    {
    }
    
    /// <summary>
    /// 必填项标记颜色
    /// Required mark color
    /// </summary>
    public Color LabelRequiredMarkColor { get; set; }
    
    /// <summary>
    /// 标签颜色
    /// Label color
    /// </summary>
    public Color LabelColor { get; set; }
    
    /// <summary>
    /// 标签字体大小
    /// Label font size
    /// </summary>
    public double LabelFontSize { get; set; }
    
    /// <summary>
    /// 标签冒号间距
    /// Label colon margin-inline
    /// </summary>
    public Thickness LabelColonMargin { get; set; }
    
    /// <summary>
    /// 表单项间距
    /// Form item margin bottom
    /// </summary>
    public Thickness ItemMarginBottom { get; set; }
    
    /// <summary>
    /// 行内布局表单项间距
    /// Inline layout form item spacing
    /// </summary>
    public double InlineItemSpacing { get; set; }
    
    /// <summary>
    /// 垂直布局标签内边距
    /// Vertical layout label padding
    /// </summary>
    public Thickness VerticalLabelPadding { get; set; }
    
    /// <summary>
    /// 垂直布局标签外边距
    /// Vertical layout label margin
    /// </summary>
    public Thickness VerticalLabelMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        LabelRequiredMarkColor = SharedToken.ColorError;
        LabelColor             = SharedToken.ColorTextHeading;
        LabelFontSize          = SharedToken.FontSize;
        LabelColonMargin       = new Thickness(SharedToken.UniformlyMarginXXS / 2, 0, SharedToken.UniformlyMarginXS, 0);
        ItemMarginBottom       = new Thickness(0, 0, 0, SharedToken.UniformlyMarginLG);
        VerticalLabelPadding   = new Thickness(0, 0, 0, SharedToken.UniformlyPaddingXS);
        InlineItemSpacing = SharedToken.Spacing;
        VerticalLabelMargin    = default;
    }
}