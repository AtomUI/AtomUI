using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A transfer view that shows its items as a tree and can mask items by their key.
/// </summary>
public interface ITransferTreeView : ITransferView
{
    /// <summary>
    /// Masks the items that have one of the given keys. The mask replaces the previous one. A masked item is drawn
    /// as disabled, and the view leaves it out when it checks or unchecks nodes recursively.
    /// </summary>
    /// <param name="maskedItems">
    /// The keys of the items to mask, or <see langword="null" /> to mask none.
    /// </param>
    void SetMaskedItems(IList<EntityKey>? maskedItems);
}