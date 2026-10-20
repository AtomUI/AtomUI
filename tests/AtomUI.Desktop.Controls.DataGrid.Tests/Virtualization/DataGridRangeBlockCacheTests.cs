using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridRangeBlockCacheTests
{
    private static readonly object SourceIdentity = new();
    private static readonly DataGridSourceSchema Schema = new(
        typeof(Row),
        [],
        32,
        32);

    [Fact]
    public void Pinned_Blocks_Survive_Pressure_And_Passive_Block_Evicts()
    {
        var cache = new DataGridRangeBlockCache(capacity: 2);
        var first = Block(0);
        var second = Block(32);
        var third = Block(64);
        cache.TryAdd(SourceIdentity, first).ShouldBeTrue();
        cache.TryAdd(SourceIdentity, second).ShouldBeTrue();

        using var firstPin = cache.AcquirePin(
            Identity(first), DataGridRangeBlockPinReason.Applied);
        using var secondPin = cache.AcquirePin(
            Identity(second), DataGridRangeBlockPinReason.Realized);
        cache.TryAdd(SourceIdentity, third).ShouldBeFalse();
        cache.Contains(Identity(first)).ShouldBeTrue();
        cache.Contains(Identity(second)).ShouldBeTrue();

        secondPin.Dispose();
        cache.TryAdd(SourceIdentity, third).ShouldBeTrue();
        cache.Contains(Identity(first)).ShouldBeTrue();
        cache.Contains(Identity(second)).ShouldBeFalse();
        cache.Contains(Identity(third)).ShouldBeTrue();
        cache.Count.ShouldBe(2);
    }

    [Fact]
    public void Duplicate_Block_Is_A_NoOp_And_Wrong_Snapshot_Is_Rejected()
    {
        var cache = new DataGridRangeBlockCache(capacity: 3);
        var first = Block(0, snapshot: "s1");
        cache.TryAdd(SourceIdentity, first).ShouldBeTrue();
        cache.TryAdd(SourceIdentity, first).ShouldBeFalse();

        cache.TryAdd(SourceIdentity, Block(32, snapshot: "s2")).ShouldBeFalse();
        cache.Count.ShouldBe(1);
    }

    [Fact]
    public void Cache_Rejects_Duplicate_Keys_Across_Current_Blocks()
    {
        var cache = new DataGridRangeBlockCache(capacity: 3);
        cache.TryAdd(SourceIdentity, Block(0)).ShouldBeTrue();

        Should.Throw<DataGridSourceContractException>(() =>
            cache.TryAdd(SourceIdentity, Block(32, firstRowKey: 0)));
    }

    [Fact]
    public void Pin_Reasons_Are_Countable_And_Token_Dispose_Is_Idempotent()
    {
        var cache = new DataGridRangeBlockCache(capacity: 1);
        var block = Block(0);
        var identity = Identity(block);
        cache.TryAdd(SourceIdentity, block).ShouldBeTrue();

        var applied = cache.AcquirePin(identity, DataGridRangeBlockPinReason.Applied);
        var realized = cache.AcquirePin(identity, DataGridRangeBlockPinReason.Realized);
        var edit = cache.AcquirePin(identity, DataGridRangeBlockPinReason.Edit);
        var drag = cache.AcquirePin(identity, DataGridRangeBlockPinReason.Drag);
        cache.GetPinCount(identity, DataGridRangeBlockPinReason.Applied).ShouldBe(1);
        cache.GetPinCount(identity, DataGridRangeBlockPinReason.Realized).ShouldBe(1);
        cache.GetPinCount(identity, DataGridRangeBlockPinReason.Edit).ShouldBe(1);
        cache.GetPinCount(identity, DataGridRangeBlockPinReason.Drag).ShouldBe(1);

        edit.Dispose();
        edit.Dispose();
        cache.GetPinCount(identity, DataGridRangeBlockPinReason.Edit).ShouldBe(0);
        applied.Dispose();
        realized.Dispose();
        drag.Dispose();
        cache.TotalPinCount.ShouldBe(0);
    }

    [Fact]
    public void Clear_Epoch_Makes_Old_Token_Unable_To_Release_New_Block()
    {
        var cache = new DataGridRangeBlockCache(capacity: 1);
        var block = Block(0);
        var identity = Identity(block);
        cache.TryAdd(SourceIdentity, block).ShouldBeTrue();
        var stalePin = cache.AcquirePin(identity, DataGridRangeBlockPinReason.Applied);

        cache.Clear();
        cache.TryAdd(SourceIdentity, block).ShouldBeTrue();
        using var currentPin = cache.AcquirePin(identity, DataGridRangeBlockPinReason.Applied);
        stalePin.Dispose();

        cache.GetPinCount(identity, DataGridRangeBlockPinReason.Applied).ShouldBe(1);
        cache.Count.ShouldBe(1);
    }

    [Fact]
    public void EvictPassive_Removes_Least_Recently_Used_Unpinned_Block()
    {
        var cache = new DataGridRangeBlockCache(capacity: 3);
        var first = Block(0);
        var second = Block(32);
        var third = Block(64);
        cache.TryAdd(SourceIdentity, first).ShouldBeTrue();
        cache.TryAdd(SourceIdentity, second).ShouldBeTrue();
        cache.TryAdd(SourceIdentity, third).ShouldBeTrue();
        cache.TryGetBlock(Identity(first), out _).ShouldBeTrue();

        cache.EvictPassive().ShouldBeTrue();

        cache.Contains(Identity(second)).ShouldBeFalse();
        cache.Contains(Identity(first)).ShouldBeTrue();
        cache.Contains(Identity(third)).ShouldBeTrue();
    }

    [Fact]
    public void ClearPassive_Preserves_Pinned_Blocks_And_Removes_All_Others()
    {
        var cache = new DataGridRangeBlockCache(capacity: 3);
        var first = Block(0);
        var second = Block(32);
        var third = Block(64);
        cache.TryAdd(SourceIdentity, first).ShouldBeTrue();
        cache.TryAdd(SourceIdentity, second).ShouldBeTrue();
        cache.TryAdd(SourceIdentity, third).ShouldBeTrue();
        using var pin = cache.AcquirePin(
            Identity(second), DataGridRangeBlockPinReason.Applied);

        cache.ClearPassive();

        cache.Count.ShouldBe(1);
        cache.Contains(Identity(first)).ShouldBeFalse();
        cache.Contains(Identity(second)).ShouldBeTrue();
        cache.Contains(Identity(third)).ShouldBeFalse();
        cache.TotalPinCount.ShouldBe(1);
    }

    [Fact]
    public void Warm_TryGetBlock_Performs_Zero_Managed_Allocations()
    {
        var cache = new DataGridRangeBlockCache(capacity: 2);
        var block = Block(0);
        var identity = Identity(block);
        cache.TryAdd(SourceIdentity, block).ShouldBeTrue();
        for (var warmup = 0; warmup < 1_000; warmup++)
        {
            cache.TryGetBlock(identity, out _);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < 10_000; iteration++)
        {
            cache.TryGetBlock(identity, out _);
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        allocated.ShouldBe(0);
    }

    private static DataGridValidatedRangeBlock Block(
        int start,
        string snapshot = "s1",
        long? firstRowKey = null)
    {
        const int total = 96;
        var count = Math.Min(32, total - start);
        var request = new DataGridFetchRequest(
            DataGridQuery.Empty,
            null,
            DataGridGroupExpansion.AllExpanded,
            new DataGridRange(start, 32),
            null,
            1,
            1);
        var entries = new DataGridSourceEntry[count];
        for (var index = 0; index < count; index++)
        {
            var dataIndex = start + index;
            entries[index] = DataGridSourceEntry.CreateData(
                DataGridRowKey.FromInt64((firstRowKey ?? start) + index),
                new Row(dataIndex),
                dataIndex,
                dataIndex);
        }
        var result = new DataGridRangeResult(
            start,
            [.. entries],
            total,
            total,
            total,
            new DataGridSnapshotId(snapshot));
        return DataGridSourceContractValidator.Validate(request, result, Schema, null);
    }

    private static DataGridRangeBlockIdentity Identity(DataGridValidatedRangeBlock block) =>
        block.CreateCacheIdentity(SourceIdentity);

    private sealed record Row(int Value);
}
