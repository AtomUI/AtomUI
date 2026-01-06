using AtomUI.Theme.TokenSystem;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class FlyoutHostToken : AbstractControlDesignToken
{
    public const string ID = "FlyoutHost";
    
    /// <summary>
    /// 默认 Popup 和 PlacementTarget 的间距
    /// </summary>
    public double MarginToAnchor { get; set; }

    public FlyoutHostToken()
        : base(ID)
    {
    }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        MarginToAnchor = SharedToken.UniformlyMarginXXS;
    }
}