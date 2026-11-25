using AtomUI.Controls.Commons;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// Token-owner transparent item container: a SegmentedItem belongs to one Segmented and has no Own Tokens; its theme
// reads the Segmented TokenResource, so Controls[Segmented] reaches the items and their transitions.
[TokenOwnerTransparent]
public class SegmentedItem : AbstractSegmentedItem
{
    public SegmentedItem()
    {
    }
}