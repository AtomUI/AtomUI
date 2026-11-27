namespace AtomUI.Desktop.Controls;

internal readonly struct DataGridHeightClass : IEquatable<DataGridHeightClass>
{
    private DataGridHeightClass(DataGridSourceEntryKind kind, int groupLevel)
    {
        Kind = kind;
        GroupLevel = groupLevel;
    }

    public static DataGridHeightClass Data { get; } =
        new(DataGridSourceEntryKind.Data, -1);

    public DataGridSourceEntryKind Kind { get; }

    public int GroupLevel { get; }

    public static DataGridHeightClass GroupHeader(int level)
    {
        if (level < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }
        return new DataGridHeightClass(DataGridSourceEntryKind.GroupHeader, level);
    }

    public bool Equals(DataGridHeightClass other) =>
        Kind == other.Kind && GroupLevel == other.GroupLevel;

    public override bool Equals(object? obj) =>
        obj is DataGridHeightClass other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Kind, GroupLevel);

    public static bool operator ==(DataGridHeightClass left, DataGridHeightClass right) =>
        left.Equals(right);

    public static bool operator !=(DataGridHeightClass left, DataGridHeightClass right) =>
        !left.Equals(right);
}

internal sealed class SparseHeightDeltaIndex
{
    private readonly Dictionary<DataGridHeightClass, HeightEstimator> _estimators = [];
    private Node? _root;
    private int _lastProbeCount;

    public SparseHeightDeltaIndex(double defaultHeight)
    {
        ValidateHeight(defaultHeight, nameof(defaultHeight));
        DefaultHeight = defaultHeight;
    }

    public double DefaultHeight { get; private set; }

    public int MeasuredCount => Node.CountOf(_root);

    public int TreeHeight => Node.HeightOf(_root);

    public int LastProbeCount => _lastProbeCount;

    public int EstimatorBucketCount => _estimators.Count;

    public void RebaseDefaultHeight(double defaultHeight)
    {
        ValidateHeight(defaultHeight, nameof(defaultHeight));
        if (defaultHeight == DefaultHeight)
        {
            return;
        }
        DefaultHeight = defaultHeight;
        Rebase(_root, defaultHeight);
    }

    public void SetMeasuredHeight(
        int slot,
        double actualHeight,
        DataGridHeightClass heightClass)
    {
        if (slot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slot));
        }
        ValidateHeight(actualHeight, nameof(actualHeight));
        _root = InsertOrUpdate(_root, slot, actualHeight, heightClass);
    }

    public void SetMeasuredHeight(
        int slot,
        double baseHeight,
        double detailsHeight,
        DataGridHeightClass heightClass)
    {
        ValidateHeight(baseHeight, nameof(baseHeight));
        if (!double.IsFinite(detailsHeight) || detailsHeight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(detailsHeight));
        }
        var actualHeight = SafeAdd(baseHeight, detailsHeight);
        SetMeasuredHeight(slot, actualHeight, heightClass);
    }

    public double GetHeight(int slot)
    {
        if (slot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slot));
        }
        var node = Find(slot);
        return node?.ActualHeight ?? DefaultHeight;
    }

    public bool TryGetMeasuredHeight(int slot, out double height)
    {
        if (slot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slot));
        }
        var node = Find(slot);
        if (node is null)
        {
            height = default;
            return false;
        }
        height = node.ActualHeight;
        return true;
    }

    public double GetOffset(int slot)
    {
        if (slot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slot));
        }
        return StartOf(slot, PrefixDelta(slot));
    }

    public double GetExtent(int totalEntryCount)
    {
        if (totalEntryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalEntryCount));
        }
        return StartOf(totalEntryCount, PrefixDelta(totalEntryCount));
    }

    public int FindSlotAtOffset(double offset, int totalEntryCount)
    {
        if (!double.IsFinite(offset))
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
        if (totalEntryCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalEntryCount));
        }

        offset = Math.Max(0, offset);
        var extent = GetExtent(totalEntryCount);
        if (offset >= extent)
        {
            _lastProbeCount = 1;
            return totalEntryCount - 1;
        }

        var node = _root;
        var prefixDelta = 0d;
        var lowerSlot = 0;
        var upperSlot = totalEntryCount - 1;
        var probes = 0;
        while (node is not null)
        {
            probes++;
            var leftDelta = Node.DeltaOf(node.Left);
            if (offset < StartOf(node.Slot, SafeAdd(prefixDelta, leftDelta)))
            {
                upperSlot = Math.Min(upperSlot, node.Slot - 1);
                node = node.Left;
                continue;
            }

            // Slot intervals must join end to end: the end of a row is the start of the next slot, so it can
            // only be evaluated with the next slot's start formula. Evaluating it as "start of this row +
            // measured height" differs from that start by a few ULPs when the row height is not exactly
            // representable in binary (for example the layout rounding results at 125% and 150% scaling).
            // An offset that falls into this gap is claimed by neither row, and the lower and upper bounds
            // cross. Once the upper slot is reached no further comparison is needed: an earlier comparison
            // already established that the offset is before the following slot's start (or the total height).
            var nextPrefixDelta = SafeAdd(prefixDelta, SafeAdd(leftDelta, node.Delta));
            if (node.Slot >= upperSlot ||
                offset < StartOf(node.Slot + 1, nextPrefixDelta))
            {
                _lastProbeCount = probes;
                return Math.Min(node.Slot, totalEntryCount - 1);
            }

            prefixDelta = nextPrefixDelta;
            lowerSlot = Math.Max(lowerSlot, node.Slot + 1);
            node = node.Right;
        }

        var estimated = Math.Floor(
            Math.Max(0, SafeAdd(offset, -prefixDelta)) / DefaultHeight);
        var slot = estimated >= int.MaxValue ? int.MaxValue : (int)estimated;
        _lastProbeCount = probes + 1;
        return Math.Clamp(slot, lowerSlot, upperSlot);
    }

    public int RemoveRange(int startSlot, int count, bool absorbIntoEstimator)
    {
        if (startSlot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot));
        }
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        var endExclusive = checked(startSlot + count);
        var removedCount = 0;
        while (FindFirstAtOrAfter(_root, startSlot) is { } node &&
               node.Slot < endExclusive)
        {
            if (absorbIntoEstimator)
            {
                Absorb(node.HeightClass, node.ActualHeight);
            }
            _root = Remove(_root, node.Slot);
            removedCount++;
        }
        return removedCount;
    }

    public int RemoveHeightClass(DataGridHeightClass heightClass)
    {
        if (_root is null)
        {
            return 0;
        }
        var slots = new List<int>();
        CollectSlots(_root, heightClass, slots);
        foreach (var slot in slots)
        {
            _root = Remove(_root, slot);
        }
        return slots.Count;
    }

    public long GetEstimatorSampleCount(DataGridHeightClass heightClass) =>
        _estimators.TryGetValue(heightClass, out var estimator) ? estimator.Count : 0;

    public double GetEstimatedHeight(DataGridHeightClass heightClass) =>
        _estimators.TryGetValue(heightClass, out var estimator)
            ? estimator.Mean
            : DefaultHeight;

    private void Absorb(DataGridHeightClass heightClass, double actualHeight)
    {
        _estimators.TryGetValue(heightClass, out var estimator);
        estimator.Add(actualHeight);
        _estimators[heightClass] = estimator;
    }

    private Node? Find(int slot)
    {
        var node = _root;
        while (node is not null)
        {
            if (slot == node.Slot)
            {
                return node;
            }
            node = slot < node.Slot ? node.Left : node.Right;
        }
        return null;
    }

    // The only formula for a slot start: every slot boundary (including the total height, which is the
    // start after the last slot) must be evaluated through here, so that one boundary never yields two
    // values a few ULPs apart because of a different grouping of the operations.
    private double StartOf(int slot, double prefixDelta) =>
        Math.Max(0, SafeAdd(SafeMultiply(slot, DefaultHeight), prefixDelta));

    private double PrefixDelta(int slot)
    {
        var node = _root;
        var sum = 0d;
        while (node is not null)
        {
            if (slot <= node.Slot)
            {
                node = node.Left;
            }
            else
            {
                sum = SafeAdd(sum, SafeAdd(Node.DeltaOf(node.Left), node.Delta));
                node = node.Right;
            }
        }
        return sum;
    }

    private Node InsertOrUpdate(
        Node? node,
        int slot,
        double actualHeight,
        DataGridHeightClass heightClass)
    {
        if (node is null)
        {
            return new Node(slot, actualHeight, actualHeight - DefaultHeight, heightClass);
        }
        if (slot < node.Slot)
        {
            node.Left = InsertOrUpdate(node.Left, slot, actualHeight, heightClass);
        }
        else if (slot > node.Slot)
        {
            node.Right = InsertOrUpdate(node.Right, slot, actualHeight, heightClass);
        }
        else
        {
            node.ActualHeight = actualHeight;
            node.Delta = actualHeight - DefaultHeight;
            node.HeightClass = heightClass;
            node.Update();
            return node;
        }
        return Balance(node);
    }

    private static Node? Remove(Node? node, int slot)
    {
        if (node is null)
        {
            return null;
        }
        if (slot < node.Slot)
        {
            node.Left = Remove(node.Left, slot);
        }
        else if (slot > node.Slot)
        {
            node.Right = Remove(node.Right, slot);
        }
        else
        {
            if (node.Left is null)
            {
                return node.Right;
            }
            if (node.Right is null)
            {
                return node.Left;
            }
            var successor = FindMinimum(node.Right);
            node.Slot = successor.Slot;
            node.ActualHeight = successor.ActualHeight;
            node.Delta = successor.Delta;
            node.HeightClass = successor.HeightClass;
            node.Right = Remove(node.Right, successor.Slot);
        }
        return Balance(node);
    }

    private static Node? FindFirstAtOrAfter(Node? root, int slot)
    {
        Node? candidate = null;
        var node = root;
        while (node is not null)
        {
            if (node.Slot < slot)
            {
                node = node.Right;
            }
            else
            {
                candidate = node;
                node = node.Left;
            }
        }
        return candidate;
    }

    private static Node FindMinimum(Node node)
    {
        while (node.Left is not null)
        {
            node = node.Left;
        }
        return node;
    }

    private static void CollectSlots(
        Node? node,
        DataGridHeightClass heightClass,
        List<int> slots)
    {
        if (node is null)
        {
            return;
        }
        CollectSlots(node.Left, heightClass, slots);
        if (node.HeightClass == heightClass)
        {
            slots.Add(node.Slot);
        }
        CollectSlots(node.Right, heightClass, slots);
    }

    private static void Rebase(Node? node, double defaultHeight)
    {
        if (node is null)
        {
            return;
        }
        Rebase(node.Left, defaultHeight);
        Rebase(node.Right, defaultHeight);
        node.Delta = node.ActualHeight - defaultHeight;
        node.Update();
    }

    private static Node Balance(Node node)
    {
        node.Update();
        var balance = Node.HeightOf(node.Left) - Node.HeightOf(node.Right);
        if (balance > 1)
        {
            if (Node.HeightOf(node.Left!.Left) < Node.HeightOf(node.Left.Right))
            {
                node.Left = RotateLeft(node.Left);
            }
            return RotateRight(node);
        }
        if (balance < -1)
        {
            if (Node.HeightOf(node.Right!.Right) < Node.HeightOf(node.Right.Left))
            {
                node.Right = RotateRight(node.Right);
            }
            return RotateLeft(node);
        }
        return node;
    }

    private static Node RotateLeft(Node node)
    {
        var replacement = node.Right!;
        node.Right = replacement.Left;
        replacement.Left = node;
        node.Update();
        replacement.Update();
        return replacement;
    }

    private static Node RotateRight(Node node)
    {
        var replacement = node.Left!;
        node.Left = replacement.Right;
        replacement.Right = node;
        node.Update();
        replacement.Update();
        return replacement;
    }

    private static void ValidateHeight(double height, string parameterName)
    {
        if (!double.IsFinite(height) || height <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }

    internal static double SafeMultiply(int count, double value)
    {
        if (count == 0)
        {
            return 0;
        }
        return value > double.MaxValue / count ? double.MaxValue : count * value;
    }

    internal static double SafeAdd(double left, double right)
    {
        var sum = left + right;
        if (double.IsPositiveInfinity(sum))
        {
            return double.MaxValue;
        }
        if (double.IsNegativeInfinity(sum))
        {
            return -double.MaxValue;
        }
        return Math.Clamp(sum, -double.MaxValue, double.MaxValue);
    }

    private sealed class Node
    {
        public Node(
            int slot,
            double actualHeight,
            double delta,
            DataGridHeightClass heightClass)
        {
            Slot = slot;
            ActualHeight = actualHeight;
            Delta = delta;
            HeightClass = heightClass;
            Update();
        }

        public int Slot { get; set; }

        public double ActualHeight { get; set; }

        public double Delta { get; set; }

        public DataGridHeightClass HeightClass { get; set; }

        public Node? Left { get; set; }

        public Node? Right { get; set; }

        public int Height { get; private set; }

        public int Count { get; private set; }

        public double SubtreeDelta { get; private set; }

        public void Update()
        {
            Height = 1 + Math.Max(HeightOf(Left), HeightOf(Right));
            Count = 1 + CountOf(Left) + CountOf(Right);
            SubtreeDelta = SafeAdd(DeltaOf(Left), SafeAdd(Delta, DeltaOf(Right)));
        }

        public static int HeightOf(Node? node) => node?.Height ?? 0;

        public static int CountOf(Node? node) => node?.Count ?? 0;

        public static double DeltaOf(Node? node) => node?.SubtreeDelta ?? 0;
    }

    private struct HeightEstimator
    {
        public long Count { get; private set; }

        public double Mean { get; private set; }

        public void Add(double value)
        {
            Count = checked(Count + 1);
            Mean = Count == 1 ? value : Mean + (value - Mean) / Count;
        }
    }
}
