using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

public sealed class ControlSemanticDescriptor
{
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
        // 限定部件（如 source.header）与未限定部件共享终端 selector class，
        // 去重键使用解析路由：真正需要防重的是完全相同的路由声明。
        EnsureUnique(
            partArray.Where(static part => part.SelectorClass is not null),
            static part => part.SelectorRoute ?? $"/template/ .{part.SelectorClass}",
            "selector route");

        ControlType = controlType;
        Identity = identity;
        Parts = Array.AsReadOnly(partArray);
    }

    public Type ControlType { get; }
    public ControlTokenIdentity Identity { get; }
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
