using AtomUI.Localization;

namespace AtomUI.Toolkits.GalleryBase.Localization;

/// <summary>
/// Language resources of the Semantic Part preview in a showcase: the captions of the tabs that switch
/// between the examples and the Semantic Parts, the texts of the part list, the part details and the
/// styling example, and the fallback descriptions of parts.
/// </summary>
[LanguageCatalog]
public enum SemanticPartPreviewLangResourceKind
{
    /// <summary>
    /// Caption of the tab that shows the examples of the showcase.
    /// </summary>
    ExamplesTabLabel,
    /// <summary>
    /// Caption of the tab that shows the Semantic Part preview of the showcase.
    /// </summary>
    SemanticPartsTabLabel,
    /// <summary>
    /// Tooltip of the pin button of a part. Pinning keeps the highlight on the part.
    /// </summary>
    PinAction,
    /// <summary>
    /// Tooltip of the button of a part that opens its technical details and styling example.
    /// </summary>
    InfoAction,
    /// <summary>
    /// Label of the selector row in the part details.
    /// </summary>
    SelectorLabel,
    /// <summary>
    /// Label of the contract type row in the part details.
    /// </summary>
    ContractTypeLabel,
    /// <summary>
    /// Label of the cardinality row in the part details.
    /// </summary>
    CardinalityLabel,
    /// <summary>
    /// Label of the customization row in the part details.
    /// </summary>
    CustomizationLabel,
    /// <summary>
    /// Heading of the styling example shown in the details pane.
    /// </summary>
    StylingExampleTitle,
    /// <summary>
    /// Tag in the part details for a part whose targets lie outside the visual root of its owner.
    /// </summary>
    CrossRootTag,
    /// <summary>
    /// Tag in the part details for a part that is created at runtime instead of by the control template.
    /// </summary>
    RuntimeCreatedTag,
    /// <summary>
    /// Description of the root part when the showcase declares no description for it.
    /// </summary>
    RootFallbackDescription,
    /// <summary>
    /// Description of a part other than the root when the showcase declares no description for it.
    /// The placeholder {0} is replaced with the name of the part.
    /// </summary>
    PartFallbackDescription,
    /// <summary>
    /// Heading of the part details.
    /// </summary>
    PartDetailsTitle,
    /// <summary>
    /// Label of the selector route row in the part details.
    /// </summary>
    SelectorRouteLabel,
    /// <summary>
    /// Label of the style type row in the part details.
    /// </summary>
    StyleTypeLabel
}
