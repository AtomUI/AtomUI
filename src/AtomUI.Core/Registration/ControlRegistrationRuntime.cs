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

    // 分区收集（分级启动）：关键集先行，余量以委托暂存在包注册上（见 ControlPackageRegistration
    // .DeferredCollect 注释）。使用独立方法名，保持既有 RegisterPackage 的源码重载解析与二进制兼容。
    public static IAtomUIBuilder RegisterPartitionedPackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collectCritical,
        Action<ControlPackageRegistrationBuilder> collectDeferred,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collectCritical, prepare, complete, ensure: false,
            collectDeferred: collectDeferred);

    public static IAtomUIBuilder EnsurePackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collect, prepare, complete, ensure: true);

    // 分区收集（分级启动）Ensure 变体，独立方法名避免改变既有 EnsurePackage 的重载解析。
    public static IAtomUIBuilder EnsurePartitionedPackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collectCritical,
        Action<ControlPackageRegistrationBuilder> collectDeferred,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collectCritical, prepare, complete, ensure: true,
            collectDeferred: collectDeferred);

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
        Action<ControlPackageRegistrationBuilder>? collectDeferred = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (builder is not AtomUIBuilder root)
        {
            throw new ArgumentException("Generated control packages require the application AtomUI builder.", nameof(builder));
        }
        var state = root.ControlRegistrationState;
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
            var collection = new ControlPackageRegistrationBuilder();
            collect(collection);
            state.ThrowIfUnavailable();
            var package = collection.Build(packageId, provider, collectDeferred);
            builder.Theme.AddControlPackage(package);
            complete?.Invoke(builder);
            state.Stage(packageId);
            return builder;
        }
        catch (Exception exception)
        {
            state.Fail(exception);
            throw;
        }
    }
}
