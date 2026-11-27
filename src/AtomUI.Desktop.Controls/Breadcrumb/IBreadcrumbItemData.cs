using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Supplies the content, icon, separator and navigation data of a breadcrumb item. A
/// <see cref="Breadcrumb" /> transfers the values to the <see cref="BreadcrumbItem" /> it generates for an
/// item that implements this interface.
/// </summary>
public interface IBreadcrumbItemData
{
    /// <summary>
    /// Icon of the item, or <see langword="null" /> to show none. It is copied to
    /// <see cref="BreadcrumbItem.Icon" /> of the generated item when that has no icon set.
    /// </summary>
    PathIcon? Icon { get; }
    /// <summary>
    /// Content of the item. It becomes the content of the generated item while the breadcrumb has no
    /// <c>ItemTemplate</c>; with one, the template builds the item instead.
    /// </summary>
    object? Content { get; }
    /// <summary>
    /// URI that the system launcher opens when the item is clicked, or <see langword="null" /> for none. A value
    /// other than <see langword="null" /> is copied to <see cref="BreadcrumbItem.NavigateUri" /> of the
    /// generated item when that has no value set, and makes a click on the item raise
    /// <see cref="Breadcrumb.NavigateRequest" />.
    /// </summary>
    Uri? NavigateUri { get; }
    /// <summary>
    /// Content of the separator between this item and the next one, or <see langword="null" /> to use the
    /// <see cref="Breadcrumb.Separator" /> of the breadcrumb.
    /// </summary>
    object? Separator { get; }
    /// <summary>
    /// Template that builds the separator between this item and the next one, or <see langword="null" /> to
    /// use the <see cref="Breadcrumb.SeparatorTemplate" /> of the breadcrumb.
    /// </summary>
    IDataTemplate? SeparatorTemplate { get; }
    /// <summary>
    /// Application-defined navigation target of the item. A value other than <see langword="null" /> is copied
    /// to <see cref="BreadcrumbItem.NavigateContext" /> of the generated item when that has no value set, and
    /// makes a click on the item raise <see cref="Breadcrumb.NavigateRequest" />.
    /// </summary>
    object? NavigateContext { get; }
}