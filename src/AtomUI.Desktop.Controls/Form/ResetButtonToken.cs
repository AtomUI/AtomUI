using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design token of <see cref="ResetButton" />. It declares no value of its own: the button is painted and
/// measured with the values of <see cref="AbstractStandardButtonToken" />, per state and size. Each design
/// language fills those values in its own token rule.
/// </summary>
[ControlDesignToken]
public sealed partial class ResetButtonToken : AbstractStandardButtonToken
{
}
