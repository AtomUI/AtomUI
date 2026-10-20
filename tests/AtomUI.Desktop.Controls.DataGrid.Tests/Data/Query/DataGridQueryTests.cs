using System.Collections.Immutable;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Data.Query;

public class DataGridQueryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" age")]
    [InlineData("age ")]
    [InlineData("age\n")]
    public void FieldId_Rejects_NonCanonical_Values(string? value)
    {
        Should.Throw<ArgumentException>(() => new DataGridFieldId(value!));
    }

    [Fact]
    public void String_Identities_Are_Ordinal_And_Default_Is_Invalid()
    {
        new DataGridFieldId("age").ShouldNotBe(new DataGridFieldId("Age"));
        new DataGridOperatorId("equals").ShouldNotBe(new DataGridOperatorId("Equals"));
        new DataGridGroupKey("region/eu").ShouldNotBe(new DataGridGroupKey("region/EU"));
        new DataGridSnapshotId("revision-1").ShouldNotBe(new DataGridSnapshotId("Revision-1"));

        default(DataGridFieldId).IsValid.ShouldBeFalse();
        default(DataGridOperatorId).IsValid.ShouldBeFalse();
        default(DataGridGroupKey).IsValid.ShouldBeFalse();
        default(DataGridSnapshotId).IsValid.ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => _ = default(DataGridFieldId).Value);
        Should.Throw<InvalidOperationException>(() => _ = default(DataGridOperatorId).Value);
        Should.Throw<InvalidOperationException>(() => _ = default(DataGridGroupKey).Value);
        Should.Throw<InvalidOperationException>(() => _ = default(DataGridSnapshotId).Value);
    }

    [Fact]
    public void RowKey_Is_A_Tagged_Union_And_Default_Is_Invalid()
    {
        default(DataGridRowKey).IsValid.ShouldBeFalse();
        DataGridRowKey.FromString("42").ShouldNotBe(DataGridRowKey.FromInt64(42));
        DataGridRowKey.FromInt64(42).ShouldNotBe(DataGridRowKey.FromUInt64(42));
        DataGridRowKey.FromGuid(Guid.Empty).IsValid.ShouldBeTrue();
        Should.Throw<ArgumentException>(() => DataGridRowKey.FromString(""));
    }

    [Fact]
    public void Scalar_Normalizes_Negative_Zero_And_DateTimeOffset()
    {
        DataGridScalar.FromDouble(-0d).ShouldBe(DataGridScalar.FromDouble(0d));
        DataGridScalar.FromDateTimeOffset(
                new DateTimeOffset(2026, 9, 5, 8, 0, 0, TimeSpan.FromHours(8)))
            .ShouldBe(DataGridScalar.FromDateTimeOffset(
                new DateTimeOffset(2026, 9, 5, 0, 0, 0, TimeSpan.Zero)));
        Should.Throw<ArgumentOutOfRangeException>(() => DataGridScalar.FromDouble(double.NaN));
        Should.Throw<ArgumentOutOfRangeException>(() => DataGridScalar.FromDouble(double.PositiveInfinity));
        Should.Throw<ArgumentOutOfRangeException>(() => DataGridScalar.FromDouble(double.NegativeInfinity));
    }

    [Fact]
    public void Scalar_Preserves_Kind_And_Uses_Ordinal_String_Equality()
    {
        DataGridScalar.Null.Kind.ShouldBe(DataGridScalarKind.Null);
        DataGridScalar.FromInt64(1).ShouldNotBe(DataGridScalar.FromUInt64(1));
        DataGridScalar.FromString("value").ShouldNotBe(DataGridScalar.FromString("Value"));
        DataGridScalar.FromBoolean(true).CompareTo(DataGridScalar.FromBoolean(false)).ShouldBeGreaterThan(0);
        DataGridScalar.FromString("a").CompareTo(DataGridScalar.FromString("b")).ShouldBeLessThan(0);
        Should.Throw<ArgumentNullException>(() => DataGridScalar.FromString(null!));
    }

    [Fact]
    public void Structurally_Equal_Query_Is_Equal_And_With_Is_NoOp()
    {
        var age = new DataGridFieldId("age");
        var left = DataGridQuery.Empty.WithSorts(
            [new DataGridSort(age, DataGridSortDirection.Ascending)]);
        var right = new DataGridQuery(left.Sorts, default, default);

        left.ShouldBe(right);
        left.GetHashCode().ShouldBe(right.GetHashCode());
        ReferenceEquals(left, left.WithSorts(left.Sorts)).ShouldBeTrue();
        right.Filters.ShouldBe(ImmutableArray<DataGridFilter>.Empty);
        right.Groups.ShouldBe(ImmutableArray<DataGridGroup>.Empty);
    }

    [Fact]
    public void Query_Rejects_Duplicate_And_GroupSort_Overlap()
    {
        var age = new DataGridFieldId("age");
        var name = new DataGridFieldId("name");
        var equals = new DataGridOperatorId("equals");

        Should.Throw<ArgumentException>(() => new DataGridQuery(
            [new(age, DataGridSortDirection.Ascending),
             new(age, DataGridSortDirection.Descending)], [], []));
        Should.Throw<ArgumentException>(() => new DataGridQuery(
            [new(age, DataGridSortDirection.Ascending)], [],
            [new(age, DataGridSortDirection.Ascending)]));
        Should.Throw<ArgumentException>(() => new DataGridQuery([],
            [new(name, equals, [DataGridScalar.FromString("a")]),
             new(name, equals, [DataGridScalar.FromString("b")])], []));
        Should.Throw<ArgumentException>(() => new DataGridQuery([], [],
            [new(name, DataGridSortDirection.Ascending),
             new(name, DataGridSortDirection.Descending)]));
    }

    [Fact]
    public void Query_Validates_Default_Identity_And_Direction()
    {
        Should.Throw<ArgumentException>(() => new DataGridSort(
            default, DataGridSortDirection.Ascending));
        Should.Throw<ArgumentOutOfRangeException>(() => new DataGridSort(
            new DataGridFieldId("age"), (DataGridSortDirection)99));
        Should.Throw<ArgumentException>(() => new DataGridFilter(
            new DataGridFieldId("age"), default, []));
        Should.Throw<ArgumentException>(() => new DataGridGroup(
            default, DataGridSortDirection.Ascending));
    }

    [Fact]
    public void Query_With_Methods_Only_Replace_The_Requested_Component()
    {
        var age = new DataGridFieldId("age");
        var region = new DataGridFieldId("region");
        var equals = new DataGridOperatorId("equals");
        var query = new DataGridQuery(
            [new(age, DataGridSortDirection.Ascending)],
            [new(region, equals, [DataGridScalar.FromString("eu")])],
            []);

        var changed = query.WithSorts([new(age, DataGridSortDirection.Descending)]);

        changed.Sorts[0].Direction.ShouldBe(DataGridSortDirection.Descending);
        changed.Filters.ShouldBe(query.Filters);
        changed.Groups.ShouldBe(query.Groups);
        ReferenceEquals(query, changed).ShouldBeFalse();
    }
}
