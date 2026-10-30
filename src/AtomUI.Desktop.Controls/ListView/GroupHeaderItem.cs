namespace AtomUI.Desktop.Controls;

/// <summary>
/// Dedicated container for ListView group header entries.
/// The group identity is a construction-time trait, so it never needs to be
/// switched while a container is recycled.
/// </summary>
internal sealed class GroupHeaderItem : ListViewItem
{
    public GroupHeaderItem()
    {
        IsGroupItem = true;
    }
}
