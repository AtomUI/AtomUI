using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

// A bounded semantic model for the supported .NET platform annotation syntax. Domains are
// intervals [minimum, maximum) for disjoint runtime OS families, plus all other platforms.
// In particular IsIOS also matches MacCatalyst; normalize that overlap before any comparison.
internal sealed class PlatformAvailability : IEquatable<PlatformAvailability>
{
    private static readonly string[] Platforms = { "android", "browser", "freebsd", "ios", "linux", "maccatalyst", "macos", "tvos", "windows", "other" };
    private static readonly Version Zero = new(0, 0, 0, 0);
    private sealed record Range(Version Minimum, Version? Maximum)
    {
        internal static readonly Range All = new(Zero, null);
        internal bool IsEmpty => Maximum is not null && Minimum >= Maximum;
        internal Range Intersect(Range other) => new(Minimum > other.Minimum ? Minimum : other.Minimum,
            Maximum is null ? other.Maximum : other.Maximum is null || Maximum < other.Maximum ? Maximum : other.Maximum);
    }
    private readonly ValueArray<Range> _ranges;
    private PlatformAvailability(IEnumerable<Range> ranges) => _ranges = new(ranges);
    internal static readonly PlatformAvailability All = new(Platforms.Select(_ => Range.All));
    internal static readonly PlatformAvailability Empty = new(Platforms.Select(_ => new Range(Zero, Zero)));
    internal bool IsEmpty => _ranges.All(r => r.IsEmpty);
    internal PlatformAvailability Intersect(PlatformAvailability other) => ReferenceEquals(this, All) ? other : ReferenceEquals(other, All) ? this
        : new(_ranges.Select((r, i) => r.Intersect(other._ranges[i])));
    internal bool Covers(PlatformAvailability other) => Intersect(other).Equals(other);
    public bool Equals(PlatformAvailability? other) => other is not null && _ranges.Select((r, i) =>
        r.IsEmpty && other._ranges[i].IsEmpty || r == other._ranges[i]).All(v => v);
    public override bool Equals(object? obj) => obj is PlatformAvailability other && Equals(other);
    public override int GetHashCode() => new ValueArray<Range>(_ranges.Select(r => r.IsEmpty ? new Range(Zero, Zero) : r)).GetHashCode();

    internal static PlatformAvailability FromType(INamedTypeSymbol type, Action<Diagnostic> report)
    {
        var result = All;
        for (var current = type; current is not null; current = current.ContainingType)
        {
            result = result.Intersect(FromAttributes(current.GetAttributes(), type.Locations.FirstOrDefault() ?? Location.None, report));
        }

        result = result.Intersect(FromAssembly(type.ContainingAssembly, report));
        if (result.IsEmpty)
        {
            report(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationUnsupportedBackend, type.Locations.FirstOrDefault(),
                "type platform declarations have no common supported domain; child annotations may narrow parent availability, not re-enable an excluded platform"));
        }

        return result;
    }

    internal static PlatformAvailability FromAssembly(IAssemblySymbol assembly, Action<Diagnostic> report)
        => FromAttributes(assembly.GetAttributes(), assembly.Locations.FirstOrDefault() ?? Location.None, report);

    private static PlatformAvailability FromAttributes(IEnumerable<AttributeData> attributes, Location location, Action<Diagnostic> report)
    {
        var supported = new List<string>();
        var unsupported = new List<string>();
        foreach (var attribute in attributes)
        {
            var name = attribute.AttributeClass?.ToDisplayString();
            if (name is not ("System.Runtime.Versioning.SupportedOSPlatformAttribute" or "System.Runtime.Versioning.UnsupportedOSPlatformAttribute"))
            {
                continue;
            }

            if (attribute.ConstructorArguments.FirstOrDefault().Value is not string value)
            {
                continue;
            }

            switch (name)
            {
                case "System.Runtime.Versioning.SupportedOSPlatformAttribute": supported.Add(value); break;
                case "System.Runtime.Versioning.UnsupportedOSPlatformAttribute": unsupported.Add(value); break;
            }
        }
        return Create(supported, unsupported, location, report);
    }

    internal static PlatformAvailability Create(IEnumerable<string> supported, IEnumerable<string> unsupported, Location location, Action<Diagnostic> report)
    {
        var allow = Parse(supported).ToArray();
        var deny = Parse(unsupported).ToArray();
        if (allow.Length == 0 && deny.Length == 0)
        {
            return All;
        }

        if (allow.Any(min => deny.Any(max => max.OS == min.OS && max.Version <= min.Version)))
        {
            report(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationUnsupportedBackend, location,
                "platform re-enablement after an unsupported range requires an explicit non-overlapping availability declaration"));
        }

        var ranges = Platforms.Select(platform =>
        {
            var included = allow.Where(p => Matches(p.OS, platform)).Select(p => p.Version).ToArray();
            var excluded = deny.Where(p => Matches(p.OS, platform)).Select(p => p.Version).ToArray();
            return allow.Length > 0 && included.Length == 0 ? new Range(Zero, Zero)
                : new Range(included.Length == 0 ? Zero : included.Min()!, excluded.Length == 0 ? null : excluded.Min());
        });
        return new(ranges);

        IEnumerable<(string OS, Version Version)> Parse(IEnumerable<string> values)
        {
            foreach (var raw in values.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                var value = raw.Trim();
                var os = new string(value.TakeWhile(char.IsLetter).ToArray()).ToLowerInvariant();
                var versionText = value.Substring(os.Length);
                if (os == "osx")
                {
                    os = "macos";
                }

                var version = Zero;
                if (!Platforms.Contains(os) || os == "other" || versionText.Length != 0 &&
                    (!Version.TryParse(versionText, out version) || os is "browser" or "linux" || os != "windows" && version.Revision >= 0))
                {
                    report(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationUnsupportedBackend, location, value));
                    continue;
                }
                yield return (os, new(version.Major, Math.Max(0, version.Minor), Math.Max(0, version.Build), Math.Max(0, version.Revision)));
            }
        }
    }

    private static bool Matches(string annotationOS, string runtimeOS) => annotationOS == runtimeOS || annotationOS == "ios" && runtimeOS == "maccatalyst";
    private static string OS(string platform) => "global::System.OperatingSystem.Is" + (platform switch
    {
        "android" => "Android", "browser" => "Browser", "freebsd" => "FreeBSD", "ios" => "IOS", "linux" => "Linux",
        "maccatalyst" => "MacCatalyst", "macos" => "MacOS", "tvos" => "TvOS", "windows" => "Windows", _ => throw new InvalidOperationException()
    });
    private static string AtLeast(string platform, Version version) => OS(platform) + "VersionAtLeast(" +
        version.Major + ", " + version.Minor + ", " + version.Build + (platform == "windows" ? ", " + version.Revision : "") + ")";
    private static string IsOS(string platform) => platform == "ios"
        ? "(" + OS(platform) + "() && !" + OS("maccatalyst") + "())" : OS(platform) + "()";

    internal IEnumerable<string> Guards()
    {
        if (Equals(All))
        {
            yield break;
        }

        var otherAllowed = !_ranges[Platforms.Length - 1].IsEmpty;
        var conditions = new List<string>();
        for (var i = 0; i < Platforms.Length - 1; i++)
        {
            var platform = Platforms[i];
            var range = _ranges[i];
            if (otherAllowed)
            {
                if (range.IsEmpty)
                {
                    conditions.Add(IsOS(platform));
                }
                else
                {
                    if (range.Minimum > Zero)
                    {
                        conditions.Add("(" + IsOS(platform) + " && !" + AtLeast(platform, range.Minimum) + ")");
                    }

                    if (range.Maximum is not null)
                    {
                        conditions.Add("(" + IsOS(platform) + " && " + AtLeast(platform, range.Maximum) + ")");
                    }
                }
            }
            else if (!range.IsEmpty)
            {
                var condition = range.Minimum == Zero ? IsOS(platform) : AtLeast(platform, range.Minimum);
                if (platform == "ios" && range.Minimum > Zero)
                {
                    condition = "(" + condition + " && !" + OS("maccatalyst") + "())";
                }

                if (range.Maximum is not null)
                {
                    condition = "(" + condition + " && !" + AtLeast(platform, range.Maximum) + ")";
                }

                conditions.Add(condition);
            }
        }
        yield return conditions.Count == 0 ? "true" : otherAllowed ? string.Join(" || ", conditions) : "!(" + string.Join(" || ", conditions) + ")";
    }
}
