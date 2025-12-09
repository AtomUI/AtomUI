using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class BreadcrumbToken : AbstractControlDesignToken
{
    public const string ID = "Breadcrumb";

    /// <summary>
    /// 图标大小
    /// </summary>
    public double IconSize { get; set; }

    /// <summary>
    /// 面包屑项文字颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 最后一项文字颜色
    /// </summary>
    public Color LastItemColor { get; set; }

    /// <summary>
    /// 链接文字颜色
    /// </summary>
    public Color LinkColor { get; set; }

    /// <summary>
    /// 链接文字悬浮颜色
    /// </summary>
    public Color LinkHoverColor { get; set; }

    /// <summary>
    /// 链接文字悬浮背景颜色
    /// </summary>
    public Color LinkHoverBgColor { get; set; }

    /// <summary>
    /// BreadcrumbItem外面Border的Padding
    /// </summary>
    public Thickness BreadcrumbItemContentPadding { get; set; }

    /// <summary>
    /// 分隔符颜色
    /// </summary>
    public Color SeparatorColor { get; set; }

    /// <summary>
    /// 分隔符外间距
    /// </summary>
    public Thickness SeparatorMargin { get; set; }

    public BreadcrumbToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        IconSize                    = SharedToken.IconSize;
        ItemColor                   = SharedToken.ColorTextDescription;
        LastItemColor               = SharedToken.ColorText;
        LinkColor                   = SharedToken.ColorTextDescription;
        LinkHoverColor              = SharedToken.ColorText;
        LinkHoverBgColor            = SharedToken.ColorBgTextHover;
        SeparatorColor              = SharedToken.ColorTextDescription;
        SeparatorMargin             = new Thickness(SharedToken.UniformlyMarginXXS, 0);
        BreadcrumbItemContentPadding = new Thickness(SharedToken.UniformlyPaddingXXS, SharedToken.UniformlyPaddingXXS,
            SharedToken.UniformlyPaddingXXS, SharedToken.UniformlyPaddingXXS);
    }
}