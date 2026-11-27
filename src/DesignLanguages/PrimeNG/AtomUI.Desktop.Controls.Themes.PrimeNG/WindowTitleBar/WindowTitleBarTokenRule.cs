using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="WindowTitleBarToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class WindowTitleBarTokenRule : IControlTokenRule<PrimeNGToken, WindowTitleBarToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<WindowTitleBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TitleBarActionTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: PrimeUIX defines no values for this control, so the rule takes the Ant
        // Design formula over the PrimeNG global tokens.
        token.LogoAndTitleSpacing         = global.SizeUnit * 2;

        token.LogoAndLeftAddOnSpacing     = global.SpacingXXS;
        token.TitleBarPadding             = new Thickness(token.LogoAndTitleSpacing * 1.8, 0);
        token.CaptionButtonIconSize       = global.IconSize;

        // Aligned with the glyph size of Avalonia's native-style window decorations.
        token.WindowsCaptionIconSize      = 11;
        token.LogoSize                    = global.SizeUnit * 4;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);

        token.CaptionGroupSpacing  = global.SizeUnit * 2;

        // A managed caption button derives a fully round corner radius of max(w, h) / 2 from its own measured size.
        // Its content size is fixed at icon + padding, so the same formula is exposed to the theme here; the macOS
        // AddOn button and the adjacent caption buttons then share one source for the corner radius.


        // Window decoration does not follow the density algorithm. In compact mode ControlHeightLG, SizeLG and
        // SpacingXS all shrink, but the title bar and the DockPanel spacing should not shrink with them. The values
        // are therefore constants that are stable across platforms.
        token.Height                  = 40;
        token.HeaderHorizontalSpacing = 8;
        token.WindowsCloseButtonHoverBgColor   = Color.FromRgb(244, 67, 54);

        token.WindowsCloseButtonPressedBgColor = Color.FromArgb(190, 244, 67, 54);
        token.WindowsCloseButtonHoverColor     = ColorUtils.OnBackground(Colors.White, token.WindowsCloseButtonHoverBgColor);

        token.TitleFontSize = 13;
        token.TitleFontWeight = FontWeight.Bold;
    }
}
