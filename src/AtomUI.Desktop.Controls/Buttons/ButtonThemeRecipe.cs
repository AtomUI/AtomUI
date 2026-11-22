using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Schema;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The appearance a design-language theme selects for its button owners (stored as the owner's control token
/// <c>AppearanceSettings</c>). Without a recipe the button keeps the Ant Design behaviour. Each switch is independent:
/// <list type="bullet">
/// <item><see cref="UsesRecipeVariants"/>: the recipe's colour model (soft danger fill, filled text from
/// <c>ControlItemColorActive</c>, no status shadows, default colours on an open flyout);</item>
/// <item><see cref="DisabledOpacity"/>: disabled buttons keep their colours at this opacity instead of the grey disabled
/// paint (null keeps the grey paint);</item>
/// <item><see cref="PressMoves"/>: pressing moves the button down 1px instead of switching to the pressed colours;</item>
/// <item><see cref="UnderlineLinkOnHover"/> / <see cref="UnderlineLinkAtRest"/>: link buttons underline their label.</item>
/// </list>
/// </summary>
internal sealed record ButtonThemeRecipe(
    bool OutlineOnlyDefaultShadow,
    bool UnderlineLinkAtRest = false,
    bool UsesRecipeVariants = true,
    double? DisabledOpacity = ButtonThemeInputs.DisabledOpacity,
    bool PressMoves = true,
    bool UnderlineLinkOnHover = true)
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
