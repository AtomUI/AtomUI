using System.Collections.Immutable;
using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// MessageCard under the PrimeNG presets: <c>@primeuix/themes</c> 3.1.1 <c>toast</c> / <c>message</c> shadows — the
/// neutral message takes <c>overlay.popover.shadow</c> and each severity a single <c>0px 4px 8px 0px</c> layer in its
/// 500 stop at 4% (<c>color-mix(in srgb, {blue|green|yellow|red}.500, transparent 96%)</c>; the dark sections use the
/// same stops). The AtomUI-only <c>Loading</c> severity has no PrimeNG counterpart and takes the neutral value.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGMessageCardTokens : IControlTokenAlgorithm<MessageCardToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MessageCardToken token)
    {
        var resting = PrimeNGPresets.Resolve(context).Controls.PopoverShadows;
        // MessageCard has no severity-less variant in AtomUI; the AtomUI-only Loading severity takes the neutral
        // PrimeNG value (toast.normal = overlay.popover.shadow).
        token.InformationShadows = SeverityShadow(PrimeNGTailwindPalettes.Blue);
        token.SuccessShadows     = SeverityShadow(PrimeNGTailwindPalettes.Green);
        token.WarningShadows     = SeverityShadow(PrimeNGTailwindPalettes.Yellow);
        token.ErrorShadows       = SeverityShadow(PrimeNGTailwindPalettes.Red);
        token.LoadingShadows     = resting;
    }

    /// <summary>
    /// The PrimeUIX severity shadow: <c>0px 4px 8px 0px</c> in the palette's 500 stop at 4% opacity
    /// (96% transparent colour-mix; 4% of 255 rounds to alpha 0x0A, as the 10% layers round to 0x1A).
    /// </summary>
    internal static BoxShadows SeverityShadow(ImmutableArray<Color> palette) => new(new BoxShadow
    {
        OffsetX = 0,
        OffsetY = 4,
        Blur    = 8,
        Spread  = 0,
        Color   = Color.FromArgb(0x0A, palette[5].R, palette[5].G, palette[5].B)
    });
}
