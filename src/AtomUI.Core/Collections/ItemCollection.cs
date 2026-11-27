using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Collections;
using Avalonia.Utilities;

namespace AtomUI.Collections;

/// <summary>
/// The items of a control, either stored by the collection itself or supplied by an items source.
/// </summary>
/// <remarks>
/// <see cref="SetItemsSource" /> installs a source and makes the collection a read-only view of it; calling it with
/// <see langword="null" /> returns to items that the collection stores itself.
/// </remarks>
public class ItemCollection : IList, INotifyCollectionChanged
{
    private Mode _mode;
    private IList _source;
    private readonly TargetWeakEventSubscriber<ItemCollection, NotifyCollectionChangedEventArgs> _sourceSubscriber;

    public ItemCollection()
    {
        _sourceSubscriber = new(this, static (collection, sender, _, args) => collection.OnSourceCollectionChanged(sender, args));
        _source = CreateDefaultCollection();
        SubscribeToChanges();
    }

    /// <summary>
    /// Gets or sets the item at a zero-based index. Setting is rejected while an items source is in use.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the item.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The setter was called while an items source is in use.
    /// </exception>
    public object? this[int index]
    {
        get => _source[index];
        set
        {
            EnsureWritable();
            _source[index] = value;
        }
    }

    /// <summary>
    /// The number of items in the collection or in its items source.
    /// </summary>
    public int Count => _source.Count;
    /// <summary>
    /// Whether an items source supplies the items. The collection then rejects every change made through it.
    /// </summary>
    public bool IsReadOnly => _mode == Mode.ItemsSource;
    /// <summary>
    /// Always <see langword="false" />. The number of items can change in both modes.
    /// </summary>
    public bool IsFixedSize => false;
    /// <summary>
    /// Always <see langword="false" />. The collection does not synchronize access.
    /// </summary>
    public bool IsSynchronized => false;
    /// <summary>
    /// The collection itself, for callers that synchronize access with a lock.
    /// </summary>
    public object SyncRoot => this;

    /// <summary>
    /// Raised after the items changed, whether through this collection or through the items source.
    /// </summary>
    /// <remarks>
    /// A source reports its own changes only when it implements <see cref="INotifyCollectionChanged" />. Replacing the
    /// source with <see cref="SetItemsSource" /> raises the event with
    /// <see cref="NotifyCollectionChangedAction.Remove" /> for the items of the previous source and then with
    /// <see cref="NotifyCollectionChangedAction.Add" /> for the items of the new source, each only when that source
    /// holds items.
    /// </remarks>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Appends an item to the end of the collection.
    /// </summary>
    /// <param name="value">
    /// The item to add.
    /// </param>
    /// <returns>
    /// The zero-based index of the added item.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// An items source is in use.
    /// </exception>
    public int Add(object? value)
    {
        EnsureWritable();
        return _source.Add(value);
    }

    /// <summary>
    /// Removes every item from the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// An items source is in use.
    /// </exception>
    public void Clear()
    {
        EnsureWritable();
        _source.Clear();
    }

    /// <summary>
    /// Checks whether an item is in the collection.
    /// </summary>
    /// <param name="value">
    /// The item to find.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the collection holds an item that equals <paramref name="value" />.
    /// </returns>
    public bool Contains(object? value) => _source.Contains(value);

    /// <summary>
    /// Finds the first item that equals the given value.
    /// </summary>
    /// <param name="value">
    /// The item to find.
    /// </param>
    /// <returns>
    /// The zero-based index of the item, or -1 when the collection does not contain it.
    /// </returns>
    public int IndexOf(object? value) => _source.IndexOf(value);

    /// <summary>
    /// Inserts an item at a zero-based index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index to insert at. It may equal <see cref="Count" /> to append the item.
    /// </param>
    /// <param name="value">
    /// The item to insert.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// An items source is in use.
    /// </exception>
    public void Insert(int index, object? value)
    {
        EnsureWritable();
        _source.Insert(index, value);
    }

    /// <summary>
    /// Removes the first item that equals the given value. Nothing happens when the collection does not contain it.
    /// </summary>
    /// <param name="value">
    /// The item to remove.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// An items source is in use.
    /// </exception>
    public void Remove(object? value)
    {
        EnsureWritable();
        _source.Remove(value);
    }

    /// <summary>
    /// Removes the item at a zero-based index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the item to remove.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// An items source is in use.
    /// </exception>
    public void RemoveAt(int index)
    {
        EnsureWritable();
        _source.RemoveAt(index);
    }

    /// <summary>
    /// Copies every item to a one-dimensional array, starting at a given index.
    /// </summary>
    /// <param name="array">
    /// The array to copy to. It must have room for <see cref="Count" /> items from <paramref name="index" /> on.
    /// </param>
    /// <param name="index">
    /// The zero-based index in the array at which copying starts.
    /// </param>
    public void CopyTo(Array array, int index) => _source.CopyTo(array, index);

    /// <summary>
    /// Returns an enumerator that walks the items in index order.
    /// </summary>
    /// <returns>
    /// An enumerator over the items of the collection or of its items source.
    /// </returns>
    public IEnumerator GetEnumerator() => _source.GetEnumerator();

    /// <summary>
    /// Switches the collection between items that it stores itself and the items of a source.
    /// </summary>
    /// <param name="value">
    /// The items to show, or <see langword="null" /> to return to an empty collection that stores its own items.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The collection holds items that were added directly. Switching from one source to another is allowed.
    /// </exception>
    /// <remarks>
    /// A source that implements <see cref="IList" /> is used directly, so changes made to it appear here and reach
    /// <see cref="CollectionChanged" /> when it also implements <see cref="INotifyCollectionChanged" />. Any other
    /// enumerable is copied once and does not report later changes. While a source is set the collection is read-only.
    /// </remarks>
    public void SetItemsSource(IEnumerable? value)
    {
        if (_mode != Mode.ItemsSource && Count > 0)
        {
            throw new InvalidOperationException(
                "Items collection must be empty before using ItemsSource.");
        }

        _mode = value is not null ? Mode.ItemsSource : Mode.Items;
        var oldSource = _source;
        UnsubscribeFromChanges(oldSource);

        _source = value as IList ?? (value != null ? new AvaloniaList<object?>(value.Cast<object?>()) : CreateDefaultCollection());
        SubscribeToChanges();

        if (oldSource.Count > 0)
        {
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldSource, 0));
        }

        if (_source.Count > 0)
        {
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _source, 0));
        }
    }

    private void EnsureWritable()
    {
        if (IsReadOnly)
        {
            ThrowIsItemsSource();
        }
    }

    private void SubscribeToChanges()
    {
        if (_source is INotifyCollectionChanged incc)
        {
            WeakEvents.CollectionChanged.Subscribe(incc, _sourceSubscriber);
        }
    }

    private void UnsubscribeFromChanges(IList source)
    {
        if (source is INotifyCollectionChanged incc)
        {
            WeakEvents.CollectionChanged.Unsubscribe(incc, _sourceSubscriber);
        }
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    private static AvaloniaList<object?> CreateDefaultCollection()
    {
        return new() { ResetBehavior = ResetBehavior.Remove };
    }

    [DoesNotReturn]
    private static void ThrowIsItemsSource()
    {
        throw new InvalidOperationException(
            "Operation is not valid while ItemsSource is in use. " +
            "Access and modify elements with ItemsControl.ItemsSource instead.");
    }

    private enum Mode
    {
        Items,
        ItemsSource,
    }
}
