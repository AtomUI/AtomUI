using AtomUI.Theme.TokenSystem;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class TransferToken : AbstractControlDesignToken
{
    public const string ID = "Transfer";

    public TransferToken()
        : base(ID)
    {
    }
    
    /// <summary>
    /// 列表宽度
    /// Width of list
    /// </summary>
    public double ListWidth { get; set; }
    
    /// <summary>
    /// 大号列表宽度
    /// Width of large list
    /// </summary>
    public double ListWidthLG { get; set; }
    
    /// <summary>
    /// 列表高度
    /// Height of list
    /// </summary>
    public double ListHeight { get; set; }
    
    /// <summary>
    /// 大号列表高度
    /// Height of large list
    /// </summary>
    public double ListHeightLG { get; set; }
    
    /// <summary>
    /// 列表项高度
    /// Height of list item
    /// </summary>
    public double ItemHeight { get; set; }

    /// <summary>
    /// 列表项纵向内边距
    /// Vertical padding of list item
    /// </summary>
    public Thickness ItemPadding { get; set; }
    
    /// <summary>
    /// 顶部高度
    /// Height of header
    /// </summary>
    public double HeaderHeight { get; set; }
    
    /// <summary>
    /// 穿梭框头部内间距 
    /// </summary>
    public Thickness HeaderPadding { get; set; }
    
    /// <summary>
    /// 穿梭框头部边框
    /// </summary>
    public Thickness HeaderBorderThickness { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ListWidth    = 180;
        ListHeight   = 200;
        ListHeightLG = 200 + SharedToken.ControlHeight + SharedToken.UniformlyMarginXS * 2;
        ListWidthLG  = 250;
        HeaderHeight = SharedToken.ControlHeightLG;
        ItemHeight   = SharedToken.ControlHeight;
        ItemPadding  = new Thickness(0, (SharedToken.ControlHeight - SharedToken.FontHeight) / 2);
        HeaderPadding = new Thickness(SharedToken.UniformlyPaddingSM,
            Math.Ceiling((SharedToken.ControlHeightLG - SharedToken.LineWidth - SharedToken.FontHeight) / 2));
        HeaderBorderThickness = new Thickness(0, 0, 0, SharedToken.LineWidth);
    }
}