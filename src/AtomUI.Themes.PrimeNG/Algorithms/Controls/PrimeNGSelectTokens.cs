using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Select under the PrimeNG presets: option and panel padding. The option padding
/// (<c>UniformlyPaddingSM</c>, <c>UniformlyPaddingSM / 2 + 2</c>) is a fit to the baseline XML values (12,8;
/// compact 8,6), not a PrimeUIX derivation; PrimeUIX has no compact density. The panel padding is
/// <c>UniformlyPaddingXXS</c> (4, PrimeUIX <c>list.padding</c>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
internal sealed class PrimeNGSelectTokens : IControlTokenAlgorithm<SelectToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SelectToken token)
    {
        var horizontal = effectiveGlobal.UniformlyPaddingSM;
        token.OptionPadding       = new Thickness(horizontal, horizontal / 2 + 2);
        token.PopupContentPadding = new Thickness(effectiveGlobal.UniformlyPaddingXXS);
    }
}
