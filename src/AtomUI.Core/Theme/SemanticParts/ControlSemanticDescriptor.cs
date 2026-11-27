using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Describes the Semantic Parts of one control type for the theme system: the control type, its token identity
/// and its parts.
/// </summary>
public sealed class ControlSemanticDescriptor
{
    /// <summary>
    /// Creates the descriptor for a control from its parts. <paramref name="parts" /> must contain exactly one part
    /// named <c>root</c> whose <see cref="SemanticPartDescriptor.ContractType" /> is
    /// <paramref name="controlType" />, and the names, paths and selector routes of the parts must be unique. The
    /// parts are stored root first and then by path.
    /// </summary>
    public ControlSemanticDescriptor(
        Type controlType,
        ControlTokenIdentity identity,
        IEnumerable<SemanticPartDescriptor> parts)
    {
        ArgumentNullException.ThrowIfNull(controlType);
        ArgumentNullException.ThrowIfNull(parts);
        if (!typeof(Control).IsAssignableFrom(controlType))
        {
            throw new ArgumentException(
                $"Semantic control type '{controlType.FullName}' must derive from Avalonia.Controls.Control.",
                nameof(controlType));
        }

        SemanticPartDescriptor?[] nullableParts = parts.ToArray();
        if (nullableParts.Any(static part => part is null))
        {
            throw new ArgumentException("Semantic Part descriptors cannot contain null.", nameof(parts));
        }
        var partArray = nullableParts
                        .Select(static part => part!)
                        .OrderBy(static part => part.Name == "root" ? 0 : 1)
                        .ThenBy(static part => part.Path, StringComparer.Ordinal)
                        .ToArray();

        var roots = partArray.Where(static part => part.Name == "root").ToArray();
        if (roots.Length != 1 || roots[0].ContractType != controlType)
        {
            throw new ArgumentException(
                "A Control semantic descriptor requires exactly one root whose ContractType is the Control type.",
                nameof(parts));
        }

        EnsureUnique(partArray, static part => part.Name, "name");
        EnsureUnique(partArray, static part => part.Path, "path");
        // A qualified part (such as source.header) and an unqualified part can share the terminal selector class, so
        // the key is the resolved route: only identical route declarations must be rejected.
        EnsureUnique(
            partArray.Where(static part => part.SelectorClass is not null),
            static part => part.SelectorRoute ?? $"/template/ .{part.SelectorClass}",
            "selector route");

        ControlType = controlType;
        Identity = identity;
        Parts = Array.AsReadOnly(partArray);
    }

    /// <summary>
    /// The type of the control the parts belong to. It must derive from <see cref="Control" /> and is the contract
    /// type of the root part.
    /// </summary>
    public Type ControlType { get; }
    /// <summary>
    /// The <see cref="ControlTokenIdentity" /> of the control. <c>SemanticPartRegistry</c> looks descriptors up by
    /// it as well as by <see cref="ControlType" />.
    /// </summary>
    public ControlTokenIdentity Identity { get; }
    /// <summary>
    /// The Semantic Parts of the control, the root first and the rest in ordinal path order.
    /// </summary>
    public IReadOnlyList<SemanticPartDescriptor> Parts { get; }

    private static void EnsureUnique(
        IEnumerable<SemanticPartDescriptor> parts,
        Func<SemanticPartDescriptor, string> keySelector,
        string kind)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var part in parts)
        {
            var key = keySelector(part);
            if (!keys.Add(key))
            {
                throw new ArgumentException(
                    $"Control semantic descriptor contains duplicate Part {kind} '{key}'.",
                    nameof(parts));
            }
        }
    }
}
