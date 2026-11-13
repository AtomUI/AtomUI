using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractButtonVariantToken : AbstractControlDesignToken
{
    /// <summary>
    /// 危险按钮文本颜色
    /// </summary>
    public partial Color DangerColor { get; set; }
    /// <summary>
    /// 危险按钮阴影
    /// </summary>
    public partial BoxShadows DangerShadow { get; set; }
    /// <summary>
    /// 默认按钮激活态背景色
    /// </summary>
    public partial Color DefaultActiveBg { get; set; }
    /// <summary>
    /// 默认按钮激活态边框颜色
    /// </summary>
    public partial Color DefaultActiveBorderColor { get; set; }
    /// <summary>
    /// 默认按钮激活态文字颜色
    /// </summary>
    public partial Color DefaultActiveColor { get; set; }
    /// <summary>
    /// 默认按钮背景色
    /// </summary>
    public partial Color DefaultBg { get; set; }
    /// <summary>
    /// 默认按钮边框颜色
    /// </summary>
    public partial Color DefaultBorderColor { get; set; }
    /// <summary>
    /// 默认按钮文本颜色
    /// </summary>
    public partial Color DefaultColor { get; set; }
    /// <summary>
    /// 默认幽灵按钮边框颜色
    /// </summary>
    public partial Color DefaultGhostBorderColor { get; set; }
    /// <summary>
    /// 默认幽灵按钮文本颜色
    /// </summary>
    public partial Color DefaultGhostColor { get; set; }
    /// <summary>
    /// 默认按钮悬浮态背景色
    /// </summary>
    public partial Color DefaultHoverBg { get; set; }
    /// <summary>
    /// 默认按钮悬浮态边框颜色
    /// </summary>
    public partial Color DefaultHoverBorderColor { get; set; }
    /// <summary>
    /// 默认按钮悬浮态文本颜色
    /// </summary>
    public partial Color DefaultHoverColor { get; set; }
    /// <summary>
    /// 默认按钮阴影
    /// </summary>
    public partial BoxShadows DefaultShadow { get; set; }
    /// <summary>
    /// 幽灵按钮背景色
    /// </summary>
    public partial Color GhostBg { get; set; }
    /// <summary>
    /// 链接按钮悬浮态背景色
    /// </summary>
    public partial Color LinkHoverBg { get; set; }
    /// <summary>
    /// 主要按钮文本颜色
    /// </summary>
    public partial Color PrimaryColor { get; set; }
    /// <summary>
    /// 主要按钮阴影
    /// </summary>
    public partial BoxShadows PrimaryShadow { get; set; }
    /// <summary>
    /// 默认实心按钮的文本色
    /// Default text color for solid buttons.
    /// </summary>
    public partial Color SolidTextColor { get; set; }
    /// <summary>
    /// 文本按钮悬浮态背景色
    /// </summary>
    public partial Color TextHoverBg { get; set; }
    /// <summary>
    /// 默认文本按钮激活态文字颜色
    /// Default text color for text buttons on active
    /// </summary>
    public partial Color TextTextActiveColor { get; set; }
    /// <summary>
    /// 默认文本按钮的文本色
    /// Default text color for text buttons on hover
    /// </summary>
    public partial Color TextTextColor { get; set; }
    /// <summary>
    /// 默认文本按钮悬浮态文本颜色
    /// Default text color for text buttons on hover
    /// </summary>
    public partial Color TextTextHoverColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var controlOutlineWidth = EffectiveGlobalToken.ControlOutlineWidth;
        DefaultShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = EffectiveGlobalToken.ColorControlOutline
        });

        PrimaryShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = EffectiveGlobalToken.ColorControlOutline
        });

        DangerShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = EffectiveGlobalToken.ColorErrorOutline
        });


        PrimaryColor            = EffectiveGlobalToken.ColorTextLightSolid;
        DangerColor             = EffectiveGlobalToken.ColorTextLightSolid;
        DefaultGhostColor       = EffectiveGlobalToken.ColorBgContainer;
        GhostBg                 = Colors.Transparent;
        DefaultGhostBorderColor = EffectiveGlobalToken.ColorBgContainer;

        LinkHoverBg                = Colors.Transparent;
        TextHoverBg                = EffectiveGlobalToken.ColorFillTertiary;
        DefaultColor               = EffectiveGlobalToken.ColorText;
        DefaultBg                  = EffectiveGlobalToken.ColorBgContainer;
        DefaultBorderColor         = EffectiveGlobalToken.ColorBorder;
        DefaultHoverBg             = EffectiveGlobalToken.ColorBgContainer;
        DefaultHoverColor          = EffectiveGlobalToken.ColorPrimaryHover;
        DefaultHoverBorderColor    = EffectiveGlobalToken.ColorPrimaryHover;
        DefaultActiveBg            = EffectiveGlobalToken.ColorBgContainer;
        DefaultActiveColor         = EffectiveGlobalToken.ColorPrimaryActive;
        DefaultActiveBorderColor   = EffectiveGlobalToken.ColorPrimaryActive;

        var isBright = ColorUtils.IsBright(EffectiveGlobalToken.ColorBgSolid, Colors.White);
        if (isBright)
        {
            SolidTextColor = Colors.Black;
        }
        else
        {
            SolidTextColor = Colors.White;
        }

        TextTextColor       = EffectiveGlobalToken.ColorText;
        TextTextHoverColor  = EffectiveGlobalToken.ColorText;
        TextTextActiveColor = EffectiveGlobalToken.ColorText;

    }
}
