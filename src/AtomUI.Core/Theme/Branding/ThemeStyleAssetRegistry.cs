using System.Collections.Frozen;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Branding;

internal sealed class ThemeStyleAssetRegistry
{
    internal const ThemeStyleCapabilities SupportedCapabilities = ThemeStyleCapabilities.QualifiedStyles |
        ThemeStyleCapabilities.CompiledTemplates | ThemeStyleCapabilities.RegisteredStaticAssets;
    internal static ThemeStyleAssetRegistry Empty { get; } = new([]);
    private readonly FrozenDictionary<(string Id, string Revision), ThemeStyleAssetRegistration> _registrations;

    private ThemeStyleAssetRegistry(IEnumerable<ThemeStyleAssetRegistration> registrations)
    {
        _registrations = registrations.ToFrozenDictionary(static item => (item.Id, item.Revision));
        Registrations = Array.AsReadOnly(_registrations.Values.OrderBy(static item => item.Id, StringComparer.Ordinal)
            .ThenBy(static item => item.Revision, StringComparer.Ordinal).ToArray());
    }

    internal IReadOnlyList<ThemeStyleAssetRegistration> Registrations { get; }

    internal bool TryGet(string id, string revision, out ThemeStyleAssetRegistration? registration) =>
        _registrations.TryGetValue((id, revision), out registration);

    internal ThemeStyleAssetRegistry Append(IEnumerable<ThemeStyleAssetRegistration> additions,
        ThemeSchemaRegistry registry, SemanticPartRegistry semantics)
    {
        var registrations = _registrations.ToDictionary(static item => item.Key, static item => item.Value);
        var owners = Registrations.GroupBy(static item => item.Id, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
        var changed = false;
        foreach (var registration in additions)
        {
            ArgumentNullException.ThrowIfNull(registration);
            if ((registration.RequiredCapabilities & ~SupportedCapabilities) != 0)
            {
                throw new ThemeSchemaException($"Style asset '{registration.Id}' requires unsupported capabilities.");
            }
            var owner = registration.OwnerIdentity;
            var known = semantics.TryGetControlMetadata(owner, out var semantic) &&
                        semantic.ControlType == registration.ControlType;
            if (!known)
            {
                known = registry.TryGetControl(owner, out var token) && token.ControlType == registration.ControlType;
            }
            if (!known)
            {
                throw new ThemeSchemaException($"Style asset '{registration.Id}' has no enabled owner '{owner}'.");
            }
            if (owners.TryGetValue(registration.Id, out var previousOwner) &&
                (previousOwner.OwnerIdentity != owner || previousOwner.ControlType != registration.ControlType))
            {
                throw new ThemeSchemaException($"Style asset '{registration.Id}' cannot change owner between revisions.");
            }
            if (registrations.TryGetValue((registration.Id, registration.Revision), out var previous))
            {
                if (!ReferenceEquals(previous, registration))
                {
                    throw new ThemeSchemaException($"Style asset '{registration.Id}@{registration.Revision}' is already registered.");
                }
                continue;
            }
            owners[registration.Id] = registration;
            registrations.Add((registration.Id, registration.Revision), registration);
            changed = true;
        }
        return changed ? new ThemeStyleAssetRegistry(registrations.Values) : this;
    }
}
