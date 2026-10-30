using System.Collections.Immutable;
using Shouldly;
using Xunit;
using static AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization.PresentationTestFixtures;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridPresentationIndexTests
{
    [Fact]
    public void Empty_Presentation_Has_No_Slots_Or_Children()
    {
        using var snapshot = Snapshot(0, 0, 0, null);
        var index = new DataGridPresentationIndex(snapshot, new SparseHeightDeltaIndex(40));

        index.SlotCount.ShouldBe(0);
        index.ChildCount.ShouldBe(0);
        index.CachedEntryCount.ShouldBe(0);
        Should.Throw<ArgumentOutOfRangeException>(() => index.GetEntry(0));
    }

    [Fact]
    public void One_Row_And_Short_Tail_Map_Without_Total_Sized_Storage()
    {
        using var snapshot = Snapshot(
            1_000_000,
            1_000_000,
            1_000_000,
            null,
            new BlockSpec(999_992, 8, [
                RowEntry(999_992, 999_992, 999_992),
                RowEntry(999_993, 999_993, 999_993),
                RowEntry(999_994, 999_994, 999_994),
                RowEntry(999_995, 999_995, 999_995),
                RowEntry(999_996, 999_996, 999_996),
                RowEntry(999_997, 999_997, 999_997),
                RowEntry(999_998, 999_998, 999_998),
                RowEntry(999_999, 999_999, 999_999)]));
        var index = new DataGridPresentationIndex(snapshot, new SparseHeightDeltaIndex(40));

        index.SlotCount.ShouldBe(1_000_000);
        index.CachedEntryCount.ShouldBe(8);
        index.GetEntry(999_999).DataIndex.ShouldBe(999_999);
        index.FindSlot(DataGridRowKey.FromInt64(999_999)).ShouldBe(999_999);
    }

    [Fact]
    public void Group_Header_Consumes_A_Slot_But_Not_A_Row_Child_Index()
    {
        using var snapshot = Snapshot(
            3,
            2,
            2,
            null,
            new BlockSpec(0, 3, [
                GroupEntry("north"),
                RowEntry(0, 0, 10),
                RowEntry(1, 1, 11)]));
        var index = new DataGridPresentationIndex(snapshot, new SparseHeightDeltaIndex(40));

        index.TryGetRowChildIndex(0, out _).ShouldBeFalse();
        index.TryGetRowChildIndex(1, out var firstChild).ShouldBeTrue();
        firstChild.ShouldBe(0);
        index.TryGetRowChildIndex(2, out var secondChild).ShouldBeTrue();
        secondChild.ShouldBe(1);
        index.ChildCount.ShouldBe(2);
        index.FindSlot(new DataGridGroupKey("north")).ShouldBe(0);
    }

    [Fact]
    public void Missing_Committed_Block_Is_An_Invariant_Failure_Not_A_Null_Row()
    {
        using var snapshot = Snapshot(
            64,
            64,
            64,
            null,
            new BlockSpec(0, 32, Enumerable.Range(0, 32)
                .Select(index => RowEntry(index, index, index))
                .ToImmutableArray()));
        var index = new DataGridPresentationIndex(snapshot, new SparseHeightDeltaIndex(40));

        Should.Throw<DataGridPresentationInvariantException>(() => index.GetEntry(32));
    }

    [Fact]
    public void Paged_Window_Preserves_Long_Data_Index_And_Int_Child_Index()
    {
        var pageStart = (long)int.MaxValue + 100;
        using var snapshot = Snapshot(
            2,
            2,
            pageStart + 2,
            new DataGridPageRequest(pageStart, 2),
            new BlockSpec(0, 2, [
                RowEntry(0, pageStart, 1),
                RowEntry(1, pageStart + 1, 2)]));
        var index = new DataGridPresentationIndex(snapshot, new SparseHeightDeltaIndex(40));

        index.GetEntry(0).DataIndex.ShouldBe(pageStart);
        index.GetEntry(1).WindowDataIndex.ShouldBe(1);
        index.ChildCount.ShouldBe(2);
    }

    [Fact]
    public void First_Complete_Anchor_And_Group_Anchor_Restore_By_Identity()
    {
        using var snapshot = Snapshot(
            3,
            2,
            2,
            null,
            new BlockSpec(0, 3, [
                GroupEntry("north"),
                RowEntry(0, 0, 10),
                RowEntry(1, 1, 11)]));
        var heights = new SparseHeightDeltaIndex(40);
        var index = new DataGridPresentationIndex(snapshot, heights);

        var firstComplete = index.CaptureFirstCompleteAnchor(5);
        firstComplete.FallbackSlot.ShouldBe(1);
        index.RestoreAnchor(firstComplete).ShouldBe(5, tolerance: 0.001);

        var groupAnchor = index.CaptureAnchorAtSlot(0, viewportOffset: 12);
        groupAnchor.GroupKey.ShouldBe(new DataGridGroupKey("north"));
        index.RestoreAnchor(groupAnchor).ShouldBe(12, tolerance: 0.001);
    }

    [Fact]
    public void Warm_Committed_Entry_Lookup_Allocates_Nothing()
    {
        using var snapshot = Snapshot(
            32,
            32,
            32,
            null,
            new BlockSpec(0, 32, Enumerable.Range(0, 32)
                .Select(index => RowEntry(index, index, index))
                .ToImmutableArray()));
        var index = new DataGridPresentationIndex(snapshot, new SparseHeightDeltaIndex(40));
        for (var warmup = 0; warmup < 1_000; warmup++)
        {
            _ = index.GetEntry(warmup & 31);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < 10_000; iteration++)
        {
            _ = index.GetEntry(iteration & 31);
        }

        (GC.GetAllocatedBytesForCurrentThread() - before).ShouldBe(0);
    }
}
