using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

/// <summary>
/// The run-time description of one semantic part of a control. The source generator creates one for each
/// <see cref="SemanticPartAttribute" /> declaration on the control and one for the implicit <c>root</c> part.
/// The Gallery semantic preview reads them to locate and outline the parts.
/// </summary>
public sealed class SemanticPartDescriptor
{
    /// <summary>
    /// Validates the values and stores each of them in the property of the same name. A part named <c>root</c> is
    /// accepted only as the implicit root: path <c>root</c>, <see cref="SemanticPartCardinality.Single" />
    /// cardinality, <see cref="SemanticPartCustomization.Root" /> customization and no other metadata.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">
    /// <paramref name="name" />, <paramref name="path" /> or <paramref name="contractType" /> is
    /// <see langword="null" />, or <paramref name="selectorClass" /> is <see langword="null" /> for a part other
    /// than the root.
    /// </exception>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// <paramref name="cardinality" /> or <paramref name="customization" /> is not a defined enumeration value.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// A value is malformed or not allowed for the part, or <paramref name="theme" /> does not match
    /// <paramref name="customization" />.
    /// </exception>
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

    /// <summary>
    /// The name of the part, unique within its control; <c>root</c> for the implicit root part.
    /// See <see cref="SemanticPartAttribute.Name" />.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The path of the part, which names its dedicated style class; <c>root</c> for the implicit root part.
    /// See <see cref="SemanticPartAttribute.Path" />.
    /// </summary>
    public string Path { get; }
    /// <summary>
    /// The class token on the part element, or <see langword="null" /> for the implicit root part.
    /// See <see cref="SemanticPartAttribute.SelectorClass" />.
    /// </summary>
    public string? SelectorClass { get; }
    /// <summary>
    /// The selector route from the control to the part element. It is <see langword="null" /> for the implicit
    /// root part only; for a declared part an undeclared route is stored as <c>/template/ .</c> followed by
    /// <see cref="SelectorClass" />. See <see cref="SemanticPartAttribute.SelectorRoute" />.
    /// </summary>
    public string? SelectorRoute { get; }
    /// <summary>
    /// The type of element that implements the part; the control type itself for the implicit root part.
    /// See <see cref="SemanticPartAttribute.ContractType" />.
    /// </summary>
    public Type ContractType { get; }
    /// <summary>
    /// The number of elements the part has; <see cref="SemanticPartCardinality.Single" /> for the implicit root
    /// part. See <see cref="SemanticPartAttribute.Cardinality" />.
    /// </summary>
    public SemanticPartCardinality Cardinality { get; }
    /// <summary>
    /// How applications restyle the part; <see cref="SemanticPartCustomization.Root" /> for the implicit root
    /// part. See <see cref="SemanticPartAttribute.Customization" />.
    /// </summary>
    public SemanticPartCustomization Customization { get; }
    /// <summary>
    /// The theme metadata of the part. It is required when <see cref="Customization" /> is
    /// <see cref="SemanticPartCustomization.SelectorAndTheme" /> and <see langword="null" /> for every other
    /// customization. It describes the <see cref="Avalonia.Styling.ControlTheme" /> property of the control that
    /// <see cref="SemanticPartAttribute.ThemePropertyName" /> names.
    /// </summary>
    public ControlThemeSemanticPartDescriptor? Theme { get; }
    /// <summary>
    /// Whether the part element lives in another visual root than the control, so that the semantic preview must
    /// locate and highlight it there. See <see cref="SemanticPartAttribute.CrossVisualRoot" />.
    /// </summary>
    public bool CrossVisualRoot { get; }
    /// <summary>
    /// The release version in which the part was introduced, or <see langword="null" /> when the value was blank.
    /// See <see cref="SemanticPartAttribute.Since" />.
    /// </summary>
    public string? Since { get; }
    /// <summary>
    /// Whether the control creates the part element in code at run time instead of the theme template.
    /// See <see cref="SemanticPartAttribute.RuntimeCreated" />.
    /// </summary>
    public bool RuntimeCreated { get; }
    /// <summary>
    /// Whether the selector route crosses from the control's template into a nested control that has a template
    /// of its own. See <see cref="SemanticPartAttribute.CrossNestedOwners" />.
    /// </summary>
    public bool CrossNestedOwners { get; }

    /// <summary>
    /// Whether the part is transparent or hidden by design while the control is at rest, in which case the
    /// semantic preview skips its <c>Opacity</c> &gt; 0 eligibility check and still outlines the part.
    /// See <see cref="SemanticPartAttribute.RestHidden" />.
    /// </summary>
    public bool RestHidden { get; }

    /// <summary>
    /// The type of the dedicated style class of the part, or <see langword="null" /> for the implicit root part.
    /// </summary>
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
        // Optional leading self-anchor: a filter class that the owner node itself carries, such as a direction limit.
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
