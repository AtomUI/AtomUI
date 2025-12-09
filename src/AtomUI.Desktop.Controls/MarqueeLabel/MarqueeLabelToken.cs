using AtomUI.Theme.TokenSystem;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class MarqueeLabelToken : AbstractControlDesignToken
{
    public const string ID = "MarqueeLabel";

    /// <summary>
    /// 周期这件的间隔
    /// </summary>
    public double CycleSpace { get; set; }

    /// <summary>
    /// 默认速度，像素每秒
    /// </summary>
    public double DefaultSpeed { get; set; }

    public MarqueeLabelToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        CycleSpace   = 200;
        DefaultSpeed = 150;
    }
}