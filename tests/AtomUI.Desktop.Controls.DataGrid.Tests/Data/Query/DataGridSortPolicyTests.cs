using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Data.Query;

public class DataGridSortPolicyTests
{
    private static readonly DataGridFieldId Age = new("age");
    private static readonly DataGridFieldId Name = new("name");
    private static readonly DataGridFieldId Score = new("score");

    [Fact]
    public void Normal_Gesture_Replaces_Other_Sorts_And_Cycles_Three_States()
    {
        var query = Query(
            (Name, DataGridSortDirection.Ascending),
            (Score, DataGridSortDirection.Descending));

        query = DataGridSortPolicy.ApplyGesture(query, Age, append: false, DataGridSortDirections.All);
        AssertSorts(query, (Age, DataGridSortDirection.Ascending));

        query = DataGridSortPolicy.ApplyGesture(query, Age, append: false, DataGridSortDirections.All);
        AssertSorts(query, (Age, DataGridSortDirection.Descending));

        query = DataGridSortPolicy.ApplyGesture(query, Age, append: false, DataGridSortDirections.All);
        query.Sorts.ShouldBeEmpty();
    }

    [Fact]
    public void Normal_Gesture_On_Existing_MultiSort_Uses_Its_Current_Direction()
    {
        var query = Query(
            (Age, DataGridSortDirection.Ascending),
            (Name, DataGridSortDirection.Descending));

        var changed = DataGridSortPolicy.ApplyGesture(
            query, Name, append: false, DataGridSortDirections.All);

        changed.Sorts.ShouldBeEmpty();
    }

    [Fact]
    public void Shift_Gesture_Appends_Cycles_In_Place_And_Compresses_Priority()
    {
        var query = Query(
            (Age, DataGridSortDirection.Ascending),
            (Name, DataGridSortDirection.Descending));

        query = DataGridSortPolicy.ApplyGesture(query, Score, append: true, DataGridSortDirections.All);
        AssertSorts(query,
            (Age, DataGridSortDirection.Ascending),
            (Name, DataGridSortDirection.Descending),
            (Score, DataGridSortDirection.Ascending));

        query = DataGridSortPolicy.ApplyGesture(query, Age, append: true, DataGridSortDirections.All);
        AssertSorts(query,
            (Age, DataGridSortDirection.Descending),
            (Name, DataGridSortDirection.Descending),
            (Score, DataGridSortDirection.Ascending));

        query = DataGridSortPolicy.ApplyGesture(query, Name, append: true, DataGridSortDirections.All);
        AssertSorts(query,
            (Age, DataGridSortDirection.Descending),
            (Score, DataGridSortDirection.Ascending));
    }

    [Theory]
    [InlineData(DataGridSortDirections.Ascending, DataGridSortDirection.Ascending)]
    [InlineData(DataGridSortDirections.Descending, DataGridSortDirection.Descending)]
    public void Single_Direction_Gesture_Cycles_Between_None_And_Allowed(
        DataGridSortDirections allowed,
        DataGridSortDirection expected)
    {
        var query = DataGridSortPolicy.ApplyGesture(
            DataGridQuery.Empty, Age, append: false, allowed);
        query.Sorts[0].Direction.ShouldBe(expected);

        DataGridSortPolicy.ApplyGesture(query, Age, append: false, allowed)
                          .Sorts.ShouldBeEmpty();
    }

    [Fact]
    public void Forced_Replace_And_AppendOrReplace_Have_Stable_Order()
    {
        var query = Query(
            (Age, DataGridSortDirection.Ascending),
            (Name, DataGridSortDirection.Ascending));

        var replaced = DataGridSortPolicy.Apply(
            query,
            Name,
            DataGridSortDirection.Descending,
            DataGridSortUpdateMode.Replace,
            DataGridSortDirections.All);
        AssertSorts(replaced, (Name, DataGridSortDirection.Descending));

        var appended = DataGridSortPolicy.Apply(
            query,
            Score,
            DataGridSortDirection.Descending,
            DataGridSortUpdateMode.AppendOrReplace,
            DataGridSortDirections.All);
        AssertSorts(appended,
            (Age, DataGridSortDirection.Ascending),
            (Name, DataGridSortDirection.Ascending),
            (Score, DataGridSortDirection.Descending));

        var updatedInPlace = DataGridSortPolicy.Apply(
            appended,
            Name,
            DataGridSortDirection.Descending,
            DataGridSortUpdateMode.AppendOrReplace,
            DataGridSortDirections.All);
        AssertSorts(updatedInPlace,
            (Age, DataGridSortDirection.Ascending),
            (Name, DataGridSortDirection.Descending),
            (Score, DataGridSortDirection.Descending));
    }

    [Fact]
    public void Null_Forced_Direction_Removes_Only_Requested_Field()
    {
        var query = Query(
            (Age, DataGridSortDirection.Ascending),
            (Name, DataGridSortDirection.Descending),
            (Score, DataGridSortDirection.Ascending));

        var changed = DataGridSortPolicy.Apply(
            query,
            Name,
            null,
            DataGridSortUpdateMode.Replace,
            DataGridSortDirections.All);

        AssertSorts(changed,
            (Age, DataGridSortDirection.Ascending),
            (Score, DataGridSortDirection.Ascending));
    }

    [Fact]
    public void Policy_Rejects_No_Allowed_Direction_And_Unsupported_Request()
    {
        Should.Throw<ArgumentException>(() => DataGridSortPolicy.ApplyGesture(
            DataGridQuery.Empty, Age, append: false, DataGridSortDirections.None));
        Should.Throw<ArgumentException>(() => DataGridSortPolicy.Apply(
            DataGridQuery.Empty,
            Age,
            DataGridSortDirection.Descending,
            DataGridSortUpdateMode.Replace,
            DataGridSortDirections.Ascending));
    }

    [Fact]
    public void NoOp_Returns_Original_Query_Instance()
    {
        var query = Query((Age, DataGridSortDirection.Ascending));

        var result = DataGridSortPolicy.Apply(
            query,
            Age,
            DataGridSortDirection.Ascending,
            DataGridSortUpdateMode.Replace,
            DataGridSortDirections.All);

        ReferenceEquals(query, result).ShouldBeTrue();
    }

    private static DataGridQuery Query(
        params (DataGridFieldId Field, DataGridSortDirection Direction)[] sorts) =>
        DataGridQuery.Empty.WithSorts(
            [.. sorts.Select(sort => new DataGridSort(sort.Field, sort.Direction))]);

    private static void AssertSorts(
        DataGridQuery query,
        params (DataGridFieldId Field, DataGridSortDirection Direction)[] expected)
    {
        query.Sorts.Length.ShouldBe(expected.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            query.Sorts[index].Field.ShouldBe(expected[index].Field);
            query.Sorts[index].Direction.ShouldBe(expected[index].Direction);
        }
    }
}
