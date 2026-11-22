using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class NotificationCardToken : AbstractControlDesignToken
{

    public NotificationCardToken()

    {
    }

    /// <summary>
    /// 提醒框背景色
    /// </summary>
    public partial Color NotificationBg { get; set; }

    /// <summary>
    /// 提醒框内边距
    /// </summary>
    public partial Thickness NotificationPadding { get; set; }

    /// <summary>
    /// 提醒框图标尺寸
    /// </summary>
    public partial double NotificationIconSize { get; set; }

    /// <summary>
    /// 提醒框图标外边距
    /// </summary>
    public partial Thickness NotificationIconMargin { get; set; }

    /// <summary>
    /// 提醒框关闭按钮尺寸
    /// </summary>
    public partial double NotificationCloseButtonSize { get; set; }
    
    /// <summary>
    /// 内容区域标题与描述之间的间距
    /// </summary>
    public partial double NotificationSectionSpacing { get; set; }

    /// <summary>
    /// 标题与描述的右侧留白，避免文本压到关闭按钮
    /// </summary>
    public partial Thickness NotificationTitlePadding { get; set; }

    /// <summary>
    /// 操作组的上边距
    /// </summary>
    public partial Thickness NotificationActionsMargin { get; set; }

    /// <summary>
    /// 关闭按钮由卡片内容区左上角偏移到内容区右上角的覆盖层外边距
    /// </summary>
    public partial Thickness NotificationCloseButtonMargin { get; set; }

    /// <summary>
    /// 提醒框进度条背景色
    /// </summary>
    public partial IImmutableBrush? NotificationProgressBg { get; set; }

    /// <summary>
    /// 提醒框进度条底槽色。进度条先以该色铺满整条作为底槽，再叠加彩色进度值；
    /// 缺底槽时剩余时间在卡片背景上不可见。
    /// </summary>
    public partial Color NotificationProgressTrackBg { get; set; }

    /// <summary>
    /// 提醒框进度条高度
    /// </summary>
    public partial double NotificationProgressHeight { get; set; }

    /// <summary>
    /// 进度条外边距
    /// </summary>
    public partial Thickness NotificationProgressMargin { get; set; }

    /// <summary>
    /// 提醒框宽度
    /// </summary>
    public partial double NotificationWidth { get; set; }


    /// <summary>
    /// 提醒框上边缘外边距
    /// </summary>
    public partial Thickness NotificationTopMargin { get; set; }

    /// <summary>
    /// 提醒框下边缘外边距
    /// </summary>
    public partial Thickness NotificationBottomMargin { get; set; }

    /// <summary>
    /// 提醒框默认阴影（Default 类型回退值）
    /// </summary>
    public partial BoxShadows RestingShadows { get; set; }

    /// <summary>
    /// 提醒框信息样式阴影
    /// </summary>
    public partial BoxShadows InformationShadows { get; set; }

    /// <summary>
    /// 提醒框成功样式阴影
    /// </summary>
    public partial BoxShadows SuccessShadows { get; set; }

    /// <summary>
    /// 提醒框警告样式阴影
    /// </summary>
    public partial BoxShadows WarningShadows { get; set; }

    /// <summary>
    /// 提醒框错误样式阴影
    /// </summary>
    public partial BoxShadows ErrorShadows { get; set; }

    /// <summary>
    /// Border thickness of the card frame; a design language draws a start bar with a thick leading side.
    /// </summary>
    public partial Thickness NotificationBorderThickness { get; set; }

    /// <summary>
    /// Border colour of a notification without a severity.
    /// </summary>
    public partial Color NotificationBorderColor { get; set; }

    /// <summary>
    /// Background of an information notification.
    /// </summary>
    public partial Color InformationBg { get; set; }

    /// <summary>
    /// Icon colour of an information notification.
    /// </summary>
    public partial Color InformationIconColor { get; set; }

    /// <summary>
    /// Border colour of an information notification.
    /// </summary>
    public partial Color InformationBorderColor { get; set; }

    /// <summary>
    /// Title colour of an information notification.
    /// </summary>
    public partial Color InformationTitleColor { get; set; }

    /// <summary>
    /// Description colour of an information notification.
    /// </summary>
    public partial Color InformationDescriptionColor { get; set; }

    /// <summary>
    /// Close icon colour of an information notification.
    /// </summary>
    public partial Color InformationCloseIconColor { get; set; }

    /// <summary>
    /// Close icon colour of an information notification while the close button is hovered or pressed.
    /// </summary>
    public partial Color InformationCloseIconHoverColor { get; set; }

    /// <summary>
    /// Background of an success notification.
    /// </summary>
    public partial Color SuccessBg { get; set; }

    /// <summary>
    /// Icon colour of an success notification.
    /// </summary>
    public partial Color SuccessIconColor { get; set; }

    /// <summary>
    /// Border colour of an success notification.
    /// </summary>
    public partial Color SuccessBorderColor { get; set; }

    /// <summary>
    /// Title colour of an success notification.
    /// </summary>
    public partial Color SuccessTitleColor { get; set; }

    /// <summary>
    /// Description colour of an success notification.
    /// </summary>
    public partial Color SuccessDescriptionColor { get; set; }

    /// <summary>
    /// Close icon colour of an success notification.
    /// </summary>
    public partial Color SuccessCloseIconColor { get; set; }

    /// <summary>
    /// Close icon colour of an success notification while the close button is hovered or pressed.
    /// </summary>
    public partial Color SuccessCloseIconHoverColor { get; set; }

    /// <summary>
    /// Background of an warning notification.
    /// </summary>
    public partial Color WarningBg { get; set; }

    /// <summary>
    /// Icon colour of an warning notification.
    /// </summary>
    public partial Color WarningIconColor { get; set; }

    /// <summary>
    /// Border colour of an warning notification.
    /// </summary>
    public partial Color WarningBorderColor { get; set; }

    /// <summary>
    /// Title colour of an warning notification.
    /// </summary>
    public partial Color WarningTitleColor { get; set; }

    /// <summary>
    /// Description colour of an warning notification.
    /// </summary>
    public partial Color WarningDescriptionColor { get; set; }

    /// <summary>
    /// Close icon colour of an warning notification.
    /// </summary>
    public partial Color WarningCloseIconColor { get; set; }

    /// <summary>
    /// Close icon colour of an warning notification while the close button is hovered or pressed.
    /// </summary>
    public partial Color WarningCloseIconHoverColor { get; set; }

    /// <summary>
    /// Background of an error notification.
    /// </summary>
    public partial Color ErrorBg { get; set; }

    /// <summary>
    /// Icon colour of an error notification.
    /// </summary>
    public partial Color ErrorIconColor { get; set; }

    /// <summary>
    /// Border colour of an error notification.
    /// </summary>
    public partial Color ErrorBorderColor { get; set; }

    /// <summary>
    /// Title colour of an error notification.
    /// </summary>
    public partial Color ErrorTitleColor { get; set; }

    /// <summary>
    /// Description colour of an error notification.
    /// </summary>
    public partial Color ErrorDescriptionColor { get; set; }

    /// <summary>
    /// Close icon colour of an error notification.
    /// </summary>
    public partial Color ErrorCloseIconColor { get; set; }

    /// <summary>
    /// Close icon colour of an error notification while the close button is hovered or pressed.
    /// </summary>
    public partial Color ErrorCloseIconHoverColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        RestingShadows     = EffectiveGlobalToken.BoxShadows;
        InformationShadows = EffectiveGlobalToken.BoxShadows;
        SuccessShadows     = EffectiveGlobalToken.BoxShadows;
        WarningShadows     = EffectiveGlobalToken.BoxShadows;
        ErrorShadows       = EffectiveGlobalToken.BoxShadows;

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
        // The severity backgrounds below default to this card background, so an explicit value carries over.
        if (!IsExplicit(NotificationCardTokenKind.NotificationBg))
        {
            NotificationBg = EffectiveGlobalToken.ColorBgElevated;
        }
        // The severity surfaces default to the plain card: only the icon carries the severity.
        NotificationBorderThickness = default;
        NotificationBorderColor     = Colors.Transparent;
        InformationBg               = NotificationBg;
        InformationIconColor        = EffectiveGlobalToken.ColorPrimary;
        InformationBorderColor      = Colors.Transparent;
        InformationTitleColor       = EffectiveGlobalToken.ColorTextHeading;
        InformationDescriptionColor = EffectiveGlobalToken.ColorText;
        InformationCloseIconColor   = EffectiveGlobalToken.ColorIcon;
        InformationCloseIconHoverColor = EffectiveGlobalToken.ColorIconHover;
        SuccessBg               = NotificationBg;
        SuccessIconColor        = EffectiveGlobalToken.ColorSuccess;
        SuccessBorderColor      = Colors.Transparent;
        SuccessTitleColor       = EffectiveGlobalToken.ColorTextHeading;
        SuccessDescriptionColor = EffectiveGlobalToken.ColorText;
        SuccessCloseIconColor   = EffectiveGlobalToken.ColorIcon;
        SuccessCloseIconHoverColor = EffectiveGlobalToken.ColorIconHover;
        WarningBg               = NotificationBg;
        WarningIconColor        = EffectiveGlobalToken.ColorWarning;
        WarningBorderColor      = Colors.Transparent;
        WarningTitleColor       = EffectiveGlobalToken.ColorTextHeading;
        WarningDescriptionColor = EffectiveGlobalToken.ColorText;
        WarningCloseIconColor   = EffectiveGlobalToken.ColorIcon;
        WarningCloseIconHoverColor = EffectiveGlobalToken.ColorIconHover;
        ErrorBg               = NotificationBg;
        ErrorIconColor        = EffectiveGlobalToken.ColorError;
        ErrorBorderColor      = Colors.Transparent;
        ErrorTitleColor       = EffectiveGlobalToken.ColorTextHeading;
        ErrorDescriptionColor = EffectiveGlobalToken.ColorText;
        ErrorCloseIconColor   = EffectiveGlobalToken.ColorIcon;
        ErrorCloseIconHoverColor = EffectiveGlobalToken.ColorIconHover;
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
    }
    
}
