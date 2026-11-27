using System.Runtime.CompilerServices;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The behavior Own Tokens of a button owner (see <see cref="AbstractButtonGeometryToken" />), read once per theme
/// snapshot and owner. A button outside a themed root gets <see cref="Default" />, the Ant Design behavior.
/// </summary>
internal sealed record ButtonBehavior(
    bool RecipeVariants,
    bool OutlineOnlyDefaultShadow,
    bool DisabledByOpacity,
    double DisabledOpacity,
    bool PressMoves,
    bool UnderlineLinkOnHover,
    bool UnderlineLinkAtRest)
{
    internal static readonly ButtonBehavior Default = new(false, false, false, 1.0, false, false, false);

    private static readonly ThemeTokenResolver s_resolver = new();

    // The four button owners in OwnerOf order; a snapshot's values never change, so they are cached with it.
    private static readonly ConditionalWeakTable<ThemeSnapshot, ButtonBehavior?[]> s_cache = new();

    internal static ButtonBehavior Get(Button button)
    {
        if (ThemeScope.ResolveContext(button) is not { } context)
        {
            return Default;
        }

        var snapshot = context.Snapshot;
        var index    = OwnerIndexOf(button);
        var values   = s_cache.GetValue(snapshot, static _ => new ButtonBehavior?[4]);
        return values[index] ??= Read(snapshot, index);
    }

    internal static ControlTokenIdentity OwnerOf(Button button) => IdentityOf(OwnerIndexOf(button));

    private static ControlTokenIdentity IdentityOf(int index) => index switch
    {
        1 => DropdownButtonTokens.Identity,
        2 => SubmitButtonTokens.Identity,
        3 => ResetButtonTokens.Identity,
        _ => ButtonTokens.Identity
    };

    private static int OwnerIndexOf(Button button) => button switch
    {
        DropdownButton => 1,
        SubmitButton => 2,
        ResetButton => 3,
        _ => 0
    };

    private static ButtonBehavior Read(ThemeSnapshot snapshot, int index)
    {
        if (!snapshot.Registry.TryGetControl(IdentityOf(index), out var owner))
        {
            return Default;
        }

        var slot = owner.Slot;
        return index switch
        {
            1 => new ButtonBehavior(
                s_resolver.GetOwn<bool>(snapshot, slot, DropdownButtonTokenKind.RecipeVariants),
                s_resolver.GetOwn<bool>(snapshot, slot, DropdownButtonTokenKind.OutlineOnlyDefaultShadow),
                s_resolver.GetOwn<bool>(snapshot, slot, DropdownButtonTokenKind.DisabledByOpacity),
                s_resolver.GetOwn<double>(snapshot, slot, DropdownButtonTokenKind.DisabledOpacity),
                s_resolver.GetOwn<bool>(snapshot, slot, DropdownButtonTokenKind.PressMoves),
                s_resolver.GetOwn<bool>(snapshot, slot, DropdownButtonTokenKind.UnderlineLinkOnHover),
                s_resolver.GetOwn<bool>(snapshot, slot, DropdownButtonTokenKind.UnderlineLinkAtRest)),
            2 => new ButtonBehavior(
                s_resolver.GetOwn<bool>(snapshot, slot, SubmitButtonTokenKind.RecipeVariants),
                s_resolver.GetOwn<bool>(snapshot, slot, SubmitButtonTokenKind.OutlineOnlyDefaultShadow),
                s_resolver.GetOwn<bool>(snapshot, slot, SubmitButtonTokenKind.DisabledByOpacity),
                s_resolver.GetOwn<double>(snapshot, slot, SubmitButtonTokenKind.DisabledOpacity),
                s_resolver.GetOwn<bool>(snapshot, slot, SubmitButtonTokenKind.PressMoves),
                s_resolver.GetOwn<bool>(snapshot, slot, SubmitButtonTokenKind.UnderlineLinkOnHover),
                s_resolver.GetOwn<bool>(snapshot, slot, SubmitButtonTokenKind.UnderlineLinkAtRest)),
            3 => new ButtonBehavior(
                s_resolver.GetOwn<bool>(snapshot, slot, ResetButtonTokenKind.RecipeVariants),
                s_resolver.GetOwn<bool>(snapshot, slot, ResetButtonTokenKind.OutlineOnlyDefaultShadow),
                s_resolver.GetOwn<bool>(snapshot, slot, ResetButtonTokenKind.DisabledByOpacity),
                s_resolver.GetOwn<double>(snapshot, slot, ResetButtonTokenKind.DisabledOpacity),
                s_resolver.GetOwn<bool>(snapshot, slot, ResetButtonTokenKind.PressMoves),
                s_resolver.GetOwn<bool>(snapshot, slot, ResetButtonTokenKind.UnderlineLinkOnHover),
                s_resolver.GetOwn<bool>(snapshot, slot, ResetButtonTokenKind.UnderlineLinkAtRest)),
            _ => new ButtonBehavior(
                s_resolver.GetOwn<bool>(snapshot, slot, ButtonTokenKind.RecipeVariants),
                s_resolver.GetOwn<bool>(snapshot, slot, ButtonTokenKind.OutlineOnlyDefaultShadow),
                s_resolver.GetOwn<bool>(snapshot, slot, ButtonTokenKind.DisabledByOpacity),
                s_resolver.GetOwn<double>(snapshot, slot, ButtonTokenKind.DisabledOpacity),
                s_resolver.GetOwn<bool>(snapshot, slot, ButtonTokenKind.PressMoves),
                s_resolver.GetOwn<bool>(snapshot, slot, ButtonTokenKind.UnderlineLinkOnHover),
                s_resolver.GetOwn<bool>(snapshot, slot, ButtonTokenKind.UnderlineLinkAtRest))
        };
    }
}
