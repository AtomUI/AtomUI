namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-class names that <see cref="ListView" /> sets from <see cref="ListView.TotalItemCount" />.
/// </summary>
public static class ListPseudoClass
{
    /// <summary>
    /// Set while the list holds no item.
    /// </summary>
    public const string Empty = ":empty";
    /// <summary>
    /// Set while the list holds exactly one item.
    /// </summary>
    public const string SingleItem = ":singleitem";
}