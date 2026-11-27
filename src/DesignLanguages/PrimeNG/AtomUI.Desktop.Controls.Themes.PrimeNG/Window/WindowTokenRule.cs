using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="WindowToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class WindowTokenRule : IControlTokenRule<PrimeNGToken, WindowToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<WindowToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: PrimeUIX defines no values for this control, so the rule takes the Ant
        // Design formula over the PrimeNG global tokens.
        token.DefaultBackground        = global.ColorBgContainer;
        token.DefaultForeground        = global.ColorText;
        token.CornerRadius             = new CornerRadius(12);
        token.SystemBarColor           = new SolidColorBrush(global.ColorBgContainer);
        // Window decoration is not part of the density algorithm. The compact algorithm shrinks ControlHeightLG and
        // SizeLG, and TitleBarHeight with them, so a value that is stable across platforms cuts the compact chain.
        token.TitleBarHeight               = 40;
        // The values match the translucent composite of the Win10 DWM shadow texture along the top edge of the window:
        // about #707070 for the active state and #AAAAAA for the inactive one on a white background. The border is
        // therefore a dark gray with alpha, not a fixed solid color.
        token.FrameBorderActive        = Color.FromArgb(0xA8, 0x26, 0x26, 0x26);
        token.FrameBorderInactive      = Color.FromArgb(0x64, 0x26, 0x26, 0x26);
        token.FrameShadows             = global.BoxShadowsSecondary;
        token.FullscreenPopoverShadows = new BoxShadows(
            new BoxShadow
            {
                OffsetX = 0, OffsetY = 6, Blur = 16, Spread = 0, Color
                    = Color.FromArgb(20, 0, 0, 0)
            },
            [
                new BoxShadow
                {
                    OffsetX = 0, OffsetY = 3, Blur = 6, Spread = -4,
                    Color   = Color.FromArgb(31, 0, 0, 0)
                },
                new BoxShadow
                {
                    OffsetX = 0, OffsetY = 9, Blur = 28, Spread = 8,
                    Color   = Color.FromArgb(13, 0, 0, 0)
                },
            ]);
    }
}
