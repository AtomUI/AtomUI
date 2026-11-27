using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Animations;

/// <summary>
/// Identifies a component animation without referring to the component's CLR type.
/// </summary>
public enum MotionSlot
{
    SelectOpen, SelectClose,
    DropdownOpen, DropdownClose,
    SubMenuOpen, SubMenuClose,
    HorizontalMenuOpen, HorizontalMenuClose,
    NavigationMenuOpen, NavigationMenuClose,
    PopoverOpen, PopoverClose,
    TooltipOpen, TooltipClose,
    DialogOpen, DialogClose,
    DrawerOpen, DrawerClose,
    OverlayMaskOpen, OverlayMaskClose,
    InlineExpand, InlineCollapse,
    MessageEnter, MessageLeave,
    NotificationEnter, NotificationLeave
}

/// <summary>
/// Supplies optional component animation definitions for one design language.
/// </summary>
public abstract class MotionScheme
{
    /// <summary>
    /// Returns false to inherit, or true with null to disable the slot.
    /// </summary>
    public abstract bool TryGetMotion(MotionSlot slot, in DesignVariant variant, out AbstractMotion? motion);
}

internal static class ScopedMotionSelection
{
    internal static bool TryResolve(StyledElement target, MotionSlot slot, out AbstractMotion? motion)
    {
        if (TryResolveResource(target, slot, out motion))
        {
            return true;
        }
        return TryResolve(ThemeScope.ResolveContext(target)?.Snapshot, slot, out motion);
    }

    internal static bool TryResolveResource(StyledElement target, object slot, out AbstractMotion? motion)
    {
        if (target.TryFindResource(slot, target.ActualThemeVariant, out var resource))
        {
            if (resource is null || resource is AbstractMotion)
            {
                motion = (AbstractMotion?)resource;
                return true;
            }
            throw new InvalidOperationException($"Motion resource '{slot}' must be an AbstractMotion or null.");
        }
        motion = null;
        return false;
    }

    internal static bool TryResolve(ThemeSnapshot? snapshot, MotionSlot slot, out AbstractMotion? motion)
    {
        motion = null;
        if (snapshot is null)
        {
            return false;
        }
        var variant = snapshot.Variant;
        if (snapshot.Language.Language.MotionScheme?.TryGetMotion(slot, in variant, out motion) == true)
        {
            return true;
        }
        for (var type = snapshot.Language.LanguageTokenType.BaseType;
             type is not null && type != typeof(LanguageDesignToken); type = type.BaseType)
        {
            foreach (var candidate in snapshot.Registry.Languages)
            {
                if (candidate.LanguageTokenType == type &&
                    candidate.Language.MotionScheme?.TryGetMotion(slot, in variant, out motion) == true)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
