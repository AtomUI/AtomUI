using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using AtomUI.Collections;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Selection;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

internal class TransferListViewSelectionModel : SelectionModel<object?>, ISelectionModel
{
    private IList? _writableSelectedItems;
    private int _ignoreModelChanges;
    private bool _ignoreSelectedItemsChanges;
    private bool _skipSyncFromSelectedItems;
    private bool _isResetting;
    private ListCollectionView? _collectionView;

    IEnumerable? ISelectionModel.Source 
    {
        get => Source;
        set => SetListSource(value);
    }
    
    public new IEnumerable? Source
    {
        get => base.Source;
        set => SetListSource(value);
    }
    
    public TransferListViewSelectionModel()
    {
        SelectionChanged += OnSelectionChanged;
        SourceReset      += OnSourceReset;
    }

    [AllowNull]
    public IList WritableSelectedItems
    {
        get
        {
            if (_writableSelectedItems is null)
            {
                _writableSelectedItems = new AvaloniaList<object?>();
                SubscribeToSelectedItems();
            }

            return _writableSelectedItems;
        }
        set
        {
            value ??= new AvaloniaList<object?>();

            if (value.IsFixedSize)
            {
                throw new NotSupportedException("Cannot assign fixed size selection to SelectedItems.");
            }

            if (_writableSelectedItems != value)
            {
                UnsubscribeFromSelectedItems();
                _writableSelectedItems = value;
                SyncFromSelectedItems();
                SubscribeToSelectedItems();
                    
                if (ItemsView is null)
                {
                    this.SetInitSelectedItems(value);
                }

                RaisePropertyChanged(nameof(WritableSelectedItems));
            }
        }
    }

    internal void Update(IEnumerable? source, Optional<IList?> selectedItems)
    {
        var previousSource                = Source;
        var previousWritableSelectedItems = _writableSelectedItems;

        base.OnSourceCollectionChangeStarted();
            
        try
        {
            _skipSyncFromSelectedItems = true;
            SetListSource(source);
            if (selectedItems.HasValue)
            {
                WritableSelectedItems = selectedItems.Value;
            }
        }
        finally 
        { 
            _skipSyncFromSelectedItems = false;
        }

        // We skipped the sync from WritableSelectedItems before; do it now that both
        // the source and WritableSelectedItems are updated.
        if (previousWritableSelectedItems != _writableSelectedItems)
        {
            base.OnSourceCollectionChangeFinished();
            SyncFromSelectedItems();
        }
        else if (previousSource != Source)
        {
            SyncFromSelectedItems();
            base.OnSourceCollectionChangeFinished();
        }
        else
        {
            base.OnSourceCollectionChangeFinished();
        }
    }

    protected void SetListSource(IEnumerable? value)
    {
        var collectionView = value as ListCollectionView;
        if (collectionView != null)
        {
            if (_collectionView == collectionView)
            {
                return;
            }

            
        }
        else
        {
            if (Source == value)
            {
                return;
            }
        }

        object?[]? oldSelection = null;

        if (Source is object && value is object)
        {
            oldSelection = new object?[WritableSelectedItems.Count];
            WritableSelectedItems.CopyTo(oldSelection, 0);
        }

        try
        {
            _ignoreSelectedItemsChanges = true;
            ++_ignoreModelChanges;
            this.SetSource(value);
            _collectionView = collectionView;
        }
        finally
        {
            --_ignoreModelChanges;
            _ignoreSelectedItemsChanges = false;
        }

        if (oldSelection is null)
        {
            SyncToSelectedItems();
        }
        else
        {
            SyncFromSelectedItems();
        }
    }

    private void SyncToSelectedItems()
    {
        if (_writableSelectedItems is object &&
            !SequenceEqual(_writableSelectedItems, base.SelectedItems))
        {
            try
            {
                _ignoreSelectedItemsChanges = true;
                _writableSelectedItems.Clear();

                foreach (var i in base.SelectedItems)
                {
                    _writableSelectedItems.Add(i);
                }
            }
            finally
            {
                _ignoreSelectedItemsChanges = false;
            }
        }
    }

    private void SyncFromSelectedItems()
    {
        if (_skipSyncFromSelectedItems || Source is null || _writableSelectedItems is null)
        {
            return;
        }

        try
        {
            ++_ignoreModelChanges;

            using (BatchUpdate())
            {
                Clear();

                for (var i = 0; i < _writableSelectedItems.Count; ++i)
                {
                    var index = IndexOf(Source, _writableSelectedItems[i]);

                    if (index != -1)
                    {
                        Select(index);
                    }
                    else
                    {
                        _writableSelectedItems.RemoveAt(i);
                        --i;
                    }
                }
            }
        }
        finally
        {
            --_ignoreModelChanges;
        }
    }

    private void SubscribeToSelectedItems()
    {
        if (_writableSelectedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += OnSelectedItemsCollectionChanged;
        }
    }

    private void UnsubscribeFromSelectedItems()
    {
        if (_writableSelectedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= OnSelectedItemsCollectionChanged;
        }
    }

    private void OnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        if (_ignoreModelChanges > 0)
        {
            return;
        }

        try
        {
            var items      = WritableSelectedItems;
            var deselected = e.DeselectedItems.ToArray();
            var selected   = e.SelectedItems.ToArray();

            _ignoreSelectedItemsChanges = true;

            foreach (var i in deselected)
            {
                items.Remove(i);
            }

            foreach (var i in selected)
            {
                items.Add(i);
            }
        }
        finally
        {
            _ignoreSelectedItemsChanges = false;
        }
    }

    protected override void OnSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            ++_ignoreModelChanges;
            _isResetting = true;
        }

        base.OnSourceCollectionChanged(e);
    }

    protected override void OnSourceCollectionChangeFinished()
    {
        base.OnSourceCollectionChangeFinished();

        if (_isResetting)
        {
            --_ignoreModelChanges;
            _isResetting = false;
        }
    }

    private void OnSourceReset(object? sender, EventArgs e) => SyncFromSelectedItems();

    private void OnSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_ignoreSelectedItemsChanges)
        {
            return;
        }

        if (_writableSelectedItems == null)
        {
            throw new AvaloniaInternalException("CollectionChanged raised but we don't have items.");
        }

        void Remove()
        {
            foreach (var i in e.OldItems!)
            {
                var index = IndexOf(Source, i);

                if (index != -1)
                {
                    Deselect(index);
                }
            }
        }

        try
        {
            using var operation = BatchUpdate();

            ++_ignoreModelChanges;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Remove();
                    Add(e.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    Add(_writableSelectedItems);
                    break;
            }
        }
        finally
        {
            --_ignoreModelChanges;
        }
    }

    private void Add(IList newItems)
    {
        foreach (var i in newItems)
        {
            var index = IndexOf(Source, i);

            if (index != -1)
            {
                Select(index);
            }
        }
    }

    private static int IndexOf(object? source, object? item)
    {
        if (source is IList l)
        {
            return l.IndexOf(item);
        }
        else if (source is ItemsSourceView v)
        {
            return v.IndexOf(item);
        }

        return -1;
    }

    private static bool SequenceEqual(IList first, IReadOnlyList<object?> second)
    {
        if (first is IEnumerable<object?> e)
        {
            return e.SequenceEqual(second);
        }

        var       comparer = EqualityComparer<object?>.Default;
        var       e1       = first.GetEnumerator();
        using var e2       = second.GetEnumerator();

        while (e1.MoveNext())
        {
            if (!(e2.MoveNext() && comparer.Equals(e1.Current, e2.Current)))
            {
                return false;
            }
        }

        return !e2.MoveNext();
    }
    
}