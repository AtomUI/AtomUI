using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Collections;
using Avalonia.Utilities;

namespace AtomUI.Collections;

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

    public object? this[int index]
    {
        get => _source[index];
        set
        {
            EnsureWritable();
            _source[index] = value;
        }
    }

    public int Count => _source.Count;
    public bool IsReadOnly => _mode == Mode.ItemsSource;
    public bool IsFixedSize => false;
    public bool IsSynchronized => false;
    public object SyncRoot => this;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public int Add(object? value)
    {
        EnsureWritable();
        return _source.Add(value);
    }

    public void Clear()
    {
        EnsureWritable();
        _source.Clear();
    }

    public bool Contains(object? value) => _source.Contains(value);

    public int IndexOf(object? value) => _source.IndexOf(value);

    public void Insert(int index, object? value)
    {
        EnsureWritable();
        _source.Insert(index, value);
    }

    public void Remove(object? value)
    {
        EnsureWritable();
        _source.Remove(value);
    }

    public void RemoveAt(int index)
    {
        EnsureWritable();
        _source.RemoveAt(index);
    }

    public void CopyTo(Array array, int index) => _source.CopyTo(array, index);

    public IEnumerator GetEnumerator() => _source.GetEnumerator();

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
