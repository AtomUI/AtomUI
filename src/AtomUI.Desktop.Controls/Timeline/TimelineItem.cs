using AtomUI.Controls.Commons;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One node of a <see cref="Timeline" />, showing an indicator on the line with an optional label and a content body.
/// </summary>
// Token-owner transparent item container: a TimelineItem belongs to one Timeline and has no Own Tokens.
// Its theme and its indicator read the Timeline TokenResource, so Controls[Timeline] reaches the items.
[TokenOwnerTransparent]
public class TimelineItem : AbstractTimelineItem
{
    public TimelineItem()
    {
    }
}