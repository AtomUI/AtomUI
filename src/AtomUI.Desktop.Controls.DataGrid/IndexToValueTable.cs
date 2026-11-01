// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using AtomUI.Desktop.Controls.Utils;

namespace AtomUI.Desktop.Controls;

internal class IndexToValueTable<T> : IEnumerable<Range<T>>
{
    private List<Range<T>> _list;

    public IndexToValueTable()
    {
        _list = new List<Range<T>>();
    }

    private IndexToValueTable(int capacity)
    {
        _list = new List<Range<T>>(capacity);
    }

    /// <summary>
    /// Add a value with an associated index to the table
    /// </summary>
    /// <param name="index">Index where the value is to be added or updated</param>
    /// <param name="value">Value to add</param>
    public void AddValue(int index, T value)
    {
        AddValues(index, 1, value);
    }

    /// <summary>
    /// Add multiples values with an associated start index to the table 
    /// </summary>
    /// <param name="startIndex">index where first value is added</param>
    /// <param name="count">Total number of values to add (must be greater than 0)</param>
    /// <param name="value">Value to add</param>
    public void AddValues(int startIndex, int count, T value)
    {
        Debug.Assert(count > 0);
        AddValuesPrivate(startIndex, count, value, null);
    }

    /// <summary>
    /// Clears the index table
    /// </summary>
    public void Clear()
    {
        _list.Clear();
    }

    /// <summary>
    /// Returns true if the given index is contained in the table
    /// </summary>
    /// <param name="index">index to search for</param>
    /// <returns>True if the index is contained in the table</returns>
    public bool Contains(int index)
    {
        return IsCorrectRangeIndex(this.FindRangeIndex(index), index);
    }

    public IndexEnumerable EnumerateIndexes()
    {
        return new IndexEnumerable(_list);
    }

    public IndexEnumerable EnumerateIndexes(int startIndex)
    {
        return new IndexEnumerable(_list, startIndex);
    }

    /// <summary>
    /// Returns the value at a given index or the default value if the index is not in the table
    /// </summary>
    /// <param name="index">index to search for</param>
    /// <returns>the value at the given index or the default value if index is not in the table</returns>
    public T? GetValueAt(int index)
    {
        return GetValueAt(index, out bool found);
    }

    /// <summary>
    /// Returns the value at a given index or the default value if the index is not in the table
    /// </summary>
    /// <param name="index">index to search for</param>
    /// <param name="found">set to true by the method if the index was found; otherwise, false</param>
    /// <returns>the value at the given index or the default value if index is not in the table</returns>
    public T? GetValueAt(int index, out bool found)
    {
        int rangeIndex = FindRangeIndex(index);
        if (IsCorrectRangeIndex(rangeIndex, index))
        {
            found = true;
            return _list[rangeIndex].Value;
        }
        found = false;
        return default;
    }

    /// <summary>
    /// Removes a value and its index from the table
    /// </summary>
    /// <param name="index">index to remove</param>
    public void RemoveIndexAndValue(int index)
    {
        RemoveIndexesAndValues(index, 1);
    }

    /// <summary>
    /// Removes multiple indexes from the table.  This does not alter Values in the table
    /// </summary>
    /// <param name="startIndex">first index to remove</param>
    /// <param name="count">total number of indexes to remove</param>
    public void RemoveIndexes(int startIndex, int count)
    {
        int lowerRangeIndex = FindRangeIndex(startIndex);
        if (lowerRangeIndex < 0)
        {
            lowerRangeIndex = 0;
        }
        int i = lowerRangeIndex;
        while (i < _list.Count)
        {
            Range<T> range = _list[i];
            if (range.UpperBound >= startIndex)
            {
                if (range.LowerBound >= startIndex + count)
                {
                    // Both bounds will remain after the removal
                    range.LowerBound -= count;
                    range.UpperBound -= count;
                }
                else
                {
                    int currentIndex = i;
                    if (range.LowerBound <= startIndex)
                    {
                        // Range gets split up
                        if (range.UpperBound >= startIndex + count)
                        {
                            i++;
                            _list.Insert(i, new Range<T>(startIndex, range.UpperBound - count, range.Value));
                        }
                        range.UpperBound = startIndex - 1;
                    }
                    else
                    {
                        range.LowerBound =  startIndex;
                        range.UpperBound -= count;
                    }
                    if (RemoveRangeIfInvalid(range, currentIndex))
                    {
                        i--;
                    }
                }
            }
            i++;
        }
        if (!Merge(lowerRangeIndex))
        {
            Merge(lowerRangeIndex + 1);
        }
    }

    /// <summary>
    /// Removes multiple values and their indexes from the table
    /// </summary>
    /// <param name="startIndex">first index to remove</param>
    /// <param name="count">total number of indexes to remove</param>
    public void RemoveIndexesAndValues(int startIndex, int count)
    {
        RemoveValues(startIndex, count);
        RemoveIndexes(startIndex, count);
    }

    /// <summary>
    /// Removes multiple values from the table.  This does not alter other indexes in the table.
    /// </summary>
    /// <param name="startIndex">first index where values should be removed </param>
    /// <param name="count">total number of values to remove</param>
    public void RemoveValues(int startIndex, int count)
    {
        Debug.Assert(count > 0);

        int lowerRangeIndex = FindRangeIndex(startIndex);
        if (lowerRangeIndex < 0)
        {
            lowerRangeIndex = 0;
        }
        while ((lowerRangeIndex < _list.Count) && (_list[lowerRangeIndex].UpperBound < startIndex))
        {
            lowerRangeIndex++;
        }
        if (lowerRangeIndex >= _list.Count || _list[lowerRangeIndex].LowerBound > startIndex + count - 1)
        {
            // If all the values are above our below our values, we have nothing to remove
            return;
        }
        if (_list[lowerRangeIndex].LowerBound < startIndex)
        {
            // Need to split this up
            _list.Insert(lowerRangeIndex, new Range<T>(_list[lowerRangeIndex].LowerBound, startIndex - 1, _list[lowerRangeIndex].Value));
            lowerRangeIndex++;
        }
        _list[lowerRangeIndex].LowerBound = startIndex + count;
        if (!RemoveRangeIfInvalid(_list[lowerRangeIndex], lowerRangeIndex))
        {
            lowerRangeIndex++;
        }
        while ((lowerRangeIndex < _list.Count) && (_list[lowerRangeIndex].UpperBound < startIndex + count))
        {
            _list.RemoveAt(lowerRangeIndex);
        }
        if ((lowerRangeIndex < _list.Count) && (_list[lowerRangeIndex].UpperBound >= startIndex + count) &&
            (_list[lowerRangeIndex].LowerBound < startIndex + count))
        {
            // Chop off the start of the remaining Range if it contains values that we're removing
            _list[lowerRangeIndex].LowerBound = startIndex + count;
            RemoveRangeIfInvalid(_list[lowerRangeIndex], lowerRangeIndex);
        }
    }

    private void AddValuesPrivate(int startIndex, int count, T value, int? startRangeIndex)
    {
        Debug.Assert(count > 0);

        int      endIndex = startIndex + count - 1;
        Range<T> newRange = new Range<T>(startIndex, endIndex, value);
        if (_list.Count == 0)
        {
            _list.Add(newRange);
        }
        else
        {
            int      lowerRangeIndex = startRangeIndex ?? FindRangeIndex(startIndex);
            Range<T>? lowerRange      = (lowerRangeIndex < 0) ? null : _list[lowerRangeIndex];
            if (lowerRange == null)
            {
                if (lowerRangeIndex < 0)
                {
                    lowerRangeIndex = 0;
                }
                _list.Insert(lowerRangeIndex, newRange);
            }
            else
            {
                if (lowerRange.Value != null && !lowerRange.Value.Equals(value) && (lowerRange.UpperBound >= startIndex))
                {
                    // Split up the range
                    if (lowerRange.UpperBound > endIndex)
                    {
                        _list.Insert(lowerRangeIndex + 1, new Range<T>(endIndex + 1, lowerRange.UpperBound, lowerRange.Value));
                    }
                    lowerRange.UpperBound = startIndex - 1;
                    if (!RemoveRangeIfInvalid(lowerRange, lowerRangeIndex))
                    {
                        lowerRangeIndex++;
                    }
                    _list.Insert(lowerRangeIndex, newRange);
                }
                else
                {
                    _list.Insert(lowerRangeIndex + 1, newRange);
                    if (!Merge(lowerRangeIndex))
                    {
                        lowerRangeIndex++;
                    }
                }
            }

            // At this point the newRange has been inserted in the correct place, now we need to remove
            // any subsequent ranges that no longer make sense and possibly update the one at newRange.UpperBound
            int upperRangeIndex = lowerRangeIndex + 1;
            while ((upperRangeIndex < _list.Count) && (_list[upperRangeIndex].UpperBound < endIndex))
            {
                _list.RemoveAt(upperRangeIndex);
            }
            if (upperRangeIndex < _list.Count)
            {
                Range<T> upperRange = _list[upperRangeIndex];
                if (upperRange.LowerBound <= endIndex)
                {
                    // Update the range
                    upperRange.LowerBound = endIndex + 1;
                    RemoveRangeIfInvalid(upperRange, upperRangeIndex);
                }
                Merge(lowerRangeIndex);
            }
        }
    }

    // Returns the index of the range that contains the input or the range before if the input is not found
    private int FindRangeIndex(int index)
    {
        if (_list.Count == 0)
        {
            return -1;
        }

        // Do a binary search for the index
        int      front = 0;
        int      end   = _list.Count - 1;
        Range<T>? range = null;
        while (end > front)
        {
            int median = (front + end) / 2;
            range = _list[median];
            if (range.UpperBound < index)
            {
                front = median + 1;
            }
            else if (range.LowerBound > index)
            {
                end = median - 1;
            }
            else
            {
                // we found it
                return median;
            }
        }

        if (front == end)
        {
            range = _list[front];
            if (range.ContainsIndex(index) || (range.UpperBound < index))
            {
                // we found it or the index isn't there and we're one range before
                return front;
            }
            // not found and we're one range after
            return front - 1;
        }
        // end is one index before front in this case so it's the range before
        return end;
    }

    private bool Merge(int lowerRangeIndex)
    {
        int upperRangeIndex = lowerRangeIndex + 1;
        if ((lowerRangeIndex >= 0) && (upperRangeIndex < _list.Count))
        {
            Range<T> lowerRange = _list[lowerRangeIndex];
            Range<T> upperRange = _list[upperRangeIndex];
            if ((lowerRange.UpperBound + 1 >= upperRange.LowerBound) && (lowerRange.Value != null && lowerRange.Value.Equals(upperRange.Value)))
            {
                lowerRange.UpperBound = Math.Max(lowerRange.UpperBound, upperRange.UpperBound);
                _list.RemoveAt(upperRangeIndex);
                return true;
            }
        }
        return false;
    }

    private bool IsCorrectRangeIndex(int rangeIndex, int index)
    {
        return (-1 != rangeIndex) && (_list[rangeIndex].ContainsIndex(index));
    }

    private bool RemoveRangeIfInvalid(Range<T> range, int rangeIndex)
    {
        if (range.UpperBound < range.LowerBound)
        {
            _list.RemoveAt(rangeIndex);
            return true;
        }
        return false;
    }

    public IEnumerator<Range<T>> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public readonly struct IndexEnumerable
    {
        private readonly List<Range<T>> _ranges;
        private readonly int _startIndex;

        internal IndexEnumerable(List<Range<T>> ranges)
            : this(ranges, int.MinValue)
        {
        }

        internal IndexEnumerable(List<Range<T>> ranges, int startIndex)
        {
            _ranges     = ranges;
            _startIndex = startIndex;
        }

        public IndexEnumerator GetEnumerator()
        {
            return new IndexEnumerator(_ranges, _startIndex);
        }
    }

    public struct IndexEnumerator
    {
        private readonly List<Range<T>> _ranges;
        private readonly int _startIndex;
        private int _rangeIndex;
        private int _current;
        private int _upperBound;
        private bool _initialized;

        internal IndexEnumerator(List<Range<T>> ranges)
            : this(ranges, int.MinValue)
        {
        }

        internal IndexEnumerator(List<Range<T>> ranges, int startIndex)
        {
            _ranges      = ranges;
            _startIndex  = startIndex;
            _rangeIndex  = -1;
            _current     = 0;
            _upperBound  = -1;
            _initialized = false;
            Current      = 0;
        }

        public int Current { get; private set; }

        public bool MoveNext()
        {
            if (!_initialized || _current >= _upperBound)
            {
                _rangeIndex = _initialized ? _rangeIndex + 1 : 0;
                _initialized = true;

                while (_rangeIndex < _ranges.Count && _ranges[_rangeIndex].UpperBound < _startIndex)
                {
                    _rangeIndex++;
                }

                if (_rangeIndex >= _ranges.Count)
                {
                    return false;
                }

                Range<T> range = _ranges[_rangeIndex];
                _current    = Math.Max(range.LowerBound, _startIndex);
                _upperBound = range.UpperBound;
                Current     = _current;
                return true;
            }

            _current++;
            Current = _current;
            return true;
        }
    }
}
