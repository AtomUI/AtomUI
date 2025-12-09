using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class AddOnDecoratedBoxToken : AbstractControlDesignToken
{
    public const string ID = "AddOnDecoratedBox";

    public AddOnDecoratedBoxToken()
        : base(ID)
    {
    }

    /// <summary>
    /// 输入框内边距
    /// </summary>
    public Thickness Padding { get; set; }

    /// <summary>
    /// 小号输入框内边距
    /// </summary>
    public Thickness PaddingSM { get; set; }

    /// <summary>
    /// 大号输入框内边距
    /// </summary>
    public Thickness PaddingLG { get; set; }

    /// <summary>
    /// 前/后置标签背景色
    /// </summary>
    public Color AddonBg { get; set; }

    /// <summary>
    /// 悬浮态边框色
    /// </summary>
    public Color HoverBorderColor { get; set; }

    /// <summary>
    /// 激活态边框色
    /// </summary>
    public Color ActiveBorderColor { get; set; }

    /// <summary>
    /// 激活态阴影
    /// </summary>
    public BoxShadow ActiveShadow { get; set; }

    /// <summary>
    /// 错误状态时激活态阴影
    /// </summary>
    public BoxShadow ErrorActiveShadow { get; set; }

    /// <summary>
    /// 警告状态时激活态阴影
    /// </summary>
    public BoxShadow WarningActiveShadow { get; set; }

    /// <summary>
    /// hover 状态时背景颜色
    /// </summary>
    public Color HoverBg { get; set; }

    /// <summary>
    /// 激活状态时背景颜色
    /// </summary>
    public Color ActiveBg { get; set; }

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

    /// <summary>
    /// 左边内部小组件的边距
    /// </summary>
    public Thickness LeftInnerAddOnMargin { get; set; }

    /// <summary>
    /// 右边内部小组件的边距
    /// </summary>
    public Thickness RightInnerAddOnMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var fontSize     = SharedToken.FontSize;
        var fontSizeLG   = SharedToken.FontSizeLG;
        var lineHeight   = SharedToken.RelativeLineHeight;
        var lineHeightLG = SharedToken.RelativeLineHeightLG;
        var lineWidth    = SharedToken.LineWidth;
        Padding = new Thickness(SharedToken.UniformlyPaddingSM - lineWidth,
            Math.Round((SharedToken.ControlHeight - fontSize * lineHeight) / 2 * 10) / 10 - lineWidth);
        PaddingSM = new Thickness(SharedToken.ControlPaddingHorizontalSM - lineWidth,
            Math.Round((SharedToken.ControlHeightSM - fontSize * lineHeight) / 2 * 10) / 10 - lineWidth);
        PaddingLG = new Thickness(SharedToken.ControlPaddingHorizontal - lineWidth,
            Math.Ceiling((SharedToken.ControlHeightLG - fontSizeLG * lineHeightLG) / 2 * 10) / 10 -
            lineWidth);
        AddOnPadding   = new Thickness(SharedToken.UniformlyPaddingSM, 0);
        AddOnPaddingSM = new Thickness(SharedToken.ControlPaddingHorizontalSM, 0);
        AddOnPaddingLG = new Thickness(SharedToken.ControlPaddingHorizontal, 0);

        AddonBg           = SharedToken.ColorFillAlter;
        ActiveBorderColor = SharedToken.ColorPrimary;
        HoverBorderColor  = SharedToken.ColorPrimaryHover;
        ActiveShadow = new BoxShadow
        {
            Spread = SharedToken.ControlOutlineWidth,
            Color  = SharedToken.ColorControlOutline
        };
        ErrorActiveShadow = new BoxShadow
        {
            Spread = SharedToken.ControlOutlineWidth,
            Color  = SharedToken.ColorErrorOutline
        };
        WarningActiveShadow = new BoxShadow
        {
            Spread = SharedToken.ControlOutlineWidth,
            Color  = SharedToken.ColorWarningOutline
        };
        HoverBg    = SharedToken.ColorBgContainer;
        ActiveBg   = SharedToken.ColorTransparent;

        LeftInnerAddOnMargin  = new Thickness(0, 0, SharedToken.UniformlyMarginXXS, 0);
        RightInnerAddOnMargin = new Thickness(SharedToken.UniformlyMarginXXS, 0, 0, 0);
    }
}