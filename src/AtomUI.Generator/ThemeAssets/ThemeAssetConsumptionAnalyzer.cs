using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator;

/// <summary>
/// Static checks over the parsed theme AXAML assets of one package:
/// <c>ATOMUIGEN041</c> (global Token read kind versus the ControlTheme's Token identity, and load-time frozen
/// <c>SharedTokenResourceValue</c> reads in any ControlTheme),
/// <c>ATOMUIGEN045</c> (visual literals) and <c>ATOMUIGEN046</c> (template node attributes that mask state Setters).
/// The analysis is pure <see cref="XElement"/> traversal. Only provable patterns are reported; anything that cannot be
/// resolved statically is left to review. The parsed documents never enter the incremental cache.
/// </summary>
internal static class ThemeAssetConsumptionAnalyzer
{
    private const string AvaloniaNamespace = "https://github.com/avaloniaui";
    private const string SharedMarker = "shared:";
    private const string LayoutMarker = "layout";

    private static readonly Regex s_sharedTokenResource = new(
        @"\{\s*(?:[\w.-]+:)?SharedTokenResource\b(?:\s+(?:Kind\s*=\s*)?(?<name>[\w.]+))?",
        RegexOptions.CultureInvariant);

    // `{atom:SharedTokenResourceValue Kind=MotionDurationMid}`: a value read once from the application at load.
    private static readonly Regex s_frozenTokenResource = new(
        @"\{\s*(?:[\w.-]+:)?SharedTokenResourceValue\b(?:\s+(?:Kind\s*=\s*)?(?<name>[\w.]+))?",
        RegexOptions.CultureInvariant);

    private static readonly Regex s_hexColor = new(
        @"^#(?:[0-9A-Fa-f]{3}|[0-9A-Fa-f]{4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$",
        RegexOptions.CultureInvariant);

    // Signed numbers and comma/space separated tuples (Thickness, Point, CornerRadius).
    private static readonly Regex s_numeric = new(@"^[-+\d.,\s]+$", RegexOptions.CultureInvariant);

    // The last compound selector must be `<Type>#<Name>` (optionally followed by classes or pseudo-classes) after /template/.
    private static readonly Regex s_templateNodeSelector = new(
        @"/template/\s+(?<type>[\w|]*)#(?<name>[\w-]+)(?<rest>[^\s>]*)\s*$",
        RegexOptions.CultureInvariant);

    private static readonly Regex s_stateSegment = new(@"[:.][A-Za-z_]", RegexOptions.CultureInvariant);
    private static readonly Regex s_bracketGroup = new(@"\[[^\]]*\]", RegexOptions.CultureInvariant);

    // Attributes (and Setter Property names) that carry colour values.
    private static readonly HashSet<string> s_colorProperties = new(StringComparer.Ordinal)
    {
        "Background", "Foreground", "BorderBrush", "Fill", "Stroke", "Color"
    };

    // Attributes (and Setter Property names) that carry numeric visual values.
    private static readonly HashSet<string> s_numericProperties = new(StringComparer.Ordinal)
    {
        "CornerRadius", "BorderThickness", "FontSize", "StrokeThickness", "Width", "Height",
        "MinWidth", "MinHeight", "Padding", "Margin", "Opacity", "FontWeight", "Spacing", "ZIndex"
    };

    // Any property named `*Offset` (BadgeOffset, HorizontalOffset, VerticalOffset, ...) carries a numeric placement value.
    private static bool IsNumericProperty(string property) =>
        s_numericProperties.Contains(property) || property.EndsWith("Offset", StringComparison.Ordinal);

    // Avalonia's named colours except Transparent, which (like ATOMUIGEN044 for code) is not a visual decision.
    private static readonly HashSet<string> s_namedColors = new(
        ("AliceBlue AntiqueWhite Aqua Aquamarine Azure Beige Bisque Black BlanchedAlmond Blue BlueViolet Brown BurlyWood " +
         "CadetBlue Chartreuse Chocolate Coral CornflowerBlue Cornsilk Crimson Cyan DarkBlue DarkCyan DarkGoldenrod " +
         "DarkGray DarkGreen DarkKhaki DarkMagenta DarkOliveGreen DarkOrange DarkOrchid DarkRed DarkSalmon DarkSeaGreen " +
         "DarkSlateBlue DarkSlateGray DarkTurquoise DarkViolet DeepPink DeepSkyBlue DimGray DodgerBlue Firebrick " +
         "FloralWhite ForestGreen Fuchsia Gainsboro GhostWhite Gold Goldenrod Gray Green GreenYellow Honeydew HotPink " +
         "IndianRed Indigo Ivory Khaki Lavender LavenderBlush LawnGreen LemonChiffon LightBlue LightCoral LightCyan " +
         "LightGoldenrodYellow LightGray LightGreen LightPink LightSalmon LightSeaGreen LightSkyBlue LightSlateGray " +
         "LightSteelBlue LightYellow Lime LimeGreen Linen Magenta Maroon MediumAquamarine MediumBlue MediumOrchid " +
         "MediumPurple MediumSeaGreen MediumSlateBlue MediumSpringGreen MediumTurquoise MediumVioletRed MidnightBlue " +
         "MintCream MistyRose Moccasin NavajoWhite Navy OldLace Olive OliveDrab Orange OrangeRed Orchid PaleGoldenrod " +
         "PaleGreen PaleTurquoise PaleVioletRed PapayaWhip PeachPuff Peru Pink Plum PowderBlue Purple Red RosyBrown " +
         "RoyalBlue SaddleBrown Salmon SandyBrown SeaGreen SeaShell Sienna Silver SkyBlue SlateBlue SlateGray Snow " +
         "SpringGreen SteelBlue Tan Teal Thistle Tomato Turquoise Violet Wheat White WhiteSmoke Yellow YellowGreen")
        .Split(' '),
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Control names that referenced packages publish as Token owners (<c>ThemeBuildOwnerAttribute.Id</c>). Matching is by
    /// simple control name, the same granularity used for this package's own controls. The attribute is only read here;
    /// its validation belongs to <c>ThemeBuildContracts</c>.
    /// </summary>
    internal static IEnumerable<string> GetReferencedIdentityControls(Compilation compilation)
    {
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            foreach (var attribute in assembly.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == "AtomUI.Theme.Definitions.Compiled.ThemeBuildOwnerAttribute" &&
                    attribute.ConstructorArguments.Length > 2 &&
                    attribute.ConstructorArguments[2].Value is string { Length: > 0 } id)
                {
                    yield return id;
                }
            }
        }
    }

    /// <param name="tokenIdentityControls">Names of the controls of this package that have a Token identity.</param>
    internal static void Analyze(
        ThemeAssetInfo asset,
        XElement root,
        ISet<string> tokenIdentityControls,
        Action<Diagnostic> report)
    {
        var analysis = new Analysis(asset, tokenIdentityControls, report);
        analysis.Visit(root, null, false);
        foreach (var theme in root.DescendantsAndSelf().Where(IsControlTheme))
        {
            analysis.AnalyzeTemplateMasking(theme);
        }
    }

    internal static bool IsControlTheme(XElement element) =>
        string.Equals(element.Name.LocalName, "ControlTheme", StringComparison.Ordinal);

    private static bool IsStyle(XElement element) =>
        string.Equals(element.Name.LocalName, "Style", StringComparison.Ordinal);

    internal static string? AttributeValue(XElement element, string localName) =>
        element.Attributes().FirstOrDefault(attribute =>
            !attribute.IsNamespaceDeclaration && string.Equals(attribute.Name.LocalName, localName, StringComparison.Ordinal))?.Value;

    // A marker comment applies to the immediately following element only: it must be the nearest non-blank preceding node.
    private static bool HasPrecedingComment(XElement element, string marker)
    {
        for (var node = element.PreviousNode; node is not null; node = node.PreviousNode)
        {
            if (node is XText text && string.IsNullOrWhiteSpace(text.Value))
            {
                continue;
            }

            return node is XComment comment && comment.Value.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        return false;
    }

    // `Border.Background` -> `Background`
    private static string StripOwner(string propertyName)
    {
        var separator = propertyName.LastIndexOf('.');
        return separator >= 0 ? propertyName.Substring(separator + 1) : propertyName;
    }

    /// <summary>
    /// Resolves the TargetType of a <c>ControlTheme</c> element: its simple type name and whether that type is a Token owner
    /// (has a Token identity) among <paramref name="tokenIdentityControls"/>.
    /// </summary>
    internal static ThemeScope CreateScope(XElement controlTheme, ISet<string> tokenIdentityControls)
    {
        var targetType = AttributeValue(controlTheme, "TargetType")?.Trim();
        if (string.IsNullOrEmpty(targetType))
        {
            return new ThemeScope(null, false);
        }

        if (targetType!.StartsWith("{x:Type", StringComparison.Ordinal) && targetType.EndsWith("}", StringComparison.Ordinal))
        {
            targetType = targetType.Substring("{x:Type".Length, targetType.Length - "{x:Type".Length - 1).Trim();
        }

        var separator = targetType.IndexOf(':');
        var prefix = separator >= 0 ? targetType.Substring(0, separator) : string.Empty;
        var name = separator >= 0 ? targetType.Substring(separator + 1) : targetType;
        var xmlNamespace = prefix.Length == 0
            ? controlTheme.GetDefaultNamespace().NamespaceName
            : controlTheme.GetNamespaceOfPrefix(prefix)?.NamespaceName;
        // Types in the Avalonia namespace are framework primitives even when a package control shares the name.
        var hasIdentity = !string.Equals(xmlNamespace, AvaloniaNamespace, StringComparison.Ordinal) &&
                          tokenIdentityControls.Contains(name);
        return new ThemeScope(name, hasIdentity);
    }

    internal sealed class ThemeScope
    {
        internal ThemeScope(string? targetTypeName, bool hasIdentity)
        {
            TargetTypeName = targetTypeName;
            HasIdentity = hasIdentity;
        }

        internal string? TargetTypeName { get; }
        internal bool HasIdentity { get; }
    }

    private sealed class Analysis
    {
        private readonly ThemeAssetInfo _asset;
        private readonly ISet<string> _identities;
        private readonly Action<Diagnostic> _report;

        internal Analysis(ThemeAssetInfo asset, ISet<string> identities, Action<Diagnostic> report)
        {
            _asset = asset;
            _identities = identities;
            _report = report;
        }

        internal void Visit(XElement element, ThemeScope? theme, bool visualScope)
        {
            if (IsControlTheme(element))
            {
                theme = CreateScope(element, _identities);
                visualScope = true;
            }
            else if (IsStyle(element))
            {
                visualScope = true;
            }

            if (theme is not null)
            {
                AnalyzeSharedReads(element, theme);
            }

            if (visualScope)
            {
                AnalyzeVisualLiterals(element);
            }

            foreach (var child in element.Elements())
            {
                Visit(child, theme, visualScope);
            }
        }

        // ---- ATOMUIGEN041 -------------------------------------------------------------

        private void AnalyzeSharedReads(XElement element, ThemeScope theme)
        {
            foreach (var attribute in element.Attributes())
            {
                if (attribute.IsNamespaceDeclaration)
                {
                    continue;
                }

                var frozen = s_frozenTokenResource.Match(attribute.Value);
                if (frozen.Success)
                {
                    // No `shared:` exemption: the frozen value is wrong for every control, shared or not.
                    _report(Diagnostic.Create(
                        AtomUIDiagnosticDescriptors.ThemeAssetFrozenTokenRead,
                        LocateAttribute(attribute),
                        theme.TargetTypeName ?? "<unknown>",
                        frozen.Groups["name"].Success ? frozen.Groups["name"].Value : "?"));
                    continue;
                }

                var match = s_sharedTokenResource.Match(attribute.Value);
                if (!match.Success || HasPrecedingComment(element, SharedMarker))
                {
                    continue;
                }

                var tokenName = match.Groups["name"].Success ? match.Groups["name"].Value : "?";
                var replacement = theme.HasIdentity ? theme.TargetTypeName + "TokenResource" : "OwnerTokenResource";
                _report(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.ThemeAssetSharedReadMismatch,
                    LocateAttribute(attribute),
                    theme.TargetTypeName ?? "<unknown>",
                    tokenName,
                    replacement));
            }
        }

        // ---- ATOMUIGEN045 -------------------------------------------------------------

        private void AnalyzeVisualLiterals(XElement element)
        {
            var isSetter = string.Equals(element.Name.LocalName, "Setter", StringComparison.Ordinal);
            var setterProperty = isSetter ? AttributeValue(element, "Property") : null;
            foreach (var attribute in element.Attributes())
            {
                if (attribute.IsNamespaceDeclaration)
                {
                    continue;
                }

                string? property = null;
                if (isSetter)
                {
                    if (setterProperty is not null && string.Equals(attribute.Name.LocalName, "Value", StringComparison.Ordinal))
                    {
                        property = StripOwner(setterProperty.Trim());
                    }
                }
                else
                {
                    property = StripOwner(attribute.Name.LocalName);
                }

                if (property is null || !IsVisualLiteral(property, attribute.Value.Trim(), element))
                {
                    continue;
                }

                _report(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.ThemeAssetVisualLiteral,
                    LocateAttribute(attribute),
                    property,
                    attribute.Value.Trim()));
            }
        }

        private static bool IsVisualLiteral(string property, string value, XElement element)
        {
            if (s_colorProperties.Contains(property))
            {
                return s_hexColor.IsMatch(value) || s_namedColors.Contains(value);
            }

            if (!IsNumericProperty(property) || !s_numeric.IsMatch(value))
            {
                return false;
            }

            var numbers = value.Split([',', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                               .Select(static part => double.TryParse(
                                   part, System.Globalization.NumberStyles.Float,
                                   System.Globalization.CultureInfo.InvariantCulture, out var number)
                                   ? (double?)number
                                   : null)
                               .ToArray();
            if (numbers.Length == 0 || numbers.Any(static number => number is null))
            {
                return false;
            }

            // Zero removes the visual (no line, no padding); an Opacity of 0 or 1 is a visibility toggle.
            if (numbers.All(static number => number == 0) ||
                (property == "Opacity" && numbers.Length == 1 && numbers[0] == 1))
            {
                return false;
            }

            return !HasPrecedingComment(element, LayoutMarker);
        }

        // ---- ATOMUIGEN046 -------------------------------------------------------------

        internal void AnalyzeTemplateMasking(XElement theme)
        {
            var owned = theme.Descendants().Where(element => ReferenceEquals(OwnerOf(element), theme)).ToArray();
            var states = new List<StateStyle>();
            foreach (var style in owned.Where(IsStyle))
            {
                var selector = AttributeValue(style, "Selector");
                if (selector is null)
                {
                    continue;
                }

                var match = s_templateNodeSelector.Match(selector.Trim());
                if (!match.Success)
                {
                    continue;
                }

                var prefix = selector.Substring(0, selector.IndexOf("/template/", StringComparison.Ordinal));
                var stateText = s_bracketGroup.Replace(prefix + match.Groups["rest"].Value, string.Empty);
                if (!s_stateSegment.IsMatch(stateText))
                {
                    continue;
                }

                var properties = new HashSet<string>(style.Elements()
                    .Where(static child => string.Equals(child.Name.LocalName, "Setter", StringComparison.Ordinal))
                    .Select(static setter => AttributeValue(setter, "Property"))
                    .Where(static property => !string.IsNullOrWhiteSpace(property))
                    .Select(static property => StripOwner(property!.Trim())), StringComparer.Ordinal);
                if (properties.Count != 0)
                {
                    states.Add(new StateStyle(
                        selector.Trim(), StripNamespace(match.Groups["type"].Value), match.Groups["name"].Value, properties));
                }
            }

            if (states.Count == 0)
            {
                return;
            }

            foreach (var node in owned.Where(static element => element.Ancestors().Any(static ancestor =>
                         string.Equals(ancestor.Name.LocalName, "ControlTemplate", StringComparison.Ordinal))))
            {
                var nodeName = AttributeValue(node, "Name");
                if (string.IsNullOrEmpty(nodeName))
                {
                    continue;
                }

                foreach (var attribute in node.Attributes())
                {
                    if (IsNonProperty(attribute) || !IsDefaultValue(attribute.Value))
                    {
                        continue;
                    }

                    var propertyName = StripOwner(attribute.Name.LocalName);
                    var state = states.FirstOrDefault(candidate =>
                        candidate.NodeName == nodeName &&
                        (candidate.TypeName.Length == 0 || candidate.TypeName == node.Name.LocalName) &&
                        candidate.Properties.Contains(propertyName));
                    if (state is null)
                    {
                        continue;
                    }

                    _report(Diagnostic.Create(
                        AtomUIDiagnosticDescriptors.ThemeAssetTemplateAttributeMasksSetter,
                        LocateAttribute(attribute),
                        propertyName,
                        nodeName,
                        state.Selector));
                }
            }
        }

        private static XElement? OwnerOf(XElement element) => element.Ancestors().FirstOrDefault(IsControlTheme);

        private static string StripNamespace(string typeName)
        {
            var separator = typeName.LastIndexOf('|');
            return separator >= 0 ? typeName.Substring(separator + 1) : typeName;
        }

        private static bool IsNonProperty(XAttribute attribute)
        {
            if (attribute.IsNamespaceDeclaration)
            {
                return true;
            }

            var name = attribute.Name;
            return name.NamespaceName.Length != 0 ||
                   name.LocalName == "Name" ||
                   name.LocalName == "Classes" ||
                   name.LocalName.StartsWith("Classes.", StringComparison.Ordinal);
        }

        // Token reads and plain literals are both default values; bindings and other markup extensions are not.
        private static bool IsDefaultValue(string value)
        {
            var trimmed = value.TrimStart();
            return !trimmed.StartsWith("{", StringComparison.Ordinal) ||
                   trimmed.IndexOf("TokenResource", StringComparison.Ordinal) >= 0;
        }

        // ---- locations ----------------------------------------------------------------

        // The reported span covers `Name="value"` of the attribute as written in the source text.
        private Location LocateAttribute(XAttribute attribute)
        {
            var source = _asset.Source;
            if (((IXmlLineInfo)attribute).HasLineInfo())
            {
                var lineInfo = (IXmlLineInfo)attribute;
                var lineIndex = lineInfo.LineNumber - 1;
                if (lineIndex >= 0 && lineIndex < source.Lines.Count)
                {
                    var start = source.Lines[lineIndex].Start + lineInfo.LinePosition - 1;
                    var end = FindAttributeEnd(source, start);
                    var span = TextSpan.FromBounds(start, end);
                    return Location.Create(_asset.Path, span, source.Lines.GetLinePositionSpan(span));
                }
            }

            return _asset.CreateLocation();
        }

        private static int FindAttributeEnd(SourceText source, int start)
        {
            var length = source.Length;
            var index = start;
            while (index < length && source[index] != '=')
            {
                index++;
            }

            index++;
            while (index < length && char.IsWhiteSpace(source[index]))
            {
                index++;
            }

            if (index >= length || (source[index] != '"' && source[index] != '\''))
            {
                return Math.Min(Math.Max(index, start), length);
            }

            var quote = source[index];
            index++;
            while (index < length && source[index] != quote)
            {
                index++;
            }

            return Math.Min(index + 1, length);
        }
    }

    private sealed class StateStyle
    {
        internal StateStyle(string selector, string typeName, string nodeName, HashSet<string> properties)
        {
            Selector = selector;
            TypeName = typeName;
            NodeName = nodeName;
            Properties = properties;
        }

        internal string Selector { get; }
        internal string TypeName { get; }
        internal string NodeName { get; }
        internal HashSet<string> Properties { get; }
    }
}
