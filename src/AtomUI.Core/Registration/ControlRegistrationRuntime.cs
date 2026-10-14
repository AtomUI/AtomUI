using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Theme.Resources;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ControlRegistrationRuntime
{
    public const int AbiVersion = 1;
    internal const string TrimmedSwitchName = "AtomUI.Registration.Trimmed";

#if NET10_0_OR_GREATER
    [FeatureSwitchDefinition(TrimmedSwitchName)]
#endif
    public static bool IsTrimmed => AppContext.TryGetSwitch(TrimmedSwitchName, out var trimmed) && trimmed;

    public static IAtomUIBuilder RegisterPackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collect, prepare, complete, ensure: false);

    // Legacy generated ABI: both collectors now contribute to one complete package before publication.
    public static IAtomUIBuilder RegisterPartitionedPackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collectCritical,
        Action<ControlPackageRegistrationBuilder> collectDeferred,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collectCritical, prepare, complete, ensure: false,
            collectRemaining: collectDeferred);

    public static IAtomUIBuilder EnsurePackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collect, prepare, complete, ensure: true);

    // Legacy Ensure ABI uses the same complete collection and idempotency checks.
    public static IAtomUIBuilder EnsurePartitionedPackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collectCritical,
        Action<ControlPackageRegistrationBuilder> collectDeferred,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collectCritical, prepare, complete, ensure: true,
            collectRemaining: collectDeferred);

    public static void CollectFragments(
        ControlPackageRegistrationBuilder builder,
        IReadOnlyDictionary<string, Type> map,
        IReadOnlyList<string> candidateKeys)
    {
        ArgumentNullException.ThrowIfNull(builder);
        try
        {
            ArgumentNullException.ThrowIfNull(map);
            ArgumentNullException.ThrowIfNull(candidateKeys);
            var selected = new HashSet<Type>();
            for (var index = 0; index < candidateKeys.Count; index++)
            {
                if (!map.TryGetValue(candidateKeys[index], out var proxy) || !selected.Add(proxy))
                {
                    continue;
                }
                if (!proxy.IsSealed || !typeof(ControlRegistrationFragmentAttribute).IsAssignableFrom(proxy) || proxy.ContainsGenericParameters)
                {
                    throw new InvalidOperationException($"Invalid control registration proxy '{proxy}'.");
                }
                var fragment = proxy.GetCustomAttribute<ControlRegistrationFragmentAttribute>(inherit: false);
                if (fragment is null || fragment.GetType() != proxy)
                {
                    throw new InvalidOperationException($"Control registration proxy '{proxy}' must have its own fragment Attribute.");
                }
                builder.AddFragment(fragment);
            }
        }
        catch (Exception exception)
        {
            builder.Fail(exception);
            throw;
        }
    }

    private static IAtomUIBuilder Register(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare,
        Action<IAtomUIBuilder>? complete,
        bool ensure,
        Action<ControlPackageRegistrationBuilder>? collectRemaining = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (builder is not AtomUIBuilder root)
        {
            throw new ArgumentException("Generated control packages require the application AtomUI builder.", nameof(builder));
        }
        var state = root.ControlRegistrationState;
        ControlPackageRegistrationBuilder? collection = null;
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
            ArgumentNullException.ThrowIfNull(createProvider);
            ArgumentNullException.ThrowIfNull(collect);
            if (ensure && state.IsStaged(packageId))
            {
                return builder;
            }
            state.Enter(packageId);
            prepare?.Invoke(builder);
            state.ThrowIfUnavailable();
            var provider = createProvider();
            ArgumentNullException.ThrowIfNull(provider);
            collection = new ControlPackageRegistrationBuilder(state.CanDeferFragments);
            collect(collection);
            state.ThrowIfUnavailable();
            if (collectRemaining is not null)
            {
                // A collector can catch a mutation error; never run another callback on a failed collection.
                collection.ThrowIfUnavailable();
                collectRemaining(collection);
                state.ThrowIfUnavailable();
            }
            var package = collection.Build(packageId, provider);
            builder.Theme.AddControlPackage(package);
            complete?.Invoke(builder);
            state.Stage(packageId);
            return builder;
        }
        catch (Exception exception)
        {
            collection?.Fail(exception);
            state.Fail(exception);
            root.ThemeManagerBuilder.DiscardPendingRegistrations();
            throw;
        }
    }
}
