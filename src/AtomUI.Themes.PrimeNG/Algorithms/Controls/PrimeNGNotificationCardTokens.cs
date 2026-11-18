using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// NotificationCard under the PrimeNG presets: <c>@primeuix/themes</c> 3.1.1 <c>toast</c> shadows — the default
/// notification (no severity) takes <c>overlay.popover.shadow</c> and each severity a single <c>0px 4px 8px 0px</c>
/// layer in its 500 stop at 4% (see <see cref="PrimeNGMessageCardTokens.SeverityShadow"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGNotificationCardTokens : IControlTokenAlgorithm<NotificationCardToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, NotificationCardToken token)
    {
        token.RestingShadows     = PrimeNGPresets.Resolve(context).Controls.PopoverShadows;
        token.InformationShadows = PrimeNGMessageCardTokens.SeverityShadow(PrimeNGTailwindPalettes.Blue);
        token.SuccessShadows     = PrimeNGMessageCardTokens.SeverityShadow(PrimeNGTailwindPalettes.Green);
        token.WarningShadows     = PrimeNGMessageCardTokens.SeverityShadow(PrimeNGTailwindPalettes.Yellow);
        token.ErrorShadows       = PrimeNGMessageCardTokens.SeverityShadow(PrimeNGTailwindPalettes.Red);
    }
}
