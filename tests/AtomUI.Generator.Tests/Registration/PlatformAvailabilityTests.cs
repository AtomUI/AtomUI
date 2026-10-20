using System.Reflection;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Registration.TypeMapRegistrationGeneratorTests;

namespace AtomUI.Generator.Tests.Registration;

public class PlatformAvailabilityTests
{
    [Theory]
    [InlineData("windows", "6.1", false)]
    [InlineData("windows", "6.2", true)]
    [InlineData("windows", "9.9", true)]
    [InlineData("windows", "10.0", false)]
    [InlineData("macos", "15.0", false)]
    [InlineData("browser", "0.0", false)]
    public void Version_Window_Guard_Preserves_Both_Boundaries(string os, string version, bool expected)
    {
        var diagnostics = new List<Diagnostic>();
        var availability = PlatformAvailability.Create(["windows6.2"], ["windows10.0"], Location.None, diagnostics.Add);
        diagnostics.ShouldBeEmpty();
        Evaluate(availability, os, version).ShouldBe(expected);
    }

    [Theory]
    [InlineData("ios", "12.0", false)]
    [InlineData("ios", "13.0", true)]
    [InlineData("maccatalyst", "12.0", false)]
    [InlineData("maccatalyst", "13.0", true)]
    [InlineData("maccatalyst", "14.0", false)]
    [InlineData("macos", "14.0", false)]
    public void IOS_Predicate_Overlap_And_Catalyst_Restriction_Agree_With_The_Emitted_Guard(string os, string version, bool expected)
    {
        var availability = PlatformAvailability.Create(["ios13.0"], ["maccatalyst14.0"], Location.None, d => throw new Exception(d.ToString()));
        Evaluate(availability, os, version).ShouldBe(expected);
        var catalyst = PlatformAvailability.Create(["maccatalyst13.0"], ["maccatalyst14.0"], Location.None, d => throw new Exception(d.ToString()));
        availability.Covers(catalyst).ShouldBeTrue();
        catalyst.Covers(availability).ShouldBeFalse();
    }

    [Theory]
    [InlineData("windows", "6.2", false)]
    [InlineData("windows", "7.0", true)]
    [InlineData("windows", "10.0", false)]
    [InlineData("linux", "7.0", false)]
    public void Type_Containing_Type_And_Assembly_Annotations_Intersect(string os, string version, bool expected)
    {
        var compilation = Compilation("""
            [assembly: System.Runtime.Versioning.UnsupportedOSPlatform("windows10.0")]
            namespace Demo;
            [System.Runtime.Versioning.SupportedOSPlatform("windows6.2")]
            public class Outer {
                [System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
                public class Inner : Avalonia.Controls.Control { }
            }
            """, []);
        var availability = PlatformAvailability.FromType(compilation.GetTypeByMetadataName("Demo.Outer+Inner")!, d => throw new Exception(d.ToString()));
        Evaluate(availability, os, version).ShouldBe(expected);
    }

    [Fact]
    public void Equivalent_Versions_And_OS_Aliases_Have_Equal_Domains()
    {
        var first = PlatformAvailability.Create(["osx12.0", "windows6.2"], ["windows10.0"], Location.None, _ => { });
        var second = PlatformAvailability.Create(["windows6.2.0.0", "macos12.0.0"], ["windows10.0.0.0"], Location.None, _ => { });
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    private static bool Evaluate(PlatformAvailability availability, string os, string version)
        => EvaluateGuards(availability.Guards(), os, version);

    internal static bool EvaluateGuards(IEnumerable<string> guards, string os, string version = "0.0")
    {
        // Execute the emitted guard against the documented BCL predicate behavior on multiple
        // platform/version inputs. No assertion depends on this test machine's OS.
        var expression = string.Join(" || ", guards.Select(g => "(" + g + ")"));
        if (expression.Length == 0) expression = "false";
        expression = expression.Replace("global::System.OperatingSystem.", "ProbeOS.");
        var platformMethods = new[] { "Android", "Browser", "FreeBSD", "IOS", "Linux", "MacCatalyst", "MacOS", "TvOS", "Windows" }
            .Select(name => $"public static bool Is{name}() => Platform == \"{name.ToLowerInvariant()}\"" +
                (name == "IOS" ? " || Platform == \"maccatalyst\"" : "") + ";\n" +
                $"public static bool Is{name}VersionAtLeast(int major, int minor, int build, int revision = 0) => Is{name}() && Version >= new System.Version(major, minor, build, revision);");
        var code = "public static class ProbeOS { public static string Platform = \"\"; public static System.Version Version = new(0,0,0,0); " +
            string.Join("\n", platformMethods) + " public static bool Run(string os, string version) { Platform = os; var v = System.Version.Parse(version); Version = new(v.Major,v.Minor,System.Math.Max(0,v.Build),System.Math.Max(0,v.Revision)); return !(" + expression + "); } }";
        var compilation = Compilation(code, [], "PlatformProbe" + Guid.NewGuid().ToString("N"));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        return (bool)Assembly.Load(stream.ToArray()).GetType("ProbeOS")!.GetMethod("Run")!.Invoke(null, [os, version])!;
    }
}
