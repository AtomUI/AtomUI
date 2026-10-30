using System.Diagnostics;
using System.Security;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class RegistrationSourceConsumerTests
{
    [Fact]
    public void Analyzer_does_not_export_build_host_runtime_files_as_application_content()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AtomUI.slnx")))
        {
            root = root.Parent;
        }

        using var process = Process.Start(new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root!.FullName,
            ArgumentList = { "msbuild", "src/AtomUI.Generator/AtomUI.Generator.csproj", "-target:GetCopyToOutputDirectoryItems", "-getTargetResult:GetCopyToOutputDirectoryItems" },
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
        })!;
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        process.ExitCode.ShouldBe(0, error);
        using var result = JsonDocument.Parse(output);
        result.RootElement.GetProperty("TargetResults").GetProperty("GetCopyToOutputDirectoryItems").GetProperty("Items").GetArrayLength().ShouldBe(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Source_and_test_projects_receive_only_the_applicable_registration_generator(bool hasGenerator, bool isTestProject)
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AtomUI.slnx")))
        {
            root = root.Parent;
        }

        var directory = Path.Combine(Path.GetTempPath(), "atomui-source-consumer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var repo = SecurityElement.Escape(root!.FullName);
            File.WriteAllText(Path.Combine(directory, "Directory.Build.props"), $"<Project><Import Project=\"{repo}/Directory.Build.props\"/></Project>");
            File.WriteAllText(Path.Combine(directory, "Directory.Build.targets"), $"<Project><Import Project=\"{repo}/Directory.Build.targets\"/></Project>");
            var existing = hasGenerator ? $"<ProjectReference Include=\"{repo}/src/AtomUI.Generator/AtomUI.Generator.csproj\" OutputItemType=\"Analyzer\" ReferenceOutputAssembly=\"false\"/>" : "";
            var project = Path.Combine(directory, "Probe.csproj");
            File.WriteAllText(project, $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><IsTestProject>{isTestProject}</IsTestProject></PropertyGroup><ItemGroup><ProjectReference Include=\"UnrelatedAnalyzer.csproj\" OutputItemType=\"Analyzer\" ReferenceOutputAssembly=\"false\"/>{existing}</ItemGroup></Project>");
            using var process = Process.Start(new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = root.FullName,
                ArgumentList = { "msbuild", project, "-getItem:ProjectReference" },
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
            })!;
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.ExitCode.ShouldBe(0, error);
            using var result = JsonDocument.Parse(output);
            var references = result.RootElement.GetProperty("Items").GetProperty("ProjectReference").EnumerateArray()
                .Select(i => Path.GetFileName(i.GetProperty("Identity").GetString())).ToArray();
            references.Count(n => n == "AtomUI.Generator.csproj").ShouldBe(hasGenerator || !isTestProject ? 1 : 0);
            references.ShouldContain("UnrelatedAnalyzer.csproj");

            if (!hasGenerator && isTestProject)
            {
                // Exercise the real resolver with references already resolved to no AtomUI analyzer.
                // Advertised repository tool paths must not make this project start a missing worker.
                var resolveProject = Path.Combine(directory, "Resolve.proj");
                File.WriteAllText(resolveProject,
                    $"<Project><Import Project=\"{repo}/Directory.Build.props\"/>" +
                    $"<PropertyGroup><AtomUIBuildTasksAssembly>{SecurityElement.Escape(Path.Combine(directory, "missing-worker.dll"))}</AtomUIBuildTasksAssembly></PropertyGroup>" +
                    $"<Import Project=\"{repo}/build/AtomUI.Registration.targets\"/><Target Name=\"ResolveReferences\"/></Project>");
                using var resolve = Process.Start(new ProcessStartInfo("dotnet")
                {
                    WorkingDirectory = root.FullName,
                    ArgumentList = { "msbuild", resolveProject, "-nologo", "-target:AtomUIResolveRegistrationGenerator" },
                    UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
                })!;
                var resolveOutput = resolve.StandardOutput.ReadToEnd();
                var resolveError = resolve.StandardError.ReadToEnd();
                resolve.WaitForExit();
                resolve.ExitCode.ShouldBe(0, resolveOutput + resolveError);
            }
        }
        finally { Directory.Delete(directory, true); }
    }
}
