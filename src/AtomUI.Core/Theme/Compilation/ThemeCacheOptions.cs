namespace AtomUI.Theme.Compilation;

internal sealed record ThemeCacheOptions(
    int SnapshotEntryLimit = 32,
    long SnapshotRetainedBytesLimit = 32L * 1024 * 1024,
    int ControlEntryLimit = 2048,
    long ControlRetainedBytesLimit = 32L * 1024 * 1024,
    int ProductionEntryLimit = ThemeCacheOptions.DefaultProductionEntryLimit,
    long ProductionRetainedBytesLimit = ThemeCacheOptions.DefaultProductionRetainedBytesLimit)
{
    internal const int DefaultProductionEntryLimit = 64;
    internal const long DefaultProductionRetainedBytesLimit = 32L * 1024 * 1024;
}
