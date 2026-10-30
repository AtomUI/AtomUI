using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace AtomUI.Media;

/// <summary>Shares a platform FontManager collection across independently owned application scopes.</summary>
internal static class FontCollectionLifetime
{
    private static readonly ConditionalWeakTable<FontManager, CollectionSet> Collections = new();

    internal static IDisposable Acquire(FontManager manager, Uri key, Func<IFontCollection> factory)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);
        var set = Collections.GetValue(manager, static _ => new CollectionSet());
        lock (set)
        {
            EnsureRegistered(manager, set, key, factory);
            set.References[key] = checked(set.References[key] + 1);
            return new Lease(set, key);
        }
    }

    internal static void RegisterPersistent(FontManager manager, Uri key, Func<IFontCollection> factory)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);
        var set = Collections.GetValue(manager, static _ => new CollectionSet());
        lock (set)
        {
            EnsureRegistered(manager, set, key, factory);
        }
    }

    private static void EnsureRegistered(FontManager manager, CollectionSet set, Uri key, Func<IFontCollection> factory)
    {
        if (set.References.ContainsKey(key))
        {
            return;
        }
        var collection = factory() ?? throw new InvalidOperationException("The font collection factory returned null.");
        try
        {
            if (collection.Key != key)
            {
                throw new InvalidOperationException("The font collection factory returned a different registration key.");
            }
            // FontManager is process-scoped and owns collection disposal. Its public removal
            // API accepts only a URI, so a scope cannot safely remove its exact instance:
            // another host may have replaced the entry in the meantime.
            manager.AddFontCollection(collection);
            set.References.Add(key, 0);
        }
        catch (Exception error)
        {
            try
            {
                collection.Dispose();
            }
            catch (Exception cleanupError)
            {
                throw new AggregateException("Font collection registration and cleanup failed.", error, cleanupError);
            }
            throw;
        }
    }

    private sealed class CollectionSet
    {
        internal Dictionary<Uri, int> References { get; } = new();
    }

    private sealed class Lease(CollectionSet set, Uri key) : IDisposable
    {
        private CollectionSet? _set = set;

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _set, null);
            if (owner is null)
            {
                return;
            }
            lock (owner)
            {
                owner.References[key]--;
            }
        }
    }
}
