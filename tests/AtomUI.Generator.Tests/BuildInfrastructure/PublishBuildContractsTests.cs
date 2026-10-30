using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.BuildInfrastructure;

public sealed class PublishBuildContractsTests
{
    private static readonly string[] PropertyNames =
    [
        "IsTrimmable", "IsAotCompatible", "EnableTrimAnalyzer", "EnableAotAnalyzer",
        "EnableSingleFileAnalyzer", "PublishAot"
    ];

    private static readonly (string Argument, string Library)[] HomebrewLibraries =
    [
        ("-L/opt/homebrew/lib", "/opt/homebrew/lib/libbrotlienc.dylib"),
        ("-L/opt/homebrew/opt/openssl@3/lib", "/opt/homebrew/opt/openssl@3/lib/libssl.dylib"),
        ("-L/usr/local/lib", "/usr/local/lib/libbrotlienc.dylib"),
        ("-L/usr/local/opt/openssl@3/lib", "/usr/local/opt/openssl@3/lib/libssl.dylib")
    ];

    [Theory]
    [InlineData("AtomUI.Probe", "net10.0", "Library", false, false, true)]
    [InlineData("AtomUI.Probe", "net10.0-browser", "Library", false, false, true)]
    [InlineData("AtomUI.Probe", "net8.0", "Library", false, false, true)]
    [InlineData("AtomUI.Probe", "netstandard2.0", "Library", false, false, false)]
    [InlineData("Other.Probe", "net10.0", "Library", false, false, false)]
    [InlineData("AtomUI.Probe", "net10.0", "Exe", false, false, false)]
    [InlineData("AtomUI.Probe", "net10.0", "WinExe", false, false, false)]
    [InlineData("AtomUI.Probe.Tests", "net10.0", "Library", true, false, false)]
    [InlineData("AtomUI.Probe", "net10.0", "Library", false, true, false)]
    public async Task Compatibility_defaults_apply_only_to_eligible_runtime_libraries(
        string name, string framework, string output, bool test, bool roslyn, bool compatible)
    {
        using var fixture = new EvaluationFixture();
        var project = fixture.CreateSdkProject(name, new()
        {
            ["TargetFramework"] = framework, ["OutputType"] = output,
            ["IsTestProject"] = Boolean(test), ["IsRoslynComponent"] = Boolean(roslyn)
        });
        var result = await fixture.Evaluate(project);
        Enabled(result, "IsTrimmable").ShouldBe(compatible);
        Enabled(result, "IsAotCompatible").ShouldBe(compatible);
        // These are final SDK values, after both repository compatibility defaults and SDK
        // analyzer inference. Compatibility alone must not turn on publish-only analyzers.
        AssertAnalyzers(result, false, false, false);
    }

    [Theory]
    [InlineData("AtomUI.Probe", false, false)]
    [InlineData("AtomUI.Probe", false, true)]
    [InlineData("AtomUI.Probe", true, false)]
    [InlineData("AtomUI.Probe", true, true)]
    [InlineData("Other.Probe", false, false)]
    [InlineData("Other.Probe", true, true)]
    public async Task Authored_compatibility_overrides_survive_repository_and_sdk_defaults(
        string name, bool trim, bool aot)
    {
        using var fixture = new EvaluationFixture();
        var result = await fixture.Evaluate(fixture.CreateSdkProject(name, new()
        {
            ["IsTrimmable"] = Boolean(trim), ["IsAotCompatible"] = Boolean(aot)
        }));
        Property(result, "IsTrimmable").ShouldBe(Boolean(trim));
        Property(result, "IsAotCompatible").ShouldBe(Boolean(aot));
        AssertAnalyzers(result, false, false, false);
    }

    [Theory]
    [InlineData(false, false, false, false, false, false, false)]
    [InlineData(true, false, false, false, true, false, false)]
    [InlineData(false, true, false, false, true, true, false)]
    [InlineData(false, false, true, false, true, true, false)]
    [InlineData(false, false, false, true, false, false, true)]
    [InlineData(true, false, false, true, true, false, true)]
    [InlineData(false, true, false, true, true, true, true)]
    [InlineData(false, false, true, true, true, true, true)]
    [InlineData(true, false, true, false, true, true, false)]
    [InlineData(true, true, false, true, true, true, true)]
    public async Task Publish_modes_enable_only_their_matching_analyzers_after_sdk_evaluation(
        bool trimmed, bool nativeAot, bool browserAot, bool singleFile,
        bool trimAnalyzer, bool aotAnalyzer, bool singleFileAnalyzer)
    {
        using var fixture = new EvaluationFixture();
        var result = await fixture.Evaluate(fixture.CreateSdkProject("AtomUI.Probe", new()
        {
            ["TargetFramework"] = browserAot ? "net10.0-browser" : "net10.0",
            ["PublishTrimmed"] = Boolean(trimmed), ["PublishAot"] = Boolean(nativeAot),
            ["RunAOTCompilation"] = Boolean(browserAot), ["PublishSingleFile"] = Boolean(singleFile)
        }));
        AssertAnalyzers(result, trimAnalyzer, aotAnalyzer, singleFileAnalyzer);
    }

    [Theory]
    [InlineData("EnableTrimAnalyzer", "PublishTrimmed", false, false, false)]
    [InlineData("EnableAotAnalyzer", "PublishAot", true, false, false)]
    [InlineData("EnableSingleFileAnalyzer", "PublishSingleFile", false, false, false)]
    public async Task Authored_analyzer_opt_out_overrides_its_publish_mode(
        string analyzer, string mode, bool trim, bool aot, bool singleFile)
    {
        using var fixture = new EvaluationFixture();
        var result = await fixture.Evaluate(fixture.CreateSdkProject("AtomUI.Probe", new()
        {
            [mode] = "true", [analyzer] = "false"
        }));
        AssertAnalyzers(result, trim, aot, singleFile);
    }

    [Theory]
    [InlineData("EnableTrimAnalyzer", true, false, false)]
    [InlineData("EnableAotAnalyzer", false, true, false)]
    [InlineData("EnableSingleFileAnalyzer", false, false, true)]
    public async Task Authored_analyzer_opt_in_survives_ordinary_build_defaults(
        string analyzer, bool trim, bool aot, bool singleFile)
    {
        using var fixture = new EvaluationFixture();
        var result = await fixture.Evaluate(fixture.CreateSdkProject("AtomUI.Probe", new() { [analyzer] = "true" }));
        AssertAnalyzers(result, trim, aot, singleFile);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Homebrew_linker_arguments_require_native_aot_macos_and_the_corresponding_library(bool publishAot)
    {
        using var fixture = new EvaluationFixture();
        var project = fixture.WriteProject("Native.proj", new XElement("Project",
            new XElement("PropertyGroup", new XElement("PublishAot", Boolean(publishAot))),
            new XElement("ItemGroup", new XElement("LinkerArg", new XAttribute("Include", "existing-argument"))),
            new XElement("Import", new XAttribute("Project", fixture.HomebrewTarget))));
        var result = await fixture.Evaluate(project);
        var arguments = result.GetProperty("Items").GetProperty("LinkerArg").EnumerateArray().ToArray();
        arguments.Count(item => item.GetProperty("Identity").GetString() == "existing-argument").ShouldBe(1);
        AssertHomebrewArguments(result, fixture.HomebrewTarget, publishAot);
    }

    [Fact]
    public void Unavailable_Homebrew_layouts_still_declare_their_specific_library_guards()
    {
        var target = XDocument.Load(Path.Combine(RepositoryRoot(), "build/MacOSHomebrewNativeAot.targets"));
        // An evaluation on this OS cannot exercise the other host branch of IsOSPlatform.
        // Check that guard only at this unavailable-platform boundary.
        foreach (var group in target.Descendants("LinkerArg").Select(item => item.Parent!).Distinct())
        {
            var condition = ((string?)group.Attribute("Condition")).ShouldNotBeNull();
            condition.ShouldContain("$([MSBuild]::IsOSPlatform('OSX'))");
            if (!OperatingSystem.IsMacOS()) condition.ShouldContain("'$(PublishAot)' == 'true'");
        }
        foreach (var (argument, library) in HomebrewLibraries)
        {
            // Do not create system-library sentinels or pretend this host executes another OS.
            // Installed paths are covered by actual evaluated items; these checks cover only
            // branches this machine cannot exercise, including the alternate Homebrew layout.
            if (OperatingSystem.IsMacOS() && File.Exists(library)) continue;
            var item = target.Descendants("LinkerArg").Single(element => (string?)element.Attribute("Include") == argument);
            ((string?)item.Attribute("Condition")).ShouldBe($"Exists('{library}')");
        }
    }

    [Theory]
    [InlineData("controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj", "GalleryPublishAot", "Release", true, true)]
    [InlineData("controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj", "GalleryPublishAot", "Release", false, false)]
    [InlineData("controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj", "GalleryPublishAot", "Debug", true, false)]
    [InlineData("tests/fixtures/LanguagePackEndToEnd/Consumer/Consumer.csproj", "ConsumerPublishAot", "Release", true, true)]
    [InlineData("tests/fixtures/LanguagePackEndToEnd/Consumer/Consumer.csproj", "ConsumerPublishAot", "Release", false, false)]
    public async Task Real_publish_consumers_conditionally_import_the_shared_native_linker_settings(
        string relativeProject, string publishProperty, string configuration, bool requested, bool expectedAot)
    {
        using var fixture = new EvaluationFixture();
        var project = Path.Combine(RepositoryRoot(), relativeProject);
        string[] properties = [$"-p:Configuration={configuration}", $"-p:{publishProperty}={Boolean(requested)}"];
        var result = await fixture.Evaluate(project, properties);
        Enabled(result, "PublishAot").ShouldBe(expectedAot);
        AssertHomebrewArguments(result, fixture.HomebrewTarget, expectedAot);

        // Preprocessing evaluates the real project's conditional imports even on a host with
        // no Homebrew libraries. This checks import effects, rather than grepping its .csproj.
        var flattened = await fixture.Preprocess(project, properties);
        var importedArguments = flattened.Descendants().Where(element => element.Name.LocalName == "LinkerArg")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(value => HomebrewLibraries.Any(entry => entry.Argument == value)).ToArray();
        importedArguments.ShouldBe(expectedAot ? HomebrewLibraries.Select(entry => entry.Argument).ToArray() : [], ignoreOrder: true);
    }

    private static string Boolean(bool value) => value ? "true" : "false";
    private static string Property(JsonElement result, string name) => result.GetProperty("Properties").GetProperty(name).GetString()!;
    private static bool Enabled(JsonElement result, string name) => Property(result, name).Equals("true", StringComparison.OrdinalIgnoreCase);

    private static void AssertAnalyzers(JsonElement result, bool trim, bool aot, bool singleFile)
    {
        Property(result, "EnableTrimAnalyzer").ShouldBe(Boolean(trim));
        Property(result, "EnableAotAnalyzer").ShouldBe(Boolean(aot));
        Property(result, "EnableSingleFileAnalyzer").ShouldBe(Boolean(singleFile));
    }

    private static void AssertHomebrewArguments(JsonElement result, string target, bool publishAot)
    {
        var arguments = result.GetProperty("Items").GetProperty("LinkerArg").EnumerateArray()
            .Where(item => HomebrewLibraries.Any(entry => entry.Argument == item.GetProperty("Identity").GetString())).ToArray();
        var expected = publishAot && OperatingSystem.IsMacOS()
            ? HomebrewLibraries.Where(entry => File.Exists(entry.Library)).Select(entry => entry.Argument).ToArray()
            : [];
        arguments.Select(item => item.GetProperty("Identity").GetString()).ShouldBe(expected, ignoreOrder: true);
        arguments.ShouldAllBe(item => Path.GetFullPath(item.GetProperty("DefiningProjectFullPath").GetString()!) == target);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("AtomUI.slnx was not found.");
    }

    private sealed class EvaluationFixture : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "atomui-publish-contracts-" + Guid.NewGuid().ToString("N"));
        public string HomebrewTarget { get; } = Path.Combine(RepositoryRoot(), "build/MacOSHomebrewNativeAot.targets");

        public EvaluationFixture() => Directory.CreateDirectory(_directory);

        public string CreateSdkProject(string name, Dictionary<string, string> properties)
        {
            var root = RepositoryRoot();
            WriteProject("Directory.Build.props", new XElement("Project",
                new XElement("Import", new XAttribute("Project", Path.Combine(root, "Directory.Build.props"))),
                new XElement("PropertyGroup",
                    new XElement("BaseIntermediateOutputPath", Path.Combine(_directory, "obj") + Path.DirectorySeparatorChar),
                    new XElement("MSBuildProjectExtensionsPath", Path.Combine(_directory, "obj") + Path.DirectorySeparatorChar),
                    new XElement("OutputPath", Path.Combine(_directory, "bin") + Path.DirectorySeparatorChar))));
            WriteProject("Directory.Build.targets", new XElement("Project",
                new XElement("Import", new XAttribute("Project", Path.Combine(root, "Directory.Build.targets")))));
            return WriteProject(name + ".csproj", new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"), new XElement("OutputType", "Library"),
                    properties.Select(pair => new XElement(pair.Key, pair.Value)))));
        }

        public string WriteProject(string name, XElement project)
        {
            var path = Path.Combine(_directory, name);
            new XDocument(project).Save(path);
            return path;
        }

        public async Task<JsonElement> Evaluate(string project, params string[] properties)
        {
            var output = await Run(project, [.. properties, "-getProperty:" + string.Join(',', PropertyNames), "-getItem:LinkerArg"]);
            using var result = JsonDocument.Parse(output);
            return result.RootElement.Clone();
        }

        public async Task<XDocument> Preprocess(string project, params string[] properties)
        {
            var output = Path.Combine(_directory, "preprocessed.xml");
            await Run(project, [.. properties, "-preprocess:" + output]);
            return XDocument.Load(output);
        }

        private static async Task<string> Run(string project, string[] arguments)
        {
            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = RepositoryRoot(), UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            foreach (var argument in new[] { "msbuild", project, "-nologo", "-nr:false" }.Concat(arguments)) start.ArgumentList.Add(argument);
            using var process = Process.Start(start)!;
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
            try { await process.WaitForExitAsync(timeout.Token); }
            finally
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync(CancellationToken.None);
                }
            }
            var output = await stdout;
            process.ExitCode.ShouldBe(0, $"dotnet msbuild {project} {string.Join(' ', arguments)}\n{output}\n{await stderr}");
            return output;
        }

        public void Dispose() => Directory.Delete(_directory, recursive: true);
    }
}
