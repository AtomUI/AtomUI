using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Theme.Resources;

namespace AtomUI.Registration;

/// <summary>
/// Runtime side of the build-generated control registration: the ABI version, the trimming switch, the
/// lookup of registration fragments and the entry points that generated package code calls.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ControlRegistrationRuntime
{
    /// <summary>
    /// The version of the control registration ABI that this build of AtomUI implements. A package marker
    /// generated for another version is rejected when it is constructed.
    /// </summary>
    public const int AbiVersion = 1;
    internal const string TrimmedSwitchName = "AtomUI.Registration.Trimmed";

/// <summary>
/// Whether the application was published trimmed, read from the feature switch
/// <c>AtomUI.Registration.Trimmed</c>. Generated startup code and the startup configuration read it to
/// choose the registration path of a trimmed publish.
/// </summary>
#if NET10_0_OR_GREATER
    [FeatureSwitchDefinition(TrimmedSwitchName)]
#endif
    public static bool IsTrimmed => AppContext.TryGetSwitch(TrimmedSwitchName, out var trimmed) && trimmed;

    /// <summary>
    /// Returns the registration fragment of the proxy type that <paramref name="map" /> stores under
    /// <paramref name="key" />, or <see langword="null" /> when the map has no entry for the key.
    /// </summary>
    /// <param name="map">
    /// Selection map from fragment key to proxy type.
    /// </param>
    /// <param name="key">
    /// The fragment key to look up.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The mapped type is not a sealed, non-generic subclass of
    /// <see cref="ControlRegistrationFragmentAttribute" /> that carries an instance of itself as its attribute.
    /// </exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ControlRegistrationFragmentAttribute? ResolveFragment(IReadOnlyDictionary<string, Type> map, string key)
    {
        if (!map.TryGetValue(key, out var proxy))
        {
            return null;
        }
        if (!proxy.IsSealed || !typeof(ControlRegistrationFragmentAttribute).IsAssignableFrom(proxy) || proxy.ContainsGenericParameters)
        {
            throw new InvalidOperationException($"Invalid control registration proxy '{proxy}'.");
        }
        var fragment = proxy.GetCustomAttribute<ControlRegistrationFragmentAttribute>(false);
        if (fragment is null || fragment.GetType() != proxy)
        {
            throw new InvalidOperationException($"Control registration proxy '{proxy}' has no matching fragment Attribute.");
        }
        return fragment;
    }

    /// <summary>
    /// Registers one control package with the application and returns <paramref name="builder" />.
    /// </summary>
    /// <param name="builder">
    /// The application builder. Any other <see cref="IAtomUIBuilder" /> throws an <see cref="ArgumentException" />.
    /// </param>
    /// <param name="packageId">
    /// Identifier of the package; must not be null, empty or white space.
    /// </param>
    /// <param name="createProvider">
    /// Creates the control themes provider of the package.
    /// </param>
    /// <param name="collect">
    /// Fills the registration of the package through a <see cref="ControlPackageRegistrationBuilder" />.
    /// </param>
    /// <param name="prepare">
    /// Optional callback that runs before the package is collected.
    /// </param>
    /// <param name="complete">
    /// Optional callback that runs after the package was added to the theme manager. The package counts as
    /// registered only after it returned.
    /// </param>
    /// <remarks>
    /// A package that is already registered or being registered throws an
    /// <see cref="InvalidOperationException" />; use <see cref="EnsurePackage" /> to skip such a package.
    /// When any step throws, the registration state of the builder stays failed, so it cannot register another
    /// package, and every pending theme registration is discarded before the exception is rethrown.
    /// </remarks>
    public static IAtomUIBuilder RegisterPackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collect, prepare, complete, ensure: false);

    /// <summary>
    /// Registers one control package like <see cref="RegisterPackage" /> does, or returns the builder unchanged
    /// when the package is already registered.
    /// </summary>
    public static IAtomUIBuilder EnsurePackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collect, prepare, complete, ensure: true);

    /// <summary>
    /// Adds to <paramref name="builder" /> the fragments of the proxies that <paramref name="map" /> stores
    /// under the keys of <paramref name="candidateKeys" />, in list order.
    /// </summary>
    /// <param name="builder">
    /// The package collection that receives the fragments.
    /// </param>
    /// <param name="map">
    /// Selection map from fragment key to proxy type.
    /// </param>
    /// <param name="candidateKeys">
    /// The fragment keys to look up.
    /// </param>
    /// <remarks>
    /// Keys missing from the map are skipped, and each proxy is added only once. When a proxy is invalid or
    /// adding a fragment throws, the builder is failed for good and the exception is rethrown.
    /// </remarks>
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
        bool ensure)
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
            collection = new ControlPackageRegistrationBuilder(deferFragments: true);
            collect(collection);
            state.ThrowIfUnavailable();
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
