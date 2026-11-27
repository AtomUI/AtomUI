using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

/// <summary>
/// Base class of a group description for list collection views. It holds the explicitly declared group keys and
/// raises a change notification when they change; a derived class decides which group an item belongs to.
/// </summary>
public abstract class AbstractListGroupDescription : INotifyPropertyChanged, IListGroupDescription
{
    /// <summary>
    /// Gets the keys of the subgroups that exist before any item is grouped. The view creates one subgroup per key,
    /// in this order, ahead of the subgroups it derives from items, and keeps these subgroups when they are empty.
    /// The list is empty by default.
    /// </summary>
    /// <remarks>
    /// Changing the list raises <see cref="PropertyChanged" /> for this property.
    /// </remarks>
    public AvaloniaList<object> GroupKeys { get; }

    public AbstractListGroupDescription()
    {
        GroupKeys = new AvaloniaList<object>();
        GroupKeys.CollectionChanged += OnGroupKeysCollectionChanged;
    }

    /// <summary>
    /// Occurs when <see cref="GroupKeys" /> changes. It is the event behind the explicit
    /// <see cref="INotifyPropertyChanged.PropertyChanged" /> implementation of this class.
    /// </summary>
    protected virtual event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add => PropertyChanged += value;
        remove => PropertyChanged -= value;
    }
    
    private void OnGroupKeysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(GroupKeys)));
    }
    
    /// <summary>
    /// Raises <see cref="PropertyChanged" /> with the given arguments. It is called when <see cref="GroupKeys" />
    /// changes. A derived class calls it to report a change of its own; an override that does not raise the event
    /// itself must call the base member.
    /// </summary>
    /// <param name="e">
    /// The arguments, which name the changed property.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
    
    /// <summary>
    /// Returns the key of the subgroup that the item belongs to at the given grouping level.
    /// </summary>
    /// <param name="item">
    /// The item to group.
    /// </param>
    /// <param name="level">
    /// The zero-based nesting level of the group that is divided; 0 for the top-level groups.
    /// </param>
    /// <param name="culture">
    /// The culture of the view, <see cref="IListCollectionView.Culture" />.
    /// </param>
    /// <returns>
    /// The group key. An <c>ICollection</c> puts the item in one subgroup per element; <see langword="null" />
    /// puts it in none.
    /// </returns>
    public abstract object? GroupKeyFromItem(object item, int level, CultureInfo culture);
    /// <summary>
    /// Returns whether an item belongs to the subgroup that has the given group key. The view calls it for each
    /// existing subgroup with the key computed for the item, and adds the item to the first subgroup that matches.
    /// </summary>
    /// <param name="groupKey">
    /// The key the subgroup was created with.
    /// </param>
    /// <param name="itemKey">
    /// The key returned by <see cref="GroupKeyFromItem" /> for the item.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item belongs to the subgroup; otherwise <see langword="false" />.
    /// </returns>
    /// <remarks>
    /// The default compares the keys by value, with the static <c>Equals</c> method of <see cref="object" />. An
    /// override can call the base member for key types it does not handle.
    /// </remarks>
    public virtual bool KeysMatch(object groupKey, object itemKey)
    {
        return Equals(groupKey, itemKey);
    }
}