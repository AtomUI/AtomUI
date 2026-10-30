using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridViewportPlannerTests
{
    [Fact]
    public void FixedHeight_Jump_To_Last_Viewport_Is_Constant_Time()
    {
        var planner = new DataGridViewportPlanner(rowHeight: 40, overscanViewports: 1);

        var plan = planner.Plan(
            offset: 39_999_960,
            viewportHeight: 400,
            totalEntryCount: 1_000_000);

        plan.VisibleRange.ShouldBe(new DataGridRange(999_999, 1));
        plan.PrefetchRange!.Value.EndExclusive.ShouldBe(1_000_000);
        planner.LastProbeCount.ShouldBeLessThanOrEqualTo(3);
    }

    [Fact]
    public void Prefetch_Adds_One_Viewport_On_Both_Sides_And_Clamps()
    {
        var planner = new DataGridViewportPlanner(rowHeight: 40, overscanViewports: 1);

        var plan = planner.Plan(offset: 400, viewportHeight: 400, totalEntryCount: 100);

        plan.VisibleRange.ShouldBe(new DataGridRange(10, 10));
        plan.PrefetchRange.ShouldBe(new DataGridRange(0, 30));
        plan.DesiredViewport.ShouldBe(new DataGridDesiredViewport(10, 10, 0));
    }

    [Fact]
    public void Sparse_Height_Deltas_Map_Offsets_Without_Scanning_From_Zero()
    {
        var heights = new SparseHeightDeltaIndex(40);
        heights.SetMeasuredHeight(2, 50, DataGridHeightClass.Data);
        heights.SetMeasuredHeight(5, 20, DataGridHeightClass.Data);
        var planner = new DataGridViewportPlanner(40, 0, heights);

        heights.GetOffset(3).ShouldBe(130, tolerance: 0.001);
        heights.GetOffset(6).ShouldBe(230, tolerance: 0.001);
        heights.FindSlotAtOffset(129, 10).ShouldBe(2);
        heights.FindSlotAtOffset(130, 10).ShouldBe(3);
        var plan = planner.Plan(80, 50, 10);

        plan.VisibleRange.ShouldBe(new DataGridRange(2, 1));
        planner.LastProbeCount.ShouldBeLessThanOrEqualTo(heights.TreeHeight + 2);
    }

    [Fact]
    public void RowDetails_Contributes_To_The_Same_Sparse_Entry_Height()
    {
        var heights = new SparseHeightDeltaIndex(40);

        heights.SetMeasuredHeight(
            slot: 1,
            baseHeight: 40,
            detailsHeight: 25,
            DataGridHeightClass.Data);

        heights.GetHeight(1).ShouldBe(65, tolerance: 0.001);
        heights.GetOffset(2).ShouldBe(105, tolerance: 0.001);
    }

    [Fact]
    public void Rebasing_Default_Height_Preserves_Measured_Actual_Heights()
    {
        var heights = new SparseHeightDeltaIndex(40);
        heights.SetMeasuredHeight(2, 60, DataGridHeightClass.Data);
        heights.SetMeasuredHeight(5, 20, DataGridHeightClass.Data);

        heights.RebaseDefaultHeight(50);

        heights.DefaultHeight.ShouldBe(50, tolerance: 0.001);
        heights.MeasuredCount.ShouldBe(2);
        heights.GetHeight(2).ShouldBe(60, tolerance: 0.001);
        heights.GetHeight(5).ShouldBe(20, tolerance: 0.001);
        heights.GetOffset(3).ShouldBe(160, tolerance: 0.001);
        heights.GetExtent(6).ShouldBe(280, tolerance: 0.001);
    }

    [Fact]
    public void Removing_A_Height_Class_Drops_Only_Its_Sparse_Overrides()
    {
        var heights = new SparseHeightDeltaIndex(40);
        heights.SetMeasuredHeight(1, 60, DataGridHeightClass.Data);
        heights.SetMeasuredHeight(2, 70, DataGridHeightClass.GroupHeader(0));

        heights.RemoveHeightClass(DataGridHeightClass.Data).ShouldBe(1);

        heights.MeasuredCount.ShouldBe(1);
        heights.GetHeight(1).ShouldBe(40, tolerance: 0.001);
        heights.GetHeight(2).ShouldBe(70, tolerance: 0.001);
    }

    [Fact]
    public void Eviction_Absorbs_Samples_And_Drops_Per_Slot_State()
    {
        var heights = new SparseHeightDeltaIndex(40);
        var groupClass = DataGridHeightClass.GroupHeader(level: 2);
        heights.SetMeasuredHeight(10, 64, groupClass);

        heights.RemoveRange(0, 32, absorbIntoEstimator: true).ShouldBe(1);

        heights.MeasuredCount.ShouldBe(0);
        heights.EstimatorBucketCount.ShouldBe(1);
        heights.GetEstimatorSampleCount(groupClass).ShouldBe(1);
        heights.GetEstimatedHeight(groupClass).ShouldBe(64, tolerance: 0.001);
    }

    [Fact]
    public void Repeated_Block_Churn_Remains_Bounded()
    {
        var heights = new SparseHeightDeltaIndex(40);
        for (var cycle = 0; cycle < 10_000; cycle++)
        {
            var blockStart = (cycle & 1) * 32;
            for (var offset = 0; offset < 32; offset++)
            {
                heights.SetMeasuredHeight(
                    blockStart + offset,
                    40 + (offset & 3),
                    DataGridHeightClass.Data);
            }
            heights.RemoveRange(blockStart, 32, absorbIntoEstimator: true);
        }

        heights.MeasuredCount.ShouldBe(0);
        heights.EstimatorBucketCount.ShouldBe(1);
    }

    [Fact]
    public void Extent_Is_Always_Finite_And_Empty_Plan_Has_No_Source_Range()
    {
        var heights = new SparseHeightDeltaIndex(double.MaxValue / 2);
        var planner = new DataGridViewportPlanner(
            double.MaxValue / 2,
            overscanViewports: 1,
            heights);

        heights.GetExtent(int.MaxValue).ShouldBe(double.MaxValue);
        var empty = planner.Plan(0, 400, 0);
        empty.VisibleRange.ShouldBeNull();
        empty.PrefetchRange.ShouldBeNull();
        empty.DesiredViewport.VisibleCount.ShouldBe(0);
    }

    [Fact]
    public void Sparse_Index_Matches_A_Naive_Model_Across_Deterministic_Churn()
    {
        const int slotCount = 64;
        const double defaultHeight = 40;
        var random = new Random(455);
        var actual = new Dictionary<int, double>();
        var heights = new SparseHeightDeltaIndex(defaultHeight);

        for (var operation = 0; operation < 1_000; operation++)
        {
            if ((operation % 5) == 0)
            {
                var start = random.Next(slotCount);
                var count = random.Next(1, slotCount - start + 1);
                heights.RemoveRange(start, count, absorbIntoEstimator: false);
                for (var slot = start; slot < start + count; slot++)
                {
                    actual.Remove(slot);
                }
            }
            else
            {
                var slot = random.Next(slotCount);
                var height = random.Next(10, 101);
                actual[slot] = height;
                heights.SetMeasuredHeight(slot, height, DataGridHeightClass.Data);
            }

            heights.MeasuredCount.ShouldBe(actual.Count);
            var expectedOffset = 0d;
            for (var slot = 0; slot < slotCount; slot++)
            {
                heights.GetOffset(slot).ShouldBe(expectedOffset, tolerance: 0.0001);
                var slotHeight = actual.GetValueOrDefault(slot, defaultHeight);
                heights.FindSlotAtOffset(
                           expectedOffset + slotHeight / 2,
                           slotCount)
                       .ShouldBe(slot);
                expectedOffset += slotHeight;
            }
            heights.GetExtent(slotCount).ShouldBe(expectedOffset, tolerance: 0.0001);
            heights.TreeHeight.ShouldBeLessThanOrEqualTo(8);
        }
    }

    [Fact]
    public void Warm_Fixed_Height_Planning_Allocates_Nothing()
    {
        var planner = new DataGridViewportPlanner(40, overscanViewports: 1);
        for (var warmup = 0; warmup < 1_000; warmup++)
        {
            _ = planner.Plan(warmup, 400, 1_000_000);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < 10_000; iteration++)
        {
            _ = planner.Plan(iteration, 400, 1_000_000);
        }

        (GC.GetAllocatedBytesForCurrentThread() - before).ShouldBe(0);
    }
}
