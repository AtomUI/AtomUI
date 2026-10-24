using System.Diagnostics;
using AtomUI.Build.Tasks.Isolation;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class TypeMapPublishGateTests : IClassFixture<ManagedNativeCompilerFixture>
{
    private readonly ManagedNativeCompilerFixture _compilerFixture;

    public TypeMapPublishGateTests(ManagedNativeCompilerFixture compilerFixture)
    {
        _compilerFixture = compilerFixture;
    }

    [Fact]
    public void Incorrect_actual_linker_binary_is_rejected()
    {
        var result = TaskProcessHost.Execute(new TaskRequest
        {
            TaskName = "ValidateRegistrationToolchainTask",
            Properties =
            {
                ["LinkerAssembly"] = typeof(TaskProcessHost).Assembly.Location,
                ["TargetFrameworkIdentifier"] = ".NETCoreApp",
                ["TargetFrameworkVersion"] = "v10.0"
            }
        });
        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG006");
    }

    [Theory]
    [InlineData("10.0.0", "v10.0", true)]
    [InlineData("10.0.9-servicing.26270.113+901ca941248413c79832d2fdbd709da0c4386353", "v10.0", true)]
    [InlineData("11.0.0-preview.3.26201.1+0123456789abcdef", "v11.0", true)]
    [InlineData("9.0.11", "v10.0", false)]
    [InlineData("10.0.8-servicing.26229.119+94ea82652cdd4e0f8046b5bd5becbd11461482ca", "v11.0", false)]
    [InlineData("not-a-version", "v10.0", false)]
    public void Desktop_native_compiler_matches_target_and_typemap_introduction_version(string reportedVersion, string targetVersion, bool accepted)
    {
        var directory = Directory.CreateTempSubdirectory("atomui-fake-ilc-");
        try
        {
            var compiler = _compilerFixture.CreateCompiler(directory.FullName, reportedVersion);

            var result = TaskProcessHost.Execute(new TaskRequest
            {
                TaskName = "ValidateRegistrationToolchainTask",
                Properties =
                {
                    ["NativeCompiler"] = compiler,
                    ["TargetFrameworkIdentifier"] = ".NETCoreApp",
                    ["TargetFrameworkVersion"] = targetVersion
                }
            });

            result.Success.ShouldBe(accepted);
            if (!accepted)
            {
                result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG006");
            }
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.0", "10.0", true)]
    [InlineData("illink, Version=11.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "11.0.0-preview.3.26201.1+01234567", "11.0", true)]
    [InlineData("illink, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "9.0.11", "10.0", false)]
    [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef", "10.0.9-servicing.26270.113+901ca941", "10.0", false)]
    [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "11.0.0-preview.3.26201.1+01234567", "11.0", false)]
    public void Desktop_linker_requires_official_identity_matching_target_and_typemap_version(
        string identity, string informationalVersion, string targetVersion, bool accepted)
    {
        TypeMapBuildContract.IsSupportedDesktopLinker(identity, informationalVersion, Version.Parse(targetVersion)).ShouldBe(accepted);
    }

    [Fact]
    public void Missing_receipt_is_a_publish_error_even_with_no_reachable_slots()
    {
        var result = TaskProcessHost.Execute(new TaskRequest
        {
            TaskName = "VerifyTypeMapReceiptTask",
            Properties = { ["ReceiptPath"] = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"), ["SignaturePath"] = "missing" }
        });
        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG007");
    }

    [Fact]
    public void Browser_publish_does_not_gate_typemap_support_on_sdk_version_strings()
    {
        foreach (var sdk in new[] { "10.0.100", "10.0.401", "11.0.100", "12.0.100" })
        {
            var accepted = TaskProcessHost.Execute(new TaskRequest
            {
                TaskName = "PrepareTypeMapLinkTask",
                Properties = { ["SdkVersion"] = sdk, ["ReceiptPath"] = "relative.json" }
            });
            accepted.Success.ShouldBeFalse();
            accepted.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG006" && d.Message.Contains("absolute"));
        }
    }
}

// One native apphost per class keeps all six existing version cases executable on every OS.
public sealed class ManagedNativeCompilerFixture : IDisposable
{
    private readonly DirectoryInfo _directory;
    private readonly string _outputDirectory;

    public ManagedNativeCompilerFixture()
    {
        _directory = Directory.CreateTempSubdirectory("atomui-test-native-compiler-");
        _outputDirectory = Path.Combine(_directory.FullName, "output");
        try
        {
            foreach (var name in new[] { "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props" })
            {
                File.WriteAllText(Path.Combine(_directory.FullName, name), "<Project />");
            }
            File.WriteAllText(Path.Combine(_directory.FullName, "NuGet.Config"),
                "<configuration><packageSources><clear /></packageSources></configuration>");
            var project = Path.Combine(_directory.FullName, "ilc.csproj");
            File.WriteAllText(project, """
                <Project Sdk="Microsoft.NET.Sdk">
                    <PropertyGroup>
                        <TargetFramework>net10.0</TargetFramework>
                        <OutputType>Exe</OutputType>
                        <UseAppHost>true</UseAppHost>
                        <IsPackable>false</IsPackable>
                    </PropertyGroup>
                </Project>
                """);
            File.WriteAllText(Path.Combine(_directory.FullName, "Program.cs"), """
                if (args.Length != 1 || args[0] != "--version")
                {
                    return 2;
                }
                System.Console.WriteLine(System.IO.File.ReadAllText(
                    System.IO.Path.Combine(System.AppContext.BaseDirectory, "version.txt")));
                return 0;
                """);

            var repository = new DirectoryInfo(AppContext.BaseDirectory);
            while (!File.Exists(Path.Combine(repository.FullName, "AtomUI.slnx")))
            {
                repository = repository.Parent ?? throw new InvalidOperationException("Cannot locate the repository SDK configuration.");
            }
            using var build = Process.Start(new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = repository.FullName,
                ArgumentList =
                {
                    "build", project, "--output", _outputDirectory, "--nologo", "--verbosity", "quiet",
                    "--disable-build-servers", "-p:NuGetAudit=false"
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }) ?? throw new InvalidOperationException("Cannot start the compiler fixture build.");
            var output = build.StandardOutput.ReadToEndAsync();
            var errors = build.StandardError.ReadToEndAsync();
            if (!build.WaitForExit(60_000))
            {
                build.Kill(entireProcessTree: true);
                build.WaitForExit();
                throw new TimeoutException("Compiler fixture build exceeded 60 seconds.");
            }
            var buildOutput = output.GetAwaiter().GetResult();
            var buildErrors = errors.GetAwaiter().GetResult();
            if (build.ExitCode != 0)
            {
                throw new InvalidOperationException($"Compiler fixture build failed: {buildOutput} {buildErrors}");
            }
        }
        catch
        {
            _directory.Delete(recursive: true);
            throw;
        }
    }

    public string CreateCompiler(string directory, string reportedVersion)
    {
        foreach (var file in Directory.EnumerateFiles(_outputDirectory))
        {
            var target = Path.Combine(directory, Path.GetFileName(file));
            File.Copy(file, target);
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(target, File.GetUnixFileMode(file));
            }
        }
        File.WriteAllText(Path.Combine(directory, "version.txt"), reportedVersion);
        return Path.Combine(directory, OperatingSystem.IsWindows() ? "ilc.exe" : "ilc");
    }

    public void Dispose()
    {
        _directory.Delete(recursive: true);
    }
}
