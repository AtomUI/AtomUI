using System.ComponentModel;
using System.Collections.Frozen;
using Avalonia.Controls;

namespace AtomUI.Registration;

/// <summary>
/// Groups the theme resource registrations of a package. Registrations in the
/// <see cref="Shared" /> phase are ordered before those in the <see cref="Control" /> phase.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public enum ControlResourcePhase
{
    /// <summary>
    /// Resources that are not tied to a single control. No registration written by the source generator uses
    /// this phase.
    /// </summary>
    Shared = 0,
    /// <summary>
    /// Phase of the resources of one control's theme asset. The source generator writes every registration
    /// with this phase.
    /// </summary>
    Control = 1
}

/// <summary>
/// Links a theme asset to the factory that creates its resource provider, and positions the asset in the
/// resource order of its package through <see cref="Phase" /> and <see cref="Order" />.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlThemeResourceRegistration
{
    /// <summary>
    /// Creates a registration with an open resource set: the asset is consulted for every resource key.
    /// </summary>
    /// <param name="assetId">
    /// Identifier of the theme asset; must not be null, empty or white space.
    /// </param>
    /// <param name="phase">
    /// The phase of the registration. Any value other than <see cref="ControlResourcePhase.Shared" /> or
    /// <see cref="ControlResourcePhase.Control" /> throws an <see cref="ArgumentOutOfRangeException" />.
    /// </param>
    /// <param name="order">
    /// The position of the registration within its phase; lower values come first.
    /// </param>
    /// <param name="factory">
    /// Creates the resource provider of the asset; must not be null.
    /// </param>
    public ControlThemeResourceRegistration(string assetId, ControlResourcePhase phase, int order, Func<IResourceProvider> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetId);
        ArgumentNullException.ThrowIfNull(factory);
        if (phase is not (ControlResourcePhase.Shared or ControlResourcePhase.Control))
        {
            throw new ArgumentOutOfRangeException(nameof(phase));
        }
        AssetId = assetId;
        Phase = phase;
        Order = order;
        Factory = factory;
    }

    /// <summary>
    /// The identifier of the theme asset whose resources the factory provides. It must equal the
    /// <see cref="AtomUI.Theme.Schema.ControlThemeAssetDescriptor.AssetId" /> of the asset.
    /// </summary>
    public string AssetId { get; }
    /// <summary>
    /// The phase of the registration. Registrations are ordered by phase first, then by
    /// <see cref="Order" />, then by <see cref="AssetId" />.
    /// </summary>
    public ControlResourcePhase Phase { get; }
    /// <summary>
    /// The position of the registration within its <see cref="Phase" />; lower values come first.
    /// </summary>
    public int Order { get; }
    /// <summary>
    /// Creates the resource provider of the asset. It runs once, when a resource of the asset is first looked
    /// up or validated, and returning null throws an <see cref="InvalidOperationException" />.
    /// </summary>
    public Func<IResourceProvider> Factory { get; }

    // A null value means an open resource set: the asset is consulted for every key, so no resource key may
    // be excluded because of it.
    internal IReadOnlySet<object>? ResourceKeys { get; private init; }

    /// <summary>
    /// Creates a registration for an asset whose resource keys are completely known when it is generated.
    /// </summary>
    /// <remarks>
    /// <paramref name="resourceKeys" /> must cover the top-level keys of every variant and include of the
    /// asset. Lookups for keys outside the set skip the asset, so use the constructor when the factory may add
    /// keys dynamically. The list is copied, so later changes to it have no effect.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ControlThemeResourceRegistration CreateGenerated(
        string assetId, ControlResourcePhase phase, int order,
        Func<IResourceProvider> factory, IReadOnlyList<object> resourceKeys)
    {
        ArgumentNullException.ThrowIfNull(resourceKeys);
        if (resourceKeys.Any(static key => key is null))
        {
            throw new ArgumentException("Generated resource keys cannot contain null.", nameof(resourceKeys));
        }
        return new ControlThemeResourceRegistration(assetId, phase, order, factory)
        {
            ResourceKeys = resourceKeys.ToFrozenSet()
        };
    }

    internal static int CompareOrder(ControlThemeResourceRegistration left, ControlThemeResourceRegistration right)
    {
        var result = left.Phase.CompareTo(right.Phase);
        if (result == 0)
        {
            result = left.Order.CompareTo(right.Order);
        }
        return result == 0 ? StringComparer.Ordinal.Compare(left.AssetId, right.AssetId) : result;
    }

    internal bool HasSameMetadata(ControlThemeResourceRegistration other) =>
        ReferenceEquals(this, other) || (AssetId == other.AssetId && Phase == other.Phase && Order == other.Order && Factory.Equals(other.Factory) &&
        (ResourceKeys is null ? other.ResourceKeys is null : other.ResourceKeys is not null && ResourceKeys.SetEquals(other.ResourceKeys)));
}
