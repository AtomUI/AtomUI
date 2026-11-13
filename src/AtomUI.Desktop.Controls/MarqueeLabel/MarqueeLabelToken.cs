using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MarqueeLabelToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 周期这件的间隔
    /// </summary>
    public partial double CycleSpace { get; set; }

    /// <summary>
    /// 默认速度，像素每秒
    /// </summary>
    public partial double DefaultSpeed { get; set; }

    public MarqueeLabelToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        CycleSpace   = 200;
        DefaultSpeed = 150;
    }
    
}
