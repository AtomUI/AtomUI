using System.Reflection;
using AtomUI.Build.Tasks.Isolation;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class RegistrationBuildTasksTests
{
    [Fact]
    public void Identical_generator_copies_are_resolved_once()
    {
        using var fixture = new ToolFixture();
        var result = fixture.Resolve(fixture.Generator, fixture.CopyGenerator());
        result.Success.ShouldBeTrue();
        result.Properties["GeneratorAssembly"].ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Same_filename_with_different_content_fails_instead_of_first_wins()
    {
        using var fixture = new ToolFixture();
        var copy = fixture.CopyGenerator();
        using (var output = File.OpenWrite(copy)) { output.Seek(0, SeekOrigin.End); output.WriteByte(1); }
        var result = fixture.Resolve(fixture.Generator, copy);
        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG005");
    }

    [Fact]
    public void Missing_generator_dependency_fails_instead_of_injecting_partial_bundle()
    {
        using var fixture = new ToolFixture();
        var copy = fixture.CopyGenerator();
        File.Delete(Path.Combine(Path.GetDirectoryName(copy)!, "System.Reflection.Metadata.dll"));
        var result = fixture.Resolve(copy);
        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG006");
    }

    [Fact]
    public void Same_generator_with_different_dependency_content_is_a_conflict()
    {
        using var fixture = new ToolFixture();
        var copy = fixture.CopyGenerator();
        using (var output = File.OpenWrite(Path.Combine(Path.GetDirectoryName(copy)!, "System.Reflection.Metadata.dll")))
        { output.Seek(0, SeekOrigin.End); output.WriteByte(1); }
        fixture.Resolve(fixture.Generator, copy).Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG005");
    }

    private sealed class ToolFixture : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "atomui-registration-tools-" + Guid.NewGuid().ToString("N"));
        internal string Generator { get; }
        internal ToolFixture()
        {
            Generator = typeof(RegistrationBuildTasksTests).Assembly
                .GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "FixtureGeneratorAssembly").Value!;
            File.Exists(Generator).ShouldBeTrue("The test project must build its own configured generator bundle.");
            Directory.CreateDirectory(_directory);
        }
        internal string CopyGenerator()
        {
            foreach (var source in Directory.EnumerateFiles(Path.GetDirectoryName(Generator)!, "*.dll"))
            {
                File.Copy(source, Path.Combine(_directory, Path.GetFileName(source)), true);
            }

            return Path.Combine(_directory, "AtomUI.Generator.dll");
        }
        internal TaskResponse Resolve(params string[] paths) => TaskProcessHost.Execute(new TaskRequest
        {
            TaskName = "ResolveRegistrationToolsTask",
            Items = { ["Candidates"] = paths.Select(p => new TaskWireItem
            { ItemSpec = p, Metadata = new(StringComparer.OrdinalIgnoreCase) { ["Kind"] = "Generator" } }).ToArray() }
        });
        public void Dispose() => Directory.Delete(_directory, true);
    }
}
