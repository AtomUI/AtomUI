using AtomUI.Theme.TokenSystem;

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
    
    public SpaceToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GapSmallSize  = SharedToken.SpacingXS;
        GapMiddleSize = SharedToken.Spacing;
        GapLargeSize = SharedToken.SpacingLG;
    }
}