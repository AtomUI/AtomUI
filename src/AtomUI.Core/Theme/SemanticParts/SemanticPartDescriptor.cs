using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

public sealed class SemanticPartDescriptor
{
    public SemanticPartDescriptor(
        string name,
        string path,
        string? selectorClass,
        Type contractType,
        SemanticPartCardinality cardinality,
        SemanticPartCustomization customization,
        ControlThemeSemanticPartDescriptor? theme,
        bool crossVisualRoot,
        string? since,
        bool runtimeCreated,
        string? selectorRoute = null,
        Type? styleType = null,
        bool crossNestedOwners = false,
        bool restHidden = false)
    {
        ValidatePartName(name, nameof(name));
        ValidatePartPath(path, nameof(path));
        ArgumentNullException.ThrowIfNull(contractType);
        if (cardinality is not (SemanticPartCardinality.Single or
            SemanticPartCardinality.Optional or
            SemanticPartCardinality.Multiple))
        {
            throw new ArgumentOutOfRangeException(
                nameof(cardinality),
                cardinality,
                "Unsupported Semantic Part cardinality.");
        }
        if (customization is not (SemanticPartCustomization.Root or
            SemanticPartCustomization.Selector or
            SemanticPartCustomization.SelectorAndTheme))
        {
            throw new ArgumentOutOfRangeException(
                nameof(customization),
                customization,
                "Unsupported Semantic Part customization mode.");
        }
        if (!typeof(StyledElement).IsAssignableFrom(contractType))
        {
            throw new ArgumentException(
                $"Semantic Part ContractType '{contractType.FullName}' must derive from Avalonia.StyledElement.",
                nameof(contractType));
        }

        var isRoot = string.Equals(name, "root", StringComparison.Ordinal);
        if (isRoot)
        {
            if (!string.Equals(path, "root", StringComparison.Ordinal) ||
                selectorClass is not null ||
                cardinality != SemanticPartCardinality.Single ||
                customization != SemanticPartCustomization.Root ||
                theme is not null ||
                crossVisualRoot ||
                crossNestedOwners ||
                runtimeCreated ||
                restHidden ||
                selectorRoute is not null ||
                styleType is not null)
            {
                throw new ArgumentException(
                    "The root Semantic Part must use path 'root', Single cardinality, Root customization, and no selector, theme, cross-root, cross-nested, runtime-created, or rest-hidden metadata.",
                    nameof(name));
            }
        }
        else
        {
            if (string.Equals(path, "root", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Only the implicit root Semantic Part can use path 'root'.",
                    nameof(path));
            }
            ValidateSelectorClass(selectorClass, nameof(selectorClass));
            selectorRoute = NormalizeSelectorRoute(
                selectorRoute,
                selectorClass!,
                runtimeCreated,
                nameof(selectorRoute));
            if (customization == SemanticPartCustomization.Root)
            {
                throw new ArgumentException(
                    "Only the implicit root Semantic Part can use Root customization.",
                    nameof(customization));
            }
        }

        if (customization == SemanticPartCustomization.SelectorAndTheme)
        {
            if (theme is null)
            {
                throw new ArgumentException(
                    "SelectorAndTheme Semantic Parts require strongly typed theme metadata.",
                    nameof(theme));
            }
            if (!typeof(Control).IsAssignableFrom(contractType))
            {
                throw new ArgumentException(
                    "SelectorAndTheme Semantic Parts must target an Avalonia Control.",
                    nameof(contractType));
            }
        }
        else if (theme is not null)
        {
            throw new ArgumentException(
                "Semantic Part theme metadata is only valid with SelectorAndTheme customization.",
                nameof(theme));
        }

        Name = name;
        Path = path;
        SelectorClass = selectorClass;
        SelectorRoute = selectorRoute;
        ContractType = contractType;
        Cardinality = cardinality;
        Customization = customization;
        Theme = theme;
        CrossVisualRoot = crossVisualRoot;
        Since = string.IsNullOrWhiteSpace(since) ? null : since;
        RuntimeCreated = runtimeCreated;
        CrossNestedOwners = crossNestedOwners;
        RestHidden = restHidden;
        StyleType = styleType;
    }

    public string Name { get; }
    public string Path { get; }
    public string? SelectorClass { get; }
    public string? SelectorRoute { get; }
    public Type ContractType { get; }
    public SemanticPartCardinality Cardinality { get; }
    public SemanticPartCustomization Customization { get; }
    public ControlThemeSemanticPartDescriptor? Theme { get; }
    public bool CrossVisualRoot { get; }
    public string? Since { get; }
    public bool RuntimeCreated { get; }
    public bool CrossNestedOwners { get; }

    /// <summary>部件静止态透明/隐藏是设计语义；语义预览定位跳过 Opacity 资格过滤。</summary>
    public bool RestHidden { get; }

    public Type? StyleType { get; }

    private static void ValidatePartName(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (!IsValidPartSegments(value))
        {
            throw new ArgumentException(
                $"'{value}' is not a valid dot-separated camelCase Semantic Part name.",
                parameterName);
        }
    }

    private static void ValidatePartPath(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (!IsValidPartSegments(value))
        {
            throw new ArgumentException(
                $"'{value}' is not a valid Semantic Part path.",
                parameterName);
        }
    }

    private static bool IsValidPartSegments(ReadOnlySpan<char> value)
    {
        var start = 0;
        for (var index = 0; index <= value.Length; index++)
        {
            if (index == value.Length || value[index] == '.')
            {
                if (!IsCamelCaseSegment(value[start..index]))
                {
                    return false;
                }
                start = index + 1;
            }
        }
        return true;
    }

    private static bool IsCamelCaseSegment(ReadOnlySpan<char> value)
    {
        if (value.Length == 0 || value[0] is < 'a' or > 'z')
        {
            return false;
        }
        for (var index = 1; index < value.Length; index++)
        {
            if (value[index] is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9'))
            {
                return false;
            }
        }
        return true;
    }

    private static void ValidateSelectorClass(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (string.Equals(value, "semantic-root", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "'semantic-root' is reserved because the root Semantic Part is implicit.",
                parameterName);
        }

        if (!IsValidSelectorClass(value.AsSpan()))
        {
            throw new ArgumentException(
                $"'{value}' is not a valid Semantic Part selector class.",
                parameterName);
        }
    }

    private static string NormalizeSelectorRoute(
        string? value,
        string selectorClass,
        bool runtimeCreated,
        string parameterName)
    {
        if (value is null)
        {
            if (runtimeCreated)
            {
                throw new ArgumentException(
                    "Runtime-created Semantic Parts must declare an explicit SelectorRoute.",
                    parameterName);
            }

            return $"/template/ .{selectorClass}";
        }

        if (!IsValidSelectorRoute(value, selectorClass))
        {
            throw new ArgumentException(
                $"'{value}' is not a valid owner-relative Semantic Part selector route for '{selectorClass}'.",
                parameterName);
        }

        return value;
    }

    private static bool IsValidSelectorRoute(string value, string selectorClass)
    {
        if (value.Length == 0 || char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
        {
            return false;
        }

        var remaining = value.AsSpan();
        // 可选的首段自锚点：owner 节点自身携带的过滤类（方向限定等场景）。
        if (remaining[0] == '.')
        {
            var separator = remaining.IndexOf(' ');
            if (separator < 0 || !IsValidRouteClassToken(remaining[..separator]))
            {
                return false;
            }

            remaining = remaining[(separator + 1)..];
        }

        while (remaining.Length > 0)
        {
            var separator = remaining.IndexOf(' ');
            if (separator < 0)
            {
                return false;
            }
            var combinator = remaining[..separator];
            if (!combinator.SequenceEqual("/template/") &&
                !combinator.SequenceEqual(">") && !combinator.SequenceEqual(">>"))
            {
                return false;
            }
            remaining = remaining[(separator + 1)..];
            separator = remaining.IndexOf(' ');
            var target = separator < 0 ? remaining : remaining[..separator];
            if (!IsValidRouteClassToken(target))
            {
                return false;
            }
            if (separator < 0)
            {
                return target[1..].SequenceEqual(selectorClass);
            }
            remaining = remaining[(separator + 1)..];
        }
        return false;
    }

    private static bool IsValidRouteClassToken(ReadOnlySpan<char> token)
    {
        if (token.Length < 2 || token[0] != '.')
        {
            return false;
        }

        return IsValidSelectorClass(token[1..]);
    }

    private static bool IsValidSelectorClass(ReadOnlySpan<char> value)
    {
        const string prefix = "semantic-";
        if (!value.StartsWith(prefix, StringComparison.Ordinal) || value.Length == prefix.Length ||
            value.SequenceEqual("semantic-root"))
        {
            return false;
        }
        var previousWasHyphen = false;
        for (var index = prefix.Length; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '-')
            {
                if (previousWasHyphen || index == value.Length - 1)
                {
                    return false;
                }
                previousWasHyphen = true;
                continue;
            }
            if (character is not (>= 'a' and <= 'z') and not (>= '0' and <= '9'))
            {
                return false;
            }
            previousWasHyphen = false;
        }
        return true;
    }
}
