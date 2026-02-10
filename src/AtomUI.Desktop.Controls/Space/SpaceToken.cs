using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class SpaceToken : AbstractControlDesignToken
{
    public const string ID = "Space";
    
    /// <summary>
    /// 小间距尺寸
    /// </summary>
    public double GapSmallSize  { get; set; }
    /// <summary>
    /// 中等间距尺寸
    /// </summary>
    public double GapMiddleSize  { get; set; }
    
    /// <summary>
    /// 大间距尺寸
    /// </summary>
    public double GapLargeSize  { get; set; }
    
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
    
    public SpaceToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GapSmallSize  = SharedToken.SpacingXS;
        GapMiddleSize = SharedToken.Spacing;
        GapLargeSize  = SharedToken.SpacingLG;
        AddonBg       = SharedToken.ColorFillAlter;
        
        AddOnPadding   = new Thickness(SharedToken.UniformlyPaddingSM, 0);
        AddOnPaddingSM = new Thickness(SharedToken.ControlPaddingHorizontalSM, 0);
        AddOnPaddingLG = new Thickness(SharedToken.ControlPaddingHorizontal, 0);
    }
}