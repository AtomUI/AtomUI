using System.Collections;
using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The view of one panel of a transfer: the list or tree that shows the items of the panel and keeps the keys
/// of the items checked in it.
/// </summary>
/// <remarks>
/// The view is the content of the item decorator of the panel, which drives it: the decorator pushes the
/// state of the panel in through the <c>Set</c> methods and the <c>Notify</c> methods, and reads changes
/// back through the events. <see cref="TransferListView" /> and <see cref="TransferTreeView" /> are the
/// built-in views.
/// </remarks>
public interface ITransferView
{
    /// <summary>
    /// The keys of the items checked in this panel, or <see langword="null" /> when no item is checked. The
    /// value and the checked items follow each other: a new value replaces the checked items with the items
    /// whose keys it contains, and a change made in the view replaces the value with a new list of the keys
    /// of the checked items that can be checked. While the view is attached, the change notifications of a
    /// collection that implements
    /// <see cref="System.Collections.Specialized.INotifyCollectionChanged" /> are applied to the items as well.
    /// The owning transfer sets the value and reads it when <see cref="SelectedKeyChanged" /> is raised.
    /// </summary>
    IList<EntityKey>? SelectedKeys { get; set; }
    /// <summary>
    /// The number of items the view addresses through page-relative indices. The list view counts the rows
    /// of its current page, which are all rows while paging is off, so the count reported through
    /// <see cref="ItemCountChanged" /> covers every page and can be larger.
    /// </summary>
    int ItemCount { get; }
    /// <summary>
    /// Whether the view renders an item template. While it is <see langword="true" />, the item decorator of
    /// the panel hands its own template to the view through <see cref="SetItemTemplate" />.
    /// </summary>
    bool IsSupportItemTemplate { get; }
    /// <summary>
    /// Whether the view pages its items. While it is <see langword="true" />, the item decorator of the panel
    /// forwards its pagination setting and page size through <see cref="SetPaginationEnabled" /> and
    /// <see cref="SetPageSize" />; otherwise it calls neither.
    /// </summary>
    bool IsSupportPagination { get; }
    /// <summary>
    /// The panel the view belongs to. The item decorator of the panel sets it when the view is attached.
    /// The views add the item class of that panel to their items, and <see cref="NotifyTransferCompleted" />
    /// tells the receiving panel from the sending one by it. The default is
    /// <see cref="TransferViewType.Source" />.
    /// </summary>
    TransferViewType ViewType { get; set; }
  
    /// <summary>
    /// Raised when the user asks to take items out of the target panel, before the owning transfer removes
    /// the reported keys from its target keys. The view removes nothing itself. The list view raises it for
    /// a click on the remove button of a row and for <see cref="TransferSelectAction.RemoveCurrentPage" />
    /// with the items of the current page; the tree view never raises it.
    /// </summary>
    event EventHandler<TransferItemsRemovedEventArgs>? ItemsRemoved;
    /// <summary>
    /// Raised after the number of items in the view changed, with the new count. The list view counts its
    /// rows over every page; the tree view counts the nodes at every depth that are not masked.
    /// </summary>
    event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    /// <summary>
    /// Raised after <see cref="SelectedKeys" /> changed or was rebuilt from the checked items, with the
    /// number of keys it holds.
    /// </summary>
    event EventHandler<SelectionCountChangedEventArgs>? SelectionCountChanged;
    /// <summary>
    /// Raised after <see cref="SelectedKeys" /> changed and before the checked items are made to follow it,
    /// also when the collection assigned to the value raises <c>CollectionChanged</c>. The owning transfer
    /// reads the value here.
    /// </summary>
    event EventHandler? SelectedKeyChanged;
    
    /// <summary>
    /// Checks every item of the view that can be checked, which makes <see cref="SelectedKeys" /> hold the
    /// keys of the checked items. The item decorator of the panel calls it when its <c>IsAllSelected</c>
    /// property changes to <see langword="true" />.
    /// </summary>
    void SelectAll();
    /// <summary>
    /// Unchecks every item of the view, which empties <see cref="SelectedKeys" />. The item decorator of the
    /// panel calls it when its <c>IsAllSelected</c> property changes to <see langword="false" />.
    /// </summary>
    void DeselectAll();
    /// <summary>
    /// Tells the view that the owning transfer is about to move checked items between the panels. Both
    /// views receive it before a transfer of the items checked in a panel; only the view of the source
    /// panel receives it before items are taken out of the target panel through <see cref="ItemsRemoved" />.
    /// <see cref="NotifyTransferCompleted" /> follows once the owning transfer has changed its target keys.
    /// The list view saves its selected keys and its page here.
    /// </summary>
    /// <param name="transferDirection">
    /// The direction of the move: <see cref="TransferDirection.ToTarget" /> moves the items checked in the
    /// source panel to the target panel, <see cref="TransferDirection.ToSource" /> moves the items checked
    /// in the target panel back to the source panel.
    /// </param>
    void NotifyAboutToTransfer(TransferDirection transferDirection);
    /// <summary>
    /// Tells the view that the move announced by <see cref="NotifyAboutToTransfer" /> is over: the owning
    /// transfer has changed its target keys. It is called on the views that received the announcement. The
    /// list view keeps the selection of the receiving panel, clears the selection of the sending panel and,
    /// when paging is on, goes back to the page it was on.
    /// </summary>
    /// <param name="transferDirection">
    /// The direction of the finished move, the value given to <see cref="NotifyAboutToTransfer" />.
    /// </param>
    void NotifyTransferCompleted(TransferDirection transferDirection);
    /// <summary>
    /// Applies an action chosen in the select menu of the panel to the items of the view. The item decorator
    /// of the panel forwards every action of the menu. It handles
    /// <see cref="TransferSelectAction.SelectAll" /> and <see cref="TransferSelectAction.DeselectAll" />
    /// itself through <see cref="SelectAll" /> and <see cref="DeselectAll" />, so what is left to a view is
    /// the actions on the current page: the list view selects or toggles the rows of the current page, or
    /// reports them through <see cref="ItemsRemoved" />. A view ignores an action it does not handle.
    /// </summary>
    void NotifySelectAction(TransferSelectAction selectAction);
    /// <summary>
    /// Tells the view whether the owning transfer moves items from the source panel to the target panel
    /// only. The item decorator of the panel calls it when the view is attached and when the setting
    /// changes. The default implementation does nothing.
    /// </summary>
    public void NotifyIsOneWay(bool isOneWay) {}
    /// <summary>
    /// Enables or disables checking the items of the view. The item decorator of the panel calls it when the
    /// view is attached and when <see cref="ViewType" /> or the one-way setting of the transfer changes:
    /// the source panel is always enabled, the target panel only when the transfer is not one-way.
    /// </summary>
    void SetSelectionEnabled(bool enabled);
    /// <summary>
    /// Gives the view the icon built from the <c>SelectionsIconTemplate</c> of the item decorator of the
    /// panel, or <see langword="null" /> when the decorator has no such template. The decorator calls it
    /// when the view is attached and when the template changes. The default implementation does nothing.
    /// </summary>
    public void SetSelectionsIcon(PathIcon? icon) {}
    /// <summary>
    /// Replaces the items of the view with the items of the panel. The item decorator of the panel calls it
    /// when the view is attached, whenever its items source changes and again when the pagination setting
    /// or the page size changes; it passes <see langword="null" /> when the view is detached. The list view
    /// drops the selected keys that a new source does not contain.
    /// </summary>
    void SetItemsSource(IEnumerable? itemsSource);
    /// <summary>
    /// Sets the template that renders an item of the view. The item decorator of the panel calls it when the
    /// view is attached and when its own item template changes, only while
    /// <see cref="IsSupportItemTemplate" /> is <see langword="true" />; it passes <see langword="null" />
    /// when it has no template. The default implementation does nothing.
    /// </summary>
    public void SetItemTemplate(IDataTemplate? itemTemplate) {}
    /// <summary>
    /// Enables or disables paging of the items of the view. The item decorator of the panel calls it when
    /// the view is attached and when the pagination setting or the page size of the transfer changes, only
    /// while <see cref="IsSupportPagination" /> is <see langword="true" />. The default implementation does
    /// nothing.
    /// </summary>
    public void SetPaginationEnabled(bool enabled) {}
    /// <summary>
    /// Sets the number of items on a page. The item decorator of the panel calls it right after
    /// <see cref="SetPaginationEnabled" />, only while <see cref="IsSupportPagination" /> is
    /// <see langword="true" />; a view that does not page its items ignores it.
    /// </summary>
    void SetPageSize(int pageSize);
}

internal interface ITransferDecoratorProvider
{
    void ProvideTransferDecorator(TransferItemDecorator decorator);
}