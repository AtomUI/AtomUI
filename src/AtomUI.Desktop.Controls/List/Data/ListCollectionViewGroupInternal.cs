using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;

namespace AtomUI.Desktop.Controls.Data;

internal class ListCollectionViewGroupInternal : ListCollectionViewGroup
{
    /// <summary>
    /// GroupDescription used to define how to group the items
    /// </summary>
    private IListGroupDescription? _groupBy;

    /// <summary>
    /// Parent group of this ListCollectionViewGroupInternal
    /// </summary>
    private readonly ListCollectionViewGroupInternal? _parentGroup;

    /// <summary>
    /// Used for detecting stale enumerators
    /// </summary>
    private int _version;

    public ListCollectionViewGroupInternal(object key, ListCollectionViewGroupInternal? parent)
        : base(key)
    {
        _parentGroup = parent;
    }

    public override bool IsBottomLevel => _groupBy == null;

    internal int FullCount { get; set; }

    internal override IListGroupDescription? GroupBy
    {
        get => _groupBy;
        set
        {
            bool oldIsBottomLevel = IsBottomLevel;

            if (_groupBy != null)
            {
                ((INotifyPropertyChanged)_groupBy).PropertyChanged -= OnGroupByChanged;
            }

            _groupBy = value;

            if (_groupBy != null)
            {
                ((INotifyPropertyChanged)_groupBy).PropertyChanged += OnGroupByChanged;
            }

            if (oldIsBottomLevel != IsBottomLevel)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsBottomLevel)));
            }
        }
    }

    private void OnGroupByChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnGroupByChanged();
    }

    protected virtual void OnGroupByChanged()
    {
        _parentGroup?.OnGroupByChanged();
    }

    /// <summary>
    /// Gets or sets the most recent index where activity took place
    /// </summary>
    internal int LastIndex { get; set; }

    /// <summary>
    /// Gets the first item (leaf) added to this group.  If this can't be determined,
    /// DependencyProperty.UnsetValue.
    /// </summary>
    internal object SeedItem
    {
        get
        {
            if (ItemCount > 0 && (GroupBy == null || GroupBy.GroupKeys.Count == 0))
            {
                // look for first item, child by child
                for (int k = 0, n = Items.Count; k < n; ++k)
                {
                    if (!(Items[k] is ListCollectionViewGroupInternal subgroup))
                    {
                        // child is an item - return it
                        return Items[k];
                    }
                    if (subgroup.ItemCount > 0)
                    {
                        // child is a nonempty subgroup - ask it
                        return subgroup.SeedItem;
                    }
                    //// otherwise child is an empty subgroup - go to next child
                }

                // we shouldn't get here, but just in case...

                return AvaloniaProperty.UnsetValue;
            }
           
            // the group is empty, or it has explicit subgroups.
            // In either case, we cannot determine the first item -
            // it could have gone into any of the subgroups.
            return AvaloniaProperty.UnsetValue;
        }
    }

    internal override ListCollectionViewGroupInternal? Parent => _parentGroup;

    /// <summary>
    /// Adds the specified item to the collection
    /// </summary>
    /// <param name="item">Item to add</param>
    internal void Add(object item)
    {
        ChangeCounts(item, 1);
        ProtectedItems.Add(item);
    }

    /// <summary>
    /// Clears the collection of items
    /// </summary>
    internal void Clear()
    {
        ProtectedItems.Clear();
        FullCount          = 1;
        ProtectedItemCount = 0;
    }

    /// <summary>
    /// Finds the index of the specified item
    /// </summary>
    /// <param name="item">Item we are looking for</param>
    /// <param name="seed">Seed of the item we are looking for</param>
    /// <param name="comparer">Comparer used to find the item</param>
    /// <param name="low">Low range of item index</param>
    /// <param name="high">High range of item index</param>
    /// <returns>Index of the specified item</returns>
    protected virtual int FindIndex(object item, object seed, IComparer? comparer, int low, int high)
    {
        int index;

        if (comparer != null)
        {
            if (comparer is ListComparer listComparer)
            {
                // reset the IListComparer before each search. This cannot be done
                // any less frequently (e.g. in Root.AddToSubgroups), due to the
                // possibility that the item may appear in more than one subgroup.
                listComparer.Reset();
            }

            if (comparer is CollectionViewGroupComparer groupComparer)
            {
                // reset the CollectionViewGroupComparer before each search. This cannot be done
                // any less frequently (e.g. in Root.AddToSubgroups), due to the
                // possibility that the item may appear in more than one subgroup.
                groupComparer.Reset();
            }

            for (index = low; index < high; ++index)
            {
                object seed1 = (ProtectedItems[index] is ListCollectionViewGroupInternal subgroup)
                    ? subgroup.SeedItem
                    : ProtectedItems[index];
                if (seed1 == AvaloniaProperty.UnsetValue)
                {
                    continue;
                }

                if (comparer.Compare(seed, seed1) < 0)
                {
                    break;
                }
            }
        }
        else
        {
            index = high;
        }

        return index;
    }

    /// <summary>
    /// Returns an enumerator over the leaves governed by this group
    /// </summary>
    /// <returns>Enumerator of leaves</returns>
    internal IEnumerator GetLeafEnumerator()
    {
        return new LeafEnumerator(this);
    }

    /// <summary>
    /// Insert a new item or subgroup and return its index.  Seed is a
    /// representative from the subgroup (or the item itself) that
    /// is used to position the new item/subgroup w.r.t. the order given
    /// by the comparer. (If comparer is null, just add at the end).
    /// </summary>
    /// <param name="item">Item we are looking for</param>
    /// <param name="seed">Seed of the item we are looking for</param>
    /// <param name="comparer">Comparer used to find the item</param>
    /// <returns>The index where the item was inserted</returns>
    internal int Insert(object item, object seed, IComparer comparer)
    {
        // never insert the new item/group before the explicit subgroups
        int low   = (GroupBy == null) ? 0 : GroupBy.GroupKeys.Count;
        int index = FindIndex(item, seed, comparer, low, ProtectedItems.Count);

        // now insert the item
        ChangeCounts(item, +1);
        ProtectedItems.Insert(index, item);

        return index;
    }

    /// <summary>
    /// Return the item at the given index within the list of leaves governed
    /// by this group
    /// </summary>
    /// <param name="index">Index of the leaf</param>
    /// <returns>Item at given index</returns>
    internal object? LeafAt(int index)
    {
        for (int k = 0, n = Items.Count; k < n; ++k)
        {
            if (Items[k] is ListCollectionViewGroupInternal subgroup)
            {
                // current item is a group - either drill in, or skip over
                if (index < subgroup.ItemCount)
                {
                    return subgroup.LeafAt(index);
                }
                index -= subgroup.ItemCount;
            }
            else
            {
                // current item is a leaf - see if we're done
                if (index == 0)
                {
                    return Items[k];
                }
                index -= 1;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the index of the given item within the list of leaves governed
    /// by the full group structure.  The item must be a (direct) child of this
    /// group.  The caller provides the index of the item within this group,
    /// if known, or -1 if not.
    /// </summary>
    /// <param name="item">Item we are looking for</param>
    /// <param name="index">Index of the leaf</param>
    /// <returns>Number of items under that leaf</returns>
    internal int LeafIndexFromItem(object? item, int index)
    {
        int result = 0;

        // accumulate the number of predecessors at each level
        for (ListCollectionViewGroupInternal? group = this;
             group != null;
             item = group, group = group.Parent, index = -1)
        {
            // accumulate the number of predecessors at the level of item
            for (int k = 0, n = group.Items.Count; k < n; ++k)
            {
                // if we've reached the item, move up to the next level
                if ((index < 0 && Equals(item, group.Items[k])) ||
                    index == k)
                {
                    break;
                }

                // accumulate leaf count
                ListCollectionViewGroupInternal? subgroup = group.Items[k] as ListCollectionViewGroupInternal;
                result += subgroup?.ItemCount ?? 1;
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the index of the given item within the list of leaves governed
    /// by this group
    /// </summary>
    /// <param name="item">Item we are looking for</param>
    /// <returns>Number of items under that leaf</returns>
    internal int LeafIndexOf(object? item)
    {
        int leaves = 0; // number of leaves we've passed over so far
        for (int k = 0, n = Items.Count; k < n; ++k)
        {
            if (Items[k] is ListCollectionViewGroupInternal subgroup)
            {
                int subgroupIndex = subgroup.LeafIndexOf(item);
                if (subgroupIndex < 0)
                {
                    leaves += subgroup.ItemCount; // item not in this subgroup
                }
                else
                {
                    return leaves + subgroupIndex; // item is in this subgroup
                }
            }
            else
            {
                // current item is a leaf - compare it directly
                if (Equals(item, Items[k]))
                {
                    return leaves;
                }
                leaves += 1;
            }
        }

        // item not found
        return -1;
    }

    /// <summary>
    /// Removes the specified item from the collection
    /// </summary>
    /// <param name="item">Item to remove</param>
    /// <param name="returnLeafIndex">Whether we want to return the leaf index</param>
    /// <returns>Leaf index where item was removed, if value was specified. Otherwise '-1'</returns>
    internal int Remove(object item, bool returnLeafIndex)
    {
        int index      = -1;
        int localIndex = ProtectedItems.IndexOf(item);

        if (localIndex >= 0)
        {
            if (returnLeafIndex)
            {
                index = LeafIndexFromItem(null, localIndex);
            }

            ChangeCounts(item, -1);
            ProtectedItems.RemoveAt(localIndex);
        }

        return index;
    }

    /// <summary>
    /// Removes an empty group from the PagedCollectionView grouping
    /// </summary>
    /// <param name="group">Empty subgroup to remove</param>
    private static void RemoveEmptyGroup(ListCollectionViewGroupInternal group)
    {
        ListCollectionViewGroupInternal? parent = group.Parent;

        if (parent != null)
        {
            IListGroupDescription? groupBy = parent.GroupBy;
            int                    index   = parent.ProtectedItems.IndexOf(group);

            // remove the subgroup unless it is one of the explicit groups
            if (index >= groupBy?.GroupKeys.Count)
            {
                parent.Remove(group, false);
            }
        }
    }

    /// <summary>
    /// Update the item count of the CollectionViewGroup
    /// </summary>
    /// <param name="item">CollectionViewGroup to update</param>
    /// <param name="delta">Delta to change count by</param>
    protected void ChangeCounts(object item, int delta)
    {
        bool changeLeafCount = !(item is ListCollectionViewGroup);

        for (ListCollectionViewGroupInternal? group = this;
             group != null;
             group = group._parentGroup)
        {
            group.FullCount += delta;
            if (changeLeafCount)
            {
                group.ProtectedItemCount += delta;

                if (group.ProtectedItemCount == 0)
                {
                    RemoveEmptyGroup(group);
                }
            }
        }

        unchecked
        {
            // this invalidates enumerators
            ++_version;
        }
    }

    /// <summary>
    /// Enumerator for the leaves in the CollectionViewGroupInternal class.
    /// </summary>
    private class LeafEnumerator : IEnumerator
    {
        private object? _current; // current item
        private ListCollectionViewGroupInternal? _group; // parent group
        private int _index; // current index into Items
        private IEnumerator? _subEnum; // enumerator over current subgroup
        private int _version; // parent group's version at ctor

        /// <summary>
        /// Initializes a new instance of the LeafEnumerator class.
        /// </summary>
        /// <param name="group">CollectionViewGroupInternal that uses the enumerator</param>
        public LeafEnumerator(ListCollectionViewGroupInternal group)
        {
            _group = group;
            DoReset(); // don't call virtual Reset in ctor
        }

        /// <summary>
        /// Private helper to reset the enumerator
        /// </summary>
        private void DoReset()
        {
            Debug.Assert(_group != null, "_group should have been initialized in constructor");
            _version = _group._version;
            _index   = -1;
            _subEnum = null;
        }

        /// <summary>
        /// Reset implementation for IEnumerator
        /// </summary>
        void IEnumerator.Reset()
        {
            DoReset();
        }

        /// <summary>
        /// MoveNext implementation for IEnumerator
        /// </summary>
        /// <returns>Returns whether the MoveNext operation was successful</returns>
        bool IEnumerator.MoveNext()
        {
            Debug.Assert(_group != null, "_group should have been initialized in constructor");
            Debug.Assert(_group.Key != null);
            
            // check for invalidated enumerator
            if (_group._version != _version)
            {
                throw new InvalidOperationException();
            }

            // move forward to the next leaf
            while (_subEnum == null || !_subEnum.MoveNext())
            {
                // done with the current top-level item.  Move to the next one.
                ++_index;
                if (_index >= _group.Items.Count)
                {
                    return false;
                }

                if (_group.Items[_index] is ListCollectionViewGroupInternal subgroup)
                {
                    // current item is a subgroup - get its enumerator
                    _subEnum = subgroup.GetLeafEnumerator();
                }
                else
                {
                    // current item is a leaf - it's the new Current
                    _current       = _group.Items[_index];
                    _subEnum       = null;
                    return true;
                }
            }

            // the loop terminates only when we have a subgroup enumerator
            // positioned at the new Current item
            _current = _subEnum.Current;
            return true;
        }

        /// <summary>
        /// Gets the current implementation for IEnumerator
        /// </summary>
        object? IEnumerator.Current
        {
            get
            {
                Debug.Assert(_group != null, "_group should have been initialized in constructor");
                if (_index < 0 || _index >= _group.Items.Count)
                {
                    throw new InvalidOperationException();
                }

                return _current;
            }
        }
    }

    // / <summary>
    // / This comparer is used to insert an item into a group in a position consistent
    // / with a given IList.  It only works when used in the pattern that FindIndex
    // / uses, namely first call Reset(), then call Compare(item, itemSequence) any number of
    // / times with the same item (the new item) as the first argument, and a sequence
    // / of items as the second argument that appear in the IList in the same sequence.
    // / This makes the total search time linear in the size of the IList.  (To give
    // / the correct answer regardless of the sequence of arguments would involve
    // / calling IndexOf and leads to O(N^2) total search time.) 
    // / </summary>
    internal class ListComparer : IComparer
    {
        /// <summary>
        /// Constructor for the ListComparer that takes
        /// in an IList.
        /// </summary>
        /// <param name="list">IList used to compare on</param>
        internal ListComparer(IList list)
        {
            _list  = list;
            _index = 0;
        }

        /// <summary>
        /// Sets the index that we start comparing
        /// from to 0.
        /// </summary>
        internal void Reset()
        {
            _index = 0;
        }

        /// <summary>
        /// Sets our IList to a new instance
        /// of a list being passed in and resets
        /// the index.
        /// </summary>
        /// <param name="list">IList used to compare on</param>
        internal void ResetList(IList list)
        {
            _list  = list;
            _index = 0;
        }

        /// <summary>
        /// Compares objects x and y to see which one
        /// should appear first.
        /// </summary>
        /// <param name="x">The first object</param>
        /// <param name="y">The second object</param>
        /// <returns>-1 if x is less than y, +1 otherwise</returns>
        public int Compare(object? x, object? y)
        {
            if (Equals(x, y))
            {
                return 0;
            }

            // advance the index until seeing one x or y
            int n = _list.Count;
            for (; _index < n; ++_index)
            {
                object? z = _list[_index];
                if (Equals(x, z))
                {
                    return -1; // x occurs first, so x < y
                }
                if (Equals(y, z))
                {
                    return +1; // y occurs first, so x > y
                }
            }

            // if we don't see either x or y, declare x > y.
            // This has the effect of putting x at the end of the list.
            return +1;
        }

        private int _index;
        private IList _list;
    }

    // / <summary>
    // / This comparer is used to insert an item into a group in a position consistent
    // / with a given ListCollectionViewGroupRoot. We will only use this when dealing with
    // / a temporary ListCollectionViewGroupRoot that points to the correct grouping of the
    // / entire collection, and we have paging that requires us to keep the paged group
    // / consistent with the order of items in the temporary group.
    // / </summary>
    internal class CollectionViewGroupComparer : IComparer
    {
        /// <summary>
        /// Constructor for the CollectionViewGroupComparer that takes
        /// in an ListCollectionViewGroupRoot.
        /// </summary>
        /// <param name="group">ListCollectionViewGroupRoot used to compare on</param>
        internal CollectionViewGroupComparer(ListCollectionViewGroupRoot group)
        {
            _group = group;
            _index = 0;
        }

        /// <summary>
        /// Sets the index that we start comparing
        /// from to 0.
        /// </summary>
        internal void Reset()
        {
            _index = 0;
        }

        /// <summary>
        /// Sets our group to a new instance of a
        /// ListCollectionViewGroupRoot being passed in
        /// and resets the index.
        /// </summary>
        /// <param name="group">ListCollectionViewGroupRoot used to compare on</param>
        internal void ResetGroup(ListCollectionViewGroupRoot group)
        {
            _group = group;
            _index = 0;
        }

        /// <summary>
        /// Compares objects x and y to see which one
        /// should appear first.
        /// </summary>
        /// <param name="x">The first object</param>
        /// <param name="y">The second object</param>
        /// <returns>-1 if x is less than y, +1 otherwise</returns>
        public int Compare(object? x, object? y)
        {
            if (Equals(x, y))
            {
                return 0;
            }

            // advance the index until seeing one x or y
            int n = _group.ItemCount;
            for (; _index < n; ++_index)
            {
                object? z = _group.LeafAt(_index);
                if (Equals(x, z))
                {
                    return -1; // x occurs first, so x < y
                }
                if (Equals(y, z))
                {
                    return +1; // y occurs first, so x > y
                }
            }

            // if we don't see either x or y, declare x > y.
            // This has the effect of putting x at the end of the list.
            return +1;
        }

        private int _index;
        private ListCollectionViewGroupRoot _group;
    }
}