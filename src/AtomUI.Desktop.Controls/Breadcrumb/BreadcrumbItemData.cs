using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A mutable set of values for one breadcrumb item in a data source. A <see cref="Breadcrumb" /> transfers
/// the values to the <see cref="BreadcrumbItem" /> it generates for the item; the effect of each value is
/// stated on <see cref="IBreadcrumbItemData" />.
/// </summary>
public class BreadcrumbItemData : IBreadcrumbItemData
{
    /// <inheritdoc />
    public PathIcon? Icon { get; set; }
    /// <inheritdoc />
    public object? Content { get; set; }
    /// <inheritdoc />
    public Uri? NavigateUri { get; set; }
    /// <inheritdoc />
    public object? Separator { get; set; }
    /// <inheritdoc />
    public IDataTemplate? SeparatorTemplate { get; set; }
    /// <inheritdoc />
    public object? NavigateContext { get; set; }
}