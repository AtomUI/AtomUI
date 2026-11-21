using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractImagePreviewerToken : AbstractControlDesignToken
{

    /// <summary>
    /// 预览操作图标大小
    /// Size of preview operation icon
    /// </summary>
    public partial double PreviewOperationSize { get; set; }

    /// <summary>
    /// 预览操作图标颜色
    /// Color of preview operation icon
    /// </summary>
    public partial Color PreviewOperationColor { get; set; }

    /// <summary>
    /// 预览操作图标悬浮颜色
    /// Color of hovered preview operation icon
    /// </summary>
    public partial Color PreviewOperationHoverColor { get; set; }

    /// <summary>
    /// 预览操作图标禁用颜色
    /// Disabled color of preview operation icon
    /// </summary>
    public partial Color PreviewOperationColorDisabled { get; set; }

    /// <summary>
    /// 预览切换按钮尺寸
    /// Size of preview switch button
    /// </summary>

    /// <summary>
    /// 加载失败占位图标尺寸（AtomUI 扩展，antd 加载失败时显示 fallback 图片）
    /// Size of the load-failure placeholder icon (an AtomUI extension; antd shows a fallback image)
    /// </summary>
    public partial double ErrorIconSize { get; set; }

    /// <summary>
    /// 封面的背景颜色
    /// </summary>
    public partial Color MaskBgColor { get; set; }

    /// <summary>
    /// 预览窗口最小宽度
    /// </summary>
    public partial double DialogMinWidth { get; set; }

    /// <summary>
    /// 预览窗口最小高度
    /// </summary>
    public partial double DialogMinHeight { get; set; }

    /// <summary>
    /// 预览窗口标题栏背景色
    /// </summary>
    public partial Color TitleBarBackgroundColor { get; set; }

    /// <summary>
    /// 浮动导航按钮背景颜色
    /// </summary>
    public partial Color NavButtonBgColor { get; set; }

    /// <summary>
    /// 浮动导航按钮 Hover 背景颜色
    /// </summary>
    public partial Color NavButtonBgHoverColor { get; set; }

    /// <summary>
    /// 浮动工具栏内间距
    /// </summary>
    public partial Thickness FloatToolbarPadding { get; set; }

    /// <summary>
    /// 浮动工具栏索引指示器内间距
    /// </summary>
    public partial Thickness FloatToolbarIndicatorPadding { get; set; }

    /// <summary>
    /// 封面默认的宽度
    /// </summary>
    public partial double CoverImageWidth { get; set; }



    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // Floating controls can cover light or dark image content. Pair a dark scrim with light icons.
        PreviewOperationColor         = EffectiveGlobalToken.ColorWhite.SetAlphaF(0.9);
        PreviewOperationHoverColor    = EffectiveGlobalToken.ColorWhite;
        PreviewOperationColorDisabled = EffectiveGlobalToken.ColorWhite.SetAlphaF(0.4);
        PreviewOperationSize          = EffectiveGlobalToken.FontSizeIcon * 1.5;

        ErrorIconSize                 = EffectiveGlobalToken.ControlHeightLG * 1.2;
        MaskBgColor                   = ColorUtils.FromRgbF(0.3, 0, 0, 0);
        DialogMinWidth                = 710;
        DialogMinHeight               = 240;
        CoverImageWidth               = 200;

        if (isDarkMode)
        {
            TitleBarBackgroundColor       = EffectiveGlobalToken.ColorBorderSecondary;
        }
        else
        {
            TitleBarBackgroundColor       = EffectiveGlobalToken.ColorBorderSecondary.Darken(1);
        }

        NavButtonBgColor             = EffectiveGlobalToken.ColorBlack.SetAlphaF(0.72);
        NavButtonBgHoverColor        = EffectiveGlobalToken.ColorBlack.SetAlphaF(0.84);
        FloatToolbarPadding          = new Thickness(EffectiveGlobalToken.UniformlyPaddingLG, 0);
        // The index indicator is transparent and centred above the actions, so only its block padding shows; the default
        // adds none (the former inline-only default never rendered).
        FloatToolbarIndicatorPadding = new Thickness(0);
    }

}
