using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The tag of the desktop control set: text with an optional icon and an optional close button. It maps each
/// <c>TagStatus</c> to the matching status color of <see cref="TagToken" />, which lets a design language
/// give the tag its own status colors. All other behavior comes from <see cref="AbstractTag" />.
/// </summary>
public partial class Tag : AbstractTag
{
    public Tag()
    {
    }

    /// <summary>
    /// Returns the key of the token of <see cref="TagToken" /> that holds the main color of a status tag, which
    /// lets a design language give the tag its own status colors: <c>TagTokenKind.SuccessColor</c> for
    /// <c>TagStatus.Success</c>, <c>TagTokenKind.InfoColor</c> for <c>TagStatus.Info</c>,
    /// <c>TagTokenKind.WarningColor</c> for <c>TagStatus.Warning</c> and <c>TagTokenKind.ErrorColor</c> for
    /// <c>TagStatus.Error</c>. The result is never <see langword="null" />, so a status tag takes its main color
    /// from these tokens and never from the global status color.
    /// </summary>
    private protected override object StatusColorTokenKey(TagStatus status) => status switch
    {
        TagStatus.Success => TagTokenKind.SuccessColor,
        TagStatus.Info    => TagTokenKind.InfoColor,
        TagStatus.Warning => TagTokenKind.WarningColor,
        _                 => TagTokenKind.ErrorColor
    };
}