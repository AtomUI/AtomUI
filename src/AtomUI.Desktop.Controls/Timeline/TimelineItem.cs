using AtomUI.Controls.Commons;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// Token-owner transparent item container: a TimelineItem belongs to one Timeline and has no Own Tokens; its theme and its
// indicator read the Timeline TokenResource, so Controls[Timeline] reaches the items.
[TokenOwnerTransparent]
public class TimelineItem : AbstractTimelineItem
{
    public TimelineItem()
    {
    }
}