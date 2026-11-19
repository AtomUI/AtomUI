using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;

namespace AtomUI.Desktop.Controls;

internal static class ToggleSwitchResources
{
    public static SwitchLoadingIndicatorTokenResourceKeys LoadingIndicatorTokenResourceKeys { get; } = new(
        ToggleSwitchTokenKind.SwitchDisabledOpacity,
        ControlTokenResourceKey.Global(ToggleSwitchTokens.Identity, SharedTokenKind.LineWidth));
}

public partial class ToggleSwitch : AbstractToggleSwitch
{
    private static readonly (object NormalMin, object NormalMax, object SmallMin, object SmallMax) s_contentMarginKeys =
        (ToggleSwitchTokenKind.InnerMinMargin, ToggleSwitchTokenKind.InnerMaxMargin,
         ToggleSwitchTokenKind.InnerMinMarginSM, ToggleSwitchTokenKind.InnerMaxMarginSM);

    private protected override (object NormalMin, object NormalMax, object SmallMin, object SmallMax)? ContentMarginKeys =>
        s_contentMarginKeys;

    public ToggleSwitch()
    {
    }
}
