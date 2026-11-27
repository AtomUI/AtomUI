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

/// <summary>
/// The toggle switch for desktop apps: a track that holds the on and the off content and a handle that slides to
/// the other end when the switch is checked. It derives from <see cref="AbstractToggleSwitch" /> and reads its
/// sizes and colors from <see cref="ToggleSwitchToken" /> through its theme.
/// </summary>
public partial class ToggleSwitch : AbstractToggleSwitch
{
    private static readonly (object NormalMin, object NormalMax, object SmallMin, object SmallMax) s_contentMarginKeys =
        (ToggleSwitchTokenKind.InnerMinMargin, ToggleSwitchTokenKind.InnerMaxMargin,
         ToggleSwitchTokenKind.InnerMinMarginSM, ToggleSwitchTokenKind.InnerMaxMarginSM);

    /// <summary>
    /// The keys of the <c>ToggleSwitchTokenKind</c> tokens that bound the inner content margin of the switch,
    /// as a minimum and a maximum margin, each for the normal and for the small size. Their values feed
    /// <c>InnerMinMargin</c> and <c>InnerMaxMargin</c> where those are not set explicitly.
    /// </summary>
    private protected override (object NormalMin, object NormalMax, object SmallMin, object SmallMax)? ContentMarginKeys =>
        s_contentMarginKeys;

    public ToggleSwitch()
    {
    }
}
