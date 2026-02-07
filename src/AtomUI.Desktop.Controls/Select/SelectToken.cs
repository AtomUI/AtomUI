using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class SelectToken : AbstractControlDesignToken
{
    public const string ID = "Select";
    
    /// <summary>
    /// 多选标签背景色
    /// Background color of multiple tag
    /// </summary>
    public Color MultipleItemBg { get; set; }
    
    /// <summary>
    /// 多选标签高度
    /// Height of multiple tag
    /// </summary>
    public double MultipleItemHeight { get; set; }
    
    /// <summary>
    /// 小号多选标签高度
    /// Height of multiple tag with small size
    /// </summary>
    public double MultipleItemHeightSM { get; set; }
    
    /// <summary>
    /// 大号多选标签高度
    /// Height of multiple tag with large size
    /// </summary>
    public double MultipleItemHeightLG { get; set; }
    
    /// <summary>
    /// 多选框禁用背景
    /// Background color of multiple selector when disabled
    /// </summary>
    public Color MultipleSelectorBgDisabled { get; set; }
    
    /// <summary>
    /// 多选标签禁用文本颜色
    /// Text color of multiple tag when disabled
    /// </summary>
    public Color MultipleItemColorDisabled { get; set; }
    
    /// <summary>
    /// 选项选中时文本颜色
    /// Text color when option is selected
    /// </summary>
    public Color OptionSelectedColor { get; set; }
    
    /// <summary>
    /// 选项选中时文本字重
    /// Font weight when option is selected
    /// </summary>
    public FontWeight OptionSelectedFontWeight { get; set; }
    
    /// <summary>
    /// 选项选中时背景色
    /// Font weight when option is selected
    /// </summary>
    public Color OptionSelectedBg { get; set; }
    
    /// <summary>
    /// 选项激活态时背景色
    /// Background color when option is active
    /// </summary>
    public Color OptionActiveBg { get; set; }
    
    /// <summary>
    /// 选项内间距
    /// Padding of option
    /// </summary>
    public Thickness OptionPadding { get; set; }
    
    /// <summary>
    /// 选项字体大小
    /// Font size of option
    /// </summary>
    public double OptionFontSize { get; set; }
    
    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public double OptionHeight { get; set; }
    
    public Thickness SelectAffixPadding { get; set; }
    
    public Thickness FixedItemMargin { get; set; }
    
    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }
    
    /// <summary>
    /// 多选模式下的输入框内边距
    /// </summary>
    public Thickness MultiModePadding { get; set; }

    /// <summary>
    /// 多选模式下的小号输入框内边距
    /// </summary>
    public Thickness MultiModePaddingSM { get; set; }

    /// <summary>
    /// 多选模式下的大号输入框内边距
    /// </summary>
    public Thickness MultiModePaddingLG { get; set; }
    
    /// <summary>
    /// 输入框内边距
    /// </summary>
    public Thickness Padding { get; set; }

    /// <summary>
    /// 小号输入框内边距
    /// </summary>
    public Thickness PaddingSM { get; set; }

    /// <summary>
    /// 多选模式下的大号输入框内边距
    /// </summary>
    public Thickness PaddingLG { get; set; }
    
    public SelectToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        
        // Item height default use `controlHeight - 2 * paddingXXS`,
        // but some case `paddingXXS=0`.
        // Let's fallback it.
        double dblPaddingXXS      = SharedToken.UniformlyPaddingXXS * 2;
        double dblLineWidth       = SharedToken.LineWidth * 2;
        double multipleItemHeight = Math.Min(SharedToken.ControlHeight - dblPaddingXXS, SharedToken.ControlHeight - dblLineWidth);
        double multipleItemHeightSM = Math.Min(SharedToken.ControlHeightSM - dblPaddingXXS, SharedToken.ControlHeightSM - dblLineWidth);
        double multipleItemHeightLG = Math.Min(SharedToken.ControlHeightLG - dblPaddingXXS, SharedToken.ControlHeightLG - dblLineWidth);
        FixedItemMargin = new Thickness(Math.Floor(SharedToken.UniformlyPaddingXXS / 2));

        OptionSelectedColor      = SharedToken.ColorText;
        OptionSelectedFontWeight = SharedToken.FontWeightStrong;
        OptionSelectedBg         = SharedToken.ControlItemBgActive;
        OptionActiveBg           = SharedToken.ControlItemBgHover;
        OptionPadding            = new Thickness(SharedToken.ControlPaddingHorizontal, (SharedToken.ControlHeight - SharedToken.FontHeight) / 2);
        OptionFontSize =  SharedToken.FontSize;
        OptionHeight = SharedToken.ControlHeight;
        MultipleItemBg = SharedToken.ColorFillSecondary;
        MultipleItemHeight = multipleItemHeight - 2;
        MultipleItemHeightSM = multipleItemHeightSM + 4;
        MultipleItemHeightLG = multipleItemHeightLG;
        MultipleSelectorBgDisabled = SharedToken.ColorBgContainerDisabled;
        MultipleItemColorDisabled = SharedToken.ColorTextDisabled;
        SelectAffixPadding = SharedToken.PaddingXXS;
        
        PopupContentPadding = new Thickness(SharedToken.UniformlyPaddingXXS / 2);
        
        var lineWidth    = SharedToken.LineWidth;
        
        var multiPaddingVertical = Math.Round((SharedToken.ControlHeight - SharedToken.FontHeight) / 2 * 10) / 10 - lineWidth;
        var multiPaddingVerticalSM = Math.Round((SharedToken.ControlHeightSM - SharedToken.FontHeight) / 2 * 10) / 10 - lineWidth;
        var multiPaddingVerticalLG = Math.Ceiling((SharedToken.ControlHeightLG - SharedToken.FontHeightLG) / 2 * 10) / 10 -
                                     lineWidth;
        
        var multiPaddingRight = SharedToken.UniformlyPaddingSM - lineWidth;
        MultiModePadding = new Thickness(multiPaddingVertical, multiPaddingVertical, multiPaddingRight, multiPaddingVertical);
        
        var multiPaddingRightSM = SharedToken.ControlPaddingHorizontalSM - lineWidth;
        MultiModePaddingSM = new Thickness(multiPaddingVerticalSM, multiPaddingVerticalSM, multiPaddingRightSM, multiPaddingVerticalSM);
        
        var multiPaddingRightLG = SharedToken.ControlPaddingHorizontal - lineWidth;
        MultiModePaddingLG = new Thickness(multiPaddingVerticalLG, multiPaddingVerticalLG, multiPaddingRightLG, multiPaddingVerticalLG);
        
        Padding   = new Thickness(multiPaddingRight, multiPaddingVertical);
        PaddingSM = new Thickness(multiPaddingRightSM, multiPaddingVerticalSM);
        PaddingLG = new Thickness(multiPaddingRightLG,  multiPaddingVerticalLG);
    }
}