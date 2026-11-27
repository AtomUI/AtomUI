using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="WindowToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class WindowTokenRule : IControlTokenRule<AntDesignToken, WindowToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<WindowToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.DefaultBackground        = global.ColorBgContainer;
        token.DefaultForeground        = global.ColorText;
        token.CornerRadius             = new CornerRadius(12);
        token.SystemBarColor           = new SolidColorBrush(global.ColorBgContainer);
        // Window decoration does not follow the density algorithm. The compact algorithm shrinks ControlHeightLG
        // and SizeLG, and TitleBarHeight with them, so the height here is a cross-platform constant that cuts
        // the compact chain.
        token.TitleBarHeight               = 40;
        // The values match the translucent composite of the Win10 DWM shadow texture along the top edge of a window:
        // about #707070 over white while the window is active, about #AAAAAA while it is not. The border is thus a
        // dark gray with alpha, not a fixed solid color.
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
