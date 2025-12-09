using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class TagToken : AbstractControlDesignToken
{
    public const string ID = "Tag";

    /// <summary>
    /// 默认背景色
    /// </summary>
    public Color DefaultBg { get; set; }

    /// <summary>
    /// 默认文字颜色
    /// </summary>
    public Color DefaultColor { get; set; }

    public double TagFontSize { get; set; }
    public double TagLineHeight { get; set; }
    public double TagIconSize { get; set; }
    public double TagCloseIconSize { get; set; }
    public Thickness TagPadding { get; set; }
    public Thickness TagTextPaddingInline { get; set; }
    public Color TagBorderlessBg { get; set; }

    public TagToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TagFontSize      = SharedToken.FontSizeSM;
        TagLineHeight    = SharedToken.FontHeightSM;
        TagCloseIconSize = SharedToken.IconSizeXS;
        TagIconSize      = SharedToken.FontSizeIcon;
        TagPadding       = new Thickness(SharedToken.SizeXS - 1, 0);
        // TODO 这个地方需要看看
        DefaultBg            = ColorUtils.OnBackground(SharedToken.ColorFillQuaternary, SharedToken.ColorBgContainer);
        TagBorderlessBg      = DefaultBg;
        DefaultColor         = SharedToken.ColorText;
        TagTextPaddingInline = new Thickness(SharedToken.UniformlyPaddingXXS, 0);
    }
}