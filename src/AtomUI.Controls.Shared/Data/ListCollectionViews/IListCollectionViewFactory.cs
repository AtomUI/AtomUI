namespace AtomUI.Controls.Data;

/// <summary>
/// Supplies <see cref="IListCollectionView" /> instances.
/// </summary>
public interface IListCollectionViewFactory
{
    /// <summary>
    /// Creates a list collection view.
    /// </summary>
    /// <returns>
    /// The created view.
    /// </returns>
    IListCollectionView CreateView();
}