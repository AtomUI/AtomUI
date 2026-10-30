using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

internal static class PresentationTestFixtures
{
    private static readonly DataGridFieldId GroupField = new("group");

    public static DataGridPresentationSnapshot Snapshot(
        int totalEntryCount,
        int windowDataCount,
        long totalDataCount,
        DataGridPageRequest? pageRequest,
        params BlockSpec[] blockSpecs)
    {
        var grouped = blockSpecs.Any(static block =>
            block.Entries.Any(static entry => entry.Kind == DataGridSourceEntryKind.GroupHeader));
        var query = grouped
            ? DataGridQuery.Empty.WithGroups([
                new DataGridGroup(GroupField, DataGridSortDirection.Ascending)])
            : DataGridQuery.Empty;
        var schema = new DataGridSourceSchema(
            typeof(Row),
            [new DataGridFieldSchema(
                GroupField,
                typeof(string),
                DataGridSortDirections.All,
                [],
                canGroup: true)],
            preferredRangeSize: 32,
            maximumRangeSize: 32);
        var source = new NoFetchSource(schema);
        var generation = new DataGridGeneration(
            source,
            schema,
            query,
            pageRequest,
            DataGridGroupExpansion.AllExpanded,
            queryRevision: 1,
            dataGeneration: 1,
            epoch: 1,
            snapshotExpiryRestarts: 0,
            fallback: null,
            cancellationToken: default);
        DataGridSourceResultIdentity? identity = null;
        var blocks = ImmutableArray.CreateBuilder<DataGridValidatedRangeBlock>(blockSpecs.Length);
        foreach (var spec in blockSpecs)
        {
            var request = new DataGridFetchRequest(
                query,
                pageRequest,
                DataGridGroupExpansion.AllExpanded,
                new DataGridRange(spec.StartIndex, spec.RequestCount),
                identity?.Snapshot,
                1,
                1);
            var result = new DataGridRangeResult(
                spec.StartIndex,
                spec.Entries,
                totalEntryCount,
                windowDataCount,
                totalDataCount,
                new DataGridSnapshotId("snapshot"));
            var block = DataGridSourceContractValidator.Validate(
                request, result, schema, identity);
            identity ??= block.Identity;
            blocks.Add(block);
        }

        identity ??= new DataGridSourceResultIdentity(
            new DataGridFetchRequest(
                query,
                pageRequest,
                DataGridGroupExpansion.AllExpanded,
                new DataGridRange(0, 1),
                null,
                1,
                1),
            new DataGridRangeResult(
                0,
                [],
                totalEntryCount,
                windowDataCount,
                totalDataCount,
                new DataGridSnapshotId("snapshot")));
        generation.ResultIdentity = identity;
        var visibleStart = blockSpecs.Length == 0 ? 0 : blockSpecs.Min(static block => block.StartIndex);
        var visibleCount = blockSpecs.Sum(static block => block.Entries.Length);
        return new DataGridPresentationSnapshot(
            generation,
            new DataGridDesiredViewport(visibleStart, visibleCount, 0),
            blocks.MoveToImmutable(),
            ImmutableArray<IDisposable>.Empty,
            identity);
    }

    public static DataGridSourceEntry RowEntry(int windowIndex, long dataIndex, long key) =>
        DataGridSourceEntry.CreateData(
            DataGridRowKey.FromInt64(key),
            new Row(dataIndex),
            windowIndex,
            dataIndex);

    public static DataGridSourceEntry GroupEntry(string key, int level = 0) =>
        DataGridSourceEntry.CreateGroupHeader(new DataGridGroupEntry(
            new DataGridGroupKey(key),
            GroupField,
            DataGridScalar.FromString(key),
            level,
            leafCount: 1));

    internal readonly record struct BlockSpec(
        int StartIndex,
        int RequestCount,
        ImmutableArray<DataGridSourceEntry> Entries);

    internal sealed record Row(long Value);

    private sealed class NoFetchSource(DataGridSourceSchema schema) : IDataGridSource
    {
        public DataGridSourceSchema Schema { get; } = schema;

        public event EventHandler? Invalidated
        {
            add { }
            remove { }
        }

        public ValueTask<DataGridRangeResult> FetchAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Presentation fixtures do not fetch.");
    }
}
