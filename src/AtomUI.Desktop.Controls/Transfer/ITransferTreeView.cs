using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface ITransferTreeView : ITransferView
{
    void SetMaskedItems(IList<EntityKey>? maskedItems);
}