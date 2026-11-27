using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="NotificationCardToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class NotificationCardTokenRule : IControlTokenRule<PrimeNGToken, NotificationCardToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<NotificationCardToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides and the severity colors, read by the formulas below.
        var statuses = PrimeNGNotificationCardTokens.EvaluateGlobal(in context, global);
        token.RestingShadows     = global.BoxShadows;
        token.InformationShadows = global.BoxShadows;
        token.SuccessShadows     = global.BoxShadows;
        token.WarningShadows     = global.BoxShadows;
        token.ErrorShadows       = global.BoxShadows;

        // Spacing takes the global Token of the same name directly, without a control-level scale:
        //   card padding             = paddingMD (vertical) / paddingLG (horizontal)
        //   icon to content gap      = marginSM
        //   title to description gap = marginXS
        //   action group top margin  = marginSM
        //   space right of the title = paddingLG
        //   close button offset      = paddingMD (vertical) / paddingLG (horizontal)
        // An earlier version multiplied four of these values by 2/3, which made the card much shorter than the
        // reference implementation; that scale was this control's own invention and has been removed.
        var paddingMD = global.UniformlyPaddingMD;
        var paddingLG = global.UniformlyPaddingLG;
        var marginXS  = global.UniformlyMarginXS;
        var marginSM  = global.UniformlyMarginSM;

        // The card padding is symmetric: paddingMD vertically and paddingLG horizontally.
        token.NotificationPadding = new Thickness(paddingLG, paddingMD, paddingLG, paddingMD);
        token.NotificationBg = global.ColorBgElevated;
        // The severity surfaces default to the plain card: only the icon carries the severity.
        token.NotificationBorderThickness = default;
        token.NotificationBorderColor     = Colors.Transparent;
        token.InformationBg               = token.NotificationBg;
        token.InformationIconColor        = statuses.Primary;
        token.InformationBorderColor      = Colors.Transparent;
        token.InformationTitleColor       = global.ColorTextHeading;
        token.InformationDescriptionColor = global.ColorText;
        token.InformationCloseIconColor   = global.ColorIcon;
        token.InformationCloseIconHoverColor = global.ColorIconHover;
        token.SuccessBg               = token.NotificationBg;
        token.SuccessIconColor        = statuses.Success;
        token.SuccessBorderColor      = Colors.Transparent;
        token.SuccessTitleColor       = global.ColorTextHeading;
        token.SuccessDescriptionColor = global.ColorText;
        token.SuccessCloseIconColor   = global.ColorIcon;
        token.SuccessCloseIconHoverColor = global.ColorIconHover;
        token.WarningBg               = token.NotificationBg;
        token.WarningIconColor        = statuses.Warning;
        token.WarningBorderColor      = Colors.Transparent;
        token.WarningTitleColor       = global.ColorTextHeading;
        token.WarningDescriptionColor = global.ColorText;
        token.WarningCloseIconColor   = global.ColorIcon;
        token.WarningCloseIconHoverColor = global.ColorIconHover;
        token.ErrorBg               = token.NotificationBg;
        token.ErrorIconColor        = statuses.Error;
        token.ErrorBorderColor      = Colors.Transparent;
        token.ErrorTitleColor       = global.ColorTextHeading;
        token.ErrorDescriptionColor = global.ColorText;
        token.ErrorCloseIconColor   = global.ColorIcon;
        token.ErrorCloseIconHoverColor = global.ColorIconHover;
        // Ant Design formula kept as is: the notification stage (PrimeNGNotificationCardTokens) writes the PrimeUIX
        // toast values over the icon and close button sizes, the spacing, the shadows and the severity colors;
        // the progress bar and the card margins keep the Ant Design formula.
        token.NotificationIconSize = global.FontSizeLG * global.RelativeLineHeightLG;
        token.NotificationCloseButtonSize = global.ControlHeightLG * 0.55;

        // Vertical gap between the title and the description.
        token.NotificationSectionSpacing = marginXS;
        // Gap between the action group and the content above it.
        token.NotificationActionsMargin = new Thickness(0, marginSM, 0, 0);
        // The close button overlays the card's top right corner: paddingMD from the top, paddingLG from the right.
        token.NotificationCloseButtonMargin = new Thickness(0, paddingMD, paddingLG, 0);
        // Space on the right of the title, so that the text does not run into the close button.
        token.NotificationTitlePadding = new Thickness(0, 0, paddingLG, 0);
        // The progress bar sits on the card's bottom edge. It is inset by one corner radius on each side, so that the
        // rounded corners stay free.
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
                new() { Color = statuses.Primary, Offset      = 1 }
            }
        }.ToImmutable();
        // antd fixes the notification width to 384.
        token.NotificationWidth = 384;
        // Horizontal gap between the icon and the content to its right.
        token.NotificationIconMargin         = new Thickness(0, 0, marginSM, 0);
        token.NotificationTopMargin = new Thickness(global.UniformlyMarginLG, global.UniformlyMarginLG, global.UniformlyMarginLG, 0);
        token.NotificationBottomMargin =
            new Thickness(global.UniformlyMarginLG, 0, global.UniformlyMarginLG, global.UniformlyMarginLG);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGNotificationCardTokens.EvaluateOwn(in context, global, token);
    }
}
