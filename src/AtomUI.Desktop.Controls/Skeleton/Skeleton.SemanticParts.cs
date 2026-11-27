using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The placeholder blocks are built on demand from PlaceholderTemplate while IsLoading is true, with the Skeleton as
// their templated parent; the selector classes are written statically in the placeholder template.

// The left column that holds the avatar. Created from the placeholder template only while IsLoading is true.
// SkeletonHeaderStyle applies to it.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-header",
    RuntimeCreated = true,
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
// The right column that stacks the title above the paragraph. Created from the placeholder template only while
// IsLoading is true. SkeletonSectionStyle applies to it.
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    SelectorRoute = "/template/ .semantic-section",
    RuntimeCreated = true,
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The avatar placeholder in the left column. Created from the placeholder template only while IsLoading is true;
// visible only while IsShowAvatar is true. SkeletonAvatarStyle applies to it.
[SemanticPart(
    "avatar",
    SelectorClass = "semantic-avatar",
    SelectorRoute = "/template/ .semantic-avatar",
    RuntimeCreated = true,
    ContractType = typeof(SkeletonAvatar),
    Since = "6.2.0")]
// The title placeholder at the top of the right column. Created from the placeholder template only while IsLoading
// is true; visible only while IsShowTitle is true. SkeletonTitleStyle applies to it.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-title",
    RuntimeCreated = true,
    ContractType = typeof(SkeletonTitle),
    Since = "6.2.0")]
// The paragraph placeholder below the title in the right column. Created from the placeholder template only while
// IsLoading is true; visible only while IsShowParagraph is true. SkeletonParagraphStyle applies to it.
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
