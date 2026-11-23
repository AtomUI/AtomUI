using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="NotificationCardToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnNotificationCardDerivation : IControlDerivation<ShadcnToken, NotificationCardToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<NotificationCardToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.RestingShadows     = global.BoxShadows;
        token.InformationShadows = global.BoxShadows;
        token.SuccessShadows     = global.BoxShadows;
        token.WarningShadows     = global.BoxShadows;
        token.ErrorShadows       = global.BoxShadows;

        // 间距直接取全局 Token 的同名值，不再做控件级缩放：
        //   卡片内边距         = paddingMD(纵向) / paddingLG(横向)
        //   图标与内容间距     = marginSM
        //   标题与描述间距     = marginXS
        //   操作组上边距       = marginSM
        //   标题右侧留白       = paddingLG
        //   关闭按钮偏移       = paddingMD(纵向) / paddingLG(横向)
        // 之前这里对四个值统一乘了 2/3，让卡片比对照实现矮一截，属于本控件自创的缩放，已移除。
        var paddingMD = global.UniformlyPaddingMD;
        var paddingLG = global.UniformlyPaddingLG;
        var marginXS  = global.UniformlyMarginXS;
        var marginSM  = global.UniformlyMarginSM;

        // 卡片内边距上下对称：纵向 paddingMD、横向 paddingLG。
        token.NotificationPadding = new Thickness(paddingLG, paddingMD, paddingLG, paddingMD);
        token.NotificationBg = global.ColorBgElevated;
        // The severity surfaces default to the plain card: only the icon carries the severity.
        token.NotificationBorderThickness = default;
        token.NotificationBorderColor     = Colors.Transparent;
        token.InformationBg               = token.NotificationBg;
        token.InformationIconColor        = global.ColorPrimary;
        token.InformationBorderColor      = Colors.Transparent;
        token.InformationTitleColor       = global.ColorTextHeading;
        token.InformationDescriptionColor = global.ColorText;
        token.InformationCloseIconColor   = global.ColorIcon;
        token.InformationCloseIconHoverColor = global.ColorIconHover;
        token.SuccessBg               = token.NotificationBg;
        token.SuccessIconColor        = global.ColorSuccess;
        token.SuccessBorderColor      = Colors.Transparent;
        token.SuccessTitleColor       = global.ColorTextHeading;
        token.SuccessDescriptionColor = global.ColorText;
        token.SuccessCloseIconColor   = global.ColorIcon;
        token.SuccessCloseIconHoverColor = global.ColorIconHover;
        token.WarningBg               = token.NotificationBg;
        token.WarningIconColor        = global.ColorWarning;
        token.WarningBorderColor      = Colors.Transparent;
        token.WarningTitleColor       = global.ColorTextHeading;
        token.WarningDescriptionColor = global.ColorText;
        token.WarningCloseIconColor   = global.ColorIcon;
        token.WarningCloseIconHoverColor = global.ColorIconHover;
        token.ErrorBg               = token.NotificationBg;
        token.ErrorIconColor        = global.ColorError;
        token.ErrorBorderColor      = Colors.Transparent;
        token.ErrorTitleColor       = global.ColorTextHeading;
        token.ErrorDescriptionColor = global.ColorText;
        token.ErrorCloseIconColor   = global.ColorIcon;
        token.ErrorCloseIconHoverColor = global.ColorIconHover;
        token.NotificationIconSize = global.FontSizeLG * global.RelativeLineHeightLG;
        token.NotificationCloseButtonSize = global.ControlHeightLG * 0.55;

        // 标题与描述之间的纵向间距。
        token.NotificationSectionSpacing = marginXS;
        // 操作组与上方内容之间的间距。
        token.NotificationActionsMargin = new Thickness(0, marginSM, 0, 0);
        // 关闭按钮是贴卡片右上角的覆盖层：纵向离顶 paddingMD、横向离右 paddingLG。
        token.NotificationCloseButtonMargin = new Thickness(0, paddingMD, paddingLG, 0);
        // 标题右侧留白，避免文本压到关闭按钮。
        token.NotificationTitlePadding = new Thickness(0, 0, paddingLG, 0);
        // 进度条贴卡片底边，左右各内缩一个圆角半径，避免盖住圆角。
        token.NotificationProgressHeight = 2;
        token.NotificationProgressTrackBg = global.ColorFillQuaternary;
        token.NotificationProgressMargin = new Thickness(global.BorderRadiusLG.TopLeft, 0, global.BorderRadiusLG.TopRight, 0);

        token.NotificationProgressBg = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint   = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new() { Color = global.ColorPrimaryBorderHover, Offset = 0 },
                new() { Color = global.ColorPrimary, Offset      = 1 }
            }
        }.ToImmutable();
        token.NotificationWidth = 384;
        // 图标与右侧内容之间的横向间距。
        token.NotificationIconMargin         = new Thickness(0, 0, marginSM, 0);
        token.NotificationTopMargin = new Thickness(global.UniformlyMarginLG, global.UniformlyMarginLG, global.UniformlyMarginLG, 0);
        token.NotificationBottomMargin =
            new Thickness(global.UniformlyMarginLG, 0, global.UniformlyMarginLG, global.UniformlyMarginLG);
    }
}
