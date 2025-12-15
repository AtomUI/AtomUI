using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class RadioButtonToken : AbstractControlDesignToken
{
    public const string ID = "RadioButton";

    /// <summary>
    /// 单选框大小，除去文字部分的
    /// </summary>
    public double RadioSize { get; set; }

    /// <summary>
    /// 单选框圆点大小
    /// </summary>
    public double DotSize { get; set; }

    /// <summary>
    /// 单选框圆点禁用颜色
    /// </summary>
    public Color DotColorDisabled { get; set; }

    /// 内部使用
    public Color RadioColor { get; set; }

    public Color RadioBgColor { get; set; }

    public double DotPadding { get; set; }

    public Thickness TextMargin { get; set; }

    public RadioButtonToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var lineWidth        = SharedToken.LineWidth;
        var fontSizeLG       = SharedToken.FontSizeLG;
        var colorPrimary     = SharedToken.ColorPrimary;
        var colorWhite       = SharedToken.ColorWhite;

        var dotPadding = 4; // 魔术值，需要看有没有好办法消除
        var radioSize  = fontSizeLG;

        var radioDotSize = radioSize - (dotPadding + lineWidth) * 2;

        DotPadding       = dotPadding;
        RadioSize        = radioSize;
        DotSize          = radioDotSize;
        DotColorDisabled = SharedToken.ColorTextDisabled;

        // internal
        RadioColor   = colorWhite;
        RadioBgColor = colorPrimary;
        TextMargin   = new Thickness(SharedToken.UniformlyMarginXXS, 0, 0, 0);
    }
}