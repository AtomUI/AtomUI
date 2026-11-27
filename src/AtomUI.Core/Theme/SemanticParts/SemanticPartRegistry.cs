using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using AtomUI.Registration;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Read-only catalog of the semantic descriptions of all registered controls. For each control it provides the
/// <see cref="ControlSemanticDescriptor" />, which holds the control type, the token identity of the control and
/// its semantic parts. Consumers such as the theme manager and the token resource providers look a control up by
/// its type or by its <see cref="ControlTokenIdentity" />. A registry is either built from an explicit list of
/// descriptors or backed by the startup package index; the latter reads each descriptor from the index on demand,
/// and a successful lookup reports first use to the package that owns the control.
/// </summary>
public sealed class SemanticPartRegistry
{
    private readonly StartupPackageIndex? _startupIndex;
    /// <summary>
    /// A registry without any control. Lookups fail and <see cref="Controls" /> is empty.
    /// </summary>
    public static SemanticPartRegistry Empty { get; } = new(Array.Empty<ControlSemanticDescriptor>());

    private readonly FrozenDictionary<Type, ControlSemanticDescriptor> _controlsByType;
    private readonly FrozenDictionary<ControlTokenIdentity, ControlSemanticDescriptor> _controlsByIdentity;

    internal SemanticPartRegistry(StartupPackageIndex index) : this(Array.Empty<ControlSemanticDescriptor>())
    {
        _startupIndex = index;
        Controls = new StartupSemanticList(index);
    }

    private sealed class StartupSemanticList(StartupPackageIndex index) : IReadOnlyList<ControlSemanticDescriptor>
    {
        public int Count => index.SemanticEntries.Count;
        public ControlSemanticDescriptor this[int slot] => index.GetSemanticAt(slot);
        public IEnumerator<ControlSemanticDescriptor> GetEnumerator()
        {
            for (var slot = 0; slot < Count; slot++)
            {
                yield return this[slot];
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Creates a registry from an explicit list of control descriptors. The list is read once and copied, so later
    /// changes to it are not seen by the registry.
    /// </summary>
    /// <param name="controls">
    /// The descriptors to register. Each control type and each control identity may occur only once.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="controls" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="controls" /> contains a <see langword="null" /> descriptor, or two descriptors that share a
    /// control type or a control identity.
    /// </exception>
    public SemanticPartRegistry(IEnumerable<ControlSemanticDescriptor> controls)
    {
        ArgumentNullException.ThrowIfNull(controls);
        ControlSemanticDescriptor?[] nullableControls = controls.ToArray();
        if (nullableControls.Any(static descriptor => descriptor is null))
        {
            throw new ArgumentException("Semantic control descriptors cannot contain null.", nameof(controls));
        }
        var controlArray = nullableControls
                           .Select(static descriptor => descriptor!)
                           .OrderBy(
                                static descriptor => descriptor.Identity.Catalog,
                                StringComparer.Ordinal)
                           .ThenBy(static descriptor => descriptor.Identity.Id, StringComparer.Ordinal)
                           .ToArray();

        _controlsByType = BuildUniqueMap(
                              controlArray,
                              static descriptor => descriptor.ControlType,
                              "Control type")
                          .ToFrozenDictionary();
        _controlsByIdentity = BuildUniqueMap(
                                  controlArray,
                                  static descriptor => descriptor.Identity,
                                  "Control identity")
                              .ToFrozenDictionary();
        Controls = Array.AsReadOnly(controlArray);
    }

    /// <summary>
    /// Every registered control descriptor. A registry built from a list stores a read-only copy ordered by the
    /// catalog and then by the id of the token identity, both compared ordinally. A registry backed by the startup
    /// package index exposes a view that requests each descriptor from the index when it is accessed.
    /// </summary>
    public IReadOnlyList<ControlSemanticDescriptor> Controls { get; }

    /// <summary>
    /// Looks up the descriptor registered for a control type. The type must match exactly; the descriptor of a base
    /// type is not returned for a derived type.
    /// </summary>
    /// <param name="controlType">
    /// The control type to look up.
    /// </param>
    /// <param name="descriptor">
    /// The descriptor of the control when found; otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a descriptor is registered for the type; otherwise <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="controlType" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The registry is backed by the startup package index, no descriptor is registered for the type, and the type
    /// belongs to a control package that is not enabled.
    /// </exception>
    public bool TryGetControl(
        Type controlType,
        [NotNullWhen(true)] out ControlSemanticDescriptor? descriptor)
    {
        ArgumentNullException.ThrowIfNull(controlType);
        if (_startupIndex is not null)
        {
            return _startupIndex.TryGetSemantic(controlType, out descriptor);
        }
        return _controlsByType.TryGetValue(controlType, out descriptor);
    }

    /// <summary>
    /// Looks up the descriptor registered under a control token identity. The lookup succeeds only when the identity
    /// is registered and agrees with the control type of its descriptor: the identity carries no owner type, or its
    /// owner type is that control type.
    /// </summary>
    /// <param name="identity">
    /// The token identity of the control.
    /// </param>
    /// <param name="descriptor">
    /// The descriptor of the control when found; otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a descriptor is registered for the identity; otherwise <see langword="false" />.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The registry is backed by the startup package index, no descriptor is registered for the identity, and the
    /// identity belongs to a control package that is not enabled.
    /// </exception>
    public bool TryGetControl(
        ControlTokenIdentity identity,
        [NotNullWhen(true)] out ControlSemanticDescriptor? descriptor)
    {
        if (_startupIndex is not null)
        {
            return _startupIndex.TryGetSemantic(identity, out descriptor);
        }
        if (_controlsByIdentity.TryGetValue(identity, out descriptor) &&
            ControlTokenIdentityCanonicalizer.Matches(identity, descriptor.ControlType))
        {
            return true;
        }
        descriptor = null;
        return false;
    }

    internal bool TryGetControlMetadata(ControlTokenIdentity identity,
        [NotNullWhen(true)] out ControlSemanticDescriptor? descriptor) => _startupIndex is not null
        ? _startupIndex.TryGetSemantic(identity, out descriptor, notifyFirstUse: false)
        : TryGetControl(identity, out descriptor);

    private static Dictionary<TKey, ControlSemanticDescriptor> BuildUniqueMap<TKey>(
        IEnumerable<ControlSemanticDescriptor> controls,
        Func<ControlSemanticDescriptor, TKey> keySelector,
        string kind)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, ControlSemanticDescriptor>();
        foreach (var descriptor in controls)
        {
            var key = keySelector(descriptor);
            if (!result.TryAdd(key, descriptor))
            {
                throw new ArgumentException(
                    $"Semantic Part registry contains duplicate {kind} '{key}'.",
                    nameof(controls));
            }
        }
        return result;
    }
}
