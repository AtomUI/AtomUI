namespace AtomUI.Desktop.Controls;

/// <summary>
/// An element of a <see cref="NavMenu" /> entry tree. The entry types are <see cref="INavMenuNode" />,
/// <see cref="NavMenuGroup" /> and <see cref="NavMenuDivider" />; owners accept no other type.
/// </summary>
public interface INavMenuEntry
{
}

/// <summary>
/// A divider line placed between the entries of a <see cref="NavMenu" />, of a node or of a
/// <see cref="NavMenuGroup" />. It carries no state, so the same instance can be placed in several positions.
/// </summary>
public sealed class NavMenuDivider : INavMenuEntry
{
}

internal static class NavMenuEntryOwnership
{
    public static bool IsTracked(INavMenuEntry entry)
    {
        return entry is NavMenuNode or NavMenuGroup;
    }

    public static void EnsureCanAttach(INavMenuEntry entry, object owner)
    {
        switch (entry)
        {
            case NavMenuNode node:
                node.EnsureCanAttachStructuralOwner(owner);
                break;

            case NavMenuGroup group:
                group.EnsureCanAttachStructuralOwner(owner);
                break;
        }
    }

    public static bool TryGetOwner(INavMenuEntry entry, out object? owner)
    {
        switch (entry)
        {
            case NavMenuNode node:
                return node.TryGetStructuralOwner(out owner);

            case NavMenuGroup group:
                return group.TryGetStructuralOwner(out owner);

            default:
                owner = null;
                return false;
        }
    }

    public static void Attach(INavMenuEntry entry, object owner)
    {
        switch (entry)
        {
            case NavMenuNode node:
                node.AttachStructuralOwner(owner);
                break;

            case NavMenuGroup group:
                group.AttachStructuralOwner(owner);
                break;
        }
    }

    public static void Detach(INavMenuEntry entry, object owner)
    {
        switch (entry)
        {
            case NavMenuNode node:
                node.DetachStructuralOwner(owner);
                break;

            case NavMenuGroup group:
                group.DetachStructuralOwner(owner);
                break;
        }
    }

    public static InvalidOperationException CreateDuplicateException(INavMenuEntry entry,
                                                                     object owner,
                                                                     int firstIndex,
                                                                     int duplicateIndex)
    {
        return new InvalidOperationException(
            $"{owner.GetType().Name} contains the same {entry.GetType().Name} instance at indexes " +
            $"{firstIndex} and {duplicateIndex}. Stateful NavMenu entries cannot appear more than once " +
            "in an entry tree.");
    }

    public static InvalidOperationException CreateTreeDuplicateException(INavMenuEntry entry,
                                                                         object owner,
                                                                         object firstOwner,
                                                                         object duplicateOwner)
    {
        return new InvalidOperationException(
            $"{owner.GetType().Name} contains the same {entry.GetType().Name} instance in " +
            $"{firstOwner.GetType().Name} and {duplicateOwner.GetType().Name}. Stateful NavMenu entries " +
            "cannot appear more than once in an entry tree.");
    }

    public static InvalidOperationException CreateAlreadyAttachedException(INavMenuEntry entry,
                                                                           object owner,
                                                                           object currentOwner)
    {
        return new InvalidOperationException(
            $"The {entry.GetType().Name} instance cannot be attached to {owner.GetType().Name} because it " +
            $"is already attached to {currentOwner.GetType().Name}. Remove it from its current owner before " +
            "adding it elsewhere in a NavMenu entry tree.");
    }
}
