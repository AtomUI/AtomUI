namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the item that was clicked in a <see cref="Breadcrumb.NavigateRequest" /> event.
/// </summary>
public class BreadcrumbNavigateEventArgs : EventArgs
{
    /// <summary>
    /// The item that was clicked. Its <see cref="BreadcrumbItem.NavigateUri" /> and
    /// <see cref="BreadcrumbItem.NavigateContext" /> carry the navigation data.
    /// </summary>
    public BreadcrumbItem BreadcrumbItem { get; }

    /// <summary>
    /// Creates the arguments for a click on the given item.
    /// </summary>
    /// <param name="breadcrumbItem">
    /// The item that was clicked.
    /// </param>
    public BreadcrumbNavigateEventArgs(BreadcrumbItem breadcrumbItem)
    {
        BreadcrumbItem = breadcrumbItem;
    }
}