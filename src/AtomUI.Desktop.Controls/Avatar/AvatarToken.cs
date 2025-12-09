using AtomUI.Theme.TokenSystem;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class AvatarToken : AbstractControlDesignToken
{
    public const string ID = "Avatar";

    /// <summary>
    /// 头像尺寸
    /// </summary>
    public double ContainerSize { get; set; }

    /// <summary>
    /// 大号头像尺寸
    /// </summary>
    public double ContainerSizeLG { get; set; }

    /// <summary>
    /// 小号头像尺寸
    /// </summary>
    public double ContainerSizeSM { get; set; }

    /// <summary>
    /// 头像文字大小
    /// </summary>
    public double TextFontSize { get; set; }

    /// <summary>
    /// 大号头像文字大小
    /// </summary>
    public double TextFontSizeLG { get; set; }

    /// <summary>
    /// 小号头像文字大小
    /// </summary>
    public double TextFontSizeSM { get; set; }

    /// <summary>
    /// 头像组间距
    /// </summary>
    public double GroupSpace { get; set; }

    /// <summary>
    /// 头像组重叠宽度
    /// </summary>
    public double GroupOverlapping { get; set; }

    /// <summary>
    /// 头像组边框颜色
    /// </summary>
    public Color GroupBorderColor { get; set; }

    // 内部使用别名
    public Color AvatarBg { get; set; }
    public Color AvatarColor { get; set; }

    public AvatarToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ContainerSize   = SharedToken.ControlHeight;
        ContainerSizeLG = SharedToken.ControlHeightLG;
        ContainerSizeSM = SharedToken.ControlHeightSM;
        TextFontSize    = Math.Round((SharedToken.FontSizeLG + SharedToken.FontSizeXL) / 2);
        TextFontSizeLG  = SharedToken.FontSizeHeading3;
        TextFontSizeSM  = SharedToken.FontSize;
        GroupSpace      = SharedToken.UniformlyMarginXXS;
        GroupOverlapping  = SharedToken.UniformlyMarginXS;
        GroupBorderColor  = SharedToken.ColorBorderBg;

        AvatarBg    = SharedToken.ColorTextPlaceholder;
        AvatarColor = SharedToken.ColorTextLightSolid;
    }
}