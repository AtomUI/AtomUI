using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="WindowTitleBarToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class WindowTitleBarTokenRule : IControlTokenRule<ShadcnToken, WindowTitleBarToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<WindowTitleBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TitleBarActionTokenCalculator.Calculate(in context, global, token);
        token.LogoAndTitleSpacing         = global.SizeUnit * 2;

        token.LogoAndLeftAddOnSpacing     = global.SpacingXXS;
        token.TitleBarPadding             = new Thickness(token.LogoAndTitleSpacing * 1.8, 0);
        token.CaptionButtonIconSize       = global.IconSize;

        // Ant Design formula kept as is: shadcn has no WindowTitleBar, so the language has no values of its own.
        token.WindowsCaptionIconSize      = 11;
        token.LogoSize                    = global.SizeUnit * 4;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);

        token.CaptionGroupSpacing  = global.SizeUnit * 2;

        // A managed caption button takes max(w, h) / 2 of its own measured size as the corner radius of a full
        // circle. Its content size is fixed to icon + padding, so the same formula exposes that geometry to the
        // theme and lets the macOS AddOn button share the corner radius source with the adjacent caption buttons.


        // Window decoration does not go through the density algorithm: in compact mode ControlHeightLG, SizeLG and
        // SpacingXS all shrink, and the title bar and the DockPanel spacing must not shrink with them. The values
        // are therefore stable across platforms.
        token.Height                  = 40;
        token.HeaderHorizontalSpacing = 8;
        token.WindowsCloseButtonHoverBgColor   = Color.FromRgb(244, 67, 54);

        token.WindowsCloseButtonPressedBgColor = Color.FromArgb(190, 244, 67, 54);
        token.WindowsCloseButtonHoverColor     = ColorUtils.OnBackground(Colors.White, token.WindowsCloseButtonHoverBgColor);

        token.TitleFontSize = 13;
        token.TitleFontWeight = FontWeight.Bold;
    }
}
