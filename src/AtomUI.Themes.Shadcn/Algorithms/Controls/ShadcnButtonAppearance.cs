using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

internal static class ShadcnButtonAppearance
{
    internal static void ApplyGlobal(in ThemeAlgorithmContext context, DesignToken token)
    {
        var dark = context.Appearance == ThemeAppearance.Dark;
        var surface = ShadcnBaseColors.Resolve(token.ColorNeutral, dark);
        var theme = ShadcnThemeColor.ResolveComputed(token.ColorPrimary, surface, dark);
        var hasColorTheme = theme.Theme is not null and not "neutral";

        // Official color themes use Zinc for the secondary surface. A neutral theme follows the base color.
        var secondary = hasColorTheme ? ShadcnTailwindPalette.Stop("zinc", dark ? 800 : 100) : surface.Secondary;
        token.ControlItemColorActive = hasColorTheme
            ? ShadcnTailwindPalette.Stop("zinc", dark ? 50 : 900)
            : surface.SecondaryForeground;
        token.ColorFillTertiary = secondary;
        token.ColorFillSecondary = MixSecondaryHover(secondary, token.ColorText);

        token.ColorErrorBg = MultiplyAlpha(token.ColorError, dark ? 0.2 : 0.1);
        token.ColorErrorBgHover = MultiplyAlpha(token.ColorError, dark ? 0.3 : 0.2);

        if (!context.HasConfiguredValue(SharedTokenKind.ColorLink))
        {
            token.ColorLink = token.ColorPrimary;
            token.ColorLinkHover = token.ColorPrimary;
            token.ColorLinkActive = token.ColorPrimary;
        }
    }

    internal static void ApplyOwn(in ThemeAlgorithmContext context, DesignToken global, AbstractButtonGeometryToken token)
    {
        var outline = ResolveOutline(in context, global);
        if (outline.Background is { } background)
        {
            token.DefaultBg = background;
        }
        token.DefaultHoverBg = outline.HoverBackground;
        token.DefaultBorderColor = outline.Border;
        token.DefaultHoverBorderColor = outline.Border;
        token.DefaultHoverColor = global.ColorText;

        var dark = context.Appearance == ThemeAppearance.Dark;
        token.TextHoverBg = dark ? MultiplyAlpha(global.ColorFillContent, 0.5) : global.ColorFillContent;
        token.TextTextHoverColor = global.ColorText;
        token.LinkHoverBg = Colors.Transparent;
        // Keep destructive text readable on a tinted hover surface in either appearance.
        token.DangerColor = dark
            ? ShadcnBaseColors.CompositeOver(global.ColorError, 0.80, Colors.White)
            : ShadcnBaseColors.CompositeOver(global.ColorError, 0.78, Colors.Black);
    }

    internal static (Color? Background, Color HoverBackground, Color Border) ResolveOutline(
        in ThemeAlgorithmContext context, DesignToken global)
    {
        var style = ShadcnStyles.Resolve(context);
        var dark = context.Appearance == ThemeAppearance.Dark;
        var surface = ShadcnBaseColors.Resolve(global.ColorNeutral, dark);
        var darkInputOutline = dark && (style == ShadcnStyles.Nova || style == ShadcnStyles.Vega || style == ShadcnStyles.Lyra);
        var inputHover = darkInputOutline || style == ShadcnStyles.Maia || style == ShadcnStyles.Mira;

        Color? background = null;
        if (style == ShadcnStyles.Maia || darkInputOutline || (dark && style == ShadcnStyles.Mira))
        {
            background = MultiplyAlpha(surface.Input, 0.3);
        }
        else if (dark || style == ShadcnStyles.Mira || style == ShadcnStyles.Sera)
        {
            background = Colors.Transparent;
        }

        var hoverBackground = inputHover
            ? MultiplyAlpha(surface.Input, 0.5)
            : dark ? MultiplyAlpha(surface.Input, 0.3) : global.ColorFillContent;
        return (background, hoverBackground, darkInputOutline ? surface.Input : global.ColorBorder);
    }

    private static Color MultiplyAlpha(Color color, double opacity) =>
        ShadcnStyles.WithAlpha(color, (byte)Math.Round(color.A * opacity, MidpointRounding.AwayFromZero));

    private static Color MixSecondaryHover(Color secondary, Color foreground)
    {
        const double foregroundWeight = 0.05;
        var start = ShadcnBaseColors.OklchOf(secondary);
        var end = ShadcnBaseColors.OklchOf(foreground);
        // In OKLCH an achromatic endpoint borrows the other endpoint's hue. Mix along the shorter hue arc.
        var startHue = start.Chroma < 0.0001 ? end.Hue : start.Hue;
        var endHue = end.Chroma < 0.0001 ? startHue : end.Hue;
        return ShadcnBaseColors.ColorFromOklch(
            start.Lightness + (end.Lightness - start.Lightness) * foregroundWeight,
            start.Chroma + (end.Chroma - start.Chroma) * foregroundWeight,
            startHue + Math.IEEERemainder(endHue - startHue, Math.Tau) * foregroundWeight);
    }
}
