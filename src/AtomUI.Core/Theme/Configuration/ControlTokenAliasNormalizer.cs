using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Configuration;

internal static class ControlTokenAliasNormalizer
{
    // Called on a single input layer, before precedence merging discards provenance.
    internal static IReadOnlyList<BoundTokenValue> Normalize(
        ControlTokenDescriptor control,
        IReadOnlyList<BoundTokenValue> values,
        string conflictCode,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (control.OwnTokenAliases.Count == 0 || values.Count == 0)
        {
            return values;
        }

        var seen = new Dictionary<TokenDescriptor, BoundTokenValue>();
        var result = new List<BoundTokenValue>(values.Count);
        foreach (var value in values)
        {
            var canonical = control.GetCanonicalOwnToken(value.Descriptor);
            var normalized = ReferenceEquals(canonical, value.Descriptor)
                ? value
                : value with { Descriptor = canonical };
            if (seen.TryGetValue(canonical, out var previous))
            {
                var previousValue = previous with { Descriptor = canonical };
                if (!string.Equals(previousValue.CanonicalValue, normalized.CanonicalValue, StringComparison.Ordinal))
                {
                    var location = value.Location;
                    diagnostics.Add(new ThemeDefinitionDiagnostic(conflictCode,
                        ThemeDefinitionDiagnosticSeverity.Error, location.Source, location.Line, location.Column,
                        location.Path,
                        $"Control '{control.Identity}' has conflicting Own Token names '{previous.Descriptor.Name}' and '{value.Descriptor.Name}' for '{canonical.Name}'."));
                }
                continue;
            }

            seen.Add(canonical, value);
            result.Add(normalized);
        }
        return result;
    }
}
