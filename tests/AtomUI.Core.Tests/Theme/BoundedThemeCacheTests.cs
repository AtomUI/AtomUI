using AtomUI.Theme.Compilation;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class BoundedThemeCacheTests
{
    [Fact]
    public void Theme_Cache_Defaults_Match_The_Architecture_Limits()
    {
        var options = new ThemeCacheOptions();

        options.SnapshotEntryLimit.ShouldBe(32);
        options.SnapshotRetainedBytesLimit.ShouldBe(32L * 1024 * 1024);
        options.ControlEntryLimit.ShouldBe(2048);
        options.ControlRetainedBytesLimit.ShouldBe(32L * 1024 * 1024);
    }

    [Fact]
    public async Task Equivalent_Keys_Reuse_The_Same_Value_And_Compile_Once()
    {
        var cache = CreateCache(entryLimit: 4, bytesLimit: 100);
        var attempts = 0;

        var first = await GetOrCreateAsync(cache,
            new CollisionKey("A"),
            _ => ValueTask.FromResult(new CacheValue(++attempts, 10)));
        var second = await GetOrCreateAsync(cache,
            new CollisionKey("A"),
            _ => ValueTask.FromResult(new CacheValue(++attempts, 10)));

        second.ShouldBeSameAs(first);
        attempts.ShouldBe(1);
    }

    [Fact]
    public async Task Hash_Collisions_Still_Compare_The_Full_Structural_Key()
    {
        var cache = CreateCache(entryLimit: 4, bytesLimit: 100);

        var first = await GetOrCreateAsync(cache,
            new CollisionKey("A"),
            _ => ValueTask.FromResult(new CacheValue(1, 10)));
        var second = await GetOrCreateAsync(cache,
            new CollisionKey("B"),
            _ => ValueTask.FromResult(new CacheValue(2, 10)));

        second.ShouldNotBeSameAs(first);
        second.Id.ShouldBe(2);
        cache.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Hits_Update_Lru_Order_And_Entry_Limit_Evicts_The_Oldest_Value()
    {
        var cache = CreateCache(entryLimit: 2, bytesLimit: 100);
        var attempts = new Dictionary<string, int>(StringComparer.Ordinal);

        await Get("A");
        await Get("B");
        await Get("A");
        await Get("C");
        await Get("A");
        await Get("B");

        attempts["A"].ShouldBe(1);
        attempts["B"].ShouldBe(2);
        attempts["C"].ShouldBe(1);

        async Task<CacheValue> Get(string key)
        {
            return await GetOrCreateAsync(cache,
                new CollisionKey(key),
                _ =>
                {
                    attempts.TryGetValue(key, out var count);
                    attempts[key] = count + 1;
                    return ValueTask.FromResult(new CacheValue(attempts[key], 10));
                });
        }
    }

    [Fact]
    public async Task Byte_Limit_Evicts_Lru_And_An_Oversized_Value_Is_Returned_But_Not_Cached()
    {
        var cache = CreateCache(entryLimit: 8, bytesLimit: 10);
        var attempts = 0;

        await GetOrCreateAsync(cache,
            new CollisionKey("A"),
            _ => ValueTask.FromResult(new CacheValue(++attempts, 6)));
        await GetOrCreateAsync(cache,
            new CollisionKey("B"),
            _ => ValueTask.FromResult(new CacheValue(++attempts, 6)));

        cache.Count.ShouldBe(1);
        cache.RetainedBytes.ShouldBe(6);

        var oversizedFirst = await GetOrCreateAsync(cache,
            new CollisionKey("Large"),
            _ => ValueTask.FromResult(new CacheValue(++attempts, 11)));
        var oversizedSecond = await GetOrCreateAsync(cache,
            new CollisionKey("Large"),
            _ => ValueTask.FromResult(new CacheValue(++attempts, 11)));

        oversizedSecond.ShouldNotBeSameAs(oversizedFirst);
        attempts.ShouldBe(4);
        cache.Count.ShouldBe(1);
        cache.RetainedBytes.ShouldBe(6);
    }

    [Fact]
    public async Task Failed_And_Canceled_Production_Is_Never_Cached()
    {
        var cache = CreateCache(entryLimit: 4, bytesLimit: 100);
        var failedAttempts = 0;
        var canceledAttempts = 0;

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await GetOrCreateAsync(cache,
                new CollisionKey("Failure"),
                _ => throw new InvalidOperationException((++failedAttempts).ToString())));
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await GetOrCreateAsync(cache,
                new CollisionKey("Failure"),
                _ => throw new InvalidOperationException((++failedAttempts).ToString())));

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await GetOrCreateAsync(cache,
                new CollisionKey("Canceled"),
                _ => ValueTask.FromCanceled<CacheValue>(CanceledToken(++canceledAttempts))));
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await GetOrCreateAsync(cache,
                new CollisionKey("Canceled"),
                _ => ValueTask.FromCanceled<CacheValue>(CanceledToken(++canceledAttempts))));

        failedAttempts.ShouldBe(2);
        canceledAttempts.ShouldBe(2);
        cache.Count.ShouldBe(0);

        var late = new TaskCompletionSource<CacheValue>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = GetOrCreateAsync(cache, new CollisionKey("Closed"), _ => new ValueTask<CacheValue>(late.Task)).AsTask();
        cache.Dispose();
        cache.InFlightCount.ShouldBe(1);
        late.SetResult(new CacheValue(3, 10));
        await Should.ThrowAsync<OperationCanceledException>(async () => await pending);
        cache.InFlightCount.ShouldBe(0);
        cache.Count.ShouldBe(0);
        await Should.ThrowAsync<ObjectDisposedException>(async () => await GetOrCreateAsync(cache,
            new CollisionKey("New"), _ => ValueTask.FromResult(new CacheValue(4, 10))));
    }

    [Fact]
    public async Task Concurrent_Equivalent_Requests_Share_One_In_Flight_Production()
    {
        var cache = CreateCache(entryLimit: 4, bytesLimit: 100);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;

        var tasks = Enumerable.Range(0, 32)
                              .Select(_ => GetOrCreateAsync(cache,
                                  new CollisionKey("Shared"),
                                  async _ =>
                                  {
                                      Interlocked.Increment(ref attempts);
                                      started.TrySetResult();
                                      await release.Task;
                                      return new CacheValue(1, 10);
                                  }).AsTask())
                              .ToArray();

        await started.Task;
        attempts.ShouldBe(1);
        release.SetResult();
        var values = await Task.WhenAll(tasks);

        values.ShouldAllBe(value => ReferenceEquals(value, values[0]));
        cache.InFlightCount.ShouldBe(0);
    }

    [Fact]
    public async Task Concurrent_Equivalent_Requests_share_one_oversized_production()
    {
        var cache = CreateCache(entryLimit: 4, bytesLimit: 10);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;

        var tasks = Enumerable.Range(0, 32)
                              .Select(_ => GetOrCreateAsync(cache,
                                  new CollisionKey("Oversized"),
                                  async _ =>
                                  {
                                      Interlocked.Increment(ref attempts);
                                      started.TrySetResult();
                                      await release.Task;
                                      return new CacheValue(1, 11);
                                  }).AsTask())
                              .ToArray();

        await started.Task;
        attempts.ShouldBe(1);
        release.SetResult();
        var values = await Task.WhenAll(tasks);

        values.ShouldAllBe(value => ReferenceEquals(value, values[0]));
        attempts.ShouldBe(1);
        cache.InFlightCount.ShouldBe(0);
        cache.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Caller_Cancellation_Only_Cancels_That_Waiter_Not_The_Shared_Production()
    {
        using var cache = new BoundedLruCache<CollisionKey, CacheValue>(4, 100,
            static value => value.RetainedBytes, productionRetainedBytesLimit: 512);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;
        using var cancellation = new CancellationTokenSource();

        var canceledWaiter = cache.GetOrCreateAsync(
            new CollisionKey("Shared"),
            async token =>
            {
                token.IsCancellationRequested.ShouldBeFalse();
                Interlocked.Increment(ref attempts);
                started.SetResult();
                await release.Task;
                return new CacheValue(1, 10);
            },
            cancellation.Token, productionInputRetainedBytesEstimator: static () => 384).AsTask();
        var survivingWaiter = GetOrCreateAsync(cache,
            new CollisionKey("Shared"),
            _ => throw new InvalidOperationException("The shared producer must be reused.")).AsTask();

        try
        {
            await started.Task;
            cancellation.Cancel();
            await Should.ThrowAsync<OperationCanceledException>(async () => await canceledWaiter);
            cache.InFlightCount.ShouldBe(1);
            await Should.ThrowAsync<InvalidOperationException>(async () => await cache.GetOrCreateAsync(
                new CollisionKey("Other"), _ => ValueTask.FromResult(new CacheValue(2, 10)),
                CancellationToken.None, productionInputRetainedBytesEstimator: static () => 384));
        }
        finally
        {
            release.TrySetResult();
        }

        (await survivingWaiter).Id.ShouldBe(1);
        attempts.ShouldBe(1);
        cache.Count.ShouldBe(1);
    }

    private static BoundedLruCache<CollisionKey, CacheValue> CreateCache(
        int entryLimit,
        long bytesLimit)
    {
        return new BoundedLruCache<CollisionKey, CacheValue>(
            entryLimit,
            bytesLimit,
            static value => value.RetainedBytes);
    }

    private static ValueTask<CacheValue> GetOrCreateAsync(
        BoundedLruCache<CollisionKey, CacheValue> cache,
        CollisionKey key,
        Func<CancellationToken, ValueTask<CacheValue>> factory)
    {
        return cache.GetOrCreateAsync(
            key,
            factory,
            TestContext.Current.CancellationToken);
    }

    private static CancellationToken CanceledToken(int _)
    {
        var source = new CancellationTokenSource();
        source.Cancel();
        return source.Token;
    }

    private sealed record CacheValue(int Id, long RetainedBytes);

    private readonly record struct CollisionKey(string Value)
    {
        public override int GetHashCode() => 1;
    }
}
