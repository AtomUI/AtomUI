using System.Collections.Immutable;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Selection;

public class DataGridSelectionStateTests
{
    [Fact]
    public void Constructor_Normalizes_Keys_And_Merges_Intervals()
    {
        var scope = Scope("s1");
        var state = new DataGridSelectionState(
            [Key(3), Key(1), Key(3), Key(2)],
            null,
            [
                new DataGridSelectionInterval(20, 30, scope),
                new DataGridSelectionInterval(10, 15, scope),
                new DataGridSelectionInterval(14, 21, scope)
            ],
            [Key(9), Key(8), Key(9)]);

        state.ExplicitKeys.ShouldBe([Key(1), Key(2), Key(3)]);
        state.IndexIntervals.ShouldBe([
            new DataGridSelectionInterval(10, 30, scope)]);
        state.ExcludedKeys.ShouldBe([Key(8), Key(9)]);
    }

    [Fact]
    public void Membership_Uses_AllMatching_Explicit_Intervals_And_Exclusions()
    {
        var scope = Scope("s1");
        var state = new DataGridSelectionState(
            [Key(1)],
            scope,
            [new DataGridSelectionInterval(10, 20, scope)],
            [Key(12)]);

        state.Contains(Key(1), 1, scope).ShouldBeTrue();
        state.Contains(Key(11), 11, scope).ShouldBeTrue();
        state.Contains(Key(12), 12, scope).ShouldBeFalse();
        state.Contains(Key(99), 99, scope).ShouldBeTrue();
        state.Contains(Key(99), 99, Scope("other")).ShouldBeFalse();
    }

    [Fact]
    public void Transitions_Follow_Query_And_Invalidation_Matrix()
    {
        var oldScope = Scope("s1");
        var nextScope = Scope("s2", oldScope.Source);
        var original = new DataGridSelectionState(
            [Key(1)],
            oldScope,
            [new DataGridSelectionInterval(10, 20, oldScope)],
            [Key(12)]);

        var sorted = original.TransitionTo(
            nextScope,
            DataGridSelectionTransition.SortOrGroup);
        sorted.ExplicitKeys.ShouldBe([Key(1)]);
        sorted.AllMatchingQuery.ShouldBe(nextScope);
        sorted.IndexIntervals.ShouldBeEmpty();
        sorted.ExcludedKeys.ShouldBe([Key(12)]);

        var filtered = original.TransitionTo(
            nextScope,
            DataGridSelectionTransition.Filter);
        filtered.ExplicitKeys.ShouldBe([Key(1)]);
        filtered.AllMatchingQuery.ShouldBeNull();
        filtered.IndexIntervals.ShouldBeEmpty();
        filtered.ExcludedKeys.ShouldBeEmpty();

        var invalidated = original.TransitionTo(
            nextScope,
            DataGridSelectionTransition.Invalidation);
        invalidated.AllMatchingQuery.ShouldBe(nextScope);
        invalidated.IndexIntervals.ShouldBeEmpty();
        invalidated.ExcludedKeys.ShouldBe([Key(12)]);
    }

    [Fact]
    public void Invalidation_Drops_Exclusions_That_Only_Belong_To_Dropped_Intervals()
    {
        var oldScope = Scope("s1");
        var nextScope = Scope("s2", oldScope.Source);
        var original = new DataGridSelectionState(
            [],
            null,
            [new DataGridSelectionInterval(10, 20, oldScope)],
            [Key(12)]);

        var invalidated = original.TransitionTo(
            nextScope,
            DataGridSelectionTransition.Invalidation);

        invalidated.ShouldBeSameAs(DataGridSelectionState.Empty);
        invalidated.ExcludedKeys.ShouldBeEmpty();
    }

    [Fact]
    public void Single_Mode_Allows_Only_One_Explicit_Key()
    {
        var state = new DataGridSelectionState(
            [Key(1), Key(2)],
            null,
            [],
            []);

        var single = state.NormalizeForMode(DataGridSelectionMode.Single);

        single.ExplicitKeys.ShouldBe([Key(1)]);
        single.AllMatchingQuery.ShouldBeNull();
        single.IndexIntervals.ShouldBeEmpty();
        single.ExcludedKeys.ShouldBeEmpty();

        state.NormalizeForMode(DataGridSelectionMode.None)
             .ShouldBeSameAs(DataGridSelectionState.Empty);
    }

    [Fact]
    public void Single_Mode_Deselecting_Previous_Key_Does_Not_Clear_Current_Selection()
    {
        var scope = Scope("s1");
        var selected = DataGridSelectionState.Empty
            .WithKey(Key(1), 0, scope, isSelected: true, single: true)
            .WithKey(Key(2), 1, scope, isSelected: true, single: true);

        var afterPreviousRowDeselected = selected.WithKey(
            Key(1),
            0,
            scope,
            isSelected: false,
            single: true);

        afterPreviousRowDeselected.ShouldBeSameAs(selected);
        afterPreviousRowDeselected.ExplicitKeys.ShouldBe([Key(2)]);

        afterPreviousRowDeselected.WithKey(
                Key(2),
                1,
                scope,
                isSelected: false,
                single: true)
            .ShouldBeSameAs(DataGridSelectionState.Empty);
    }

    [Fact]
    public void Selecting_A_Row_Already_Covered_By_A_Broad_Expression_Does_Not_Grow_Explicit_Keys()
    {
        var scope = Scope("s1");
        var state = new DataGridSelectionState([], scope, [], [Key(12)]);

        var selected = state.WithKey(Key(12), 12, scope, isSelected: true, single: false);

        selected.ExplicitKeys.ShouldBeEmpty();
        selected.ExcludedKeys.ShouldBeEmpty();
        selected.Contains(Key(12), 12, scope).ShouldBeTrue();
    }

    [Fact]
    public void Invalid_Keys_And_Intervals_Are_Rejected()
    {
        Should.Throw<ArgumentException>(() => new DataGridSelectionState(
            [default], null, [], []));
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new DataGridSelectionInterval(-1, 2, Scope("s1")));
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new DataGridSelectionInterval(2, 2, Scope("s1")));
    }

    private static DataGridRowKey Key(long value) => DataGridRowKey.FromInt64(value);

    private static DataGridSelectionScope Scope(
        string snapshot,
        IDataGridSource? source = null) => new(
            source ?? new EmptySource(),
            DataGridQuery.Empty,
            new DataGridSnapshotId(snapshot));

    private sealed class EmptySource : IDataGridSource
    {
        public DataGridSourceSchema Schema { get; } = new(
            typeof(object),
            [new DataGridFieldSchema(
                new DataGridFieldId("id"),
                typeof(long),
                DataGridSortDirections.All,
                ImmutableArray<DataGridFilterOperatorSchema>.Empty,
                false)],
            32,
            32);

        public event EventHandler? Invalidated
        {
            add { }
            remove { }
        }

        public ValueTask<DataGridRangeResult> FetchAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
