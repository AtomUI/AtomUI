using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Schema;

namespace AtomUI.Desktop.Controls;

internal sealed record ButtonThemeRecipe(bool OutlineOnlyDefaultShadow, bool UnderlineLinkAtRest = false)
{
    internal static ButtonThemeRecipe? Get(Button button)
    {
        if (ThemeScope.ResolveContext(button) is not { } context)
        {
            return null;
        }

        var snapshot = context.Snapshot;
        return snapshot.Registry.TryGetControl(OwnerOf(button), out var owner)
            ? snapshot.Controls[owner.Slot].AppearanceSettings as ButtonThemeRecipe
            : null;
    }

    internal static ControlTokenIdentity OwnerOf(Button button) => button switch
    {
        DropdownButton => DropdownButtonTokens.Identity,
        SubmitButton => SubmitButtonTokens.Identity,
        ResetButton => ResetButtonTokens.Identity,
        _ => ButtonTokens.Identity
    };
}
