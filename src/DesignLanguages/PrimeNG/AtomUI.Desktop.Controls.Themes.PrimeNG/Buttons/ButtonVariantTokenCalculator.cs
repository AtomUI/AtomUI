using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractButtonVariantToken" /> layer.
/// </summary>
public static class ButtonVariantTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared button variant layer: the normal, hover and active colors of the
    /// outlined and text variants, the link, ghost, solid, primary and danger colors, the link and danger accent
    /// colors, and the shadows of the default, primary and danger buttons.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractButtonVariantToken token)
    {
        var controlOutlineWidth = global.ControlOutlineWidth;
        // Ant Design formula kept as is; the PrimeNG button stage (PrimeNGButtonFamilyTokens.EvaluateOwn)
        // clears the three shadows and replaces the Default and Text variant colors and the link and danger
        // accents; in dark also DangerColor and SolidTextColor.
        token.DefaultShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = global.ColorControlOutline
        });

        token.PrimaryShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = global.ColorControlOutline
        });

        token.DangerShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = global.ColorErrorOutline
        });


        token.PrimaryColor            = global.ColorTextLightSolid;
        token.DangerColor             = global.ColorTextLightSolid;
        token.DefaultGhostColor       = global.ColorBgContainer;
        token.GhostBg                 = Colors.Transparent;
        token.DefaultGhostBorderColor = global.ColorBgContainer;

        token.LinkHoverBg                = Colors.Transparent;
        token.LinkColor                  = global.ColorLink ?? global.ColorInfo;
        token.DangerAccentColor          = global.ColorError;
        token.TextHoverBg                = global.ColorFillTertiary;
        token.DefaultColor               = global.ColorText;
        token.DefaultBg                  = global.ColorBgContainer;
        token.DefaultBorderColor         = global.ColorBorder;
        token.DefaultHoverBg             = global.ColorBgContainer;
        token.DefaultHoverColor          = global.ColorPrimaryHover;
        token.DefaultHoverBorderColor    = global.ColorPrimaryHover;
        token.DefaultActiveBg            = global.ColorBgContainer;
        token.DefaultActiveColor         = global.ColorPrimaryActive;
        token.DefaultActiveBorderColor   = global.ColorPrimaryActive;

        var isBright = ColorUtils.IsBright(global.ColorBgSolid, Colors.White);
        if (isBright)
        {
            token.SolidTextColor = Colors.Black;
        }
        else
        {
            token.SolidTextColor = Colors.White;
        }

        token.TextTextColor       = global.ColorText;
        token.TextTextHoverColor  = global.ColorText;
        token.TextTextActiveColor = global.ColorText;
    }
}
