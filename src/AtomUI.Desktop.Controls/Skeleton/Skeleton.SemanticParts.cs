using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// 占位块由 PlaceholderTemplate 在 IsLoading 为真时按需构建，模板父级为 Skeleton；标记静态写在占位模板中。

[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-header",
    RuntimeCreated = true,
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    SelectorRoute = "/template/ .semantic-section",
    RuntimeCreated = true,
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "avatar",
    SelectorClass = "semantic-avatar",
    SelectorRoute = "/template/ .semantic-avatar",
    RuntimeCreated = true,
    ContractType = typeof(SkeletonAvatar),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-title",
    RuntimeCreated = true,
    ContractType = typeof(SkeletonTitle),
    Since = "6.2.0")]
[SemanticPart(
    "paragraph",
    SelectorClass = "semantic-paragraph",
    SelectorRoute = "/template/ .semantic-paragraph",
    RuntimeCreated = true,
    ContractType = typeof(SkeletonParagraph),
    Since = "6.2.0")]
public partial class Skeleton
{
}
