namespace AtomUI.Generator;

internal sealed class ThemeAssetSemanticTemplateInfo
{
    internal ThemeAssetSemanticTemplateInfo(
        int index,
        IReadOnlyList<ThemeAssetSemanticMarkerInfo> markers)
    {
        Index = index;
        Markers = markers;
    }

    internal int Index { get; }
    internal IReadOnlyList<ThemeAssetSemanticMarkerInfo> Markers { get; }
}

internal sealed class ThemeAssetSemanticThemeInfo
{
    internal ThemeAssetSemanticThemeInfo(
        ThemeAssetTargetTypeReference targetType,
        ThemeAssetTargetTypeReference? basedOn,
        IReadOnlyList<ThemeAssetSemanticTemplateInfo> templates,
        bool overridesBaseTemplate,
        string? xmlClass)
    {
        TargetType = targetType;
        BasedOn = basedOn;
        Templates = templates;
        OverridesBaseTemplate = overridesBaseTemplate;
        XmlClass = xmlClass;
    }

    internal ThemeAssetTargetTypeReference TargetType { get; }
    internal ThemeAssetTargetTypeReference? BasedOn { get; }
    internal IReadOnlyList<ThemeAssetSemanticTemplateInfo> Templates { get; }
    internal bool OverridesBaseTemplate { get; }
    internal string? XmlClass { get; }
}

internal sealed class ThemeAssetSemanticMarkerInfo
{
    internal ThemeAssetSemanticMarkerInfo(
        string selectorClass,
        string xmlNamespace,
        string typeName,
        int nodeId,
        bool isStaticallyEnabled)
    {
        SelectorClass = selectorClass;
        XmlNamespace = xmlNamespace;
        TypeName = typeName;
        NodeId = nodeId;
        IsStaticallyEnabled = isStaticallyEnabled;
    }

    internal string SelectorClass { get; }
    internal string XmlNamespace { get; }
    internal string TypeName { get; }
    internal int NodeId { get; }
    internal bool IsStaticallyEnabled { get; }
}
