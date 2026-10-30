using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class NotificationCardToken : AbstractControlDesignToken
{

    public NotificationCardToken()

    {
    }

    /// <summary>
    /// 提醒框背景色
    /// </summary>
    public Color NotificationBg { get; set; }

    /// <summary>
    /// 提醒框内边距
    /// </summary>
    public Thickness NotificationPadding { get; set; }

    /// <summary>
    /// 提醒框图标尺寸
    /// </summary>
    public double NotificationIconSize { get; set; }

    /// <summary>
    /// 提醒框图标外边距
    /// </summary>
    public Thickness NotificationIconMargin { get; set; }

    /// <summary>
    /// 提醒框关闭按钮尺寸
    /// </summary>
    public double NotificationCloseButtonSize { get; set; }
    
    /// <summary>
    /// 提醒框关闭按钮内间距
    /// </summary>
    public Thickness NotificationCloseButtonPadding { get; set; }

    /// <summary>
    /// 内容区域标题与描述之间的间距
    /// </summary>
    public double NotificationSectionSpacing { get; set; }

    /// <summary>
    /// 标题与描述的右侧留白，避免文本压到关闭按钮
    /// </summary>
    public Thickness NotificationTitlePadding { get; set; }

    /// <summary>
    /// 操作组的上边距
    /// </summary>
    public Thickness NotificationActionsMargin { get; set; }

    /// <summary>
    /// 关闭按钮由卡片内容区左上角偏移到内容区右上角的覆盖层外边距
    /// </summary>
    public Thickness NotificationCloseButtonMargin { get; set; }

    /// <summary>
    /// 提醒框进度条背景色
    /// </summary>
    public IImmutableBrush? NotificationProgressBg { get; set; }

    /// <summary>
    /// 提醒框进度条底槽色。进度条先以该色铺满整条作为底槽，再叠加彩色进度值；
    /// 缺底槽时剩余时间在卡片背景上不可见。
    /// </summary>
    public Color NotificationProgressTrackBg { get; set; }

    /// <summary>
    /// 提醒框进度条高度
    /// </summary>
    public double NotificationProgressHeight { get; set; }

    /// <summary>
    /// 进度条外边距
    /// </summary>
    public Thickness NotificationProgressMargin { get; set; }

    /// <summary>
    /// 提醒框宽度
    /// </summary>
    public double NotificationWidth { get; set; }


    /// <summary>
    /// 提醒框上边缘外边距
    /// </summary>
    public Thickness NotificationTopMargin { get; set; }

    /// <summary>
    /// 提醒框下边缘外边距
    /// </summary>
    public Thickness NotificationBottomMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        // 间距直接取全局 Token 的同名值，不再做控件级缩放：
        //   卡片内边距         = paddingMD(纵向) / paddingLG(横向)
        //   图标与内容间距     = marginSM
        //   标题与描述间距     = marginXS
        //   操作组上边距       = marginSM
        //   标题右侧留白       = paddingLG
        //   关闭按钮偏移       = paddingMD(纵向) / paddingLG(横向)
        // 之前这里对四个值统一乘了 2/3，让卡片比对照实现矮一截，属于本控件自创的缩放，已移除。
        var paddingMD = EffectiveGlobalToken.UniformlyPaddingMD;
        var paddingLG = EffectiveGlobalToken.UniformlyPaddingLG;
        var marginXS  = EffectiveGlobalToken.UniformlyMarginXS;
        var marginSM  = EffectiveGlobalToken.UniformlyMarginSM;

        // 卡片内边距上下对称：纵向 paddingMD、横向 paddingLG。
        NotificationPadding = new Thickness(paddingLG, paddingMD, paddingLG, paddingMD);
        NotificationBg = EffectiveGlobalToken.ColorBgElevated;
        NotificationIconSize = EffectiveGlobalToken.FontSizeLG * EffectiveGlobalToken.RelativeLineHeightLG;
        NotificationCloseButtonSize = EffectiveGlobalToken.ControlHeightLG * 0.55;

        // 标题与描述之间的纵向间距。
        NotificationSectionSpacing = marginXS;
        // 操作组与上方内容之间的间距。
        NotificationActionsMargin = new Thickness(0, marginSM, 0, 0);
        // 关闭按钮是贴卡片右上角的覆盖层：纵向离顶 paddingMD、横向离右 paddingLG。
        NotificationCloseButtonMargin = new Thickness(0, paddingMD, paddingLG, 0);
        // 标题右侧留白，避免文本压到关闭按钮。
        NotificationTitlePadding = new Thickness(0, 0, paddingLG, 0);
        // 进度条贴卡片底边，左右各内缩一个圆角半径，避免盖住圆角。
        NotificationProgressHeight = 2;
        NotificationProgressTrackBg = EffectiveGlobalToken.ColorFillQuaternary;
        NotificationProgressMargin = new Thickness(EffectiveGlobalToken.BorderRadiusLG.TopLeft, 0, EffectiveGlobalToken.BorderRadiusLG.TopRight, 0);

        NotificationProgressBg = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint   = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new() { Color = EffectiveGlobalToken.ColorPrimaryBorderHover, Offset = 0 },
                new() { Color = EffectiveGlobalToken.ColorPrimary, Offset      = 1 }
            }
        }.ToImmutable();
        NotificationWidth = 384;
        // 图标与右侧内容之间的横向间距。
        NotificationIconMargin         = new Thickness(0, 0, marginSM, 0);
        NotificationTopMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginLG, EffectiveGlobalToken.UniformlyMarginLG, EffectiveGlobalToken.UniformlyMarginLG, 0);
        NotificationBottomMargin =
            new Thickness(EffectiveGlobalToken.UniformlyMarginLG, 0, EffectiveGlobalToken.UniformlyMarginLG, EffectiveGlobalToken.UniformlyMarginLG);
        NotificationCloseButtonPadding = EffectiveGlobalToken.PaddingXXS;
    }
    
}
