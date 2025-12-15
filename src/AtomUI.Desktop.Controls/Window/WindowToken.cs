using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public class WindowToken : AbstractControlDesignToken
{
    public const string ID = "Window";

    /// <summary>
    /// 窗口默认的背景色
    /// </summary>
    public Color DefaultBackground { get; set; }

    /// <summary>
    /// 窗口默认的前景色
    /// </summary>
    public Color DefaultForeground { get; set; }
    
    /// <summary>
    /// 窗口圆角，后期可能
    /// </summary>
    public CornerRadius CornerRadius { get; set; }
    
    public SolidColorBrush? SystemBarColor { get; set; }

    public WindowToken()
        : base("Window")
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        DefaultBackground = SharedToken.ColorBgContainer;
        DefaultForeground = SharedToken.ColorText;
        CornerRadius      = new CornerRadius(12);
        SystemBarColor    = new SolidColorBrush(SharedToken.ColorBgContainer);
    }
}