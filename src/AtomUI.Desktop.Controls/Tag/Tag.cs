using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

public partial class Tag : AbstractTag
{
    public Tag()
    {
    }

    private protected override object StatusColorTokenKey(TagStatus status) => status switch
    {
        TagStatus.Success => TagTokenKind.SuccessColor,
        TagStatus.Info    => TagTokenKind.InfoColor,
        TagStatus.Warning => TagTokenKind.WarningColor,
        _                 => TagTokenKind.ErrorColor
    };
}