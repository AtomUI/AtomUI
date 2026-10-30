using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.ExceptionServices;

namespace AtomUI.Desktop.Controls;

internal sealed class NavMenuEntryCollection : IList<INavMenuEntry>,
                                                IList,
                                                INotifyCollectionChanged,
                                                INotifyPropertyChanged
{
    private readonly List<INavMenuEntry> _entries = [];
    private readonly object _owner;
    private readonly Action<INavMenuEntry> _validate;
    private readonly Action<INavMenuEntry> _attach;
    private readonly Action<INavMenuEntry> _detach;
    private readonly NavMenuEntryOwnershipCoordinator _ownershipCoordinator;
    private bool _isReplacing;

    public NavMenuEntryCollection(object owner,
                                  Action<INavMenuEntry> validate,
                                  Action<INavMenuEntry> attach,
                                  Action<INavMenuEntry> detach)
    {
        _owner    = owner;
        _validate = validate;
        _attach   = attach;
        _detach   = detach;
        _ownershipCoordinator = new NavMenuEntryOwnershipCoordinator(owner, () => _entries);
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Count => _entries.Count;
    public bool IsReadOnly => false;

    bool IList.IsFixedSize => false;
    bool IList.IsReadOnly => false;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => ((ICollection)_entries).SyncRoot;

    public INavMenuEntry this[int index]
    {
        get => _entries[index];
        set
        {
            EnsureCanMutate();
            var oldEntry = _entries[index];
            if (ReferenceEquals(oldEntry, value))
            {
                return;
            }

            // Parent callbacks and collection observers must see one committed replacement.
            // Reentrant writes to this collection would publish changes against an unfinished
            // Replace; writes to other owners still use the normal structural ownership checks.
            _isReplacing = true;
            try
            {
                Validate(value, index);
                var prospectiveEntries = _entries.ToList();
                prospectiveEntries[index] = value;
                _ownershipCoordinator.Validate(prospectiveEntries);
                _entries[index] = value;
                _ownershipCoordinator.Synchronize();

                List<Exception>? callbackExceptions = null;
                CompleteStep(() => _detach(oldEntry));
                CompleteStep(() => _attach(value));
                CompleteStep(RaiseIndexerChanged);
                CompleteStep(() => CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        value,
                        oldEntry,
                        index)));

                if (callbackExceptions is { Count: 1 })
                {
                    ExceptionDispatchInfo.Capture(callbackExceptions[0]).Throw();
                }
                else if (callbackExceptions is not null)
                {
                    throw new AggregateException(callbackExceptions);
                }

                void CompleteStep(Action callback)
                {
                    try
                    {
                        callback();
                    }
                    catch (Exception exception)
                    {
                        // The collection is committed. Finish parent projection and notification
                        // before propagating every callback failure to the caller.
                        (callbackExceptions ??= []).Add(exception);
                    }
                }
            }
            finally
            {
                _isReplacing = false;
            }
        }
    }

    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = NavMenuEntryValidation.Validate(value, _owner, index);
    }

    public void Add(INavMenuEntry item)
    {
        Insert(_entries.Count, item);
    }

    int IList.Add(object? value)
    {
        Add(NavMenuEntryValidation.Validate(value, _owner, Count));
        return Count - 1;
    }

    public void AddRange(IEnumerable<INavMenuEntry> entries)
    {
        EnsureCanMutate();
        ArgumentNullException.ThrowIfNull(entries);

        var additions = entries.ToArray();
        for (var index = 0; index < additions.Length; index++)
        {
            Validate(additions[index], _entries.Count + index);
        }

        var prospectiveEntries = _entries.Concat(additions).ToArray();
        _ownershipCoordinator.Validate(prospectiveEntries);

        if (additions.Length == 0)
        {
            return;
        }

        var insertionIndex = _entries.Count;
        _entries.AddRange(additions);
        _ownershipCoordinator.Synchronize();
        foreach (var entry in additions)
        {
            _attach(entry);
        }

        RaiseCountAndIndexerChanged();
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                additions,
                insertionIndex));
    }

    public void Clear()
    {
        EnsureCanMutate();
        if (_entries.Count == 0)
        {
            return;
        }

        var oldEntries = _entries.ToArray();
        _entries.Clear();
        foreach (var entry in oldEntries)
        {
            _detach(entry);
        }
        _ownershipCoordinator.Synchronize();

        RaiseCountAndIndexerChanged();
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(INavMenuEntry item)
    {
        return _entries.Contains(item);
    }

    bool IList.Contains(object? value)
    {
        return value is INavMenuEntry entry && Contains(entry);
    }

    public void CopyTo(INavMenuEntry[] array, int arrayIndex)
    {
        _entries.CopyTo(array, arrayIndex);
    }

    void ICollection.CopyTo(Array array, int index)
    {
        ((ICollection)_entries).CopyTo(array, index);
    }

    public IEnumerator<INavMenuEntry> GetEnumerator()
    {
        return _entries.GetEnumerator();
    }

    public int IndexOf(INavMenuEntry item)
    {
        return _entries.IndexOf(item);
    }

    int IList.IndexOf(object? value)
    {
        return value is INavMenuEntry entry ? IndexOf(entry) : -1;
    }

    public void Insert(int index, INavMenuEntry item)
    {
        EnsureCanMutate();
        Validate(item, index);
        var prospectiveEntries = _entries.ToList();
        prospectiveEntries.Insert(index, item);
        _ownershipCoordinator.Validate(prospectiveEntries);
        _entries.Insert(index, item);
        _ownershipCoordinator.Synchronize();
        _attach(item);
        RaiseCountAndIndexerChanged();
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                item,
                index));
    }

    void IList.Insert(int index, object? value)
    {
        Insert(index, NavMenuEntryValidation.Validate(value, _owner, index));
    }

    public bool Remove(INavMenuEntry item)
    {
        EnsureCanMutate();
        var index = _entries.IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    void IList.Remove(object? value)
    {
        if (value is INavMenuEntry entry)
        {
            Remove(entry);
        }
    }

    public void RemoveAt(int index)
    {
        EnsureCanMutate();
        var oldEntry = _entries[index];
        _entries.RemoveAt(index);
        _detach(oldEntry);
        _ownershipCoordinator.Synchronize();
        RaiseCountAndIndexerChanged();
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove,
                oldEntry,
                index));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void EnsureCanMutate()
    {
        if (_isReplacing)
        {
            throw new InvalidOperationException(
                "Cannot modify a NavMenu entry collection while its replacement is being applied or notified.");
        }
    }

    private void Validate(INavMenuEntry? entry, int index)
    {
        var validatedEntry = NavMenuEntryValidation.Validate(entry, _owner, index);
        _validate(validatedEntry);
        NavMenuEntryOwnership.EnsureCanAttach(validatedEntry, _owner);
    }

    private void RaiseCountAndIndexerChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        RaiseIndexerChanged();
    }

    private void RaiseIndexerChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
}
