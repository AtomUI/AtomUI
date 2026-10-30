using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;

public class DataGridSourceContractTests
{
    [Theory]
    [InlineData(0, 128)]
    [InlineData(31, 128)]
    [InlineData(128, 127)]
    [InlineData(128, 4097)]
    public void Schema_Rejects_Invalid_Range_Size(int preferred, int maximum)
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => SourceFixtures.Schema(preferred, maximum));
    }

    [Fact]
    public void Schema_Rejects_Duplicate_Fields_Operators_And_Unknown_Capabilities()
    {
        var age = new DataGridFieldId("age");
        var equals = new DataGridFilterOperatorSchema(
            new DataGridOperatorId("equals"), 1, 1, DataGridScalarKinds.SignedInteger);

        Should.Throw<ArgumentException>(() => new DataGridSourceSchema(
            typeof(SourceFixtures.Row),
            [SourceFixtures.Field(age), SourceFixtures.Field(age)],
            32,
            128));
        Should.Throw<ArgumentException>(() => new DataGridFieldSchema(
            age,
            typeof(int),
            DataGridSortDirections.All,
            [equals, equals],
            canGroup: false));
        Should.Throw<ArgumentOutOfRangeException>(() => new DataGridFilterOperatorSchema(
            new DataGridOperatorId("invalid"), 2, 1, DataGridScalarKinds.SignedInteger));
        Should.Throw<ArgumentException>(() => new DataGridFilterOperatorSchema(
            new DataGridOperatorId("invalid"), 0, 1, DataGridScalarKinds.None));
        Should.Throw<ArgumentException>(() => new DataGridFieldSchema(
            age,
            typeof(int),
            DataGridSortDirections.None,
            [],
            canGroup: true));
    }

    [Fact]
    public void Schema_Rejects_Display_Accessor_Type_Mismatches()
    {
        var wrongValueAccessor = DataGridFieldDisplayAccessor.Create<SourceFixtures.Row, string>(
            static row => row.Value.ToString());
        Should.Throw<ArgumentException>(() => new DataGridFieldSchema(
            new DataGridFieldId("id"),
            typeof(int),
            DataGridSortDirections.All,
            [],
            canGroup: false,
            displayAccessor: wrongValueAccessor));

        var wrongItemAccessor = DataGridFieldDisplayAccessor.Create<string, int>(
            static value => value.Length);
        var field = new DataGridFieldSchema(
            new DataGridFieldId("id"),
            typeof(int),
            DataGridSortDirections.All,
            [],
            canGroup: false,
            displayAccessor: wrongItemAccessor);
        Should.Throw<ArgumentException>(() => new DataGridSourceSchema(
            typeof(SourceFixtures.Row),
            [field],
            32,
            128));
    }

    [Fact]
    public void Range_And_PageRequest_Use_Checked_NonNegative_Domains()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new DataGridRange(-1, 1));
        Should.Throw<ArgumentOutOfRangeException>(() => new DataGridRange(0, 0));
        Should.Throw<OverflowException>(() => new DataGridRange(int.MaxValue, 2));
        Should.Throw<ArgumentOutOfRangeException>(() => new DataGridPageRequest(-1, 1));
        Should.Throw<ArgumentOutOfRangeException>(() => new DataGridPageRequest(0, 0));
        Should.Throw<OverflowException>(() => new DataGridPageRequest(long.MaxValue, 1));
    }

    [Fact]
    public void GroupExpansion_Is_Sorted_Deduplicated_And_NoOp_Aware()
    {
        var a = new DataGridGroupKey("a");
        var b = new DataGridGroupKey("b");
        var expansion = new DataGridGroupExpansion([b, a, b]);

        expansion.CollapsedGroups.ShouldBe([a, b]);
        ReferenceEquals(expansion, expansion.Collapse(a)).ShouldBeTrue();
        var expanded = expansion.Expand(a);
        expanded.CollapsedGroups.ShouldBe([b]);
        ReferenceEquals(expanded, expanded.Expand(a)).ShouldBeTrue();
    }

    [Fact]
    public void FetchRequest_Rejects_Invalid_Generation_And_Expansion_Without_Groups()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => SourceFixtures.Request(
            queryRevision: -1));
        Should.Throw<ArgumentOutOfRangeException>(() => SourceFixtures.Request(
            dataGeneration: -1));
        Should.Throw<ArgumentException>(() => SourceFixtures.Request(
            expansion: new DataGridGroupExpansion([new DataGridGroupKey("region/eu")])));
    }

    [Fact]
    public void Validator_Accepts_Exact_Full_And_Short_Tail_Results()
    {
        var schema = SourceFixtures.Schema();
        var fullRequest = SourceFixtures.Request(start: 32, count: 32);
        var full = SourceFixtures.Result(start: 32, entryCount: 32, totalEntryCount: 100);
        var firstBlock = DataGridSourceContractValidator.Validate(
            fullRequest, full, schema, expectedIdentity: null);
        firstBlock.Entries.Length.ShouldBe(32);

        var tailRequest = SourceFixtures.Request(
            start: 96,
            count: 32,
            expectedSnapshot: new DataGridSnapshotId("snapshot"));
        var tail = SourceFixtures.Result(start: 96, entryCount: 4, totalEntryCount: 100);
        DataGridSourceContractValidator.Validate(
            tailRequest, tail, schema, firstBlock.Identity).Entries.Length.ShouldBe(4);
    }

    [Fact]
    public void Validator_Rejects_Partial_NonTail_Result()
    {
        var request = SourceFixtures.Request(start: 32, count: 32);
        var result = SourceFixtures.Result(start: 32, entryCount: 4, totalEntryCount: 100);

        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                request, result, SourceFixtures.Schema(), expectedIdentity: null));
    }

    [Fact]
    public void Validator_Rejects_Mismatched_Start_And_Snapshot()
    {
        var startRequest = SourceFixtures.Request(start: 32, count: 32);
        var wrongStart = SourceFixtures.Result(start: 0, entryCount: 32, totalEntryCount: 100);
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                startRequest, wrongStart, SourceFixtures.Schema(), expectedIdentity: null));

        var snapshotRequest = SourceFixtures.Request(
            expectedSnapshot: new DataGridSnapshotId("expected"));
        var wrongSnapshot = SourceFixtures.Result(
            start: 0, entryCount: 32, totalEntryCount: 100, snapshot: "other");
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                snapshotRequest, wrongSnapshot, SourceFixtures.Schema(), expectedIdentity: null));
    }

    [Fact]
    public void Validator_Rejects_Changed_Totals_For_The_Same_Result_Identity()
    {
        var schema = SourceFixtures.Schema();
        var first = DataGridSourceContractValidator.Validate(
            SourceFixtures.Request(start: 0, count: 32),
            SourceFixtures.Result(start: 0, entryCount: 32, totalEntryCount: 100),
            schema,
            expectedIdentity: null);

        var changed = SourceFixtures.Result(
            start: 32,
            entryCount: 32,
            totalEntryCount: 101,
            windowDataCount: 101,
            totalDataCount: 101);
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                SourceFixtures.Request(
                    start: 32,
                    count: 32,
                    expectedSnapshot: new DataGridSnapshotId("snapshot")),
                changed,
                schema,
                first.Identity));
    }

    [Fact]
    public void Validator_Rejects_Duplicate_Keys_Invalid_Item_And_NonIncreasing_Indices()
    {
        var duplicate = new DataGridRangeResult(
            0,
            [
                DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(1), new SourceFixtures.Row(1), 0, 0),
                DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(1), new SourceFixtures.Row(2), 1, 1)
            ],
            2,
            2,
            2,
            new DataGridSnapshotId("snapshot"));
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                SourceFixtures.Request(count: 2), duplicate, SourceFixtures.Schema(), null));

        var invalidItem = new DataGridRangeResult(
            0,
            [DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(1), "wrong item", 0, 0)],
            1,
            1,
            1,
            new DataGridSnapshotId("snapshot"));
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                SourceFixtures.Request(count: 1), invalidItem, SourceFixtures.Schema(), null));

        var descending = new DataGridRangeResult(
            0,
            [
                DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(2), new SourceFixtures.Row(2), 1, 1),
                DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(1), new SourceFixtures.Row(1), 0, 0)
            ],
            2,
            2,
            2,
            new DataGridSnapshotId("snapshot"));
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                SourceFixtures.Request(count: 2), descending, SourceFixtures.Schema(), null));
    }

    [Fact]
    public void Validator_Validates_Page_Count_And_Global_Data_Index()
    {
        var request = SourceFixtures.Request(
            count: 3,
            pageRequest: new DataGridPageRequest(10, 3));
        var result = new DataGridRangeResult(
            0,
            [
                DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(10), new SourceFixtures.Row(10), 0, 10),
                DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(11), new SourceFixtures.Row(11), 1, 11),
                DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(12), new SourceFixtures.Row(12), 2, 12)
            ],
            3,
            3,
            100,
            new DataGridSnapshotId("snapshot"));
        DataGridSourceContractValidator.Validate(
            request, result, SourceFixtures.Schema(), null).Entries.Length.ShouldBe(3);

        var wrongWindowCount = new DataGridRangeResult(
            0,
            result.Entries,
            3,
            2,
            100,
            result.Snapshot);
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                request, wrongWindowCount, SourceFixtures.Schema(), null));
    }

    [Fact]
    public void Validator_Accepts_Valid_Group_Payload_And_Rejects_Wrong_Level_Field()
    {
        var region = new DataGridFieldId("region");
        var query = DataGridQuery.Empty.WithGroups(
            [new DataGridGroup(region, DataGridSortDirection.Ascending)]);
        var request = SourceFixtures.Request(query: query, count: 2);
        var validGroup = new DataGridGroupEntry(
            new DataGridGroupKey("region/eu"),
            region,
            DataGridScalar.FromString("eu"),
            0,
            1);
        var valid = new DataGridRangeResult(
            0,
            [
                DataGridSourceEntry.CreateGroupHeader(validGroup),
                DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(1), new SourceFixtures.Row(1), 0, 0)
            ],
            2,
            1,
            1,
            new DataGridSnapshotId("snapshot"));
        DataGridSourceContractValidator.Validate(
            request, valid, SourceFixtures.Schema(), null).Entries.Length.ShouldBe(2);

        var wrongGroup = new DataGridGroupEntry(
            new DataGridGroupKey("age/1"),
            new DataGridFieldId("age"),
            DataGridScalar.FromInt64(1),
            0,
            1);
        var malformed = new DataGridRangeResult(
            0,
            [
                DataGridSourceEntry.CreateGroupHeader(wrongGroup),
                valid.Entries[1]
            ],
            2,
            1,
            1,
            valid.Snapshot);
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.Validate(
                request, malformed, SourceFixtures.Schema(), null));
    }

    [Fact]
    public void Validator_Rejects_Unpaged_Total_Beyond_Presentation_Domain()
    {
        var result = new DataGridRangeResult(
            0,
            [],
            0,
            0,
            (long)int.MaxValue + 1,
            new DataGridSnapshotId("snapshot"));

        Should.Throw<DataGridPresentationLimitExceededException>(() =>
            DataGridSourceContractValidator.Validate(
                SourceFixtures.Request(count: 1), result, SourceFixtures.Schema(), null));
    }

    [Fact]
    public void Request_Query_Is_Validated_Against_Schema_Before_Fetch()
    {
        var unknown = DataGridQuery.Empty.WithSorts(
            [new DataGridSort(new DataGridFieldId("unknown"), DataGridSortDirection.Ascending)]);
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.ValidateRequest(
                SourceFixtures.Request(query: unknown), SourceFixtures.Schema()));

        var descendingAge = DataGridQuery.Empty.WithSorts(
            [new DataGridSort(new DataGridFieldId("ascending-only"), DataGridSortDirection.Descending)]);
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.ValidateRequest(
                SourceFixtures.Request(query: descendingAge), SourceFixtures.Schema()));

        var region = new DataGridFieldId("region");
        var descendingGroup = DataGridQuery.Empty.WithGroups(
            [new DataGridGroup(region, DataGridSortDirection.Descending)]);
        var ascendingGroupSchema = new DataGridSourceSchema(
            typeof(SourceFixtures.Row),
            [SourceFixtures.Field(
                region,
                DataGridSortDirections.Ascending,
                canGroup: true)],
            32,
            128);
        Should.Throw<DataGridSourceContractException>(() =>
            DataGridSourceContractValidator.ValidateRequest(
                SourceFixtures.Request(query: descendingGroup), ascendingGroupSchema));
    }

    private static class SourceFixtures
    {
        public sealed record Row(int Value);

        public static DataGridSourceSchema Schema(int preferred = 32, int maximum = 128) =>
            new(
                typeof(Row),
                [
                    Field(new DataGridFieldId("age")),
                    Field(new DataGridFieldId("region"), canGroup: true),
                    Field(
                        new DataGridFieldId("ascending-only"),
                        DataGridSortDirections.Ascending)
                ],
                preferred,
                maximum);

        public static DataGridFieldSchema Field(
            DataGridFieldId id,
            DataGridSortDirections directions = DataGridSortDirections.All,
            bool canGroup = false) =>
            new(
                id,
                typeof(int),
                directions,
                [new DataGridFilterOperatorSchema(
                    new DataGridOperatorId("equals"),
                    1,
                    1,
                    DataGridScalarKinds.SignedInteger)],
                canGroup);

        public static DataGridFetchRequest Request(
            int start = 0,
            int count = 32,
            DataGridQuery? query = null,
            DataGridPageRequest? pageRequest = null,
            DataGridGroupExpansion? expansion = null,
            DataGridSnapshotId? expectedSnapshot = null,
            long queryRevision = 0,
            long dataGeneration = 0) =>
            new(
                query ?? DataGridQuery.Empty,
                pageRequest,
                expansion ?? DataGridGroupExpansion.AllExpanded,
                new DataGridRange(start, count),
                expectedSnapshot,
                queryRevision,
                dataGeneration);

        public static DataGridRangeResult Result(
            int start,
            int entryCount,
            int totalEntryCount,
            int? windowDataCount = null,
            long? totalDataCount = null,
            string snapshot = "snapshot")
        {
            var entries = new DataGridSourceEntry[entryCount];
            for (var index = 0; index < entryCount; index++)
            {
                var dataIndex = start + index;
                entries[index] = DataGridSourceEntry.CreateData(
                    DataGridRowKey.FromInt64(dataIndex),
                    new Row(dataIndex),
                    dataIndex,
                    dataIndex);
            }
            return new DataGridRangeResult(
                start,
                [.. entries],
                totalEntryCount,
                windowDataCount ?? totalEntryCount,
                totalDataCount ?? totalEntryCount,
                new DataGridSnapshotId(snapshot));
        }
    }
}
