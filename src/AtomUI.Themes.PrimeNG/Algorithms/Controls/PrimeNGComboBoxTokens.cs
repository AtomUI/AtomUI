using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// ComboBox under the PrimeNG presets: PrimeUIX 3.1.1 <c>list.option</c> — a row of the 0.875rem line (1.5 line height
/// = 21) plus 0.25rem padding top and bottom, so 29 instead of the Ant Design <c>ControlHeight</c>, and 0.625rem
/// horizontal padding. The row height also drives the popup's page size.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGComboBoxTokens : IControlTokenAlgorithm<ComboBoxToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ComboBoxToken token)
    {
        // list.option: padding 0.25rem 0.625rem over the 0.875rem line at the 16px root the preset renders at, so the
        // row is 14 * 1.5 + 2 * 4 = 29 and the label starts 10 in. Aura, Lara and Nora share the values, and PrimeUIX
        // does not scale the list row with the control size.
        token.ItemHeight  = 29;
        token.ItemPadding = new Thickness(10, 0);
    }
}
