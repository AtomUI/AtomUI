using System.ComponentModel;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

/// <summary>
/// A group of a grouped list collection view: the records that were put under one key and, when the group is
/// divided further, its subgroups. <see cref="IListCollectionView.Groups" /> returns the top-level groups. The root
/// that holds them and the concrete groups are internal to the library.
/// </summary>
public abstract class ListCollectionViewGroup : INotifyPropertyChanged
{
    private int _itemCount;
    /// <summary>
    /// The key the group was created with: a key declared by the group description, or the key that the group
    /// description returned for its records.
    /// </summary>
    public object Key { get; }
    /// <summary>
    /// The number of records in the group, counting the records of its subgroups. Subgroups are not counted.
    /// </summary>
    public int ItemCount => _itemCount;
    /// <summary>
    /// The direct children of the group: its records and its subgroups, which are again
    /// <see cref="ListCollectionViewGroup" /> instances.
    /// </summary>
    public IAvaloniaReadOnlyList<object> Items => ProtectedItems;
    /// <summary>
    /// The list behind <see cref="Items" />. A derived class inserts and removes the direct children of the group here
    /// and keeps <see cref="ProtectedItemCount" /> in step itself; changing the list does not change the count.
    /// </summary>
    protected AvaloniaList<object> ProtectedItems { get; }
    
    /// <summary>
    /// The record count behind <see cref="ItemCount" />, for derived classes to update when records are added or
    /// removed. Assigning it stores the value and raises <see cref="PropertyChanged" /> for <see cref="ItemCount" />,
    /// also when the value did not change.
    /// </summary>
    protected int ProtectedItemCount
    {
        get => _itemCount;
        set
        {
            _itemCount = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemCount)));
        }
    }
    
    internal abstract ListCollectionViewGroupInternal? Parent { get; }
    internal abstract IListGroupDescription? GroupBy { get; set; }

    /// <summary>
    /// Creates an empty group for the given key.
    /// </summary>
    /// <param name="key">
    /// The key that identifies the group among its siblings.
    /// </param>
    protected ListCollectionViewGroup(object key)
    {
        Key            = key;
        ProtectedItems = new AvaloniaList<object>();
    }

    /// <summary>
    /// Gets whether the group is not divided into subgroups, so that every entry of <see cref="Items" /> is a record.
    /// </summary>
    public abstract bool IsBottomLevel { get; }

    /// <summary>
    /// Occurs after a property of the group changed. Assigning <see cref="ProtectedItemCount" /> raises it for
    /// <see cref="ItemCount" />; a derived class raises it for its own properties by calling
    /// <see cref="OnPropertyChanged" />. The explicit
    /// <see cref="INotifyPropertyChanged.PropertyChanged" /> implementation exposes it to subscribers.
    /// </summary>
    protected virtual event PropertyChangedEventHandler? PropertyChanged;

    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add =>  PropertyChanged += value;
        remove => PropertyChanged -= value;
    }

    /// <summary>
    /// Raises <see cref="PropertyChanged" /> with the given arguments. A derived class calls it after one of its own
    /// properties changed; an override that does not raise the event itself must call the base member.
    /// </summary>
    /// <param name="e">
    /// The arguments, which name the changed property.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
}