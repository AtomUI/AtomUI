using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token of the value shown by <see cref="StatisticCountUp" />. It declares no member of its own; the
/// theme reads the value font size from it.
/// </summary>
[ControlDesignToken]
public sealed partial class StatisticCountUpToken : AbstractStatisticValueToken
{
}
