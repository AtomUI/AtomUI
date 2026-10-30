using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "avatar",
    SelectorClass = "semantic-avatar",
    ContractType = typeof(SkeletonAvatar),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(SkeletonTitle),
    Since = "6.2.0")]
[SemanticPart(
    "paragraph",
    SelectorClass = "semantic-paragraph",
    ContractType = typeof(SkeletonParagraph),
    Since = "6.2.0")]
public partial class Skeleton
{
}
