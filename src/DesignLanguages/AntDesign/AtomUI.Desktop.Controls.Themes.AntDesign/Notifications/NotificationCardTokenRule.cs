using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="NotificationCardToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class NotificationCardTokenRule : IControlTokenRule<AntDesignToken, NotificationCardToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<NotificationCardToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.RestingShadows     = global.BoxShadows;
        token.InformationShadows = global.BoxShadows;
        token.SuccessShadows     = global.BoxShadows;
        token.WarningShadows     = global.BoxShadows;
        token.ErrorShadows       = global.BoxShadows;

        // Spacing takes the global token of the same name directly; no value is scaled at the control level:
        //   card padding             = paddingMD (vertical) / paddingLG (horizontal)
        //   icon to content gap      = marginSM
        //   title to description gap = marginXS
        //   actions top margin       = marginSM
        //   space right of the title = paddingLG
        //   close button offset      = paddingMD (vertical) / paddingLG (horizontal)
        // An earlier version multiplied four of these values by 2/3, which left the card a good deal shorter than
        // the reference implementation. That scaling was invented for this control and has been removed.
        var paddingMD = global.UniformlyPaddingMD;
        var paddingLG = global.UniformlyPaddingLG;
        var marginXS  = global.UniformlyMarginXS;
        var marginSM  = global.UniformlyMarginSM;

        // The card padding is the same on opposite sides: paddingMD vertically and paddingLG horizontally.
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

        // The vertical gap between the title and the description.
        token.NotificationSectionSpacing = marginXS;
        // The gap between the action group and the content above it.
        token.NotificationActionsMargin = new Thickness(0, marginSM, 0, 0);
        // The close button is an overlay pinned to the top right corner of the card: paddingMD from the top and
        // paddingLG from the right.
        token.NotificationCloseButtonMargin = new Thickness(0, paddingMD, paddingLG, 0);
        // Space on the right of the title, so that the text does not run under the close button.
        token.NotificationTitlePadding = new Thickness(0, 0, paddingLG, 0);
        // The progress bar sits on the bottom edge of the card, inset by one corner radius on each side so that it
        // does not cover the rounded corners.
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
        // antd fixes the notification width to 384.
        token.NotificationWidth = 384;
        // The horizontal gap between the icon and the content on its right.
        token.NotificationIconMargin         = new Thickness(0, 0, marginSM, 0);
        token.NotificationTopMargin = new Thickness(global.UniformlyMarginLG, global.UniformlyMarginLG, global.UniformlyMarginLG, 0);
        token.NotificationBottomMargin =
            new Thickness(global.UniformlyMarginLG, 0, global.UniformlyMarginLG, global.UniformlyMarginLG);
    }
}
