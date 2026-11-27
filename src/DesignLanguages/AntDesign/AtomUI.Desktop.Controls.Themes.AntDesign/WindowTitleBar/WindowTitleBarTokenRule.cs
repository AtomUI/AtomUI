using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="WindowTitleBarToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class WindowTitleBarTokenRule : IControlTokenRule<AntDesignToken, WindowTitleBarToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<WindowTitleBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TitleBarActionTokenCalculator.Calculate(in context, global, token);
        token.LogoAndTitleSpacing         = global.SizeUnit * 2;

        token.LogoAndLeftAddOnSpacing     = global.SpacingXXS;
        token.TitleBarPadding             = new Thickness(token.LogoAndTitleSpacing * 1.8, 0);
        token.CaptionButtonIconSize       = global.IconSize;

        // Aligned with the glyph size of Avalonia's native-style window decorations.
        token.WindowsCaptionIconSize      = 11;
        token.LogoSize                    = global.SizeUnit * 4;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);

        token.CaptionGroupSpacing  = global.SizeUnit * 2;

        // The caption button rules round their corners via TitleBarActionTokenCalculator.CalculateCaptionCornerRadius.


        // Window decoration does not follow the density algorithm. In compact mode ControlHeightLG, SizeLG and
        // SpacingXS shrink, but the title bar height and the DockPanel spacing must not shrink with them, so the
        // values below are cross-platform constants.
        token.Height                  = 40;
        token.HeaderHorizontalSpacing = 8;
        token.WindowsCloseButtonHoverBgColor   = Color.FromRgb(244, 67, 54);

        token.WindowsCloseButtonPressedBgColor = Color.FromArgb(190, 244, 67, 54);
        token.WindowsCloseButtonHoverColor     = ColorUtils.OnBackground(Colors.White, token.WindowsCloseButtonHoverBgColor);

        token.TitleFontSize = 13;
        token.TitleFontWeight = FontWeight.Bold;
    }
}
